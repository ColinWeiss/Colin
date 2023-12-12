using Colin.Core.Resources;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Colin.Core.Modulars.Particles
{
    public class Particle : ISceneModule, IRenderableISceneModule
    {
        public RenderTarget2D ModuleRt { get; set; }

        public bool Visible { get; set; }

        public bool FinalPresentation { get; set; }

        public Scene Scene { get; set; }

        public bool Enable { get; set; }

        public void DoInitialize()
        {

        }
        public void Start()
        {
        }
        public void DoUpdate(GameTime time)
        {
        }

        private bool RenderInitialized = false;
        public void DoRender(SpriteBatch batch)
        {
            ParticleInstanceBasic particleInstance;
            if (RenderInitialized is false)
            {
                for (int count = 0; count < CodeResources<ParticleInstanceBasic>.Resources.Values.Count; count++)
                {
                    particleInstance = CodeResources<ParticleInstanceBasic>.Resources.Values.ElementAt(count);
                    particleInstance.DoInitialize(EngineInfo.Graphics.GraphicsDevice);
                }
                RenderInitialized = true;
            }
            for (int count = 0; count < CodeResources<ParticleInstanceBasic>.Resources.Values.Count; count++)
            {
                particleInstance = CodeResources<ParticleInstanceBasic>.Resources.Values.ElementAt(count);
                particleInstance.DoCompute(EngineInfo.Graphics.GraphicsDevice);
            }
            for (int count = 0; count < CodeResources<ParticleInstanceBasic>.Resources.Values.Count; count++)
            {
                particleInstance = CodeResources<ParticleInstanceBasic>.Resources.Values.ElementAt(count);
                particleInstance.DoRender(EngineInfo.Graphics.GraphicsDevice);
            }
        }
        public void Dispose()
        {
        }
    }
}
