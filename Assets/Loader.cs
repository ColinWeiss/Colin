using System.Reflection;

namespace Colin.Assets
{
    /// <summary>
    /// 基础加载器.
    /// </summary>
    public static class Loader
    {
        /// <summary>
        /// 获取加载是否完成的值.
        /// </summary>
        public static bool Loaded { get; private set; } = false;

        /// <summary>
        /// 对程序内所有的 <seealso cref="ILoadable"/> 对象执行加载操作.
        /// </summary>
        public static async void LoadAssets( )
        {
            foreach ( Type type in Assembly.GetEntryAssembly( ).GetTypes( ) )
            {
                if ( !type.IsAbstract && type.GetInterfaces( ).Contains( typeof( ILoadable ) ) )
                {
                    var instance = (ILoadable)Activator.CreateInstance( type );
                    await Task.Run( instance.LoadContents );
                }
            }
            Loaded = true;
        }
    }
}