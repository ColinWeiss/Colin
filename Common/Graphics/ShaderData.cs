using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Colin.Common.Graphics
{
    /// <summary>
    /// 代表一个着色器数据.
    /// </summary>
    public class ShaderData
    {
        /// <summary>
        /// 着色器.
        /// </summary>
        public Effect? Effect { get; private set; }

        /// <summary>
        /// 用于内部优化的bool, 该值用于判断该着色器是否已经进入管理器.
        /// </summary>
        internal bool ForInternalOptimizationBoolen;

        /// <summary>
        /// 重载该函数以进行着色器资源的加载、着色器数据字段/属性的初始化.
        /// </summary>
        public virtual void LoadContent( )
        {

        }

        public virtual void UpdateShader( GameTime gameTime )
        {

        }

        public void ApplyPass( string passName ) => Effect.CurrentTechnique.Passes[passName].Apply( );

        public ShaderData( Effect effect )
        {
            Effect = effect;
            ForInternalOptimizationBoolen = false;
        }
    }
}