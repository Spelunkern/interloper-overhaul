# Changelog

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
