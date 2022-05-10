using Microsoft.Xna.Framework.Graphics;

namespace Colin.Graphics
{
    /// <summary>
    /// 表示一个屏幕着色器.
    /// </summary>
    public class ScreenShader : ShaderData
    {
        /// <summary>
        /// 指示该屏幕着色器是否启用.
        /// </summary>
        public bool Enable = false;

        /// <summary>
        /// 用于标识该屏幕着色器的字符串.
        /// </summary>
        public string ScreenShaderName;

        public ScreenShader( Effect effect, string screenShaderName ) : base( effect )
        {
            ScreenShaderName = screenShaderName;
        }
    }
}