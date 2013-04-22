using UnityEngine;
using System.Collections;

public class MoveCamera : MonoBehaviour
{

    public int Speed = 5;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    UpdateCamera();
	}

    private void UpdateCamera()
    {
        var position = transform.position;

        // zoom out
        if (Input.GetKey(KeyCode.Keypad9))
        {
            position = new Vector3(position.x, position.y + Speed, position.z);
        }
        // zoom in
        if (Input.GetKey(KeyCode.Keypad7))
        {
            position = new Vector3(position.x, position.y - Speed, position.z);
        }

        // left
        if (Input.GetKey(KeyCode.Keypad4))
        {
            position = new Vector3(position.x - Speed, position.y, position.z);
        }
        // right
        if (Input.GetKey(KeyCode.Keypad6))
        {
            position = new Vector3(position.x + Speed, position.y, position.z);
        }

        // up
        if (Input.GetKey(KeyCode.Keypad8))
        {
            position = new Vector3(position.x, position.y, position.z + Speed);
        }
        // down
        if (Input.GetKey(KeyCode.Keypad2))
        {
            position = new Vector3(position.x, position.y, position.z - Speed);
        }


        transform.position = position;
    }



}
