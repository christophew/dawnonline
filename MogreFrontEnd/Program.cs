using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting;
using Mogre;
using Mogre.TutorialFramework;
using System;
using MogreFrontEnd;


namespace Mogre.Tutorials
{
    class Tutorial : BaseApplication
    {
        private DawnClient.DawnClient _dawnClient = new DawnClient.DawnClient();
        private DawnToMogre _mogreModel;

        protected MOIS.InputManager mInputMgr;
        protected MOIS.Keyboard mKeyboard;
        protected MOIS.Mouse mMouse;

        public static void Main()
        {
            new Tutorial().Go();
        }

        protected override void CreateScene()
        {
            if (!_dawnClient.Connect())
            {
                throw new ServerException("Dawn server not found");
            }

            _mogreModel = new DawnToMogre(mSceneMgr, _dawnClient, mCamera);
            _mogreModel.SimulationToOgre();


            mSceneMgr.AmbientLight = ColourValue.Black;
            mSceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_NONE;
            //mSceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_STENCIL_MODULATIVE;


            // Ground
            Plane plane = new Plane(Vector3.UNIT_Y, 0);

            MeshManager.Singleton.CreatePlane("ground",
                ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, plane,
                1500, 1500, 20, 20, true, 1, 5, 5, Vector3.UNIT_Z);

            // Ground
            //Entity groundEnt = mSceneMgr.CreateEntity("GroundEntity", "ground");
            //mSceneMgr.RootSceneNode.CreateChildSceneNode(new Vector3(0, -3, 0)).AttachObject(groundEnt);
            //groundEnt.SetMaterialName("Examples/Rockwall");
            //groundEnt.CastShadows = false;

            {
                // Light
                Light pointLight = mSceneMgr.CreateLight();
                pointLight.Type = Light.LightTypes.LT_POINT;
                pointLight.Position = new Vector3(0, 150, 250);
                pointLight.DiffuseColour = ColourValue.White;
                pointLight.SpecularColour = ColourValue.White;
            }
            {
                // Light
                Light pointLight = mSceneMgr.CreateLight();
                pointLight.Type = Light.LightTypes.LT_POINT;
                pointLight.Position = new Vector3(500, 150, 250);
                pointLight.DiffuseColour = ColourValue.White;
                pointLight.SpecularColour = ColourValue.White;
            }
        }

        protected override void InitializeInput()
        {
            base.InitializeInput();

            int windowHandle;
            mWindow.GetCustomAttribute("WINDOW", out windowHandle);
            mInputMgr = MOIS.InputManager.CreateInputSystem((uint)windowHandle);

            mKeyboard = (MOIS.Keyboard)mInputMgr.CreateInputObject(MOIS.Type.OISKeyboard, false);
            mMouse = (MOIS.Mouse)mInputMgr.CreateInputObject(MOIS.Type.OISMouse, false);
        }

        protected override void CreateFrameListeners()
        {
            base.CreateFrameListeners();
            mRoot.FrameRenderingQueued += UpdateSimulation;
            mRoot.FrameRenderingQueued += ProcessUnbufferedInput;
        }

        private bool ProcessUnbufferedInput(FrameEvent evt)
        {
            mKeyboard.Capture();
            mMouse.Capture();

            _mogreModel.UpdateAvatar(mKeyboard);

            return true;
        }

        protected bool UpdateSimulation(FrameEvent evt)
        {
            //long millisecondsSinceLastFrame = (long)(evt.timeSinceLastFrame*1000.0);

            //_dawnWorld.ThinkAll(30, new TimeSpan(millisecondsSinceLastFrame));
            //_dawnWorld.ApplyMove(millisecondsSinceLastFrame);
            //_dawnWorld.UpdatePhysics(millisecondsSinceLastFrame);

            //_mogreModel.SimulationToOgre();

            _dawnClient.Update();
            _mogreModel.SimulationToOgre();

            return true;
        }

        protected override void CreateCamera()
        {
            //mCamera = mSceneMgr.CreateCamera("PlayerCam");
            //mCamera.Position = new Vector3(200, 100, 300);
            //mCamera.LookAt(new Vector3(200, 0, 100));
            //mCamera.NearClipDistance = 5;
            //mCameraMan = new CameraMan(mCamera);

            mCamera = mSceneMgr.CreateCamera("FirstPersonCam");
            mCamera.Position = new Vector3(0, 1, 0);
            mCamera.LookAt(new Vector3(0, 0, 10));
            mCamera.NearClipDistance = 1;
            mCameraMan = new CameraMan(mCamera);
        }

        protected override void CreateViewports()
        {
            Viewport viewport = mWindow.AddViewport(mCamera);
            viewport.BackgroundColour = ColourValue.Black;
            mCamera.AspectRatio = (float)viewport.ActualWidth / viewport.ActualHeight;
        }
    }
}