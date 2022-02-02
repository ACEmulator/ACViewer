using ACE.Entity;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    /// <summary>
    /// Represents a chessboard in game
    /// </summary>
    public class Game : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Game(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Game(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            UseRadius = 6.5f;
        }
    }
}
