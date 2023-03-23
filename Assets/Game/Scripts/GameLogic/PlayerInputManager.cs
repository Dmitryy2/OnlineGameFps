using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour {

	[HideInInspector]public static PlayerInputManager instance;
	[Header("Player Input Manager")]
	public bool FreezePlayerControls;		//Freeze all player controls?
	public float AimSpeedModifier = 1f;		//Multiplier to set the aim speed, lower means slower aiming. higher is faster aiming
	public float InputX;			
	public float InputY;
	public float MouseX;
	public float MouseY;
	public bool Jump;
	public bool Sprint;
	public bool IsAuto;
	public bool Attack;
	public bool Aim;
	public bool Reload;
	public bool NextWeapon;
	public bool FirstSeat;
	public bool SecondSeat;
	public bool LeanLeft;
	public bool LeanRight;
	public bool ChangeCamera;
	public bool Suicide;
	public bool Pause;
	public bool UseButton;
	public bool Scoreboard;
	public bool SwitchFireMode;
	public bool Crouch;

	void Awake()
	{
		if (instance == null) {
			instance = this;
		} else if (instance != null) {
			DestroyImmediate (this.gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (GameManager.instance.MatchActive) { //Only check for input while actually ingame
			HandlePlayerInput ();
		}
	}

	void HandlePlayerInput()
	{
		if (!FreezePlayerControls && !GameManager.instance.InVehicle) {								//If our controls are not currently frozen, assign each action to an button / input type
			InputX = Input.GetAxis ("Horizontal");
			InputY = Input.GetAxis ("Vertical");
			MouseX = Input.GetAxis ("Mouse X") * AimSpeedModifier;
			MouseY = Input.GetAxis ("Mouse Y") * AimSpeedModifier;
			Jump = Input.GetKeyDown (KeyCode.Space);
			Sprint = Input.GetKey (KeyCode.LeftShift);
			Reload = Input.GetKeyDown (KeyCode.R);
			if (IsAuto) {
				Attack = Input.GetKey (KeyCode.Mouse0);
			} else {
				Attack = Input.GetKeyDown (KeyCode.Mouse0);
			}
			Aim = Input.GetKey (KeyCode.Mouse1);
			NextWeapon = Input.GetKeyDown (KeyCode.Alpha1);
			LeanLeft = Input.GetKey (KeyCode.Q);
			LeanRight = Input.GetKey (KeyCode.E);
			Suicide = Input.GetKeyDown (KeyCode.P);
			Pause = Input.GetKeyDown (KeyCode.Escape);
			UseButton = Input.GetKeyDown (KeyCode.F);
			Scoreboard = Input.GetKey (KeyCode.Tab);
			SwitchFireMode = Input.GetKeyDown (KeyCode.N);
			Crouch = Input.GetKey (KeyCode.C);
			FirstSeat = false;
			SecondSeat = false;
		}  
		if (FreezePlayerControls && GameManager.instance.InVehicle && !InGameUI.instance.Paused) {
			ChangeCamera = Input.GetKeyDown (KeyCode.V);
			UseButton = Input.GetKeyDown (KeyCode.F);
			Pause = Input.GetKeyDown (KeyCode.Escape);
			Scoreboard = Input.GetKey (KeyCode.Tab);
			InputX = Input.GetAxis ("Horizontal");
			InputY = Input.GetAxis ("Vertical");
			MouseX = Input.GetAxis ("Mouse X") * AimSpeedModifier;
			MouseY = Input.GetAxis ("Mouse Y") * AimSpeedModifier;
			Jump = Input.GetKeyDown (KeyCode.Space);
			FirstSeat = Input.GetKeyDown (KeyCode.F1);
			SecondSeat = Input.GetKeyDown (KeyCode.F2);
			if (IsAuto) {
				Attack = Input.GetKey (KeyCode.Mouse0);
			} else {
				Attack = Input.GetKeyDown (KeyCode.Mouse0);
			}
		}
		if(FreezePlayerControls && !GameManager.instance.InVehicle || InGameUI.instance.Paused || !GameManager.instance.MatchActive) {
			InputX = 0f;
			InputY = 0f;
			MouseX = 0;
			MouseY = 0;
			Attack = false;
			ChangeCamera = false;
			LeanLeft = false;
			LeanRight = false;
			Sprint = false;
			Pause = Input.GetKeyDown (KeyCode.Escape);			//So we can still access the pause menu when our player controls are frozen
			UseButton = false;
			SwitchFireMode = false;
			Scoreboard = Input.GetKey (KeyCode.Tab);			//So we can still access the scoreboard when our player controls are frozen
			Crouch = false;
			FirstSeat = false;
			SecondSeat = false;
		}
	}
}
