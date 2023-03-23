using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerThirdPersonController : MonoBehaviour {

	[Header("Third Person References")]
	public PlayerMovementController PlayerMovementManager;		//Reference to the PlayerMovementManager
	public PhotonView ThirdPersonPhotonView;					//Reference to this playermodel's photonview
	public Animator PlayerThirdPersonAnimator;					//Reference to the animator of this playermodel
	public Rigidbody[] PlayerThirdPersonRigidbodies;			//Get all rigidbodies from the thirdperson model
	public Collider[] PlayerThirdPersonColliders;				//Get all colliders from the thirdperson model
	public Renderer[] ThirdPersonRenderers;						//Get all renderers from the thirdperson model
	public Transform ThirdPersonWeaponHolder;					//Get the transform where all thirdpersonweapon models are being spawned and attached to
	public Transform PlayerCamera;								//Reference to this player's camera
	public GameObject ThirdPersonWorldWeapon;					//Referene to the current thirdpersonweapon model
	public float SyncedAimangle;								//Used to smooth out the aim angle on remote players
	public bool WeaponIK = false;								//Used to determine if the left and right hand's IK need to be enabled
	public Transform LeftHand;									//The transform of the bone from the left hand
	public Vector3 Offset;										//Offset used for the lefthand position
	public bool UseTarget = true;
	public Transform Target;
	public Transform Chest;
	public Vector3 TargetDefaultValues;
	public float TargetStartHeight = 0.5f;
	public float PlayerPoseModifier = 0.3f;
	public Rigidbody ThirdPersonModelRigidbody;
	public Collider ThirdPersonModelCollider;
	public bool PlayDrawAnimation = false;						//This is a once set bool, so the remote players dont play their draw animation when I just connected to the game or when they spawn

	void Start()
	{
		if (ThirdPersonPhotonView.isMine) {						//If this photonview belows to this player instance
			ShowPlayerModel (false);							//Hide the playermodel
			SetPlayerModelColliders (false);					//Disable the thirdperson model collisions on the local player
		}
	}

	void Update()
	{
		if (ThirdPersonPhotonView.isMine) {
			Vector3 TargetHeight = new Vector3 (0f, this.PlayerThirdPersonAnimator.GetFloat ("Aim") + TargetStartHeight + PlayerPoseModifier, 0f);
			Target.position = TargetHeight;
		}
	}

	void LateUpdate()
	{
		HandleThirdPersonAiming ();	
	}

	public void SetPlayerModelColliders(bool Toggle)
	{
		foreach (Collider PlayerCollider in PlayerThirdPersonColliders) {
			PlayerCollider.isTrigger = false;
			PlayerCollider.enabled = Toggle;
		}

		foreach (Rigidbody PlayerRigidbody in PlayerThirdPersonRigidbodies) {
			PlayerRigidbody.isKinematic = !Toggle;
		}
	}

	public void ShowPlayerModel(bool Toggle)
	{
		foreach(Renderer ThirdPersonRenderer in ThirdPersonRenderers) {
			ThirdPersonRenderer.enabled = Toggle;
		}
		if (ThirdPersonWorldWeapon != null) {
			ThirdPersonWorldWeapon.SetActive (Toggle);
		}
	}

	public void ThirdPersonPlayerKilled()		//Local function when the player dies
	{
		if (!GameManager.instance.InVehicle) {
			PlayerCamera.SetParent (PlayerThirdPersonColliders [7].transform);			//Attach the playercamera to the head bone of the playermodel
			ThirdPersonPhotonView.RPC ("OnThirdPersonDeath", PhotonTargets.All, null);	//Send an rpc to tell everyone this playermodel is now a ragdoll
		}
	}

	[PunRPC]
	public void SetThirdPersonWeapon(int WeaponID)
	{
		EnableWeaponIK (false);
		if (PlayDrawAnimation) {
			PlayerThirdPersonAnimator.SetTrigger ("DrawWeapon");
		} else if (!PlayDrawAnimation) {
			EnableWeaponIK (true);
			PlayDrawAnimation = true;
		}
		if (ThirdPersonWorldWeapon != null) {
			Destroy (ThirdPersonWorldWeapon);
			ThirdPersonWorldWeapon = Instantiate (GameManager.instance.AllGameWeapons[WeaponID].ThirdPersonPrefab, ThirdPersonWeaponHolder);
			PlayerThirdPersonAnimator.SetInteger ("WeaponType", ThirdPersonWorldWeapon.GetComponent<ThirdPersonWeapon> ().WeaponHoldType);
			if (ThirdPersonPhotonView.isMine)
				ThirdPersonWorldWeapon.SetActive (false);
		} else {
			ThirdPersonWorldWeapon = Instantiate (GameManager.instance.AllGameWeapons[WeaponID].ThirdPersonPrefab, ThirdPersonWeaponHolder);
			PlayerThirdPersonAnimator.SetInteger ("WeaponType", ThirdPersonWorldWeapon.GetComponent<ThirdPersonWeapon> ().WeaponHoldType);
			if (ThirdPersonPhotonView.isMine)
				ThirdPersonWorldWeapon.SetActive (false);
		}
	}
		

	[PunRPC]
	public void ThirdPersonReload()
	{
		PlayerThirdPersonAnimator.SetBool ("Reloading", true);
		EnableWeaponIK (false);
	}

	[PunRPC]
	public void FinishReload()
	{
		PlayerThirdPersonAnimator.SetBool ("Reloading", false);
		EnableWeaponIK (true);
	}

	public void EnableWeaponIK(bool Toggle)
	{
		ThirdPersonPhotonView.RPC ("ThirdPersonEnableWeaponIK", PhotonTargets.AllBuffered, Toggle);
	}

	[PunRPC]
	public void ThirdPersonEnableWeaponIK(bool Toggle)
	{
		this.WeaponIK = Toggle;
	}

	[PunRPC]
	public void ThirdPersonFireWeapon()
	{
		ThirdPersonWeapon TpWeapon = ThirdPersonWorldWeapon.GetComponent<ThirdPersonWeapon> ();
		PlayerThirdPersonAnimator.SetTrigger ("Shoot");
		if(TpWeapon.WeaponFireSound.Length != 0)
		{
			TpWeapon.MuzzleFlash.Play ();
			TpWeapon.ThirdPersonAudioSource.PlayOneShot (TpWeapon.WeaponFireSound [0]);
		}
		if (TpWeapon.WeaponFireLoopSound != null) {
			TpWeapon.MuzzleFlash.Play ();
			TpWeapon.ThirdPersonAudioSource.loop = true;
			TpWeapon.ThirdPersonAudioSource.clip = TpWeapon.WeaponFireLoopSound; //Insert the looped fire animation
			TpWeapon.ThirdPersonAudioSource.Play();	//Play the weapon shoot animation
		}
	}

	[PunRPC]
	public void OnThirdPersonDeath()
	{
		//SetPlayerModelColliders (true);
		//PlayerThirdPersonAnimator.enabled = false;
		EnableWeaponIK(false);
		UseTarget = false;
		PlayerThirdPersonAnimator.SetLayerWeight (1, 0f);
		PlayerThirdPersonAnimator.SetLayerWeight (2, 0f);
		PlayerThirdPersonAnimator.SetLayerWeight (3, 0f);
		PlayerThirdPersonAnimator.SetTrigger("Death");
		ThirdPersonModelCollider.enabled = true;
		ThirdPersonModelRigidbody.isKinematic = false;
	}

	void OnAnimatorIK(int layer)
	{
		if (WeaponIK) {
			PlayerThirdPersonAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
			PlayerThirdPersonAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
			PlayerThirdPersonAnimator.SetIKPosition(AvatarIKGoal.LeftHand, ThirdPersonWorldWeapon.GetComponent<ThirdPersonWeapon>().LeftHandTransform.position);
			PlayerThirdPersonAnimator.SetIKRotation(AvatarIKGoal.LeftHand, ThirdPersonWorldWeapon.GetComponent<ThirdPersonWeapon>().LeftHandTransform.rotation);
		}
	}

	public void HandleThirdPersonAiming()
	{
		if (PlayerThirdPersonAnimator.GetBool("Sprinting") == false && UseTarget == true) {
			Chest.LookAt (Target);
			Chest.rotation = Chest.rotation * Quaternion.Euler (Offset);
		}
		if (ThirdPersonPhotonView.isMine) {	//If this is our instance of the game
			float AimAngle = PlayerCamera.transform.localRotation.x;	//Get get our current local aimangle
			PlayerThirdPersonAnimator.SetFloat ("Aim", AimAngle * -1.5f); //Set the float AimAngle in the animator on our player instance
		}
		if (!ThirdPersonPhotonView.isMine) { //If this is a other player
			PlayerThirdPersonAnimator.SetFloat ("Aim", Mathf.Lerp (PlayerThirdPersonAnimator.GetFloat ("Aim"), SyncedAimangle, 0.05f)); //Lerp the aimangle so it looks smooth on our instance
		}
	}

	public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) { //If we are sending this information
			stream.SendNext (PlayerThirdPersonAnimator.GetFloat ("Aim")); //Send the aimangle from the animator
		}
		else if (stream.isReading) { //If we are receiving this information
			SyncedAimangle = (float)stream.ReceiveNext();	//Set the syncedaimangle from this playermodel to the information we just received from the client this playermodel belongs to
		}
	}
}
