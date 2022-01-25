using System;

using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.Physics.Animation;
using ACE.Server.Physics.Collision;
using ACE.Server.WorldObjects;

namespace ACE.Server.Physics.Common
{
    public class WeenieObject
    {
        public uint ID;
        public double UpdateTime;
        public WorldObject WorldObject;

        public bool IsMonster { get; set; }

        public bool IsCombatPet { get; set; }

        public bool IsFactionMob { get; set; }

        public FactionBits Faction1Bits { get; set; }

        public CreatureType? FoeType { get; set; }

        public PlayerKillerStatus PlayerKillerStatus { get; set; }

        public WeenieObject() { }

        public WeenieObject(WorldObject worldObject)
        {
            WorldObject = worldObject;

            /*if (!(worldObject is Creature creature))
                return;

            IsCombatPet = worldObject is CombatPet;

            IsMonster = creature.IsMonster && !IsCombatPet;

            Faction1Bits = creature.Faction1Bits ?? FactionBits.None;

            IsFactionMob = IsMonster && Faction1Bits != FactionBits.None;

            FoeType = creature.FoeType;

            PlayerKillerStatus = creature.PlayerKillerStatus;*/
        }

        public bool SameFaction(PhysicsObj obj)
        {
            return (Faction1Bits & obj.WeenieObj.Faction1Bits) != 0;
        }

        public bool PotentialFoe(PhysicsObj obj)
        {
            /*return FoeType != null && FoeType == obj.WeenieObj.WorldObject?.CreatureType ||
                obj.WeenieObj.FoeType != null && obj.WeenieObj.FoeType == WorldObject?.CreatureType;*/
            return false;
        }

        public bool CanJump(float extent)
        {
            return true;
        }

        public bool InqJumpVelocity(float extent, out float velocity_z)
        {
            velocity_z = 0.0f;

            /*var player = WorldObject as Player;

            if (player == null)
                return false;*/

            /*if (!WorldObject.IsPlayer) return false;

            var burden = InqBurden();
            if (burden == null)
                return false;

            var stamina = player.Stamina.Current;

            var jumpSkill = player.GetCreatureSkill(Skill.Jump).Current;

            if (stamina == 0)
                jumpSkill = 0;

            var height = MovementSystem.GetJumpHeight((float)burden, jumpSkill, extent, 1.0f);

            velocity_z = (float)Math.Sqrt(height * 19.6);

            return true;*/
            return false;
        }

        /// <summary>
        /// Returns the player's load / burden as a percentage,
        /// usually in the range 0.0 - 3.0 (max 300% typically)
        /// </summary>
        public float? InqBurden()
        {
            /*var player = WorldObject as Player;

            if (player == null)
                return null;

            var strength = (int)player.Strength.Current;

            var numAugs = player.AugmentationIncreasedCarryingCapacity;

            var capacity = EncumbranceSystem.EncumbranceCapacity(strength, numAugs);

            var encumbrance = player.EncumbranceVal ?? 0;

            var burden = EncumbranceSystem.GetBurden(capacity, encumbrance);

            return burden;*/
            return null;
        }

        public bool InqRunRate(ref float rate)
        {
            // get run skill from WorldObject
            uint runSkill = 0;
            /*if (WorldObject is Creature creature)
                runSkill = creature.GetCreatureSkill(Skill.Run).Current;*/
            if (WorldObject?.IsCreature ?? false)
                runSkill = WorldObject.RunSkill;

            //rate = (float)MovementSystem.GetRunRate(0.0f, 300, 1.0f);
            rate = (float)MovementSystem.GetRunRate(0.0f, (int)runSkill, 1.0f);
            //Console.WriteLine($"{WorldObject.Name} ({WorldObject.Guid}) - WeenieObject.InqRunRate: runSkill = {runSkill}, rate = {rate}");
            return true;
        }

        public bool IsCorpse()
        {
            //return WorldObject is Corpse;
            return false;
        }

        public bool IsImpenetrable()
        {
            //return WorldObject is Player player && player.PlayerKillerStatus == PlayerKillerStatus.Free;
            return false;
        }

        public bool IsPK()
        {
            //return WorldObject is Player player && player.IsPK;
            return false;
        }

        public bool IsPKLite()
        {
            //return WorldObject is Player player && player.IsPKL;
            return false;
        }

        public bool IsPlayer()
        {
            //return WorldObject is Player;
            return WorldObject?.IsPlayer ?? false;
        }

        public bool IsCreature()
        {
            //return WorldObject is Creature;
            return WorldObject?.IsCreature ?? false;
        }

        public bool IsStorage()
        {
            //return WorldObject is Storage;
            return false;
        }

        public float JumpStaminaCost(float extent, int staminaCost)
        {
            return 0;
        }

        public void InqCollisionProfile(ObjCollisionProfile prof)
        {
            if (WorldObject == null)
                return;

            prof.WCID = ID;
            //prof.ItemType = WorldObject.ItemType;

            //if (WorldObject is Creature)
            if (WorldObject.IsCreature)
                prof.Flags |= ObjCollisionProfileFlags.Creature;

            //if (WorldObject is Player)
            if (WorldObject.IsPlayer)
                prof.Flags |= ObjCollisionProfileFlags.Player;

            //if (WorldObject.Attackable)
            if (WorldObject.IsCreature)
                prof.Flags |= ObjCollisionProfileFlags.Attackable;

            //if (WorldObject is Door)
                //prof.Flags |= ObjCollisionProfileFlags.Door;
        }

        public int DoCollision(ObjCollisionProfile prof, ObjectGuid guid, PhysicsObj target)
        {
            var wo = WorldObject;

            if (wo == null)
                return -1;

            var targetWO = target.WeenieObj.WorldObject;

            if (targetWO == null)
                return -1;

            // no collision with self
            if (wo.Guid.Equals(targetWO.Guid))
                return -1;

            /*Console.WriteLine("ObjCollisionProfile");
            Console.WriteLine("Source: " + WorldObject.Name);
            Console.WriteLine("Target: " + obj.WeenieObj.WorldObject.Name);*/

            //wo.OnCollideObject(targetWO);

            return 0;
        }

        public int DoCollision(EnvCollisionProfile prof, ObjectGuid guid, PhysicsObj target)
        {
            /*Console.WriteLine("EnvCollisionProfile");
            Console.WriteLine("Source: " + WorldObject.Name);
            Console.WriteLine("Target: " + target.WeenieObj.WorldObject.Name);
            Console.WriteLine("Velocity: " + prof.Velocity);*/

            var wo = WorldObject;

            if (wo == null)
                return 0;

            /*if (wo is Player player)
                player.HandleFallingDamage(prof);
            else
                wo.OnCollideEnvironment();*/

            return 0;
        }

        public void DoCollisionEnd(ObjectGuid targetGuid)
        {
            var wo = WorldObject;

            if (wo == null)
                return;

            /*var target = wo.CurrentLandblock?.GetObject(targetGuid);

            if (target != null)
                wo.OnCollideObjectEnd(target);*/
        }

        public void OnMotionDone(uint motionID, bool success)
        {
            //WorldObject.HandleMotionDone(motionID, success);
        }

        public void OnMoveComplete(WeenieError status)
        {
            //WorldObject.OnMoveComplete(status);
        }

        public bool CanBypassMoveRestrictions()
        {
            // acclient checks both of these here
            //return WorldObject.IgnoreHouseBarriers/* && WorldObject is Admin*/;
            return true;
        }

        public bool CanMoveInto(WeenieObject mover)
        {
            /*var house = WorldObject as House;
            if (house == null)
            {
                log.Error($"{WorldObject?.Name} ({WorldObject?.Guid}).CanMoveInto({mover.WorldObject?.Name} ({mover.WorldObject?.Guid}) - couldn't find house");
                return true;
            }
            var rootHouse = house.RootHouse;
            if (rootHouse == null)
            {
                log.Error($"{WorldObject?.Name} ({WorldObject?.Guid}).CanMoveInto({mover.WorldObject?.Name} ({mover.WorldObject?.Guid}) - couldn't find root house");
                return true;
            }
            var player = mover?.WorldObject as Player;
            if (player == null)
            {
                log.Error($"{WorldObject?.Name} ({WorldObject?.Guid}).CanMoveInto({mover.WorldObject?.Name} ({mover.WorldObject?.Guid}) - couldn't find player");
                return true;
            }
            var result = rootHouse.HouseOwner == null || rootHouse.OpenStatus || rootHouse.HasPermission(player);
            //Console.WriteLine($"{player.Name} can move into {rootHouse.Name} ({rootHouse.Guid}): {result}");
            return result;*/
            return true;
        }
    }
}
