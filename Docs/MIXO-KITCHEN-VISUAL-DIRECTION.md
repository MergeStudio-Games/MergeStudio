# Mixo Kitchen visual direction

## Product identity

Mixo Kitchen uses an original boutique-bistro theme: midnight teal cabinetry, warm
travertine, brushed copper, cream porcelain and small coral/gold accents. The game
board is the restaurant counter itself, so gameplay stays readable without a large
opaque panel covering the scene.

The interface uses Nunito under the SIL Open Font License. Generated production art
is stored under `Assets/Resources/MixoKitchen`; the food set contains 36 transparent
209x209 sprites and the portrait kitchen background is 838x1877.

## Gameplay readability

- Food silhouettes must remain recognizable at 80-150 UI pixels.
- Interactive food sits on a consistent porcelain disc with a deep-teal shadow.
- Decorative detail stays at the top and outer edges; the counter center remains calm.
- The seven-slot tray is always visible and uses the darkest surface in the hierarchy.
- Selection travels to the tray on a curved 240 ms arc. Triple completion adds sound,
  particles and haptic feedback.
- Difficulty increases through item variety, pile size, layers and time pressure rather
  than by shrinking early-level items below comfortable touch size.

## Reference audit

Market screenshots were used only to compare interaction patterns and information
hierarchy. No commercial APK code, artwork, sound or branding is included.

- Cook Merge: target counts, dense food pile and lower collection area.
- Match Family / Triple Find: compact level/timer header, central pile and persistent
  booster strip.
- F-Droid Triple Match (`com.sidhant.triplematch`, GPL-3.0): offline behavior and
  seven-slot triple-match loop. The signed ARM64 APK is kept in ignored research
  storage and its GPL code is not incorporated.
- `unitycoder/MatchSweets` at commit
  `92c1961f63cddc2e8cbd73ddeea18673f1b0d61e` (MIT): reviewed for general Unity
  presentation patterns only.
- `thefcan/unity-match3` at commit
  `345d1a51d22a0f17955dfe853cf2caa706dc0bdb`: reviewed for architecture ideas;
  no code was copied because the checkout does not provide a root license file.

## Generated-art specification

The premium food sheet was generated as a regular 6x6 grid of 36 distinct foods,
using one orthographic camera, warm studio lighting, consistent scale and a genuinely
transparent background. The kitchen background was generated as a 9:20 portrait
bistro with a quiet travertine play surface and no embedded UI, text, logos or food.

Generated source sheets are retained in `Assets/Resources/MixoKitchen/UI` so crops can
be reproduced and audited. Shipped icons are separate sprites in `PremiumFood`.
