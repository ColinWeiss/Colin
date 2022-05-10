using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Colin;
using Colin.Extensions;

namespace Colin.Graphics
{
    /// <summary>
    /// 顶点批处理器: PrimitiveBatch 是一个自动处理高效渲染的类.
    /// <para>类似于 SpriteBatch.</para>
    /// <para>PrimitiveBatch 可以渲染线、点以及屏幕上的三角形.</para>
    /// </summary>
    public class PrimitiveBatch : IDisposable
    {
        private bool _hasBegun = false;

        private bool _isDisposed = false;

        /// <summary>
        /// 顶点缓冲区的位置索引.
        /// </summary>
        private int _positionInBuffer = 0;

        /// <summary>
        /// 获取当前顶点排序方式中, 每个基本体所使用的顶点数量.
        /// </summary>
        public int NumVertsPerPrimitive
        {
            get
            {
                int result = 0;
                switch ( PrimitiveType )
                {
                    case PrimitiveType.LineList:
                        result = 2;
                        break;
                    case PrimitiveType.LineStrip:
                        result = 1;
                        break;
                    case PrimitiveType.TriangleList:
                        result = 3;
                        break;
                    case PrimitiveType.TriangleStrip:
                        result = 2;
                        break;
                }
                return result;

            }
        }

        /// <summary>
        /// 包含基本渲染效果.
        /// </summary>
        public BasicEffect BasicEffect;

        /// <summary>
        /// 指示该处理器如何对顶点数据进行排序.
        /// </summary>
        public PrimitiveType PrimitiveType { get; private set; }

        /// <summary>
        /// 控制顶点缓冲区的大小.
        /// <para>更大的缓冲区将减少刷新次数, 可以提高性能.</para>
        /// <para>然而缓冲区过大可能造成内存浪费.</para>
        /// </summary>
        public int DefaultBufferSize { get; private set; }

        /// <summary>
        /// 调用 <seealso cref="AddVertex(Vector2,Color)"/> 及其重载函数进行顶点块的填充.
        /// <para>[!] Flush将使用此数组, 并将确定要从中绘制的顶点.</para>
        /// </summary>
        public VertexPositionColorTexture[ ] Vertices { get; private set; }

        /// <summary>
        /// 实例化一个包含描述顶点坐标、颜色的顶点信息的顶点批处理器.
        /// </summary>
        /// <param name="bufferSize">顶点缓冲区的大小.</param>
        public PrimitiveBatch( int bufferSize )
        {
            DefaultBufferSize = bufferSize;
            Vertices = new VertexPositionColorTexture[ DefaultBufferSize ];
            BasicEffect = new BasicEffect( Engine.Instance.GraphicsDevice );
            BasicEffect.VertexColorEnabled = true;
            BasicEffect.Projection = Matrix.CreateOrthographicOffCenter
                ( 0, HardwareInfo.GameViewWidth,
                 HardwareInfo.GameViewHeight, 0,
                0, 1 );
        }

        /// <summary>
        /// 实例化一个包含描述纹理、纹理颜色、顶点坐标、颜色的顶点信息的顶点批处理器.
        /// </summary>
        /// <param name="bufferSize">顶点缓冲区的大小.</param>
        /// <param name="texture">纹理.</param>
        public PrimitiveBatch( int bufferSize, Texture2D texture )
        {
            DefaultBufferSize = bufferSize;
            Vertices = new VertexPositionColorTexture[ DefaultBufferSize ];
            BasicEffect = new BasicEffect( Engine.Instance.GraphicsDevice );
            BasicEffect.VertexColorEnabled = true;
            BasicEffect.TextureEnabled = true;
            BasicEffect.Texture = texture;
            BasicEffect.Projection = Matrix.CreateOrthographicOffCenter
                ( 0, HardwareInfo.GameViewWidth,
                 HardwareInfo.GameViewHeight, 0,
                0, 1 );
        }

        /// <summary>
        /// 开始顶点的添加, 并设置顶点数据的排序方式.
        /// </summary>
        /// <param name="primitiveType">顶点数据排序方式.</param>
        public void Begin( PrimitiveType primitiveType )
        {
            if ( _hasBegun ) throw new InvalidOperationException( "End must be called before Begin can be called again." );
            PrimitiveType = primitiveType;
            BasicEffect.CurrentTechnique.Passes[ 0 ].Apply( );
            _hasBegun = true;
            HardwareInfo.Graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
        }

        /// <summary>
        /// 将具有指定数据的顶点添加至批处理器中.
        /// <para>[!] 该方法不具有UV纹理坐标参数, 故调用该方法时将自动将 <see cref="BasicEffect.TextureEnabled"/> 设置为 <see href="false"/>.</para>
        /// <para>[!] 若要设置顶点所映射的UV纹理坐标, 请查询该方法同名的重载函数 <seealso cref="AddVertex(Vector2,Vector2,Color)"/>.</para>
        /// </summary>
        /// <param name="vertexPosition">顶点位置.</param>
        /// <param name="color">顶点颜色.</param>
        public void AddVertex( Vector2 vertexPosition, Color color )
        {
            BasicEffect.TextureEnabled = false;
            AddVertex( Vector2.Zero, vertexPosition, color );
        }

        /// <summary>
        /// 将具有指定数据的顶点添加至批处理器中.
        /// </summary>
        /// <param name="textureCoordinate">顶点映射的UV纹理坐标.</param>
        /// <param name="vertexPosition">顶点位置.</param>
        /// <param name="color">顶点颜色.</param>
        public void AddVertex( Vector2 textureCoordinate, Vector2 vertexPosition, Color color )
        {
            if ( !_hasBegun )
                throw new InvalidOperationException( "Begin must be called before AddVertex can be called." );
            bool newPrimitive = _positionInBuffer % NumVertsPerPrimitive == 0;
            if ( newPrimitive && _positionInBuffer + NumVertsPerPrimitive >= Vertices.Length )
                Flush( );
            Vertices[ _positionInBuffer ].TextureCoordinate = textureCoordinate;
            Vertices[ _positionInBuffer ].Position = new Vector3( vertexPosition, 0 );
            Vertices[ _positionInBuffer ].Color = color;
            _positionInBuffer++;
        }

        /// <summary>
        /// 结束顶点的添加, 并进行基本体的绘制.
        /// </summary>
        public void End( )
        {
            if ( !_hasBegun )
                throw new InvalidOperationException( "必须先调用Begin, 然后才能调用End." );
            Flush( );
            _hasBegun = false;
        }

        /// <summary>
        /// 进行基本体的绘制.
        /// </summary>
        protected void Flush( )
        {
            if ( !_hasBegun )
                throw new InvalidOperationException( "必须先调用Begin, 然后才能调用Flush." );
            if ( _positionInBuffer == 0 )
                return;
            int primitiveCount = _positionInBuffer / NumVertsPerPrimitive;
            Engine.Instance.GraphicsDevice.DrawUserPrimitives( PrimitiveType, Vertices, 0, primitiveCount );
            _positionInBuffer = 0;
        }

        public void Dispose( )
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        protected virtual void Dispose( bool disposing )
        {
            if ( disposing && !_isDisposed )
            {
                if ( BasicEffect != null )
                    BasicEffect.Dispose( );
                _isDisposed = true;
            }
        }

    }
}