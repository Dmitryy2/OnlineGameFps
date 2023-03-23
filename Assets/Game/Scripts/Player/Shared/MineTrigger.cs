using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineTrigger : MonoBehaviour {

	void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			other.SendMessage("PlayerLandMine",SendMessageOptions.DontRequireReceiver);
		}
	}
}
