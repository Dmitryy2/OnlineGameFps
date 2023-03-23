using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterTrigger : MonoBehaviour {


	void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			other.SendMessage("PlayerEnterWater", SendMessageOptions.DontRequireReceiver);
		}
	}

	void OnTriggerExit(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			other.SendMessage("PlayerExitWater", SendMessageOptions.DontRequireReceiver);
		}
	}
}
