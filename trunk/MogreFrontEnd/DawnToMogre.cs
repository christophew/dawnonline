using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DawnGame;
using DawnOnline.Simulation.Entities;
using Mogre;
using Math = Mogre.Math;

namespace MogreFrontEnd
{
    class DawnToMogre
    {
        private SceneManager mSceneMgr;
        internal DawnWorld _dawnWorld;
        private Dictionary<IEntity, SceneNode> _entities = new Dictionary<IEntity, SceneNode>();


        internal DawnToMogre(SceneManager sceneManager, DawnWorld dawnWorld)
        {
            mSceneMgr = sceneManager;
            _dawnWorld = dawnWorld;
        }

        internal void SimulationToOgre()
        {
            var currentEntities = new Dictionary<IEntity, bool>();

            CubeWorldNodes(currentEntities);
            BulletNodes(currentEntities);
            CreatureNodes(currentEntities);


            // Remove destroyed entities
            var entitiesCopy = _entities.ToList();
            foreach (var entity in entitiesCopy)
            {
                if (!currentEntities.ContainsKey(entity.Key))
                {
                    _entities.Remove(entity.Key);
                    mSceneMgr.RootSceneNode.RemoveAndDestroyChild(entity.Value.Name);
                }
            }
        }

        private void CubeWorldNodes(Dictionary<IEntity, bool> currentNodes)
        {
            var entities = _dawnWorld.Environment.GetObstacles();
            foreach (var current in entities)
            {
                EntityToNode(current, currentNodes);
            }
        }

        private void BulletNodes(Dictionary<IEntity, bool> currentNodes)
        {
            var entities = _dawnWorld.Environment.GetBullets();
            foreach (var current in entities)
            {
                EntityToNode(current, currentNodes);
            }
        }

        private void CreatureNodes(Dictionary<IEntity, bool> currentNodes)
        {
            var entities = _dawnWorld.Environment.GetCreatures();
            foreach (var current in entities)
            {
                EntityToNode(current, currentNodes);
            }
        }

        private void EntityToNode(IEntity entity, Dictionary<IEntity, bool> currentNodes)
        {
            SceneNode node = null;
            float initialAngle = -Math.HALF_PI;

            if (!_entities.TryGetValue(entity, out node))
            {
                switch (entity.Specy)
                {
                    case EntityType.Avatar:
                        node = CreateDummyNode(entity);
                        break;
                    case EntityType.Predator:
                        node = CreatePredatorNode(entity);
                        break;
                    case EntityType.Turret:
                        node = CreateDummyNode(entity);
                        break;
                    case EntityType.Box:
                        node = CreateWallNode(entity);
                        break;
                    case EntityType.Wall:
                        node = CreateWallNode(entity);
                        break;
                    case EntityType.Treasure:
                        node = CreateTreasureNode(entity);
                        break;
                    case EntityType.PredatorFactory:
                        node = CreateDummyNode(entity);
                        break;
                    case EntityType.Bullet:
                        node = CreateDummyNode(entity);
                        break;
                    case EntityType.Rocket:
                        node = CreateDummyNode(entity);
                        break;
                    case EntityType.SpawnPoint:
                        node = CreateDummyNode(entity);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                _entities.Add(entity, node);
            }

            Radian angle = entity.Place.Angle + initialAngle;

            //node.SetOrientation(0, 0, 0, 0);
            node.ResetOrientation();
            node.Yaw(-angle);

            node.SetPosition(entity.Place.Position.X, 0, entity.Place.Position.Y);
            currentNodes.Add(entity, true);
        }

        private SceneNode CreateDummyNode(IEntity entity)
        {
            var box = mSceneMgr.CreateEntity("cone.mesh");
            SceneNode node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(2.5f, 2.5f, 2.5f);

            return node;
        }

        private SceneNode CreatePredatorNode(IEntity entity)
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

            return rootNode;
        }

        private SceneNode CreateWallNode(IEntity entity)
        {
            var box = mSceneMgr.CreateEntity("cube.mesh");
            SceneNode node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(2.5f, 2.5f, 2.5f);

            return node;
        }

        private SceneNode CreateTreasureNode(IEntity entity)
        {
            var box = mSceneMgr.CreateEntity("cube.mesh");
            SceneNode node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            node.AttachObject(box);
            node.Scale(0.2f, 0.2f, 0.2f);

            return node;
        }

        private static readonly Random _randomize = new Random((int)DateTime.Now.Ticks);
        Dictionary<IEntity, MaterialPtr> _familyColorMapper = new Dictionary<IEntity, MaterialPtr>();

        private MaterialPtr GetFamilyMaterial(IEntity entity)
        {
            var creature = entity as ICreature;
            if (creature == null)
                return null;

            MaterialPtr material;
            if (!_familyColorMapper.TryGetValue(creature.SpawnPoint ?? creature, out material))
            {
                var skipColor = _randomize.Next(3);
                var color = new ColourValue(
                    skipColor == 0 ? 0 : _randomize.Next(255),
                    skipColor == 1 ? 0 : _randomize.Next(255),
                    skipColor == 2 ? 0 : _randomize.Next(255));

                //var texture = TextureManager.Singleton.Load("Chrome.jpg", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
                material = MaterialManager.Singleton.Create("MyMaterial", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
                var pass = material.GetTechnique(0).GetPass(0);
                //var unitState = pass.CreateTextureUnitState();
                pass.SetDiffuse(color.r, color.g, color.b, 1.0f);
                //unitState.SetTextureName(texture.Name);

                _familyColorMapper.Add(creature.SpawnPoint ?? creature, material);
            }

            return material;
        }

        private SceneNode FindSceneNode(IEntity entity)
        {
            SceneNode node;
            if (_entities.TryGetValue(entity, out node))
                return node;
            return null;
        }
    }
}
