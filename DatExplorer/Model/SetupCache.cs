using System.Collections.Generic;

namespace DatExplorer.Model
{
    public static class SetupCache
    {
        public static Dictionary<uint, Setup> Cache;

        static SetupCache()
        {
            Cache = new Dictionary<uint, Setup>();
        }

        public static Setup Get(uint setupID)
        {
            if (!Cache.TryGetValue(setupID, out var setup))
            {
                setup = new Setup(setupID);
                Cache.Add(setupID, setup);
            }
            return setup;
        }
    }
}
