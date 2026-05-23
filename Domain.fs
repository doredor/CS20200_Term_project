module Domain

type Path =
    | Safety
    | Normal
    | Abyss

type RewardTier =
    | Silver
    | Gold
    | Prism

type Item =
    | FirstAidKit
    | RubberHammer
    | LargeFirstAidKit
    | ReinforcedShoes
    | FullRecovery
    | PrismShield

type ActiveEffect =
    | ReinforcedShoesUntil of stage: int
    | PrismShieldUntil of stage: int

type GameState = {
    Stage: int
    Lives: int
    Inventory: Item list
    CurrentPath: Path option
    PathHistory: Path list
    TotalClearedSteps: int
    UsedItemCount: int
    ActiveEffects: ActiveEffect list
}

type StepResult =
    | Safe
    | Broken
    | ProtectedBroken

type GameStatus =
    | Running
    | GameOver