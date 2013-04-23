using System;
using DawnClient;
using UnityEngine;
using System.Collections;

public class CreatureScript : MonoBehaviour
{
    public DawnClientEntity Entity; 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    void Update()
    {

        if (Entity == null)
            return;

        // Update position & angle
        //{
        //    transform.position = new Vector3(Entity.PlaceX, 0, Entity.PlaceY);

        //    // Why do we need a negative angle?
        //    transform.eulerAngles = new Vector3(0, (float) RadianToDegree(-Entity.Angle), 0);
        //}
    }

    void OnGUI()
    {
        if (Entity == null)
            return;

        if (Camera.main == null)
            return;

        var screenPos = Camera.main.WorldToScreenPoint(transform.position);
        var labelRect = new Rect(screenPos.x, Screen.height - screenPos.y, Screen.width, Screen.height);

        //var label = string.Format("({0}, {1})", (int)transform.position.x, (int)transform.position.z);
        var label = Entity.DamagePercent + "/" + Entity.ResourcePercent;

        GUI.Label(labelRect, label);
    }

    private static double RadianToDegree(double angle)
    {
        return angle * (180.0 / Math.PI);
    }
}
