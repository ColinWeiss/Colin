using Colin.Core.Events;

namespace Colin.Core.Common
{
    public class SceneEventResponder : ISceneModule
    {
        public Scene Scene { get; set; }

        public bool Enable { get; set; }

        public event EventHandler ClientSizeChanged;
        public event EventHandler OrientationChanged;

        public event Action Reset;

        public MouseEventResponder Mouse;

        public KeysEventResponder KeysEvent;

        public void Start() { }
        public void DoInitialize()
        {
            Mouse = new MouseEventResponder("Scene.MouseEventResponder");
            KeysEvent = new KeysEventResponder("Scene.KeysEventResponder");
            KeysEvent.Postorder = false;
        }
        public void DoUpdate(GameTime time)
        {
            Reset?.Invoke();
            MouseEventArgs mouseEvent = new MouseEventArgs("MouseEvent");
            Mouse.Response(mouseEvent);
            Keys[] lasts = KeyboardResponder.StateLast.GetPressedKeys();
            Keys[] current = KeyboardResponder.State.GetPressedKeys();
            KeyEventArgs keysEvent;
            foreach (var key in lasts)
            {
                if (!current.Contains(key))
                {
                    keysEvent = new KeyEventArgs("KeyEvent");
                    keysEvent.Key = key;
                    keysEvent.ClickAfter = true;
                    KeysEvent.Response(keysEvent);
                }
            }
            foreach (var key in current)
            {
                if (!lasts.Contains(key))
                {
                    keysEvent = new KeyEventArgs("KeyEvent");
                    keysEvent.Key = key;
                    keysEvent.ClickBefore = true;
                    KeysEvent.Response(keysEvent);
                }
                if (lasts.Contains(key))
                {
                    keysEvent = new KeyEventArgs("KeyEvent");
                    keysEvent.Key = key;
                    keysEvent.Down = true;
                    KeysEvent.Response(keysEvent);
                }
            }
        }
        public void InvokeSizeChange(object sender, EventArgs e)
        {
            ClientSizeChanged?.Invoke(this, e);
            OrientationChanged?.Invoke(this, e);
        }
        public void Dispose() { }
    }
}