using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeaponSway : MonoBehaviour {

	public PlayerMovementController PlayerMovement;
	public float smoothGun = 2;
	public float tiltAngle = 15;
	public float tiltAngleY = 15;
	public float amount = 0.02f;
	public float maxAmount = 0.03f;
	public float AimInfluence = 0.1f;
	public float Smooth = 3;
	public float SmoothRotation= 2;

	private Vector3 def;

	void  Start () {
		def = transform.localPosition;
	}

	void  Update (){
		if (!GameManager.instance.InVehicle && GameManager.instance.IsAlive && transform.root.GetComponent<PlayerMovementController>() != null) {
			HandleWeaponSway ();
		}
	}

	void HandleWeaponSway()
	{
		float TiltGun = PlayerInputManager.instance.InputX * tiltAngle;
		float TiltGunY = -transform.root.GetComponent<PlayerMovementController>().controller.velocity.y * tiltAngleY;

		Quaternion target2 = Quaternion.Euler(TiltGunY, 0, TiltGun);

		transform.localRotation = Quaternion.Slerp(transform.localRotation, target2, Time.deltaTime * smoothGun);

		float factorX;
		float factorY;

		if (PlayerInputManager.instance.Aim) {
			factorX = PlayerInputManager.instance.MouseX * amount * AimInfluence;
			factorY = PlayerInputManager.instance.MouseY * amount * AimInfluence;
		} else {
			factorX = PlayerInputManager.instance.MouseX * amount;
			factorY = PlayerInputManager.instance.MouseY * amount;
		}

		if (factorX > maxAmount)
			factorX = maxAmount;

		if (factorX < -maxAmount)
			factorX = -maxAmount;

		if (factorY > maxAmount)
			factorY = maxAmount;

		if (factorY < -maxAmount)
			factorY = -maxAmount;


		Vector3 Final = new Vector3(def.x+factorX, def.y+factorY, def.z);
		transform.localPosition = Vector3.Lerp(transform.localPosition, Final, Time.deltaTime * Smooth);


		float tiltAroundZ = PlayerInputManager.instance.MouseX * tiltAngle;
		float tiltAroundX = PlayerInputManager.instance.MouseY * tiltAngle;
		Quaternion target = Quaternion.Euler (tiltAroundX, 0, tiltAroundZ);
		transform.localRotation = Quaternion.Slerp(transform.localRotation, target,Time.deltaTime * SmoothRotation);    
	}
}