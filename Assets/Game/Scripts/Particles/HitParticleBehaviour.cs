using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitParticleBehaviour : MonoBehaviour {

	public AudioSource SurfaceParticleAudioSource;
	public AudioClip[] SurfaceParticleSoundEffects;
	public float DestroyTimer;

	void Start()
	{
		SurfaceParticleAudioSource.PlayOneShot (SurfaceParticleSoundEffects [Random.Range (0, SurfaceParticleSoundEffects.Length - 1)], 1f); //Plays a random hit sound from the SurfaceParticleSoundEffects Array
		StartCoroutine (DestroyAfterTime (DestroyTimer)); //Start a routine to destroy this particle after a certain amount of time
	}

	IEnumerator DestroyAfterTime(float TimeToWait)
	{
		yield return new WaitForSeconds (TimeToWait);
		Destroy (gameObject);
	}
}
