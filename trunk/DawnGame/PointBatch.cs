using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace A.Namespace.Of.Your.Choice.Graphics
{
    public sealed class PointBatch
    {
        private GraphicsDevice graphicsDevice;
        private List<VertexPositionColor> points =
            new List<VertexPositionColor>();
        private VertexDeclaration vertexDeclaration;
        private BasicEffect basicEffect;
        private int pointSize;

        public PointBatch(GraphicsDevice graphicsDevice, float alpha, int pointSize)
        {
            this.graphicsDevice = graphicsDevice;

            basicEffect = new BasicEffect(graphicsDevice, null);
            basicEffect.VertexColorEnabled = true;
            basicEffect.Alpha = alpha;
            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0.0F,
                graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height, 0.0F,
                0.0F, -1.0F);
            basicEffect.View = Matrix.Identity;
            basicEffect.World = Matrix.Identity;

            vertexDeclaration = new VertexDeclaration(graphicsDevice,
                VertexPositionColor.VertexElements);

            this.pointSize = pointSize;
        }

        public void Begin()
        {
            points.Clear();
        }

        public void Batch(Vector2 point, Color color)
        {
            VertexPositionColor batchPoint = new VertexPositionColor(
                new Vector3(point, 0.0F), color);

            points.Add(batchPoint);
        }

        public void End()
        {
            if (points.Count > 0)
            {
                graphicsDevice.VertexDeclaration = vertexDeclaration;
                graphicsDevice.RenderState.PointSize = pointSize;
                graphicsDevice.RenderState.FillMode = FillMode.Solid;

                basicEffect.Begin();

                foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
                {
                    effectPass.Begin();

                    graphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                        PrimitiveType.PointList, points.ToArray(), 0, points.Count);

                    effectPass.End();
                }

                basicEffect.End();
            }
        }
    }
}
