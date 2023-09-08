namespace Colin.Core.Common
{
    public class Camera
    {
        public Matrix View;

        public Matrix Projection;

        public Vector2 Translate;

        public Vector2 Position;
        public Vector2 PositionLast;
        public Vector2 TargetPosition;

        public float Rotation;
        public float RotationVelocity;
        public float TargetRotation;

        public Vector2 Zoom;
        public Vector2 ZoomVelocity;
        public Vector2 TargetZoom;

        public bool Trace = true;

        public Vector2 Amount;

        public Vector2 Velocity;

        public bool Enable { get; set; }

        public Scene Scene { get; set; }

        public Matrix Transform => View * Projection;

        private int _width;
        public int Width => _width;

        private int _height;
        public int Height => _height;

        public Point Size => new Point( Width, Height );
        public Vector2 SizeF => new Vector2( Width, Height );

        public void DoInitialize( int width, int height )
        {
            _width = width;
            _height = height;
            Projection = Matrix.CreateOrthographicOffCenter( 0f, width, height, 0f, 0f, 1f );
            View = Matrix.Identity;
            Translate = EngineInfo.ViewCenter;
            Zoom = Vector2.One;
            TargetZoom = Vector2.One;
            ResetCamera();
        }

        public void DoUpdate( GameTime time )
        {
            if(Trace)
            {
                Velocity = (TargetPosition - Position) * 0.1f;
                ZoomVelocity = (TargetZoom - Zoom) * 0.1f;
                RotationVelocity = (TargetRotation - Rotation) * 0.1f;
                if(Vector2.Distance( TargetPosition, Position ) < 0.1f)
                    Position = TargetPosition;
                if(Math.Abs( Rotation - RotationVelocity ) < 0.017f)
                    Rotation = RotationVelocity;
            }
            Zoom += ZoomVelocity;
            Rotation += RotationVelocity;
            PositionLast = Position;
            Position += Velocity + Amount;
            Amount = Vector2.Zero;
            SetView();
        }
        public void MoveCamera( Vector2 amount )
        {
            Amount += amount;
            TargetPosition = Position + amount;
        }

        public void RotateCamera( float amount )
        {
            Rotation += amount;
        }

        public void ResetCamera()
        {
            Rotation = 0f;
            // Zoom = Vector2.One;
            SetView();
        }

        private void SetView()
        {
            Matrix matRotation = Matrix.CreateRotationZ( Rotation );
            Matrix matZoom = Matrix.CreateScale( Zoom.X, Zoom.Y, 1f );
            Vector3 trCenter = new Vector3( Translate, 0f );
            Vector3 translateBody = new Vector3( -Position, 0f );
            View =
                Matrix.CreateTranslation( translateBody ) *
                matZoom *
                matRotation *
                Matrix.CreateTranslation( trCenter );
        }

        public Vector2 ConvertScreenToWorld( Vector2 location )
        {
            Vector3 t = new Vector3( location, 0 );
            t = EngineInfo.Graphics.GraphicsDevice.Viewport.Unproject( t, Projection, View, Matrix.Identity );
            return new Vector2( t.X, t.Y );
        }

        public Vector2 ConvertWorldToScreen( Vector2 location )
        {
            Vector3 t = new Vector3( location, 0 );
            t = EngineInfo.Graphics.GraphicsDevice.Viewport.Project( t, Projection, View, Matrix.Identity );
            return new Vector2( t.X, t.Y );
        }
    }
}