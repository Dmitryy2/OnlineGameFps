using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtTrigger : MonoBehaviour {

	public int DamagePerTick = 999;

	void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Player"))
		{
			other.SendMessage("PlayerHurtTrigger", DamagePerTick ,SendMessageOptions.DontRequireReceiver);
		}
	}
}
