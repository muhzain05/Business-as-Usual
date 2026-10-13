using System.Collections.Generic;
using UnityEngine;
using BusinessAsUsual.Shopkeeper;
using BusinessAsUsual.Town;
using BusinessAsUsual.Multiplayer;
using BusinessAsUsual.Prototype;

namespace BusinessAsUsual.FirstPerson
{
    // Live paper minimap. The town/interior images are baked once in the editor
    // (Business as Usual > Phase 7A > Bake Minimap); runtime only draws them with UV windows,
    // so there is no per-frame texture reconstruction.
    public sealed class TownMinimap
    {
        public sealed class Region
        {
            public string title; public Texture2D texture; public Rect world; // world.x = min X, world.y = min Z
            public bool Contains(Vector3 p) => p.x >= world.xMin && p.x <= world.xMax && p.z >= world.yMin && p.z <= world.yMax;
        }
        // Colleagues appear only when this player could actually see them: close and unobstructed.
        public const float ColleagueRange = 14f;
        public static readonly Rect TownWorld = new Rect(-16, -14, 32, 28);
        public static readonly Rect MarketWorld = new Rect(-46.5f, -1.5f, 13, 15);
        public readonly Region Town, Market;
        public int VisibleColleagues { get; private set; }
        public int DrawnObjectives { get; private set; }
        public int ClampedObjectives { get; private set; }
        public Vector2 LastPlayerPoint { get; private set; }
        readonly ShopUITheme theme;
        readonly TownGraphics graphics;
        readonly List<Vector3> colleagues = new List<Vector3>();
        readonly RaycastHit[] hits = new RaycastHit[4];

        public sealed class MapObjective
        {
            public TownObjective objective;
            public Vector3 world;
            public bool entrance;
        }

        // One source for both map sizes. Locked previews are not objectives. Interior work
        // projects to the actual entry trigger, once, rather than to an arbitrary blue pin.
        public static List<MapObjective> ActiveMarkers(Region region, TownGuidance guide)
        {
            var result = new List<MapObjective>();
            if (guide == null) return result;
            var seen = new HashSet<string>();
            foreach (var o in guide.objectives)
            {
                if (!o.active || !o.target) continue;
                Vector3 p = o.Position;
                if (float.IsNaN(p.x) || float.IsNaN(p.z) || float.IsInfinity(p.x) || float.IsInfinity(p.z)) continue;
                bool entrance = false;
                string key = o.id;
                if (!region.Contains(p))
                {
                    if (region.world != TownWorld || !MarketWorld.Contains(new Vector2(p.x, p.z))) continue;
                    var shop = NetworkShop.Instance;
                    Interactable entry = null;
                    if (shop) foreach (var target in shop.Targets.Values)
                        if (target is TownStation s && s.kind == TownStation.Kind.Portal && MarketWorld.Contains(new Vector2(s.destination.x, s.destination.z))) { entry = s; break; }
                    if (!entry) continue;
                    p = entry.InteractionPoint; key = NetworkShop.Id(entry); entrance = true;
                }
                if (seen.Add(key)) result.Add(new MapObjective { objective = o, world = p, entrance = entrance });
            }
            return result;
        }

        public static Vector2 ClampMarker(Vector2 p, Vector2 size, float iconSize) =>
            new Vector2(Mathf.Clamp(p.x, iconSize * .62f, size.x - iconSize * .62f), Mathf.Clamp(p.y, iconSize * .62f, size.y - iconSize * .62f));

        public TownMinimap(ShopUITheme theme, TownGraphics graphics)
        {
            this.theme = theme; this.graphics = graphics;
            Town = new Region { title = "DAYLIGHT / TOWN MAP", texture = Resources.Load<Texture2D>("Town/Map/DaylightTownMap"), world = TownWorld };
            Market = new Region { title = "DAYLIGHT MARKET / FLOOR", texture = Resources.Load<Texture2D>("Town/Map/DaylightMarketMap"), world = MarketWorld };
        }
        public bool Baked => Town.texture;
        public Region RegionFor(Vector3 p) => Market.Contains(p) ? Market : Town;

        // World (x,z) -> normalized map (0..1, y down = north up).
        public static Vector2 Normalized(Region region, Vector3 world) =>
            new Vector2((world.x - region.world.xMin) / region.world.width, (region.world.yMax - world.z) / region.world.height);
        public static Vector3 World(Region region, Vector2 normalized) =>
            new Vector3(region.world.xMin + normalized.x * region.world.width, 0, region.world.yMax - normalized.y * region.world.height);

        public void Refresh(NetworkPlayer self, Vector3 eye)
        {
            colleagues.Clear();
            foreach (var other in Object.FindObjectsByType<NetworkPlayer>(FindObjectsSortMode.None))
            {
                if (!other || other == self || !other.IsSpawned) continue;
                Vector3 p = other.transform.position, head = p + Vector3.up * .9f;
                if ((head - eye).sqrMagnitude > ColleagueRange * ColleagueRange || RegionFor(p) != RegionFor(self.transform.position)) continue;
                int count = Physics.RaycastNonAlloc(eye, (head - eye).normalized, hits, Vector3.Distance(eye, head), self.Interactor.interactionMask, QueryTriggerInteraction.Ignore);
                bool blocked = false;
                for (int i = 0; i < count; i++) if (!hits[i].collider.transform.IsChildOf(other.transform) && !hits[i].collider.transform.IsChildOf(self.transform)) { blocked = true; break; }
                if (!blocked) colleagues.Add(p);
            }
            VisibleColleagues = colleagues.Count;
        }

        // Compact player-centred view (north up, rotating player arrow).
        public void DrawCompact(Rect card, TownGuidance guide, Vector3 player, float facing, TownJobs jobs, float span = 15f)
        {
            var region = RegionFor(player);
            TownGraphics.Card(card);
            theme.Text(card.x + 15, card.y + 11, region.title, 18, 1, card.width - 60);
            theme.Text(card.xMax - 30, card.y + 11, "N", 18, 2, 20);
            var map = new Rect(card.x + 14, card.y + 42, card.width - 28, card.height - 83);
            float viewW = span, viewH = span * map.height / map.width;
            Vector2 c = Normalized(region, player);
            var uv = new Rect(c.x - viewW / region.world.width / 2, 1 - c.y - viewH / region.world.height / 2, viewW / region.world.width, viewH / region.world.height);
            ShopUITheme.Fill(map, ShopUITheme.Pale);
            GUI.BeginGroup(map);
            if (region.texture) GUI.DrawTextureWithTexCoords(new Rect(0, 0, map.width, map.height), region.texture, uv);
            Vector2 ToMap(Vector3 w) => new Vector2(((w.x - player.x) / viewW + .5f) * map.width, ((player.z - w.z) / viewH + .5f) * map.height);
            Landmarks(region, ToMap, map.size, false);
            Markers(region, guide, ToMap, map.size, false);
            foreach (var p in colleagues) Colleague(ToMap(p), map.size, false);
            var self = new Vector2(map.width / 2, map.height / 2); LastPlayerPoint = self;
            Arrow(self, facing, 30);
            GUI.EndGroup();
            Footer(card, jobs);
        }

        // Whole-region expanded map (M) or handbook page.
        public void DrawFull(Rect card, TownGuidance guide, Vector3 player, float facing, TownJobs jobs, bool legend)
        {
            var region = RegionFor(player);
            TownGraphics.Card(card);
            theme.Text(card.x + 20, card.y + 14, region.title, 24, 1, card.width - 80);
            theme.Text(card.xMax - 36, card.y + 14, "N", 22, 2, 24);
            var area = new Rect(card.x + 18, card.y + 52, card.width - 36, card.height - (legend ? 130 : 96));
            float aspect = region.world.width / region.world.height;
            var map = area.width / area.height > aspect ? new Rect(area.center.x - area.height * aspect / 2, area.y, area.height * aspect, area.height) : new Rect(area.x, area.center.y - area.width / aspect / 2, area.width, area.width / aspect);
            ShopUITheme.Fill(map, ShopUITheme.Pale);
            GUI.BeginGroup(map);
            if (region.texture) GUI.DrawTexture(new Rect(0, 0, map.width, map.height), region.texture);
            Vector2 ToMap(Vector3 w) { var n = Normalized(region, w); return new Vector2(n.x * map.width, n.y * map.height); }
            Landmarks(region, ToMap, map.size, true);
            Markers(region, guide, ToMap, map.size, true);
            foreach (var p in colleagues) Colleague(ToMap(p), map.size, true);
            var self = ToMap(player); LastPlayerPoint = new Vector2(self.x / map.width, self.y / map.height);
            Arrow(self, facing, 40);
            GUI.EndGroup();
            if (legend)
            {
                float y = card.yMax - 66;
                graphics.Icon("you", new Rect(card.x + 24, y, 30, 30)); theme.Text(card.x + 62, y + 2, "You", 22, 1, 120);
                ShopUITheme.Fill(new Rect(card.x + 190, y + 7, 18, 18), ShopUITheme.Coral); theme.Text(card.x + 218, y + 2, "Staff in view", 22, 1, 220);
                graphics.Icon(guide.Primary?.icon ?? "slip", new Rect(card.x + 450, y, 30, 30)); theme.Text(card.x + 488, y + 2, guide.Primary != null ? guide.Primary.title : "No active work", 22, 1, card.width - 520);
                theme.Text(card.xMax - 20, y + 2, "M  Close", 22, 1, 160, 2);
            }
            else Footer(card, jobs);
        }

        void Footer(Rect card, TownJobs jobs)
        {
            float progress = jobs ? TownGuidance.Productivity(jobs) : 0;
            theme.Text(card.x + 15, card.yMax - 30, "TODAY", 16, 1, 80);
            theme.Line(card.x + 90, card.yMax - 18, card.width - 152, false, ShopUITheme.Rule, 5);
            theme.Line(card.x + 90, card.yMax - 18, (card.width - 152) * progress, false, ShopUITheme.Green, 5);
            theme.Text(card.xMax - 12, card.yMax - 31, Mathf.RoundToInt(progress * 100) + "%", 18, 1, 62, 2);
        }

        void Landmarks(Region region, System.Func<Vector3, Vector2> toMap, Vector2 size, bool large)
        {
            if (region != Town) return;
            Place(toMap(new Vector3(-8, 0, 6.5f)), "MARKET", "shop", size, large);
            Place(toMap(new Vector3(8, 0, 6.5f)), "GARAGE", "wrench", size, large);
            Place(toMap(new Vector3(8, 0, -7.7f)), "YARD", "recycle", size, large);
            Place(toMap(new Vector3(0, 0, 6.5f)), "SQUARE", "clock", size, large);
            Place(toMap(new Vector3(0, 0, 11.2f)), "ALLEY", "bag", size, large);
        }
        void Place(Vector2 p, string name, string icon, Vector2 size, bool large)
        {
            float w = large ? 104 : 64, h = large ? 26 : 19;
            if (p.x < -w || p.y < -h || p.x > size.x + w || p.y > size.y + h) return;
            ShopUITheme.Fill(new Rect(p.x - w / 2, p.y - h / 2, w, h), new Color(ShopUITheme.Paper.r, ShopUITheme.Paper.g, ShopUITheme.Paper.b, .82f));
            theme.Text(p.x, p.y - h / 2 + 2, name, large ? 17 : 12, 1, w - 4, 1);
        }

        void Markers(Region region, TownGuidance guide, System.Func<Vector3, Vector2> toMap, Vector2 size, bool large)
        {
            DrawnObjectives = 0; ClampedObjectives = 0;
            if (guide == null) return;
            foreach (var marker in ActiveMarkers(region, guide))
            {
                var objective = marker.objective;
                var world = marker.world;
                var p = toMap(world);
                float iconSize = large ? 30 : 22, inset = iconSize * .6f;
                bool clamped = p.x < inset || p.y < inset || p.x > size.x - inset || p.y > size.y - inset;
                if (clamped) { p = ClampMarker(p, size, iconSize); ClampedObjectives++; }
                DrawnObjectives++;
                float alpha = objective.active ? 1 : .45f;
                if (objective.active)
                {
                    float pulse = .5f + .5f * Mathf.Sin(Time.unscaledTime * 4);
                    ShopUITheme.Fill(new Rect(p.x - iconSize * .62f, p.y - iconSize * .62f, iconSize * 1.24f, iconSize * 1.24f), new Color(ShopUITheme.Mustard.r, ShopUITheme.Mustard.g, ShopUITheme.Mustard.b, clamped ? .9f : .35f + .35f * pulse));
                }
                graphics.Icon(marker.entrance ? "door" : objective.icon, new Rect(p.x - iconSize / 2, p.y - iconSize / 2, iconSize, iconSize), clamped ? Mathf.Min(alpha, .85f) : alpha);
            }
        }

        static void Colleague(Vector2 p, Vector2 size, bool large)
        {
            if (p.x < 0 || p.y < 0 || p.x > size.x || p.y > size.y) return;
            float s = large ? 16 : 12;
            ShopUITheme.Fill(new Rect(p.x - s / 2 - 2, p.y - s / 2 - 2, s + 4, s + 4), ShopUITheme.Ink);
            ShopUITheme.Fill(new Rect(p.x - s / 2, p.y - s / 2, s, s), ShopUITheme.Coral);
        }

        void Arrow(Vector2 at, float facing, float size)
        {
            // The existing "you" stamp turns with the camera, so it shows where the player looks.
            // A mustard disc keeps the stamp readable over any map colour.
            ShopUITheme.Fill(new Rect(at.x - size * .42f, at.y - size * .42f, size * .84f, size * .84f), new Color(ShopUITheme.Mustard.r, ShopUITheme.Mustard.g, ShopUITheme.Mustard.b, .85f));
            var old = GUI.matrix;
            GUIUtility.RotateAroundPivot(facing, at);
            graphics.Icon("you", new Rect(at.x - size / 2, at.y - size / 2, size, size));
            GUI.matrix = old;
        }
    }
}
