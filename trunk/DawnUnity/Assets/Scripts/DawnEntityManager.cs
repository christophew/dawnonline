using System;
using System.Collections.Generic;
using SharedConstants;
using UnityEngine;
using System.Collections;
using DawnClient;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.Lite;
using System.Linq;
using Random = UnityEngine.Random;

public class DawnEntityManager : MonoBehaviour
{
    public Transform WallTemplate;
    public Transform PredatorTemplate;


    private DawnClient.DawnClient _dawnClient;

    private int _update1;
    private int _update2;
    private int _update3;

    private string _debugInfoNrOfWalls;
    private string _debugInfoNrOfPredators;
    private string _debugInfoNrOfPredators2;

    private

	// Use this for initialization
	void Start () {

        Application.runInBackground = true; //without this Photon will loose connection if not focussed

	    _dawnClient = new DawnClient.DawnClient();
        _dawnClient.Connect();
	}
	
	// Update is called once per frame
	void Update ()
	{
	    _update1++;
        if (!_dawnClient.WorldLoaded)
        {
            _update2++;
            _dawnClient.Update();
            return;
        }


        _update3++;
        _dawnClient.SendCommandsToServer();

	    _debugInfoNrOfWalls = _dawnClient.DawnWorld.GetEntities().Where(e => e.Specy == EntityType.Wall).Count().ToString();
        _debugInfoNrOfPredators = _dawnClient.DawnWorld.GetEntities().Where(e => e.Specy == EntityType.Predator).Count().ToString();
        _debugInfoNrOfPredators2 = _dawnClient.DawnWorld.GetEntities().Where(e => e.Specy == EntityType.Predator2).Count().ToString();


        // Update nodes & create new nodes
        var currentEntities = new HashSet<int>();

        var entities = _dawnClient.DawnWorld.GetEntities();
        foreach (var current in entities)
        {
            EntityToNode(current, currentEntities);
        }

        // Remove destroyed entities
        var entitiesCopy = _entities.ToList();
        foreach (var entity in entitiesCopy)
        {
            if (!currentEntities.Contains(entity.Key))
            {
                _entities.Remove(entity.Key);
                // TODO: delete node
                //mSceneMgr.RootSceneNode.RemoveAndDestroyChild(entity.Value.Node.Name);
            }
        }
	}

    void OnGUI()
    {
        if (Application.isEditor)  // or check the app debug flag
        {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "WorldLoaded: " + _dawnClient.WorldLoaded);
            GUI.Label(new Rect(0, 20, Screen.width, Screen.height), "Update1: " + _update1);
            GUI.Label(new Rect(100, 20, Screen.width, Screen.height), "Update2: " + _update2);
            GUI.Label(new Rect(200, 20, Screen.width, Screen.height), "Update3: " + _update3);
            GUI.Label(new Rect(0, 80, Screen.width, Screen.height), "#walls: " + _debugInfoNrOfWalls);
            GUI.Label(new Rect(0, 100, Screen.width, Screen.height), "#predators: " + _debugInfoNrOfPredators);
            GUI.Label(new Rect(100, 100, Screen.width, Screen.height), "#predators2: " + _debugInfoNrOfPredators2);
        }
    }


    private Dictionary<int, Transform> _entities = new Dictionary<int, Transform>();


    private void EntityToNode(DawnClientEntity entity, HashSet<int> currentEntities)
    {
        Transform entityTransform = null;
        //float initialAngle = -Math.HALF_PI;

        //float angle = entity.Angle; // +initialAngle;
        //Vector2 position = new Vector2(entity.PlaceX, entity.PlaceY);

        if (_entities.TryGetValue(entity.Id, out entityTransform))
        {
            // TODO: update x, y, angle
            //entityTransform.position.Set(entity.PlaceX, 0, entity.PlaceY);
            entityTransform.position = new Vector3(entity.PlaceX, 0, entity.PlaceY);
        }
        else
        {
            Transform node = null;

            if (entity.Specy == EntityType.Wall)
                node = SpawnObject(entity, WallTemplate);
            else if (entity.Specy == EntityType.Predator || entity.Specy == EntityType.Predator2)
                node = SpawnObject(entity, PredatorTemplate);
            else
            {
				// TODO
                node = SpawnObject(entity, WallTemplate);
            }

            _entities.Add(entity.Id, node);
        }



        if (currentEntities != null)
        {
            currentEntities.Add(entity.Id);
        }
    }


    private Transform SpawnObject(DawnClientEntity entity, Transform template)
    {
        Vector3 position = new Vector3(entity.PlaceX, 0, entity.PlaceY);
        //Quaternion rotation = Quaternion.A;
        Transform newObj = (Transform)Instantiate(template, position, transform.rotation);

        return newObj;
    }
}
