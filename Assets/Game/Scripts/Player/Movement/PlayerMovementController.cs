using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovementController: MonoBehaviour {

	[Header("Player References")]
	public MouseLook PlayerMouseLook;
	public Transform PlayerCamera;
	public Transform PlayerWeaponHolder;
	public PlayerWeaponManager PlayerWeaponManager;
	public PlayerThirdPersonController PlayerThirdPersonController;
	public PlayerStats PlayerStatistics;
	public GameObject PlayerLegs;
	public Animator PlayerLegsAnimator;
	public PhotonView PlayerPhotonView;

	[Header("Player Movement Variables")]
	public float walkSpeed = 6.0f;
	public float runSpeed = 11.0f;
	public bool InWater = false;
	public bool limitDiagonalSpeed = true;
	private Vector3 moveDirection = Vector3.zero;
	[SerializeField]private bool grounded = false;
	public CharacterController controller;
	private Transform myTransform;
	[SerializeField]private float PlayerMovementSpeed;
	public float PlayerMovementVelocity;
	private RaycastHit hit;
	public float NormalGravity;
	public WalkingState PlayerWalkingState;
	private float DistanceToObstacle;

	[Header("Player Jump Variables")]
	public float jumpSpeed = 8.0f;
	public float gravity = 20.0f;
	public float fallingDamageThreshold = 10.0f;
	private float fallStartLevel;
	private bool falling;
	public bool WasStanding = false;
	private float slideLimit;

	[Header("Player Crouch Behaviour")]
	public PlayerStance PlayerStanceState;
	public Vector3 PlayerCenterOffset;
	public Vector3 PlayerCameraOffset;
	public float PlayerNormalHeight = 1.8f;
	public float PlayerCrouchSpeed = 3f;
	public float PlayerCrouchedHeight = 0.8f;

	[Header("Player Leaning Behaviour")]
	public Transform LeaningPivotTransform;
	public bool isLeaning = false;
	public float LeanSpeed = 100f;
	public float MaxLeanAngle = 20f;
	private float CurrentLeanAngle = 0f;

	[Header("Player Sliding Variables")]
	public bool slideWhenOverSlopeLimit = false;
	public bool slideOnTaggedObjects = false;
	public float slideSpeed = 12.0f;
	public bool airControl = false;
	public float antiBumpFactor = .75f;
	public int antiBunnyHopFactor = 1;
	private float rayDistance;
	private Vector3 contactPoint;
	private bool playerControl = false;
	private int jumpTimer;

	[Header("Player Camera Animation")]
	public Animation PlayerCameraAnimationComponent;
	public AnimationClip PlayerCameraLand;
	public AnimationClip PlayerCameraRun;

	void Awake()
	{
		PlayerMouseLook.Init (transform, PlayerWeaponHolder);
	}

	void Start() {
		//controller = GetComponent<CharacterController>();
		PlayerLegs.SetActive(true);
		myTransform = transform;
		PlayerMovementSpeed = walkSpeed;
		rayDistance = controller.height * .5f + controller.radius;
		slideLimit = controller.slopeLimit - .1f;
		jumpTimer = antiBunnyHopFactor;
	}

	void Update() {
		RotateView ();
		PlayerLeaning ();
		PlayerMovement ();
	}

	void FixedUpdate()
	{
		PlayerMovementVelocity = controller.velocity.magnitude;
	}

	private void PlayerMovement()
	{
		float inputX = PlayerInputManager.instance.InputX;
		float inputY = PlayerInputManager.instance.InputY;
		float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f && limitDiagonalSpeed)? .7071f : 1.0f;

		PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Grounded", grounded);
		PlayerLegsAnimator.SetBool ("Grounded", grounded);

		if (grounded) {
			PlayerThirdPersonController.PlayerThirdPersonAnimator.SetFloat ("Vertical", inputY, 0.1f, Time.deltaTime); //Set the Vertical float for our thirdperson animator
			PlayerThirdPersonController.PlayerThirdPersonAnimator.SetFloat ("Horizontal", inputX, 0.1f, Time.deltaTime); //Set the Horizontal float for our thirdperson animator
			PlayerLegsAnimator.SetFloat ("Vertical", inputY, 0.1f, Time.deltaTime); //Set the Vertical float for our thirdperson animator
			PlayerLegsAnimator.SetFloat ("Horizontal", inputX, 0.1f, Time.deltaTime); //Set the Horizontal float for our thirdperson animator
			gravity = NormalGravity; 

			bool sliding = false;
			// See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
			// because that interferes with step climbing amongst other annoyances
			if (Physics.Raycast(myTransform.position, -Vector3.up, out hit, rayDistance)) {
				if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
					sliding = true;
			}
			// However, just raycasting straight down from the center can fail when on steep slopes
			// So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
			else {
				Physics.Raycast(contactPoint + Vector3.up, -Vector3.up, out hit);
				if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
					sliding = true;
			}

			// If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
			if (falling) {
				falling = false;
				PlayerLegsAnimator.SetBool ("Jump", false);
				PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Jump", false);
				if (controller.height == PlayerNormalHeight || controller.height == PlayerCrouchedHeight) {	//If we aren't transitioning from standing to crouched.
					if (myTransform.position.y < fallStartLevel - 0.3f && !InWater) {	//If we fell higher than 0.3 meters 
						PlayerStatistics.PlayerGearAudioSource.PlayOneShot (PlayerStatistics.GearPlayerLandSound, PlayerStatistics.GearSoundVolume);	//Play the land sound
						PlayerCameraAnimationComponent.CrossFade (PlayerCameraLand.name, 0.1f);		//Play the camera land animation
					}
				}
				//Debug.Log ("Landed");
				if (myTransform.position.y < fallStartLevel - fallingDamageThreshold)
					FallingDamageAlert (fallStartLevel - myTransform.position.y);
			}
			if (WasStanding && !grounded)
			{
				WasStanding = false;
				PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Jump", true);
			}
			else if (!WasStanding && grounded)
			{
				WasStanding = true;
				PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Jump", false);
			} 

			if (PlayerInputManager.instance.Crouch && !PlayerInputManager.instance.Sprint) {
				PlayerStanceState = PlayerStance.Crouching;
				//controller.height = Mathf.Lerp(controller.height, PlayerCrouchedHeight, Time.deltaTime * 10f);
				PlayerWeaponHolder.localPosition = Vector3.Lerp(PlayerWeaponHolder.localPosition, PlayerCameraOffset, Time.deltaTime * 10);
				controller.height = PlayerCrouchedHeight;
				controller.center = PlayerCenterOffset;
				PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Crouched", true);
				PlayerLegsAnimator.SetBool ("Crouched", true);
			} else {
				PlayerStanceState = PlayerStance.Standing;
				//controller.height = Mathf.Lerp(controller.height, PlayerNormalHeight, Time.deltaTime * 10f);
				PlayerWeaponHolder.localPosition = Vector3.Lerp(PlayerWeaponHolder.localPosition, Vector3.zero, Time.deltaTime * 10);
				controller.height = PlayerNormalHeight;
				controller.center = Vector3.zero;
				PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Crouched", false);
				PlayerLegsAnimator.SetBool ("Crouched", false);
			}

			//Set the walking state according to the players actual walking state so we can trigger certain events like fp weapon movement and such
			if (inputX != 0 && PlayerMovementVelocity > 0.1f|| inputY != 0 && PlayerMovementVelocity > 0.1f) {
				if (PlayerInputManager.instance.Sprint) {
					PlayerWalkingState = WalkingState.Running;
					PlayerMovementSpeed = runSpeed;
					//PlayerThirdPersonController.PlayerThirdPersonAnimator.SetLayerWeight (1, 0f);
					PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Sprinting", true); //Let our thirdperson have a sprint animation
					PlayerLegsAnimator.SetBool ("Sprinting", true);
					PlayerStatistics.RunMultiplier = 1.3f; //Set the multiplier so footstep sound will play quicker after each other
					if (!PlayerCameraAnimationComponent.isPlaying) {
						PlayerCameraAnimationComponent.Play (PlayerCameraRun.name);
					}
				} else {
					PlayerWalkingState = WalkingState.Walking;
					//PlayerThirdPersonController.PlayerThirdPersonAnimator.SetLayerWeight (1, 1f);
					if (PlayerStanceState == PlayerStance.Crouching) {
						PlayerMovementSpeed = PlayerCrouchSpeed;
					} else {
						PlayerMovementSpeed = walkSpeed;
					}
					PlayerStatistics.RunMultiplier = 1f; //Set the multiplier to its default value to increase the time before each step soundeffect
					PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Sprinting", false); //Disable the thirdperson animation
					PlayerLegsAnimator.SetBool ("Sprinting", false);
				}
			} else {
				PlayerWalkingState = WalkingState.Idle;
				//PlayerThirdPersonController.PlayerThirdPersonAnimator.SetLayerWeight (1, 1f);
				PlayerStatistics.RunMultiplier = 1f; //Set the multiplier to its default value to increase the time before each step soundeffect
				PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Sprinting", false); //Disable the thirdperson animation
				PlayerLegsAnimator.SetBool ("Sprinting", false);
			}

			// If sliding (and it's allowed), or if we're on an object tagged "Slide", get a vector pointing down the slope we're on
			if ( (sliding && slideWhenOverSlopeLimit) || (slideOnTaggedObjects && hit.collider.tag == "Slide") ) {
				Vector3 hitNormal = hit.normal;
				moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
				Vector3.OrthoNormalize (ref hitNormal, ref moveDirection);
				moveDirection *= slideSpeed;
				playerControl = false;
			}
			// Otherwise recalculate moveDirection directly from axes, adding a bit of -y to avoid bumping down inclines
			else {
				moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
				moveDirection = myTransform.TransformDirection(moveDirection) * PlayerMovementSpeed;
				playerControl = true;
			}

			if (grounded && controller.velocity.magnitude > 0.1 && PlayerStatistics.StepTimer <= Time.time && !GameManager.instance.InVehicle && !InWater) { //If we are on the ground and moving and the step timer has reached a set treshold
				PlayerStatistics.StepTimer = Time.time + (PlayerStatistics.StepInterval / PlayerMovementSpeed * PlayerStatistics.RunMultiplier); //Reset the step timer to a set amount
				PlayerStatistics.FootstepAudiosource.PlayOneShot (PlayerStatistics.FootstepSounds [Random.Range (0, PlayerStatistics.FootstepSounds.Length)], PlayerStatistics.FootStepVolume); //Play a random footstep sound from the footstep array
				PlayerPhotonView.RPC("PlayFootstepSoundNetwork", PhotonTargets.Others, "Normal");
			} else if (grounded && controller.velocity.magnitude > 0.1 && PlayerStatistics.StepTimer <= Time.time && !GameManager.instance.InVehicle && InWater) { //If we are on the ground and moving and the step timer has reached a set treshold
				PlayerStatistics.StepTimer = Time.time + (4f / PlayerMovementSpeed * PlayerStatistics.RunMultiplier); //Reset the step timer to a set amount
				PlayerStatistics.FootstepAudiosource.PlayOneShot (PlayerStatistics.PlayerWaterWadeSound, 0.5f); //Play a the water wade soundeffect
				PlayerPhotonView.RPC("PlayFootstepSoundNetwork", PhotonTargets.Others, "Water");
			}

			if (grounded && controller.velocity.magnitude > 0 && PlayerWalkingState == WalkingState.Running && PlayerStatistics.SprintBreathTimer <= Time.time && !GameManager.instance.InVehicle) { //If we are on the ground and moving and the sprintrattle timer has reached a set treshold
				PlayerStatistics.SprintBreathTimer = Time.time + PlayerStatistics.SprintBreathInterval; //Reset the sprint rattle timer to a set amount
				PlayerStatistics.PlayerGearAudioSource.PlayOneShot (PlayerStatistics.PlayerSprintBreathSounds [Random.Range (0, PlayerStatistics.PlayerSprintBreathSounds.Length)], PlayerStatistics.BreathSoundVolume); //Play a random breath sound from the rattle array
			}

			// Jump! But only if the jump button has been released and player has been grounded for a given number of frames
			if (!PlayerInputManager.instance.Jump)
				jumpTimer++;
			else if (jumpTimer >= antiBunnyHopFactor) {
				moveDirection.y = jumpSpeed;
				jumpTimer = 0;
			}

		}
		else if(!grounded)
		{
			//If we currently are walking or running while we are not on the ground, then set the walkingstate to idle so the player can also shoot while sprint jumping
			if (PlayerWalkingState == WalkingState.Running || PlayerWalkingState == WalkingState.Walking) {
				PlayerWalkingState = WalkingState.Idle;
			}

			// If we stepped over a cliff or something, set the height at which we started falling
			if (!falling && PlayerStanceState != PlayerStance.Crouching) {
				falling = true;
				fallStartLevel = myTransform.position.y;
				PlayerThirdPersonController.PlayerThirdPersonAnimator.SetBool ("Jump", true); //Enable the thirdperson animation
				PlayerLegsAnimator.SetBool ("Jump", true);
			}

			// If air control is allowed, check movement but don't touch the y component
			if (airControl && playerControl) {
				moveDirection.x = inputX * PlayerMovementSpeed * inputModifyFactor;
				moveDirection.z = inputY * PlayerMovementSpeed * inputModifyFactor;
				moveDirection = myTransform.TransformDirection(moveDirection);
			}
		}

		// Apply gravity
		moveDirection.y -= gravity * Time.deltaTime;

		// Move the controller, and set grounded true or false depending on whether we're standing on something
		if (GameManager.instance.IsAlive && !GameManager.instance.InVehicle) {
			grounded = (controller.Move (moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
		} else {
			grounded = false;
		}
	}


		
	// Store point that we're in contact with for use in FixedUpdate if needed
	void OnControllerColliderHit (ControllerColliderHit hit) {
		contactPoint = hit.point;
	}

	void PlayerEnterWater()
	{
		InWater = true;
	}

	void PlayerExitWater()
	{
		InWater = false;
	}

	// If falling damage occured, this is the place to do something about it. You can make the player
	// have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
	void FallingDamageAlert (float fallDistance) {
		if (fallDistance > 2.5f) {
			int dmg = Mathf.RoundToInt (fallDistance * 7f);
			PlayerPhotonView.RPC("ApplyPlayerDamage", PhotonTargets.All , dmg, "Falling", PhotonNetwork.player, 1f);
		}
	}

	private void PlayerLeaning()
	{
		if (PlayerInputManager.instance.LeanLeft && !Input.GetKey(KeyCode.LeftShift)) { //If we press Q and arent sprinting
			isLeaning = true;
			CurrentLeanAngle = Mathf.MoveTowardsAngle(CurrentLeanAngle, MaxLeanAngle, LeanSpeed * Time.deltaTime); //Smoothing translate our pivot point to a set angle
		}
		else if (PlayerInputManager.instance.LeanRight && !Input.GetKey(KeyCode.LeftShift)) {
			isLeaning = true;
			CurrentLeanAngle = Mathf.MoveTowardsAngle(CurrentLeanAngle, -MaxLeanAngle, LeanSpeed * Time.deltaTime); //Smoothing translate our pivot point to a set angle
		}
		else {
			isLeaning = false;
			CurrentLeanAngle = Mathf.MoveTowardsAngle(CurrentLeanAngle, 0f, LeanSpeed * Time.deltaTime); //Reset the pivot point to a straight upward position
		}
		LeaningPivotTransform.localRotation = Quaternion.AngleAxis(CurrentLeanAngle, Vector3.forward);
	}

	private void RotateView()
	{
		PlayerMouseLook.LookRotation (transform, PlayerWeaponHolder.transform);
	}
}

public enum WalkingState
{
	Idle,
	Walking,
	Running
}

public enum PlayerStance
{
	Standing,
	Crouching
}
	