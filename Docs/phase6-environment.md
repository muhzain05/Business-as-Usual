# Daylight Town environment

The Phase 6 scene is `Unity/BusinessAsUsual/Assets/Town/Scenes/DaylightTown.unity`. It extends the original compact 32 × 28 m TownPrototype: the market, garage, sanitation yard, square, street and rear alley keep their established positions. The approved larger market interior sits in a connected room at x = −40 m; server-validated entry and exit move the existing player without restarting the multiplayer session. The original scenes and approved shop interior remain intact.

Open [the review gallery](Diagnostics/Phase6/review.html) for matched original/final street, square, shopfront, garage, yard, alley and overhead images. All seven pairs use identical 1280 × 720 framing. Six cameras are at 1.7 m player height; the overhead camera is at 32 m. These comparison captures are rendered by temporary Unity cameras while stopped. The profession and navigation images in the same gallery come from the running Windows game processes.

## Original art

Blender MCP created 24 reusable models plus a lower-detail roof variant in a separate library scene. The source is `Assets/Town/Working/DaylightTown_AssetLibrary.blend`; exports, normalized Unity meshes and reusable prefabs are under `Assets/Town/Art` and `Assets/Town/Prefabs/Environment`. `Scripts/create_town_assets.py` and `Scripts/refine_town_assets.py` record construction and refinement. The user's original Sunny Blender scene was restored after each modeling operation. No paid generator or downloaded asset was used.

Warm plaster, sage, dark teal, terracotta and mustard unify the town with the approved handbook palette. Original small repeating plaster, wood, asphalt and paving textures and five drawn notice posters come from `Scripts/create_town_textures.py`. Meshes reuse the shared material palette, with instancing enabled.

The square has a clock landmark, noticeboard, slatted benches and planters. Daylight Market has framed windows, a striped awning, tiled roof and readable entrance. Mikan Motor has a rounded garage opening, two original service vans, striped bays, workbench and tools. The covered sanitation yard has material-coded bins, a cart, fenced storage and a route desk. Curbs, crossings, lamps, windows, pipes, AC units, crates and neighboring houses give the street and alley context without expanding the playable boundary.

The lighting uses a warm directional sun, ambient color and restrained fog. The town cameras use the established rendering path with post-processing disabled after bloom caused excessive glare in an earlier art test. This affects only the new town cameras; the approved shop and global renderer settings are preserved. Town signs use the shop's depth-tested text shader so geometry and Sunny occlude them. Dynamic atlas updates are isolated from the font materials used by the handbook. Static scenery uses shared meshes/materials and suitable static flags. Decorative objects have no network components.

## Collision and performance

Existing core floor, wall and workplace colliders are retained. Remodeled vehicles, benches, bins, landmark stems, planters and trees receive explicit simple collision volumes. Small wall-mounted decorations do not add collision. A planter initially blocked parts-counter access; it was moved and its collision volume fitted before final validation. The black diagnostic door/workstation fixtures are excluded from the town, including automated town runs.

[The geometry report](Diagnostics/Phase6/environment-budget.json) counts 256 active environment renderers, 24 material assets and 28 distinct meshes: 399,828 triangles across instances and 117,454 unique triangles. It includes workplace fixtures and excludes the offset approved interior and runtime Sunny characters. The low-detail distant roof reduces its source from 50,888 to 3,562 triangles. These counts are geometry observations, not measured draw calls.

Two independent players traversed the street, garage entrance, yard, alley and square with production motor input. Automated navigation begins from controlled positions; it is not exhaustive human exploration. [Frame observations](Diagnostics/Phase6/performance-observations.json) document short samples from two windowed 1080p Development players while Unity was also running on this PC. Shipping hardware, GPU timings and draw calls still need profiler measurements.

Remaining visual work includes a dedicated baked-lighting pass, finer distant roof silhouettes, more varied facade wear and broader camera-collision review near interior walls and the yard fence. The scene uses real-time sunlight and ambient lighting; it does not claim a completed light bake. Vehicles are stationary service props. Background houses are scenery beyond the retained boundary. No traffic, driving, day/night cycle or expanded town was introduced.
