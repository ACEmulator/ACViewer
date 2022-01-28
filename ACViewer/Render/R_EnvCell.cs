using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.Server.Physics.Common;

namespace ACViewer.Render
{
    public class R_EnvCell
    {
        // - an envcell has 1 environment
        // - an environment has multiple cellstructs (unique?)
        // - a cellstruct has a vertexarray, and a list of polygons (a list of indices)

        // currently drawing on a per-poly basis (ideal for textures?)
        // 1 draw cell per state change? (ie, 1 draw call per xWorld set)
        // - draw cellstruct (combine polygon list into 1 index buffer)
        // - draw environment (combine cellstructs into 1 vertex / index buffer. this requires offsets?)
        // - instancing?

        public static Effect Effect => Render.Effect;

        public EnvCell EnvCell { get; set; }

        public R_Environment Environment { get; set; }

        public List<R_PhysicsObj> StaticObjs { get; set; }

        public Matrix WorldTransform { get; set; }

        public List<Texture2D> Textures { get; set; }

        public R_EnvCell(EnvCell envCell)
        {
            EnvCell = envCell;

            Environment = R_EnvironmentCache.Get(envCell.EnvironmentID);

            BuildStaticObjs();

            BuildWorldTransform();

            BuildTextures();
        }

        public void BuildWorldTransform()
        {
            WorldTransform = EnvCell.Pos.ToXna();
        }

        public void BuildStaticObjs()
        {
            StaticObjs = new List<R_PhysicsObj>();

            if (EnvCell.StaticObjects == null) return;

            foreach (var staticObj in EnvCell.StaticObjects)
                StaticObjs.Add(new R_PhysicsObj(staticObj));
        }

        public void BuildTextures()
        {
            Textures = new List<Texture2D>();

            foreach (var surfaceID in EnvCell._envCell.Surfaces)
                Textures.Add(TextureCache.Get(surfaceID));
        }

        public void Draw(Matrix landblock)
        {
            DrawEnv();
            DrawStaticObjs(landblock);
        }

        public void DrawEnv()
        {
            Effect.Parameters["xWorld"].SetValue(WorldTransform);

            Environment.Draw(EnvCell.CellStructureID, Textures);
        }

        public void DrawStaticObjs(Matrix landblock)
        {
            foreach (var staticObj in StaticObjs)
                staticObj.Draw(landblock);
        }
    }
}
