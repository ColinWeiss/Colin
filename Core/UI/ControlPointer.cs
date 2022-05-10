namespace Colin.Core.UI
{
    /// <summary>
    /// 指示当前操作指向的控件.
    /// </summary>
    public class ControlPointer
    {
        /// <summary>
        /// 指针当前位于 <seealso cref="ControlOperator.GetControls()"/> 的位置.
        /// </summary>
        public int Count { get; private set; } = 0;

        /// <summary>
        /// 该控件指针所绑定的控件执行器.
        /// </summary>
        public ControlOperator? Operator { get; private set; }

        /// <summary>
        /// 当前指向的控件.
        /// </summary>
        public Control? Control { get; private set; }

        /// <summary>
        /// 将指针移向上一个控件.
        /// </summary>
        public void LastControl( )
        {
            Count--;
            if ( Count < 0 )
                Count = Operator.GetActiveControls( ).Count - 1;
            Control = Operator.GetActiveControls( )[ Count ];
            if ( !Control.CanGetForPointer )
                LastControl( );
        }

        /// <summary>
        /// 将指针移向下一个控件.
        /// </summary>
        public void NextControl( )
        {
            Count++;
            if ( Count > Operator.GetActiveControls( ).Count - 1 )
                Count = 0;
            Control = Operator.GetActiveControls( )[ Count ];
            if ( !Control.CanGetForPointer )
                NextControl( );
        }

        public ControlPointer( ControlOperator controlOperator )
        {
            Operator = controlOperator;
        }
    }
}