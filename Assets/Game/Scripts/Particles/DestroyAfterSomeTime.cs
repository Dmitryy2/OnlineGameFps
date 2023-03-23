using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterSomeTime : MonoBehaviour {

	public float TimeBeforeDestroy = 5f;
	// Use this for initialization
	void Start () {
		StartCoroutine (DestroyAfterTime (TimeBeforeDestroy));
	}
		
	IEnumerator DestroyAfterTime(float DestroyTime)
	{
		yield return new WaitForSeconds (DestroyTime);
		Destroy (this.gameObject);
	}
}
