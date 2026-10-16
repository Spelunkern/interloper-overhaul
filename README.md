# Interloper Overhaul

A personal mod for **The Long Dark**, focused on restoring content excluded from
standard **Interloper**, with a few general quality-of-life changes. By Spelunkern.

## Version 0.20.0: exploration-driven loot

These rules supersede the earlier restoration distributions in standard Interloper.
Existing rolled, saved, owned, and stored items remain intact. Start a new run for
new firearm/tool locations; existing allocation snapshots retain every old location
and creation flag, including items in unloaded scenes.

| Newly restored source | Premium clothing | Other clothing and tools | Ordinary supplies |
| --- | --- | --- | --- |
| Ordinary buildings, vehicles, and containers | Excluded | Excluded | 5% retention |
| Renewable beachcombing, including large containers | Excluded | Excluded | 5% retention |
| Remote destinations and ordinary memento/radio caches | Excluded | 1% retention | 5% retention |
| Verified deep destinations and Tales bunkers | 1% retention | 5% retention | 5% retention |

These are additional retention rolls after native selection, not chances per
container. Original Interloper sources keep their availability; a native world
hammer or native beach find is not reclassified just because its item can also
occur in a restored source. Finite equipment, ammunition, casings, and authenticated
progression rewards have their separate rules. Container decisions use the actual
container's scene, hierarchy, and position rather than its canonical item prefab.

Deep destinations are the Timberwolf Mountain summit tail and summit cave, the
Ash Canyon gold-mine interior, the two HRV signal-fire destinations and Lake Cave
site, the audited HRV1 lockbox and HRV2 rock cache, the Concentrator watchroom,
and Tales bunker interiors. Outdoor HRV areas
use small three-dimensional destination bounds; the rest of the region remains
ordinary. Remote supply access includes normal radio/memento caches, the Coastal
lookout destination, and the old Mystery Lake lookout. Beach sources take precedence
regardless of which scene contains them. Classification does not promote an entire
outdoor region just because that region is dangerous.

- New runs allocate the single rifle, revolver, hunting knife, and hatchet across
  four distinct destinations selected from the summit cave, Ash Canyon gold mine,
  HRV Lake Cave, and the two HRV signal-fire destinations. Conditions, empty guns,
  permanent issuance flags, and no beach/Trader replacements remain unchanged.
- Mailboxes retain 75% paper and 4% common books. Research books fall to 0.25% in
  ordinary locations, 0.5% at remote destinations, and 1% at deep destinations.
  The remaining outcomes are empty; Book Smarts and book weights are unchanged.
- Mementos remain deterministic and supply-based. Remote/deep caches favor repair,
  medical, fire-starting, and ammunition ingredients. Only the first slot of a deep
  memento can roll rare equipment (1%); ordinary/remote caches cannot. Existing
  searched contents are preserved. Tales reward exemptions require a native Tales
  bunker scene and its Design hierarchy, rather than just a reward item name.
- All eight integrated manufacturing recipes require the Cannery ammunition bench,
  enforced in both blueprint availability and the native start operation. Tool
  refining requires quality tools, more scrap/leather, and four/six hours. Flare
  shells cost two unused flares or 100 g powder and take ninety minutes.
  Refining remains repeatable and does not consume the unique world allocation.
- Noisemakers still require discovery and an ordinary workbench, retain their
  requested weight/damage/morale effects, and now consume twice the native powder.
  Ammunition ingredients keep their rarity exemption, with spawn-chance floors of
  25% at ordinary/beach sites, 50% remote, and 75% deep; higher native chances remain.
- Existing item weights, permanent accessories, feats, and variant schematics keep
  their configured benefits. Variant crafting and time-only firearm reversal remain
  subject to the learned schematic and Cannery access.

Verified coordinates come from the locally installed 2.55 assets. Added finite
placements use stable separate identities and audited gear positions; the former
site catalog is retained exclusively to resolve saved allocations.

## Version 0.19.0: temporary afflictions, defensive tools, and roadside supplies

These changes apply to standard Interloper, including loaded afflictions.

- **Frostbite:** reaching a body's full frostbite meter causes one temporary
  affliction for that area. Each affected area drains ten condition points per
  game hour; different areas stack. There is no permanent condition cap or
  immediate condition loss. Native cold, clothing, and frozen-clothing checks
  still fill meters. Warming reduces meters rapidly, with a full meter recovering
  in at most fifteen warm game minutes. An area's affliction ends only when its
  meter empties. Recovery and separate areas survive saves; old permanent cases
  become temporary full meters without granting free condition healing.
- **Severe lacerations:** keep the initial attack damage and initial wounds, then
  immediately end the ongoing syndrome. No seventy-two-hour condition/stamina
  debuff, recurring bleeding, or extra periodic infection risks. Existing recurring
  laceration syndromes are also cleared; their existing wounds still need care.
- **Cabin fever:** cannot develop. Existing cabin fever and risk are cleared.
- **Intestinal parasites:** immediately reduce maximum condition and fatigue
  capacity by 20%; those penalties never escalate. Treatment is one normal dose
  of antibiotics or reishi tea, followed by twenty-four accumulated hours of
  sleeping or passing time. Medication and partial recovery persist through saves.
  Rest before medication does not count. Native infection risks remain in place,
  including the previous Cooking-5 exception for ruined foods.
- **Noisemakers:** must still be found to learn the survival recipe. Keep ordinary
  workbench crafting and the previous weight, damage, and morale changes. Existing
  legitimately learned recipes remain learned.
- **Prybar:** double its native struggle damage multiplier (1.25 to 2.5) and
  double its flee-chance multiplier (1.5 to 3). Other struggle properties remain.
- **Vehicles:** +15°C interior warmth and +5°C built-in bed warmth. Fuel covers
  gain a native searchable container with a stable ID. On first generation, 10%
  of tanks contain a fuel bottle holding 0.05–0.15 litres of kerosene. Containers
  and the small fuel amounts use native saves; reopening and reloading do not
  regenerate loot. Industrial tanks and ordinary glove-box/trunk loot are unchanged.
- **Mailboxes:** use the native physical-item spawner, with a stable per-run and
  per-mailbox selection: 75% newspaper, 4% ordinary book, 4% skill book, 17% empty.
  Book selection includes nine common and nine research books. These finds use
  their own rarity rather than receiving the restored-loot filter a second time.
  Existing native gear persistence continues to track collected items.
- **Rock caches:** require twenty-five stones instead of fifty. The native
  requirement checks, placement blueprint, and completed build share the new cost.
  Dismantling returns no more than twenty-five stones, preventing material duplication.
  Capacity, build time, and region limits stay unchanged.

## Version 0.18.0: cooking, shelters, explosives, and memento rewards

These changes apply to standard Interloper.

- **Cooking 5:** 250 points instead of 350. Its food-poisoning and parasite
  immunities remain active for food above zero condition. Ruined food uses its
  own native risks again; foods without parasite risk do not acquire it.
- **Cattail Bundle:** manually craft ten whole, uneaten stalks into one bundle
  in one minute, anywhere, without tools, light, or fire. It has the stalk icon,
  1,500 base calories, 0.50 kg, and ten stalks' nutrients and eating time.
  Partially consumed bundles preserve their remaining calories and proportional
  weight through inventory, container, and world saves. Bundles are crafted only.
- **Pemmican:** loses 0.1% condition per game day indoors and outdoors, including
  compensation for Interloper's ordinary deterioration multiplier.
- **Curing boxes:** salt displays use its actual reduced carried mass, accounting
  separately for jars (0.10 kg) and bags (3 kg). Mixed deposits and subsequent
  consumption persist per box in the save. Native curing yield, resource amounts,
  and capacity are preserved; capacity is shown as a percentage. Salt already
  stored before this update uses the jar conversion because its source is unknown.
- **Water purification tablets:** 0.10 kg per native unit instead of 0.25 kg.
- **Snow shelters:** +30°C inside, with a +5°C native bed bonus while sleeping
  or passing time. Ordinary daily and blizzard deterioration are both halved.
- **Noisemakers:** 0.10 kg and craftable at ordinary workbenches, with the native
  materials and time; the survival recipe requires finding a noisemaker to learn
  it. Wildlife blast damage is 250
  instead of 50. Player blast and in-hand detonation damage are fixed at 250;
  original blast radii and native injuries remain. Their native thrown/hit morale
  events impose at least a 100-percentage-point timberwolf morale penalty.
- **Ammunition ingredients:** stump remover weighs 0.20 kg. Stump remover and
  dusting sulfur retain all restored opportunities, bypassing the mod's 5% restored
  loot filter. Authored world placements have at least 75% base spawn chance;
  ordinary difficulty scaling and location selection still apply. Memento supplies
  can also include either ingredient. Finished ammunition and casings retain their
  existing rarity rules.
- **Mementos:** newly generated contents under authored memento cache branches
  use random ordinary food, repair supplies, medical supplies, or ammunition-making
  ingredients instead of fixed premium equipment. Each authored reward slot gets
  one selection, stable for the run and cache. The pool contains no firearms,
  variants, premium clothing, or finished ammunition. Existing searched contents,
  player storage, and unique Tales rewards are preserved.

## Version 0.17.0: fire materials, feats, Mending, and cougars

These changes apply to standard Interloper, including existing gear and saves.
Fuel durations are base values before normal Firestarting and fire-duration modifiers.
Success bonuses are percentage points, added before the native 0–100% limit.

| Material | Weight | Fire benefit |
| --- | --- | --- |
| Newspaper, rolled newspaper, stack of papers | 0.05 kg each | +8 success points when used as tinder |
| Tinder plug, birch bark, cattail head | Unchanged | +4 success points when used as tinder |
| Cash | 0.01 kg | +4 success points when used as tinder |
| Ordinary and research books, including open models | 0.20 kg | 30 base minutes |
| Firelog | 1.00 kg | +50 success points as fuel, 180 base minutes, +10°C |
| Coal | 0.20 kg | |
| Reclaimed wood | 0.20 kg | Neutral fuel success modifier |
| Sticks | 0.10 kg | 10 base minutes |

Tinder remains required at Firestarting levels 1–2 and optional at levels 3–5.
Selecting it improves both the displayed chance and the actual fire-start roll.
The tinder label displays its bonus. Native consumption rules still apply, and
repeated previews do not accumulate bonuses or modify the selected fuel permanently.

- **Blizzard Walker:** reduces the headwind walking penalty by 50%.
- **Book Smarts:** grants 100% extra research skill points (2× normal XP).
- **Straight to the Heart:** extends eligible stimulant effects by 100% (2× duration).

Feats still require their normal unlocks and must be selected for the run.
Descriptions use the new benefits; other difficulties restore the authored values.

**Mending level 5 requires 250 points**, instead of 350. Earlier thresholds and
repair rewards remain unchanged. At level 5, ruined gear with an existing native
repair action can be repaired through the normal examine menu. Materials, tools,
time, success chance, and condition gain remain those of that item's repair action.
The item stays at zero condition until a repair succeeds; viewing or cancelling
repair does not make it usable. Food and objects without a native repair action
are not made repairable. The level-5 skill description mentions the new benefit.

**Normal and aurora cougars have half health**: the local authored 300 HP becomes
150 HP. Their passing swipe's base player damage range drops from 20–40 to 10–20;
the same 50% scaling applies to passing attacks against other wildlife. Native
protection calculations, attack timing, injuries, and struggle behavior remain in
place. Existing wounds retain their proportion of maximum health. Save files keep
the native HP scale, so loading repeatedly does not halve health again.

## Clothing, food, and drink balance (0.16.0)

The following changes apply to standard Interloper, including existing gear.
Clothing values are at full condition; protection and sprint penalties use percent.

| Clothing | Weight | Other changes |
| --- | --- | --- |
| Rabbitskin hat | 0.25 kg | |
| Hardhat | 0.50 kg | 10% protection |
| Cotton scarf | Unchanged | +1°C wind protection |
| Bearskin coat | 4.00 kg | 5°C warmth, 15% sprint penalty |
| Moose-hide cloak | 3.50 kg | 4°C warmth, 25% protection |
| Wolfskin coat | 2.50 kg | 10% protection, 5% sprint penalty |
| Wolfskin pants | 2.00 kg | |
| Deerskin pants | 1.50 kg | |
| Improvised crampons | 1.00 kg | No protection, 10% sprint penalty; climbing benefits retained |

| Food or ingredient | Full weight | Full calories, where changed |
| --- | --- | --- |
| Condensed milk, dog food | 0.20 kg each | |
| Peaches | 0.25 kg | |
| Tomato soup | Unchanged | 600 |
| Sodas, including the energy drink | 0.20 kg each | |
| Canned corn | 0.25 kg | |
| Canned ham | Unchanged | 1,500 |
| Canned pineapple | 0.25 kg | 600 |
| Cereal | Unchanged | 1,000 |
| Dried apples | Unchanged | 600 |
| Pickles | 0.50 kg | 800 |
| Cooking oil | 0.50 kg | |
| Flour | 1.00 kg | |
| Potatoes, raw and cooked | 0.10 kg each | |
| Oats | 0.75 kg | |
| Salt | 0.10 kg | |
| Salt bag | 3.00 kg | |

Oil, flour, oats, and salt retain their native recipe quantities and container
capacities. Their carried mass scales with the remaining contents. Existing food
retains its consumed proportion and cooking calorie bonus when migrated; repeated
save/load cycles do not multiply calories or refill empty food.

Acorn coffee matches normal coffee: 10% immediate fatigue recovery, 50% fatigue
accumulation for one hour. Burdock tea retains its medicinal effect and gains the
native birch-bark restoration benefit: 2.5 condition per hour for two hours.
Other difficulties retain their original clothing, food, and drink behavior.

## Armor balance and permanent accessory upgrades (0.15.0)

In standard Interloper, the **Ballistic Vest** weighs **2 kg**, provides **20% maximum
protection**, and reduces the sprint bar by **10%**. Its portable native repair action
requires **one cured leather**, takes **30 base minutes**, and restores **10 percentage
points** of condition, capped at 100%. No tool or workshop is required, and this repair
has a guaranteed success chance. Existing wear is preserved; repairs do not turn the
vest into a new item. The moose-hide satchel and wool ear wrap each weigh **0.10 kg**.
The satchel retains its normal equipment slot and carrying-capacity behavior.

The **Foreman's Tool Belt** and **Rifle Holster** become permanent, run-specific
collectible upgrades. Picking either up unlocks its bonus and removes the physical
item. Neither occupies an accessory slot, adds carried weight, needs to be worn,
or imposes a clothing penalty. Duplicate pickups do not stack. The world models,
reward locations, costs, and progression still apply. English pickup notifications,
item descriptions, and journal notes explain the permanent unlocks.

- **Tool Belt:** reduces the weight of the heaviest carried item of each tool type by
  **50%**, with no three-type limit. Weapons and ammunition are excluded. Tools use the
  game's Tool category; knife, hatchet, toolkit, lantern, flashlight, and spray-paint
  variants share their functional type. A stack contributes only one item's weight.
- **Rifle Holster:** reduces the weight of **one heaviest carried rifle by 50%**, across
  common rifles and all rifle variants, including Ranger's Rifle. It does not reduce
  revolver or bow weight. The belt provides no additional weapon discount.

Selection follows the current inventory and actual item weight, so dropping or
storing the heaviest eligible item immediately changes the selected item. The bonuses
apply to carried weight without changing the physical weight of dropped or stored
gear. Native wearable belt/holster reductions are suppressed to prevent double counting.

Unlock flags are stored inside the native save slot and reset for a new run. Existing
carried or equipped belts and holsters are converted into unlocks after loading; an
already worn item is removed from its slot. No new run is required for these changes.
Other difficulties retain the original items, weights, and vest repair restrictions.
Automated probes cover native repair setup, condition gain/capping, save payloads,
weapon exclusion, tool selection beyond three types, total inventory weight, pickup,
duplicate prevention, equipment blocking, and existing-inventory migration.

## Item weight and lantern warmth balance (0.14.0)

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

Standard Interloper gains eight native crafting recipes at the **Cannery ammunition workbench** (restricted in 0.20.0).
They use the existing crafting UI, light/tool requirements, material consumption,
work in progress, and native gear saves. No ModComponent, settings menu, external
blueprint files, or custom item bundles are required.

| Recipe | Consumed materials | Output | Base time |
| --- | --- | --- | --- |
| Refine a hunting knife | 1 improvised knife + 4 scrap metal + 1 cured leather; quality tools | 1 hunting knife | 240 min |
| Refine a hatchet | 1 improvised hatchet + 6 scrap metal + 2 cured leather; quality tools | 1 standard hatchet | 360 min |
| Efficient arrow shafts | 1 cured birch sapling | 4 arrow shafts | 60 min |
| Red-flare shell | 2 unused red flares + 1 scrap metal + 1 cloth | 1 flare shell | 90 min |
| Blue-flare shell | 2 unused marine flares + 1 scrap metal + 1 cloth | 1 flare shell | 90 min |
| Gunpowder shell | 100 g gunpowder + 1 scrap metal + 1 cloth | 1 flare shell | 90 min |
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

The launcher starts Steam in the background if it is absent, waits for its process,
and sets the native game app IDs before loading the mod runtime. This prevents the
-998 startup screen caused by an unavailable Steam client. Steam must be signed in;
launcher diagnostics are saved under `~/.local/state/interloper-overhaul`.
The launcher skips MelonLoader's own startup screen. On Linux, it disables ReadyToRun
for the .NET 6 runtime following intermittent loader startup failures observed during
validation. Tiered compilation and write-xor-execute are also disabled by default,
matching the runtime probe configuration. Each `DOTNET_` variable can be overridden
explicitly; these settings do not claim to eliminate every loader startup failure.

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
The 0.20.0 probe passed 1,024 native runtime checks and 3,296 policy boundary checks.
These include clothing and food migration, ingredient inventory mass, tinder UI and
success calculations, enabled feat effects, Mending thresholds, native cougar save
round trips, Cooking thresholds and immunity scopes, partial-bundle reloads and
native crafting factories, mixed curing-salt saves, shelter settings, noisemaker
recipes and damage fields, deterministic memento replacements, and isolation from
Stalker and Misery. New checks cover separate temporary frostbite gauges and
legacy migration, parasite medication/rest saves, affliction clearing, native prybar
flee calculations, vehicle warmth, actual mailbox research-book spawning, unique
fuel-tank IDs, small fuel amounts and empty-container saves, and the native rock-cache
requirement checks at twenty-four and twenty-five stones. Exploration checks cover
source-container geography, deep clothing retention, renewable-source exclusions,
authenticated native Tales generation, rejected manufacturing starts outside the
Cannery, and reversible/idempotent noisemaker powder costs. Policy checks cover new
finite destination diversity, legacy allocation compatibility, mailbox frequencies,
and deep-memento slot limits. An isolated launcher check verifies missing-Steam
startup, app IDs, and that the game preload is not inherited by Steam.
New expedition coordinates still need a full survival traversal and visual pickup
check; these menu probes do not measure a complete world's effective loot yield.
Ruined-repair, noisemaker morale, and cougar-swipe
checks observe native entry points with controlled callbacks; they do not run combat
or consume a player's repair resources. The new affliction and roadside probes
use controlled components at the main menu; they do not replace a survival
playthrough, first-aid UI inspection, or a complete vehicle-search interaction.
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
