using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FourWheeled_Movement : MonoBehaviour {

	[Header("Vehicle References")]
	public WheelCollider[] WheelColliders;

	[Header("Vehicle Handeling")]
	public float MaxTorque = 2000f;
	public float MaxSteerAngle = 35f;
	public float MaxBrakeTorque = 4000f;
	private float SteerAngle = 0f;
	private float Acceleration = 0f;


	void OnDisable()
	{
		DisableInput ();
	}

	void DisableInput()
	{
		for (int i = 0; i < 4; i++) {
			WheelColliders [i].motorTorque = 0f;
			WheelColliders [i].brakeTorque = 2000f;
		}
	}

	void FixedUpdate()
	{
		SteerAngle = PlayerInputManager.instance.InputX;
		Acceleration = PlayerInputManager.instance.InputY;

		float FinalSteerAngle = SteerAngle * 35f;
		WheelColliders [0].steerAngle = FinalSteerAngle;
		WheelColliders [1].steerAngle = FinalSteerAngle;

		for (int i = 0; i < 4; i++) {
			WheelColliders [i].motorTorque = Acceleration * MaxTorque;
		}

		if (PlayerInputManager.instance.Jump) {
			for (int i = 0; i < 4; i++) {
				WheelColliders [i].brakeTorque = MaxBrakeTorque;
			}
		} else {
			for (int i = 0; i < 4; i++) {
				WheelColliders [i].brakeTorque = 0f;
			}
		}
	}
}
