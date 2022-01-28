﻿using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using ACE.DatLoader;
using ACE.Entity.Enum;

using ACViewer.Model;

namespace ACViewer.Render
{
    public class GfxObjInstance_Shared
    {
        public GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public Effect Effect => Render.Effect;

        public GfxObj GfxObj { get; set; }

        public Dictionary<TextureFormat, GfxObjInstance_TextureFormat> BaseFormats_Solid { get; set; }

        public Dictionary<TextureFormat, GfxObjInstance_TextureFormat> BaseFormats_Alpha { get; set; }

        public List<VertexPositionNormalTextures> Vertices { get; set; }

        public List<VertexInstance> Instances { get; set; }

        public VertexBuffer Shared_VB { get; set; }

        public VertexBuffer Instances_VB { get; set; }

        public VertexBufferBinding[] Bindings { get; set; }

        public GfxObjInstance_Shared(GfxObj gfxObj, Dictionary<TextureFormat, TextureAtlas> textureAtlases)
        {
            GfxObj = gfxObj;

            BuildStatic(gfxObj, textureAtlases);

            Instances = new List<VertexInstance>();
        }

        public static readonly SurfaceType AlphaSurfaceTypes = SurfaceType.Base1ClipMap | SurfaceType.Translucent | SurfaceType.Alpha | SurfaceType.Additive;

        public void BuildStatic(GfxObj gfxObj, Dictionary<TextureFormat, TextureAtlas> textureAtlases)
        {
            BaseFormats_Solid = new Dictionary<TextureFormat, GfxObjInstance_TextureFormat>();

            BaseFormats_Alpha = new Dictionary<TextureFormat, GfxObjInstance_TextureFormat>();

            Vertices = new List<VertexPositionNormalTextures>();

            var vertexTable = new Dictionary<VertexPositionNormalTextures, short>();

            foreach (var poly in gfxObj.Polygons)
            {
                var textureFormat = new TextureFormat(poly.Texture.Format, poly.Texture.Width, poly.Texture.Height, gfxObj.HasWrappingUVs);

                if (!textureAtlases.TryGetValue(textureFormat, out var textureAtlas))
                {
                    textureAtlas = new TextureAtlas(textureFormat);
                    textureAtlases.Add(textureFormat, textureAtlas);
                }

                var surfaceIdx = poly._polygon.PosSurface;
                var surfaceID = gfxObj._gfxObj.Surfaces[surfaceIdx];

                var surface = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Surface>(surfaceID);

                var baseFormats = (surface.Type & AlphaSurfaceTypes) == 0 ? BaseFormats_Solid : BaseFormats_Alpha;
                
                if (!baseFormats.TryGetValue(textureFormat, out var baseFormat))
                {
                    baseFormat = new GfxObjInstance_TextureFormat(textureAtlas);
                    baseFormats.Add(textureFormat, baseFormat);
                }

                baseFormat.AddPolygon(poly, gfxObj.VertexArray, surfaceID, Vertices, vertexTable);
            }
        }

        public void AddInstance(Vector3 position, Quaternion orientation, Vector3 scale)
        {
            Instances.Add(new VertexInstance(position, orientation, scale));
        }

        public void OnCompleted()
        {
            BuildBuffers();

            BuildBindings();
        }

        private void BuildBuffers()
        {
            // build base buffers

            // build shared vertex buffer
            Shared_VB = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTextures), Vertices.Count, BufferUsage.WriteOnly);
            Shared_VB.SetData(Vertices.ToArray());

            // build index buffers per-texture format
            foreach (var baseFormat in BaseFormats_Solid.Values)
                baseFormat.BuildBuffer();

            foreach (var baseFormat in BaseFormats_Alpha.Values)
                baseFormat.BuildBuffer();

            //
            // build instances
            Instances_VB = new VertexBuffer(GraphicsDevice, typeof(VertexInstance), Instances.Count, BufferUsage.WriteOnly);
            Instances_VB.SetData(Instances.ToArray());
        }

        private void BuildBindings()
        {
            Bindings = new VertexBufferBinding[2];
            Bindings[0] = new VertexBufferBinding(Shared_VB);
            Bindings[1] = new VertexBufferBinding(Instances_VB, 0, 1);
        }

        public void Draw()
        {
            GraphicsDevice.SetVertexBuffers(Bindings);

            Effect.CurrentTechnique = Effect.Techniques["TexturedInstance"];

            foreach (var baseFormat in BaseFormats_Solid.Values)
                baseFormat.Draw(Instances.Count);

            if (Buffer.drawAlpha)
                Effect.CurrentTechnique = Effect.Techniques["TexturedInstanceAlpha"];

            foreach (var baseFormat in BaseFormats_Alpha.Values)
                baseFormat.Draw(Instances.Count);
        }

        public void Dispose()
        {
            if (Shared_VB != null)
                Shared_VB.Dispose();

            if (Instances_VB != null)
                Instances_VB.Dispose();

            foreach (var baseFormat in BaseFormats_Solid.Values)
                baseFormat.Dispose();

            foreach (var baseFormat in BaseFormats_Alpha.Values)
                baseFormat.Dispose();
        }
    }
}
