namespace Colin.Core.Modulars.UserInterfaces.Prefabs
{
    public class InputTextBox : Division
    {
        public InputTextBox( string name, int limit = 16 ) : base( name )
        {
            Limit = limit;
            Label = new Label( "Text" );
        }

        public Label Label;

        public string Text;

        public string DisplayText;

        /// <summary>
        /// 获取该文本框限制的字符数量.
        /// </summary>
        public readonly int Limit;

        public bool Editing = false;

        public Rectangle InputRect;

        public int CursorPosition;

        public bool AllowBreaks = false;

        public Keys[] FunctionKeys = new Keys[]
        {
            Keys.Back,
            Keys.LeftShift,
            Keys.RightShift,
            Keys.LeftAlt,
            Keys.RightAlt
        };

        public override void OnInit()
        {
            Text = "";
            Layout.ScissorEnable = true;
            Label.Interact.IsInteractive = false;
            Register( Label );

            Events.GetFocus += ( ) =>
            {
                EngineInfo.IMEHandler.StartTextComposition();
                EngineInfo.IMEHandler.SetTextInputRect( ref InputRect );
            };
            Events.LoseFocus += (  ) =>
            {
                EngineInfo.IMEHandler.StopTextComposition();
                Label.SetText( Text );
            };
            EngineInfo.IMEHandler.TextInput += IMEHandler_TextInput;
            base.OnInit();
        }

        private void IMEHandler_TextInput( object sender, MonoGame.IMEHelper.TextInputEventArgs e )
        {
            if(Editing)
            {
                if(e.Key == Keys.Back && Text.Length > 0)
                {
                    Text = Text.Remove( CursorPosition - 1, 1 );
                    CursorPosition--;
                }
                else if(e.Key == Keys.Enter && AllowBreaks)
                {
                    Text += "\n";
                    CursorPosition++;
                }
                else if(CursorPosition == Text.Length && !FunctionKeys.Contains( e.Key ))
                {
                    Text += e.Character;
                    CursorPosition++;
                }
                else if(!FunctionKeys.Contains( e.Key ))
                {
                    Text = Text.Insert( CursorPosition, e.Character.ToString() );
                    CursorPosition += e.Character.ToString().Length;
                }
            }
        }

        public override void OnUpdate( GameTime time )
        {
            Editing = Interface.Focus == this;
            InputRect = Layout.TotalHitBox;
            InputRect.Y += 16;
            InputRect.X += 16;
            if(Text != string.Empty)
            {
                if(KeyboardResponder.IsKeyClickAfter( Keys.Left ))
                    CursorPosition--;
                if(KeyboardResponder.IsKeyClickAfter( Keys.Right ))
                    CursorPosition++;
                if(KeyboardResponder.IsKeyClickAfter( Keys.Up ))
                    CursorPosition = Math.Clamp( CursorPosition, 0, Text.Length );
            }
            else
                CursorPosition = 0;

            DisplayText = Text.Insert( CursorPosition, "|" );
            if(Editing)
                Label.SetText( DisplayText );
            else
                Label.SetText( Text );

            base.OnUpdate( time );
        }
    }
}