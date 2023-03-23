using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stryker_Weapons : MonoBehaviour {

	[Header("Stryker Main Gun")]
	public PhotonView Stryker_Photonview;
	public GameObject Main_Projectile;
	public Transform Stryker_Main_Barrel;
	public ParticleSystem Stryker_Muzzleflash_Main;
	public AudioSource Stryker_Main_Gun_Audiosource;
	public AudioClip[] Stryker_Main_Gun_Sounds;
	private float ShootTimer_Main;
	private float FireRate_Main = 0.5f;		//Variable to control the firerate
	[Header("Stryker Secondary Gun")]
	public Transform Stryker_Sec_Barrel;
	public ParticleSystem Stryker_Muzzleflash_Sec;
	private float ShootTimer_Sec;
	//private float FireRate_Sec = 0.1f;		//Variable to control the firerate

	void OnEnable()
	{
		Stryker_Photonview = GetComponent<PhotonView> ();
		PlayerInputManager.instance.IsAuto = true;
	}

	void Update()
	{
		HandlePlayerInput ();
	}

	void HandlePlayerInput()
	{
		if (PlayerInputManager.instance.Attack) {
			Stryker_Main_Gun ();
		}
		//if (PlayerInputManager.instance.Aim) {
		//	Stryker_Main_Gun ();
		//}
	}

	public void Stryker_Main_Gun()
	{
		if (ShootTimer_Main <= Time.time) {						//If we can shoot rightnow according the firerate
			ShootTimer_Main = Time.time + FireRate_Main;		//Set the timer again
			PhotonNetwork.Instantiate (Main_Projectile.name, Stryker_Main_Barrel.position, Stryker_Main_Barrel.rotation, 0);
			Stryker_Photonview.RPC ("Stryker_Main_Gun_Visuals", PhotonTargets.All, null);
		}
	}

	[PunRPC]
	public void Stryker_Main_Gun_Visuals()
	{
		if (Stryker_Muzzleflash_Main != null) {
			Stryker_Muzzleflash_Main.Play ();
		}
		if (Stryker_Main_Gun_Audiosource != null) {
			Stryker_Main_Gun_Audiosource.PlayOneShot (Stryker_Main_Gun_Sounds[0]);
		}
	}
}
