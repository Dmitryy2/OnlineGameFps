using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMap_Global : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameManager.instance.AmbientPlay (1, true, 0.15f);
	}
}
