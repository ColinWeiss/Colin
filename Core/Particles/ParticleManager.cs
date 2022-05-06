using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colin.Core.Particles
{
    /// <summary>
    /// 粒子管理器.
    /// </summary>
    public class ParticleManager : ObjectPool<Particle>
    {
        /// <summary>
        /// 根据粒子类型创建一个粒子.
        /// </summary>
        /// <typeparam name="T">粒子类型.</typeparam>
        /// <param name="pos">粒子生成的位置.</param>
        /// <param name="vel">给予粒子的初始速度.</param>
        /// <param name="activeTime">设置粒子的活跃时间.</param>
        /// <param name="scale">粒子的初始大小.</param>
        /// <param name="color">粒子的颜色.</param>
        public void CreateParticle<T>( Vector2 pos , Vector2 vel , float activeTime , float scale , Color color ) where T : Particle , new( )
        {
            for ( int count = 0; count < Objects.Length ; count++ )
            {
                if( Objects[count].Empty )
                {
                    Objects[ count ] = new Particle
                    {
                        Position = pos,
                        Velocity = vel,
                        ActiveTime = activeTime,
                        Scale = scale,
                        ElementColor = color
                    };
                    Objects[ count ].OnActive( );
                    ActiveObject( count );
                    break;
                }
            }
        }

        /// <summary>
        /// 根据粒子类型创建一个粒子.
        /// </summary>
        /// <typeparam name="T">粒子类型.</typeparam>
        /// <param name="pos">粒子生成的位置.</param>
        /// <param name="vel">给予粒子的初始速度.</param>
        /// <param name="activeTime">设置粒子的活跃时间.</param>
        /// <param name="scale">粒子的初始大小.</param>
        public void CreateParticle<T>( Vector2 pos, Vector2 vel, float activeTime, float scale ) where T : Particle, new()
        {
            for ( int count = 0; count < Objects.Length; count++ )
            {
                if ( Objects[ count ].Empty )
                {
                    Objects[ count ] = new Particle
                    {
                        Position = pos,
                        Velocity = vel,
                        ActiveTime = activeTime,
                        Scale = scale,
                        ElementColor = Color.White
                    };
                    Objects[ count ].OnActive( );
                    ActiveObject( count );
                    break;
                }
            }
        }

        /// <summary>
        /// 根据粒子类型创建一个粒子.
        /// </summary>
        /// <typeparam name="T">粒子类型.</typeparam>
        /// <param name="pos">粒子生成的位置.</param>
        /// <param name="vel">给予粒子的初始速度.</param>
        /// <param name="activeTime">设置粒子的活跃时间.</param>
        public void CreateParticle<T>( Vector2 pos, Vector2 vel, float activeTime ) where T : Particle, new()
        {
            for ( int count = 0; count < Objects.Length; count++ )
            {
                if ( Objects[ count ].Empty )
                {
                    Objects[ count ] = new Particle
                    {
                        Position = pos,
                        Velocity = vel,
                        ActiveTime = activeTime,
                        Scale = 1f,
                        ElementColor = Color.White
                    };
                    Objects[ count ].OnActive( );
                    ActiveObject( count );
                    break;
                }
            }
        }

        /// <summary>
        /// 根据粒子类型创建一个粒子.
        /// </summary>
        /// <typeparam name="T">粒子类型.</typeparam>
        /// <param name="pos">粒子生成的位置.</param>
        /// <param name="vel">给予粒子的初始速度.</param>
        public void CreateParticle<T>( Vector2 pos, Vector2 vel ) where T : Particle, new()
        {
            for ( int count = 0; count < Objects.Length; count++ )
            {
                if ( Objects[ count ].Empty )
                {
                    Objects[ count ] = new Particle
                    {
                        Position = pos,
                        Velocity = vel,
                        ActiveTime = Engine.Instance.TargetFrame,
                        Scale = 1f,
                        ElementColor = Color.White
                    };
                    Objects[ count ].OnActive( );
                    ActiveObject( count );
                    break;
                }
            }
        }

        /// <summary>
        /// 根据粒子类型创建一个粒子.
        /// </summary>
        /// <typeparam name="T">粒子类型.</typeparam>
        /// <param name="pos">粒子生成的位置.</param>
        public void CreateParticle<T>( Vector2 pos ) where T : Particle, new()
        {
            for ( int count = 0; count < Objects.Length; count++ )
            {
                if ( Objects[ count ].Empty )
                {
                    Objects[ count ] = new Particle
                    {
                        Position = pos,
                        Velocity = Vector2.Zero,
                        ActiveTime = Engine.Instance.TargetFrame,
                        Scale = 1f,
                        ElementColor = Color.White
                    };
                    Objects[ count ].OnActive( );
                    ActiveObject( count );
                    break;
                }
            }
        }

        public ParticleManager( int poolSize ) : base( poolSize ) { }
    }
}