# Debug panel guide

[Overview](../README.md) · [Feature reference](features.md) · [Manual tests](development.md#manual-playtest-checklist)

The integrated panel is for quick testing. It needs no external console, settings
mod, or asset bundle. Opening it does not activate any gameplay options.

## Controls

| Input | Action |
| --- | --- |
| F8 | Open/close the panel |
| Escape while open | Close the panel |
| Enable test tools for this session | Arm actions after loading a game |
| F9 while enabled | Start native flight, or exit and return to its origin |
| WASD and mouse during flight | Move and look with the native flight camera |
| E | Ascend |
| Left Ctrl or Q | Descend |
| Land at flight camera | Move the survivor using native teleport-and-exit |

The panel is draggable and scales to smaller displays. Movement, camera input,
firing, and interactions are blocked while it is open; closing restores the prior
cursor/input-block state. Tools are unavailable at the main menu, while restoring
a save, or after death.

## Player and flight

- **God mode:** native invulnerability plus negative-condition interception.
- **Keep needs full:** replenish hunger, thirst, fatigue, and warmth continuously.
- **Ignore carried weight:** override inventory/encumbrance calculations without
  changing physical item mass.
- **Restore condition and needs:** restore adjusted maximum condition and needs;
  existing afflictions remain.
- **Treat frostbite and parasites:** explicitly clear those two reworked conditions.
- **Flight:** the camera detaches; the survivor stays at the origin until landing.
  The survivor is temporarily protected while flying. Returning/landing restores
  the previous native god state unless God mode is still enabled.

Finish climbing, vehicles, struggles, and other interactions before starting
flight. F9 returns to the starting point; it does not land at the camera. Use the
landing button for that operation. Native speed modifiers remain available.
Full terrain placement and flight movement still require in-world testing.

## Items and schematics

Search the curated item list or enter a native prefab ID manually. Set a quantity
from **1–20** and condition from **0–100%**. Zero condition creates ruined gear for
repair tests. Firearms are empty when granted.

Explicit debug variants stay physical usable variants; they are not automatically
converted into schematic pickups. **Learn selected variant schematic** is a
separate action. Debug grants do not consume or reroll finite world allocations.
Owned debug gear is preserved by the normal rarity filters. Granting a Tool Belt
or Rifle Holster can unlock its permanent collectible benefit.

## World and diagnostics

Freeze the time of day or set an hour from **0–24**. A position bookmark can return
the survivor only within the same active scene and outside flight/interactions.
The diagnostics show coordinates, active scene, world seed, and creation flags for
the four finite firearm/tool allocations.

## What persists?

| State | Behavior |
| --- | --- |
| Panel options and bookmark | Reset when disabled, loading/starting a game, or changing the active scene |
| Additive scenery streaming | Does not reset an active debug flight |
| Temporary native god/time-lock flags | Serialized with their pre-debug values; external pre-existing flags are preserved |
| Item grants, learned schematics, permanent accessory unlocks | Real game changes that can be saved |
| Health/needs/treatment, time changes, and teleports | Real game changes that can be saved |

Use a test save for experiments. Closing the panel hides it; it does not turn off
armed options. Disable the master checkbox to restore the temporary session flags.

Implementation: [DebugTools.cs](../src/InterloperOverhaul/DebugTools.cs).
Native checks: [DebugToolsValidation.cs](../tests/DebugToolsValidation.cs).
