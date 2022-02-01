using System;
using System.Collections.Generic;

using ACE.Database.Models.World;

namespace ACE.Server.WorldObjects
{
    partial class WorldObject
    {
        public List<LandblockInstance> LinkedInstances { get; set; } = new List<LandblockInstance>();

        public WorldObject ParentLink { get; set; }
        public List<WorldObject> ChildLinks { get; set; } = new List<WorldObject>();

        public virtual void SetLinkProperties(WorldObject wo)
        {
            // empty base
            Console.WriteLine($"{Name}.SetLinkProperties({wo.Name}) called for unknown parent type: {WeenieType}");
        }

        public virtual void UpdateLinkProperties(WorldObject wo)
        {
            // empty base
            Console.WriteLine($"{Name}.UpdateLinkProperties({wo.Name}) called for unknown parent type: {WeenieType}");
        }
    }
}
