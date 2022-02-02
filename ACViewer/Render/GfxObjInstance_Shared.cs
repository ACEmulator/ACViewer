using System.Collections.Generic;

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
        
        public Effect Effect_Clamp => Render.Effect_Clamp;

        public GfxObj GfxObj { get; set; }

        public Dictionary<TextureFormatChain, GfxObjInstance_TextureFormat> BaseFormats_Solid { get; set; }

        public Dictionary<TextureFormatChain, GfxObjInstance_TextureFormat> BaseFormats_Alpha { get; set; }

        public List<VertexPositionNormalTextures> Vertices { get; set; }

        public List<VertexInstance> Instances { get; set; }

        public VertexInstance[] Instances_ { get; set; }

        public VertexBuffer Shared_VB { get; set; }

        public VertexBuffer Instances_VB { get; set; }

        public VertexBufferBinding[] Bindings { get; set; }

        public GfxObjInstance_Shared(GfxObj gfxObj, Dictionary<TextureFormat, TextureAtlasChain> textureAtlasChains, Dictionary<uint, uint> textureChanges = null, PaletteChanges paletteChanges = null)
        {
            GfxObj = gfxObj;

            BuildStatic(gfxObj, textureAtlasChains, textureChanges, paletteChanges);

            Instances = new List<VertexInstance>();
        }

        public static readonly SurfaceType AlphaSurfaceTypes = SurfaceType.Base1ClipMap | SurfaceType.Translucent | SurfaceType.Alpha | SurfaceType.Additive;

        public void BuildStatic(GfxObj gfxObj, Dictionary<TextureFormat, TextureAtlasChain> textureAtlasChains, Dictionary<uint, uint> textureChanges = null, PaletteChanges paletteChanges = null)
        {
            BaseFormats_Solid = new Dictionary<TextureFormatChain, GfxObjInstance_TextureFormat>();

            BaseFormats_Alpha = new Dictionary<TextureFormatChain, GfxObjInstance_TextureFormat>();

            Vertices = new List<VertexPositionNormalTextures>();

            var vertexTable = new Dictionary<VertexPositionNormalTextures, short>();

            foreach (var poly in gfxObj.Polygons)
            {
                // get actual transformed texture -- cannot rely on poly.Texture original format
                var surfaceIdx = poly._polygon.PosSurface;
                var surfaceID = gfxObj._gfxObj.Surfaces[surfaceIdx];

                var textureId = TextureCache.GetSurfaceTextureID(surfaceID, textureChanges);
                var texture = TextureCache.Get(surfaceID, textureId, paletteChanges);
                
                var textureFormat = new TextureFormat(texture.Format, texture.Width, texture.Height, gfxObj.HasWrappingUVs);

                if (!textureAtlasChains.TryGetValue(textureFormat, out var textureAtlasChain))
                {
                    textureAtlasChain = new TextureAtlasChain(textureFormat);
                    textureAtlasChains.Add(textureFormat, textureAtlasChain);
                }

                var surface = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Surface>(surfaceID);

                var surfaceTextureId = TextureCache.GetSurfaceTextureID(surfaceID, textureChanges);

                var surfaceTexturePalette = new SurfaceTexturePalette(surfaceID, surfaceTextureId, paletteChanges);

                var atlasIdx = textureAtlasChain.GetAtlasIdx(surfaceTexturePalette);

                var textureAtlas = textureAtlasChain.TextureAtlases[atlasIdx];

                var baseFormats = (surface.Type & AlphaSurfaceTypes) == 0 ? BaseFormats_Solid : BaseFormats_Alpha;

                if (!baseFormats.TryGetValue(textureAtlas.TextureFormatChain, out var baseFormat))
                {
                    baseFormat = new GfxObjInstance_TextureFormat(textureAtlas);
                    baseFormats.Add(textureAtlas.TextureFormatChain, baseFormat);
                }

                var textureIdx = textureAtlas.Textures[surfaceTexturePalette];

                baseFormat.AddPolygon(poly, gfxObj.VertexArray, surfaceID, Vertices, vertexTable, textureIdx);
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
            Instances_ = Instances.ToArray();
            Instances_VB = new VertexBuffer(GraphicsDevice, typeof(VertexInstance), Instances.Count, BufferUsage.WriteOnly);
            Instances_VB.SetData(Instances_);
        }

        private void BuildBindings()
        {
            Bindings = new VertexBufferBinding[2];
            Bindings[0] = new VertexBufferBinding(Shared_VB);
            Bindings[1] = new VertexBufferBinding(Instances_VB, 0, 1);
        }

        private bool isDirty { get; set; }
        
        public void UpdateInstance(int idx, Vector3 position, Quaternion orientation, Vector3 scale)
        {
            Instances_[idx].Position = position;
            Instances_[idx].Orientation = new Vector4(orientation.X, orientation.Y, orientation.Z, orientation.W);
            Instances_[idx].Scale = scale;
            isDirty = true;
        }

        public void Draw()
        {
            if (isDirty)
            {
                Instances_VB.SetData(Instances_);
                isDirty = false;
            }
            
            GraphicsDevice.SetVertexBuffers(Bindings);

            Effect.CurrentTechnique = Effect.Techniques["TexturedInstance"];
            Effect_Clamp.CurrentTechnique = Effect_Clamp.Techniques["TexturedInstance"];

            foreach (var baseFormat in BaseFormats_Solid.Values)
                baseFormat.Draw(Instances.Count);

            if (Buffer.drawAlpha)
            {
                Effect.CurrentTechnique = Effect.Techniques["TexturedInstanceAlpha"];
                Effect_Clamp.CurrentTechnique = Effect_Clamp.Techniques["TexturedInstanceAlpha"];
            }

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
