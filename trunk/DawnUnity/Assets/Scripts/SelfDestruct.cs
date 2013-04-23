using System;
using UnityEngine;
using System.Collections;

public class SelfDestruct : MonoBehaviour
{

    public float TimeToLive = 1;

    private float _startTime;

	// Use this for initialization
	void Start ()
	{
	    _startTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
	    if (Time.time > _startTime + TimeToLive)
	    {
	        Destroy(gameObject);
	    }
	}
}
