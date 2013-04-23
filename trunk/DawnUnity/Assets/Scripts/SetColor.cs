using UnityEngine;
using System.Collections;

public class SetColor : MonoBehaviour
{
    public Color Color;

	// Use this for initialization
	void Start () 
    {
        if (renderer != null)
        {
            renderer.material.color = Color;
        }

	}
}
