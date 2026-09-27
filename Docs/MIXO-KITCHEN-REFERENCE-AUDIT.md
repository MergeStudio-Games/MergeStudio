# Mixo Kitchen reference and asset audit

Date: 2026-09-25

## Product direction

The visual reference is a **triple-match object puzzle**, not the current
merge-2 board. The target loop is:

1. Food and kitchen objects appear in a layered, partially occluded pile.
2. The player taps an exposed object and sends it to a seven-slot tray.
3. Three identical objects clear with animation, sound and haptic feedback.
4. The level is won when the pile is empty and lost when the tray fills.
5. Later levels add more object families, deeper layers, blockers, timers,
   objectives and boosters.

The current repository is valuable as studio infrastructure, but it is not a
usable visual or gameplay foundation for this target. It currently contains a
7x9 merge-2 domain model and runtime-generated placeholder UI, with no food
images, 3D models or authored prefabs.

## Sources evaluated

| Source | What it provides | License/readiness | Decision |
| --- | --- | --- | --- |
| [Kenney Food Kit](https://kenney.nl/assets/food-kit) | 200 optimized food and kitchen models in GLB, FBX and OBJ formats, plus previews | CC0; suitable for commercial use | Use as the first production art library after Unity import and visual normalization |
| [Quaternius Ultimate Food Pack](https://quaternius.com/packs/ultimatefood.html) | 103 food and consumable models in FBX, OBJ and Blend formats | CC0; suitable for commercial use | Use as a secondary library after art-direction compatibility review |
| [SerhatKarabag/unity-match-3d-puzzle](https://github.com/SerhatKarabag/unity-match-3d-puzzle) | Seven-slot triple matching, boosters, ScriptableObject levels, editor, 23 objects and 10 levels | No repository license; Unity 2021.3.1f1; no tests | Architecture reference only; do not copy code or art into the product |
| [wgrodzicki/triple-match-frenzy](https://github.com/wgrodzicki/triple-match-frenzy) | Layered tile generation, occlusion graph, tray pooling and async animation flow | No repository license; incomplete LFS objects prevent a clean clone | Architecture reference only; do not copy code or art into the product |
| [Triple Match 3D template](https://www.unitytemplate.com/product/triple-match-3d-relaxing-puzzle) | Full commercial template with progression, boosters, UI, save and monetization claims | Third-party marketplace, USD 249, few sales and no useful reviews | Do not make it the production base without a source-code and license audit |
| [Cooking Game Template](https://www.gameassetdeals.com/asset/334514/cooking-game-template-restaurant-system-with-210-levels-boosters) | 210+ time-management cooking levels, customers, equipment and recipes | Commercial Unity Asset Store package; landscape-only and different gameplay | Not suitable as the core; useful only as a progression/economy reference |

## Recommended production architecture

- Keep the repository's save recovery, localization, event channels, CI and
  Android tooling.
- Replace `MergeBoard` gameplay with a tested pure-C# triple-match domain:
  deterministic level seed, tray capacity, match resolution, occlusion graph,
  win/loss rules and solvability validation.
- Use ScriptableObject level definitions and an editor that can generate,
  preview, validate and batch-balance 100 authored levels.
- Render food as small, consistently scaled 3D objects in a bounded central
  play area. Keep clear top HUD and bottom tray/booster safe zones.
- Import a curated subset of Kenney models first. Normalize pivots, bounds,
  material palette, collider proxies, thumbnails and addressable labels through
  an editor import pipeline instead of hand-editing every model.
- Add object pooling, GPU instancing/SRP batching, atlas-backed UI, URP mobile
  lighting, particles, audio pooling and haptics with device quality tiers.

## Level curve for the first 100 levels

| Range | New complexity |
| --- | --- |
| 1-10 | 6-10 food types, shallow pile, no timer, guided tray tutorial |
| 11-25 | 10-16 types, deeper overlap, undo and hint introductions |
| 26-40 | Mixed ingredient/dish families, timer goals and frozen objects |
| 41-60 | Containers, covered objects, limited shuffles and two-part goals |
| 61-80 | Dense piles, moving trays, recipe collections and tighter timers |
| 81-100 | Combined blockers, multi-stage boards, scarce boosters and mastery levels |

Every generated board must contain object counts divisible by three and must
pass a deterministic solver before it is accepted. Difficulty should be driven
by measured branching, occlusion depth, tray pressure and expected solution
length rather than object count alone.

## Local research downloads

The inspected files are kept under the ignored `Build/Research` directory:

- `Build/Research/KenneyFoodKit`
- `Build/Research/kenney_food-kit.zip`
- `Build/Research/unity-match-3d-puzzle`
- `Build/Research/triple-match-frenzy`

Only the Kenney package is currently cleared for direct product use. The two
GitHub projects remain reference material because neither repository declares
an open-source license.
