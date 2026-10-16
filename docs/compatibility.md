# Save compatibility and scope

[Overview](../README.md) · [Features](features.md) · [Installation](installation.md)

## Supported environment and scope

The verified environment is native **Linux x64**, The Long Dark **2.55**, Unity
**6000.0.60f1**, MelonLoader **0.7.3**, .NET runtime **6.0.36**, and .NET SDK
**10.0.x**. Other platforms/game versions have not been validated by this project.

Content and balance changes target **standard Interloper**. Other difficulties,
Custom, challenges, and story mode retain their native settings. Fast startup is
global. The integrated GM/debug panel has been removed.
DLC content still requires ownership and native access/progression.

## New versus existing runs

| Feature | Existing-run behavior | New run needed? |
| --- | --- | --- |
| Coastal Highway houses | Saved selections are redirected to intact variants when the region loads | No, for the house redirect |
| Bunker interiors | Existing assignments, including old Interloper-specific interiors, are preserved | Yes, for normal initial distribution |
| Containers and loot | Searched containers, visited-scene contents, and saved finds are not regenerated; revised retention rates apply only to new opportunities | Yes, for the complete restored distribution |
| Finite firearms/tools | Known mod allocations preserve the first copy and add one independently tracked second copy per eligible type; older unknown counts stay excluded | No for tracked runs; yes for the full guarantees in untracked legacy runs |
| Finite flashlights | Existing locations, identities, and issuance flags remain; no additional lights are issued | Yes, for exact new-run counts in pre-flashlight saves |
| Region/feat selection | Existing starting choices are not replaced | Yes |
| Item weights, clothing, food, fuel, skills | Applied to existing eligible gear and native systems; food retains consumed proportion | No |
| Permanent accessories | Carried/equipped Tool Belts and Rifle Holsters convert into run unlocks after loading | No |
| Afflictions | Existing cases use the reworked treatment/clearing rules; permanent frostbite migrates to temporary full meters | No |
| Manufacturing | Recipes integrate into existing Interloper saves | No, but schematics/access/materials are still required |

## Persistence guarantees and limits

Allocation plans and creation flags live inside the native save slot, with stable
native item identities. They are not separate loose save files. Loading/revisiting
does not issue replacements for lost or destroyed finite equipment. Legacy site
catalogs remain resolvable for snapshots created by earlier versions. Format-four
migration keeps each first-copy destination and adds one second firearm/tool per
tracked type, at a distinct deep destination. These new copies have independent
identities and issuance flags. Repeated migration/reload cannot grant further copies
or replace lost originals. Formats predating tracked tools keep their unknown-tool
legacy flag; pre-allocation saves remain excluded. Flashlight migration is unchanged.

Already saved/acquired tools and variants are preserved, so an older run can contain
more equipment than the current new-run distribution allows. Save migration does
not delete those items to enforce new counts. If allocation data cannot be decoded,
spawning fails closed instead of silently replacing the plan.

Crafted gear is player-owned and does not consume world allocations.
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
or affliction methods is not guaranteed. Include the complete
mod list when reporting an interaction; do not assume two independent weight or
spawn patches will combine safely.

## Removing the mod

Back up the save folder before modded play. The project does not provide a complete
uninstall migration for modified saves. Removing the DLL does not reverse ordinary
saved changes such as inventories, teleports, or treatment.

The custom Cattail Bundle depends on this mod's item factory, and schematic/accessory
unlocks are represented by mod-managed state rather than equivalent vanilla physical
items. A pre-mod backup is the reliable way to return to a fully vanilla run.

The former GM/debug options were session-only and are no longer included. Items,
learned recipes, treatment, or other ordinary changes already saved while testing
remain part of that save; removing the panel does not roll back inventories or
progression.
