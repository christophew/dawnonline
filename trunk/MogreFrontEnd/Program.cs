using DawnGame;
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

            int counter = 0;
            //var texture = TextureManager.Singleton.Load("Dirt.jpg", ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME);
            foreach (var obstacle in _dawnWorld.Environment.GetObstacles())
            {
                var box = mSceneMgr.CreateEntity("obstacle" + counter++, "cube.mesh");
                var node = mSceneMgr.RootSceneNode.CreateChildSceneNode(new Vector3(obstacle.Place.Position.X, 0, obstacle.Place.Position.Y));
                node.AttachObject(box);
                //node.Scale(0.1f, 0.1f, 0.1f);
                node.Scale(2.5f, 2.5f, 2.5f);
            }

            foreach (var creature in _dawnWorld.Environment.GetCreatures())
            {
                var box = mSceneMgr.CreateEntity("obstacle" + counter++, "ogrehead.mesh");
                //box.SetMaterial(material);
                var node = mSceneMgr.RootSceneNode.CreateChildSceneNode(new Vector3(creature.Place.Position.X, 0, creature.Place.Position.Y));
                node.AttachObject(box);
                node.Scale(0.07f, 0.07f, 0.07f);
                //node.Scale(2.5f, 2.5f, 2.5f);
            }

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

        protected override void CreateCamera()
        {
            mCamera = mSceneMgr.CreateCamera("PlayerCam");
            mCamera.Position = new Vector3(0, 10, 500);
            mCamera.LookAt(Vector3.ZERO);
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