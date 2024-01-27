using Colin.Core.Events;

namespace Colin.Core.Modulars.Ecses.Components
{
    /// <summary>
    /// 利用该组件获取输入.
    /// </summary>
    public class EcsComInput : EcsComScript
    {
        public bool ControlLeft;

        public bool ControlRight;

        public bool ControlUp;

        public bool ControlDown;

        public bool ControlJump;

        public bool ControlKeepJump;

        public bool OnControllMove => ControlLeft || ControlRight;

        public Vector2 MousePosition;

        public override void DoInitialize()
        {
            Ecs.KeysEvent.ClickBefore += KeysEvent_ClickBefore;
            Ecs.KeysEvent.Down += KeysEvent_Down;
            base.DoInitialize();
        }
        private void KeysEvent_ClickBefore(object sender, KeyEventArgs e)
        {
            if (KeyClickBefore(Keys.Space, e))
                ControlJump = true;
        }
        private void KeysEvent_Down(object sender, KeyEventArgs e)
        {
            if (KeyDown(Keys.A, e))
                ControlLeft = true;
            if (KeyDown(Keys.D, e))
                ControlRight = true;
            if (KeyDown(Keys.W, e))
                ControlUp = true;
            if (KeyDown(Keys.S, e))
                ControlDown = true;
            if (KeyDown(Keys.Space, e))
                ControlKeepJump = true;
        }
        private bool KeyClickBefore(Keys key, KeyEventArgs e)
        {
            if (e.ClickBefore && e.Key == key)
            {
                e.Captured = true;
                return true;
            }
            else
                return false;
        }
        private bool KeyDown(Keys key, KeyEventArgs e)
        {
            if (e.Down && e.Key == key)
            {
                e.Captured = true;
                return true;
            }
            else
                return false;
        }

        public override void DoUpdate()
        {
            if (KeyboardResponder.IsKeyUp(Keys.A))
                ControlLeft = false;
            if (KeyboardResponder.IsKeyUp(Keys.D))
                ControlRight = false;
            if (KeyboardResponder.IsKeyUp(Keys.W))
                ControlUp = false;
            if (KeyboardResponder.IsKeyUp(Keys.S))
                ControlDown = false;
            if (KeyboardResponder.IsKeyClickAfter(Keys.Space))
                ControlJump = false;
            if (KeyboardResponder.IsKeyUp(Keys.Space))
                ControlKeepJump = false;
            base.DoUpdate();
        }
    }
}