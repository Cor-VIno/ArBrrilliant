# Display Layer Rules

This document records the current display priority rules for `02_Combat_Test`.

## Current Sorting Layers

The project currently only defines Unity's built-in `Default` Sorting Layer.

Do not add new Sorting Layers casually. For the current demo, display priority is controlled by `Order in Layer` and Canvas sorting order.

## Recommended Priority

Low to high:

1. Background / ground
2. Enemy body sprites
3. Player body sprite
4. Projectile sprites
5. Skill VFX / hit VFX
6. World Space enemy overhead UI
7. Screen Space combat UI
8. Dialogue / pause / result UI

## Current Values

### Characters

- Enemy sprite: `Default`, order `0`
- Player sprite: `Default`, order `0`

When formal character art is added, prefer:

- Enemy: order `0`
- Player: order `10`

### Projectile

No visible projectile prefab is currently bound in projectile data. When adding one, use:

- Projectile sprite: `Default`, order `20`

### VFX

Current test feedback prefabs use:

- Test Caster Burst: order `50`
- Test Forward Slash: order `51`
- Test Projectile Spawn: order `52`
- Test Dash: order `53`
- Test Perfect Dodge: order `54`
- Test Sword Art Burst: order `55`

Formal VFX should generally stay in the `30-80` range unless a specific effect must sit behind characters.

### World Space UI

Enemy overhead UI should render above characters, projectiles, and normal VFX:

- HealthBarRoot Canvas: order `100`
- ToughnessBarRoot Canvas: order `101`

Shield display on shield enemies is an overlay inside `HealthBarRoot`, not a separate active bar.

### Screen Space UI

The main combat Canvas is Screen Space Overlay and currently uses sorting order `10`.

Screen Space Overlay is not blocked by world sprites, projectiles, or world-space VFX.

Current screen UI includes:

- PlayerHealthRoot
- StrokeSlotRoot
- AvailableSwordArtHintText
- InsightSelectionRoot

Future pause, dialogue, and result UI should be placed later in the Canvas hierarchy or on a higher-priority Canvas.

## Rules For New Assets

- Do not use Physics Layers to solve render ordering.
- Do not put hitboxes or hurtboxes on visual sorting objects just for rendering.
- VFX may cover characters briefly, but should not cover overhead UI for long.
- World Space UI should use Canvas sorting orders at or above `100`.
- Screen Space UI should remain on Screen Space Overlay unless there is a specific reason to change it.
- If new Sorting Layers are introduced later, update this document and migrate deliberately.
