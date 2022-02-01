using System;
using System.Numerics;

using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Entity.Models;
using ACE.Server.Entity;

namespace ACE.Server.WorldObjects
{
    public class SpellProjectile : WorldObject
    {
        public Spell Spell;
        public ProjectileSpellType SpellType { get; set; }

        public Position SpawnPos { get; set; }
        public float DistanceToTarget { get; set; }
        public uint LifeProjectileDamage { get; set; }

        //public SpellProjectileInfo Info { get; set; }

        /// <summary>
        /// Only set to true when this spell was launched by using the built-in spell on a caster
        /// </summary>
        public bool IsWeaponSpell { get; set; }

        /// <summary>
        /// If a spell projectile is from a proc source,
        /// make sure there is no attempt to re-proc again when the spell projectile hits
        /// </summary>
        public bool FromProc { get; set; }

        public int DebugVelocity;

        /// <summary>
        /// A new biota be created taking all of its values from weenie.
        /// </summary>
        public SpellProjectile(Weenie weenie, ObjectGuid guid) : base(weenie, guid)
        {
            SetEphemeralValues();
        }

        /// <summary>
        /// Restore a WorldObject from the database.
        /// </summary>
        public SpellProjectile(Biota biota) : base(biota)
        {
            SetEphemeralValues();
        }

        private void SetEphemeralValues()
        {
            // Override weenie description defaults
            ValidLocations = null;
            DefaultScriptId = null;
        }

        /// <summary>
        /// Perfroms additional set up of the spell projectile based on the spell id or its derived type.
        /// </summary>
        public void Setup(Spell spell, ProjectileSpellType spellType)
        {
            Spell = spell;
            SpellType = spellType;

            //InitPhysicsObj();

            // Runtime changes to default state
            ReportCollisions = true;
            Missile = true;
            AlignPath = true;
            PathClipped = true;
            IgnoreCollisions = false;

            // FIXME: use data here
            if (!Spell.Name.Equals("Rolling Death"))
                Ethereal = false;

            if (SpellType == ProjectileSpellType.Bolt || SpellType == ProjectileSpellType.Streak
                || SpellType == ProjectileSpellType.Arc || SpellType == ProjectileSpellType.Volley || SpellType == ProjectileSpellType.Blast
                || WeenieClassId == 7276 || WeenieClassId == 7277 || WeenieClassId == 7279 || WeenieClassId == 7280)
            {
                DefaultScriptId = (uint)PlayScript.ProjectileCollision;
                DefaultScriptIntensity = 1.0f;
            }

            // Some wall spells don't have scripted collisions
            if (WeenieClassId == 7278 || WeenieClassId == 7281 || WeenieClassId == 7282 || WeenieClassId == 23144)
            {
                ScriptedCollision = false;
            }

            AllowEdgeSlide = false;

            // No need to send an ObjScale of 1.0f over the wire since that is the default value
            if (ObjScale == 1.0f)
                ObjScale = null;

            if (SpellType == ProjectileSpellType.Ring)
            {
                if (spell.Id == 3818)
                {
                    DefaultScriptId = (uint)PlayScript.Explode;
                    DefaultScriptIntensity = 1.0f;
                    ScriptedCollision = true;
                }
                else
                {
                    ScriptedCollision = false;
                }
            }

            // Projectiles with RotationSpeed get omega values and "align path" turned off which
            // creates the nice swirling animation
            if ((RotationSpeed ?? 0) != 0)
            {
                AlignPath = false;
                PhysicsObj.Omega = new Vector3((float)(Math.PI * 2 * RotationSpeed), 0, 0);
            }
        }

        public static ProjectileSpellType GetProjectileSpellType(uint spellID)
        {
            var spell = new Spell(spellID);

            if (spell.Wcid == 0)
                return ProjectileSpellType.Undef;

            if (spell.NumProjectiles == 1)
            {
                if (spell.Category >= SpellCategory.AcidStreak && spell.Category <= SpellCategory.SlashingStreak ||
                         spell.Category == SpellCategory.NetherStreak || spell.Category == SpellCategory.Fireworks)
                    return ProjectileSpellType.Streak;

                else if (spell.NonTracking)
                    return ProjectileSpellType.Arc;

                else
                    return ProjectileSpellType.Bolt;
            }

            if (spell.Category >= SpellCategory.AcidRing && spell.Category <= SpellCategory.SlashingRing || spell.SpreadAngle == 360)
                return ProjectileSpellType.Ring;

            if (spell.Category >= SpellCategory.AcidBurst && spell.Category <= SpellCategory.SlashingBurst ||
                spell.Category == SpellCategory.NetherDamageOverTimeRaising3)
                return ProjectileSpellType.Blast;

            // 1481 - Flaming Missile Volley
            if (spell.Category >= SpellCategory.AcidVolley && spell.Category <= SpellCategory.BladeVolley || spell.Name.Contains("Volley"))
                return ProjectileSpellType.Volley;

            if (spell.Category >= SpellCategory.AcidWall && spell.Category <= SpellCategory.SlashingWall)
                return ProjectileSpellType.Wall;

            if (spell.Category >= SpellCategory.AcidStrike && spell.Category <= SpellCategory.SlashingStrike)
                return ProjectileSpellType.Strike;

            return ProjectileSpellType.Undef;
        }

        public float GetProjectileScriptIntensity(ProjectileSpellType spellType)
        {
            if (spellType == ProjectileSpellType.Wall)
            {
                return 0.4f;
            }
            if (spellType == ProjectileSpellType.Ring)
            {
                if (Spell.Level == 6 || Spell.Id == 3818)
                    return 0.4f;
                if (Spell.Level == 7)
                    return 1.0f;
            }

            // Bolt, Blast, Volley, Streak and Arc all seem to use this scale
            // TODO: should this be based on spell level, or power of first scarab?
            // ie. can this use Spell.Formula.ScarabScale?
            switch (Spell.Level)
            {
                case 1:
                    return 0f;
                case 2:
                    return 0.2f;
                case 3:
                    return 0.4f;
                case 4:
                    return 0.6f;
                case 5:
                    return 0.8f;
                case 6:
                case 7:
                case 8:
                    return 1.0f;
                default:
                    return 0f;
            }
        }

        public bool WorldEntryCollision { get; set; }

        public void ProjectileImpact()
        {
            //Console.WriteLine($"{Name}.ProjectileImpact()");

            ReportCollisions = false;
            Ethereal = true;
            IgnoreCollisions = true;
            NoDraw = true;
            Cloaked = true;
            LightsStatus = false;

            PhysicsObj.set_active(false);

            if (PhysicsObj.entering_world)
            {
                // this path should only happen if spell_projectile_ethereal = false
                //EnqueueBroadcast(new GameMessageScript(Guid, PlayScript.Launch, GetProjectileScriptIntensity(SpellType)));
                WorldEntryCollision = true;
            }

            //EnqueueBroadcast(new GameMessageSetState(this, PhysicsObj.State));
            //EnqueueBroadcast(new GameMessageScript(Guid, PlayScript.Explode, GetProjectileScriptIntensity(SpellType)));

            // this should only be needed for spell_projectile_ethereal = true,
            // however it can also fix a display issue on client in default mode,
            // where GameMessageSetState updates projectile to ethereal before it has actually collided on client,
            // causing a 'ghost' projectile to continue to sail through the target

            PhysicsObj.Velocity = Vector3.Zero;
            //EnqueueBroadcast(new GameMessageVectorUpdate(this));

            /*ActionChain selfDestructChain = new ActionChain();
            selfDestructChain.AddDelaySeconds(5.0);
            selfDestructChain.AddAction(this, () => Destroy());
            selfDestructChain.EnqueueChain();*/
        }

        /// <summary>
        /// Handles collision with scenery or other static objects that would block a projectile from reaching its target,
        /// in which case the projectile should be removed with no further processing.
        /// </summary>
        public override void OnCollideEnvironment()
        {
            //Console.WriteLine($"{Name}.OnCollideEnvironment()");

            /*if (Info != null && ProjectileSource is Player player && player.DebugSpell)
            {
                player.Session.Network.EnqueueSend(new GameMessageSystemChat($"{Name}.OnCollideEnvironment()", ChatMessageType.Broadcast));
                player.Session.Network.EnqueueSend(new GameMessageSystemChat(Info.ToString(), ChatMessageType.Broadcast));
            }*/

            ProjectileImpact();
        }

        public override void OnCollideObject(WorldObject target)
        {
            //Console.WriteLine($"{Name}.OnCollideObject({target.Name})");
        }

        /// <summary>
        /// Sets the physics state for a launched projectile
        /// </summary>
        public void SetProjectilePhysicsState(WorldObject target, bool useGravity)
        {
            if (useGravity)
                GravityStatus = true;

            //CurrentMotionState = null;
            Placement = null;

            // TODO: Physics description timestamps (sequence numbers) don't seem to be getting updated

            //Console.WriteLine("SpellProjectile PhysicsState: " + PhysicsObj.State);

            var pos = Location.Pos;
            var rotation = Location.Rotation;
            PhysicsObj.Position.Frame.Origin = pos;
            PhysicsObj.Position.Frame.Orientation = rotation;

            var velocity = Velocity;
            //velocity = Vector3.Transform(velocity, Matrix4x4.Transpose(Matrix4x4.CreateFromQuaternion(rotation)));
            PhysicsObj.Velocity = velocity;

            if (target != null)
                PhysicsObj.ProjectileTarget = target.PhysicsObj;

            PhysicsObj.set_active(true);
        }
    }
}
