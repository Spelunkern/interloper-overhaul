# Changelog

## 0.20.0

- Start Steam before exporting the mod loader environment and set the native app IDs
  in the desktop/Steam launcher, preventing the missing-client -998 startup failure.

- Gate newly restored equipment by audited access difficulty: no added clothing or
  tools in ordinary/renewable sources; premium clothing requires deep exploration.
- Use real source-container geography when filtering shared gear prefabs, and give
  beach sources precedence over surrounding deep locations.
- Move new-run finite weapons/tools to four distinct expedition destinations while
  retaining the complete legacy placement catalog and saved issuance snapshots.
- Reduce ordinary mailbox research books from 4% to 0.25%, with explicit remote/deep
  rates; preserve papers, common books, Book Smarts, and existing item weights.
- Theme remote/deep mementos around expedition supplies, with one 1% rare-equipment
  slot in deep mementos. Authenticate Tales exceptions by scene and source hierarchy.
- Require the Cannery for integrated manufacturing, increase tool-refining materials
  and time, and require quality tools. Increase flare-shell inputs and crafting time.
- Double noisemaker powder costs while preserving discovery, ordinary-workbench
  crafting, damage, weight, and morale changes; scale chemical spawn floors by access.

## 0.19.0

- Rework frostbite into temporary, independently recoverable body-area afflictions
  dealing ten condition points per game hour each, with rapid warm recovery and
  no permanent condition cap. Preserve partial recovery and migrate legacy cases.
- Keep initial severe-laceration attack damage and wounds while removing the
  seventy-two-hour recurring syndrome and its periodic extra wounds and risks.
- Disable cabin fever and clear existing cabin fever and risk.
- Give parasites fixed immediate 20% condition/fatigue-cap penalties, one native
  medication dose, and twenty-four hours of rest, preserving treatment on reload.
- Restore discovery-based noisemaker recipe learning without removing ordinary
  workbench crafting or previous noisemaker balance.
- Double prybar struggle damage and flee multipliers.
- Increase vehicle interior warmth to 15°C and built-in bed warmth to 5°C.
- Add persistent searchable vehicle fuel tanks with rare 0.05–0.15 L kerosene finds.
- Stock native mailbox spawners with frequent newspapers and rare ordinary or
  research books using deterministic selections and native pickup persistence.
- Reduce rock-cache construction to twenty-five stones and cap dismantling refunds
  at the new cost without changing capacity, build time, or regional limits.

## 0.18.0

- Lower Cooking 5 to 250 points; restore food-specific poisoning and parasite
  risks only when the consumed food is ruined.
- Add a one-minute, anywhere recipe for a Cattail Bundle: ten whole stalks,
  1,500 calories, 0.50 kg, native icon and nutrients, with partial-food save support.
- Set actual pemmican decay to 0.1% per day indoors and outdoors.
- Track mixed jar/bag salt mass in curing boxes and save it without changing yield.
- Reduce water purification tablets, noisemaker, and stump remover weights.
- Increase snow shelter warmth to 30°C and bed warmth to 5°C; halve all shelter decay.
- Allow ordinary-workbench noisemaker crafting, raise wildlife and fixed player
  explosion damage to 250, and strongly reduce timberwolf morale on native events.
- Increase stump remover and sulfur availability while retaining rare finished ammo.
- Replace newly generated fixed memento-cache rewards with deterministic random
  ordinary supplies, preserving searched caches and unique Tales rewards.

## 0.17.0

- Make all seven tinder types grant an actual fire-start success bonus at every
  skill level, retaining optional tinder at levels 3–5 and showing the chosen bonus.
- Rebalance paper, cash, firelog, coal, reclaimed wood, and stick weights; increase
  book, firelog, and stick duration, strengthen firelogs, and remove reclaimed wood's
  negative fuel success modifier.
- Increase Blizzard Walker to 50%, Book Smarts to 100% extra research XP, and
  Straight to the Heart to 100% longer eligible effects; retain native feat eligibility.
- Lower Mending level 5 to 250 points and allow paid native repair actions on ruined
  gear at that level without granting free condition for inspection or cancellation.
- Halve cougar health and passing swipe damage while preserving wounds and
  native-compatible save health, without repeated scaling on load.

## 0.16.0

- Rebalance Interloper clothing weights, warmth, wind protection, protection, and
  sprint penalties; retain the improvised crampons' climbing benefits.
- Reduce the requested food and ingredient weights and increase tomato soup,
  canned ham, pineapple, cereal, dried apple, and pickle calories.
- Preserve partially consumed food and cooking calorie bonuses across legacy-save
  migration and repeated reloads. Ingredient recipe quantities remain unchanged.
- Match acorn coffee's fatigue benefit to normal coffee and add birch-bark condition
  restoration to burdock tea without removing its medicinal effect.
- Keep these changes exclusive to standard Interloper.

## 0.15.0

- Set the Ballistic Vest to 2 kg, 20% maximum protection, and a 10% sprint-bar penalty in Interloper.
- Add portable vest repairs: one cured leather, 30 base minutes, and ten condition percentage points, capped at full condition, without tools, a workshop, or repair failure.
- Reduce the moose-hide satchel and wool ear wrap to 0.10 kg each.
- Turn the Foreman's Tool Belt and Rifle Holster into permanent per-run collectible upgrades, with no carried item, equipment slot, condition requirement, or clothing penalty.
- Give the belt a 50% reduction for one heaviest carried item per tool type, without a type limit. Exclude weapons and ammunition, and group functional tool variants together.
- Give the holster a 50% reduction for the heaviest carried rifle, including all rifle variants. Prevent native wearable bonuses and duplicate pickups from stacking.
- Persist unlocks in the native save slot, reset them on new runs, migrate existing carried/equipped accessories, and retain original behavior outside Interloper.
- Add policy and runtime probes for armor repair, accessory weights, selection, carried totals, collectible pickup, saves, equipment blocking, migration, and difficulty isolation.

## 0.14.0

- Apply the requested lighter weights to can openers, all insulated flasks, recycled cans, improvised tools, stones, standard arrows, both hand flares, car batteries, replacement fuses, all ordinary/research books, snares, ropes, woodworking tools, and both spray-paint can models.
- Increase standard and Spelunker's lantern warmth from 3°C to 6°C using their native heat sources.
- Remove passive condition decay from insulated flasks without repairing existing wear, changing fall damage, preventing drink cooling, or removing contents weight.
- Keep spray paint at 0.20 kg total while preserving its remaining paint and uses.
- Apply item balance to existing and newly created Interloper gear, preserve native stack/content calculations and saves, and isolate authored assets and other difficulties.
- Add runtime checks for native prefabs, weight previews, save/reload, stacks, flask contents/aging, lantern warmth/fuel, and difficulty isolation.

## 0.13.0

- Replace authored variant weapon placements and selected Tales/cache variant rewards with physical, variant-specific Workshop Schematics. Preserve native placement selection and reward progression.
- Add eleven Cannery-only upgrades, including the Bushcraft Bow, with learned schematic requirements, reusable tools, light, and explicit material/time costs.
- Add seven time-only firearm reversals. Reserve the source weapon during work, preserve its condition and native identity, reject ruined/loaded inputs, and save recipe-specific partial progress.
- Learn picked-up schematics permanently per run, record their descriptions in journal notes, and avoid inventory clutter or accidental vanilla Bushcraft unlocking.
- Restore variant Trader eligibility through the native Stalker checks and retain costs and story unlocks; convert received variant rewards into learned schematics.
- Allocate exactly two standard flashlights at distinct verified world sites and one Miner's Flashlight at the native Concentrator watchroom for new runs. Persist separate native identities and creation flags; suppress additional loot/beachcombing/Trader lights.
- Migrate finite allocations to version three without adding flashlights to older saves or changing existing firearm/tool identities.
- Extend policy and native runtime probes for allocation persistence, physical schematic saves, recipe requirements, reservations, rollback, and conversion identity/condition.

## 0.12.0

- Integrate eight ammunition-workbench recipes: refine improvised knives/hatchets, produce four arrow shafts per cured birch sapling, make flare shells from unused red/marine flares or measured gunpowder, and recycle three unused flares into 100 g powder.
- Use the current native blueprint system and existing crafting UI without ModComponent, settings, custom item bundles, or additional mod dependencies.
- Restrict recipes to standard Interloper, reuse authored Wwise events to avoid the 2.55 JSON audio factory's obsolete reflection fields, and register recipes idempotently.
- Preserve manufactured output through the loot filters. The finite knife/hatchet quota applies to world allocation; paid upgrades are repeatable. Firearms, variants, beachcombing exclusions, and found-ammunition scarcity retain their rules.
- Exclude used/lit/wet flares and consume eligible inputs explicitly during manufacturing. Keep gunpowder output at its measured amount through native saves.
- Enable native milling repair/restoration for the distress pistol and standard crampons, attaching their component before gear deserialization to preserve restoration progress.
- Retain native machine access and power requirements; omit reloading crates, forced firearm conditions, cannery-code bypasses, and settings from the reference mods.

## 0.11.0

- Allocate exactly one hunting knife and one standard hatchet per new Interloper run, with seed-selected native world positions and 1–100% initial condition.
- Persist tool locations, creation flags, and stable native gear identities alongside the firearm allocation; never replace lost, ruined, or harvested tools.
- Suppress additional standard tools in world/container/reward loot, Trader offers, and all fresh beachcombing finds, including large-find contents. Preserve improvised tools and crafting.
- Upgrade allocation payloads to version two while preserving version-one firearm plans and existing tools without adding extras. Existing saves require a new run for the tool-count guarantee.
- Validate tool serialization and wear, save migration, repeat population, additional world/scripted loot, beachcombing rejection, and Trader stock.

## 0.10.0

- Temporarily disable new rifle/revolver variants, special bows, and the Survival Knife in standard Interloper.
- Block fresh world placements (including skipped spawn rolls), random/scripted container rewards, beachcombing finds, and Trader offers, including variant special requests.
- Preserve saved/acquired variants, other difficulties, and the existing one-standard-rifle/one-standard-revolver allocation.
- Resolve variant Trader identities from the installed 2.55 DLC catalog without loading assets in stock filters.

## 0.9.0

- Allocate exactly one standard rifle and one standard revolver for each new Interloper run, independently selected from 28 audited base-game outdoor placements.
- Start allocated firearms at a deterministic random condition from 1% to 100%, with no loaded ammunition, starting rounds, or spent casings.
- Persist allocation and independent creation flags inside the native save slot; use stable native object GUIDs and normal gear serialization to preserve later condition and loaded rounds.
- Suppress additional standard firearms in generated world/container/radial loot and Trader stock, while retaining special variants and existing acquired/saved weapons.
- Reduce found rifle/revolver ammunition retention to 0.25% and found casings to 0.5%; preserve crafting and recovered player-fired casings.
- Apply stable ammunition/casing availability per Trader aurora without mutating the cached stock or special-request rewards.
- Preserve pre-allocation saves without injecting extra guns; require a new run for the exact two-weapon allocation.
- Extend policy and native-runtime validation for allocation, save payloads, weapon initialization/serialization, and Trader supplies. Allow more time for slow native startup in the isolated probe.

## 0.8.0

- Add an independent rarity roll for newly generated restored loot: 5% ordinary items, 1% premium gear, and 0.25% special weapon variants.
- Filter newly selected random and scripted container loot without replacement, including restored reward containers and all-premium tables.
- Apply rarity to fresh world placements, including guaranteed spawns, and newly generated radial/beachcombing finds while preserving consumed counts and respawn timers.
- Track restored scene sources before their difficulty disablers are removed; preserve native Interloper placements even when the canonical prefab was excluded from container tables.
- Preserve unique Tales rewards, the HRV Signal Fire satchel, empty casings, collectibles, structures, and existing player/saved inventory. Keep fixed Trader exchanges unchanged.
- Use stable world-seed/location/item/opportunity sampling without consuming the shared random stream; leave authored table weights and spawn settings intact.
- Add distribution, real-prefab tier, native generation-hook, and synthetic radial/saved-item checks.

## 0.7.0

- Restore remaining difficulty-excluded beachcombing contents, including canoe arrows and the broken arrow that are also excluded on Stalker.
- Limit this additional override to known large beachcombing prefab hierarchies on Interloper; preserve exclusions shared by all three ordinary Survival modes.
- Preserve original loose-item and large-find tables, weights, location compatibility, timing, quantities, expiry, and locks.
- Validate actual normal/post-blizzard loot-table entries and original large-find prefabs from the local game installation.
- Keep Addressables references and the beachcombing validation probe out of production build dependencies.

## 0.6.0

- Restore normal Survival object eligibility throughout Interloper, including weapons, ammunition, tools, clothing, unique variants, plants, and world-object parent groups.
- Remove NoFirearms, NoRifle, and NoRevolver gear bans from Interloper while retaining unrelated tags, quest gates, and DLC requirements.
- Restore authored Medium-resource counts for completely disabled random object groups, without increasing existing positive Interloper counts.
- Preserve wildlife and survivor spawns, reduced-cache exclusions, and unrelated survival settings. Item-specific balancing is deferred.
- Validate actual formerly excluded gear prefabs against native loot filtering, along with positive-count and difficulty boundaries.
- Translate all repository documentation into English.

## 0.5.0

- Restore normal Timberwolf Mountain cargo assemblies and disable their Interloper-only alternatives.
- Restore the Moose-Hide Satchel at either possible Mysterious Signal Fire location, retaining the original random selection.
- Allow empty rifle and revolver casings in original loot tables and locations, without globally enabling firearms or live ammunition in this version.
- Enable region selection using normal Stalker eligibility while retaining outdoor starts.
- Allow three already unlocked feats for new Interloper games.
- Fix Tales reward generation by avoiding hooks on private getters whose unused arguments are optimized out by the native compiler. Restore the generation context on exit.
- Check real casing and ammunition prefabs and native starting-region validation.

## 0.4.0

- Restore excluded unique Tales rewards, including the Tactical Jacket and Security Chief's Rifle Holster.
- Restore normal radio-cache and crash-box variants, disabling reduced alternatives to prevent duplicates.
- Restore difficulty-excluded content in the four Signal Void bunkers.
- Generate these container rewards with Stalker filtering and reductions, using private table clones to protect global loot.
- Restore exchanges unlocked by Sutherland's special requests while preserving trust, deliveries, and progression.
- Preserve unrelated filters, research-set selection, locks, and mission progress. Do not regenerate searched containers.

## 0.3.0

- Open the main menu automatically, skipping startup logos, disclaimers, and the menu introduction movie.
- Continue after loading without waiting for a keypress.
- Preserve profile loading, file loading, and error handling.
- Validate the visible menu without keyboard or mouse input.
- Disable ReadyToRun in the Linux launcher after observing intermittent CLR 6 startup failures.

## 0.2.0

- Adopt the name Interloper Overhaul.
- Restore original intact Coastal Highway houses on Interloper, including saved-selection reloads.
- Selectively restore world containers, mementos, caches, and map objects excluded by difficulty.
- Use normal bunker distribution in new worlds while preserving existing saved assignments.
- Leave loot tables and parameters unbalanced in this version.
- Add policy checks and in-game runtime validation, separate from the production build.

## 0.1.0

- Add the C# and MelonLoader scaffold, Linux scripts, and public repository.
