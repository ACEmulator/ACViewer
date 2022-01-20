using System.Collections.Generic;

namespace ACViewer.Render
{
    public static class R_EnvironmentCache
    {
        public static Dictionary<uint, R_Environment> Cache { get; set; }

        static R_EnvironmentCache()
        {
            Cache = new Dictionary<uint, R_Environment>();
        }

        public static R_Environment Get(uint envID)
        {
            Cache.TryGetValue(envID, out var env);

            if (env != null)
                return env;

            env = new R_Environment(envID);
            Cache.Add(envID, env);
            return env;
        }
    }
}
