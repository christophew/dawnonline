using UnityEngine;
using System.Collections;

public class Aging : MonoBehaviour
{


    private float _startTime;

	// Use this for initialization
	void Start ()
	{
	    _startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update ()
	{
        //var increase = new Vector3(0, GetAgeInSeconds() * 0.01f, 0);
        //transform.localScale += increase;
        var newScale = new Vector3(transform.localScale.x, transform.localScale.y + GetAgeInSeconds() * 0.00001f, transform.localScale.z);
        transform.localScale = newScale;
	}

    private float GetAgeInSeconds()
    {
        return Time.time - _startTime;
    }
}
