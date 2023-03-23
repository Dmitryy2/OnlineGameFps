using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stryker_Movement : MonoBehaviour {

	[Header("Vehicle References")]
	public WheelCollider[] PoweredWheels;
	public WheelCollider[] SteeringWheels;

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
		foreach(WheelCollider PowerWheel in PoweredWheels) {
			PowerWheel.motorTorque = 0f;
			PowerWheel.brakeTorque = 2000f;
		}
	}

	void FixedUpdate()
	{
		SteerAngle = PlayerInputManager.instance.InputX;
		Acceleration = PlayerInputManager.instance.InputY;

		float FinalSteerAngle = SteerAngle * 35f;
		foreach (WheelCollider SteerWheel in SteeringWheels) {
			SteerWheel.steerAngle = FinalSteerAngle;
		}

		foreach(WheelCollider PowerWheel in PoweredWheels) {
			PowerWheel.motorTorque = Acceleration * MaxTorque;
		}

		if (PlayerInputManager.instance.Jump) {
			foreach(WheelCollider PowerWheel in PoweredWheels) {
				PowerWheel.brakeTorque = MaxBrakeTorque;
			}
		} else {
			foreach(WheelCollider PowerWheel in PoweredWheels) {
				PowerWheel.brakeTorque = 0f;
			}
		}
	}
}
