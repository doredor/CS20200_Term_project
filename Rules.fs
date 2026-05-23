module Rules

open Domain

let initialState : GameState = {
    Stage = 1
    Lives = 9
    Inventory = []
    CurrentPath = None
    PathHistory = []
    TotalClearedSteps = 0
    UsedItemCount = 0
    ActiveEffects = []
}

// Stage 1~10: 2 steps
// Stage 11~20: 3 steps
// Stage 21~30: 4 steps
let stepsPerStage (stage: int) : int =
    ((stage-1) / 10) + 1

// A sector starts every 5 stages.
// Stage 1, 6, 11, 16, ...
let isSectorStart (stage: int) : bool =
    if (stage % 5 = 1) then true
    else false

// A sector ends after every 5 completed stages.
// Stage 5, 10, 15, ...
let isSectorEnd (stage: int) : bool =
    if (stage % 5 = 0) then true
    else false

// Safety -> 2 panels
// Normal -> 3 panels
// Abyss -> 4 panels
let panelCountOfPath (path: Path) : int =
    match path with
    | Safety -> 2
    | Normal -> 3
    | Abyss -> 4

// Safety -> Silver
// Normal -> Gold
// Abyss -> Prism
let rewardTierOfPath (path: Path) : RewardTier =
    match path with
    | Safety -> Silver
    | Normal -> Gold
    | Abyss -> Prism

// Silver -> First Aid Kit, Rubber Hammer
// Gold -> Large First Aid Kit, Reinforced Shoes
// Prism -> Full Recovery, Prism Shield
let rewardItemsOfTier (tier: RewardTier) : Item list =
    match tier with
    | Silver -> [FirstAidKit; RubberHammer]
    | Gold -> [LargeFirstAidKit; ReinforcedShoes]
    | Prism -> [FullRecovery; PrismShield]

let rewardItemsOfPath (path: Path) : Item list =
    path
    |> rewardTierOfPath
    |> rewardItemsOfTier

// Convert user input number to Path.
// 1 -> Safety
// 2 -> Normal
// 3 -> Abyss
let pathFromNumber (n: int) : Path =
    match n with
    | 1 -> Safety
    | 2 -> Normal
    | 3 -> Abyss
    | _ -> failwith "Invalid path number"

// Healing amount of each item.
// Non-healing items should return 0.
let healingAmountOfItem (item: Item) : int =
    match item with
    | FirstAidKit -> 2
    | LargeFirstAidKit -> 5
    | FullRecovery -> 9
    | RubberHammer -> 0
    | ReinforcedShoes -> 0
    | PrismShield -> 0

// Whether an item creates a stage-based protection effect.
// FirstAidKit, LargeFirstAidKit, FullRecovery, RubberHammer do not create active effects.
let activeEffectOfItem (currentStage: int) (item: Item) : ActiveEffect option =
    match item with
    | ReinforcedShoes ->
        Some (ReinforcedShoesUntil currentStage)

    | PrismShield ->
        Some (PrismShieldUntil (currentStage + 1))

    | FirstAidKit
    | LargeFirstAidKit
    | FullRecovery
    | RubberHammer ->
        None

let isEffectActiveAtStage (stage: int) (effect: ActiveEffect) : bool =
    match effect with
    | ReinforcedShoesUntil untilStage ->
        stage <= untilStage
    | PrismShieldUntil untilStage ->
        stage <= untilStage

// Check whether current active effects prevent life loss at the current stage.
let isProtectedAtCurrentStage (state: GameState) : bool =
    state.ActiveEffects
    |> List.exists (isEffectActiveAtStage state.Stage)

// Remove expired active effects.
// For example, if current stage is 6,
// ReinforcedShoesUntil 5 should be removed.
let removeExpiredEffects (state: GameState) : GameState =
    let activeEffects =
        state.ActiveEffects
        |> List.filter (isEffectActiveAtStage state.Stage)

    { state with ActiveEffects = activeEffects }

// Apply healing item effect.
// Non-healing items should not change Lives here.
let applyHealingItem (item: Item) (state: GameState) : GameState =
    let amount = healingAmountOfItem item
    { state with Lives = state.Lives + amount }

// Add active effect if the item has one.
let applyActiveEffectItem (item: Item) (state: GameState) : GameState =
    match activeEffectOfItem state.Stage item with
    | Some effect ->
        { state with ActiveEffects = effect :: state.ActiveEffects }
    | None ->
        state

// Common item effect wrapper.
// RubberHammer is special because it affects the current step's panel choices,
// so it will be handled later in GameEngine.fs.
let applyItemEffect (item: Item) (state: GameState) : GameState =
    state
    |> applyHealingItem item
    |> applyActiveEffectItem item

let increaseUsedItemCount (state: GameState) : GameState =
    { state with UsedItemCount = state.UsedItemCount + 1 }

let loseOneLife (state: GameState) : GameState =
    { state with Lives = state.Lives - 1 }

let clearOneStep (state: GameState) : GameState =
    { state with TotalClearedSteps = state.TotalClearedSteps + 1 }

let advanceStage (state: GameState) : GameState =
    { state with Stage = state.Stage + 1 }

let setCurrentPath (path: Path) (state: GameState) : GameState =
    { state with
        CurrentPath = Some path
        PathHistory = path :: state.PathHistory }

let addItemToInventory (item: Item) (state: GameState) : GameState =
    { state with Inventory = item :: state.Inventory }

// Remove item by 0-based index.
// Example: removeItemAt 0 [A; B; C] = [B; C]
let removeItemAt (index: int) (items: Item list) : Item list =
    List.removeAt index items

let isGameOver (state: GameState) : bool =
    state.Lives <= 0