module Input

// Try to convert string input into int.
// Example:
// "3" -> Some 3
// "abc" -> None
let tryParseInt (input: string) : int option =
    match System.Int32.TryParse(input) with
    | true, value -> Some value
    | false, _ -> None

// Read an integer in the given range.
// If input is not a number or outside the range,
// print an error message and ask again.
let rec readIntInRange (prompt: string) (minValue: int) (maxValue: int) : int =
    printf "%s" prompt
    let input = System.Console.ReadLine()
    let i = tryParseInt input

    match i with
    | Some n when minValue <= n && n <= maxValue ->
        n

    | Some _ ->
        printfn "Invalid input. Please enter an integer value in range %d to %d." minValue maxValue
        readIntInRange prompt minValue maxValue

    | None ->
        printfn "Invalid input. Please enter an integer value."
        readIntInRange prompt minValue maxValue

// Path input:
// 1 -> Safety
// 2 -> Normal
// 3 -> Abyss
let readPathChoice () : int =
    readIntInRange "Choose path: 1) Safety  2) Normal  3) Abyss > " 1 3

// Panel input.
// panelCount depends on the current path.
// Safety: 2
// Normal: 3
// Abyss: 4
let readPanelChoice (panelCount: int) : int =
    readIntInRange (sprintf "Choose a panel from 1 to %d > " panelCount) 1 panelCount

// Item input.
// 0 means use no item.
// 1..inventoryCount means use the corresponding item.
let readItemChoice (inventoryCount: int) : int =
    if inventoryCount = 0 then
        readIntInRange "Inventory is empty. Enter 0 to use no item > " 0 0
    else
        readIntInRange (sprintf "Use item? 0 for none, 1~%d for item > " inventoryCount) 0 inventoryCount

// Reward input.
// There are always two reward items in the proposal.
// 1 -> first reward item
// 2 -> second reward item
let readRewardChoice () : int =
    readIntInRange "Choose reward item: 1 or 2 > " 1 2

// Ask whether the player wants to continue.
// This is optional.
// You may not need this in the final game loop.
let readYesNo (prompt: string) : bool =
    failwith "implement"