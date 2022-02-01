using System;

using ACE.Entity;

namespace ACE.Server.Managers
{
    /// <summary>
    /// Used to assign global guids and ensure they are unique to server.
    /// </summary>
    public static class GuidManager
    {
        private static uint nextDynamicGuid = 0x80000000;
        
        /// <summary>
        /// These represent items are generated in the world.
        /// Some of them will be saved to the Shard db.
        /// They can be monsters, loot, etc..
        /// </summary>
        public static ObjectGuid NewDynamicGuid()
        {
            return new ObjectGuid(nextDynamicGuid++);
        }

        /// <summary>
        /// Guid will be added to the recycle queue, and available for use in GuidAllocator.recycleTime
        /// </summary>
        /// <param name="guid"></param>
        public static void RecycleDynamicGuid(ObjectGuid guid)
        {
            //dynamicAlloc.Recycle(guid.Full);
        }
    }
}
