using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DawnClient;
using DawnGame.Cameras;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RoundLineCode;

namespace DawnGame
{
    class DawnWorldRenderer
    {
        private GameObject _creatureModel;
        private GameObject _creatureModel_Avatar;
        //private GameObject _creatureModel_Monkey;
        private GameObject _cubeModel;
        private GameObject _wallModel;
        private GameObject _bulletModel;
        private GameObject _rocketModel;
        private GameObject _gunModel;
        private GameObject _treasureModel;
        private GameObject _predatorFactoryModel;
        private GameObject _spawnPointModel;

        private Game _game;
        private DawnClient.DawnClient _dawnClient;
        private DawnClient.DawnClientWorld _dawnWorld;
        private ICamera _camera;

        RoundLineManager roundLineManager;
        int roundLineTechniqueIndex = 0;
        string[] roundLineTechniqueNames;
        private Matrix _viewProjMatrix;
        Matrix lineWorldMatrix = Matrix.CreateRotationX(MathHelper.PiOver2); // draw lines on the z-plane

        private static TimeSpan _lastThink;
        private static TimeSpan _lastMove;
        private static DateTime _creaturesAddedAt;

        private readonly Stopwatch _thinkTimer = new Stopwatch();
        private readonly Stopwatch _moveTimer = new Stopwatch();
        private readonly Stopwatch _updateTimer = new Stopwatch();
        public long ThinkTime { get { return _thinkTimer.ElapsedMilliseconds; } }
        public long MoveTime { get { return _moveTimer.ElapsedMilliseconds; } }
        public long UpdateTime { get { return _updateTimer.ElapsedMilliseconds; } }


        public DawnWorldRenderer(Game game, DawnClient.DawnClient dawnClient)
        {
            _game = game;
            _dawnClient = dawnClient;
            _dawnWorld = dawnClient.DawnWorld;
        }

        public void LoadContent()
        {
            _creatureModel = new GameObject(_game.Content.Load<Model>(@"shark"), new Vector3(MathHelper.PiOver2, 0, 0), Vector3.Zero, 1f);
            _creatureModel_Avatar = new GameObject(_game.Content.Load<Model>(@"directx"), new Vector3(MathHelper.PiOver2, 0, 0), Vector3.Zero, 1f);
            _gunModel = new GameObject(_game.Content.Load<Model>(@"gun"), new Vector3(MathHelper.PiOver2, 0, -MathHelper.PiOver2), Vector3.Zero, 0.7f);

            _cubeModel = new GameObject(_game.Content.Load<Model>(@"box"), new Vector3(MathHelper.PiOver2, 0, 0), Vector3.Zero, 2.5f);
            _wallModel = new GameObject(_game.Content.Load<Model>(@"brickwall"), new Vector3(MathHelper.PiOver2, 0, 0), Vector3.Zero, 2.5f);
            _predatorFactoryModel = new GameObject(_game.Content.Load<Model>(@"Factory4"), new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, -50, 0), 5f);

            //_bulletModel = new GameObject(Content.Load<Model>(@"bullet"), Vector3.Zero, 1f);
            _bulletModel = new GameObject(_game.Content.Load<Model>(@"firebullet"), Vector3.Zero, Vector3.Zero, 0.1f);
            _rocketModel = new GameObject(_game.Content.Load<Model>(@"firebullet"), Vector3.Zero, Vector3.Zero, 0.15f);
            //_spawnPointModel = new GameObject(_game.Content.Load<Model>(@"floor3"), new Vector3(MathHelper.PiOver2, 0, 0), new Vector3(0, -28, 0), 1f);
            _spawnPointModel = new GameObject(_game.Content.Load<Model>(@"cube3"), Vector3.Zero, Vector3.Zero, 1.5f);

            _treasureModel = new GameObject(_game.Content.Load<Model>(@"cube3"), Vector3.Zero, Vector3.Zero, 0.5f);


            // RoundlineManager
            roundLineManager = new RoundLineManager();
            roundLineManager.Init(_game.GraphicsDevice, _game.Content);
            roundLineTechniqueNames = roundLineManager.TechniqueNames;
        }

        public void Update(GameTime gameTime)
        {
            KeyboardState keyboardState = Keyboard.GetState();

            // Avatar
            UpdateAvatar();

            // World
            //if (keyboardState.IsKeyDown(Keys.P) && ((DateTime.Now - _creaturesAddedAt).TotalSeconds > 5))
            //{
            //    _dawnWorld.AddCreatures(EntityType.Predator, 10);
            //    _creaturesAddedAt = DateTime.Now;
            //}

            _dawnClient.Update();

        }

        private void UpdateAvatar()
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.Z))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                    _dawnClient.SendAvatorCommand(AvatarCommand.WalkForward);
                else
                     _dawnClient.SendAvatorCommand(AvatarCommand.RunForward);
            }
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))
                     _dawnClient.SendAvatorCommand(AvatarCommand.WalkBackward);
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.Q))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                     _dawnClient.SendAvatorCommand(AvatarCommand.TurnLeftSlow);
                else
                     _dawnClient.SendAvatorCommand(AvatarCommand.TurnLeft);
            }
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))
            {
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                    _dawnClient.SendAvatorCommand(AvatarCommand.TurnRightSlow);
                else
                    _dawnClient.SendAvatorCommand(AvatarCommand.TurnRight);
            }
            if (keyboardState.IsKeyDown(Keys.A))
                _dawnClient.SendAvatorCommand(AvatarCommand.StrafeLeft);
            if (keyboardState.IsKeyDown(Keys.E))
                _dawnClient.SendAvatorCommand(AvatarCommand.StrafeRight);
            if (keyboardState.IsKeyDown(Keys.Space))
                _dawnClient.SendAvatorCommand(AvatarCommand.Fire);
            if (keyboardState.IsKeyDown(Keys.LeftControl))
                _dawnClient.SendAvatorCommand(AvatarCommand.FireRocket);

            //if (keyboardState.IsKeyDown(Keys.T))
            //         _dawnClient.SendAvatorCommand(AvatarCommand.WalkForward);
            //   _dawnWorld.Avatar.BuildEntity(EntityType.Turret);
        }

        public void Draw(GameTime gameTime, ICamera camera)
        {
            _viewProjMatrix = camera.View * camera.Projection;
            _camera = camera;


            {
                //Draw2DWorld(gameTime);
                //Draw3DWorld();
                DrawCubeWorld();
                DrawBullets();

                // Draw creatures
                {
                    var creatures = _dawnWorld.GetEntities();
                    foreach (var current in creatures)
                    {
                        //DrawCreature(current, roundLineManager, viewProjMatrix, time, curTechniqueName);
                        DrawEntity(current);
                        //DrawCreatureInfo(current);
                    }
                }
            }

            //DrawSkyDome();
        }

        private void DrawCubeWorld()
        {
            var obstacles = _dawnWorld.GetEntities();
            foreach (var current in obstacles)
            {
                DrawEntity(current);
            }
        }

        private void DrawBullets()
        {
            var obstacles = _dawnWorld.GetEntities();
            foreach (var current in obstacles)
            {
                DrawEntity(current);
            }
        }

        private void DrawEntity(DawnClientEntity entity)
        {
            var rotation = new Vector3(0, -entity.Angle, 0);
            var position = new Vector3(entity.PlaceX, 0f, entity.PlaceY);

            switch (entity.Specy)
            {
                case DawnClientEntity.EntityType.Avatar:
                    _creatureModel_Avatar.DrawObject(_camera, position, rotation);
                    break;
                case DawnClientEntity.EntityType.Predator:
                    _creatureModel.DrawObject(_camera, position, rotation);
                    break;
                case DawnClientEntity.EntityType.Turret:
                    _gunModel.DrawObject(_camera, position, rotation);
                    break;
                case DawnClientEntity.EntityType.Box:
                    _cubeModel.DrawObject(_camera, position, rotation);
                    break;
                case DawnClientEntity.EntityType.Wall:
                    _wallModel.DrawObject(_camera, position, rotation);
                    break;
                case DawnClientEntity.EntityType.Treasure:
                    _treasureModel.DrawObject(_camera, position, rotation, true);
                    break;
                case DawnClientEntity.EntityType.PredatorFactory:
                    _predatorFactoryModel.DrawObject(_camera, position, rotation);
                    break;
                case DawnClientEntity.EntityType.Bullet:
                    _bulletModel.DrawObject(_camera, position, rotation, true);
                    break;
                case DawnClientEntity.EntityType.Rocket:
                    _rocketModel.DrawObject(_camera, position, rotation, true);
                    break;
                case DawnClientEntity.EntityType.SpawnPoint:
                    _spawnPointModel.DrawObject(_camera, position, rotation, true);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        //private void DrawCreatureInfo(ICreature creature)
        //{
        //    var pos = creature.Place.Position;
        //    var angle = creature.Place.Angle;
        //    Color color = creature.CanAttack() ? Color.Black : Color.Red;

        //    var attackMiddle = new Vector2(
        //        (float)(pos.X + Math.Cos(angle) * creature.CharacterSheet.MeleeRange),
        //        (float)(pos.Y + Math.Sin(angle) * creature.CharacterSheet.MeleeRange));

        //    DrawCircle(attackMiddle, creature.CharacterSheet.MeleeRange, color);
        //}

        //private void Draw2DWorld(GameTime gameTime)
        //{
        //    float time = (float)gameTime.TotalGameTime.TotalSeconds;
        //    string curTechniqueName = roundLineTechniqueNames[roundLineTechniqueIndex];
        //    {
        //        var obstacles = _dawnWorld.Environment.GetObstacles();
        //        foreach (var current in obstacles)
        //        {
        //            var points = current.Place.Form.Shape.Points;
        //            var pos = current.Place.Position;

        //            DrawPolygon(points, time, curTechniqueName);
        //        }
        //    }
        //}

        //private void DrawPolygon(IList<Vector> points, float time, string curTechniqueName)
        //{
        //    List<RoundLine> lines = new List<RoundLine>();
        //    //List<Line> lines = new List<Line>();

        //    for (int i = 0; i < points.Count; i++)
        //    {
        //        DawnOnline.Simulation.Collision.Vector point1 = points[i];
        //        DawnOnline.Simulation.Collision.Vector point2;
        //        if (i + 1 >= points.Count)
        //        {
        //            point2 = points[0];
        //        }
        //        else
        //        {
        //            point2 = points[i + 1];
        //        }
        //        //var vector1 = new Vector2((float)(point1.X + position.X), (float)(point1.Y + position.Y));
        //        //var vector2 = new Vector2((float)(point2.X + position.X), (float)(point2.Y + position.Y));
        //        var vector1 = new Vector2((float)(point1.X), (float)(point1.Y));
        //        var vector2 = new Vector2((float)(point2.X), (float)(point2.Y));

        //        RoundLine line = new RoundLine(vector1, vector2);
        //        //Line line = new Line(vector1, vector2);
        //        lines.Add(line);
        //    }

        //    roundLineManager.Draw(lines, 3, Color.Black, lineWorldMatrix * _viewProjMatrix, time, curTechniqueName);
        //    //lineManager.Draw(lines, 3, Color.Black.ToVector4(), viewMatrix, projMatrix, time, null, lineWorldMatrix, 0.97f);
        //}

        private void DrawCircle(Vector2 centre, double radius, Color color)
        {
            float time = (float)1f / 60f;
            string curTechniqueName = roundLineTechniqueNames[roundLineTechniqueIndex];

            const int nrOfVertexes = 32;

            List<RoundLine> lines = new List<RoundLine>();
            //List<Line> lines = new List<Line>();

            Vector2 lastPoint = new Vector2((float)(centre.X + radius), (float)(centre.Y));

            for (int i = 1; i < nrOfVertexes + 1; i++)
            {
                float currentAngle = i * MathHelper.TwoPi / nrOfVertexes;

                var newPoint = new Vector2((float)(centre.X + Math.Cos(currentAngle) * radius), (float)(centre.Y + Math.Sin(currentAngle) * radius));

                RoundLine line = new RoundLine(lastPoint, newPoint);
                //Line line = new Line(lastPoint, newPoint);

                lines.Add(line);

                lastPoint = newPoint;
            }

            roundLineManager.Draw(lines, 1, color, lineWorldMatrix * _viewProjMatrix, time, curTechniqueName);
            //lineManager.Draw(lines, 1, color.ToVector4(), viewMatrix, projMatrix, time, null, lineWorldMatrix, 1);
        }

    }
}
