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
        private static DawnClient.DawnClient _dawnClient;
        private DawnToMogre _mogreModel;

        protected MOIS.InputManager mInputMgr;
        protected MOIS.Keyboard mKeyboard;
        protected MOIS.Mouse mMouse;

        public static void Main()
        {
            _dawnClient = new DawnClient.DawnClient();
            _dawnClient.WorldLoadedEvent += delegate
            {
                _dawnClient.RequestAvatarCreationOnServer();
            };


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

            // Camera anchors
            CreateTopdownCameraAnchor();
        }

        protected override void InitializeInput()
        {
            base.InitializeInput();

            int windowHandle;
            mWindow.GetCustomAttribute("WINDOW", out windowHandle);
            mInputMgr = MOIS.InputManager.CreateInputSystem((uint)windowHandle);

            mKeyboard = (MOIS.Keyboard)mInputMgr.CreateInputObject(MOIS.Type.OISKeyboard, false);
            mMouse = (MOIS.Mouse)mInputMgr.CreateInputObject(MOIS.Type.OISMouse, false);

            //mKeyboard.KeyPressed += new MOIS.KeyListener.KeyPressedHandler(OnPressCameraCommands);
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

        //protected bool OnPressCameraCommands(MOIS.KeyEvent arg)
        protected override bool OnKeyPressed(MOIS.KeyEvent arg)
        {
            base.OnKeyPressed(arg);

            switch (arg.key)
            {
                case MOIS.KeyCode.KC_F1:
                    SwitchCamera();
                    break;
            }

            return true;
        }

        protected bool UpdateSimulation(FrameEvent evt)
        {
            if (!_dawnClient.WorldLoaded)
            {
                _dawnClient.Update();
                return true;
            }

            //long millisecondsSinceLastFrame = (long)(evt.timeSinceLastFrame*1000.0);

            //_dawnWorld.ThinkAll(30, new TimeSpan(millisecondsSinceLastFrame));
            //_dawnWorld.ApplyMove(millisecondsSinceLastFrame);
            //_dawnWorld.UpdatePhysics(millisecondsSinceLastFrame);

            //_mogreModel.SimulationToOgre();

            _dawnClient.SendCommandsToServer();
            _mogreModel.SimulationToOgre();

            return true;
        }

        private Camera _switchCamera;
        private void SwitchCamera()
        {
            mCamera.DetachFromParent();
            _topDownCameraAnchor.AttachObject(mCamera);
        }

        private SceneNode _topDownCameraAnchor;
        private bool _topdownActive = false;
        private void CreateTopdownCameraAnchor()
        {
            if (_topdownActive)
            {
                _topdownActive = false;
                mCamera.DetachFromParent();
                return;
            }

            _topdownActive = true;
            _topDownCameraAnchor = mSceneMgr.RootSceneNode.CreateChildSceneNode();
            _topDownCameraAnchor.Translate(200, 500, 200);
            _topDownCameraAnchor.Pitch(Math.HALF_PI);

            //mCamera.Roll(0);
            //mCamera.Pitch(0);
            //mCamera.(0);
        }

        protected override void CreateCamera()
        {
            _switchCamera = mSceneMgr.CreateCamera("PlayerCam");
            _switchCamera.Position = new Vector3(200, 100, 300);
            _switchCamera.LookAt(new Vector3(200, 0, 100));
            _switchCamera.NearClipDistance = 5;
            
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