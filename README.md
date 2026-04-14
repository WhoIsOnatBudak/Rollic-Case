# BusJam1 — Unity Casual Puzzle Game

A Bus Jam-style mobile puzzle game built in Unity 2022.3.19f1.  
Players tap colored passengers on a grid to route them through a waiting area and onto matching-colored buses before time runs out.

---

## Requirements

- **Unity:** 2022.3.19f1
- **Render Pipeline:** Built-in
- **TextMeshPro** (included in Unity package manager)

---

## Project Structure

```
Assets/
├── Levels/             # Level data (JSON) — level1.json … level8.json
├── Prefabs/            # Bus, Passenger, Tile, effect prefabs
├── Scenes/
│   ├── MainMenu.unity  # Start screen (Build Index 0)
│   ├── Level.unity     # Gameplay screen (Build Index 1)
│   └── Editor.unity    # Level editor (not in build)
└── Scripts/
    ├── LevelManager.cs
    ├── GridManager.cs
    ├── WaitingAreaManager.cs
    ├── BusStation.cs
    ├── Bus.cs
    ├── InputManager.cs
    ├── LevelUIController.cs
    ├── MainMenuManager.cs
    ├── PowerUpManager.cs
    ├── PassengerContent.cs
    ├── SpawnerContent.cs
    ├── ObstacleContent.cs
    ├── TileContent.cs
    ├── Tile.cs
    ├── BusSeatEffectPlayer.cs
    ├── LevelData.cs
    ├── ColorType.cs
    └── SceneEdit/
        ├── LevelSelectPanel.cs
        ├── LevelEditorPanel.cs
        ├── TileEditorPanel.cs
        ├── BusQueuePanel.cs
        └── LevelItemUI.cs
```

---

## Scenes

### 1. MainMenu (Build Index 0)

The start screen. Shows the current level number the player is about to play.

**Key behaviors:**
- Reads `PlayerPrefs["CurrentLevel"]` and displays `"Level X"` on screen.
- If the player has completed all 8 levels, shows `"You Win!"` instead and locks the Start button.
- **Start button** — loads the gameplay scene (Build Index 1).
- **Reset button** — resets `CurrentLevel` back to 1 and updates the display immediately.

**Script:** [MainMenuManager.cs](Assets/Scripts/MainMenuManager.cs)

---

### 2. Level (Build Index 1)

The main gameplay screen. Loads whichever level the player is currently on from JSON, then runs the puzzle loop.

**Key behaviors:**
- Reads `CurrentLevel` from PlayerPrefs, picks the matching JSON from the `levelJsons` array.
- If the player is past the last level, it repeats the final level.
- Displays `"Level X"` in the top UI throughout the session.
- **Timer** starts counting down the first time the player taps a passenger (not on scene load). When it hits zero, the game over panel appears.
- **Win condition:** all passengers (including spawner queues) cleared and waiting area empty.
- **Lose condition:** waiting area fills up entirely, or timer reaches zero.
- On level complete, `CurrentLevel` is incremented and saved to PlayerPrefs, then the level complete panel appears.

**Power-ups** (3 uses each, appear after first tap):
- **Extra Tile** — adds one extra slot to the waiting area.
- **Undo** — returns the last moved passenger back to its original grid tile.

**Scripts:**  
[LevelManager.cs](Assets/Scripts/LevelManager.cs) — singleton orchestrator  
[GridManager.cs](Assets/Scripts/GridManager.cs) — grid creation, BFS pathfinding  
[WaitingAreaManager.cs](Assets/Scripts/WaitingAreaManager.cs) — waiting queue  
[BusStation.cs](Assets/Scripts/BusStation.cs) — bus spawning and transitions  
[LevelUIController.cs](Assets/Scripts/LevelUIController.cs) — timer, level text, panels  
[InputManager.cs](Assets/Scripts/InputManager.cs) — mouse/touch raycast  
[PowerUpManager.cs](Assets/Scripts/PowerUpManager.cs) — extra tile & undo logic  

---

### 3. Editor (not in build)

A standalone in-engine level editor. Open this scene at any time in the Unity Editor to create or modify levels. Changes are saved as JSON files directly to `Assets/Levels/` and are immediately usable in gameplay.

> This scene is **not included in the build settings**. It is editor-only tooling.

**How to use:**

1. Open `Assets/Scenes/Editor.unity` in the Unity Editor.
2. Enter Play Mode.
3. The **Level Select Panel** loads automatically.

---

## Level Editor — Detailed Walkthrough

### Level Select Panel

The first thing you see when the Editor scene starts. Reads every `*.json` file under `Assets/Levels/` and lists them as scrollable items.

| Action | How |
|---|---|
| Open an existing level | Click its row in the list |
| Create a brand new level | Click **New Level** |

Selecting either option hides this panel and opens the **Level Editor Panel**.

**Script:** [LevelSelectPanel.cs](Assets/Scripts/SceneEdit/LevelSelectPanel.cs)

---

### Level Editor Panel

The main editing surface. Shows four numeric controls and two sub-panel launchers.

| Control | Range | Description |
|---|---|---|
| Width | 2 – 6 | Number of grid columns |
| Height | 2 – 6 | Number of grid rows |
| Timer | 5 – 100 s (5 s steps) | Countdown the player gets in gameplay |
| Waiting Area | 1 – 10 | Number of waiting slots beside the road |

**Important:** Increasing or decreasing grid size preserves the content of tiles that fit within the new dimensions. Cells that fall outside the new bounds are discarded.

| Button | Action |
|---|---|
| **Edit Tiles** | Opens the Tile Editor Panel (overlays current panel) |
| **Bus Queue** | Opens the Bus Queue Panel (overlays current panel) |
| **Save** | Writes the level to `Assets/Levels/levelN.json` and calls `AssetDatabase.Refresh()` so Unity picks it up immediately |
| **Go Back** | Returns to the Level Select Panel without saving |

**Script:** [LevelEditorPanel.cs](Assets/Scripts/SceneEdit/LevelEditorPanel.cs)

---

### Tile Editor Panel

Opened via the **Edit Tiles** button. Shows a visual grid that mirrors the level's current width × height. Each cell is a colored button; its color indicates its content type.

| Color | Content |
|---|---|
| Teal | Empty |
| Dark grey | Obstacle |
| Passenger color (Red/Blue/Green/Yellow) | Passenger |
| Orange | Spawner |

**Editing a tile — step by step:**

1. **Tap a tile** → Content Picker appears with four choices: Empty, Obstacle, Passenger, Spawner.
2. **Empty / Obstacle** → applied immediately, grid refreshes.
3. **Passenger** → Color Picker appears. Choose Red, Blue, Green, or Yellow. Applied immediately.
4. **Spawner** → Spawner Config panel appears:
   - Pick a **direction** (Up / Down / Left / Right). The selected direction is highlighted in cyan; others are dark blue.
   - Add up to **3 colors** to the spawn queue using the color buttons.
   - Remove the last color with **Delete**.
   - **Confirm** saves the spawner to the tile (requires at least 1 color).
   - **Cancel** closes without changes.

The grid auto-sizes its cells to fit the panel — cells are capped at 140 px each so the layout stays readable even on a 2×2 or 6×6 grid.

**Script:** [TileEditorPanel.cs](Assets/Scripts/SceneEdit/TileEditorPanel.cs)

---

### Bus Queue Panel

Opened via the **Bus Queue** button. Shows the sequence of buses the player must fill during the level.

- Add a bus to the end of the queue by clicking one of the four color buttons (Red, Blue, Green, Yellow).
- **Delete** removes the last bus in the queue.
- The queue displays as a 9-column grid (wraps automatically for longer queues).
- Maximum **18 buses** per level. Color buttons disable automatically when the cap is reached.
- **Close** returns to the Level Editor Panel; the queue is kept in memory until **Save** is pressed.

**Script:** [BusQueuePanel.cs](Assets/Scripts/SceneEdit/BusQueuePanel.cs)

---

## Level Data Format

Levels are plain JSON files at `Assets/Levels/levelN.json`.

```json
{
  "levelNumber": 3,
  "timerSeconds": 45.0,
  "gridWidth": 4,
  "gridHeight": 3,
  "waitingAreaLength": 4,
  "buses": [
    { "color": "Red" },
    { "color": "Blue" },
    { "color": "Red" }
  ],
  "grid": [
    { "x": 0, "y": 0, "contentType": "Passenger", "color": "Red",  "direction": "", "spawnColors": null },
    { "x": 1, "y": 0, "contentType": "Spawner",   "color": "",     "direction": "Down", "spawnColors": ["Blue","Blue"] },
    { "x": 2, "y": 0, "contentType": "Empty",     "color": "",     "direction": "", "spawnColors": null },
    { "x": 3, "y": 0, "contentType": "Obstacle",  "color": "",     "direction": "", "spawnColors": null }
  ]
}
```

| Field | Type | Notes |
|---|---|---|
| `levelNumber` | int | Used for display ("Level 3") and file naming |
| `timerSeconds` | float | Countdown length; editable in the level editor |
| `gridWidth` / `gridHeight` | int | 2–6 each |
| `waitingAreaLength` | int | 1–10 |
| `buses` | array | Ordered sequence of buses; `color` is Red / Blue / Green / Yellow |
| `grid` | array | One entry per cell; `contentType` is Empty / Obstacle / Passenger / Spawner |

---

## Available Levels

| Level | Timer | Grid | Waiting Area | Buses |
|---|---|---|---|---|
| 1 | 30 s | 2 × 2 | 3 | 1 |
| 2 | 35 s | 3 × 3 | 3 | varies |
| 3 | 40 s | 3 × 3 | 4 | varies |
| 4 | 45 s | 4 × 3 | 4 | varies |
| 5 | 50 s | 4 × 4 | 4 | varies |
| 6 | 55 s | 4 × 4 | 5 | varies |
| 7 | 60 s | 5 × 4 | 5 | varies |
| 8 | 60 s | 5 × 5 | 5 | varies |

---

## Build Settings

| Index | Scene | Purpose |
|---|---|---|
| 0 | MainMenu | Start screen |
| 1 | Level | Gameplay |

`Editor.unity` is intentionally excluded from the build.

---

## Level Progression

Progress is stored in `PlayerPrefs` under the key `CurrentLevel` (integer, default 1).  
The value increments by 1 each time the player completes a level.  
The Reset button on the main menu sets it back to 1.
