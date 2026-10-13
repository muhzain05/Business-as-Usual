# Phase 8A Rescue V2 typography

## Which font files can be embedded and redistributed with the game?

### Takeaway
All five requested candidate families and the proposed reader have SIL Open Font License 1.1 distributions in primary repositories. Preserve each family's exact copyright/license file with redistributed font assets; the selected comparison files are unmodified static TTFs downloaded from pinned primary repository commits.

### Cited Findings
- OFL 1.1 permits commercial software bundling, embedding and redistribution, subject to its conditions: retain copyright/license notices, do not sell fonts by themselves, and respect any declared Reserved Font Names in modified versions. The font license does not impose that license on the game or documents made with the font. — [Official OFL text](https://openfontlicense.org/open-font-license-official-text/).
- Candidate-specific licenses: [Dela Gothic One](https://github.com/google/fonts/blob/main/ofl/delagothicone/OFL.txt), [Bricolage Grotesque](https://github.com/ateliertriay/bricolage/blob/84745e5b96261ae5f8c6c856e262fe78d1d6efdd/OFL.txt), [Fredoka](https://github.com/hafontia-zz/Fredoka-One/blob/35c584ff23450c9bcdf8819706e12fcdeefe1712/OFL.txt), [M PLUS Rounded 1c](https://github.com/google/fonts/blob/main/ofl/roundedmplus1c/OFL.txt), [Yusei Magic](https://github.com/google/fonts/blob/main/ofl/yuseimagic/OFL.txt), [Atkinson Hyperlegible](https://github.com/google/fonts/blob/main/ofl/atkinsonhyperlegible/OFL.txt).
- These exact fetched license headers do not declare Reserved Font Names. Preserve licenses anyway; this observation is about the fetched distributions, not every historical or future release. — [Pinned download and verification manifest](font-download-manifest.json).
- Six fetched font files have valid TrueType `00010000` signatures, parse in fontTools, contain complete printable ASCII, and have no variable `fvar` table. Manifest records pinned download URLs, sizes, SHA-256, actual weight metadata, glyph counts, and default numeral advances. — [Local binary verification](font-download-manifest.json).

### Inferences
- Shipping the unchanged font files plus exact licenses in a ThirdPartyNotices/fonts folder is the simplest accountable route. TMP atlases should retain source attribution; creating an atlas must not silently discard licensing provenance.
- M PLUS and Yusei's full Japanese font binaries need not be imported for an English prototype merely to show that they were considered. Dela is downloaded because it is one of the three actual comparison headings; generate an English glyph atlas rather than all 9,194 glyphs.

### Gaps
- No legal opinion is offered beyond reading the exact licenses. No fonts have yet been imported into Unity by this research task. Font binary verification is not proof of runtime rendering or usability.

## Which expressive heading and reading pair should be tested?

### Takeaway
Compare Bricolage Grotesque + Atkinson Hyperlegible, Dela Gothic One + Atkinson Hyperlegible, and Fredoka + Atkinson Hyperlegible with identical mechanic handbook content. Bricolage is the recommended direction because it supplies expressive headings and a calmer small-optical-size label companion without making every label feel like a novelty heading.

### Cited Findings
| Family | Verified distribution/weights | Source-backed character and coverage |
|---|---|---|
| Dela Gothic One | One static Regular file, weight metadata 400 despite very heavy appearance | Flat, thick display face intended for posters/packaging. Latin support and Japanese coverage. — [Description](https://github.com/google/fonts/blob/main/ofl/delagothicone/DESCRIPTION.en_us.html), [Metadata](https://github.com/google/fonts/blob/main/ofl/delagothicone/METADATA.pb) |
| Bricolage Grotesque | Variable weight 200–800, width 75–100, optical size 12–96; foundry also ships actual static TTFs | Grotesque influences with deliberately expressive character; smaller optical sizes become more neutral and use ink traps. — [Foundry](https://github.com/ateliertriay/bricolage), [Static files](https://github.com/ateliertriay/bricolage/tree/84745e5b96261ae5f8c6c856e262fe78d1d6efdd/fonts/ttf), [Metadata](https://github.com/google/fonts/blob/main/ofl/bricolagegrotesque/METADATA.pb) |
| Fredoka | Weight 300–700, width 75–125; foundry static Light/Regular/Medium/SemiBold/Bold | Rounded headline character. Latin and Hebrew versions are documented. — [Description](https://github.com/google/fonts/blob/main/ofl/fredoka/DESCRIPTION.en_us.html), [Metadata](https://github.com/google/fonts/blob/main/ofl/fredoka/METADATA.pb), [Static files](https://github.com/hafontia-zz/Fredoka-One/tree/35c584ff23450c9bcdf8819706e12fcdeefe1712/fonts/ttf) |
| M PLUS Rounded 1c | Seven static styles: Thin, Light, Regular, Medium, Bold, ExtraBold, Black | Rounded terminals, proportional Latin optimized for text and full-width Japanese. Google description reports 8,546 glyphs. — [Primary distribution](https://github.com/google/fonts/tree/main/ofl/roundedmplus1c), [Description](https://github.com/google/fonts/blob/main/ofl/roundedmplus1c/DESCRIPTION.en_us.html) |
| Yusei Magic | One static Regular, weight metadata 400 | Permanent-marker handwriting, thick vertical/thinner horizontal strokes. Google Latin Core and Japanese coverage are documented. — [Foundry](https://github.com/tanukifont/YuseiMagic), [Description](https://github.com/google/fonts/blob/main/ofl/yuseimagic/DESCRIPTION.en_us.html), [Metadata](https://github.com/google/fonts/blob/main/ofl/yuseimagic/METADATA.pb) |
| Atkinson Hyperlegible | Actual static Regular 400 and Bold 700, plus italic forms available | Designer explicitly distinguishes B/8, O/0, and 1/I/i/l; original family supports 27 languages. — [Braille Institute](https://www.brailleinstitute.org/freefont/), [Exact original TTF metadata](https://github.com/google/fonts/blob/main/ofl/atkinsonhyperlegible/METADATA.pb) |

Downloaded files, relative to `Docs/Diagnostics/Phase8ARescueV2/FontCandidates/`:

| Comparison | Heading | Reader | Optional section label |
|---|---|---|---|
| A — recommended | `BricolageGrotesque/BricolageGrotesque96pt-ExtraBold.ttf` | `AtkinsonHyperlegible/AtkinsonHyperlegible-Regular.ttf` | `BricolageGrotesque/BricolageGrotesque12pt-Bold.ttf` |
| B — strong packaging | `DelaGothicOne/DelaGothicOne-Regular.ttf` | Same Atkinson Regular | `AtkinsonHyperlegible/AtkinsonHyperlegible-Bold.ttf` |
| C — softer town | `Fredoka/Fredoka-Bold.ttf` | Same Atkinson Regular | `AtkinsonHyperlegible/AtkinsonHyperlegible-Bold.ttf` |

The file identities and availability above are verified by [the download manifest](font-download-manifest.json).

### Inferences
- **A:** bold shop-sign personality with enough irregularity to support the game's comedic tone. Keep the 96pt cut for large display only; the 12pt cut is a meaningful label alternative. Its 800 heading should not be reused for receipts or body copy.
- **B:** strongest nostalgic packaging identity, but wide Latin forms can consume too much width in long titles. Heavy counters risk clogging at small pixel sizes. Use only short, large headings and real Regular styling; artificial bold would be inappropriate.
- **C:** readable rounded Latin silhouette at headline size, but a whole UI in it could feel too soft or childish for suspicion and working-town credibility. Restrict to headings; use restrained workplace colors and the same reader.
- **M PLUS:** useful reader alternative for future Japanese support, Medium/Bold for labels. Its rounded forms could work well, but full Japanese atlas generation is a separate localization decision. Latin small-size readability has not been rendered here.
- **Yusei:** good candidate for handwritten route notes, a chalk sign or optional handwritten annotation. Stroke contrast may lose thin horizontal details at small sizes; using it for every instruction would overstate the handwriting motif. Single available weight limits hierarchy.
- Atkinson's differentiation makes it the strongest common reader for part IDs, quantities, prices and receipts. A stated legibility aim is not a guarantee of accessibility for every player; compare at actual 720p and offer a scalable text setting.
- All six downloaded files have proportional default numerals. Atkinson, Bricolage and Dela binaries contain a `tnum` OpenType feature; Fredoka Bold does not. Do not assume TMP honors these substitutions just because they are in the source font. Reserve fixed cells for timer digits and right-align price/quantity columns until the runtime behavior is verified. — Measurement details: [font manifest](font-download-manifest.json).

### Gaps
- These recommendations are visual-design judgments, not player research. No real players have participated. No Unity screenshots have yet been evaluated as part of this research assignment.
- Latin and numerals in M PLUS/Yusei have not been visually rendered in the local game; they remain evaluated candidates rather than tested pairings.

## How should the chosen pair be implemented and validated in TMP?

### Takeaway
Use actual static weights rather than requesting variable axes or synthetic bold. Build a controlled English SDF atlas, test the identical short illustrated handbook layout with all three pairs, and confirm actual Unity screenshots before calling a font compatible or selected.

### Cited Findings
- TMP's Font Asset Creator converts a Unity source font to a TMP font asset, atlas texture and material. Its documentation describes importing a `.ttf`, selecting a character set, adjusting sampling/atlas settings, then generating and inspecting the atlas. — [Unity Font Asset Creator](https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/FontAssetsCreator.html).
- Unity advises that small character sets can fit smaller atlases and that larger character sets can require larger or multiple atlases; SDF resolution affects gradient detail. This does not establish settings for this particular project. — [Unity Font Asset Creator](https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/manual/FontAssetsCreator.html).
- Actual selected static weights: Bricolage96pt ExtraBold 800, Bricolage12pt Bold 700, Fredoka Bold 700, Dela Regular 400, Atkinson Regular 400/Bold 700. — [Binary metadata](font-download-manifest.json).

### Inferences
Suggested initial type scale at a 1280×720 reference resolution, scaled uniformly by CanvasScaler:

| Use | Face | Size | Line-height intent |
|---|---|---:|---|
| Main menu / large display | Bricolage96pt ExtraBold | 40 | 1.05× |
| Handbook title | Bricolage96pt ExtraBold | 32 | 1.10× |
| Section heading | Bricolage12pt Bold | 24 | 1.15× |
| Body / optional detail | Atkinson Regular | 18 | 1.35× |
| Interaction label | Atkinson Bold | 20 | 1.15× |
| Numeric inventory | Atkinson Bold | 20 | Fixed cells, stable baseline |
| Receipt value | Atkinson Regular | 18 | Right-aligned, 1.3× row height |
| Small status | Atkinson Regular | 14–16 | 1.25×; never essential instruction |

Use shared 8px spacing, minimum 24px panel inset, contrast-checked dark ink on warm paper, real button focus states and no fake bold, outlines or heavy shadows. Scale the UI, not the letters independently. A 720p body at 18px should remain 18px, rather than a 1080p reference making it shrink to 12px.

Identical comparison content:

> MECHANIC / SERVICE TICKET
> Battery replacement
> 01 OPEN HOOD
> 02 CHECK BATTERY
> 03 GRAB REPLACEMENT
> 04 REPLACE & TEST
> 05 CLOSE & SIGN
> E Open Hood
> Bay 1 / Van 08 · 2/5 steps
> Part B08-01 · $19.90 · 07:48
> O0 1Il B8 5689

Use the exact same viewport, colors, icon artwork, strings, title size, reader size, spacing, and button dimensions. Do not make one option look better by adding decoration or changing its copy. Capture 1280×720 and 1920×1080, plus narrow resizing and 1920×1200. Check title fit, glyph counters, numeral distinction, receipt columns, baseline alignment, clipping and focus-state contrast. Keep expected fallback glyphs intentional; don't let missing glyphs quietly change the whole reader face.

Recommended atlas experiment: SDF, ASCII plus actual UI punctuation/currency and accent coverage from the string inventory, 1024² as an initial trial, then adjust based on atlas and player screenshots. This is a proposed starting setting, not a measured requirement. Set per-weight TMP assets and real font-weight mappings. Preserve source TTFs to rebuild; retain license notices in a human-readable build folder. Confirm source-face identity, glyph coverage and actual rendered weights in the player.

### Gaps
- TMP import, SDF quality, kerning, fallback behavior and OpenType numeral substitution remain untested by this notes-only task. No claim of production readiness is justified until the coordinator's actual Unity import/render checks pass.
- Exact CanvasScaler strategy must be coordinated with the full redesign; the scale above is a design starting point and may change after actual viewport testing.
