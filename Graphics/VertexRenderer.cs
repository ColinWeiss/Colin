using Microsoft.Xna.Framework.Content;

namespace Colin.Core.Graphics
{
    public struct VertexInfo : IVertexType
    {
        public static readonly VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[2]
        {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(3 * sizeof(float), VertexElementFormat.Color, VertexElementUsage.Color, 0)
        });
        public Vector3 position;
        public Color color;

        public VertexInfo(Vector3 pos, Color color)
        {
            position = pos;
            this.color = color;
        }
        public VertexDeclaration VertexDeclaration => _vertexDeclaration;
    }

    public class VertexRenderer
    {
        Texture2D texture;
        Effect effect;

        VertexDeclaration instanceVertexDeclaration;

        VertexBuffer instanceBuffer;
        VertexBuffer geometryBuffer;
        IndexBuffer indexBuffer;

        VertexBufferBinding[] bindings;
        InstanceInfo[] instances;

        struct InstanceInfo
        {
            public Vector4 Position;
        };

        int instanceCount = 10000;

        public void Initialize(GraphicsDevice device)
        {
            GenerateInstanceVertexDeclaration();
            GenerateGeometry(device);
            GenerateInstanceInformation(device, instanceCount);

            bindings = new VertexBufferBinding[2];
            bindings[0] = new VertexBufferBinding(geometryBuffer);
            bindings[1] = new VertexBufferBinding(instanceBuffer, 0, 1);
        }

        public void Load(ContentManager Content)
        {
            effect = Content.Load<Effect>("InstancingShader");
            texture = TextureAssets.Get("UI/Default");
        }

        private void GenerateInstanceVertexDeclaration()
        {
            VertexElement[] instanceStreamElements = new VertexElement[2];

            instanceStreamElements[0] =
                    new VertexElement(0, VertexElementFormat.Vector4,
                        VertexElementUsage.Position, 1);

            instanceStreamElements[1] =
                new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector2,
                    VertexElementUsage.TextureCoordinate, 1);

            instanceVertexDeclaration = new VertexDeclaration(instanceStreamElements);
        }

        public void GenerateGeometry(GraphicsDevice device)
        {
            VertexPositionTexture[] vertices = new VertexPositionTexture[4];

            vertices[0].Position = new Vector3(0, 0, 0);
            vertices[0].TextureCoordinate = new Vector2(0, 0);

            vertices[1].Position = new Vector3(0, 10, 0);
            vertices[1].TextureCoordinate = new Vector2(1, 0);

            vertices[2].Position = new Vector3(20, 0, 0);
            vertices[2].TextureCoordinate = new Vector2(0, 1);

            vertices[3].Position = new Vector3(20, 20, 0);
            vertices[3].TextureCoordinate = new Vector2(1, 1);

            geometryBuffer = new VertexBuffer(device, VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            geometryBuffer.SetData(vertices);

            int[] indices = new int[6]
            {
                1 , 0 , 2 , 1 , 2 , 3
            };

            indexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, 6, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        private void GenerateInstanceInformation(GraphicsDevice device, int count)
        {
            instances = new InstanceInfo[count];
            Random rnd = new Random();

            for (int i = 0; i < count; i++)
            {
                instances[i].Position = new Vector4(-rnd.Next(1600), -rnd.Next(1600), 0, 0);
            }
            instanceBuffer = new VertexBuffer(device, instanceVertexDeclaration, count, BufferUsage.WriteOnly);
            instanceBuffer.SetData(instances);
        }

        [Obsolete]
        public void Draw(ref Matrix view, ref Matrix projection, GraphicsDevice device)
        {
            device.Clear(Color.CornflowerBlue);

            effect.CurrentTechnique = effect.Techniques["Instancing"];
            effect.Parameters["worldPosition"].SetValue(view * projection);
            effect.Parameters["sprite"].SetValue(texture);

            device.Indices = indexBuffer;

            effect.CurrentTechnique.Passes[0].Apply();

            device.SetVertexBuffers(bindings);
            device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2, instanceCount);
        }
    }
}