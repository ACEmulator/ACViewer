using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

using ACE.Database.Models.World;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;

namespace ACViewer.Data
{
    /// <summary>
    /// A weenie that has been resolved to a renderable dat asset. Searchable text is split
    /// into tiers so a match on the actual name/class ranks above a match that's only an
    /// incidental mention inside another weenie's flavor text (e.g. a Drudge's LongDesc
    /// name-dropping Mosswarts shouldn't outrank an actual Mosswart).
    /// </summary>
    public class WeenieSearchEntry
    {
        public uint Wcid;
        public string ClassName;
        public string Name;
        public string WeenieType;
        public uint SetupId;

        // grouping key -- many WCIDs commonly share one Setup dat asset (e.g. same model at
        // different Scale/PaletteBase for a Baby/Chief/Elder tier), so results are grouped by this
        public string SetupIdHex => SetupId.ToString("X8");

        // lowercased search fields, ordered by how much they're trusted:
        // tier 0-1: the player-facing display name
        public string NameLower;
        // tier 2: structured category data (WeenieType, CreatureType, HeritageGroup, etc.) --
        // authoritative, unlike ClassName/DescText below
        public string CategoryText;
        // tier 3: the dev/internal class name -- can reference unrelated content it was reused/placed
        // in (e.g. a Drudge named "...mosswartexodus" for the dungeon it spawns in), so it's trusted
        // less than the actual Name or structured CreatureType/HeritageGroup data
        public string ClassNameLower;
        // tier 4: narrative/free text (Title, Use, ShortDesc, LongDesc, ...)
        public string DescText;

        public override string ToString() => $"{Wcid} - {Name} ({WeenieType})";
    }

    /// <summary>
    /// A one-time, in-memory index of ace_world weenie names/descriptions/categories,
    /// built the same way FileExplorer's other lookup tables (DIDTables, LootArmorList) are --
    /// loaded once, then searched in memory, so the UI never blocks on a live query per keystroke.
    /// </summary>
    public static class WeenieSearchIndex
    {
        private const string CacheFilename = "WeenieSearchCache.json";

        private class CacheFile
        {
            public DateTime BuiltAt { get; set; }
            public DateTime LastModifiedMarker { get; set; }
            public string DatabaseHost { get; set; }
            public string DatabaseName { get; set; }
            public List<WeenieSearchEntry> Entries { get; set; }
        }

        public static List<WeenieSearchEntry> Entries { get; private set; }

        // distinct WeenieType names actually present in the indexed data, for the type-filter
        // dropdown -- built from the real data rather than the full WeenieType enum, since not
        // every enum member shows up among renderable weenies
        public static List<string> AvailableWeenieTypes { get; private set; }

        public static bool IsLoaded => Entries != null;

        // human-readable summary of the last Load() call (cache hit or DB rebuild), for the status log
        public static string LastLoadSummary { get; private set; }

        /// <summary>
        /// forceRefresh bypasses the cache and always rebuilds from the database, saving a fresh cache
        /// </summary>
        public static void Load(bool forceRefresh = false)
        {
            var dbHost = ACViewer.Config.ConfigManager.Config.Database.Host;
            var dbName = ACViewer.Config.ConfigManager.Config.Database.DatabaseName;

            using (var context = new WorldDbContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                // the weenie table's last_Modified column auto-updates on any row change, so its
                // max value is a cheap fingerprint for "has anything changed since the cache was built"
                var marker = context.Weenie.Max(w => w.LastModified);

                if (!forceRefresh && TryLoadFromCache(marker, dbHost, dbName))
                    return;

                var weenies = context.Weenie
                    .Include(r => r.WeeniePropertiesString)
                    .Include(r => r.WeeniePropertiesInt)
                    .Include(r => r.WeeniePropertiesDID)
                    .ToList();

                var entries = new List<WeenieSearchEntry>();

                foreach (var weenie in weenies)
                {
                    var setupId = weenie.WeeniePropertiesDID.FirstOrDefault(d => d.Type == (ushort)PropertyDataId.Setup)?.Value ?? 0;

                    // nothing to render for this weenie in the dat viewer -- skip it
                    if (setupId == 0)
                        continue;

                    var name = weenie.WeeniePropertiesString.FirstOrDefault(s => s.Type == (ushort)PropertyString.Name)?.Value ?? "";
                    var weenieType = (WeenieType)weenie.Type;

                    var categoryTerms = new List<string> { System.Enum.GetName(typeof(WeenieType), weenieType) };
                    var descTerms = new List<string>();

                    foreach (var prop in weenie.WeeniePropertiesString)
                    {
                        if (prop.Type == (ushort)PropertyString.Name)
                            continue;

                        descTerms.Add(prop.Value);
                    }

                    foreach (var prop in weenie.WeeniePropertiesInt)
                    {
                        var enumName = ((PropertyInt)prop.Type).GetValueEnumName(prop.Value);
                        if (enumName != null)
                            categoryTerms.Add(enumName);
                    }

                    entries.Add(new WeenieSearchEntry
                    {
                        Wcid = weenie.ClassId,
                        ClassName = weenie.ClassName,
                        Name = name,
                        WeenieType = System.Enum.GetName(typeof(WeenieType), weenieType),
                        SetupId = setupId,
                        NameLower = name.ToLowerInvariant(),
                        CategoryText = string.Join(" ", categoryTerms.Where(t => !string.IsNullOrEmpty(t))).ToLowerInvariant(),
                        ClassNameLower = (weenie.ClassName ?? "").ToLowerInvariant(),
                        DescText = string.Join(" ", descTerms.Where(t => !string.IsNullOrEmpty(t))).ToLowerInvariant()
                    });
                }

                Entries = entries;
                AvailableWeenieTypes = entries.Select(e => e.WeenieType).Distinct()
                    .OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToList();

                var builtAt = DateTime.Now;
                SaveCache(marker, dbHost, dbName, builtAt, entries);

                LastLoadSummary = $"Weenie search index rebuilt from database {dbHost}/{dbName} at {builtAt:yyyy-MM-dd HH:mm:ss} " +
                    $"(db last modified {marker:yyyy-MM-dd HH:mm:ss}) -- {entries.Count:N0} weenies, cached for next run";
            }
        }

        /// <returns>true if the cache was present, valid for this marker/database, and successfully loaded</returns>
        private static bool TryLoadFromCache(DateTime marker, string dbHost, string dbName)
        {
            try
            {
                if (!File.Exists(CacheFilename))
                    return false;

                var cache = JsonConvert.DeserializeObject<CacheFile>(File.ReadAllText(CacheFilename));

                if (cache?.Entries == null)
                    return false;

                if (cache.LastModifiedMarker != marker || cache.DatabaseHost != dbHost || cache.DatabaseName != dbName)
                    return false;

                Entries = cache.Entries;
                AvailableWeenieTypes = Entries.Select(e => e.WeenieType).Distinct()
                    .OrderBy(t => t, StringComparer.OrdinalIgnoreCase).ToList();

                LastLoadSummary = $"Weenie search index loaded from cache -- last refreshed {cache.BuiltAt:yyyy-MM-dd HH:mm:ss} " +
                    $"from {cache.DatabaseHost}/{cache.DatabaseName} (db last modified {cache.LastModifiedMarker:yyyy-MM-dd HH:mm:ss}) -- {Entries.Count:N0} weenies";

                return true;
            }
            catch
            {
                // corrupt/unreadable cache file -- fall back to rebuilding from the database
                return false;
            }
        }

        private static void SaveCache(DateTime marker, string dbHost, string dbName, DateTime builtAt, List<WeenieSearchEntry> entries)
        {
            try
            {
                var cache = new CacheFile
                {
                    BuiltAt = builtAt,
                    LastModifiedMarker = marker,
                    DatabaseHost = dbHost,
                    DatabaseName = dbName,
                    Entries = entries
                };

                var json = JsonConvert.SerializeObject(cache, new JsonSerializerSettings { Formatting = Formatting.Indented });
                File.WriteAllText(CacheFilename, json);
            }
            catch
            {
                // caching is a nice-to-have -- if writing it fails (e.g. read-only directory), just skip it
            }
        }

        /// <summary>
        /// Lower is a better match: 0/1 = the display Name, 2 = structured category data
        /// (WeenieType, CreatureType, HeritageGroup, ...), 3 = only the internal dev class name,
        /// 4 = only found in narrative text. Category (2) outranks ClassName (3) because the dev
        /// class name can reference unrelated content (see ClassNameLower doc above) while
        /// CreatureType/HeritageGroup are authoritative about what the weenie actually is.
        /// Returns null if the term isn't found anywhere in the entry.
        /// </summary>
        private static int? MatchTier(WeenieSearchEntry e, string term)
        {
            if (e.Name.Equals(term, StringComparison.OrdinalIgnoreCase)) return 0;
            if (e.NameLower.Contains(term)) return 1;
            if (e.CategoryText.Contains(term)) return 2;
            if (e.ClassNameLower.Contains(term)) return 3;
            if (e.DescText.Contains(term)) return 4;
            return null;
        }

        /// <summary>
        /// weenieTypeFilters: null/empty means no type restriction, otherwise an entry's WeenieType
        /// must be one of the given values (multi-select). query: null/empty is allowed as long as
        /// at least one type filter is given -- that's a "browse by type" with no text ranking needed.
        /// </summary>
        public static List<WeenieSearchEntry> Search(string query, ICollection<string> weenieTypeFilters = null)
        {
            if (Entries == null)
                return new List<WeenieSearchEntry>();

            var hasQuery = !string.IsNullOrWhiteSpace(query);
            var hasTypeFilter = weenieTypeFilters != null && weenieTypeFilters.Count > 0;

            if (!hasQuery && !hasTypeFilter)
                return new List<WeenieSearchEntry>();

            IEnumerable<WeenieSearchEntry> source = Entries;
            if (hasTypeFilter)
            {
                var typeSet = new HashSet<string>(weenieTypeFilters);
                source = source.Where(e => typeSet.Contains(e.WeenieType));
            }

            if (!hasQuery)
            {
                // pure type browse -- no text to rank against, just list alphabetically
                return source.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase).ThenBy(e => e.Wcid).ToList();
            }

            var term = query.Trim().ToLowerInvariant();

            return source
                .Select(e => (Entry: e, Tier: MatchTier(e, term)))
                .Where(m => m.Tier != null)
                .OrderBy(m => m.Tier)
                .ThenBy(m => m.Entry.Name.Length)
                .ThenBy(m => m.Entry.Wcid)
                .Select(m => m.Entry)
                .ToList();
        }
    }
}
