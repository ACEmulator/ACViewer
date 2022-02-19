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
using ACE.Server.Physics.Util;
using ACE.Server.WorldObjects;

using ACViewer.Enum;
using ACViewer.Model;
using ACViewer.Primitives;
using ACViewer.Render;
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

            // convert viewport coords to [-1, 1]
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
            Dir = dir;

            LastPickResult = PickResult;

            ClearSelection();

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

            var maxSteps = 500;
            var stepSize = 1.0f;
            var i = 0;

            var stepDir = (dir * stepSize).ToNumerics();

            var singleBlock = WorldViewer.Instance.SingleBlock;

            if (singleBlock != uint.MaxValue)
            {
                var landblock = LScape.get_landblock(singleBlock);
                
                // custom for single landblock IsDungeon
                if (landblock.IsDungeon)
                {
                    if (startPos.Landblock != singleBlock >> 16)
                        startPos.Reframe(singleBlock);
                    
                    var adjustCell = AdjustCell.Get(startPos.Landblock);

                    for ( ; i < maxSteps; i++)
                    {
                        var foundCell = adjustCell.GetCell(startPos.Frame.Origin);

                        if (foundCell != null)
                        {
                            startPos.ObjCellID = foundCell.Value;
                            break;
                        }
                        startPos.Frame.Origin += stepDir;
                    }
                }
            }

            // todo: make this static
            var pickerObj = PhysicsObj.makeObject(setupId, pickerGuid.Full, true);
            pickerObj.State |= PhysicsState.PathClipped;
            pickerObj.State &= ~PhysicsState.Gravity;

            pickerObj.set_object_guid(pickerGuid);

            var worldObj = new WorldObject();
            //worldObj.Name = "Picker";

            var weenie = new WeenieObject(worldObj);
            pickerObj.set_weenie_obj(weenie);

            // perform transition
            PhysicsObj.IsPicking = true;

            var showedMsg = false;

            var spawned = false;

            for ( ; i < maxSteps; i++)
            {
                if (!spawned)
                {
                    var success = pickerObj.enter_world(startPos);

                    if (!success)
                    {
                        startPos.Frame.Origin += stepDir;
                        continue;
                    }
                    else
                    {
                        //Console.WriteLine($"Successfully spawned picker @ {startPos}");
                        spawned = true;
                    }
                }

                var nextPos = new ACE.Server.Physics.Common.Position(pickerObj.Position);

                nextPos.Frame.Origin += stepDir;

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

            if (!spawned)
                Console.WriteLine($"Failed to spawn picker @ {Camera.GetPosition()}");
            else if (!showedMsg)
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

//                    MainWindow.Instance.Status.WriteLine($"Selected {landCell.ID:X8}");

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

                    //MainWindow.Instance.Status.WriteLine($"Selected {envCell.ID:X8}");

                    break;

                case PickType.GfxObj:

                    // can toggle between single poly, full gfxobj, and full setup
                    // default to full setup for now

                    foreach (var part in PickResult.PhysicsObj.PartArray.Parts)
                    {
                        if (part.GfxObj.ID == 0x010001ec)   // skip anchor locations
                            continue;

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

                        //MainWindow.Instance.Status.WriteLine($"Selected {setupID:X8}");
                    }
                    else
                    {
                        // in the case of simple setup, try to get GfxObj id from first part
                        var gfxObjId = partArray.Parts[0].GfxObj._dat.Id;

                        var gfxObj = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.GfxObj>(gfxObjId);

                        if (gfxObj != null)
                            FileInfo.Instance.SetInfo(new FileTypes.GfxObj(gfxObj).BuildTree());

                        //MainWindow.Instance.Status.WriteLine($"Selected {gfxObjId:X8}");
                    }
                    //else
                        //Console.WriteLine($"Unknown model ID for object @ {PickResult.PhysicsObj.Position}");

                    if (PickResult.PhysicsObj.WeenieObj?.WorldObject != null)
                    {
                        var wo = PickResult.PhysicsObj.WeenieObj.WorldObject;

                        FileInfo.Instance.SetInfo(new FileTypes.WorldObject(wo).BuildTree());
                        BuildLinks(wo);

                        if (wo is Portal portal && LastPickResult?.PhysicsObj != null && wo.PhysicsObj == LastPickResult.PhysicsObj && PickResult.ClickTime - LastPickResult.ClickTime < PickResult.DoubleClickTime)
                        {
                            if (portal.Destination != null)
                            {
                                Teleport.Origin = portal.Destination.Pos;
                                Teleport.Orientation = portal.Destination.Rotation;

                                Teleport.teleport(portal.Destination.Cell);
                            }
                            else
                                Console.WriteLine($"Portal destination null for {portal.WeenieClassId} - {portal.Name} @ {portal.PhysicsObj.Position}");
                        }
                    }
                    break;
            }
            HitVertices = hitVertices.ToArray();
            HitIndices = hitIndices.ToArray();
        }

        public static void AddVisibleCells()
        {
            var envCell = PickResult?.ObjCell as ACE.Server.Physics.Common.EnvCell;

            if (envCell == null) return;
            
            var hitVertices = new List<VertexPositionColor>(HitVertices);
            var hitIndices = new List<int>(HitIndices);
            
            var i = hitVertices.Count;
            
            foreach (var visibleCell in envCell.VisibleCells.Values)
            {
                var transform = visibleCell.Pos.ToXna();

                foreach (var polygon in visibleCell.CellStructure.Polygons.Values)
                {
                    var startIdx = i;

                    foreach (var v in polygon.Vertices)
                    {
                        hitVertices.Add(new VertexPositionColor(Vector3.Transform(v.Origin.ToXna(), transform), Color.Orange));
                        hitIndices.AddRange(new List<int>() { i, i + 1 });
                        i++;
                    }
                    hitIndices.RemoveAt(hitIndices.Count - 1);
                    hitIndices.Add(startIdx);
                }
            }

            HitVertices = hitVertices.ToArray();
            HitIndices = hitIndices.ToArray();
        }

        private static readonly SpherePrimitive SpherePrimitive = new SpherePrimitive(GraphicsDevice, 1.0f, 10);

        private static readonly CylinderPrimitive CylinderPrimitive = new CylinderPrimitive(GraphicsDevice, 1.0f, 1.0f, 20);

        public static List<Matrix> SphereTransforms { get; set; }

        public static List<Matrix> CylinderTransforms { get; set; }

        public static VertexPositionColor[] PhysicsVertices { get; set; }
        public static int[] PhysicsIndices { get; set; }

        public static void ShowCollision()
        {
            if (PickResult == null) return;
            
            SphereTransforms = null;
            CylinderTransforms = null;
            PhysicsVertices = null;
            PhysicsIndices = null;

            if (PickResult.PhysicsObj != null)
            {
                if (PickResult.PhysicsObj.PartArray.GetNumSphere() > 0)
                {
                    SphereTransforms = new List<Matrix>();

                    var worldPos = PickResult.PhysicsObj.Position.GetWorldPos();

                    foreach (var sphere in PickResult.PhysicsObj.PartArray.GetSphere())
                    {
                        var sphereDiameter = sphere.Radius * 2 * PickResult.PhysicsObj.Scale;

                        // translate local center by rotation, but not world pos
                        var transform = Matrix.CreateScale(sphereDiameter) * Matrix.CreateTranslation(sphere.Center.ToXna()) * Matrix.CreateFromQuaternion(PickResult.PhysicsObj.Position.Frame.Orientation.ToXna()) * Matrix.CreateTranslation(worldPos);

                        SphereTransforms.Add(transform);
                    }
                }

                if (PickResult.PhysicsObj.PartArray.GetNumCylsphere() > 0)
                {
                    CylinderTransforms = new List<Matrix>();

                    var worldPos = PickResult.PhysicsObj.Position.GetWorldPos();

                    foreach (var cylSphere in PickResult.PhysicsObj.PartArray.GetCylSphere())
                    {
                        var cylSphereDiameter = cylSphere.Radius * 2 * PickResult.PhysicsObj.Scale;
                        var cylSphereHeight = cylSphere.Height * PickResult.PhysicsObj.Scale;

                        // translate local low point by rotation, but not world pos
                        var transform = Matrix.CreateScale(new Vector3(cylSphereDiameter, cylSphereDiameter, cylSphereHeight)) * Matrix.CreateTranslation(cylSphere.LowPoint.ToXna()) * Matrix.CreateFromQuaternion(PickResult.PhysicsObj.Position.Frame.Orientation.ToXna()) * Matrix.CreateTranslation(worldPos);

                        CylinderTransforms.Add(transform);
                    }
                }

                if (PickResult.PhysicsObj.State.HasFlag(PhysicsState.HasPhysicsBSP))
                {
                    var physicsVertices = new List<VertexPositionColor>();
                    var physicsIndices = new List<int>();

                    var i = 0;

                    foreach (var part in PickResult.PhysicsObj.PartArray.Parts)
                    {
                        if (part.GfxObj.ID == 0x010001ec)   // skip anchor locations
                            continue;

                        var transform = part.Pos.ToXna();

                        if (part.GfxObjScale != System.Numerics.Vector3.Zero)
                            transform = Matrix.CreateScale(part.GfxObjScale.ToXna()) * transform;

                        foreach (var polygon in part.GfxObj.PhysicsPolygons.Values)
                        {
                            var startIdx = i;

                            foreach (var v in polygon.Vertices)
                            {
                                physicsVertices.Add(new VertexPositionColor(Vector3.Transform(v.Origin.ToXna(), transform), Color.Orange));
                                physicsIndices.AddRange(new List<int>() { i, i + 1 });
                                i++;
                            }
                            physicsIndices.RemoveAt(physicsIndices.Count - 1);
                            physicsIndices.Add(startIdx);
                        }
                    }

                    PhysicsVertices = physicsVertices.ToArray();
                    PhysicsIndices = physicsIndices.ToArray();
                }
            }
            else if (PickResult.ObjCell is ACE.Server.Physics.Common.EnvCell envCell)
            {
                var transform = envCell.Pos.ToXna();

                var physicsVertices = new List<VertexPositionColor>();
                var physicsIndices = new List<int>();

                var i = 0;

                foreach (var polygon in envCell.CellStructure.PhysicsPolygons.Values)
                {
                    var startIdx = i;

                    foreach (var v in polygon.Vertices)
                    {
                        physicsVertices.Add(new VertexPositionColor(Vector3.Transform(v.Origin.ToXna(), transform), Color.Orange));
                        physicsIndices.AddRange(new List<int>() { i, i + 1 });
                        i++;
                    }
                    physicsIndices.RemoveAt(physicsIndices.Count - 1);
                    physicsIndices.Add(startIdx);
                }

                PhysicsVertices = physicsVertices.ToArray();
                PhysicsIndices = physicsIndices.ToArray();
            }
        }

        private static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;
        
        private static Effect Effect => Render.Render.Effect;

        public static void DrawHitPoly()
        {
            if (HitVertices == null) return;

            if (PickResult.PhysicsObj?.IsDestroyed ?? false)
            {
                ClearSelection();
                return;
            }

            var rs = new RasterizerState();
            rs.CullMode = Microsoft.Xna.Framework.Graphics.CullMode.None;
            rs.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rs;

            if (SphereTransforms == null && CylinderTransforms == null && PhysicsVertices == null)
            {
                Effect.CurrentTechnique = Effect.Techniques["Picker"];

                foreach (var pass in Effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, HitVertices, 0, HitVertices.Length, HitIndices, 0, HitIndices.Length / 2);
                }
            }
            else
            {
                if (SphereTransforms != null)
                {
                    foreach (var sphereTransform in SphereTransforms)
                        SpherePrimitive.Draw(sphereTransform, Camera.ViewMatrix, Camera.ProjectionMatrix, Color.Orange);
                }

                if (CylinderTransforms != null)
                {
                    foreach (var cylinderTransform in CylinderTransforms)
                        CylinderPrimitive.Draw(cylinderTransform, Camera.ViewMatrix, Camera.ProjectionMatrix, Color.Orange);
                }

                if (PhysicsVertices != null)
                {
                    Effect.CurrentTechnique = Effect.Techniques["Picker"];

                    foreach (var pass in Effect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, PhysicsVertices, 0, PhysicsVertices.Length, PhysicsIndices, 0, PhysicsIndices.Length / 2);
                    }
                }
            }

            if (RenderLinks != null)
            {
                if (!RenderLinks.Head.WorldObject.PhysicsObj.IsDestroyed)
                    RenderLinks.Draw();
                else
                {
                    RenderLinks.Dispose();
                    RenderLinks = null;
                }
            }
        }

        public static RenderLinks RenderLinks { get; set; }

        public static void BuildLinks(WorldObject wo)
        {
            if (RenderLinks != null)
                RenderLinks.Dispose();
            
            RenderLinks = null;

            var node = new LinkNode(wo);
            node.AddParentChains();
            node.AddChildTrees();

            if (node.Parent != null || node.Children != null)
            {
                RenderLinks = new RenderLinks(node);
            }
        }

        public static void ClearSelection()
        {
            HitVertices = null;
            HitIndices = null;
            SphereTransforms = null;
            CylinderTransforms = null;
            PhysicsVertices = null;
            PhysicsIndices = null;

            if (RenderLinks != null)
            {
                RenderLinks.Dispose();
                RenderLinks = null;
            }
        }
    }
}
