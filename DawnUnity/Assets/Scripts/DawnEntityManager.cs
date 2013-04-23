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
    public Transform AvatarTemplate;
    public Transform WallTemplate;
    public Transform BoxTemplate;
    public Transform PredatorTemplate;
    public Transform Predator2Template;
    public Transform SpawnPoint1Template;
    public Transform SpawnPoint2Template;
    public Transform TreasureTemplate;
    public Transform BulletTemplate;
    public Transform RocketTemplate;
    public Transform DefaultTemplate;

    public Transform ExplosionTemplate;


    private DawnClient.DawnClient _dawnClient;

    private bool _connectError;
    private int _debugLoadingCounter;
    private string _debugInfoNrOfWalls;
    private string _debugInfoNrOfBoxes;
    private string _debugInfoNrOfPredators;
    private string _debugInfoNrOfPredators2;


	// Use this for initialization
	void Start () {

        Application.runInBackground = true; //without this Photon will loose connection if not focussed

	    _dawnClient = new DawnClient.DawnClient();
        _dawnClient.WorldLoadedEvent += delegate
        {
            _dawnClient.RequestAvatarCreationOnServer();
        };

        _connectError = !_dawnClient.Connect();
	}
	
	// Update is called once per frame
	void Update ()
	{
        if (!_dawnClient.WorldLoaded)
        {
            _debugLoadingCounter++;
            _dawnClient.Update();
            return;
        }

        _dawnClient.SendCommandsToServer();

	    _debugInfoNrOfWalls = _dawnClient.DawnWorld.GetEntities().Where(e => e.Specy == EntityType.Wall).Count().ToString();
        _debugInfoNrOfBoxes = _dawnClient.DawnWorld.GetEntities().Where(e => e.Specy == EntityType.Box).Count().ToString();
        _debugInfoNrOfPredators = _dawnClient.DawnWorld.GetEntities().Where(e => e.Specy == EntityType.Predator).Count().ToString();
        _debugInfoNrOfPredators2 = _dawnClient.DawnWorld.GetEntities().Where(e => e.Specy == EntityType.Predator2).Count().ToString();


        // Update nodes & create new nodes
        var currentEntities = new HashSet<int>();

        var entities = _dawnClient.DawnWorld.GetEntities();
        foreach (var current in entities)
        {
            EntityToNode(current, currentEntities);
        }

        // TODO: move to EntityScript
        // Remove destroyed entities
        var entitiesCopy = _entities.ToList();
        foreach (var entity in entitiesCopy)
        {
            if (!currentEntities.Contains(entity.Key))
            {
                var position = entity.Value.position;
                CreateExplosion(position);

                _entities.Remove(entity.Key);
                Destroy(entity.Value.gameObject);

            }
        }
	}

    void OnGUI()
    {
        if (!Application.isEditor)  // or check the app debug flag
            return;

        if (_connectError)
        {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Connect Error!");
            return;
        }

        if (!_dawnClient.WorldLoaded)
        {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "WorldLoading: " + _debugLoadingCounter);
            return;
        }

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "#walls: " + _debugInfoNrOfWalls);
        GUI.Label(new Rect(0, 10, Screen.width, Screen.height), "#boxes: " + _debugInfoNrOfBoxes);
        GUI.Label(new Rect(0, 20, Screen.width, Screen.height), "#predators: " + _debugInfoNrOfPredators);
        GUI.Label(new Rect(0, 30, Screen.width, Screen.height), "#predators2: " + _debugInfoNrOfPredators2);
    }

    
    private Dictionary<int, Transform> _entities = new Dictionary<int, Transform>();

    private Transform GetTemplate(EntityType entityType)
    {
        Transform template = null;
        switch (entityType)
        {
            case EntityType.Avatar:
                template = AvatarTemplate;
                break;
            case EntityType.Wall:
                template = WallTemplate;
                break;
            case EntityType.Box:
                template = BoxTemplate;
                break;
            case EntityType.Predator:
                template = PredatorTemplate;
                break;
            case EntityType.Predator2:
                template = Predator2Template;
                break;
            case EntityType.SpawnPoint1:
                template = SpawnPoint1Template;
                break;
            case EntityType.SpawnPoint2:
                template = SpawnPoint2Template;
                break;
            case EntityType.Treasure:
                template = TreasureTemplate;
                break;
            case EntityType.Bullet:
                template = BulletTemplate;
                break;
            case EntityType.Rocket:
                template = RocketTemplate;
                break;
        }

        return template ?? DefaultTemplate;
    }

    private static double RadianToDegree(double angle)
    {
        return angle * (180.0 / Math.PI);
    }

    private void EntityToNode(DawnClientEntity entity, HashSet<int> currentEntities)
    {
        if (!_entities.ContainsKey(entity.Id))
        {
            var template = GetTemplate(entity.Specy);
            var node = SpawnObject(entity, template);
            _entities.Add(entity.Id, node);
        }

        if (currentEntities != null)
        {
            currentEntities.Add(entity.Id);
        }
    }

    private Transform CreateExplosion(Vector3 position)
    {
        Transform newObj = (Transform)Instantiate(ExplosionTemplate, position, transform.rotation);

        return newObj;
    }

    private Transform SpawnObject(DawnClientEntity entity, Transform template)
    {
        // Invert z-axis = convert left-handed to right-handed orientation
        Vector3 position = new Vector3(entity.PlaceX, 0, -entity.PlaceY);

        //Quaternion rotation = Quaternion.A;
        //Quaternion angle = Quaternion.AngleAxis((float) RadianToDegree(entity.Angle), new Vector3(0, 1, 0));
        Transform newObj = (Transform)Instantiate(template, position, transform.rotation);
        newObj.eulerAngles = new Vector3(0, (float)RadianToDegree(entity.Angle), 0);

        // Set color
        if (entity.SpawnPointId != 0)
        {
            var body = newObj.FindChild("Body");
            if (body != null && body.renderer != null)
            {
                body.renderer.material.color = GetFamilyMaterial(entity);
            }
        }

        // Entity
        {
            var entityScript = newObj.GetComponent("EntityScript") as EntityScript;
            if (entityScript != null)
            {
                entityScript.Entity = entity;
            }
        }

        // Creature
        {
            var creatureScript = newObj.GetComponent("CreatureScript") as CreatureScript;
            if (creatureScript != null)
            {
                creatureScript.Entity = entity;
            }
        }

        // Avatar
        if (entity.Specy == EntityType.Avatar)
        {
            var avatarControlScript = newObj.GetComponent("AvatarScript") as AvatarScript;
            if (avatarControlScript != null)
            {
                avatarControlScript.DawnClient = _dawnClient;
                avatarControlScript.Avatar = entity;
            }
        }

        return newObj;
    }

    private static Dictionary<int, Color> _familyColorMapper = new Dictionary<int, Color>();

    private static Color GetFamilyMaterial(DawnClientEntity entity)
    {
        if (entity.SpawnPointId == 0)
            return Color.white;

        Color color;
        if (!_familyColorMapper.TryGetValue(entity.SpawnPointId, out color))
        {
            color = new Color(
                Random.Range(0f, 255f)/255f,
                Random.Range(0f, 255f)/255f,
                Random.Range(0f, 255f)/255f);

            _familyColorMapper.Add(entity.SpawnPointId, color);
        }

        return color;
    }

}
