using System;
using System.Collections.Generic;
using System.Numerics;

using Microsoft.Xna.Framework.Graphics;

using ACE.Server.Physics;

namespace ACViewer.Render
{
    public class ParticleBatchDraw
    {
        public static GraphicsDevice GraphicsDevice => GameView.Instance.GraphicsDevice;

        public static Effect Effect => Render.Effect;

        public EffectParameters EffectParameters { get; set; }

        // static buffer = billboard
        
        // instance buffer
        public List<ParticleDeclaration> Instances { get; set; }
        public ParticleDeclaration[] Instances_ { get; set; }

        public VertexBuffer InstanceBuffer { get; set; }

        public ParticleTextureFormat TextureFormat { get; set; }
        public Dictionary<uint, int> TextureIndex { get; set; }

        public VertexBufferBinding[] Bindings { get; set; }

        public ParticleBatchDraw(ParticleTextureFormat textureFormat)
        {
            Init();

            TextureFormat = textureFormat;
        }


        public void Init()
        {
            EffectParameters = new EffectParameters();

            Instances = new List<ParticleDeclaration>();
            TextureIndex = new Dictionary<uint, int>();
        }

        /// <summary>
        /// Returns the starting index into Instances
        /// </summary>
        public int AddParticleSlot(uint textureID, Vector2 dims)
        {
            // since we are looping over this, change so dict lookup only happens once
            if (!TextureIndex.TryGetValue(textureID, out var textureIdx))
            {
                textureIdx = (byte)TextureIndex.Count;
                TextureIndex.Add(textureID, textureIdx);
            }

            var startIdx = Instances.Count;
            
            var instance = new ParticleDeclaration(textureIdx, dims);
            Instances.Add(instance);

            return startIdx;
        }

        public void OnCompleted()
        {
            BuildTextures();
            BuildInstanceBuffer();
            BuildBindings();
        }

        public void BuildTextures()
        {
            var bytesPerPixel = TextureFormat.GetBytesPerPixel();

            var textures = new Texture2D(GraphicsDevice, TextureFormat.Width, TextureFormat.Height, false, TextureFormat.SurfaceFormat, TextureIndex.Count);

            foreach (var kvp in TextureIndex)
            {
                var surface = kvp.Key;
                var idx = kvp.Value;
                var texture = TextureCache.Get(surface);
                //Console.WriteLine($"Texture format: {texture.Format}");
                var data = new byte[(int)(texture.Width * texture.Height * bytesPerPixel)];
                texture.GetData(data);
                textures.SetData(0, idx, null, data, 0, data.Length);
            }

            EffectParameters.Texture = textures;
        }

        public void BuildInstanceBuffer()
        {
            Instances_ = Instances.ToArray();

            // DynamicVertexBuffer?
            InstanceBuffer = new VertexBuffer(GraphicsDevice, typeof(ParticleDeclaration), Instances.Count, BufferUsage.WriteOnly);
            InstanceBuffer.SetData(Instances_);
        }

        public void BuildBindings()
        {
            Bindings = new VertexBufferBinding[2];
            Bindings[0] = new VertexBufferBinding(Billboard.VertexBuffer);
            Bindings[1] = new VertexBufferBinding(InstanceBuffer, 0, 1);
        }

        public void Draw()
        {
            GraphicsDevice.SetVertexBuffers(Bindings);

            Effect.Parameters["xTextures"].SetValue(EffectParameters.Texture);

            if (TextureFormat.IsAdditive)
                GraphicsDevice.BlendState = BlendState.Additive;
            else
                GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            
            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleStrip, 0, 0, 2, Instances.Count);
            }
        }

        public void SpawnParticle(ParticleEmitter emitter, int particleIdx)
        {
            UpdateParticle(emitter, particleIdx);
        }
        
        public void UpdateParticle(ParticleEmitter emitter, int particleIdx)
        {
            var instanceIdx = emitter.BatchIdx + particleIdx;

            var part = emitter.Parts[particleIdx];

            Instances_[instanceIdx].Position = part.Pos.GetWorldPos().ToNumerics();
            Instances_[instanceIdx].ScaleOpacityActive = new Vector3(part.GfxObjScale.X, 1.0f - part.CurTranslucency, 1);
        }

        public void DestroyParticle(ParticleEmitter emitter, int particleIdx)
        {
            var instanceIdx = emitter.BatchIdx + particleIdx;

            Instances_[instanceIdx].ScaleOpacityActive = Vector3.Zero;
        }

        public void UpdateInstanceBuffer()
        {
            // dirty flag?
            InstanceBuffer.SetData(Instances_);
        }

        public void Dispose()
        {
            EffectParameters.Dispose();
            InstanceBuffer.Dispose();
        }
    }
}
