using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAbleObjectManager : MonoBehaviour {

	public static DestroyAbleObjectManager instance;
	public GameObject[] AvailableProps;
	public List<GameObject> CurrentDestroyAbleObjects = new List<GameObject>();

	void Start()
	{
		instance = this;
	}

	public void Init()
	{
		SpawnObject (0, new Vector3 (-7f, 1f, 0f), Quaternion.identity);
	}

	public void SpawnObject(int index, Vector3 position, Quaternion rotation)
	{
		GameObject SpawnedObject = PhotonNetwork.InstantiateSceneObject (AvailableProps[index].name, position, rotation, 0, null);
		CurrentDestroyAbleObjects.Add (SpawnedObject);
	}
}
