using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    /// <summary>
    /// ** Buy Data Flow **
    ///
    /// Player.HandleActionBuyItem -> Vendor.BuyItems_ValidateTransaction -> Player.FinalizeBuyTransaction -> Vendor.BuyItems_FinalTransaction
    ///     
    /// </summary>
    public class Vendor : Creature
    {
        private bool inventoryloaded { get; set; }

        public uint? AlternateCurrency
        {
            get => GetProperty(PropertyDataId.AlternateCurrency);
            set { if (!value.HasValue) RemoveProperty(PropertyDataId.AlternateCurrency); else SetProperty(PropertyDataId.AlternateCurrency, value.Value); }
        }

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Vendor(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Vendor(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            ObjectDescriptionFlags |= ObjectDescriptionFlag.Vendor;

            /*if (!PropertyManager.GetBool("vendor_shop_uses_generator").Item)
            {
                GeneratorProfiles.RemoveAll(p => p.Biota.WhereCreate.HasFlag(RegenLocationType.Shop));
            }*/

            //OpenForBusiness = ValidateVendorRequirements();
        }

        public bool OpenForBusiness
        {
            get => GetProperty(PropertyBool.OpenForBusiness) ?? true;
            set { if (value) RemoveProperty(PropertyBool.OpenForBusiness); else SetProperty(PropertyBool.OpenForBusiness, value); }
        }

        public int? MerchandiseItemTypes
        {
            get => GetProperty(PropertyInt.MerchandiseItemTypes);
            set { if (!value.HasValue) RemoveProperty(PropertyInt.MerchandiseItemTypes); else SetProperty(PropertyInt.MerchandiseItemTypes, value.Value); }
        }

        public int? MerchandiseMinValue
        {
            get => GetProperty(PropertyInt.MerchandiseMinValue);
            set { if (!value.HasValue) RemoveProperty(PropertyInt.MerchandiseMinValue); else SetProperty(PropertyInt.MerchandiseMinValue, value.Value); }
        }

        public int? MerchandiseMaxValue
        {
            get => GetProperty(PropertyInt.MerchandiseMaxValue);
            set { if (!value.HasValue) RemoveProperty(PropertyInt.MerchandiseMaxValue); else SetProperty(PropertyInt.MerchandiseMaxValue, value.Value); }
        }

        public double? BuyPrice
        {
            get => GetProperty(PropertyFloat.BuyPrice);
            set { if (!value.HasValue) RemoveProperty(PropertyFloat.BuyPrice); else SetProperty(PropertyFloat.BuyPrice, value.Value); }
        }

        public double? SellPrice
        {
            get => GetProperty(PropertyFloat.SellPrice);
            set { if (!value.HasValue) RemoveProperty(PropertyFloat.SellPrice); else SetProperty(PropertyFloat.SellPrice, value.Value); }
        }

        public bool? DealMagicalItems
        {
            get => GetProperty(PropertyBool.DealMagicalItems);
            set { if (!value.HasValue) RemoveProperty(PropertyBool.DealMagicalItems); else SetProperty(PropertyBool.DealMagicalItems, value.Value); }
        }

        public bool? VendorService
        {
            get => GetProperty(PropertyBool.VendorService);
            set { if (!value.HasValue) RemoveProperty(PropertyBool.VendorService); else SetProperty(PropertyBool.VendorService, value.Value); }
        }

        public int? VendorHappyMean
        {
            get => GetProperty(PropertyInt.VendorHappyMean);
            set { if (!value.HasValue) RemoveProperty(PropertyInt.VendorHappyMean); else SetProperty(PropertyInt.VendorHappyMean, value.Value); }
        }

        public int? VendorHappyVariance
        {
            get => GetProperty(PropertyInt.VendorHappyVariance);
            set { if (!value.HasValue) RemoveProperty(PropertyInt.VendorHappyVariance); else SetProperty(PropertyInt.VendorHappyVariance, value.Value); }
        }

        public int? VendorHappyMaxItems
        {
            get => GetProperty(PropertyInt.VendorHappyMaxItems);
            set { if (!value.HasValue) RemoveProperty(PropertyInt.VendorHappyMaxItems); else SetProperty(PropertyInt.VendorHappyMaxItems, value.Value); }
        }

        public int NumItemsSold
        {
            get => GetProperty(PropertyInt.NumItemsSold) ?? 0;
            set { if (value == 0) RemoveProperty(PropertyInt.NumItemsSold); else SetProperty(PropertyInt.NumItemsSold, value); }
        }

        public int NumItemsBought
        {
            get => GetProperty(PropertyInt.NumItemsBought) ?? 0;
            set { if (value == 0) RemoveProperty(PropertyInt.NumItemsBought); else SetProperty(PropertyInt.NumItemsBought, value); }
        }

        public int NumServicesSold
        {
            get => GetProperty(PropertyInt.NumServicesSold) ?? 0;
            set { if (value == 0) RemoveProperty(PropertyInt.NumServicesSold); else SetProperty(PropertyInt.NumServicesSold, value); }
        }

        public int MoneyIncome
        {
            get => GetProperty(PropertyInt.MoneyIncome) ?? 0;
            set { if (value == 0) RemoveProperty(PropertyInt.MoneyIncome); else SetProperty(PropertyInt.MoneyIncome, value); }
        }

        public int MoneyOutflow
        {
            get => GetProperty(PropertyInt.MoneyOutflow) ?? 0;
            set { if (value == 0) RemoveProperty(PropertyInt.MoneyOutflow); else SetProperty(PropertyInt.MoneyOutflow, value); }
        }
    }
}