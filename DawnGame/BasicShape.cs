﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TexturedBox
{
    class BasicShape
    {
        public Vector3 shapeSize;
        public Vector3 shapePosition;
        private VertexPositionNormalTexture[] shapeVertices;
        private int shapeTriangles;
        private VertexBuffer shapeBuffer;
        
        public Matrix worldMatrix;
        public Texture2D shapeTexture;


        private BasicEffect _cubeEffect;

        public BasicShape(Vector3 size, Vector3 position, float angleY)
        {
            shapeSize = size;
            //shapeSize = new Vector3(size.X / 2.0f, size.Y, size.Z);

            worldMatrix = Matrix.CreateRotationY(-angleY) * Matrix.CreateTranslation(position);

            BuildShape2();
        }

        public void Draw(GraphicsDevice device, Matrix cameraMatrix, Matrix projectionMatrix)
        {
            if (_cubeEffect == null)
            {
                _cubeEffect = new BasicEffect(device);
                _cubeEffect.Texture = shapeTexture;
                _cubeEffect.TextureEnabled = true;
            }


            foreach (EffectPass pass in _cubeEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
            }

            _cubeEffect.World = worldMatrix;
            _cubeEffect.View = cameraMatrix;
            _cubeEffect.Projection = projectionMatrix;
            RenderShape(device);
       }

        public void DrawBatch(GraphicsDevice device, Matrix cameraMatrix, Matrix projectionMatrix, BasicEffect effect)
        {
            effect.World = worldMatrix;
            effect.View = cameraMatrix;
            effect.Projection = projectionMatrix;

            RenderShape(device);
        }


        internal void RenderShape(GraphicsDevice device)
        {
            shapeBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), shapeVertices.Length,
                                           BufferUsage.WriteOnly);

            shapeBuffer.SetData(shapeVertices);
            device.SetVertexBuffer(shapeBuffer);

            //var vertexDeclaration = new VertexDeclaration(VertexPositionTexture.VertexDeclaration.GetVertexElements());

            //device.VertexDeclaration = new VertexDeclaration(
            //    device, VertexPositionNormalTexture.VertexElements);

            device.DrawPrimitives(PrimitiveType.TriangleList, 0, shapeTriangles);
        }

        private void BuildShape()
        {
            shapeTriangles = 12;

            shapeVertices = new VertexPositionNormalTexture[36];

            Vector3 topLeftFront = shapePosition +
                new Vector3(-1.0f, 1.0f, -1.0f) * shapeSize;
            Vector3 bottomLeftFront = shapePosition +
                new Vector3(-1.0f, -1.0f, -1.0f) * shapeSize;
            Vector3 topRightFront = shapePosition +
                new Vector3(1.0f, 1.0f, -1.0f) * shapeSize;
            Vector3 bottomRightFront = shapePosition +
                new Vector3(1.0f, -1.0f, -1.0f) * shapeSize;
            Vector3 topLeftBack = shapePosition +
                new Vector3(-1.0f, 1.0f, 1.0f) * shapeSize;
            Vector3 topRightBack = shapePosition +
                new Vector3(1.0f, 1.0f, 1.0f) * shapeSize;
            Vector3 bottomLeftBack = shapePosition +
                new Vector3(-1.0f, -1.0f, 1.0f) * shapeSize;
            Vector3 bottomRightBack = shapePosition +
                new Vector3(1.0f, -1.0f, 1.0f) * shapeSize;

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f) * shapeSize;
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f) * shapeSize;
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f) * shapeSize;
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f) * shapeSize;
            Vector3 leftNormal = new Vector3(-1.0f, 0.0f, 0.0f) * shapeSize;
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f) * shapeSize;

            Vector2 textureTopLeft = new Vector2(0.5f * shapeSize.X, 0.0f * shapeSize.Y);
            Vector2 textureTopRight = new Vector2(0.0f * shapeSize.X, 0.0f * shapeSize.Y);
            Vector2 textureBottomLeft = new Vector2(0.5f * shapeSize.X, 0.5f * shapeSize.Y);
            Vector2 textureBottomRight = new Vector2(0.0f * shapeSize.X, 0.5f * shapeSize.Y);

            // Front face.
            shapeVertices[0] = new VertexPositionNormalTexture(
                topLeftFront, frontNormal, textureTopLeft);
            shapeVertices[1] = new VertexPositionNormalTexture(
                bottomLeftFront, frontNormal, textureBottomLeft);
            shapeVertices[2] = new VertexPositionNormalTexture(
                topRightFront, frontNormal, textureTopRight);
            shapeVertices[3] = new VertexPositionNormalTexture(
                bottomLeftFront, frontNormal, textureBottomLeft);
            shapeVertices[4] = new VertexPositionNormalTexture(
                bottomRightFront, frontNormal, textureBottomRight);
            shapeVertices[5] = new VertexPositionNormalTexture(
                topRightFront, frontNormal, textureTopRight);

            // Back face.
            shapeVertices[6] = new VertexPositionNormalTexture(
                topLeftBack, backNormal, textureTopRight);
            shapeVertices[7] = new VertexPositionNormalTexture(
                topRightBack, backNormal, textureTopLeft);
            shapeVertices[8] = new VertexPositionNormalTexture(
                bottomLeftBack, backNormal, textureBottomRight);
            shapeVertices[9] = new VertexPositionNormalTexture(
                bottomLeftBack, backNormal, textureBottomRight);
            shapeVertices[10] = new VertexPositionNormalTexture(
                topRightBack, backNormal, textureTopLeft);
            shapeVertices[11] = new VertexPositionNormalTexture(
                bottomRightBack, backNormal, textureBottomLeft);

            // Top face.
            shapeVertices[12] = new VertexPositionNormalTexture(
                topLeftFront, topNormal, textureBottomLeft);
            shapeVertices[13] = new VertexPositionNormalTexture(
                topRightBack, topNormal, textureTopRight);
            shapeVertices[14] = new VertexPositionNormalTexture(
                topLeftBack, topNormal, textureTopLeft);
            shapeVertices[15] = new VertexPositionNormalTexture(
                topLeftFront, topNormal, textureBottomLeft);
            shapeVertices[16] = new VertexPositionNormalTexture(
                topRightFront, topNormal, textureBottomRight);
            shapeVertices[17] = new VertexPositionNormalTexture(
                topRightBack, topNormal, textureTopRight);

            // Bottom face. 
            shapeVertices[18] = new VertexPositionNormalTexture(
                bottomLeftFront, bottomNormal, textureTopLeft);
            shapeVertices[19] = new VertexPositionNormalTexture(
                bottomLeftBack, bottomNormal, textureBottomLeft);
            shapeVertices[20] = new VertexPositionNormalTexture(
                bottomRightBack, bottomNormal, textureBottomRight);
            shapeVertices[21] = new VertexPositionNormalTexture(
                bottomLeftFront, bottomNormal, textureTopLeft);
            shapeVertices[22] = new VertexPositionNormalTexture(
                bottomRightBack, bottomNormal, textureBottomRight);
            shapeVertices[23] = new VertexPositionNormalTexture(
                bottomRightFront, bottomNormal, textureTopRight);

            // Left face.
            shapeVertices[24] = new VertexPositionNormalTexture(
                topLeftFront, leftNormal, textureTopRight);
            shapeVertices[25] = new VertexPositionNormalTexture(
                bottomLeftBack, leftNormal, textureBottomLeft);
            shapeVertices[26] = new VertexPositionNormalTexture(
                bottomLeftFront, leftNormal, textureBottomRight);
            shapeVertices[27] = new VertexPositionNormalTexture(
                topLeftBack, leftNormal, textureTopLeft);
            shapeVertices[28] = new VertexPositionNormalTexture(
                bottomLeftBack, leftNormal, textureBottomLeft);
            shapeVertices[29] = new VertexPositionNormalTexture(
                topLeftFront, leftNormal, textureTopRight);

            // Right face. 
            shapeVertices[30] = new VertexPositionNormalTexture(
                topRightFront, rightNormal, textureTopLeft);
            shapeVertices[31] = new VertexPositionNormalTexture(
                bottomRightFront, rightNormal, textureBottomLeft);
            shapeVertices[32] = new VertexPositionNormalTexture(
                bottomRightBack, rightNormal, textureBottomRight);
            shapeVertices[33] = new VertexPositionNormalTexture(
                topRightBack, rightNormal, textureTopRight);
            shapeVertices[34] = new VertexPositionNormalTexture(
                topRightFront, rightNormal, textureTopLeft);
            shapeVertices[35] = new VertexPositionNormalTexture(
                bottomRightBack, rightNormal, textureBottomRight);
        }

        private void BuildShape2()
        {
            shapeTriangles = 12;

            shapeVertices = new VertexPositionNormalTexture[36];

            Vector3 topLeftFront = shapePosition +
                new Vector3(0.0f, 1.0f, -1.0f) * shapeSize;
            Vector3 bottomLeftFront = shapePosition +
                new Vector3(0.0f, -1.0f, -1.0f) * shapeSize;
            Vector3 topRightFront = shapePosition +
                new Vector3(1.0f, 1.0f, -1.0f) * shapeSize;
            Vector3 bottomRightFront = shapePosition +
                new Vector3(1.0f, -1.0f, -1.0f) * shapeSize;
            Vector3 topLeftBack = shapePosition +
                new Vector3(0.0f, 1.0f, 1.0f) * shapeSize;
            Vector3 topRightBack = shapePosition +
                new Vector3(1.0f, 1.0f, 1.0f) * shapeSize;
            Vector3 bottomLeftBack = shapePosition +
                new Vector3(0.0f, -1.0f, 1.0f) * shapeSize;
            Vector3 bottomRightBack = shapePosition +
                new Vector3(1.0f, -1.0f, 1.0f) * shapeSize;

            Vector3 frontNormal = new Vector3(0.0f, 0.0f, 1.0f) * shapeSize;
            Vector3 backNormal = new Vector3(0.0f, 0.0f, -1.0f) * shapeSize;
            Vector3 topNormal = new Vector3(0.0f, 1.0f, 0.0f) * shapeSize;
            Vector3 bottomNormal = new Vector3(0.0f, -1.0f, 0.0f) * shapeSize;
            Vector3 leftNormal = new Vector3(0.0f, 0.0f, 0.0f) * shapeSize;
            Vector3 rightNormal = new Vector3(1.0f, 0.0f, 0.0f) * shapeSize;

            Vector2 textureTopLeft = new Vector2(0.5f * shapeSize.X, 0.0f * shapeSize.Y);
            Vector2 textureTopRight = new Vector2(0.0f * shapeSize.X, 0.0f * shapeSize.Y);
            Vector2 textureBottomLeft = new Vector2(0.5f * shapeSize.X, 0.5f * shapeSize.Y);
            Vector2 textureBottomRight = new Vector2(0.0f * shapeSize.X, 0.5f * shapeSize.Y);

            // Front face.
            shapeVertices[0] = new VertexPositionNormalTexture(
                topLeftFront, frontNormal, textureTopLeft);
            shapeVertices[1] = new VertexPositionNormalTexture(
                bottomLeftFront, frontNormal, textureBottomLeft);
            shapeVertices[2] = new VertexPositionNormalTexture(
                topRightFront, frontNormal, textureTopRight);
            shapeVertices[3] = new VertexPositionNormalTexture(
                bottomLeftFront, frontNormal, textureBottomLeft);
            shapeVertices[4] = new VertexPositionNormalTexture(
                bottomRightFront, frontNormal, textureBottomRight);
            shapeVertices[5] = new VertexPositionNormalTexture(
                topRightFront, frontNormal, textureTopRight);

            // Back face.
            shapeVertices[6] = new VertexPositionNormalTexture(
                topLeftBack, backNormal, textureTopRight);
            shapeVertices[7] = new VertexPositionNormalTexture(
                topRightBack, backNormal, textureTopLeft);
            shapeVertices[8] = new VertexPositionNormalTexture(
                bottomLeftBack, backNormal, textureBottomRight);
            shapeVertices[9] = new VertexPositionNormalTexture(
                bottomLeftBack, backNormal, textureBottomRight);
            shapeVertices[10] = new VertexPositionNormalTexture(
                topRightBack, backNormal, textureTopLeft);
            shapeVertices[11] = new VertexPositionNormalTexture(
                bottomRightBack, backNormal, textureBottomLeft);

            // Top face.
            shapeVertices[12] = new VertexPositionNormalTexture(
                topLeftFront, topNormal, textureBottomLeft);
            shapeVertices[13] = new VertexPositionNormalTexture(
                topRightBack, topNormal, textureTopRight);
            shapeVertices[14] = new VertexPositionNormalTexture(
                topLeftBack, topNormal, textureTopLeft);
            shapeVertices[15] = new VertexPositionNormalTexture(
                topLeftFront, topNormal, textureBottomLeft);
            shapeVertices[16] = new VertexPositionNormalTexture(
                topRightFront, topNormal, textureBottomRight);
            shapeVertices[17] = new VertexPositionNormalTexture(
                topRightBack, topNormal, textureTopRight);

            // Bottom face. 
            shapeVertices[18] = new VertexPositionNormalTexture(
                bottomLeftFront, bottomNormal, textureTopLeft);
            shapeVertices[19] = new VertexPositionNormalTexture(
                bottomLeftBack, bottomNormal, textureBottomLeft);
            shapeVertices[20] = new VertexPositionNormalTexture(
                bottomRightBack, bottomNormal, textureBottomRight);
            shapeVertices[21] = new VertexPositionNormalTexture(
                bottomLeftFront, bottomNormal, textureTopLeft);
            shapeVertices[22] = new VertexPositionNormalTexture(
                bottomRightBack, bottomNormal, textureBottomRight);
            shapeVertices[23] = new VertexPositionNormalTexture(
                bottomRightFront, bottomNormal, textureTopRight);

            // Left face.
            shapeVertices[24] = new VertexPositionNormalTexture(
                topLeftFront, leftNormal, textureTopRight);
            shapeVertices[25] = new VertexPositionNormalTexture(
                bottomLeftBack, leftNormal, textureBottomLeft);
            shapeVertices[26] = new VertexPositionNormalTexture(
                bottomLeftFront, leftNormal, textureBottomRight);
            shapeVertices[27] = new VertexPositionNormalTexture(
                topLeftBack, leftNormal, textureTopLeft);
            shapeVertices[28] = new VertexPositionNormalTexture(
                bottomLeftBack, leftNormal, textureBottomLeft);
            shapeVertices[29] = new VertexPositionNormalTexture(
                topLeftFront, leftNormal, textureTopRight);

            // Right face. 
            shapeVertices[30] = new VertexPositionNormalTexture(
                topRightFront, rightNormal, textureTopLeft);
            shapeVertices[31] = new VertexPositionNormalTexture(
                bottomRightFront, rightNormal, textureBottomLeft);
            shapeVertices[32] = new VertexPositionNormalTexture(
                bottomRightBack, rightNormal, textureBottomRight);
            shapeVertices[33] = new VertexPositionNormalTexture(
                topRightBack, rightNormal, textureTopRight);
            shapeVertices[34] = new VertexPositionNormalTexture(
                topRightFront, rightNormal, textureTopLeft);
            shapeVertices[35] = new VertexPositionNormalTexture(
                bottomRightBack, rightNormal, textureBottomRight);
        }
    }
}
