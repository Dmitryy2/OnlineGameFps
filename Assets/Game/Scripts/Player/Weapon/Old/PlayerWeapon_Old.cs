using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon_Old : MonoBehaviour {

	[Header("Weapon Stats")]
	public string WeaponName;					//The name of the weapon
	public int WeaponID;						//The id of the weapon
	public int WeaponMinDamage;					//The minimum damage of the weapon
	public int WeaponMaxDamage;					//The maximum damage of the weapon
	public float WeaponFireRate = 0.1f;			//The firerate of the weapons
	public float WeaponQuickDrawTime = 0.75f;	//The draw time of the weapon
	public int CurrentMagazineAmmo;				//The currentmagazine capacity
	public int CurrentClips;					//The current amount of clips we have 
	public int MaxAmmoPerClip = 30;				//The max amount of bullets in a clip
	public int MaxWeaponClips = 4;				//The max amout of clips we can have for this weapons
	public float WeaponMaxRange;				//The max range of this weapons
	public float WeaponDropOffRange;			//The range the weapon's damage drops [Not yet implemented]
	public WeaponAttachment WeaponAttachments;	//All attachments available to the weapon
	public WeaponFireType WeaponFiringType;		//The weapons firing type
	public BulletType WeaponBulletType;			//Sets what the bullet type the weapon fires [only bullet is available for now]
	public int ExplosionRadius = 5;				//The radius of the explosion we get when using explosive ammo
	public bool IsAiming;						//Are we currently aiming the weapon
	public bool LoopedFire = false;				//Is this weapon's fire a looped process?
	public bool IsFireLoop = false;				//Are we currently firing in looped manner?
	public LayerMask WeaponLayerMask;			//The layers we are ignoring when we hit an surface
	public Vector3 RecoilRotation;
	public Vector3 RecoilKickback;

	[Header("WeaponComponents")]
	public Transform WeaponBarrel;				//The transform from which we shoot
	public ParticleSystem MuzzleFlash;			//The muzzleflash particle
	public ParticleSystem Tracer; 				//[Beta]: It works, but it still has some glitches
	public ParticleSystem ShellEject;			//The shelleject particle
	public Vector3 WeaponOffset;				//The position offset of the weapon (only viewmodel)
	public Renderer[] WeaponRenderers;			//All renderers of this weapon

	[Header("Weapon Animations")]
	public Animation WeaponAnimationComponent;	//The attachted animation component
	public AnimationClip WeaponIdle;			//The weapons idle animation
	public AnimationClip WeaponDraw;			//The weapons Draw animation
	public AnimationClip WeaponFire;			//The weapons Fire animation
	public AnimationClip WeaponFireLoop;		//The looped version of the fire animation
	public AnimationClip WeaponReload;			//The weapons Reload animation


	[Header("Weapon Sounds")]
	public AudioSource WeaponAudioSource;		//The audiosource which all weapon sounds play from, this is set by the PlayerWeaponManager
	public AudioClip WeaponDrawSound;			//The draw sound of the weapon [Not yet implemented]
	public AudioClip[] WeaponFireSounds;		//The Fire sounds of the weapon
	public AudioClip WeaponFireLoopSound;		//The looped fire sound of the weapon
	public AudioClip WeaponReloadSound;			//The reload sound of the weapon

	//Private variables
	private PlayerMovementController PlayerMovementManager;	//Reference to the playermovmentmanager
	private float ShootTimer = 0;							//Time when the previous bullet has been shot, used to control the firerate
	private Vector3 CurrentRecoil1;
	private Vector3 CurrentRecoil2;
	private Vector3 CurrentRecoil3;
	private Vector3 CurrentRecoil4;
	private Transform WeaponHolder;							//The transform which holds this weapon
	private Camera PlayerCamera;							//Reference to the player camera

	void Start()
	{
		WeaponHolder = transform.parent;					//Set the weaponholder to this transforms parent
		PlayerCamera = transform.root.GetComponentInChildren<Camera> ();	//Get the playercamera
		PlayerMovementManager = transform.root.GetComponent<PlayerMovementController> (); //Get the playermovementmanager
		WeaponAudioSource = transform.parent.GetComponent<AudioSource> ();					//Get the weaponaudiosource
		if (WeaponFireLoopSound != null) {
			WeaponAudioSource.clip = WeaponFireLoopSound;
		}
	}

	void Update()
	{
		SetFiringMode (WeaponFiringType);			//Set the firing mode
		GetPlayerInput ();							//Functions which controls the input regarding the weapon
		UpdateWeaponHud ();							//Display the weapon statistics to the HUD
		RecoilController();							//Controls the random recoil of the weapon
	}		

	void GetPlayerInput()
	{
		if (PlayerInputManager.instance.Attack && !WeaponAnimationComponent.IsPlaying (WeaponReload.name) && !WeaponAnimationComponent.IsPlaying (WeaponDraw.name) && CurrentMagazineAmmo > 0 && PlayerMovementManager.PlayerWalkingState != WalkingState.Running) {
			FireWeapon ();
		} else if(!WeaponAnimationComponent.IsPlaying(WeaponDraw.name)){
			StopWeapon ();
		}
		if (PlayerInputManager.instance.Reload && CurrentClips >= 1 || CurrentMagazineAmmo == 0 && CurrentClips >= 1) {
			ReloadWeapon ();
		}
		if (PlayerInputManager.instance.Aim && !WeaponAnimationComponent.IsPlaying (WeaponReload.name) && PlayerMovementManager.PlayerWalkingState != WalkingState.Running) {
			AimWeapon (true);
			PlayerInputManager.instance.AimSpeedModifier = WeaponAttachments.WeaponScopes [0].ScopeAimSpeedModifier;
		} else {
			AimWeapon (false);
			PlayerInputManager.instance.AimSpeedModifier = 1f;
		}
	}
	#region WeaponFiringBehaviour
	void SetFiringMode(WeaponFireType FireMode)
	{
		if (FireMode == WeaponFireType.Automatic && !PlayerInputManager.instance.FreezePlayerControls)
			PlayerInputManager.instance.Attack = Input.GetKey (KeyCode.Mouse0);
		if(FireMode == WeaponFireType.SemiAutomatic && !PlayerInputManager.instance.FreezePlayerControls)
			PlayerInputManager.instance.Attack = Input.GetKeyDown (KeyCode.Mouse0);
	}

	void FireWeapon()
	{	
		if (ShootTimer <= Time.time) {						//If we can shoot rightnow according the firerate
			ShootTimer = Time.time + WeaponFireRate;		//Set the timer again

			CurrentRecoil1 += new Vector3(RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y));
			CurrentRecoil3 += new Vector3(Random.Range(-RecoilKickback.x, RecoilKickback.x), Random.Range(-RecoilKickback.y, RecoilKickback.y), RecoilKickback.z);

			if (WeaponFire != null) {						//If we have an normal fire animation
				WeaponAnimationComponent.CrossFadeQueued (WeaponFire.name, 0.01f, QueueMode.PlayNow);	//Play the weapon shoot animation
			}
			if (WeaponFireLoop != null) {			//If we have an normal fire animation
				WeaponAnimationComponent.clip = WeaponFireLoop; //Insert the looped fire animation
				WeaponAnimationComponent.Play();	//Play the weapon shoot animation
			}
			if (WeaponFireSounds.Length != 0) {					//If the weapon has an fire sound
				WeaponAudioSource.PlayOneShot (WeaponFireSounds[Random.Range (0, WeaponFireSounds.Length - 1)]);	//Play a random fire sound
			}
			if (WeaponFireLoopSound != null && !WeaponAudioSource.isPlaying) {
				WeaponAudioSource.loop = true;
				WeaponAudioSource.Play ();
			}
			if (ShellEject != null) {						//If the weapon has a shelleject particle effect
				ShellEject.Play ();							//Play the effect
			}
			if (MuzzleFlash != null) {						//If the weapon has a muzzleflash particle effect
				MuzzleFlash.Play ();						//Play the effect
			}
			CurrentMagazineAmmo--;							//Deduct 1 weaponammo
			if (!LoopedFire) {
				PlayerMovementManager.PlayerThirdPersonController.ThirdPersonPhotonView.RPC ("ThirdPersonFireWeapon", PhotonTargets.Others, null);	//Send an rpc to all players so that other players can also see the thirdperson model fire the weapon
			} else if(LoopedFire && !IsFireLoop){
				PlayerMovementManager.PlayerThirdPersonController.ThirdPersonPhotonView.RPC ("ThirdPersonFireWeapon", PhotonTargets.Others, null);	//Send an rpc to all players so that other players can also see the thirdperson model fire the weapon
				IsFireLoop = true;
			}
	
			RaycastHit WeaponHit;
			Ray WeaponRay = new Ray (WeaponBarrel.position, WeaponBarrel.TransformDirection (Vector3.forward)); //If you want the bullet to fire from the actual gun barrel
			//Ray WeaponRay = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.TransformDirection(Vector3.forward)); //If you want the bullet to fire from the middle of the screen
			if (Physics.Raycast (WeaponRay, out WeaponHit, WeaponMaxRange, WeaponLayerMask)) {	//Cast out an ray
				OnRayCastHit (WeaponHit, WeaponRay);	//If the raycast hit something
			}
		}
	}

	public void RecoilController()
	{
		CurrentRecoil1 = Vector3.Lerp(CurrentRecoil1, Vector3.zero, 0.1f);
		CurrentRecoil2 = Vector3.Lerp(CurrentRecoil2, CurrentRecoil1, 0.1f);
		CurrentRecoil3 = Vector3.Lerp(CurrentRecoil3, Vector3.zero, 0.1f);
		CurrentRecoil4 = Vector3.Lerp(CurrentRecoil4, CurrentRecoil3, 0.1f);

		//RecoilHolder.localEulerAngles = CurrentRecoil2;
		//RecoilHolder.localPosition = CurrentRecoil4;

		PlayerCamera.transform.localEulerAngles = CurrentRecoil2 / 1.2f;
	}

	public void StopWeapon()
	{
		if (WeaponFireLoopSound != null && WeaponAudioSource.isPlaying) {		//If the is a fireloopsound and we are playing a weapon sound from the weaponaudiosource
			WeaponAudioSource.Stop ();	//Stop the audiosource
		}
		if (WeaponFireLoop != null && WeaponAnimationComponent.IsPlaying(WeaponFireLoop.name)) {	//If we are firing this weapon in a loop and we currently are playing the firing animation
			WeaponAnimationComponent.clip = WeaponDraw;												//Set the default animation back to draw
			WeaponAnimationComponent.Stop ();														//Stop the firing animation
		}
		if (LoopedFire) {																			//If this is a looped weapon
			PlayerMovementManager.PlayerThirdPersonController.ThirdPersonPhotonView.RPC ("ThirdPersonStopWeapon", PhotonTargets.Others, null);	//Disable the weaponfire animation on the third person model
			IsFireLoop = false;																		//Set the IsFireLoop to false 
		}
	}
				
	void OnRayCastHit(RaycastHit WeaponHit, Ray WeaponRay)
	{
		if (WeaponBulletType == BulletType.Bullet) {
			HandleBulletImpact (WeaponHit);
			if (WeaponHit.transform.tag == "PlayerHitbox" && WeaponHit.transform.root.GetComponent<PlayerStats> () != null && WeaponHit.transform.root.GetComponent<PlayerStats> ().isAlive) { //If the object we shot is an player who is also alive
				InGameUI.instance.DoHitMarker ();	//Display an hitmarker
				WeaponHit.transform.root.GetComponent<PhotonView> ().RPC ("ApplyPlayerDamage", PhotonTargets.All, Random.Range (WeaponMinDamage, WeaponMaxDamage), WeaponName, PhotonNetwork.player); //Tell the other player he is been hit
			}
		}
		if (WeaponBulletType == BulletType.ExplosiveBullet) {
			
			if (WeaponHit.transform.tag == "PlayerHitbox" && WeaponHit.transform.root.GetComponent<PlayerStats> () != null && WeaponHit.transform.root.GetComponent<PlayerStats> ().isAlive) { //If the object we shot is an player who is also alive
				InGameUI.instance.DoHitMarker ();	//Display an hitmarker
			}
			HandleExplosiveBulletImpact (WeaponHit);
		}
	}

	#region Normal Bullet Impacts
	void HandleBulletImpact(RaycastHit WeaponHit)
	{
		DoBulletImpactForMaterial (WeaponHit.transform.tag, WeaponHit);	//Do a surface hit at the place the raycast hit
	}

	void DoBulletImpactForMaterial(string Type, RaycastHit WeaponHit)
	{
		if (Type == "Concrete" || Type == "Untagged") {		//If the surface we hit is concrete or has no tag
			//Instantiate (GameManager.instance.SurfaceHits[0].SurfaceParticle , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
		}
		if (Type == "Metal") {								//If the surface we hit is metal
			//Instantiate (GameManager.instance.SurfaceHits[1].SurfaceParticle , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
		}
		if (Type == "PlayerHitbox") {						//If the surface we hit is another player
			//Instantiate (GameManager.instance.SurfaceHits[2].SurfaceParticle , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
		}
		if (Type == "NoDecal") {							//If the surface we hit is tagged with nodecal
			//Dont spawn any decals
		}
	}
	#endregion

	#region Explosive Bullet Impact
	void HandleExplosiveBulletImpact(RaycastHit WeaponHit)
	{
		Collider[] NearObjects = Physics.OverlapSphere (WeaponHit.point, ExplosionRadius);
		foreach (Collider NearObject in NearObjects) {
			if (NearObject.transform.root.GetComponent<PlayerStats> () != null && NearObject.tag == "PlayerSplashHitbox") {
				NearObject.transform.root.GetComponent<PhotonView> ().RPC ("ApplyPlayerDamage", PhotonTargets.All, Random.Range (WeaponMinDamage, WeaponMaxDamage), WeaponName, PhotonNetwork.player); //Tell the other player he is been hit
			}
		}
		//PhotonNetwork.Instantiate (GameManager.instance.SurfaceHits[3].SurfaceParticle.name, WeaponHit.point, Quaternion.identity, 0);
	}

	#endregion
	#endregion

	#region WeaponReloadBehaviour
	public void ReloadWeapon()
	{
		if (!WeaponAnimationComponent.IsPlaying (WeaponReload.name) && WeaponReload != null) {		//If we arent currently playing the reload animation
			PlayerMovementManager.PlayerThirdPersonController.ThirdPersonPhotonView.RPC("ThirdPersonReload", PhotonTargets.All, null); //Send an rpc so the thirdpersonmodel also does an reload animation
			if (WeaponReloadSound != null) {		//If this weapon has a reload sound
				WeaponAudioSource.PlayOneShot (WeaponReloadSound, 1f);	//Play the reload sound
			}
			WeaponAnimationComponent.Play (WeaponReload.name);  //Play the weapon's reload animation
			Invoke ("WaitTillReload", WeaponReload.length);		//Wait till the weapon is done reloading
		}
	}

	void WaitTillReload()
	{
		PlayerMovementManager.PlayerThirdPersonController.ThirdPersonPhotonView.RPC("FinishReload", PhotonTargets.All, null);	//Tell the everyone to stop the reload animation and to enable IK again on this playermodel
		CurrentMagazineAmmo = MaxAmmoPerClip;	//Set the currentclipammo to the max size
		CurrentClips = CurrentClips - 1;		//Remove 1 clip from the total amount of clips we have
	}
	#endregion

	#region WeaponAimBehaviour
	void AimWeapon(bool State)
	{
		if (State) {	//If we are aiming right now
			IsAiming = true;	
			WeaponHolder.localPosition = Vector3.Lerp (WeaponHolder.transform.localPosition, WeaponAttachments.WeaponScopes [0].WeaponScopeOffset, 0.25f);	//Smoothly translate our weaponmodel to the weaponscopeoffset
			PlayerCamera.GetComponent<Camera> ().fieldOfView = Mathf.Lerp (PlayerCamera.fieldOfView, WeaponAttachments.WeaponScopes [0].ScopeFieldOfView, 0.25f); //Smoothly translate our fov to the weaponscope fieldofview
			InGameUI.instance.Crosshair.SetActive (false);	//Hide the crosshair
		} else if (!State) {
			IsAiming = false;
			WeaponHolder.localPosition = Vector3.Lerp(WeaponHolder.localPosition, WeaponOffset, 0.25f);	//Smoothly translate our weaponmodel back to its original position
			PlayerCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, 65, 0.25f); //Smoothly translate the fov back to the nornal value
			InGameUI.instance.Crosshair.SetActive (true);	//Show the crosshair
		}
	}
	#endregion

	void UpdateWeaponHud()
	{
		InGameUI.instance.WeaponNameText.text = WeaponName;		//Set the weaponname text in the hud to the current weapon name
		InGameUI.instance.WeaponCurrentMagazineAmmoText.text = CurrentMagazineAmmo.ToString();	//Set the currentmagazineammo in the hud to the current amount of bullets in this clip
		InGameUI.instance.WeaponReserveClipsText.text = CurrentClips.ToString();	//Set the reserve clip counter in the hud  to the amount of clips in our inventory
	}
}

[System.Serializable]
public class WeaponAttachment
{
	public WeaponScope[] WeaponScopes;
}

[System.Serializable]
public class WeaponScope
{
	public string ScopeName;
	public Vector3 WeaponScopeOffset;
	public GameObject ScopeOverlay;
	public GameObject ScopeModel;
	public Transform ScopePos;
	public float ScopeAimSpeedModifier = 1f;
	public int ScopeFieldOfView;
}

[System.Serializable]
public enum WeaponFireType
{
	Automatic,
	SemiAutomatic,
	Shotgun,
	Other
}

[System.Serializable]
public enum BulletType
{
	Bullet,
	ExplosiveBullet
}