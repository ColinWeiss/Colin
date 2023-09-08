namespace Colin.Core
{
    /// <summary>
    /// 控制台输出信息类型.
    /// </summary>
    public enum ConsoleTextType : int
    {
        /// <summary>
        /// 普通信息.
        /// </summary>
        Normal,
        /// <summary>
        /// 游戏信息.
        /// </summary>
        Game,
        /// <summary>
        /// 警告信息.
        /// </summary>
        Warning,
        /// <summary>
        /// 提示信息.
        /// </summary>
        Remind,
        /// <summary>
        /// 错误信息.
        /// </summary>
        Error
    }

    /// <summary>
    /// 你看它名字你就知道这是什么了.
    /// </summary>
    public class EngineConsole
    {
        /// <summary>  
        /// 向控制台输出信息.
        /// </summary>  
        /// <param name="informationType">信息类型.</param>  
        /// <param name="output">输出内容.</param>  
        public static void WriteLine( ConsoleTextType informationType, object output )
        {
            WriteLine( informationType, output.ToString() );
        }

        /// <summary>  
        /// 向控制台输出信息.
        /// </summary>  
        /// <param name="informationType">信息类型.</param>  
        /// <param name="output">输出内容.</param>  
        public static void WriteLine( ConsoleTextType informationType, string output )
        {
#if WINDOWS
            Console.ForegroundColor = GetConsoleColor( informationType );
#endif
            string NowTime = string.Concat( "[", DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss" ), "]" );
            string outPutText = string.Concat( NowTime, "\n =>", "[", EngineInfo.EngineName, "] ", output );
            Console.WriteLine( outPutText );
        }

        /// <summary>  
        /// 根据输出文本选择控制台文字颜色.
        /// </summary>  
        /// <param name="informationType">信息类型.</param>  
        /// <returns></returns>  
        private static ConsoleColor GetConsoleColor( ConsoleTextType informationType )
        {
            switch(informationType)
            {
                case ConsoleTextType.Normal:
                    return ConsoleColor.DarkGray;
                case ConsoleTextType.Remind:
                    return ConsoleColor.Yellow;
                case ConsoleTextType.Game:
                    return ConsoleColor.White;
                case ConsoleTextType.Warning:
                    return ConsoleColor.Yellow;
                case ConsoleTextType.Error:
                    return ConsoleColor.Red;
            }
            return ConsoleColor.White;
        }

    }
}
