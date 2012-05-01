using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DawnGame;
using DawnOnline.Simulation.Entities;
using Mogre;
using Mogre.TutorialFramework;
using System;
using MogreFrontEnd;


namespace Mogre.Tutorials
{
    class Tutorial : BaseApplication
    {
        private DawnWorld _dawnWorld = new DawnWorld();
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
            _mogreModel = new DawnToMogre(mSceneMgr, _dawnWorld);
            _mogreModel.SimulationToOgre();


            mSceneMgr.AmbientLight = ColourValue.Black;
            mSceneMgr.ShadowTechnique = ShadowTechnique.SHADOWTYPE_NONE;


            // Ground
            Plane plane = new Plane(Vector3.UNIT_Y, 0);

            MeshManager.Singleton.CreatePlane("ground",
                ResourceGroupManager.DEFAULT_RESOURCE_GROUP_NAME, plane,
                1500, 1500, 20, 20, true, 1, 5, 5, Vector3.UNIT_Z);

            //Entity groundEnt = mSceneMgr.CreateEntity("GroundEntity", "ground");
            //mSceneMgr.RootSceneNode.CreateChildSceneNode(new Vector3(0, -3, 0)).AttachObject(groundEnt);
            //groundEnt.SetMaterialName("Examples/Rockwall");
            //groundEnt.CastShadows = false;

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

            UpdateAvatar();

            return true;
        }

        private void UpdateAvatar()
        {
            _dawnWorld.Avatar.ClearActionQueue();

            if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_UP) || mKeyboard.IsKeyDown(MOIS.KeyCode.KC_Z) || mKeyboard.IsKeyDown(MOIS.KeyCode.KC_NUMPAD8))
            {
                if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT))
                    _dawnWorld.Avatar.WalkForward();
                else
                    _dawnWorld.Avatar.RunForward();
            }
            if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_DOWN) || mKeyboard.IsKeyDown(MOIS.KeyCode.KC_S) || mKeyboard.IsKeyDown(MOIS.KeyCode.KC_NUMPAD2))
                _dawnWorld.Avatar.WalkBackward();
            if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_LEFT) || mKeyboard.IsKeyDown(MOIS.KeyCode.KC_Q) || mKeyboard.IsKeyDown(MOIS.KeyCode.KC_NUMPAD4))
            {
                if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT))
                    _dawnWorld.Avatar.TurnLeftSlow();
                else
                    _dawnWorld.Avatar.TurnLeft();
            }
            if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_RIGHT) || mKeyboard.IsKeyDown(MOIS.KeyCode.KC_D) || mKeyboard.IsKeyDown(MOIS.KeyCode.KC_NUMPAD6))
            {
                if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_LSHIFT))
                    _dawnWorld.Avatar.TurnRightSlow();
                else
                    _dawnWorld.Avatar.TurnRight();
            }
            if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_A))
                _dawnWorld.Avatar.StrafeLeft();
            if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_E))
                _dawnWorld.Avatar.StrafeRight();
            if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_SPACE))
                _dawnWorld.Avatar.Fire();
            if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_LCONTROL))
                _dawnWorld.Avatar.FireRocket();

            if (mKeyboard.IsKeyDown(MOIS.KeyCode.KC_T))
                _dawnWorld.Avatar.BuildEntity(EntityType.Turret);
        }

        protected bool UpdateSimulation(FrameEvent evt)
        {
            long millisecondsSinceLastFrame = (long)(evt.timeSinceLastFrame*1000.0);

            _dawnWorld.ThinkAll(30, new TimeSpan(millisecondsSinceLastFrame));
            _dawnWorld.ApplyMove(millisecondsSinceLastFrame);
            _dawnWorld.UpdatePhysics(millisecondsSinceLastFrame);

            _mogreModel.SimulationToOgre();

            return true;
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
    }
}