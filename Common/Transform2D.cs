using System.Runtime.Serialization;

namespace Colin.Core.Common
{
    [Serializable]
    [DataContract( IsReference = true )]
    public class Transform2D
    {
        public Transform2D()
        {
            Position = Vector2.Zero;
            Rotation = 0f;
            Scale = Vector2.One;
        }

        public Transform2D Parent
        {
            get => parent;
            set
            {
                if(parent != value)
                {
                    if(parent != null)
                    {
                        parent.children.Remove( this );
                    }
                    parent = value;
                    if(parent != null)
                    {
                        parent.children.Add( this );
                    }
                    SetNeedsAbsoluteUpdate();
                }
            }
        }

        public IEnumerable<Transform2D> Children => children;

        public float AbsoluteRotation => UpdateAbsoluteAndGet( ref absoluteRotation );

        public Vector2 AbsoluteScale => UpdateAbsoluteAndGet( ref absoluteScale );

        public Vector2 AbsolutePosition => UpdateAbsoluteAndGet( ref absolutePosition );

        public float Rotation
        {
            get => localRotation;
            set
            {
                if(localRotation != value)
                {
                    localRotation = value;
                    SetNeedsLocalUpdate();
                }
            }
        }

        public Vector2 Position
        {
            get => localPosition;
            set
            {
                if(localPosition != value)
                {
                    localPosition = value;
                    SetNeedsLocalUpdate();
                }
            }
        }

        public Vector2 Scale
        {
            get => localScale;
            set
            {
                if(localScale != value)
                {
                    localScale = value;
                    SetNeedsLocalUpdate();
                }
            }
        }

        public Matrix Local => UpdateLocalAndGet( ref absolute );

        public Matrix Absolute => UpdateAbsoluteAndGet( ref absolute );

        public Matrix InvertAbsolute => UpdateAbsoluteAndGet( ref invertAbsolute );

        public void ToLocalPosition( ref Vector2 absolute, out Vector2 local )
        {
            Vector2.Transform( ref absolute, ref invertAbsolute, out local );
        }

        public void ToAbsolutePosition( ref Vector2 local, out Vector2 absolute )
        {
            Vector2.Transform( ref local, ref this.absolute, out absolute );
        }

        public Vector2 ToLocalPosition( Vector2 absolute )
        {
            Vector2 result;
            ToLocalPosition( ref absolute, out result );
            return result;
        }

        public Vector2 ToAbsolutePosition( Vector2 local )
        {
            Vector2 result;
            ToAbsolutePosition( ref local, out result );
            return result;
        }

        public void Register( Transform2D transform ) => transform.Parent = this;

        private void SetNeedsLocalUpdate()
        {
            needsLocalUpdate = true;
            SetNeedsAbsoluteUpdate();
        }

        private void SetNeedsAbsoluteUpdate()
        {
            needsAbsoluteUpdate = true;
            foreach(Transform2D transform in children)
            {
                transform.SetNeedsAbsoluteUpdate();
            }
        }

        private void UpdateLocal()
        {
            Matrix matrix = Matrix.CreateScale( Scale.X, Scale.Y, 1f );
            matrix *= Matrix.CreateRotationZ( Rotation );
            matrix *= Matrix.CreateTranslation( Position.X, Position.Y, 0f );
            local = matrix;
            needsLocalUpdate = false;
        }

        private void UpdateAbsolute()
        {
            if(Parent == null)
            {
                absolute = local;
                absoluteScale = localScale;
                absoluteRotation = localRotation;
                absolutePosition = localPosition;
            }
            else
            {
                Matrix matrix = Parent.Absolute;
                Matrix.Multiply( ref local, ref matrix, out absolute );
                absoluteScale = Parent.AbsoluteScale * Scale;
                absoluteRotation = Parent.AbsoluteRotation + Rotation;
                absolutePosition = Vector2.Zero;
                ToAbsolutePosition( ref absolutePosition, out absolutePosition );
            }
            Matrix.Invert( ref absolute, out invertAbsolute );
            needsAbsoluteUpdate = false;
        }

        private T UpdateLocalAndGet<T>( ref T field )
        {
            if(needsLocalUpdate)
                UpdateLocal();
            return field;
        }

        private T UpdateAbsoluteAndGet<T>( ref T field )
        {
            if(needsLocalUpdate)
                UpdateLocal();
            if(needsAbsoluteUpdate)
                UpdateAbsolute();
            return field;
        }

        private Transform2D parent;

        private List<Transform2D> children = new List<Transform2D>();

        private Matrix absolute;

        private Matrix invertAbsolute;

        private Matrix local;

        private float localRotation;

        private float absoluteRotation;

        private Vector2 localScale;

        private Vector2 absoluteScale;

        private Vector2 localPosition;

        private Vector2 absolutePosition;

        private bool needsAbsoluteUpdate = true;

        private bool needsLocalUpdate = true;

    }
}