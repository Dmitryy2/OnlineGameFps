using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimScript : MonoBehaviour {

	public Transform Target;
	public Transform LeftHandTarget;
	public Vector3 Offset;
	public float TargetStartHeight = 1.5f;
	public Transform Chest;
	public float PlayerPoseModifier = 0f;
	public bool WeaponIKLeft;

	Animator anim;
	void Start()
	{
		anim = GetComponent<Animator>();
	}

	void Update()
	{
		if (anim.GetBool ("Crouched") == true) {
			PlayerPoseModifier = Mathf.Lerp (PlayerPoseModifier, -0.5f, 10 * Time.deltaTime);
		} else {
			PlayerPoseModifier = Mathf.Lerp (PlayerPoseModifier,  0f, 10 * Time.deltaTime);
		}
		Vector3 TargetHeight = new Vector3 (Target.position.x, anim.GetFloat ("Aim") + TargetStartHeight + PlayerPoseModifier, Target.position.z);
		Target.position = TargetHeight;
	}

	void LateUpdate()
	{
		Chest.LookAt (Target);
		Chest.rotation = Chest.rotation * Quaternion.Euler (Offset);
	}

	void OnAnimatorIK(int layer)
	{
		if (WeaponIKLeft) {
			anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
			anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
			anim.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandTarget.position);
			anim.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandTarget.rotation);
		}
	}
}﻿
