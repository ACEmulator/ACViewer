using System;
using System.Collections.Generic;

using ACE.Entity;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public class Allegiance : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Allegiance(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            //Console.WriteLine($"Allegiance({weenie.ClassId}, {guid}): weenie constructor");

            InitializePropertyDictionaries();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Allegiance(Biota biota) : base(biota)
        {
            //Console.WriteLine($"Allegiance({biota.Id:X8}): biota constructor");

            if (MonarchId == null)
            {
                Console.WriteLine($"Allegiance({biota.Id:X8}): constructor called with no monarch");
                return;
            }

            InitializePropertyDictionaries();
            //Init(new ObjectGuid(MonarchId.Value));
        }

        private void InitializePropertyDictionaries()
        {
            if (Biota.PropertiesAllegiance == null)
                Biota.PropertiesAllegiance = new Dictionary<uint, PropertiesAllegiance>();
        }

        public string AllegianceName
        {
            get => GetProperty(PropertyString.AllegianceName);
            set { if (value == null) RemoveProperty(PropertyString.AllegianceName); else SetProperty(PropertyString.AllegianceName, value); }
        }

        public string AllegianceMotd
        {
            get => GetProperty(PropertyString.AllegianceMotd);
            set { if (value == null) RemoveProperty(PropertyString.AllegianceMotd); else SetProperty(PropertyString.AllegianceMotd, value); }
        }

        public string AllegianceMotdSetBy
        {
            get => GetProperty(PropertyString.AllegianceMotdSetBy);
            set { if (value == null) RemoveProperty(PropertyString.AllegianceMotdSetBy); else SetProperty(PropertyString.AllegianceMotdSetBy, value); }
        }

        public string AllegianceSpeakerTitle
        {
            get => GetProperty(PropertyString.AllegianceSpeakerTitle);
            set { if (value == null) RemoveProperty(PropertyString.AllegianceSpeakerTitle); else SetProperty(PropertyString.AllegianceSpeakerTitle, value); }
        }

        public string AllegianceSeneschalTitle
        {
            get => GetProperty(PropertyString.AllegianceSeneschalTitle);
            set { if (value == null) RemoveProperty(PropertyString.AllegianceSeneschalTitle); else SetProperty(PropertyString.AllegianceSeneschalTitle, value); }
        }

        public string AllegianceCastellanTitle
        {
            get => GetProperty(PropertyString.AllegianceCastellanTitle);
            set { if (value == null) RemoveProperty(PropertyString.AllegianceCastellanTitle); else SetProperty(PropertyString.AllegianceCastellanTitle, value); }
        }
    }
}
