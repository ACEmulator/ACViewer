using System.Linq;

using ACE.DatLoader;
using ACE.DatLoader.Entity;
using ACE.DatLoader.FileTypes;
using ACE.Entity.Enum;
using ACE.Entity.Enum.Properties;
using ACE.Entity.Models;
using ACE.Server.Physics;

namespace ACE.Server.WorldObjects
{
    partial class WorldObject
    {
        private PhysicsState CalculatedPhysicsState()
        {
            // This is doing 2 things. It's pulling the default flags from the PropertyInt.PhysicsState, then in turn, setting the PropertyBool counterparts ONLY if they are null.
            // This seems a bit confusing...
            // If we really want to set default states on create or load, we need to separate this function into two parts.

            // Read in Object's Default PhysicsState
            var physicsState = GetPhysicsStateOrDefault();

            if (physicsState.HasFlag(PhysicsState.Static))
                if (!Static.HasValue)
                    Static = true;
            if (physicsState.HasFlag(PhysicsState.Ethereal))
                if (!Ethereal.HasValue)
                    Ethereal = true;
            if (physicsState.HasFlag(PhysicsState.ReportCollisions))
                if (!ReportCollisions.HasValue)
                    ReportCollisions = true;
            if (physicsState.HasFlag(PhysicsState.IgnoreCollisions))
                if (!IgnoreCollisions.HasValue)
                    IgnoreCollisions = true;
            if (physicsState.HasFlag(PhysicsState.NoDraw))
                if (!NoDraw.HasValue)
                    NoDraw = true;
            if (physicsState.HasFlag(PhysicsState.Missile))
                if (!Missile.HasValue)
                    Missile = true;
            if (physicsState.HasFlag(PhysicsState.Pushable))
                if (!Pushable.HasValue)
                    Pushable = true;
            if (physicsState.HasFlag(PhysicsState.AlignPath))
                if (!AlignPath.HasValue)
                    AlignPath = true;
            if (physicsState.HasFlag(PhysicsState.PathClipped))
                if (!PathClipped.HasValue)
                    PathClipped = true;
            if (physicsState.HasFlag(PhysicsState.Gravity))
                if (!GravityStatus.HasValue)
                    GravityStatus = true;
            if (physicsState.HasFlag(PhysicsState.LightingOn))
                if (!LightsStatus.HasValue)
                    LightsStatus = true;
            if (physicsState.HasFlag(PhysicsState.ParticleEmitter))
                if (!ParticleEmitter.HasValue)
                    ParticleEmitter = true;
            if (physicsState.HasFlag(PhysicsState.Hidden))
                if (!Hidden.HasValue)
                    Hidden = true;
            if (physicsState.HasFlag(PhysicsState.ScriptedCollision))
                if (!ScriptedCollision.HasValue)
                    ScriptedCollision = true;
            if (physicsState.HasFlag(PhysicsState.Inelastic))
                if (!Inelastic.HasValue)
                    Inelastic = true;
            if (physicsState.HasFlag(PhysicsState.Cloaked))
                if (!Cloaked.HasValue)
                    Cloaked = true;
            if (physicsState.HasFlag(PhysicsState.ReportCollisionsAsEnvironment))
                if (!ReportCollisionsAsEnvironment.HasValue)
                    ReportCollisionsAsEnvironment = true;
            if (physicsState.HasFlag(PhysicsState.EdgeSlide))
                if (!AllowEdgeSlide.HasValue)
                    AllowEdgeSlide = true;
            if (physicsState.HasFlag(PhysicsState.Sledding))
                if (!Sledding.HasValue)
                    Sledding = true;
            if (physicsState.HasFlag(PhysicsState.Frozen))
                if (!IsFrozen.HasValue)
                    IsFrozen = true;

            ////Static                      = 0x00000001,
            if (Static ?? false)
                physicsState |= PhysicsState.Static;
            else
                physicsState &= ~PhysicsState.Static;
            ////Unused1                     = 0x00000002,
            ////Ethereal                    = 0x00000004,
            if (Ethereal ?? false)
                physicsState |= PhysicsState.Ethereal;
            else
                physicsState &= ~PhysicsState.Ethereal;
            ////ReportCollision             = 0x00000008,
            if (ReportCollisions ?? false)
                physicsState |= PhysicsState.ReportCollisions;
            else
                physicsState &= ~PhysicsState.ReportCollisions;
            ////IgnoreCollision             = 0x00000010,
            if (IgnoreCollisions ?? false)
                physicsState |= PhysicsState.IgnoreCollisions;
            else
                physicsState &= ~PhysicsState.IgnoreCollisions;
            ////NoDraw                      = 0x00000020,
            if (NoDraw ?? false)
                physicsState |= PhysicsState.NoDraw;
            else
                physicsState &= ~PhysicsState.NoDraw;
            ////Missile                     = 0x00000040,
            if (Missile ?? false)
                physicsState |= PhysicsState.Missile;
            else
                physicsState &= ~PhysicsState.Missile;
            ////Pushable                    = 0x00000080,
            if (Pushable ?? false)
                physicsState |= PhysicsState.Pushable;
            else
                physicsState &= ~PhysicsState.Pushable;
            ////AlignPath                   = 0x00000100,
            if (AlignPath ?? false)
                physicsState |= PhysicsState.AlignPath;
            else
                physicsState &= ~PhysicsState.AlignPath;
            ////PathClipped                 = 0x00000200,
            if (PathClipped ?? false)
                physicsState |= PhysicsState.PathClipped;
            else
                physicsState &= ~PhysicsState.PathClipped;
            ////Gravity                     = 0x00000400,
            if (GravityStatus ?? false)
                physicsState |= PhysicsState.Gravity;
            else
                physicsState &= ~PhysicsState.Gravity;
            ////LightingOn                  = 0x00000800,
            if (LightsStatus ?? false)
                physicsState |= PhysicsState.LightingOn;
            else
                physicsState &= ~PhysicsState.LightingOn;
            ////ParticleEmitter             = 0x00001000,
            if (ParticleEmitter ?? false)
                physicsState |= PhysicsState.ParticleEmitter;
            else
                physicsState &= ~PhysicsState.ParticleEmitter;
            ////Unused2                     = 0x00002000,
            ////Hidden                      = 0x00004000,
            if (Hidden ?? false)
                physicsState |= PhysicsState.Hidden;
            else
                physicsState &= ~PhysicsState.Hidden;
            ////ScriptedCollision           = 0x00008000,
            if (ScriptedCollision ?? false)
                physicsState |= PhysicsState.ScriptedCollision;
            else
                physicsState &= ~PhysicsState.ScriptedCollision;
            ////HasPhysicsBSP               = 0x00010000,
            if (CSetup.HasPhysicsBSP)
                physicsState |= PhysicsState.HasPhysicsBSP;
            else
                physicsState &= ~PhysicsState.HasPhysicsBSP;
            ////Inelastic                   = 0x00020000,
            if (Inelastic ?? false)
                physicsState |= PhysicsState.Inelastic;
            else
                physicsState &= ~PhysicsState.Inelastic;
            ////HasDefaultAnim              = 0x00040000,
            if (PhysicsObj != null && PhysicsObj.HasDefaultAnimation && CSetup.DefaultAnimation > 0)
                physicsState |= PhysicsState.HasDefaultAnim;
            else
                physicsState &= ~PhysicsState.HasDefaultAnim;
            ////HasDefaultScript            = 0x00080000,
            if (PhysicsObj != null && PhysicsObj.HasDefaultScript && CSetup.DefaultScript > 0)
                physicsState |= PhysicsState.HasDefaultScript;
            else
                physicsState &= ~PhysicsState.HasDefaultScript;
            ////Cloaked                     = 0x00100000,
            if (Cloaked ?? false)
                physicsState |= PhysicsState.Cloaked;
            else
                physicsState &= ~PhysicsState.Cloaked;
            ////ReportCollisionAsEnviroment = 0x00200000,
            if (ReportCollisionsAsEnvironment ?? false)
                physicsState |= PhysicsState.ReportCollisionsAsEnvironment;
            else
                physicsState &= ~PhysicsState.ReportCollisionsAsEnvironment;
            ////EdgeSlide                   = 0x00400000,
            if (AllowEdgeSlide ?? false)
                physicsState |= PhysicsState.EdgeSlide;
            else
                physicsState &= ~PhysicsState.EdgeSlide;
            ////Sledding                    = 0x00800000,
            if (Sledding ?? false)
                physicsState |= PhysicsState.Sledding;
            else
                physicsState &= ~PhysicsState.Sledding;
            ////Frozen                      = 0x01000000,
            if (IsFrozen ?? false)
                physicsState |= PhysicsState.Frozen;
            else
                physicsState &= ~PhysicsState.Frozen;

            return physicsState;
        }

        /// <summary>
        /// Returns the current physics state for an object,
        /// falling back to defaults if no PhysicsObj is loaded (inventory items)
        /// </summary>
        private PhysicsState GetPhysicsStateOrDefault()
        {
            if (PhysicsObj != null)
                return PhysicsObj.State;

            // special case for players logging in - sets pink bubble state here
            if (this is Player)
                return PhysicsState.IgnoreCollisions | PhysicsState.Gravity | PhysicsState.Hidden | PhysicsState.EdgeSlide;

            var defaultObjState = GetProperty(PropertyInt.PhysicsState);

            if (defaultObjState != null)
                return (PhysicsState)defaultObjState;
            else
                return PhysicsGlobals.DefaultState;
        }

        public bool? IgnoreCloIcons
        {
            get => GetProperty(PropertyBool.IgnoreCloIcons);
            set { if (!value.HasValue) RemoveProperty(PropertyBool.IgnoreCloIcons); else SetProperty(PropertyBool.IgnoreCloIcons, value.Value); }
        }

        public bool? Dyable
        {
            get => GetProperty(PropertyBool.Dyable);
            set { if (!value.HasValue) RemoveProperty(PropertyBool.Dyable); else SetProperty(PropertyBool.Dyable, value.Value); }
        }

        public virtual ACE.Entity.ObjDesc CalculateObjDesc()
        {
            if (this is Hook hook && hook.HasItem)
                return hook.Item.CalculateObjDesc();

            ACE.Entity.ObjDesc objDesc = new ACE.Entity.ObjDesc();
            ClothingTable item;

            AddBaseModelData(objDesc);

            if (ClothingBase.HasValue)
                item = DatManager.PortalDat.ReadFromDat<ClothingTable>((uint)ClothingBase);
            else
            {
                return objDesc;
            }

            if (item.ClothingBaseEffects.ContainsKey(SetupTableId))
            // Check if the player model has data. Gear Knights, this is usually you.
            {
                // Add the model and texture(s)
                ClothingBaseEffect clothingBaseEffect = item.ClothingBaseEffects[SetupTableId];
                foreach (CloObjectEffect t in clothingBaseEffect.CloObjectEffects)
                {
                    byte partNum = (byte)t.Index;
                    objDesc.AnimPartChanges.Add(new PropertiesAnimPart { Index = (byte)t.Index, AnimationId = t.ModelId });
                    //AddModel((byte)t.Index, (ushort)t.ModelId);
                    foreach (CloTextureEffect t1 in t.CloTextureEffects)
                        objDesc.TextureChanges.Add(new PropertiesTextureMap { PartIndex = (byte)t.Index, OldTexture = t1.OldTexture, NewTexture = t1.NewTexture });
                    //AddTexture((byte)t.Index, (ushort)t1.OldTexture, (ushort)t1.NewTexture);
                }

                //if (item.ClothingSubPalEffects.Count == 1 && (PaletteTemplate.HasValue | Shade.HasValue))
                //    Console.WriteLine($"Found an item with 1 ClothingSubPalEffects and a PaletteTemplate = {PaletteTemplate} and/or Shade = {Shade} ");

                if (item.ClothingSubPalEffects.Count > 0)
                {
                    //int size = item.ClothingSubPalEffects.Count;
                    //int palCount = size;

                    CloSubPalEffect itemSubPal;
                    int palOption = 0;
                    if (PaletteTemplate.HasValue)
                        palOption = (int)PaletteTemplate;
                    if (item.ClothingSubPalEffects.ContainsKey((uint)palOption))
                    {
                        itemSubPal = item.ClothingSubPalEffects[(uint)palOption];
                    }
                    else
                    {
                        itemSubPal = item.ClothingSubPalEffects[item.ClothingSubPalEffects.Keys.ElementAt(0)];
                    }

                    if (itemSubPal.Icon > 0 && !(IgnoreCloIcons ?? false) && (Shade.HasValue || PaletteTemplate.HasValue))
                        IconId = itemSubPal.Icon;

                    float shade = 0;
                    if (Shade.HasValue)
                        shade = (float)Shade;
                    for (int i = 0; i < itemSubPal.CloSubPalettes.Count; i++)
                    {
                        var itemPalSet = DatManager.PortalDat.ReadFromDat<PaletteSet>(itemSubPal.CloSubPalettes[i].PaletteSet);
                        ushort itemPal = (ushort)itemPalSet.GetPaletteID(shade);

                        for (int j = 0; j < itemSubPal.CloSubPalettes[i].Ranges.Count; j++)
                        {
                            ushort palOffset = (ushort)(itemSubPal.CloSubPalettes[i].Ranges[j].Offset / 8);
                            ushort numColors = (ushort)(itemSubPal.CloSubPalettes[i].Ranges[j].NumColors / 8);
                            if (PaletteTemplate.HasValue || Shade.HasValue)
                                objDesc.SubPalettes.Add(new PropertiesPalette { SubPaletteId = itemPal, Offset = palOffset, Length = numColors });
                            //AddPalette(itemPal, (ushort)palOffset, (ushort)numColors);
                        }
                    }
                }
            }

            return objDesc;
        }

        protected void AddBaseModelData(ACE.Entity.ObjDesc objDesc)
        {
            // Hair/head

            // if (HeadObjectDID.HasValue && !HairStyle.HasValue)
            // This Heritage check has been added for backwards compatibility. It works around the butthead Gear Knights appearance.
            if (HeadObjectDID.HasValue && !HairStyle.HasValue && Heritage.HasValue && Heritage != (int)HeritageGroup.Gearknight)
                objDesc.AnimPartChanges.Add(new PropertiesAnimPart { Index = 0x10, AnimationId = HeadObjectDID.Value });
            else if (HairStyle.HasValue && Heritage.HasValue && Gender.HasValue)
            {
                // This indicates we have a Gear Knight or Olthoi(that is, player types treat "hairstyle" as a "Body Style")

                // Load the CharGen data. It has all the anim & texture changes for the Body Style defined within it
                var cg = DatManager.PortalDat.CharGen;
                SexCG sex = cg.HeritageGroups[(uint)Heritage].Genders[(int)Gender];
                if (sex.HairStyleList.Count > (int)HairStyle) // just check for a valid entry...
                {
                    HairStyleCG hairstyle = sex.HairStyleList[(int)HairStyle];

                    // Add all the texture changes
                    foreach (var tm in hairstyle.ObjDesc.TextureChanges)
                        objDesc.TextureChanges.Add(new PropertiesTextureMap { PartIndex = tm.PartIndex, OldTexture = tm.OldTexture, NewTexture = tm.NewTexture });

                    // Add all the animation part changes
                    foreach (var part in hairstyle.ObjDesc.AnimPartChanges)
                        objDesc.AnimPartChanges.Add(new PropertiesAnimPart { Index = part.PartIndex, AnimationId = part.PartID });
                }
            }

            if (this is Player player)
                objDesc.TextureChanges.Add(new PropertiesTextureMap { PartIndex = 0x10, OldTexture = player.Character.DefaultHairTexture, NewTexture = player.Character.HairTexture });
            //AddTexture(0x10, DefaultHairTextureDID.Value, HairTextureDID.Value);
            if (HairPaletteDID.HasValue)
                objDesc.SubPalettes.Add(new PropertiesPalette { SubPaletteId = HairPaletteDID.Value, Offset = 0x18, Length = 0x8 });
            //AddPalette(HairPaletteDID.Value, 0x18, 0x8);

            // Skin
            // PaletteBaseId = PaletteBaseDID;
            if (PaletteBaseDID.HasValue)
                objDesc.PaletteID = PaletteBaseDID.Value;
            if (SkinPaletteDID.HasValue)
                objDesc.SubPalettes.Add(new PropertiesPalette { SubPaletteId = SkinPaletteDID.Value, Offset = 0x0, Length = 0x18 });
            //AddPalette(SkinPalette.Value, 0x0, 0x18);

            // Eyes
            if (DefaultEyesTextureDID.HasValue && EyesTextureDID.HasValue)
                objDesc.TextureChanges.Add(new PropertiesTextureMap { PartIndex = 0x10, OldTexture = DefaultEyesTextureDID.Value, NewTexture = EyesTextureDID.Value });
            //AddTexture(0x10, DefaultEyesTextureDID.Value, EyesTextureDID.Value);
            if (EyesPaletteDID.HasValue)
                objDesc.SubPalettes.Add(new PropertiesPalette { SubPaletteId = EyesPaletteDID.Value, Offset = 0x20, Length = 0x8 });
            //AddPalette(EyesPaletteDID.Value, 0x20, 0x8);

            // Nose & Mouth
            if (DefaultNoseTextureDID.HasValue && NoseTextureDID.HasValue)
                objDesc.TextureChanges.Add(new PropertiesTextureMap { PartIndex = 0x10, OldTexture = DefaultNoseTextureDID.Value, NewTexture = NoseTextureDID.Value });
            //AddTexture(0x10, NoseTextureDID.Value, NoseTextureDID.Value);
            if (DefaultMouthTextureDID.HasValue && MouthTextureDID.HasValue)
                objDesc.TextureChanges.Add(new PropertiesTextureMap { PartIndex = 0x10, OldTexture = DefaultMouthTextureDID.Value, NewTexture = MouthTextureDID.Value });
            //AddTexture(0x10, DefaultMouthTextureDID.Value, MouthTextureDID.Value);
        }
    }
}
