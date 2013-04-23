using DawnClient;
using SharedConstants;
using UnityEngine;
using System.Collections;

public class AvatarScript : MonoBehaviour {

    public DawnClient.DawnClient DawnClient;
    public DawnClientEntity Avatar; 

	// Use this for initialization
	void Start () 
    {
        // Switch to FT camera
        //if (DawnClient.AvatarId == Avatar.Id)
        //{
        //    if (Camera.main != null)
        //    {
        //        Camera.main.enabled = false;
        //    }
        //    var myCamera = transform.FindChild("FPCamera");
        //    if (myCamera != null)
        //    {
        //        myCamera.gameObject.SetActive(true);
        //    }
        //}
    }
	
	// Update is called once per frame
	void Update ()
	{
	    ProcessUserInput();
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
