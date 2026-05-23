# Nine Lives - CS20200 Term Project

Nine Lives is a command-line roguelike survival game implemented in F# using .NET 10.

The player starts with 9 lives and advances through an endless glass bridge. At each step, the player chooses one glass panel. Exactly one panel is safe, and all other panels break. The goal is to survive as long as possible by choosing paths, managing lives, and using reward items.

---

## Requirements

- F#
- .NET 10 SDK

---

## How to Run

Clone this repository and run the game with the following command:

```bash
dotnet run
```

If the project file is inside a subdirectory, run:

```bash
dotnet run --project CS20200_Term_project
```

The game runs entirely in the terminal. No additional external dependency is required.

---

## Game Overview

The game starts at Stage 1 with 9 lives.

At the start of each sector, the player chooses one path:

| Input | Path | Panels per Step | Reward Tier |
|---:|---|---:|---|
| 1 | Safety | 2 | Silver |
| 2 | Normal | 3 | Gold |
| 3 | Abyss | 4 | Prism |

Each step contains several numbered panels. Exactly one panel is randomly selected as the safe panel, and all other panels are breaking panels.

If the player chooses the safe panel, the player proceeds without losing a life.

If the player chooses a breaking panel, the player loses 1 life unless a defensive item prevents the loss.

The game ends immediately when the player's lives become 0.

---

## Stage and Sector Rules

A stage consists of multiple steps.

A sector consists of 5 stages.

The number of steps per stage starts at 1 and increases by 1 every 10 stages.

| Stage Range | Steps per Stage |
|---|---:|
| Stage 1–10 | 1 |
| Stage 11–20 | 2 |
| Stage 21–30 | 3 |
| Stage 31–40 | 4 |

General rule:

```text
steps per stage = ((stage - 1) / 10) + 1
```

---

## How to Play

At the beginning of each sector, choose a path:

```text
1: Safety
2: Normal
3: Abyss
```

At the beginning of each step, the game shows the player's inventory.

The player may enter:

```text
0: Use no item
item number: Use the selected item
```

The player can use at most one item per step.

After the item-use phase, the player chooses a panel number.

The available panel numbers depend on the selected path:

| Path | Available Panel Numbers |
|---|---|
| Safety | 1, 2 |
| Normal | 1, 2, 3 |
| Abyss | 1, 2, 3, 4 |

---

## Rewards and Items

After every 5 completed stages, the player receives a reward based on the path chosen for that sector.

The game shows two items from the corresponding reward tier, and the player chooses one item to add to the inventory.

### Silver Reward

| Item | Effect |
|---|---|
| First Aid Kit | Gives +2 lives |
| Rubber Hammer | Removes one breaking panel from the current step before panel selection |

### Gold Reward

| Item | Effect |
|---|---|
| Large First Aid Kit | Gives +5 lives |
| Reinforced Shoes | Prevents life loss from breaking panels until the current stage ends |

### Prism Reward

| Item | Effect |
|---|---|
| Full Recovery | Gives +9 lives |
| Prism Shield | Takes effect immediately and prevents life loss from breaking panels until the end of the next stage |

---

## Input Handling

For every user input, if the player enters a non-number input or a number outside the valid range for the current prompt, the game prints an error message and asks the player to enter the input again.

Invalid input does not change the game state.

---

## Game Result

When the game ends, the game prints:

- final stage reached
- total cleared steps
- path history
- number of used items

---

## Requirement Changes

The following changes were made after the original proposal.

---

### 1. Stage Progression Adjustment

Original requirement:

- Stages 1–10 have 2 steps per stage.
- Stages 11–20 have 3 steps per stage.
- Stages 21–30 have 4 steps per stage.
- In general, the number of steps per stage starts at 2 and increases by 1 every 10 stages.

Changed requirement:

- Stages 1–10 have 1 step per stage.
- Stages 11–20 have 2 steps per stage.
- Stages 21–30 have 3 steps per stage.
- In general, the number of steps per stage starts at 1 and increases by 1 every 10 stages.

Justification:

During implementation and playtesting, I found that starting with 2 steps per stage made the early game more punishing than intended. Since each step has only one safe panel and the player starts with limited resources, the original early-game difficulty could make the game end too quickly before the player meaningfully interacts with the reward and inventory systems.

This change adjusts only the difficulty curve. The core game structure, including stages, sectors, paths, random safe panels, reward tiers, inventory usage, and the game-ending condition, remains unchanged.

---

## LLM Usage

I used an LLM while developing this project.

I used the LLM for:

- refining the game requirements
- planning the F# implementation structure
- drafting parts of this README

I had to manually change or reprompt because:

- the LLM did not correctly understand how I wanted to divide the project into separate F# source files at first
- some suggested file structures were more complicated than necessary for this small command-line game
- the LLM's first suggestions for terminal output design were not readable enough

The main point that the LLM was not able to do correctly at first was designing the implementation structure at the right level of simplicity. It sometimes suggested changes or structures that were too complex for this project. I manually simplified the final structure and reviewed the implementation myself to make sure the game satisfies the submitted requirements.