using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonWeapon : MonoBehaviour {

	public Transform LeftHandTransform;		//Dont use these when dealing with pistols
	public Transform RightHandTransform;
	public bool IgnoreLeftHandRotation = true;
	public ParticleSystem MuzzleFlash;
	public AudioSource ThirdPersonAudioSource;
	public AudioClip[] WeaponFireSound;
	public AudioClip WeaponFireLoopSound;
	public int WeaponHoldType = 0;		//0 is rifle and 1 is pistol
}
