module GameEngine

open System
open Domain
open Rules
open Input
open Display

let rng = Random()

// Get current path from state.
// In normal gameplay, CurrentPath should always be Some path during a stage.
let getCurrentPath (state: GameState) : Path =
    match state.CurrentPath with
    | Some path -> path
    | None -> failwith "Current path is not selected"

// Generate safe panel number.
// Panel numbers are 1..panelCount.
let generateSafePanel (panelCount: int) : int =
    rng.Next(1, panelCount + 1)

// Decide step result based on selected panel, safe panel, and protection effect.
let decideStepResult (state: GameState) (selectedPanel: int) (safePanel: int) : StepResult =
    if selectedPanel = safePanel then
        Safe
    elif isProtectedAtCurrentStage state then
        ProtectedBroken
    else
        Broken

// Apply step result to GameState.
// Safe: no life loss
// Broken: lose 1 life
// ProtectedBroken: no life loss
let applyStepResult (result: StepResult) (state: GameState) : GameState =
    match result with
    | Safe -> state
    | Broken -> { state with Lives = state.Lives - 1 }
    | ProtectedBroken -> state

// Pick item from inventory using 1-based user choice.
// itemChoice = 1 means first item.
let getItemByChoice (itemChoice: int) (inventory: Item list) : Item =
    if itemChoice < 1 || itemChoice > List.length inventory then
        failwith "Invalid item choice"
    else
        inventory.[itemChoice - 1]

// Remove used item from inventory.
// itemChoice is 1-based, but removeItemAt uses 0-based index.
let removeItemByChoice (itemChoice: int) (state: GameState) : GameState =
    if itemChoice < 1 || itemChoice > List.length state.Inventory then
        failwith "Invalid item choice"
    else
        { state with Inventory = List.removeAt (itemChoice - 1) state.Inventory }

// Apply selected item.
// RubberHammer is special and should be handled in runStep,
// so this function applies only common item effects.
let useNormalItem (item: Item) (state: GameState) : GameState =
    state
    |> applyItemEffect item
    |> increaseUsedItemCount

// Ask player whether to use item.
// Return updated state and optional used item.
let handleItemUse (state: GameState) : GameState * Item option =
    let inventoryCount = List.length state.Inventory
    let itemChoice = readItemChoice inventoryCount

    if itemChoice = 0 then
        printNoItemUsed ()
        state, None
    else
        let item = getItemByChoice itemChoice state.Inventory
        let newState = state |> useNormalItem item |> removeItemByChoice itemChoice
        printUsedItem item
        newState, Some item

// Rubber Hammer removes one breaking panel from current step before panel selection.
// For simple implementation, return a list of selectable panels after removing one breaking panel.
let getSelectablePanelsAfterRubberHammer (panelCount: int) (safePanel: int) (usedItem: Item option) : int list =
    if usedItem = Some RubberHammer then
        // Remove one breaking panel (not safe panel).
        let breakingPanels = [1 .. panelCount] |> List.filter (fun p -> p <> safePanel)
        let panelToRemove = breakingPanels.[rng.Next(breakingPanels.Length)]
        [1 .. panelCount] |> List.filter (fun p -> p <> panelToRemove)
    else
        [1 .. panelCount]


// Read panel choice from selectable panels.
// This is needed because RubberHammer can remove a panel.
// If you want simpler implementation, you can skip this and only use readPanelChoice.
let rec readPanelChoiceFromList (panels: int list) : int =
    printf "Choose a panel from selectable panels > "
    let input = System.Console.ReadLine()
    let choice = tryParseInt input

    match choice with
    | Some n when List.contains n panels -> n
    | _ ->
        printfn "Invalid input. Please choose from selectable panels."
        readPanelChoiceFromList panels

// Choose path at the start of a sector.
let choosePathForSector (state: GameState) : GameState =
    printStatus state
    printPathChoices state

    let pathChoice = readPathChoice ()
    let path = pathFromNumber pathChoice

    printfn ""
    printfn "You chose the %s." (pathName path)

    { state with
        CurrentPath = Some path
        PathHistory = state.PathHistory @ [path] }

// Add reward after sector end.
// Reward depends on the current path.
let giveSectorReward (state: GameState) : GameState =
    let currentPath = getCurrentPath state
    let rewardItems = rewardItemsOfPath currentPath

    printRewardChoices rewardItems
    let rewardChoice = readRewardChoice ()
    let rewardItem = rewardItems.[rewardChoice - 1]

    addItemToInventory rewardItem state

// Run one step.
// stepNumber is 1-based.
let runStep (stepNumber: int) (totalSteps: int) (state: GameState) : GameState =
    let currentPath = getCurrentPath state
    let panelCount = panelCountOfPath currentPath

    printStepStart state stepNumber totalSteps

    // Generate safe panel and get selectable panels after Rubber Hammer effect.
    let safePanel = generateSafePanel panelCount
    let stateAfterItem, usedItemOption = handleItemUse state
    let selectablePanels = getSelectablePanelsAfterRubberHammer panelCount safePanel usedItemOption
    printSelectablePanels selectablePanels

    // Read panel choice from selectable panels.
    let selectedPanel = readPanelChoiceFromList selectablePanels

    // Decide step result and apply it to state.
    let stepResult = decideStepResult stateAfterItem selectedPanel safePanel
    printStepResult stepResult selectedPanel safePanel
    waitForEnter ()
    let newState = applyStepResult stepResult stateAfterItem

    // Clear one step and remove expired effects at the end of the step.
    newState
    |> clearOneStep
    //|> removeExpiredEffects

// Run all steps in current stage.
let rec runSteps (currentStep: int) (totalSteps: int) (state: GameState) : GameState =
    if isGameOver state then
        state
    elif currentStep > totalSteps then
        state
    else
        let nextState = runStep currentStep totalSteps state
        runSteps (currentStep + 1) totalSteps nextState

// Run one stage.
let runStage (state: GameState) : GameState =
    printStageStart state

    let totalSteps = stepsPerStage state.Stage
    let afterStepsState = runSteps 1 totalSteps state

    if isGameOver afterStepsState then
        afterStepsState
    else
        let afterRewardState =
            if isSectorEnd afterStepsState.Stage then
                giveSectorReward afterStepsState
            else    
                afterStepsState

        afterRewardState
        |> advanceStage
        |> removeExpiredEffects


// Run game loop.
let rec runGame (state: GameState) : unit =
    if isGameOver state then
        printGameOver state
    else
        let stateAfterPathChoice =
            if isSectorStart state.Stage then
                choosePathForSector state
            else
                state

        let afterStageState = runStage stateAfterPathChoice
        runGame afterStageState