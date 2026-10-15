# Changelog

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
