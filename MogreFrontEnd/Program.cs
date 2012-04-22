using System.Collections.Generic;
using System.Diagnostics;
using DawnGame;
using DawnOnline.Simulation.Entities;
using Mogre;
using Mogre.TutorialFramework;
using System;



namespace Mogre.Tutorials
{
    class Tutorial : BaseApplication
    {
        private DawnWorld _dawnWorld = new DawnWorld();


        public static void Main()
        {
            new Tutorial().Go();
        }

        protected override void CreateScene()
        {
            mSceneMgr.AmbientLight = ColourValue.Black;
            mSceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_NONE;

            SimulationToOgre();

            // Ground
            Plane plane = new Plane(Vector3.UNIT_Y, 0);

            MeshManager.Singleton.CreatePlane("ground",
                ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, plane,
                1500, 1500, 20, 20, true, 1, 5, 5, Vector3.UNIT_Z);

            Entity groundEnt = mSceneMgr.CreateEntity("GroundEntity", "ground");
            mSceneMgr.RootSceneNode.CreateChildSceneNode(new Vector3(0, -3, 0)).AttachObject(groundEnt);

            groundEnt.SetMaterialName("Examples/Rockwall");
            groundEnt.CastShadows = false;

            // Light
            Light pointLight = mSceneMgr.CreateLight("pointLight");
            pointLight.Type = Light.LightTypes.LT_POINT;
            pointLight.Position = new Vector3(0, 150, 250);
            pointLight.DiffuseColour = ColourValue.White;
            pointLight.SpecularColour = ColourValue.White;

            // Light 2
            Light pointLight2 = mSceneMgr.CreateLight("pointLight2");
            pointLight2.Type = Light.LightTypes.LT_POINT;
            pointLight2.Position = new Vector3(100, 150, 250);
            pointLight2.DiffuseColour = ColourValue.Blue;
            pointLight2.SpecularColour = ColourValue.Blue;
        }

        protected override void CreateFrameListeners()
        {
            base.CreateFrameListeners();
            mRoot.FrameRenderingQueued += UpdateSimulation;
        }

        protected bool UpdateSimulation(FrameEvent evt)
        {
            long millisecondsSinceLastFrame = (long)(evt.timeSinceLastFrame*1000.0);

            _dawnWorld.ThinkAll(30, new TimeSpan(millisecondsSinceLastFrame));
            _dawnWorld.ApplyMove(millisecondsSinceLastFrame);
            _dawnWorld.UpdatePhysics(millisecondsSinceLastFrame);

            SimulationToOgre();

            return true;
        }

        private void SimulationToOgre()
        {
            //var texture = TextureManager.Singleton.Load("Chrome.jpg", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
            //MaterialPtr material = MaterialManager.Singleton.Create("MyMaterial", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
            //var pass = material.GetTechnique(0).GetPass(0);
            //var unitState = pass.CreateTextureUnitState();
            //pass.SetDiffuse(0.5f, 0f, 0f, 0f);
            ////unitState.SetTextureName(texture.Name);

            int counter = 0;
            foreach (var obstacle in _dawnWorld.Environment.GetObstacles())
            {
                if (obstacle.Specy == EntityType.Treasure)
                    continue;

                SceneNode node = FindSceneNode(obstacle);
                if (node == null)
                {
                    var box = mSceneMgr.CreateEntity("cube.mesh");
                    //box.SetMaterial(material);
                    node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
                    node.AttachObject(box);
                    //node.Scale(0.1f, 0.1f, 0.1f);
                    node.Scale(2.5f, 2.5f, 2.5f);
                    
                    _entities.Add(obstacle, node);
                }

                node.SetPosition(obstacle.Place.Position.X, 0, obstacle.Place.Position.Y);
            }

            foreach (var creature in _dawnWorld.Environment.GetCreatures())
            {
                SceneNode node = FindSceneNode(creature);
                if (node == null)
                {
                    //var box = mSceneMgr.CreateEntity("obstacle" + counter++, "ogrehead.mesh");
                    var box = mSceneMgr.CreateEntity("ogrehead.mesh");
                    node = mSceneMgr.RootSceneNode.CreateChildSceneNode();
                    node.AttachObject(box);
                    node.Scale(0.07f, 0.07f, 0.07f);

                    _entities.Add(creature, node);
                }

                const float ogreHeadInitialAngle = Math.HALF_PI;
                float angle = creature.Place.Angle + ogreHeadInitialAngle;
                ////while (angle > Math.TWO_PI)
                ////    angle -= Math.TWO_PI;
                ////while (angle < 0)
                ////    angle += Math.TWO_PI;


                node.SetOrientation(creature.Place.Angle, 0, 1, 0);
                //node.Rotate(Vector3.UNIT_Y, Math.DegreesToRadians(creature.Place.Angle));
                node.SetPosition(creature.Place.Position.X, 0, creature.Place.Position.Y);
            }
        }

        private static readonly Random _randomize = new Random((int)DateTime.Now.Ticks);
        Dictionary<IEntity, ColourValue> _familyColorMapper = new Dictionary<IEntity, ColourValue>();

        private ColourValue GetFamilyColour(IEntity entity)
        {
            var creature = entity as ICreature;
            if (creature == null)
                return ColourValue.White;

            ColourValue color;
            if (!_familyColorMapper.TryGetValue(creature.SpawnPoint, out color))
            {
                var skipColor = _randomize.Next(3);
                color = new ColourValue(
                    skipColor == 0 ? 0 : _randomize.Next(255),
                    skipColor == 1 ? 0 : _randomize.Next(255),
                    skipColor == 2 ? 0 : _randomize.Next(255));
                _familyColorMapper.Add(creature.SpawnPoint, color);
            }

            return color;
        }

        private Dictionary<IEntity, SceneNode> _entities = new Dictionary<IEntity, SceneNode>();

        private SceneNode FindSceneNode(IEntity entity)
        {
            SceneNode node;
            if (_entities.TryGetValue(entity, out node))
                return node;
            return null;
        }

        protected override void CreateCamera()
        {
            mCamera = mSceneMgr.CreateCamera("PlayerCam");
            mCamera.Position = new Vector3(200, 100, 300);
            mCamera.LookAt(new Vector3(200, 0, 100));
            mCamera.NearClipDistance = 5;
            mCameraMan = new CameraMan(mCamera);
        }

        protected override void CreateViewports()
        {
            Viewport viewport = mWindow.AddViewport(mCamera);
            viewport.BackgroundColour = ColourValue.Black;
            mCamera.AspectRatio = (float)viewport.ActualWidth / viewport.ActualHeight;
        }

        //protected override void CreateScene()
        //{
        //    mSceneMgr.AmbientLight = ColourValue.Black;
        //    mSceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_ADDITIVE;

        //    Entity ent = mSceneMgr.CreateEntity("ninja", "ninja.mesh");
        //    ent.CastShadows = true;
        //    mSceneMgr.RootSceneNode.CreateChildSceneNode().AttachObject(ent);

        //    Plane plane = new Plane(Vector3.UNIT_Y, 0);

        //    MeshManager.Singleton.CreatePlane("ground",
        //        ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, plane,
        //        1500, 1500, 20, 20, true, 1, 5, 5, Vector3.UNIT_Z);

        //    Entity groundEnt = mSceneMgr.CreateEntity("GroundEntity", "ground");
        //    mSceneMgr.RootSceneNode.CreateChildSceneNode().AttachObject(groundEnt);

        //    groundEnt.SetMaterialName("Examples/Rockwall");
        //    groundEnt.CastShadows = false;

        //    Light pointLight = mSceneMgr.CreateLight("pointLight");
        //    pointLight.Type = Light.LightTypes.LT_POINT;
        //    pointLight.Position = new Vector3(0, 150, 250);
        //    pointLight.DiffuseColour = ColourValue.Red;
        //    pointLight.SpecularColour = ColourValue.Red;

        //    Light directionalLight = mSceneMgr.CreateLight("directionalLight");
        //    directionalLight.Type = Light.LightTypes.LT_DIRECTIONAL;
        //    directionalLight.DiffuseColour = new ColourValue(.25f, .25f, 0);
        //    directionalLight.SpecularColour = new ColourValue(.25f, .25f, 0);
        //    directionalLight.Direction = new Vector3(0, -1, 1);

        //    Light spotLight = mSceneMgr.CreateLight("spotLight");
        //    spotLight.Type = Light.LightTypes.LT_SPOTLIGHT;
        //    spotLight.DiffuseColour = ColourValue.Blue;
        //    spotLight.SpecularColour = ColourValue.Blue;
        //    spotLight.Direction = new Vector3(-1, -1, 0);
        //    spotLight.Position = new Vector3(300, 300, 0);

        //    spotLight.SetSpotlightRange(new Degree(35), new Degree(50));
        //}

    }
}