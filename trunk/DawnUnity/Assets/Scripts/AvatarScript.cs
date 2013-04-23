using System;
using DawnClient;
using SharedConstants;
using UnityEngine;
using System.Collections;

public class AvatarScript : MonoBehaviour {

    public DawnClient.DawnClient DawnClient;
    public DawnClientEntity Avatar;


    private Camera _mainCamera;

    private const string MainCameraId = "MainCamera";
    private const string FPCameraId = "FPCamera";
    private const string TDCameraId = "TDCamera";

	// Use this for initialization
	void Start ()
	{
	    _mainCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    ProcessUserInput();
	    SwitchCamera();
	}

    private void SwitchCamera()
    {
        // Camera control is only accepted on my own avatar
        if (DawnClient.AvatarId != Avatar.Id)
            return;

        if (Input.GetKey(KeyCode.F1))
            SwitchToMain();
        if (Input.GetKey(KeyCode.F2))
            SwitchToFirstPerson();
        if (Input.GetKey(KeyCode.F3))
            SwitchToTopDown();
    }

    private void SwitchToFirstPerson()
    {
        SetCameraActive(MainCameraId, false);
        SetCameraActive(FPCameraId, true);
        SetCameraActive(TDCameraId, false);
    }

    private void SwitchToTopDown()
    {
        SetCameraActive(MainCameraId, false);
        SetCameraActive(FPCameraId, false);
        SetCameraActive(TDCameraId, true);
    }

    private void SwitchToMain()
    {
        SetCameraActive(MainCameraId, true);
        SetCameraActive(FPCameraId, false);
        SetCameraActive(TDCameraId, false);
    }

    private void SetCameraActive(string id, bool newActive)
    {
        if (string.Equals(id, MainCameraId))
        {
            if (_mainCamera == null)
                throw new InvalidOperationException("No Main Camera found");
            _mainCamera.enabled = newActive;
        }

        var myCamera = transform.FindChild(id);
        if (myCamera != null)
        {
            myCamera.gameObject.SetActive(newActive);
        }
    }

    private void ProcessUserInput()
    {
        if (DawnClient == null || Avatar == null)
            return;

        if (DawnClient.AvatarId != Avatar.Id)
            return;



        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z))
        {
            DawnClient.SendAvatorCommand(Input.GetKey(KeyCode.LeftShift)
                                              ? AvatarCommand.WalkForward
                                              : AvatarCommand.RunForward);
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
            DawnClient.SendAvatorCommand(AvatarCommand.WalkBackward);
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.Q))
        {
            DawnClient.SendAvatorCommand(Input.GetKey(KeyCode.LeftShift)
                                              ? AvatarCommand.TurnLeftSlow
                                              : AvatarCommand.TurnLeft);
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.LeftShift))
                DawnClient.SendAvatorCommand(AvatarCommand.TurnRightSlow);
            else
                DawnClient.SendAvatorCommand(AvatarCommand.TurnRight);
        }
        if (Input.GetKey(KeyCode.A))
            DawnClient.SendAvatorCommand(AvatarCommand.StrafeLeft);
        if (Input.GetKey(KeyCode.E))
            DawnClient.SendAvatorCommand(AvatarCommand.StrafeRight);
        if (Input.GetKey(KeyCode.Space))
            DawnClient.SendAvatorCommand(AvatarCommand.Fire);
        if (Input.GetKey(KeyCode.LeftControl))
            DawnClient.SendAvatorCommand(AvatarCommand.FireRocket);
    }
}
