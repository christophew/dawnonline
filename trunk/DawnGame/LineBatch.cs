﻿using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using Matrix = Microsoft.Xna.Framework.Matrix;

namespace A.Namespace.Of.Your.Choice.Graphics
{
    public sealed class LineBatch
    {
        private GraphicsDevice graphicsDevice;
        private List<VertexPositionColor> points =
            new List<VertexPositionColor>();
        private List<short> indices = new List<short>();
        private VertexDeclaration vertexDeclaration;
        private BasicEffect basicEffect;

        public LineBatch(GraphicsDevice graphicsDevice, float alpha)
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
        }

        public void Begin()
        {
            points.Clear();
            indices.Clear();
        }

        public void Batch(Vector2 startPoint, Vector2 endPoint,
            Color color, float layerDepth)
        {
            Batch(startPoint, color, layerDepth);
            Batch(endPoint, color, layerDepth);
        }

        public void Batch(Vector2 startPoint, Color startColor,
            Vector2 endPoint, Color endColor, float layerDepth)
        {
            Batch(startPoint, startColor, layerDepth);
            Batch(endPoint, endColor, layerDepth);
        }

        public void Batch(Vector2 point, Color color, float layerDepth)
        {
            VertexPositionColor batchPoint =
                new VertexPositionColor(
                new Vector3(point, layerDepth), color);
            points.Add(batchPoint);

            indices.Add((short)indices.Count);
        }

        public void End()
        {
            if (points.Count > 0)
            {
                graphicsDevice.VertexDeclaration = vertexDeclaration;
                graphicsDevice.RenderState.FillMode = FillMode.Solid;

                basicEffect.Begin();

                foreach (EffectPass effectPass in basicEffect.CurrentTechnique.Passes)
                {
                    effectPass.Begin();

                    graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                        PrimitiveType.LineList, points.ToArray(), 0, points.Count,
                        indices.ToArray(), 0, points.Count / 2);

                    effectPass.End();
                }

                basicEffect.End();
            }
        }
    }
}
