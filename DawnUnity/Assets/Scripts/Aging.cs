using UnityEngine;
using System.Collections;

public class Aging : MonoBehaviour
{
    private float _startTime;
    private Vector3 _startScale;

	// Use this for initialization
	void Start ()
	{
	    _startTime = Time.time;
	    _startScale = new Vector3(transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}
	
	// Update is called once per frame
	void Update ()
	{
        //var increase = new Vector3(0, GetAgeInSeconds() * 0.01f, 0);
        //transform.localScale += increase;
        var newScale = new Vector3(_startScale.x + GetAgeInSeconds() * 0.001f, _startScale.y + GetAgeInSeconds() * 0.05f, _startScale.z + GetAgeInSeconds() * 0.001f);
        transform.localScale = newScale;
	}

    private float GetAgeInSeconds()
    {
        return Time.time - _startTime;
    }
}
