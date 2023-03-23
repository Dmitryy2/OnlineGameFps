using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemWorkShop : MonoBehaviour {

	public GameObject Weapon1;
	public GameObject Weapon2;
	public GameObject Weapon3;

	void Update()
	{
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			Weapon1.SetActive (true);
			Weapon2.SetActive (false);
			Weapon3.SetActive (false);
		}
		if (Input.GetKeyDown (KeyCode.Alpha2)) {
			Weapon1.SetActive (false);
			Weapon2.SetActive (true);
			Weapon3.SetActive (false);
		}
		if (Input.GetKeyDown (KeyCode.Alpha3)) {
			Weapon1.SetActive (false);
			Weapon2.SetActive (false);
			Weapon3.SetActive (true);
		}

		if (Input.GetKeyDown (KeyCode.Escape)) {
			SceneManager.LoadScene ("MainMenu");
		}
	}
}
