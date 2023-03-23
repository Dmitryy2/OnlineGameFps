using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponManager : MonoBehaviour {

	[Header("Player Movement Controllers")]
	public CharacterController PlayerCharacterController;		//Reference to the playercharacter controller
	public PlayerMovementController PlayerMovementController;	//Reference to the playermovement controller
	public PlayerThirdPersonController ThirdPersonController;	//Reference to the thirdperson controller
	public PhotonView PlayerPhotonView;		//The photonview attached to this player object
	public AudioSource WeaponAudioSource;	//The audiosource of the weapon

	[Header("Player Weapon Movement")]
	public Animator WeaponMovementAnimator;						//Reference to the weapon animator
	public Transform WeaponRecoilHolder;
	public Transform WeaponOffsetTransform;

	[Header("Player Weapon Inventory")]
	public List<GameObject> PlayerInventory = new List<GameObject>();	//A list which holds all weapons we currently have | Is going to be replaced with something more efficient and easier to use
	public Transform WeaponHolder;										//Reference to the weaponholder were all inventory weapons are being spawned
	[HideInInspector]public GameObject CurrentWeapon;					//The current weapon
	[HideInInspector]public int CurrentWeaponInt = 1;					//The current index of the inventory we currently are at
	[HideInInspector]public Animation CurrentWeaponAnimationComponent;	//The Animation component of the current weapon we are holding
	[HideInInspector]public AnimationClip CurrentWeaponReload;			//The reload animation component of the current weapon we are holding
	[HideInInspector]public AnimationClip CurrentWeaponDraw;			//The draw animation component of the current weapon we are holding

	void Update()
	{
		//WeaponMovementAnimationController ();						//Controls the weapon movement animations
		SwitchController ();										//Cycle through all weapons when the change weapon button is pressed
	}

	void WeaponMovementAnimationController() {
		if (PlayerInputManager.instance.FreezePlayerControls == false) {
			if (PlayerMovementController.WasStanding && !PlayerCharacterController.isGrounded) { //If we were standing and we arent grounded anymore
				PlayerMovementController.WasStanding = false;								
				//If you have a weapon jump animation / sound you can put it here
			} else if (!PlayerMovementController.WasStanding && PlayerCharacterController.isGrounded) { //If we werent standing and we grounded now
				PlayerMovementController.WasStanding = true;
			}
			if (PlayerCharacterController.isGrounded && PlayerMovementController.PlayerMovementVelocity > 0f) { //If we are grounded and our velocity in any direction is greater than 0
				if (PlayerMovementController.PlayerWalkingState == WalkingState.Running) {	//If the current walking state is running
					WeaponMovementAnimator.SetFloat ("Movement", 1f, 0.2f, Time.deltaTime);	//Set the animator blend tree float to 1f so it plays the running animation
				} else if (PlayerMovementController.PlayerWalkingState == WalkingState.Walking) { //If the current walking state is walking
					WeaponMovementAnimator.SetFloat ("Movement", 0.5f, 0.2f, Time.deltaTime); //Set the animator blend tree float to 0.5f so it plays the walking animation
				}
			} else {		//If we arent grounded and/or velocity is zero
				WeaponMovementAnimator.SetFloat ("Movement", 0f, 0.2f, Time.deltaTime); //Set the animator blend tree float to 0f so it plays the idle animation
			}
		} else {
			WeaponMovementAnimator.SetFloat ("Movement", 0f, 0.2f, Time.deltaTime); //Set the animator blend tree float to 0f so it plays the idle animation
		}
	}

	#region PlayerWeapon
	public void GiveWeapon(int WeaponID)
	{
		if (GameManager.instance.AllGameWeapons [WeaponID] != null && !GameManager.instance.NotAllowedWeapons.Contains(WeaponID)) {			//If the given weaponID has a weapon associated with it and that it is not on the NotAllowedWeaponslist
			GameObject newWep = Instantiate (GameManager.instance.AllGameWeapons[WeaponID].FirstPersonPrefab, WeaponHolder) as GameObject;  //Instantiate the weapon in the weaponholder transform
			PlayerInventory.Add (newWep);																									//Tell the playerinventory list to add the weapon to the list
			newWep.SetActive (false);																										//Hide the weapon by default
		} else if(GameManager.instance.AllGameWeapons [WeaponID] == null){																	//If there is no weapon associated with the weaponid
			Debug.LogError ("Cant find: unknown of type weaponfile with that name!");														//Display an error
		} else if(GameManager.instance.NotAllowedWeapons.Contains(WeaponID)){																//If the weaponid is in the notallowedweaponslist
			Debug.LogError ("This weapon is disabled by the server!");																		//Display an error
		}
	}

	public void SwitchController()		//Makes it possible for the player to switch weapons using a certain set key
	{
		if (PlayerInputManager.instance.NextWeapon && !CurrentWeaponAnimationComponent.IsPlaying(CurrentWeaponDraw.name) && PlayerInputManager.instance.Aim == false && PlayerInventory.Count > 1) {	//If we press 1 and we arent reloading or draw a weapon
			if (CurrentWeaponInt == PlayerInventory.Count - 1) {						//If the index of the playerinventory is equal to the length of the playerinventory minus 1
				CurrentWeaponInt = 0;													//Set the index back to zero
			} else 
			{
				CurrentWeaponInt++;														//Add 1 to the index
			}
			if (CurrentWeaponAnimationComponent.IsPlaying (CurrentWeaponReload.name)) {	//If we are playing the reload animation
				CancelInvoke ();														//Stop the reload sequence on the weapon
			}
			EquipWeapon (CurrentWeaponInt);												//Equip the weapon in weaponslot index
		}
	}

	public void TakeWeapon(int WeaponSlot)												//Removes a weapon from the inventory
	{
		if (PlayerInventory [WeaponSlot] != null) {
			DisableAllWeapons ();															//Hides all weapons
			Destroy (PlayerInventory [WeaponSlot]);											//Destroy the weapon in the specific weaponslot
			PlayerInventory.RemoveAt (WeaponSlot);											//Remove the weapon from the inventory
		}
	}

	void DisableAllWeapons()
	{
		foreach (GameObject Weapon in PlayerInventory) {								//For each weapon object in our player inventory
			Weapon.SetActive (false);													//Hide the weapon object
		}
	}

	public void EquipWeapon(int WeaponSlot)
	{
		//These 3 lines are only usable when using the old playerweapon system
		//if (CurrentWeapon != null) {													
		//	CurrentWeapon.GetComponent<PlayerWeapon> ().StopWeapon ();
		//}
		CurrentWeaponInt = WeaponSlot;													//Set the currentweaponint to the weaponslot variable (so you can set the EquipWeaponfunction without the switchcontroller)
		DisableAllWeapons ();															//Hide all weapons
		CurrentWeapon = PlayerInventory [WeaponSlot];									//Set the currentweapon to the weapon in the selected weaponslot
		CurrentWeapon.SetActive (true);													//Show the weapon we want to equip
		WeaponOffsetTransform.localPosition = CurrentWeapon.GetComponent<PlayerWeapon>().WeaponOffset;			//Set a offset to the weapon position
		CurrentWeaponAnimationComponent = CurrentWeapon.GetComponent<PlayerWeapon> ().WeaponAnimationComponent;	//Get the animation component of the currentweapon
		CurrentWeapon.GetComponent<PlayerWeapon>().Bursting = false;
		CurrentWeaponReload = CurrentWeapon.GetComponent<PlayerWeapon> ().WeaponReloadAnimation;				//Get the reload animation of the currentweapon
		CurrentWeaponDraw = CurrentWeapon.GetComponent<PlayerWeapon> ().WeaponDrawAnimation;					//Get the draw animation of the currentweapon
		WeaponAudioSource.PlayOneShot(CurrentWeapon.GetComponent<PlayerWeapon> ().WeaponDrawSound, 0.05f);
		ThirdPersonController.ThirdPersonPhotonView.RPC("SetThirdPersonWeapon", PhotonTargets.AllBuffered ,CurrentWeapon.GetComponent<PlayerWeapon> ().WeaponID); //Send an rpc to everyone so the thirdperson model while preform a weapon switch animation
	}
	#endregion
}

