# Save compatibility and scope

[Overview](../README.md) · [Features](features.md) · [Installation](installation.md)

## Supported environment and scope

The verified environment is native **Linux x64**, The Long Dark **2.55**, Unity
**6000.0.60f1**, MelonLoader **0.7.3**, .NET runtime **6.0.36**, and .NET SDK
**10.0.x**. Other platforms/game versions have not been validated by this project.

Content and balance changes target **standard Interloper**. Other difficulties,
Custom, challenges, and story mode retain their native settings. Fast startup is
global. The opt-in debug panel is independent of the Interloper balance gate.
DLC content still requires ownership and native access/progression.

## New versus existing runs

| Feature | Existing-run behavior | New run needed? |
| --- | --- | --- |
| Coastal Highway houses | Saved selections are redirected to intact variants when the region loads | No, for the house redirect |
| Bunker interiors | Existing assignments, including old Interloper-specific interiors, are preserved | Yes, for normal initial distribution |
| Containers and loot | Searched containers, visited-scene contents, and saved finds are not regenerated | Yes, for the complete restored distribution |
| Finite firearms/tools/lights | Existing allocation snapshots, identities, conditions, and issued flags remain | Yes, for current locations and exact new-run counts |
| Region/feat selection | Existing starting choices are not replaced | Yes |
| Item weights, clothing, food, fuel, skills | Applied to existing eligible gear and native systems; food retains consumed proportion | No |
| Permanent accessories | Carried/equipped Tool Belts and Rifle Holsters convert into run unlocks after loading | No |
| Afflictions | Existing cases use the reworked treatment/clearing rules; permanent frostbite migrates to temporary full meters | No |
| Manufacturing | Recipes integrate into existing Interloper saves | No, but schematics/access/materials are still required |

## Persistence guarantees and limits

Allocation plans and creation flags live inside the native save slot, with stable
native item identities. They are not separate loose save files. Loading/revisiting
does not issue replacements for lost or destroyed finite equipment. Legacy site
catalogs remain resolvable for snapshots created by earlier versions; new versions
do not reroll those destinations. Older snapshots only retain allocations their
format supports; they do not retroactively receive all newer finite items.

Already saved/acquired tools and variants are preserved, so an older run can contain
more equipment than the current new-run distribution allows. Save migration does
not delete those items to enforce new counts. If allocation data cannot be decoded,
spawning fails closed instead of silently replacing the plan.

Crafted gear and debug grants are player-owned and do not consume world allocations.
Weapon conversions retain native identity, normalized condition, and interrupted
work. Permanent accessory and learned-schematic unlocks are run-specific and do not
carry into a new game.

Partial cattail bundles retain calories/weight through native gear saves. Curing
boxes retain mixed salt accounting; pre-existing stored salt uses jar mass because
its original source is unknown. Repeated food/cougar save loads preserve proportions
rather than repeatedly multiplying the new values.

## Other mods

There is no external Cannery Manufacturing, ModComponent, settings-mod, or
save-data-mod dependency. The manufacturing implementation is integrated here;
installing an older manufacturing mod alongside it has not been validated.

Compatibility with mods patching the same loot generation, crafting, item weights,
afflictions, input, or native flight methods is not guaranteed. Include the complete
mod list when reporting an interaction; do not assume two independent weight or
spawn patches will combine safely.

## Removing the mod

Back up the save folder before modded play. The project does not provide a complete
uninstall migration for modified saves. Removing the DLL does not reverse ordinary
saved changes such as inventories, teleports, or treatment.

The custom Cattail Bundle depends on this mod's item factory, and schematic/accessory
unlocks are represented by mod-managed state rather than equivalent vanilla physical
items. A pre-mod backup is the reliable way to return to a fully vanilla run.

[Debug tool persistence](debug-tools.md#what-persists) describes the distinction
between temporary options and saveable experiments.
