﻿using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class Bindstone : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Bindstone(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Bindstone(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            ObjectDescriptionFlags |= ObjectDescriptionFlag.BindStone;

            SetProperty(PropertyInt.ShowableOnRadar, (int)ACE.Entity.Enum.RadarBehavior.ShowAlways);
            SetProperty(PropertyInt.RadarBlipColor, (int)ACE.Entity.Enum.RadarColor.LifeStone);
        }
    }
}
