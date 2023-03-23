using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyElementAfterTime : MonoBehaviour {

	public float UIElementDestroyTimer = 2f;

	void Start()
	{
		StartCoroutine (DestroyUIElement ());
	}

	IEnumerator DestroyUIElement()
	{
		yield return new WaitForSeconds (UIElementDestroyTimer);
		Destroy (this.gameObject);
	}
}
