# Interloper Overhaul

A personal mod for **The Long Dark**, focused on restoring content excluded from
standard **Interloper**, with a few general quality-of-life changes. By Spelunkern.

## Version 0.14.0: item weight and lantern warmth balance

Standard Interloper uses the following item weights, including gear already saved
in the world or inventory. No new run is required for these item adjustments.

| Item | Weight |
| --- | --- |
| Can opener | 0.10 kg |
| Insulated flasks, all seven styles (empty) | 0.10 kg |
| Recycled can (empty) | 0.10 kg |
| Improvised hatchet | 1.50 kg |
| Improvised knife | 0.50 kg |
| Stones | 0.10 kg each |
| Standard arrows | 0.10 kg each |
| Red and marine flares | 0.20 kg each |
| Car battery | 5.00 kg |
| Replacement fuse | 0.10 kg |
| Ordinary and research books, including open book models | 0.20 kg each |
| Snare | 0.20 kg |
| Mountaineering rope | 2.50 kg |
| Woodworking tools | 1.00 kg |
| Spray paint, both can models | 0.20 kg total |

Standard and Spelunker's lanterns provide **+6°C** through their native heat source.
Fuel consumption, light, heat radius, and ignition behavior retain their normal rules.
Insulated flasks no longer lose condition with time; their existing condition and
fall damage remain intact. Their drinks still cool normally and contribute their
native weight. Recycled cans also retain the weight of their contents. Spray paint
has a fixed 0.20 kg total weight, without replenishing paint or changing its uses.
Stack sizes, research progress, crafting recipes, and loot availability are unaffected.

Native gear-data clones keep the new base weights and flask decay rate local to
Interloper instances; canonical prefabs are restored after inventory weight previews.
Other difficulties keep their authored values. Automated runtime checks cover actual
item weights, stacks, flask contents and aging, both lanterns, save/reload, and
switching to Stalker and Misery in the same process.

## Variant schematics and finite flashlights (0.13.0)

In standard Interloper, authored variant weapon placements now yield physical
**Workshop Schematics** instead of usable weapons. Their native location selection
and spawn rolls still apply; the additional restored-loot rarity roll does not.
Selected Tales/cache variant rewards become schematics, and variant Trader offers
retain their native costs and progression. Random ordinary loot and beachcombing
cannot supply variant weapons. Previously serialized or owned variants are preserved.

Schematics use an existing blueprint-card model with a variant-specific English
name and description. Picking one up records it permanently for that run, appends
its description to journal notes, unlocks its recipe, and removes the card from the
inventory. All eleven variants are included, including the Bushcraft Bow.

Upgrades require the **Bleak Inlet Cannery ammunition workbench**, light, the
learned schematic, and one usable base item. Quality tools are required for firearm
and Survival Knife upgrades; bows require a standard, improvised, or Survival Knife.
Tools are reusable and use native crafting wear. There is no additional skill gate.

| Result | Base item | Additional consumed materials | Base time |
| --- | --- | --- | --- |
| Survival Knife | Hunting knife | 3 scrap metal, 1 cured leather, 1 cloth | 180 min |
| Forester's Revolver | Revolver | 3 scrap metal, 1 cured maple, 1 cured leather | 240 min |
| Warden's Revolver | Revolver | 2 scrap metal, 1 cured leather | 180 min |
| Hunter's Revolver | Revolver | 4 scrap metal, 2 cured leather | 300 min |
| Vaughn's Rifle | Rifle | 3 scrap metal, 1 cured leather | 300 min |
| Barb's Rifle | Rifle | 6 scrap metal, 1 cured maple, 2 cured leather | 420 min |
| Curator's Rifle | Rifle | 5 scrap metal, 1 cured maple, 1 cloth | 480 min |
| Ranger's Rifle | Rifle | 4 scrap metal, 1 cured maple, 2 cured leather | 420 min |
| Woodwright's Bow | Survival bow | 1 cured maple, 2 cured guts, 1 cured leather | 300 min |
| Sport Bow | Survival bow | 2 scrap metal, 1 cured maple, 2 cured guts | 360 min |
| Bushcraft Bow | Survival bow | 1 cured birch, 2 cured guts, 1 cloth | 240 min |

Once a firearm-family schematic is known, any variant in that family can be
returned to its common version at the Cannery: **30 minutes for a revolver or
60 minutes for a rifle**, with no materials or tool required and no material refund.
Upgrading and reverting preserve normalized condition and the original weapon's
native identity. Ruined weapons, loaded firearms, and firearms containing spent
casings cannot start a conversion. The base is reserved as one native work item;
interrupted work retains its recipe, condition, and progress through saves. A failed
start returns the original item. Conversions never issue an additional world weapon.

Each **new Interloper run** allocates **two standard flashlights** at two distinct,
seed-selected verified outdoor placements and **one Miner's Flashlight** at its
native Concentrator watchroom placement. Their initial condition is seed-selected
from 1–100%; native aurora/battery behavior is unchanged. Locations, creation flags,
and separate native identities persist with the save. Revisits, loss, destruction,
and harvesting do not produce replacements. Extra flashlights are blocked in
ordinary loot, beachcombing, and Trader offers. Existing allocation snapshots migrate
to version three without issuing extra lights or changing the original firearm/tool
identities; start a new game for the exact flashlight counts.

Automated checks cover native card text/saves, recipe locks and tools, world-site
replacement, reward conversion, flashlight identities/saves, unloaded input gating,
reservation/rollback, interrupted-work serialization, and condition/identity transfer.
A full Cannery UI playthrough with time acceleration, interruptions, and Trader
collection still requires in-game testing. The automated probe stays at the menu
and does not open an existing survival save.

## Integrated cannery manufacturing (0.12.0)

Standard Interloper gains eight native crafting recipes at **ammunition workbenches**.
They use the existing crafting UI, light/tool requirements, material consumption,
work in progress, and native gear saves. No ModComponent, settings menu, external
blueprint files, or custom item bundles are required.

| Recipe | Consumed materials | Output | Base time |
| --- | --- | --- | --- |
| Refine a hunting knife | 1 improvised knife + 2 scrap metal | 1 hunting knife | 120 min |
| Refine a hatchet | 1 improvised hatchet + 3 scrap metal | 1 standard hatchet | 180 min |
| Efficient arrow shafts | 1 cured birch sapling | 4 arrow shafts | 60 min |
| Red-flare shell | 1 unused red flare + 1 scrap metal + 1 cloth | 1 flare shell | 60 min |
| Blue-flare shell | 1 unused marine flare + 1 scrap metal + 1 cloth | 1 flare shell | 60 min |
| Gunpowder shell | 50 g gunpowder + 1 scrap metal + 1 cloth | 1 flare shell | 30 min |
| Recycle red flares | 3 unused red flares | 100 g gunpowder | 90 min |
| Recycle marine flares | 3 unused marine flares | 100 g gunpowder | 90 min |

Arrow-shaft production requires a knife or hatchet (standard or improvised).
Flare conversions require a standard or improvised knife. Burning, exhausted, wet,
and previously extinguished flares do not count as unused inputs. Recycling creates
a partially filled native gunpowder can, rather than a free full can. Shell recipes
consume casing/wadding materials; they do not add rifle/revolver ammunition or casings.

**The one-knife/one-hatchet rule now limits world allocation.** Paid manufacturing
upgrades are repeatable and do not consume or reroll those world allocations.
Manufactured gear is treated as player-owned, so the rarity and spawn-cap filters
cannot delete it. Additional tools still cannot spawn through beachcombing or Trader
stock. Variant upgrades follow the schematic system above.

Native **milling machines** can repair and restore the distress pistol and standard
crampons. Repairs require 1 scrap metal and 30 base minutes; full restoration from
ruined requires 3 scrap/180 minutes for the pistol or 4 scrap/210 minutes for crampons.
Aurora/power, access, native repair skill behavior, and machine availability remain
under the game's control. Interrupted restoration progress uses the native gear save.

This is a new implementation inspired by
[Cannery Manufacturing](https://github.com/ds5678/CanneryManufacturing) and its
[DLC update](https://github.com/DigitalzombieTLD/CanneryManufacturingDLC), with revised
recipes and explicit integration with this mod's loot rules. It does not import their
settings, firearm condition overrides, extra reloading crates, or cannery-code bypass.
See [third-party notices](THIRD_PARTY_NOTICES.md).

The recipes also work in existing Interloper saves. Automated checks cover the real
native catalog and ingredients, location/difficulty isolation, crafted output,
measured powder serialization, fresh-flare selection/consumption, and milling saves.
A full workshop playthrough with time acceleration and interruptions still requires
in-game testing.

## World hunting knife and hatchet allocation (0.11.0)

Each **new Interloper run** also allocates exactly **one hunting knife and one
standard hatchet**, independently selected from 33 verified native outdoor tool
placements. They start at a seed-selected condition from **1% to 100%**. Their
locations, creation flags, and native object GUIDs persist with the save slot;
loading, region revisits, losing, ruining, or harvesting them does not issue replacements.

Additional hunting knives and standard hatchets are blocked in fresh world loot,
random/scripted containers, reward caches, Trader offers (including special requests),
and **all newly generated beachcombing loot**, including contents of large finds.
The two allocated tools are placed in the world, never supplied by beachcombing.
Improvised knives/hatchets and their crafting recipes retain their existing rules.

Existing saved or acquired tools are preserved. **Start a new survival game for the
one-of-each guarantee.** Version-one allocation snapshots keep their original firearm
plan and do not receive extra tools; their pre-existing tool count is not migrated.
The save payload is now version two, under the same save-slot key, and firearm
identities and seed-based locations stay unchanged.

Validation covers all four allocated items, native world and individual tool
serialization, retained tool wear, repeated population, old payload migration,
additional scripted/world gear, beachcombing rejection, and Trader filtering.
Full travel/save/quit/reload playthrough testing remains necessary.

## Historical variant ban (0.10.0; superseded by 0.13.0)

New weapon variants no longer spawn in standard Interloper: Barb's, Curator's,
Vaughn's and Ranger rifles; Forester's, Warden's and Hunter's revolvers;
Woodwright's and Sport bows; and the Survival Knife. The block covers fresh world
placements, random/scripted containers (including reward caches), beachcombing,
and Trader offers, including variant offers marked as special requests. Rejected
loot is not replaced. Other difficulties and clothing rewards are unaffected.

Already saved or acquired variants are preserved. Start a new survival game for a
world without previously generated variants. This temporary rule supersedes their
former 0.25% retention tier; their future distribution has not been defined yet.
Validation checks all ten actual variant prefabs, skipped-roll placements, saved
and owned gear, scripted containers, fresh radial finds, reward-generation scopes,
Trader stock, and isolation from Stalker and Misery.

## Finite standard firearms (0.9.0)

Each **new Interloper run** allocates exactly **one standard rifle and one standard
revolver**. The world seed independently chooses their locations from 28 verified
base-game outdoor firearm positions. Both start unloaded, with no spent casings in
the weapon, and a seed-selected condition from **1% to 100%**. Exploration order
does not choose the locations. Locations are materialized when their regions load;
there is no replacement if an allocated weapon is lost, ruined, or harvested.

Additional standard rifles/revolvers are suppressed in fresh world loot, containers,
beachcombing, and Trader stock. Weapon variants are outside this two-weapon
allocation and now use the 0.13.0 schematic conversion system above.

Found rifle/revolver ammunition retains **0.25%** of its previous opportunities;
found empty casings retain **0.5%**. These are independent retention rolls after
native loot selection, not probabilities per container. Failed opportunities remain
empty. Crafting, ammunition already obtained, and recovered casings from firing
are preserved. Other ammunition types, such as flare shells, retain their own rules.

Trader ammunition/casing offers also use these retention rates, with a stable
decision per world seed, item, and native aurora number. Reopening the radio or
reloading cannot reroll the same aurora. Special requests without blocked weapons/tools, underlying exchange
assets, other stock, prices, and existing active trades are preserved.

The allocation and separate creation flags are stored as an opaque UTF-8 payload
under the mod's own key inside the native save slot. They travel with the game's
save snapshots instead of a separate sidecar file. Each managed firearm has a
stable native object GUID and uses the game's normal gear serialization. Loading
does not reroll its condition, empty ammunition loaded by the player, or allocate
replacement guns. No additional save-data mod dependency is required.

**Start a new survival game for this guarantee.** Saves predating this allocation
keep their existing weapons and do not receive two extra allocated guns. Their
previous weapon count is not retroactively changed. If allocation data cannot be
decoded, spawning fails closed rather than silently replacing the plan.

Validation uses real rifle/revolver prefabs in synthetic region scenes, native
save-slot payloads and world/individual gear serialization, repeat population after
restoring the allocation, recovered casing initialization, and authored Trader
gear-data references. Full travel/save/quit/reload playthrough testing remains
necessary; the probe does not open an existing save.

## Restored loot rarity (0.8.0 baseline)

Newly generated restored loot now receives an additional retention roll:

| Restored item tier | Retention relative to the restoration baseline |
| --- | --- |
| Ordinary restored items, including food | 5% (1 in 20) |
| Hunting knife and standard hatchet | One allocated item each per new run (0.11.0) |
| Other restored utility tools and premium clothing | 1% (1 in 100) |
| Special rifle/revolver variants, special bows, and the Survival Knife | Schematics at authored sites/rewards; Cannery upgrades (0.13.0) |

These are **additional retention rates**, not chances per container or guarantees
per region. Native table selection and spawn chances still apply. A rejected item
leaves its opportunity empty; it is not rerolled or replaced. This also makes
containers containing only premium gear genuinely sparse. The exact item tiers
are defined in `shared/LootBalancePolicy.cs`.

The filter covers newly selected random/scripted container loot, fresh world gear
(including formerly guaranteed placements), and fresh radial finds such as loose
beachcombing loot. Rejected radial finds consume their native opportunity and keep
the native respawn timer, preventing repeated attempts from defeating rarity.
Difficulty-excluded gear inside large washed-up finds uses the world gear filter.
Restored radio-cache and Tales bunker container loot follows these tiers even
during its temporary Stalker generation scope.

Existing Interloper loot sources are preserved: a native placed hammer, for
example, does not become rare just because hammers were excluded from container
tables. Unique Tales rewards, the HRV Signal Fire satchel,
notes, keys, structures, and containers themselves do not receive this rarity roll.
Trader firearm, variant, and ammunition availability follows the rules above; prices
and progression remain unchanged.

Rarity is deterministic from the world seed, source location, item, and opportunity
index. It does not consume the shared random stream. Already rolled world items,
saved radial finds, searched containers, and player-owned/stored gear are not
rebalanced. Start a new survival game for the full distribution.

Validation checks actual gear prefab tiers, a 100,000-opportunity distribution per
tier, native world and scripted-container hooks, synthetic radial generation state,
saved-item boundaries, and reward-scope/difficulty isolation. Complete beachcombing
respawn cycles and long survival playthroughs still require in-game testing.

## Beachcombing restoration (0.7.0)

Beachcombing now also restores difficulty-excluded contents authored inside washed-up
boats and other large finds. This includes the canoe's arrows and broken arrow,
which the general restoration previously left disabled because Stalker excludes
those specific scene objects too. The override applies only to Interloper and only
to known beachcombing prefab hierarchies. Objects excluded from all three ordinary
Survival difficulties remain excluded.

Normal and post-blizzard loose-item tables retain their original entries and weights,
with the gear eligibility restored in 0.6.0. Large finds retain their original table,
compatible-location restrictions, container locks, random selection, and expiry rules.
Beachcombing spawn frequency, quantity settings, blizzard timing, and weak-ice hazards
are not rebalanced. This does not add unrelated items to the beachcombing tables or
force every possible find to appear. New finds use the restored eligibility;
existing saved finds and already searched containers are not reset.

Validation loads the actual standard, post-blizzard, and large-find tables from the
installed game. It checks their original entries, filtered lookups, weights, authored
boat contents, and location restrictions. These tests do not simulate a full coastal
respawn cycle or modify a survival save.

## Full object availability (0.6.0)

Interloper's object exclusions are removed for weapons, ammunition, tools, clothing,
unique variants, containers, collectibles, plants, and other world objects that are
available in normal survival gameplay. This applies to scene objects, parent groups,
gear prefabs, native loot-table filtering, and the Trader's item eligibility.

- Original loot tables, weights, locations, random selections, and spawn probabilities
  remain in use before the additional restored-item rarity filter introduced in 0.8.0.
- Entirely disabled random object groups recover their authored Medium-resource
  (Stalker) count. Existing positive Interloper counts are not increased.
- The NoFirearms, NoRifle, and NoRevolver gear bans are removed from Interloper's
  configuration. Unrelated gameplay tags and exclusions shared with Stalker remain, except
  for the beachcombing contents described above.
- Wildlife and survivor spawn groups retain their original rules. Weather, health,
  decay, general container quantities, and empty-container settings are not rebalanced.
- Mission progress, locks, keys, research-set selection, and DLC ownership still apply.
  Content exclusive to Wintermute or challenges is not imported into Survival.
- Reduced radio-cache and mountain-cargo alternatives remain disabled alongside
  their restored normal versions to prevent overlapping duplicate containers.

The restored content baseline is now subject to the item rarity tiers above.
All tracked documentation, code comments, test messages, and scripts are in English.

## Other included changes

- **Coastal Highway:** restore intact houses in all five randomized building groups,
  disable the Interloper-only burnt-house group, and restore the two independently
  hidden houses, including their interior access.
- **Prepper caches:** use the original normal distribution of **three furnished and
  six abandoned interiors**. The game still selects their locations.
- **Timberwolf Mountain:** restore normal cargo assemblies, including summit cargo,
  while preserving hacksaw requirements and locks.
- **Hushed River Valley:** restore the Moose-Hide Satchel at the Mysterious Signal
  Fire, keeping the original random choice between its two locations.
- **Tales:** restore unique rewards and normal radio-cache/crash-box variants.
  Alpha, Beta, Gamma, and Omega bunker contents and radio-cache container rewards
  use Stalker generation in a synchronous scope. Private loot-table clones prevent
  their cached filters from affecting ordinary world containers. The original mode
  and table references are restored before generation returns, including on exceptions.
- **Sutherland:** restore story rewards and unlocked exchanges, preserving trust,
  delivery requirements, prices, and progression. General stock now also benefits
  from the removal of object exclusions.
- **New games:** allow normal Stalker region choices and random selection, retaining
  outdoor starts. Allow up to **three already unlocked feats**. Existing saves do not
  receive a different feat selection.
- **Direct main menu:** skip boot logos, initial disclaimers, the keypress gate, and
  the menu introduction movie. Profile loading and error handling still run. This
  startup change is global; in-game cinematics retain normal playback.

Content restoration targets standard Interloper only. Pilgrim, Voyageur, Stalker,
Misery, Custom, challenges, and story mode retain their own settings.

## Existing saves

Use a **new survival game** to obtain complete world generation and the new setup
options. Loot in visited scenes is not regenerated, and searched containers are not
refilled. No save data is deleted or reset.

Saved Coastal house selections are redirected to intact variants when loading the
region. Existing bunker assignments are preserved, including Interloper-specific
interiors; the normal distribution applies when generating a new world. Restored
bunker variants remain recognizable when saving and reloading.

## Setup and use

Verified environment: The Long Dark **2.55**, Unity **6000.0.60f1**, Linux x64,
MelonLoader **0.7.3**, .NET runtime **6.0.36**, .NET SDK **10.0.x**.

Requirements: `git`, .NET 10 SDK, `bash`, `curl`, `unzip`, `sha256sum`, and `pgrep`.
Back up `~/.local/share/Hinterland/TheLongDark` before playing with mods.

```bash
./scripts/setup.sh
./scripts/launch.sh  # First run: wait for assembly generation, then close the game.
./scripts/deploy.sh
./scripts/launch.sh
```

The private runtime is installed under `~/.local/share/tld-modding/dotnet`.
`deploy.sh` requires the game to be closed, builds Release, and installs
`Mods/InterloperOverhaul.dll`. Remove an old `Mods/TheLongDarkMod.dll` scaffold if
present so only the current mod loads. To build without installing, run
`./scripts/build.sh`.

The launcher skips MelonLoader's own startup screen. On Linux, it disables ReadyToRun
for the .NET 6 runtime following intermittent loader startup failures observed during
validation. Set `DOTNET_ReadyToRun=1` to test the original runtime configuration.

Optional variables: `TLD_GAME_DIR` for another game installation and `TLD_DOTNET_ROOT`
for another runtime. `build.sh` also accepts `-p:GameDir=/path/to/game`.
Game assets, save files, generated references, and research dumps are excluded from Git.

### Steam

In Properties > General > Launch Options, enter the absolute launcher path in quotes,
followed by `%command%`. For this checkout:

```text
"/home/franco/Projects/the-long-dark-mod/scripts/launch.sh" %command%
```

Remove this option to launch without MelonLoader. To remove only this mod, delete
`Mods/InterloperOverhaul.dll` while the game is closed.

## Development and validation

- Source: `src/InterloperOverhaul/`.
- World selection policies: `shared/WorldPolicy.cs`.
- Generated references: `<game>/MelonLoader/Il2CppAssemblies/`.
- Loader log: `<game>/MelonLoader/Latest.log`.

```bash
./scripts/build.sh -p:RestoreLockedMode=true
dotnet run --project tests/PolicyChecks/PolicyChecks.csproj -c Release
./scripts/validate-runtime.sh
```

Runtime validation requires a closed game and an installed Release DLL. It temporarily
builds a validation DLL, opens the main menu without input, and exercises the actual
Harmony patches against synthetic world objects, real gear prefabs and loot filters,
and cloned bunker configurations. The isolated probe enables background execution
so losing window focus does not pause its checks. It never opens a survival save. The installed DLL
is restored afterward; the report is written to `.local/runtime-validation.log`.
Validation code is excluded from the production build. Loader, Harmony, and native interop errors fail
the validation even if individual checks report success. The injected probe may
require forced termination during Unity/.NET shutdown after reporting its results;
this does not validate a normal playthrough or normal shutdown behavior.
The 0.14.0 probe passed 664 native runtime checks and 2,220 policy boundary checks.
It was run with `DOTNET_TieredCompilation=0` and
`DOTNET_EnableWriteXorExecute=0`, as well as the launcher's `DOTNET_ReadyToRun=0`.
This records the validation environment; it is not a claim
that intermittent loader startup or probe shutdown failures have been resolved.

GitHub Actions checks policies and scripts without distributing proprietary DLLs.
Full compilation and MelonLoader validation run locally against a licensed game
installation. The mod DLL is built at
`src/InterloperOverhaul/bin/Release/net6.0/InterloperOverhaul.dll`.

## Compatibility references

- [Interloper restrictions and rewards](https://thelongdark.fandom.com/wiki/Interloper)
- Implementation is checked against components and scenes in the local 2.55 installation.
- [MelonLoader 0.7.3](https://github.com/LavaGang/MelonLoader/releases/tag/v0.7.3)
- [Native Linux setup](https://github.com/LavaGang/MelonWiki/blob/master/docs/gettingstarted.md#linux-native-games)
- [RerollBurntBuildings: Coastal Highway building groups](https://github.com/GruffCassquatch/RerollBurntBuildings)

An independent community project, not affiliated with Hinterland.
