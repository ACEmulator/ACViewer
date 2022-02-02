using System;
using System.Numerics;

using ACE.Entity;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;

namespace ACE.Server.WorldObjects
{
    public sealed class HousePortal : Portal
    {
        public House House => ParentLink as House;

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public HousePortal(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public HousePortal(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        public override void SetLinkProperties(WorldObject wo)
        {
            // get properties from parent?
            wo.HouseId = House.HouseId;
            wo.HouseOwner = House.HouseOwner;
            wo.HouseInstance = House.HouseInstance;

            if (wo.IsLinkSpot)
            {
                var housePortals = House.GetHousePortals();
                if (housePortals.Count == 0)
                {
                    Console.WriteLine($"{Name}.SetLinkProperties({wo.Name}): found LinkSpot, but empty HousePortals");
                    return;
                }
                var i = housePortals[0];

                if (i.ObjCellId == Location.Cell && housePortals.Count > 1)
                    i = housePortals[1];

                var destination = new Position(i.ObjCellId, new Vector3(i.OriginX, i.OriginY, i.OriginZ), new Quaternion(i.AnglesX, i.AnglesY, i.AnglesZ, i.AnglesW));

                wo.SetPosition(PositionType.Destination, destination);

                // set portal destination directly?
                SetPosition(PositionType.Destination, destination);
            }
        }
    }
}
