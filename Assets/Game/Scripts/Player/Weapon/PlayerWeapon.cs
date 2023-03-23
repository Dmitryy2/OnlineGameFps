using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour {

	[Header("Weapon Stats")]
	public string WeaponName;           //Полное название этого оружия
	public int WeaponID;                //Идентификатор оружия
	public int Damage = 35;             //Урон, наносимый этим оружием
	public int ClipSize = 30;           //Максимальное количество пуль / снарядов, которое может быть в обойме
	public int Ammoleft = 30;           //Текущее количество патронов, которые у нас остались в текущей обойме
	public int Clips = 4;               //Количество обойм, имеющихся в оружии
	private float FireRate = 0.1f;      //Переменная для управления скоростью стрельбы
	public float FirstModeFireRate;     //Скорость стрельбы в первом режиме стрельбы
	public float SecondModeFireRate;    //Скорость стрельбы во втором режиме стрельбы
	public float WeaponRange;           //Дальность, на которую оружие может стрелять
	public float WeaponImpactForce;     //Сила удара, которую это оружие наносит физическим объектам

	[Header("Weapon FireMode")]
	private bool CanSwitchWeaponMode = true;
	public enum WeaponType { none, bullet, burst, shotgun, launcher};   //Все доступные типы оружия
	public enum WeaponFiringType {semi, auto};
	[Header("Primary FireMode")]
	public WeaponType FirstMode = WeaponType.bullet;                        //Основной режим огня
	public WeaponFiringType FirstModeFiringType = WeaponFiringType.auto;
	[Header("Secondary FireMode")]
	public WeaponType SecondMode = WeaponType.none;                         //Вторичный режим огня
	public WeaponFiringType SecondModeFiringType = WeaponFiringType.semi;
	[HideInInspector]public WeaponType CurrentMode = WeaponType.bullet;
	[HideInInspector]public WeaponFiringType CurrentFiringType = WeaponFiringType.auto;
	[Space(10)]
	public LayerMask WeaponLayerMask;
	private float ShootTimer = 0;	
	public Transform Weaponbarrel;
	public Transform FirstModeBarrel;
	public Transform SecondModeBarrel;

	[Header("Weapon Accuracy")]
	public float baseInaccuracyhip = 1.2f;      //Неточность при стрельбе от бедра
	public float baseInaccuracyaim = 0.1f;      //Неточность при прицеливании
	private float triggerTime = 3f;

	[Header("Weapon Aiming")]
	public Vector3 AimPosition;             //Положение оружия во время прицеливания
	public Vector3 WeaponOffset;            //Смещение этого оружия
	[Range(25,70)]
	public int AimFov = 40;                 //Угол обзора камеры при прицеливании
	[Range(0.1f,2f)]
	public float AimSpeedModifier = 1f;     //Как быстро игрок целится, когда прицеливается в
	private bool IsAiming;                  //Стремимся ли мы в настоящее время
	private Camera PlayerCamera;            //Камера игрока

	[Header("Burst Settings")]
	[Range(1,10)]
	public int shotsPerBurst = 3;           //Сколько патронов мы расстреливаем при разрыве?
	public float BurstTime = 0.07f;         //Сколько времени занимает каждая порция?
	public bool Bursting = false;           //Ведем ли мы в настоящее время очередную стрельбу

	[Header("Shotgun Settings")]
	[Range(1,20)]
	public int pelletsPerShot = 10;         //Сколько поддонов мы снимаем при обжиге

	[Header("Launcher Settings")]
	public GameObject Projectile;           //В каком снаряде мы хотим появиться?

	[Header("Weapon Recoil Settings")]
	public Transform WeaponRecoilHolder;    //Преобразование, которое мы используем для имитации отдачи
	public Vector3 RecoilRotation;
	public Vector3 RecoilKickback;
	public float RecoilModifier = 1f;
	private Vector3 CurrentRecoil1;
	private Vector3 CurrentRecoil2;
	private Vector3 CurrentRecoil3;
	private Vector3 CurrentRecoil4;

	[Header("Weapon Crosshair Settings")]
	public Texture CrosshairFirstMode;      //Перекрестие прицела для основного огня
	public Texture CrosshairSecondMode;     //Перекрестие прицела для вторичного огня

	[Header("BulletMarks Settings")]
	public GameObject ConcreteImpact;
	public GameObject WoodImpact;
	public GameObject MetalImpact;
	public GameObject FleshImpact;
	public GameObject SandImpact;
	public GameObject WaterImpact;

	[Header("Weapon Effect References")]
	public ParticleSystem WeaponPrimaryMuzzleFlash; //Дульная вспышка основного типа оружия
	public ParticleSystem WeaponShellEject;         //Дульный прицел оружия вторичного типа

	[Header("Weapon Audio Settings")]
	public AudioSource WeaponAudiosource;   //Аудиоисточник, из которого исходят звуки оружия
	public AudioClip WeaponDrawSound;       //Звук извлечения оружия
	public AudioClip WeaponCurrentFireSound;//Используемый звук огня
	public AudioClip WeaponFirePrimarySound;//Звук основного выстрела оружия
	public AudioClip WeaponFireSecondarySound;//Звук вторичного выстрела оружия
	public AudioClip WeaponReloadSound;     //Звук перезарядки оружия
	public AudioClip WeaponEmptySound;      //Оружие пустой звук
	public AudioClip WeaponSwitchModeSound; //Звук при переключении режима огня

	[Header("Weapon Animation Settings")]
	public Animator WeaponMovementAnimator;
	public enum MovementType { Rifle, Pistol };
	public MovementType WeaponMovementType;
	public Animation WeaponAnimationComponent;
	public AnimationClip WeaponPrimaryIdleAnimation;
	public AnimationClip WeaponDrawAnimation;
	public AnimationClip WeaponFireAnimation;
	public AnimationClip WeaponReloadAnimation;
	public AnimationClip WeaponFireEmptyAnimation;
	public AnimationClip WeaponSwitchAnimation;
	public AnimationClip WeaponSecondaryIdleAnimation;
	public AnimationClip WeaponFireSecondaryAnimation;
	public AnimationClip WeaponReloadSecondaryAnimation;

	[Header("Other References / Settings")]
	public PlayerMovementController PlayerMovementController;       //Ссылка на наш playermovementcontroller
	public CharacterController PlayerCharacterController;
	public PlayerWeaponManager PlayerWeaponManager;                 //Ссылка на наш playerweaponmanager
	public Transform WeaponHolder;                                  //Трансформация, в которой находится наше оружие
	public PhotonView WeaponPhotonView;

	void Start()
	{
		PlayerMovementController = transform.root.GetComponent<PlayerMovementController> ();
		PlayerCharacterController = transform.root.GetComponent<CharacterController> ();
		WeaponHolder = transform.parent;
		PlayerCamera = PlayerMovementController.PlayerCamera.GetComponent<Camera>();
		WeaponRecoilHolder = PlayerWeaponManager.WeaponRecoilHolder;
		Bursting = false;
		IsAiming = false;
		CanSwitchWeaponMode = true;
		CurrentMode = FirstMode;
		FireRate = FirstModeFireRate;
		Weaponbarrel = FirstModeBarrel;
		CurrentFiringType = FirstModeFiringType;
		WeaponAudiosource = transform.parent.GetComponent<AudioSource> ();
		triggerTime *= baseInaccuracyhip;
		WeaponCurrentFireSound = WeaponFirePrimarySound;
	} 

	void OnEnable()
	{
		PlayerWeaponManager = transform.root.GetComponent<PlayerWeaponManager> ();
		WeaponMovementAnimator = PlayerWeaponManager.WeaponMovementAnimator;
		if (WeaponMovementType == MovementType.Rifle) {
			WeaponMovementAnimator.SetLayerWeight (1, 0f);
		} else if (WeaponMovementType == MovementType.Pistol) {
			WeaponMovementAnimator.SetLayerWeight (1, 1f);
		}
		if (FirstModeFiringType == WeaponFiringType.semi) {
			PlayerInputManager.instance.IsAuto = false;
		} else {
			PlayerInputManager.instance.IsAuto = true;
		}
	}

	void Update()
	{
		if (!GameManager.instance.InVehicle) {
			GetPlayerInput ();
			RecoilController ();
			UpdateWeaponHud ();                         //Отображение статистики оружия на HUD
			WeaponMovementAnimationController();
		}
	}

	void GetPlayerInput()
	{
		if (PlayerInputManager.instance.Attack && Ammoleft >= 1 && CanSwitchWeaponMode && !WeaponAnimationComponent.IsPlaying (WeaponReloadAnimation.name) && PlayerMovementController.PlayerWalkingState != WalkingState.Running && !WeaponAnimationComponent.IsPlaying (WeaponDrawAnimation.name)) {
			if (CurrentMode == WeaponType.bullet) {
				WeaponFire ();
			} else if (CurrentMode == WeaponType.launcher) {
				WeaponFireLauncher ();
			} else if (CurrentMode == WeaponType.burst && !Bursting) {
				WeaponBurstFire ();
			} else if (CurrentMode == WeaponType.shotgun) {
				WeaponFireShotgun ();
			}
		} else if (Input.GetKeyDown (KeyCode.Mouse0) && Ammoleft == 0 && Clips == 0 && CanSwitchWeaponMode && !WeaponAnimationComponent.IsPlaying (WeaponReloadAnimation.name) && PlayerMovementController.PlayerWalkingState != WalkingState.Running && !WeaponAnimationComponent.IsPlaying (WeaponDrawAnimation.name) && !PlayerInputManager.instance.FreezePlayerControls && !PlayerWeaponManager.WeaponMovementAnimator.IsInTransition(0)) {
			if (WeaponEmptySound != null) {
				WeaponAudiosource.PlayOneShot (WeaponEmptySound, 0.4f);
			}
		}

		if (PlayerInputManager.instance.Reload && Clips >= 1 && Ammoleft != ClipSize || Ammoleft == 0 && Clips >= 1 && !WeaponAnimationComponent.IsPlaying (WeaponReloadAnimation.name) && PlayerMovementController.PlayerWalkingState != WalkingState.Running && !WeaponAnimationComponent.IsPlaying (WeaponDrawAnimation.name) && !IsAiming){
			ReloadWeapon();
		}

		if (PlayerInputManager.instance.Aim && !WeaponAnimationComponent.IsPlaying (WeaponReloadAnimation.name) && PlayerMovementController.PlayerWalkingState != WalkingState.Running && !WeaponAnimationComponent.IsPlaying (WeaponDrawAnimation.name)) {
			IsAiming = true;
			AimWeapon (true);
			RecoilModifier = 0.3f;
			triggerTime = 3f;
			triggerTime *= baseInaccuracyaim;
			PlayerInputManager.instance.AimSpeedModifier = AimSpeedModifier;
		} else {
			IsAiming = false;
			AimWeapon (false);
			RecoilModifier = 1f;
			triggerTime = 3f;
			triggerTime *= baseInaccuracyhip;
			PlayerInputManager.instance.AimSpeedModifier = 1f;
		}

		if(PlayerInputManager.instance.SwitchFireMode && SecondMode != WeaponType.none && CanSwitchWeaponMode && !WeaponAnimationComponent.IsPlaying (WeaponReloadAnimation.name) && !WeaponAnimationComponent.IsPlaying (WeaponDrawAnimation.name) && PlayerMovementController.PlayerWalkingState != WalkingState.Running){
			if(CurrentMode != FirstMode){
				StartCoroutine(SetPrimaryMode());
			}else{
				StartCoroutine(SetSecondaryMode());
			}
		}	
	}

	void WeaponMovementAnimationController() {
		if (PlayerInputManager.instance.FreezePlayerControls == false) {
			if (PlayerMovementController.WasStanding && !PlayerCharacterController.isGrounded) { //Если бы мы стояли, и мы больше не заземлены
				PlayerMovementController.WasStanding = false;								
				//Если у вас есть анимация прыжка с оружием / звук, вы можете разместить ее здесь
			} else if (!PlayerMovementController.WasStanding && PlayerCharacterController.isGrounded)
			{ //Если бы мы не стояли, а сейчас приземлились
				PlayerMovementController.WasStanding = true;
			}
			if (PlayerCharacterController.isGrounded && PlayerMovementController.PlayerMovementVelocity > 0f)
			{ //Если мы заземлены и наша скорость в любом направлении больше 0
				if (PlayerMovementController.PlayerWalkingState == WalkingState.Running)
				{   //Если текущее состояние ходьбы запущено
					WeaponMovementAnimator.SetFloat ("Movement", 1f, 0.2f, Time.deltaTime); //Установите для плавающего дерева смешивания animator значение 1f, чтобы оно воспроизводило запущенную анимацию
				} else if (PlayerMovementController.PlayerWalkingState == WalkingState.Walking)
				{ //Если текущее состояние ходьбы - это ходьба
					WeaponMovementAnimator.SetFloat ("Movement", 0.5f, 0.2f, Time.deltaTime); //Установите для аниматора blend tree float значение 0.5f, чтобы он воспроизводил анимацию ходьбы
				}
			} else {		//If we arent grounded and/or velocity is zero
				WeaponMovementAnimator.SetFloat ("Movement", 0f, 0.2f, Time.deltaTime); //Установите для animator blend tree float значение 0f, чтобы он воспроизводил анимацию в режиме ожидания
			}
		} else {
			WeaponMovementAnimator.SetFloat ("Movement", 0f, 0.2f, Time.deltaTime); //Установите для animator blend tree float значение 0f, чтобы он воспроизводил анимацию в режиме ожидания
		}
	}

	IEnumerator SetPrimaryMode()
	{
		CanSwitchWeaponMode = false;
		if (WeaponSwitchAnimation != null) {
			WeaponAnimationComponent.Rewind (WeaponSwitchAnimation.name);
			WeaponAnimationComponent.Play (WeaponSwitchAnimation.name);
		}
		if (WeaponSwitchModeSound != null) {
			WeaponAudiosource.clip = WeaponSwitchModeSound;
			WeaponAudiosource.Play ();
		}
		yield return new WaitForSeconds (WeaponSwitchAnimation.length);
		CurrentMode = FirstMode;
		WeaponCurrentFireSound = WeaponFirePrimarySound;
		WeaponAnimationComponent.CrossFade (WeaponPrimaryIdleAnimation.name, 0.2f);
		Weaponbarrel = FirstModeBarrel;
		FireRate = FirstModeFireRate;
		CanSwitchWeaponMode = true;
		if (FirstModeFiringType == WeaponFiringType.auto) {
			PlayerInputManager.instance.IsAuto = true;
		} else if (FirstModeFiringType == WeaponFiringType.semi) {
			PlayerInputManager.instance.IsAuto = false;
		}
		CurrentFiringType = FirstModeFiringType;
	}

	IEnumerator SetSecondaryMode()
	{
		CanSwitchWeaponMode = false;
		if (WeaponSwitchAnimation != null) {
			WeaponAnimationComponent.Rewind (WeaponSwitchAnimation.name);
			WeaponAnimationComponent.Play (WeaponSwitchAnimation.name);
		}
		if (WeaponSwitchModeSound != null) {
			WeaponAudiosource.clip = WeaponSwitchModeSound;
			WeaponAudiosource.Play ();
		}
		yield return new WaitForSeconds (WeaponSwitchAnimation.length);
		CurrentMode = SecondMode;
		WeaponCurrentFireSound = WeaponFireSecondarySound;
		WeaponAnimationComponent.CrossFade (WeaponSecondaryIdleAnimation.name, 1f);
		Weaponbarrel = SecondModeBarrel;
		FireRate = SecondModeFireRate;
		CanSwitchWeaponMode = true;
		if (SecondModeFiringType == WeaponFiringType.auto) {
			PlayerInputManager.instance.IsAuto = true;
		} else if (SecondModeFiringType == WeaponFiringType.semi) {
			PlayerInputManager.instance.IsAuto = false;
		}
		CurrentFiringType = SecondModeFiringType;
	}

	#region WeaponReloadBehaviour
	public void ReloadWeapon()
	{
		if (!WeaponAnimationComponent.IsPlaying (WeaponReloadAnimation.name) && WeaponReloadAnimation != null) {		//If we arent currently playing the reload animation
			PlayerMovementController.PlayerThirdPersonController.ThirdPersonPhotonView.RPC("ThirdPersonReload", PhotonTargets.All, null); //Send an rpc so the thirdpersonmodel also does an reload animation
			if (WeaponReloadSound != null) {		//If this weapon has a reload sound
				WeaponAudiosource.PlayOneShot (WeaponReloadSound, 1f);	//Play the reload sound
			}
			WeaponAnimationComponent.Play (WeaponReloadAnimation.name);  //Play the weapon's reload animation
			Invoke ("WaitTillReload", WeaponReloadAnimation.length);		//Wait till the weapon is done reloading
		}
	}

	void WaitTillReload()
	{
		PlayerMovementController.PlayerThirdPersonController.ThirdPersonPhotonView.RPC("FinishReload", PhotonTargets.Others, null);	//Tell the everyone to stop the reload animation and to enable IK again on this playermodel
		Ammoleft = ClipSize;	//Set the ammoleft to the max size
		Clips = Clips - 1;		//Remove 1 clip from the total amount of clips we have
	}
	#endregion

	#region WeaponAimBehaviour
	void AimWeapon(bool State)
	{
		if (State) {	//If we are aiming right now
			IsAiming = true;	
			WeaponHolder.localPosition = Vector3.Lerp (WeaponHolder.localPosition, AimPosition, 0.25f);	//Smoothly translate our weaponmodel to the weaponscopeoffset
			PlayerCamera.fieldOfView = Mathf.Lerp (PlayerCamera.fieldOfView, AimFov, 0.25f); //Smoothly translate our fov to the weaponscope fieldofview
			InGameUI.instance.Crosshair.SetActive (false);	//Hide the crosshair
		} else if (!State) {
			IsAiming = false;
			WeaponHolder.localPosition = Vector3.Lerp(WeaponHolder.localPosition, WeaponOffset, 0.25f);	//Smoothly translate our weaponmodel back to its original position
			PlayerCamera.fieldOfView = Mathf.Lerp(PlayerCamera.fieldOfView, 65, 0.25f); //Smoothly translate the fov back to the nornal value
			InGameUI.instance.Crosshair.SetActive (true);	//Show the crosshair
		}
	}
	#endregion

	#region Weapon Firing Behaviour
	void WeaponFire()
	{
		if (ShootTimer <= Time.time) {						//If we can shoot rightnow according the firerate
			ShootTimer = Time.time + FireRate;				//Set the timer again

			WeaponAudiosource.PlayOneShot (WeaponCurrentFireSound);

			CurrentRecoil1 += new Vector3(RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y));
			CurrentRecoil3 += new Vector3(Random.Range(-RecoilKickback.x, RecoilKickback.x), Random.Range(-RecoilKickback.y, RecoilKickback.y), RecoilKickback.z);

			if (CurrentMode == FirstMode) {
				//WeaponAnimationComponent.Rewind (WeaponFireAnimation.name);
				WeaponAnimationComponent.CrossFadeQueued (WeaponFireAnimation.name, 0.01f, QueueMode.PlayNow);
			}
			else if (CurrentMode == SecondMode) {
				//WeaponAnimationComponent.Rewind (WeaponFireSecondaryAnimation.name);
				WeaponAnimationComponent.CrossFadeQueued (WeaponFireSecondaryAnimation.name, 0.01f, QueueMode.PlayNow);
			}

			if (WeaponShellEject != null) {						//If the weapon has a shelleject particle effect
				WeaponShellEject.Play ();							//Play the effect
			}
			if (WeaponPrimaryMuzzleFlash != null) {						//If the weapon has a muzzleflash particle effect
				WeaponPrimaryMuzzleFlash.Play ();						//Play the effect
			}
				
			PlayerMovementController.PlayerThirdPersonController.ThirdPersonPhotonView.RPC ("ThirdPersonFireWeapon", PhotonTargets.Others, null);	//Send an rpc to all players so that other players can also see the thirdperson model fire the weapon
			FireBullet ();
			Ammoleft--;
		}
	}

	void WeaponBurstFire()
	{
		if (ShootTimer <= Time.time && !Bursting) {						//If we can shoot rightnow according the firerate
			ShootTimer = Time.time + FireRate;				//Set the timer again
			StartCoroutine (Burst ());
		}
	}

	IEnumerator Burst()
	{
		int shotcounter = 0;
		while (shotcounter < shotsPerBurst) {
			Bursting = true;
			shotcounter++;
			if (Ammoleft > 0) {
				CurrentRecoil1 += new Vector3(RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y));
				CurrentRecoil3 += new Vector3(Random.Range(-RecoilKickback.x, RecoilKickback.x), Random.Range(-RecoilKickback.y, RecoilKickback.y), RecoilKickback.z);

				if (CurrentMode == FirstMode) {
					//WeaponAnimationComponent.Rewind (WeaponFireAnimation.name);
					WeaponAnimationComponent.CrossFadeQueued (WeaponFireAnimation.name, 0.01f, QueueMode.PlayNow);
				}
				else if (CurrentMode == SecondMode) {
					//WeaponAnimationComponent.Rewind (WeaponFireSecondaryAnimation.name);
					WeaponAnimationComponent.CrossFadeQueued (WeaponFireSecondaryAnimation.name, 0.01f, QueueMode.PlayNow);
				}

				WeaponAudiosource.PlayOneShot (WeaponCurrentFireSound);
				if (WeaponShellEject != null) {						//If the weapon has a shelleject particle effect
					WeaponShellEject.Play ();							//Play the effect
				}
				if (WeaponPrimaryMuzzleFlash != null) {						//If the weapon has a muzzleflash particle effect
					WeaponPrimaryMuzzleFlash.Play ();						//Play the effect
				}
				PlayerMovementController.PlayerThirdPersonController.ThirdPersonPhotonView.RPC ("ThirdPersonFireWeapon", PhotonTargets.All, null);	//Send an rpc to all players so that other players can also see the thirdperson model fire the weapon
				FireBullet ();
				Ammoleft--;
			}
			yield return new WaitForSeconds (BurstTime);
		}
		yield return new WaitForSeconds (0.2f);
		Bursting = false;
	}

	void FireBullet() {
		Vector3 Spread = new Vector3 (Random.Range (-0.01f, 0.01f) * triggerTime, Random.Range (-0.01f, 0.01f) * triggerTime, 1f);
		Vector3 Direction = Weaponbarrel.TransformDirection(Spread);
		Ray WeaponRay =	 new Ray(Weaponbarrel.position, Direction);
		RaycastHit WeaponHit;

		if (Physics.Raycast (WeaponRay, out WeaponHit, WeaponRange, WeaponLayerMask)) {
			if (WeaponHit.transform.GetComponent<Rigidbody> () != null) {
				WeaponHit.transform.GetComponent<Rigidbody> ().AddForce (WeaponImpactForce * Direction, ForceMode.Impulse);
			}

			if (WeaponHit.collider.tag == "Untagged" || WeaponHit.collider.tag == "Concrete") {
				GameObject ConcreteHole = Instantiate (ConcreteImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				ConcreteHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Metal") {
				GameObject MetalHole = Instantiate (MetalImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				MetalHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Sand") {
				GameObject SandHole = Instantiate (SandImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				SandHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Wood") {
				GameObject WoodHole = Instantiate (WoodImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				WoodHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Water") {
				GameObject WaterHole = Instantiate (WaterImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				WaterHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Vehicle") { //&& WeaponHit.transform.root.GetComponent<VehicleStats> ().VehicleAlive == true
				GameObject MetalHole = Instantiate (MetalImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				MetalHole.transform.parent = WeaponHit.transform;
				//WeaponHit.transform.root.GetComponent<VehicleStats> ().VehiclePhotonView.RPC ("FinishVehicleDamage", PhotonTargets.All, Damage ,WeaponName, PhotonNetwork.player);
			}

			if (WeaponHit.collider.tag == "DestroyableObject")
			{
				WeaponHit.transform.GetComponent<PhotonView>().RPC("ApplyDamage", PhotonTargets.All, Damage); //Update the destroyable objects health on all other players.
			}


			if (WeaponHit.collider.tag == "PlayerHitbox") {
				GameObject FleshHole = Instantiate (FleshImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				FleshHole.transform.parent = WeaponHit.transform;
				if (WeaponHit.transform.tag == "PlayerHitbox" && WeaponHit.transform.root.GetComponent<PlayerStats> () != null && WeaponHit.transform.root.GetComponent<PlayerStats> ().isAlive) { //If the object we shot is an player who is also alive
					InGameUI.instance.DoHitMarker ();	//Display an hitmarker
					WeaponHit.transform.root.GetComponent<PhotonView> ().RPC ("ApplyPlayerDamage", PhotonTargets.All, Damage, WeaponName, PhotonNetwork.player, WeaponHit.transform.GetComponent<PlayerBodyPartMultiplier>().DamageModifier, false); //Tell the other player he is been hit
				}
			}
		}
	}

	void WeaponFireShotgun()
	{
		if (ShootTimer <= Time.time) {						//If we can shoot rightnow according the firerate
			ShootTimer = Time.time + FireRate;				//Set the timer again
			WeaponAudiosource.PlayOneShot (WeaponCurrentFireSound);

			CurrentRecoil1 += new Vector3 (RecoilRotation.x, Random.Range (-RecoilRotation.y, RecoilRotation.y));
			CurrentRecoil3 += new Vector3 (Random.Range (-RecoilKickback.x, RecoilKickback.x), Random.Range (-RecoilKickback.y, RecoilKickback.y), RecoilKickback.z);

			if (CurrentMode == FirstMode) {
				WeaponAnimationComponent.CrossFadeQueued (WeaponFireAnimation.name, 0.01f, QueueMode.PlayNow);
			} else if (CurrentMode == SecondMode) {
				WeaponAnimationComponent.CrossFadeQueued (WeaponFireSecondaryAnimation.name, 0.01f, QueueMode.PlayNow);
			}

			if (WeaponShellEject != null) {						//If the weapon has a shelleject particle effect
				WeaponShellEject.Play ();							//Play the effect
			}
			if (WeaponPrimaryMuzzleFlash != null) {						//If the weapon has a muzzleflash particle effect
				WeaponPrimaryMuzzleFlash.Play ();						//Play the effect
			}

			int pellets = 0;
			while (pellets < pelletsPerShot) {
				FirePellet ();
				pellets++;
			}

			Ammoleft--;
		}
	}

	void FirePellet()
	{
		Vector3 Spread = new Vector3 (Random.Range (-0.01f, 0.01f) * 1.2f, Random.Range (-0.01f, 0.01f) * 1.2f, 1f);
		Vector3 Direction = Weaponbarrel.TransformDirection(Spread);
		Ray WeaponRay =	 new Ray(Weaponbarrel.position, Direction);
		RaycastHit WeaponHit;

		if (Physics.Raycast (WeaponRay, out WeaponHit, WeaponRange, WeaponLayerMask)) {
			if (WeaponHit.transform.GetComponent<Rigidbody> () != null) {
				WeaponHit.transform.GetComponent<Rigidbody> ().AddForce (WeaponImpactForce * Direction, ForceMode.Impulse);
			}

			if (WeaponHit.collider.tag == "Untagged" || WeaponHit.collider.tag == "Concrete") {
				GameObject ConcreteHole = Instantiate (ConcreteImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				ConcreteHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Metal") {
				GameObject MetalHole = Instantiate (MetalImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				MetalHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Sand") {
				GameObject SandHole = Instantiate (SandImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				SandHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Wood") {
				GameObject WoodHole = Instantiate (WoodImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				WoodHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Water") {
				GameObject WaterHole = Instantiate (WaterImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				WaterHole.transform.parent = WeaponHit.transform;
			}

			if (WeaponHit.collider.tag == "Vehicle") { //&& WeaponHit.transform.root.GetComponent<VehicleStats> ().VehicleAlive == true
				GameObject MetalHole = Instantiate (MetalImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				MetalHole.transform.parent = WeaponHit.transform;
				//WeaponHit.transform.root.GetComponent<VehicleStats> ().VehiclePhotonView.RPC ("FinishVehicleDamage", PhotonTargets.All, Damage ,WeaponName, PhotonNetwork.player);
			}

			if (WeaponHit.collider.tag == "DestroyableObject")
			{
				WeaponHit.transform.GetComponent<PhotonView>().RPC("ApplyDamage", PhotonTargets.All, Damage); //Update the destroyable objects health on all other players.
			}


			if (WeaponHit.collider.tag == "PlayerHitbox") {
				GameObject FleshHole = Instantiate (FleshImpact , WeaponHit.point, Quaternion.FromToRotation (Vector3.forward, WeaponHit.normal)); //Spawn the surface hit prefab at the spot, align the position and rotation with the surface position and normal
				FleshHole.transform.parent = WeaponHit.transform;
				if (WeaponHit.transform.tag == "PlayerHitbox" && WeaponHit.transform.root.GetComponent<PlayerStats> () != null && WeaponHit.transform.root.GetComponent<PlayerStats> ().isAlive) { //If the object we shot is an player who is also alive
					InGameUI.instance.DoHitMarker ();	//Display an hitmarker
					WeaponHit.transform.root.GetComponent<PhotonView> ().RPC ("ApplyPlayerDamage", PhotonTargets.All, Damage, WeaponName, PhotonNetwork.player, WeaponHit.transform.GetComponent<PlayerBodyPartMultiplier>().DamageModifier, false); //Tell the other player he is been hit
				}
			}
		}
	}

	void WeaponFireLauncher()
	{
		if (ShootTimer <= Time.time) {						//If we can shoot rightnow according the firerate
			ShootTimer = Time.time + FireRate;				//Set the timer again

			WeaponAudiosource.PlayOneShot (WeaponCurrentFireSound);

			CurrentRecoil1 += new Vector3(RecoilRotation.x, Random.Range(-RecoilRotation.y, RecoilRotation.y));
			CurrentRecoil3 += new Vector3(Random.Range(-RecoilKickback.x, RecoilKickback.x), Random.Range(-RecoilKickback.y, RecoilKickback.y), RecoilKickback.z);

			if (CurrentMode == FirstMode) {
				//WeaponAnimationComponent.Rewind (WeaponFireAnimation.name);
				WeaponAnimationComponent.CrossFadeQueued (WeaponFireAnimation.name, 0.01f, QueueMode.PlayNow);
			}
			else if (CurrentMode == SecondMode) {
				//WeaponAnimationComponent.Rewind (WeaponFireSecondaryAnimation.name);
				WeaponAnimationComponent.CrossFadeQueued (WeaponFireSecondaryAnimation.name, 0.01f, QueueMode.PlayNow);
			}

			if (WeaponShellEject != null) {						//If the weapon has a shelleject particle effect
				WeaponShellEject.Play ();							//Play the effect
			}
			if (WeaponPrimaryMuzzleFlash != null) {						//If the weapon has a muzzleflash particle effect
				WeaponPrimaryMuzzleFlash.Play ();						//Play the effect
			}
				
			PlayerMovementController.PlayerThirdPersonController.ThirdPersonPhotonView.RPC ("ThirdPersonFireWeapon", PhotonTargets.All, null);	//Send an rpc to all players so that other players can also see the thirdperson model fire the weapon
			PhotonNetwork.Instantiate(Projectile.name, Weaponbarrel.position, Weaponbarrel.rotation, 0);
			Ammoleft--;
		}
	}




	public void RecoilController()
	{
		CurrentRecoil1 = Vector3.Lerp(CurrentRecoil1, Vector3.zero, 0.1f);
		CurrentRecoil2 = Vector3.Lerp(CurrentRecoil2, CurrentRecoil1, 0.1f);
		CurrentRecoil3 = Vector3.Lerp(CurrentRecoil3, Vector3.zero, 0.1f);
		CurrentRecoil4 = Vector3.Lerp(CurrentRecoil4, CurrentRecoil3, 0.1f);

		WeaponRecoilHolder.localEulerAngles = CurrentRecoil2 * RecoilModifier;
		WeaponRecoilHolder.localPosition = CurrentRecoil4 * RecoilModifier;

		PlayerCamera.transform.localEulerAngles = CurrentRecoil2 / 1.2f * RecoilModifier;
	}
	#endregion	

	void UpdateWeaponHud()
	{
		InGameUI.instance.WeaponNameText.text = WeaponName;     //Установите текст названия оружия в hud на текущее название оружия
		InGameUI.instance.WeaponCurrentMagazineAmmoText.text = Ammoleft.ToString(); //Установите ammoleft в hud на текущее количество пуль в этой обойме
		InGameUI.instance.WeaponReserveClipsText.text = Clips.ToString();   //Установите счетчик резервных клипов в hud на количество клипов в нашем инвентаре
	}
}
