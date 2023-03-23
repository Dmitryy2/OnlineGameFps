using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class VehicleManager : MonoBehaviour {

	public static VehicleManager instance;
	public List<VehicleStats> CurrentVehicles = new List<VehicleStats>();
	public const string VehicleCurrentAmountProp = "currentveh";
	public const string VehicleMaxAmountProp = "maxveh";
	public string VehicleEnterHinstring;
	public GameObject[] VehicleSpawns;

	void Awake()
	{
		instance = this;
	}

	public void Init()
	{
		VehicleSpawns = GameObject.FindGameObjectsWithTag ("VehicleSpawn");
		for (int i = 0; i < VehicleSpawns.Length; i++) {
			int RandomVehicle = Random.Range (0, GameManager.instance.VehiclePrefabs.Length);
			SpawnVehicle (i, RandomVehicle);
		}
	}

	public void SpawnVehicle(int index, int vehicletype)
	{
		GameObject SpawnedVehicle = PhotonNetwork.InstantiateSceneObject (GameManager.instance.VehiclePrefabs[vehicletype].name, VehicleSpawns [index].transform.position, VehicleSpawns [index].transform.rotation, 0, null);
		VehicleStats stats = SpawnedVehicle.GetComponent<VehicleStats> ();
		CurrentVehicles.Add (stats);
	}
}

public static class VehicleExtensions
{
	public static void SetMaxVehicle(this VehicleManager manager, int newMaxVehicles)
	{
		Hashtable veh = new Hashtable();  // using PUN's implementation of Hashtable
		veh[VehicleManager.VehicleMaxAmountProp] = newMaxVehicles;

		PhotonNetwork.room.SetCustomProperties (veh);  // this locally sets the maxvehicles and will sync it in-game asap.
	}

	public static void AddCurrentVehicle(this VehicleManager manager)
	{
		int current = VehicleManager.instance.GetCurrentVehicleAmount();
		current = current++;

		Hashtable newveh = new Hashtable();  // using PUN's implementation of Hashtable
		newveh[VehicleManager.VehicleCurrentAmountProp] = current;

		PhotonNetwork.room.SetCustomProperties(newveh);  // this locally sets the kills and will sync it in-game asap.
	}

	public static void RemoveCurrentVehicle(this VehicleManager manager)
	{
		int current = VehicleManager.instance.GetCurrentVehicleAmount();
		current = current--;

		Hashtable newveh = new Hashtable();  // using PUN's implementation of Hashtable
		newveh[VehicleManager.VehicleCurrentAmountProp] = current;

		PhotonNetwork.room.SetCustomProperties(newveh);  // this locally sets the kills and will sync it in-game asap.
	}

	public static int GetCurrentVehicleAmount(this VehicleManager manager)
	{
		object currentveh;
		if (PhotonNetwork.room.CustomProperties.TryGetValue(VehicleManager.VehicleCurrentAmountProp, out currentveh))
		{
			return (int) currentveh;
		}
		return 0;
	}

	public static int GetMaxVehicleAmount(this VehicleManager manager)
	{
		object currentveh;
		if (PhotonNetwork.room.CustomProperties.TryGetValue(VehicleManager.VehicleMaxAmountProp, out currentveh))
		{
			return (int) currentveh;
		}
		return 0;
	}
}	
