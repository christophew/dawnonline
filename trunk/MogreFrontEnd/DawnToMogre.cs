using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnClient;
using Mogre;
using Mogre.TutorialFramework;
using Math = Mogre.Math;

namespace MogreFrontEnd
{
    class DawnToMogre
    {
        private SceneManager mSceneMgr;
        internal DawnClientWorld _dawnWorld;
        internal DawnClient.DawnClient _dawnClient;
        private Dictionary<int, SlerpNode> _entities = new Dictionary<int, SlerpNode>();
        private Camera _fpCamera;


        internal DawnToMogre(SceneManager sceneManager, DawnClient.DawnClient dawnClient, Camera fpCamera)
        {
            mSceneMgr = sceneManager;
            _dawnClient = dawnClient;
            _dawnWorld = dawnClient.DawnWorld;
            _fpCamera = fpCamera;
        }

        internal void SimulationToOgre()
        {
            var currentEntities = new HashSet<int>();

            var entities = _dawnWorld.GetEntities();
            foreach (var current in entities)
            {
                EntityToNode(current, currentEntities);
            }


            // Remove destroyed entities
            var entitiesCopy = _entities.ToList();
            foreach (var entity in entitiesCopy)
            {
                if (!currentEntities.Contains(entity.Key))
                {
                    _entities.Remove(entity.Key);
                    mSceneMgr.RootSceneNode.RemoveAndDestroyChild(entity.Value.Node.Name);
                }
            }
        }

        //internal void SimulationToOgre()
        //{
        //    var currentEntities = new Dictionary<int, bool>();

        //    CubeWorldNodes(currentEntities);
        //    BulletNodes(currentEntities);
        //    CreatureNodes(currentEntities);


        //    // Remove destroyed entities
        //    var entitiesCopy = _entities.ToList();
        //    foreach (var entity in entitiesCopy)
        //    {
        //        if (!currentEntities.ContainsKey(entity.Key))
        //        {
        //            _entities.Remove(entity.Key);
        //            mSceneMgr.RootSceneNode.RemoveAndDestroyChild(entity.Value.Name);
        //        }
        //    }
        //}

        //internal SceneNode GetAvatorNode()
        //{
        //    return EntityToNode(_dawnWorld.GetAvatar(), null);
        //}

        internal void UpdateAvatar(MOIS.Keyboard keyboard)
        {
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_I))
            {
                _dawnClient.SendAvatorCommand(keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT)
                                                  ? AvatarCommand.WalkForward
                                                  : AvatarCommand.RunForward);
            }
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_K))
                _dawnClient.SendAvatorCommand(AvatarCommand.WalkBackward);
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_J))
            {
                _dawnClient.SendAvatorCommand(keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT)
                                                  ? AvatarCommand.TurnLeftSlow
                                                  : AvatarCommand.TurnLeft);
            }
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_L))
            {
                _dawnClient.SendAvatorCommand(keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT)
                                                  ? AvatarCommand.TurnRightSlow
                                                  : AvatarCommand.TurnRight);
            }
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_U))
                _dawnClient.SendAvatorCommand(AvatarCommand.StrafeLeft);
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_O))
                _dawnClient.SendAvatorCommand(AvatarCommand.StrafeRight);
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_SPACE))
                _dawnClient.SendAvatorCommand(AvatarCommand.Fire);
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_LCONTROL))
                _dawnClient.SendAvatorCommand(AvatarCommand.FireRocket);

            //if (keyboard.IsKeyDown(MOIS.KeyCode.KC_T))
            //    _dawnWorld.Avatar.BuildEntity(EntityType.Turret);
        }

        internal void UpdateAvatar_old(MOIS.Keyboard keyboard)
        {
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_UP) || keyboard.IsKeyDown(MOIS.KeyCode.KC_Z) || keyboard.IsKeyDown(MOIS.KeyCode.KC_NUMPAD8))
            {
                _dawnClient.SendAvatorCommand(keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT)
                                                  ? AvatarCommand.WalkForward
                                                  : AvatarCommand.RunForward);
            }
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_DOWN) || keyboard.IsKeyDown(MOIS.KeyCode.KC_S) || keyboard.IsKeyDown(MOIS.KeyCode.KC_NUMPAD2))
                _dawnClient.SendAvatorCommand(AvatarCommand.WalkBackward);
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_LEFT) || keyboard.IsKeyDown(MOIS.KeyCode.KC_Q) || keyboard.IsKeyDown(MOIS.KeyCode.KC_NUMPAD4))
            {
                _dawnClient.SendAvatorCommand(keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT)
                                                  ? AvatarCommand.TurnLeftSlow
                                                  : AvatarCommand.TurnLeft);
            }
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_RIGHT) || keyboard.IsKeyDown(MOIS.KeyCode.KC_D) || keyboard.IsKeyDown(MOIS.KeyCode.KC_NUMPAD6))
            {
                if (keyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT))
                    _dawnClient.SendAvatorCommand(AvatarCommand.TurnRightSlow);
                else
                    _dawnClient.SendAvatorCommand(AvatarCommand.TurnRight);
            }
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_A))
                _dawnClient.SendAvatorCommand(AvatarCommand.StrafeLeft);
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_E))
                _dawnClient.SendAvatorCommand(AvatarCommand.StrafeRight);
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_SPACE))
                _dawnClient.SendAvatorCommand(AvatarCommand.Fire);
            if (keyboard.IsKeyDown(MOIS.KeyCode.KC_LCONTROL))
                _dawnClient.SendAvatorCommand(AvatarCommand.FireRocket);

            //if (keyboard.IsKeyDown(MOIS.KeyCode.KC_T))
            //    _dawnWorld.Avatar.BuildEntity(EntityType.Turret);
        }

        //private void CubeWorldNodes(Dictionary<IEntity, bool> currentNodes)
        //{
        //    var entities = _dawnWorld.Environment.GetObstacles();
        //    foreach (var current in entities)
        //    {
        //        EntityToNode(current, currentNodes);
        //    }
        //}

        //private void BulletNodes(Dictionary<IEntity, bool> currentNodes)
        //{
        //    var entities = _dawnWorld.Environment.GetBullets();
        //    foreach (var current in entities)
        //    {
        //        EntityToNode(current, currentNodes);
        //    }
        //}

        //private void CreatureNodes(Dictionary<IEntity, bool> currentNodes)
        //{
        //    var entities = _dawnWorld.Environment.GetCreatures();
        //    foreach (var current in entities)
        //    {
        //        EntityToNode(current, currentNodes);
        //    }
        //}

        //private SceneNode EntityToNode(DawnClientEntity entity, Dictionary<IEntity, bool> currentNodes)
        private void EntityToNode(DawnClientEntity entity, HashSet<int> currentEntities)
        {
            SlerpNode slerpNode = null;
            float initialAngle = -Math.HALF_PI;

            float angle = entity.Angle + initialAngle;
            Vector2 position = new Vector2(entity.PlaceX, entity.PlaceY);

            if (_entities.TryGetValue(entity.Id, out slerpNode))
            {
                slerpNode.Update(position, angle);
            }
            else
            {
                SceneNode node = null;

                switch (entity.Specy)
                {
                    case DawnClientEntity.EntityType.Avatar:
                        node = CreateAvatorNode(entity);
                        break;
                    case DawnClientEntity.EntityType.Predator:
                        node = CreatePredatorNode(entity);
                        break;
                    case DawnClientEntity.EntityType.Turret:
                        node = CreateDummyNode(entity);
                        break;
                    case DawnClientEntity.EntityType.Box:
                        node = CreateBoxNode(entity);
                        break;
                    case DawnClientEntity.EntityType.Wall:
                        node = CreateWallNode(entity);
                        break;
                    case DawnClientEntity.EntityType.Treasure:
                        node = CreateTreasureNode(entity);
                        break;
                    case DawnClientEntity.EntityType.PredatorFactory:
                        node = CreateDummyNode(entity);
                        break;
                    case DawnClientEntity.EntityType.Bullet:
                        node = CreateBulletNode(entity);
                        break;
                    case DawnClientEntity.EntityType.Rocket:
                        node = CreateRocketNode(entity);
                        break;
                    case DawnClientEntity.EntityType.SpawnPoint:
                        node = CreateSpawnPointNode(entity);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                slerpNode = new SlerpNode(node, position, angle);

                _entities.Add(entity.Id, slerpNode);
            }


            // Attach FP-camera when possible
            // Cannot be done on creation (we are not always sure about our avatarId)
            if (entity.Specy == DawnClientEntity.EntityType.Avatar)
            {
                // Check if camera is already attached
                if (_fpCamera != null && !_fpCamera.IsAttached)
                {
                    // Is this out avatar?
                    if (entity.Id == _dawnClient.AvatarId)
                    {
                        slerpNode.Node.AttachObject(_fpCamera);
                    }
                }
            }


            if (currentEntities != null)
            {
                currentEntities.Add(entity.Id);
            }
        }

        private SceneNode CreateDummyNode(DawnClientEntity entity)
        {
            var box = mSceneMgr.CreateEntity("cone.mesh");
            SceneNode node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(2.5f, 2.5f, 2.5f);

            return node;
        }

        private SceneNode CreateSpawnPointNode(DawnClientEntity entity)
        {
            var box = mSceneMgr.CreateEntity("cone.mesh");
            SceneNode node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(2.5f, 2.5f, 2.5f);

            var material = GetFamilyMaterial(entity);
            box.SetMaterial(material);

            return node;
        }

        private SceneNode CreateBulletNode(DawnClientEntity entity)
        {
            var box = mSceneMgr.CreateEntity("sphere.mesh");
            SceneNode node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(0.1f, 0.1f, 0.1f);

            return node;
        }

        private SceneNode CreateRocketNode(DawnClientEntity entity)
        {
            var box = mSceneMgr.CreateEntity("cone.mesh");
            SceneNode rootNode = mSceneMgr.RootSceneNode.CreateChildSceneNode();

            // Use a subnode, because the orientation of the main node will be reset
            SceneNode node = rootNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(0.1f, 0.3f, 0.1f);
            node.Pitch(Math.HALF_PI);

            return rootNode;
        }



        private SceneNode CreateAvatorNode(DawnClientEntity entity)
        {
            var rootNode = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            rootNode.Scale(1.5f, 1.5f, 1.5f);

            var material = GetFamilyMaterial(entity);


            {
                var box = mSceneMgr.CreateEntity("Cube.mesh");
                var node = rootNode.CreateChildSceneNode();
                node.AttachObject(box);
                node.Scale(1f, 0.2f, 1f);

                box.SetMaterial(material);
            }

            {
                var wheel = mSceneMgr.CreateEntity("Cylinder.mesh");
                var wheelNode = rootNode.CreateChildSceneNode(new Vector3(-1, 0, 0.5f));
                wheelNode.AttachObject(wheel);
                wheelNode.Pitch(Math.HALF_PI);
                wheelNode.Roll(Math.HALF_PI);
                wheelNode.Scale(0.3f, 0.2f, 0.3f);
            }
            {
                var wheel = mSceneMgr.CreateEntity("Cylinder.mesh");
                var wheelNode = rootNode.CreateChildSceneNode(new Vector3(-1, 0, -0.5f));
                wheelNode.AttachObject(wheel);
                wheelNode.Pitch(Math.HALF_PI);
                wheelNode.Roll(Math.HALF_PI);
                wheelNode.Scale(0.3f, 0.2f, 0.3f);
            }
            {
                var wheel = mSceneMgr.CreateEntity("Cylinder.mesh");
                var wheelNode = rootNode.CreateChildSceneNode(new Vector3(1, 0, 0.5f));
                wheelNode.AttachObject(wheel);
                wheelNode.Pitch(Math.HALF_PI);
                wheelNode.Roll(Math.HALF_PI);
                wheelNode.Scale(0.3f, 0.2f, 0.3f);
            }
            {
                var wheel = mSceneMgr.CreateEntity("Cylinder.mesh");
                var wheelNode = rootNode.CreateChildSceneNode(new Vector3(1, 0, -0.5f));
                wheelNode.AttachObject(wheel);
                wheelNode.Pitch(Math.HALF_PI);
                wheelNode.Roll(Math.HALF_PI);
                wheelNode.Scale(0.3f, 0.2f, 0.3f);
            }
            // Nose
            {
                var nose = mSceneMgr.CreateEntity("cone.mesh");
                var noseNode = rootNode.CreateChildSceneNode(new Vector3(0, 0, 1));
                noseNode.AttachObject(nose);
                noseNode.Pitch(Math.HALF_PI);
                //noseNode.Roll(Math.HALF_PI);
                noseNode.Scale(0.2f, 0.2f, 0.2f);
            }

            // Light 2
            //Light avatorSpot = mSceneMgr.CreateLight("avatorSpotLight");
            //avatorSpot.Type = Light.LightTypes.LT_SPOTLIGHT;
            //avatorSpot.Position = new Vector3(0, 0, -10);
            //avatorSpot.DiffuseColour = ColourValue.White;
            //avatorSpot.SpecularColour = ColourValue.Blue;
            //avatorSpot.Direction = new Vector3(0, 0, 10);

            //rootNode.AttachObject(avatorSpot);

            return rootNode;
        }

        private SceneNode CreatePredatorNode(DawnClientEntity entity)
        {
            var rootNode = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            rootNode.Scale(1.5f, 1.5f, 1.5f);

            var material = GetFamilyMaterial(entity);


            {
                var box = mSceneMgr.CreateEntity("Cube.mesh");
                var node = rootNode.CreateChildSceneNode();
                node.AttachObject(box);
                node.Scale(1f, 0.2f, 1f);

                box.SetMaterial(material);
            }

            {
                var wheel = mSceneMgr.CreateEntity("Cylinder.mesh");
                var wheelNode = rootNode.CreateChildSceneNode(new Vector3(-1, 0, 0));
                wheelNode.AttachObject(wheel);
                wheelNode.Pitch(Math.HALF_PI);
                wheelNode.Roll(Math.HALF_PI);
                wheelNode.Scale(1f, 0.2f, 0.5f);
            }
            {
                var wheel = mSceneMgr.CreateEntity("Cylinder.mesh");
                var wheelNode = rootNode.CreateChildSceneNode(new Vector3(1, 0, 0));
                wheelNode.AttachObject(wheel);
                wheelNode.Pitch(Math.HALF_PI);
                wheelNode.Roll(Math.HALF_PI);
                wheelNode.Scale(1f, 0.2f, 0.5f);
            }

            // Nose
            {
                var nose = mSceneMgr.CreateEntity("cone.mesh");
                var noseNode = rootNode.CreateChildSceneNode(new Vector3(0, 0, 1.1f));
                noseNode.AttachObject(nose);
                noseNode.Pitch(Math.HALF_PI);
                //noseNode.Roll(Math.HALF_PI);
                noseNode.Scale(0.2f, 0.2f, 0.2f);
            }

            //// Light 2
            //Light light = mSceneMgr.CreateLight();
            //light.Type = Light.LightTypes.LT_POINT;
            //light.Position = new Vector3(0, 0, 0);
            //light.DiffuseColour = ColourValue.Red;
            //light.SpecularColour = ColourValue.Blue;
            //light.Direction = new Vector3(0, 0, 10);
            //rootNode.AttachObject(light);

            return rootNode;
        }

        private static MaterialPtr _boxMaterial = CreateMaterial(new ColourValue(0.7f, 0.7f, 0.7f));
        private SceneNode CreateBoxNode(DawnClientEntity entity)
        {
            var box = mSceneMgr.CreateEntity("cube.mesh");
            SceneNode node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(2.5f, 2.5f, 2.5f);

            box.SetMaterial(_boxMaterial); 

            return node;
        }

        private static MaterialPtr _wallMaterial = CreateMaterial(new ColourValue(0.3f, 0.3f, 0.3f));
        private SceneNode CreateWallNode(DawnClientEntity entity)
        {
            var box = mSceneMgr.CreateEntity("cube.mesh");
            SceneNode node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(2.5f, 2.5f, 2.5f);

            box.SetMaterial(_wallMaterial); 

            return node;
        }

        private SceneNode CreateTreasureNode(DawnClientEntity entity)
        {
            var box = mSceneMgr.CreateEntity("cube.mesh");
            SceneNode node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(0.2f, 0.2f, 0.2f);

            return node;
        }

        private static readonly Random _randomize = new Random((int)DateTime.Now.Ticks);
        Dictionary<int, MaterialPtr> _familyColorMapper = new Dictionary<int, MaterialPtr>();

        private MaterialPtr GetFamilyMaterial(DawnClientEntity entity)
        {
            if (entity.SpawnPointId == 0)
                return null;

            MaterialPtr material;
            if (!_familyColorMapper.TryGetValue(entity.SpawnPointId, out material))
            {
                //var skipColor = _randomize.Next(3);
                var skipColor = -1;
                var color = new ColourValue(
                    skipColor == 0 ? 0 : _randomize.Next(255) / 255f,
                    skipColor == 1 ? 0 : _randomize.Next(255) / 255f,
                    skipColor == 2 ? 0 : _randomize.Next(255) / 255f);

                //var texture = TextureManager.Singleton.Load("Chrome.jpg", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
                material = CreateMaterial(color);

                _familyColorMapper.Add(entity.SpawnPointId, material);
            }

            return material;
        }

        private static MaterialPtr CreateMaterial(ColourValue color)
        {
            MaterialPtr material = MaterialManager.Singleton.Create("MyMaterial", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
            var pass = material.GetTechnique(0).GetPass(0);
            //var unitState = pass.CreateTextureUnitState();
            pass.SetDiffuse(color.r, color.g, color.b, 1.0f);
            //unitState.SetTextureName(texture.Name);
            return material;
        }
    }
}
