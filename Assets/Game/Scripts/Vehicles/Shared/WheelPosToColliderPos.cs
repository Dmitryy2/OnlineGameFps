using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Borrowed from NCE for testing purposes
//Sets the position and rotation of the transform this script is on to the values of the specified wheelcollider
public class WheelPosToColliderPos : MonoBehaviour {

	public WheelCollider TargetCollider;
	private Vector3 WheelPos = new Vector3();
	private Quaternion WheelRot = new Quaternion();

	private void Update()
	{
		TargetCollider.GetWorldPose (out WheelPos, out WheelRot);
		transform.position = WheelPos;
		transform.rotation = WheelRot;
	}
}
