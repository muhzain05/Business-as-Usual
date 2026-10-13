# Art direction — initial character reference

## Direction

Create charming stylized town characters with expressive faces, exaggerated movement, and physical comedy. PEAK is a reference for readability; Business as Usual must develop its own identity rather than reproduce its characters or visual motifs.

## Preserve from Sunny

The supplied Meshy model has an oversized round head, compact torso, short limbs, rounded hands, large sneakers, a teal short-sleeve jacket over a pale shirt, dark shorts, simple raised oval eyes and a curved smile, and soft cheek color. These are observed traits of the initial character, not mandatory outfits for every future character.

Preserve the head-to-body proportions, rounded silhouette, broad color blocks, and friendly facial readability while repairing technical defects. Avoid stretching garments into body shapes to hide rig problems. Evaluate the silhouette from front, side, and back against the original reference.

## Expressions and modularity

The current mesh has separate head, eyes, and mouth components but no expression controls. Develop a small expression prototype on copies after approval. Keep the eyes and mouth attached to an independently controllable head and preserve their readability from the intended game camera distance. Shape keys or replaceable expression geometry are candidate approaches; the decision is still open.

Interchangeable outfits should respect shared neck, sleeve, waist, and ankle interfaces. Clothing should read as clothing during motion, with thickness or concealed openings wherever the camera can expose them. The current upper outfit merges jacket, shirt, and collar; true independent layers will need new hidden surfaces.

## Technical art priorities

1. Stable shoulders, jacket hem, pelvis, knees, and ankles in locomotion and exaggerated poses.
2. A coherent shared skeleton and reproducible bind/rest pose.
3. A base body that supports clothing swaps without holes or visible intersections.
4. Materials with explicit color/data texture conventions and portable texture references.
5. Expression controls and later physical acting, with ragdoll construction deferred.

The measured 15,304 triangles are a starting point, not an approved platform budget. Rendering platform, camera, performance budget, LOD requirements, and final world scale are undecided. Do not sacrifice the characteristic proportions merely to match realistic human anatomy.

## References

- [Original front/side/back](Diagnostics/original-views.png)
- [Measured connected components](Diagnostics/connected-components.png)
- [Walking samples](Diagnostics/walk-frames.png)
- [Running samples](Diagnostics/run-frames.png)
