using System;
using System.Collections.Generic;

namespace ACViewer.Model
{
    public static class GfxObjCache
    {
        public static Dictionary<uint, GfxObj> Cache;

        static GfxObjCache()
        {
            Init();
        }

        public static void Init()
        {
            Cache = new Dictionary<uint, GfxObj>();
        }

        public static GfxObj Get(uint gfxObjID)
        {
            if (!Cache.TryGetValue(gfxObjID, out var gfxObj))
            {
                //Console.WriteLine($"- Loading {gfxObjID:X8}");
                gfxObj = new GfxObj(gfxObjID);
                Cache.Add(gfxObjID, gfxObj);
            }
            return gfxObj;
        }
    }
}
