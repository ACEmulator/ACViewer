using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.DatLoader;
using ACE.DatLoader.FileTypes;
using ACE.Entity;
using ACE.Entity.Enum;
using ACE.Server.Physics;
using ACE.Server.Physics.Common;
using ACE.Server.WorldObjects;

using ACViewer.Enum;
using ACViewer.Model;
using ACViewer.View;

namespace ACViewer
{
    public static class Picker
    {
        private static Viewport Viewport => GameView.Instance.GraphicsDevice.Viewport;
        
        private static Camera Camera => GameView.Camera;
        
        public static void HandleLeftClick(int mouseX, int mouseY)
        {
            //Console.WriteLine($"MouseX: {mouseX}, MouseY: {mouseY}");
            
            // get 3d coordinates at this screen position

            // do this by reversing the 3d -> 2d screen transform
            var projectionInverse = Matrix.Invert(Camera.ProjectionMatrix);
            
            var viewMatrix = Matrix.CreateLookAt(Vector3.Zero, Camera.Dir, Camera.Up);  // precision destabilizes @ large position values for this operation, factor out position here
            var viewInverse = Matrix.Invert(viewMatrix);

            var transform = projectionInverse * viewInverse;

            // convert viewport coords [-1, 1]
            var nx = mouseX * 2.0f / Viewport.Width - 1.0f;
            var ny = 1.0f - mouseY * 2.0f / Viewport.Height;    // invert y

            var viewportCoordNorm = new Vector3(nx, ny, 1.0f);

            var dir = Vector3.Normalize(Vector3.Transform(viewportCoordNorm, transform));

            TryFindObject(dir);
        }

        private static readonly uint setupId = 0x02000124;  // arrow

        private static readonly ObjectGuid pickerGuid = new ObjectGuid(0x80000000);   // first dynamic guid

        public static Vector3 Dir { get; set; }

        public static PickResult PickResult { get; set; }

        public static PickResult LastPickResult { get; set; }

        public static void TryFindObject(Vector3 dir)
        {
            HitVertices = null;
            Dir = dir;

            LastPickResult = PickResult;
            PickResult = new PickResult();
            
            // first try using physics engine for this

            // try spawning a tiny 'projectile' object at the camera position

            // if successfully spawned, simulate proceeding in that direction until something is hit

            var startPos = Camera.GetPosition();

            if (startPos == null)
            {
                Console.WriteLine($"Couldn't find current camera position in world!");
                return;
            }

            // todo: make this static
            var pickerObj = PhysicsObj.makeObject(setupId, pickerGuid.Full, true);
            pickerObj.State |= PhysicsState.PathClipped;
            pickerObj.State &= ~PhysicsState.Gravity;

            pickerObj.set_object_guid(pickerGuid);

            var worldObj = new WorldObject();
            worldObj.Name = "Picker";

            var weenie = new WeenieObject(worldObj);
            pickerObj.set_weenie_obj(weenie);

            var success = pickerObj.enter_world(startPos);

            if (!success)
            {
                Console.WriteLine($"Failed to spawn picker @ {startPos}");
                return;
            }

            //Console.WriteLine($"Successfully spawned picker @ {startPos}");

            var maxSteps = 500;
            var stepSize = 1.0f;

            // perform transition
            PhysicsObj.IsPicking = true;

            var showedMsg = false;

            for (var i = 0; i < maxSteps; i++)
            {
                var nextPos = new ACE.Server.Physics.Common.Position(pickerObj.Position);

                nextPos.Frame.Origin += (dir * stepSize).ToNumerics();

                var transition = pickerObj.transition(pickerObj.Position, nextPos, false);

                // debug collision info
                if (transition == null)
                {
                    Console.WriteLine($"Null transition result!");
                    showedMsg = true;
                    break;
                }
                else if (transition.CollisionInfo.CollidedWithEnvironment || transition.CollisionInfo.CollideObject.Count > 0)
                {
                    /*if (transition.CollisionInfo.CollidedWithEnvironment)
                        Console.WriteLine($"CollidedWithEnvironment");

                    if (transition.CollisionInfo.CollideObject.Count > 0)
                    {
                        Console.WriteLine($"CollideObjs:");
                        foreach (var collideObj in transition.CollisionInfo.CollideObject)
                            Console.WriteLine($"{collideObj.PartArray.Setup._dat.Id:X8} @ {collideObj.Position.ShortLoc()}");
                    }*/

                    BuildHitPolys();
                    showedMsg = true;
                    break;
                }
                else
                {
                    pickerObj.SetPositionInternal(transition);
                }
            }

            PhysicsObj.IsPicking = false;

            if (!showedMsg)
                Console.WriteLine($"No collisions");

            // cleanup
            pickerObj.DestroyObject();
        }

        public static VertexPositionColor[] HitVertices { get; set; }
        public static int[] HitIndices { get; set; }

        public static void BuildHitPolys()
        {
            if (PickResult == null) return;

            var hitVertices = new List<VertexPositionColor>();
            var hitIndices = new List<int>();
            var i = 0;

            switch (PickResult.Type)
            {
                case PickType.LandCell:

                    // 2 triangles, can toggle between single and cell mode
                    // defaulting to cell mode for now

                    var landCell = PickResult.ObjCell as LandCell;

                    // LandCell positions are stored as the origin in the center of the cell for some reason, as per LandblockStruct
                    // get the landblock origin
                    var landblockPos = new ACE.Server.Physics.Common.Position(landCell.Pos);
                    landblockPos.Frame.Origin = System.Numerics.Vector3.Zero;

                    var transform = landblockPos.ToXna();

                    foreach (var polygon in landCell.Polygons)
                    {
                        var startIdx = i;

                        foreach (var v in polygon.Vertices)
                        {
                            hitVertices.Add(new VertexPositionColor(Vector3.Transform(v.Origin.ToXna(), transform), Color.OrangeRed));
                            hitIndices.AddRange(new List<int>() { i, i + 1 });
                            i++;
                        }
                        hitIndices.RemoveAt(hitIndices.Count - 1);
                        hitIndices.Add(startIdx);
                    }

                    //var landblockInfo = DatManager.CellDat.ReadFromDat<LandblockInfo>(landCell.CurLandblock.Info.Id);

                    //if (landblockInfo != null)
                    //FileInfo.Instance.SetInfo(new FileTypes.LandblockInfo(landblockInfo).BuildTree());

                    var landblock = DatManager.CellDat.ReadFromDat<CellLandblock>(landCell.CurLandblock.ID);

                    if (landblock != null)
                        FileInfo.Instance.SetInfo(new FileTypes.CellLandblock(landblock).BuildTree());

                    break;

                case PickType.EnvCell:

                    // can toggle between single poly and full CellStruct polys
                    // defaulting to full CellStruct polys for now
                    
                    var envCell = PickResult.ObjCell as ACE.Server.Physics.Common.EnvCell;

                    transform = envCell.Pos.ToXna();

                    foreach (var polygon in envCell.CellStructure.Polygons.Values)
                    {
                        var startIdx = i;
                        
                        foreach (var v in polygon.Vertices)
                        {
                            hitVertices.Add(new VertexPositionColor(Vector3.Transform(v.Origin.ToXna(), transform), Color.OrangeRed));
                            hitIndices.AddRange(new List<int>() { i, i + 1 });
                            i++;
                        }
                        hitIndices.RemoveAt(hitIndices.Count - 1);
                        hitIndices.Add(startIdx);
                    }

                    var _envCell = DatManager.CellDat.ReadFromDat<ACE.DatLoader.FileTypes.EnvCell>(envCell.ID);

                    if (_envCell != null)
                        FileInfo.Instance.SetInfo(new FileTypes.EnvCell(_envCell).BuildTree());
                    
                    break;

                case PickType.GfxObj:

                    // can toggle between single poly, full gfxobj, and full setup
                    // default to full setup for now

                    foreach (var part in PickResult.PhysicsObj.PartArray.Parts)
                    {
                        transform = part.Pos.ToXna();

                        if (part.GfxObjScale != System.Numerics.Vector3.Zero)
                            transform = Matrix.CreateScale(part.GfxObjScale.ToXna()) * transform;

                        foreach (var polygon in part.GfxObj.Polygons.Values)
                        {
                            var startIdx = i;

                            foreach (var v in polygon.Vertices)
                            {
                                hitVertices.Add(new VertexPositionColor(Vector3.Transform(v.Origin.ToXna(), transform), Color.OrangeRed));
                                hitIndices.AddRange(new List<int>() { i, i + 1 });
                                i++;
                            }
                            hitIndices.RemoveAt(hitIndices.Count - 1);
                            hitIndices.Add(startIdx);
                        }
                    }

                    // gfxobj or setup?
                    var partArray = PickResult.PhysicsObj.PartArray;

                    var setupID = partArray.Setup._dat.Id;

                    if (setupID >> 24 == 0x2)
                    {
                        var setup = DatManager.PortalDat.ReadFromDat<SetupModel>(setupID);

                        if (setup != null)
                            FileInfo.Instance.SetInfo(new FileTypes.Setup(setup).BuildTree());
                    }
                    else
                    {
                        // in the case of simple setup, try to get GfxObj id from first part
                        var gfxObjId = partArray.Parts[0].GfxObj._dat.Id;

                        var gfxObj = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GfxObj>(gfxObjId);

                        if (gfxObj != null)
                            FileInfo.Instance.SetInfo(new FileTypes.GfxObj(gfxObj).BuildTree());
                    }
                    //else
                        //Console.WriteLine($"Unknown model ID for object @ {PickResult.PhysicsObj.Position}");

                    break;
            }
            HitVertices = hitVertices.ToArray();
            HitIndices = hitIndices.ToArray();
        }

        private static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;
        
        private static Effect Effect => Render.Render.Effect;

        public static void DrawHitPoly()
        {
            if (HitVertices == null) return;

            var rs = new RasterizerState();
            rs.CullMode = Microsoft.Xna.Framework.Graphics.CullMode.None;
            rs.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rs;

            Effect.CurrentTechnique = Effect.Techniques["Picker"];
            
            foreach (var pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, HitVertices, 0, HitVertices.Length, HitIndices, 0, HitIndices.Length / 2);
            }
        }
    }
}
