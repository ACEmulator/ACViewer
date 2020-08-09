using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACE.Entity;

namespace ACE.Server.WorldObjects
{
    public class WorldObject
    {
        public ObjectGuid Guid;
        public string Name;
        public bool IsCreature;
        public uint RunSkill;
    }
}
