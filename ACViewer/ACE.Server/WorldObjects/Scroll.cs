using ACE.Entity;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class Scroll : WorldObject
    {
        public Server.Entity.Spell Spell;

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Scroll(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Scroll(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            if (SpellDID != null)
                Spell = new Server.Entity.Spell(SpellDID.Value, false);

            if (Spell != null)
                LongDesc = $"Inscribed spell: {Spell.Name}\n{Spell.Description}";

            Use = "Use this item to attempt to learn its spell.";
        }
    }
}
