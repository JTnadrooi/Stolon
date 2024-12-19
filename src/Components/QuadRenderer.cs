using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using DiscordRPC;
using DiscordRPC.Events;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

#nullable enable

namespace Stolon
{
    /// <summary>
    /// Renders a simple quad to the screen. Uncomment the Vertex / Index buffers to make it a static fullscreen quad. 
    /// The performance effect is barely measurable though and you need to dispose of the buffers when finished!
    /// </summary>
    public class QuadRenderer
    {
        //buffers for rendering the quad
        private readonly VertexPositionTexture[] vertexBuffer;
        private readonly short[] indexBuffer;

        public QuadRenderer(GraphicsDevice graphicsDevice)
        {
            vertexBuffer = new VertexPositionTexture[4];
            vertexBuffer[0] = new VertexPositionTexture(new Vector3(-1, 1, 1), new Vector2(0, 0));
            vertexBuffer[1] = new VertexPositionTexture(new Vector3(1, 1, 1), new Vector2(1, 0));
            vertexBuffer[2] = new VertexPositionTexture(new Vector3(-1, -1, 1), new Vector2(0, 1));
            vertexBuffer[3] = new VertexPositionTexture(new Vector3(1, -1, 1), new Vector2(1, 1));

            indexBuffer = new short[] { 0, 3, 2, 0, 1, 3 };
        }

        public void RenderQuad(GraphicsDevice graphicsDevice, Vector2 v1, Vector2 v2)
        {
            vertexBuffer[0].Position.X = v1.X;
            vertexBuffer[0].Position.Y = v2.Y;

            vertexBuffer[1].Position.X = v2.X;
            vertexBuffer[1].Position.Y = v2.Y;

            vertexBuffer[2].Position.X = v1.X;
            vertexBuffer[2].Position.Y = v1.Y;

            vertexBuffer[3].Position.X = v2.X;
            vertexBuffer[3].Position.Y = v1.Y;

            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertexBuffer, 0, 4, indexBuffer, 0, 2);
        }
    }
}
