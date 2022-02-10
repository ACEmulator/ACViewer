using System;
using System.Collections.Generic;

using ACE.Database;
using ACE.Database.Models.Auth;
using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Entity;

using Character = ACE.Database.Models.Shard.Character;

namespace ACE.Server.WorldObjects
{
    public partial class Player : Creature, IPlayer
    {
        public Account Account { get; }

        public Character Character { get; }

        //public Session Session { get; }

        public Player() : base()
        {

        }
        
        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public Player(Weenie weenie, ObjectGuid guid, uint accountId) : base(weenie, guid)
        {
            Character = new Character();
            Character.Id = guid.Full;
            Character.AccountId = accountId;
            Character.Name = GetProperty(PropertyString.Name);
            CharacterChangesDetected = true;

            Account = DatabaseManager.Authentication.GetAccountById(Character.AccountId);

            SetEphemeralValues();

            // Make sure properties this WorldObject requires are not null.
            AvailableExperience = AvailableExperience ?? 0;
            TotalExperience = TotalExperience ?? 0;

            Attackable = true;

            SetProperty(PropertyString.DateOfBirth, $"{DateTime.UtcNow:dd MMMM yyyy}");

            if (IsOlthoiPlayer)
            {
                GenerateContainList();
            }
            else
                Biota.PropertiesCreateList?.Clear();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public Player(Biota biota, IEnumerable<ACE.Database.Models.Shard.Biota> inventory, IEnumerable<ACE.Database.Models.Shard.Biota> wieldedItems, Character character/*, Session session*/) : base(biota)
        {
            Character = character;
            //Session = session;

            Account = DatabaseManager.Authentication.GetAccountById(Character.AccountId);

            SetEphemeralValues();

            SortBiotasIntoInventory(inventory);
            AddBiotasToEquippedObjects(wieldedItems);

            UpdateCoinValue(false);
        }

        public override void InitPhysicsObj()
        {
            base.InitPhysicsObj();

            // set pink bubble state
            IgnoreCollisions = true; ReportCollisions = false; Hidden = true;
        }

        private void SetEphemeralValues()
        {
            ObjectDescriptionFlags |= ObjectDescriptionFlag.Player;

            // This is the default send upon log in and the most common. Anything with a velocity will need to add that flag.
            // This should be handled automatically...
            //PositionFlags |= PositionFlags.OrientationHasNoX | PositionFlags.OrientationHasNoY | PositionFlags.IsGrounded | PositionFlags.HasPlacementID;

            FirstEnterWorldDone = false;

            SetStance(MotionStance.NonCombat, false);

            /*if (Session != null && ConfigManager.Config.Server.Accounts.OverrideCharacterPermissions)
            {
                if (Session.AccessLevel == AccessLevel.Admin)
                    IsAdmin = true;
                if (Session.AccessLevel == AccessLevel.Developer)
                    IsArch = true;
                if (Session.AccessLevel == AccessLevel.Sentinel)
                    IsSentinel = true;
                if (Session.AccessLevel == AccessLevel.Envoy)
                {
                    IsEnvoy = true;
                    IsSentinel = true; //IsEnvoy is not recognized by the client and therefore the client should treat the user as a Sentinel.
                }
                if (Session.AccessLevel == AccessLevel.Advocate)
                    IsAdvocate = true;
            }*/

            IsOlthoiPlayer = HeritageGroup == HeritageGroup.Olthoi || HeritageGroup == HeritageGroup.OlthoiAcid;

            ContainerCapacity = (byte)(7 + AugmentationExtraPackSlot);

            /*if (Session != null && AdvocateQuest && IsAdvocate) // Advocate permissions are per character regardless of override
            {
                if (Session.AccessLevel == AccessLevel.Player)
                    Session.SetAccessLevel(AccessLevel.Advocate); // Elevate to Advocate permissions
                if (AdvocateLevel > 4)
                    IsPsr = true; // Enable AdvocateTeleport via MapClick
            }*/

            CombatTable = DatManager.PortalDat.ReadFromDat<CombatManeuverTable>(CombatTableDID.Value);

            //_questManager = new QuestManager(this);

            //ContractManager = new ContractManager(this);

            //ConfirmationManager = new ConfirmationManager(this);

            //LootPermission = new Dictionary<ObjectGuid, DateTime>();

            //SquelchManager = new SquelchManager(this);

            //MagicState = new MagicState(this);

            //FoodState = new FoodState(this);

            //RecordCast = new RecordCast(this);

            //AttackQueue = new AttackQueue(this);

            if (!PlayerKillsPk.HasValue)
                PlayerKillsPk = 0;
            if (!PlayerKillsPkl.HasValue)
                PlayerKillsPkl = 0;

            return; // todo

            // =======================================
            // This code was taken from the old Load()
            // =======================================
            /*AceCharacter character;

            if (Common.ConfigManager.Config.Server.Accounts.OverrideCharacterPermissions)
            {
                if (Session.AccessLevel == AccessLevel.Admin)
                    character.IsAdmin = true;
                if (Session.AccessLevel == AccessLevel.Developer)
                    character.IsArch = true;
                if (Session.AccessLevel == AccessLevel.Envoy)
                    character.IsEnvoy = true;
                // TODO: Need to setup and account properly for IsSentinel and IsAdvocate.
                // if (Session.AccessLevel == AccessLevel.Sentinel)
                //    character.IsSentinel = true;
                // if (Session.AccessLevel == AccessLevel.Advocate)
                //    character.IsAdvocate= true;
            }*/

            // FirstEnterWorldDone = false;

            // IsAlive = true;
        }

        public bool IsDeleted => Character.IsDeleted;
        public bool IsPendingDeletion => Character.DeleteTime > 0 && !IsDeleted;


        // ******************************************************************* OLD CODE BELOW ********************************
        // ******************************************************************* OLD CODE BELOW ********************************
        // ******************************************************************* OLD CODE BELOW ********************************
        // ******************************************************************* OLD CODE BELOW ********************************
        // ******************************************************************* OLD CODE BELOW ********************************
        // ******************************************************************* OLD CODE BELOW ********************************
        // ******************************************************************* OLD CODE BELOW ********************************

        public MotionStance stance = MotionStance.NonCombat;

        public override void OnCollideEnvironment()
        {
            //HandleFallingDamage();
        }

        public override void OnCollideObject(WorldObject target)
        {
            //Console.WriteLine($"{Name}.OnCollideObject({target.Name})");

            if (target.ReportCollisions == false)
                return;

            if (target is Portal portal)
                portal.OnCollideObject(this);
            else if (target is PressurePlate pressurePlate)
                pressurePlate.OnCollideObject(this);
            else if (target is Hotspot hotspot)
                hotspot.OnCollideObject(this);
            else if (target is SpellProjectile spellProjectile)
                spellProjectile.OnCollideObject(this);
            //else if (target.ProjectileTarget != null)
                //ProjectileCollisionHelper.OnCollideObject(target, this);
        }

        public override void OnCollideObjectEnd(WorldObject target)
        {
            if (target is Hotspot hotspot)
                hotspot.OnCollideObjectEnd(this);
        }
    }
}
