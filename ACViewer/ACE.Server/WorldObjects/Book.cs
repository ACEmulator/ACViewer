using System.Collections.Generic;

using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public sealed class Book : WorldObject
    {
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Book(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            InitializePropertyDictionaries();
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Book(Biota biota) : base(biota)
        {
            InitializePropertyDictionaries();
            SetEphemeralValues();
        }

        private void InitializePropertyDictionaries()
        {
            if (Biota.PropertiesBook == null)
                Biota.PropertiesBook = new PropertiesBook();
            if (Biota.PropertiesBookPageData == null)
                Biota.PropertiesBookPageData = new List<PropertiesBookPageData>();
        }

        private void SetEphemeralValues()
        {
            ObjectDescriptionFlags |= ObjectDescriptionFlag.Book;

            // Ensure a book can always be "read"
            ActivationResponse |= ActivationResponse.Use;

            SetProperty(PropertyInt.AppraisalPages, Biota.PropertiesBookPageData.Count);

            SetProperty(PropertyInt.AppraisalMaxPages, Biota.PropertiesBook.MaxNumPages);
        }

        public void SetProperties(string name, string shortDesc, string inscription, string scribeName, string scribeAccount)
        {
            if (!string.IsNullOrEmpty(name)) SetProperty(PropertyString.Name, name);
            if (!string.IsNullOrEmpty(shortDesc)) SetProperty(PropertyString.ShortDesc, shortDesc);
            if (!string.IsNullOrEmpty(inscription)) SetProperty(PropertyString.Inscription, inscription);
            if (!string.IsNullOrEmpty(scribeName)) SetProperty(PropertyString.ScribeName, scribeName);
            if (!string.IsNullOrEmpty(scribeAccount)) SetProperty(PropertyString.ScribeAccount, scribeAccount);
        }
    }
}