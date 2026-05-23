module Display

open Domain

let waitForEnter () : unit =
    printfn ""
    printf "Press Enter to continue..."
    System.Console.ReadLine() |> ignore
    printfn ""

let printLine () : unit =
    printfn "----------------------------------------"

let printBigLine () : unit =
    printfn "========================================"

// Convert Path value to printable string.
let pathName (path: Path) : string =
    match path with
    | Safety -> "Safety path"
    | Normal -> "Normal path"
    | Abyss -> "Abyss path"

// Convert RewardTier value to printable string.
let rewardTierName (tier: RewardTier) : string =
    match tier with
    | Silver -> "Silver Reward"
    | Gold -> "Gold Reward"
    | Prism -> "Prism Reward"

// Convert Item value to printable string.
let itemName (item: Item) : string =
    match item with
    | FirstAidKit -> "First Aid Kit"
    | RubberHammer -> "Rubber Hammer"
    | LargeFirstAidKit -> "Large First Aid Kit"
    | ReinforcedShoes -> "Reinforced Shoes"
    | FullRecovery -> "Full Recovery"
    | PrismShield -> "Prism Shield"

let itemDescription (item: Item) : string =
    match item with
    | FirstAidKit ->
        "+2 lives"

    | RubberHammer ->
        "Removes one breaking panel in the current step"

    | LargeFirstAidKit ->
        "+5 lives"

    | ReinforcedShoes ->
        "Prevents life loss from breaking panels until the current stage ends"

    | FullRecovery ->
        "+9 lives"

    | PrismShield ->
        "Prevents life loss from breaking panels until the end of the next stage"

// Convert ActiveEffect value to printable string.
let activeEffectName (effect: ActiveEffect) : string =
    match effect with
    | ReinforcedShoesUntil stage ->
        sprintf "Reinforced Shoes active until Stage %d" stage

    | PrismShieldUntil stage ->
        sprintf "Prism Shield active until Stage %d" stage

// Print basic game status.
let printStatus (state: GameState) : unit =
    printfn "Stage %d | Lives: %d | Cleared Steps: %d | Used Items: %d"
        state.Stage
        state.Lives
        state.TotalClearedSteps
        state.UsedItemCount

// Print current path.
let printCurrentPath (state: GameState) : unit =
    match state.CurrentPath with
    | Some path ->
        printfn "Current path: %s" (pathName path)
    | None ->
        printfn "Current path: Not selected"

let printPathChoices (state: GameState) : unit =
    printfn ""
    printBigLine ()
    printfn "Choose your path for the next sector."
    printBigLine ()
    printfn "1) Safety"
    printfn "   Panels: 2"
    printfn "   Reward: Silver"
    printfn "   Risk: Low"
    printfn ""  
    printfn "2) Normal"
    printfn "   Panels: 3"
    printfn "   Reward: Gold"
    printfn "   Risk: Medium"
    printfn ""
    printfn "3) Abyss"
    printfn "   Panels: 4"
    printfn "   Reward: Prism"
    printfn "   Risk: High"
    printfn ""

    if state.Lives <= 4 then
        printfn "Warning: Your lives are low. Safety is recommended."
        printfn ""

// Print player's inventory.
// Items are printed as 1-based index because user input uses 1..inventoryCount.
let printInventory (inventory: Item list) : unit =
    printfn ""
    printfn "Inventory:"

    if List.isEmpty inventory then
        printfn "  Empty"
    else
        inventory
        |> List.iteri (fun index item ->
            printfn "  %d) %s - %s" (index + 1) (itemName item) (itemDescription item))

// Print active protection effects.
let printActiveEffects (effects: ActiveEffect list) : unit =
    printfn ""
    printfn "Active effects:"

    if List.isEmpty effects then
        printfn "  None"
    else
        effects
        |> List.iter (fun effect ->
            printfn "  %s" (activeEffectName effect))

// Print information at the beginning of a stage.
let printStageStart (state: GameState) : unit =
    printfn ""
    printBigLine ()
    printfn "STAGE %d START" state.Stage
    printStatus state
    printCurrentPath state
    printActiveEffects state.ActiveEffects
    printBigLine ()

let printStepStart (state: GameState) (stepNumber: int) (totalSteps: int) : unit =
    printfn ""
    printLine ()
    printfn "Step %d / %d" stepNumber totalSteps
    printfn "Lives: %d" state.Lives
    printInventory state.Inventory
    printLine ()


let printBridgePanels (panels: int list) : unit =
    printfn ""
    printfn "Glass panels ahead:"

    panels
    |> List.iter (fun p ->
        printf "[ %d ] " p)

    printfn ""
    printfn ""


// Print selectable panels.
// Example: [1; 3; 4] means panel 2 was removed by Rubber Hammer.
let printSelectablePanels (panels: int list) : unit =
    printBridgePanels panels



// Print selected panel result.
let printStepResult (result: StepResult) (selectedPanel: int) (safePanel: int) : unit =
    printfn ""
    printLine ()
    printfn "You step onto panel %d..." selectedPanel
    printfn "Safe panel: %d" safePanel
    printfn ""

    match result with
    | Safe ->
        printfn "The glass holds."
        printfn "You survived this step."

    | Broken ->
        printfn "The glass breaks."
        printfn "You lost 1 life."

    | ProtectedBroken ->
        printfn "The glass breaks, but your protection saves you."
        printfn "You lost no life."

    printLine ()

// Print reward choices.
// Reward items are printed as 1 and 2.
let printRewardChoices (items: Item list) : unit =
    printfn ""
    printBigLine ()
    printfn "SECTOR CLEARED - Choose one reward"
    printBigLine ()

    items
    |> List.iteri (fun index item ->
        printfn "%d) %s" (index + 1) (itemName item)
        printfn "   Effect: %s" (itemDescription item))

// Print item used by player.
let printUsedItem (item: Item) : unit =
    printfn "You used %s." (itemName item)

// Print message when player chooses no item.
let printNoItemUsed () : unit =
    printfn "You did not use any item."

// Print path history at game over.
let printPathHistory (history: Path list) : unit =
    printfn "Path history:"

    if List.isEmpty history then
        printfn "  None"
    else
        history
        |> List.iteri (fun index path ->
            printfn "  Sector %d: %s" (index + 1) (pathName path))

// Print final game over summary.
let printGameOver (state: GameState) : unit =
    printfn ""
    printfn "========== Game Over =========="
    printfn "Final stage reached: %d" state.Stage
    printfn "Total cleared steps: %d" state.TotalClearedSteps
    printfn "Used items: %d" state.UsedItemCount
    printPathHistory state.PathHistory
    printfn "==============================="