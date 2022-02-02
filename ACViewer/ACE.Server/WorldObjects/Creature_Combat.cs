using ACE.Entity.Enum;

namespace ACE.Server.WorldObjects
{
    partial class Creature
    {
        /// <summary>
        /// The list of combat maneuvers performable by this creature
        /// </summary>
        public DatLoader.FileTypes.CombatManeuverTable CombatTable { get; set; }

        public CombatMode CombatMode { get; protected set; }
    }
}
