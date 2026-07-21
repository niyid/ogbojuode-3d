# Ogboju Ode 3D — Unity Starter

Real, working project files — no encoded blobs, no tricks. Drop this into a
fresh Unity project and it will compile as-is.

## Setup

1. Install Unity Hub + Editor (see chat for the Ubuntu APT install steps).
   Make sure the **Android Build Support** module (with SDK/NDK/OpenJDK, or
   pointed at your existing `/mnt/android-sdk`) is installed.
2. In Unity Hub: **New Project** → **3D (URP)** template → name it
   `OgbojuOde3D`.
3. Close Unity. Copy the contents of this folder's `Assets/` directory into
   your new project's `Assets/` folder, merging so `Assets/Scripts` and
   `Assets/Editor` both exist inside it.
4. Reopen the project in Unity. Let it recompile scripts.
5. Go to **Edit → Project Settings → Tags and Layers** and add two tags:
   `Player` and `Enemy` (the scripts reference these).
6. In the menu bar, click **Ogboju Ode → Build Entire Universe**. This spawns:
   - A **hub village** at the origin — clay huts, six carved pillars (Arugba)
     around a lit bonfire, a ring of defensive spikes facing the tree line.
   - A **forest** stretching north — mahogany trees, scattered glowing
     "spirit fungus" markers.
   - The **player** (Akara-Ogun) with `PlayerVitals` (health) and
     `YorubaHunterController` attached.
   - **All three combat creatures**, spaced by danger level: Eru (fast,
     fragile) close to the village, Ijamba (slow, brutal) further in, Agbako
     (towering) deepest before the boss — one `CreatureAI` script with
     three stat presets instead of three duplicated files.
   - **The Ostrich-King (Oba Eye)**, deepest in the forest — a riddle
     encounter, not a straight fight. Ignore him and he's harmless; fail
     his riddle and he turns hostile.
   - **Three ghommid forest spirits** (Elere) wandering off the main path —
     non-hostile, each carrying a smaller riddle.
   - **Wisdom and expedition tracking** (`WisdomTracker`,
     `ExpeditionManager`) — the "bring wisdom back to civilization" loop
     from the book's structure, logged to the console for now.
   - Camera parented to the player, directional "moonlight."

   It also auto-creates the `Player`/`Enemy` tags if they don't exist yet,
   so there's no manual Tag Manager step.
7. Press **Play**. Controls: WASD to move, left-click to swing the machete,
   right-click to fire the musket (assign a `bulletPrefab` — any small
   sphere with a Rigidbody works), **E** to cast Egbe (teleport forward),
   **F** near a spirit or the Ostrich-King to hear their riddle. Creatures
   do contact damage back to `PlayerVitals`; the machete damages them
   through the shared `IDamageable` interface.

   Note: `RiddleGiver` currently auto-resolves riddles as "correct" on
   interact — there's no answer-input UI yet, so the loop is testable
   end-to-end without one. Wire a real prompt/answer UI into
   `RiddleGiver.ResolveRiddle(bool)` when you're ready; the wisdom-reward
   and Ostrich-King-hostility logic is already there waiting for it.

## Switching to Android

1. **File → Build Settings → Android → Switch Platform.**
2. **Player Settings**: set the package name to something like
   `com.techducat.ogbojuode3d`, minimum API level per your other Techducat
   apps.
3. For touch controls, build a UI Canvas with a joystick background/knob and
   three buttons (Machete / Musket / Egbe), then add the `MobileTouchUI`
   component and wire up the references in the Inspector — no code changes
   needed, `MobileJoystick` and `MobileTouchUI` are already in
   `Assets/Scripts/MobileTouchUI.cs`.
4. **Build And Run** with your phone connected via USB debugging.

## File map

- `Assets/Scripts/YorubaHunterController.cs` — player movement, machete,
  musket, Egbe teleport spell. Also accepts mobile joystick input via
  `MobileMoveInput`.
- `Assets/Scripts/IDamageable.cs` — shared damage interface used by both
  the player and creatures.
- `Assets/Scripts/PlayerVitals.cs` — player health, plus a minimal `Charm`
  data class and `Ase` charm-list stub (`equippedCharms`) that
  `irekeonibudo`'s charm enum can plug into directly.
- `Assets/Scripts/CreatureAI.cs` — one AI, three presets (`Agbako`,
  `Ijamba`, `Eru`) via `CreatureAI.ApplyPreset`, matching the stat
  differences described in the source material rather than three
  duplicated scripts.
- `Assets/Scripts/MobileTouchUI.cs` — virtual joystick + action button
  bridge for Android touch input.
- `Assets/Scripts/OstrichKingBoss.cs` — the Oba Eye. Holds ground and
  requires a `RiddleGiver` component; only becomes hostile if
  `OnRiddleFailed()` is called.
- `Assets/Scripts/RiddleGiver.cs` — riddle/interact logic shared by the
  Ostrich-King and every ghommid. Press **F** in range to trigger; rewards
  `WisdomTracker` on success.
- `Assets/Scripts/GhommidSpirit.cs` — small wandering forest-spirit
  movement, paired with `RiddleGiver` for its dialogue.
- `Assets/Scripts/WisdomTracker.cs` — singleton tracking wisdom earned from
  riddles; persists across scene loads (`DontDestroyOnLoad`) and survives
  player death, unlike `PlayerVitals`.
- `Assets/Scripts/ExpeditionManager.cs` — singleton tracking hub-vs-forest
  state based on the player's position relative to the village boundary;
  logs expedition start/return, ready to hook into UI or music later.
- `Assets/Editor/SceneSetupWizard.cs` — editor menu tool
  (`Ogboju Ode → Build Entire Universe`) that assembles the hub village,
  forest, all three creatures, the Ostrich-King, three ghommids, player,
  camera, lighting, and the two manager singletons in one click.
  Auto-creates the `Player`/`Enemy` tags.

## Note on AgbakoAI.cs (old file)

An earlier draft of this project had a standalone `AgbakoAI.cs` handling
only one creature. It's been superseded by `CreatureAI.cs`, which does the
same job for all three creatures. If you're merging this into an existing
project that already has the old `AgbakoAI.cs`, delete it to avoid a
duplicate/dead script.
