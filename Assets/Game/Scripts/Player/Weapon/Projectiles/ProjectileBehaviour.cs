using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour {

	public string ProjectileName = "Explosion";
	public float ProjectileSpeed = 2500f;
	public float ProjectileLifeTime = 5f;
	public Rigidbody ProjectileRigidbody;
	public PhotonView ProjectilePhotonView;
	public GameObject ProjectileExplosion;
	public float ExplosionRadius = 4f;
	public float ExplosionMaxDamage = 150f;
	public bool ExplodeOnImpact = true;
	public bool UseGravity = false;

	void Start()
	{
		ProjectileRigidbody.useGravity = UseGravity;
		ProjectileRigidbody.AddForce (transform.forward * ProjectileSpeed);
		StartCoroutine (DestroyAfterTime (ProjectileLifeTime));
	}

	void OnCollisionEnter(Collision ProjectileHit)
	{
		if(ExplodeOnImpact)
		{
			Instantiate (ProjectileExplosion, ProjectileHit.contacts [0].point, Quaternion.FromToRotation (Vector3.forward, ProjectileHit.contacts [0].normal));
		}
		if (ProjectilePhotonView.owner == PhotonNetwork.player) {
			ProjectileAreaOfEffect ();
		}
		StopAllCoroutines ();
		DestroyProjectile ();
	}

	IEnumerator DestroyAfterTime(float DestroyTime)
	{
		yield return new WaitForSeconds (DestroyTime);
		if(ExplodeOnImpact)
		{
			Instantiate (ProjectileExplosion, transform.position, transform.rotation);
		}	
		if (ProjectilePhotonView.owner == PhotonNetwork.player) {
			ProjectileAreaOfEffect ();
		}
		DestroyProjectile ();
	}
		
	public void ProjectileAreaOfEffect()
	{
		Collider[] RadiusObjects = Physics.OverlapSphere(transform.position, ExplosionRadius);
		foreach (Collider HitObject in RadiusObjects) {
			if (HitObject.transform.root.GetComponent<PlayerStats> () != null && HitObject.tag == "PlayerSplashHitbox") {
				//Linear Damage drop off
				float Proximity = (transform.position - HitObject.transform.position).magnitude;
				float AreaOfEffectMultiplier = 1 - (Proximity / ExplosionRadius);
				int Damage = Mathf.RoundToInt(ExplosionMaxDamage * AreaOfEffectMultiplier);
				if (Damage <= 0) {		//If the damage is "negative"
					Damage = Mathf.Abs (Damage);	//Convert it to a 
				}
				HitObject.transform.root.GetComponent<PlayerStats> ().PlayerPhotonView.RPC ("ApplyPlayerDamage", PhotonTargets.All, Damage, ProjectileName, PhotonNetwork.player, 1f, false); //Tell the other player he is been hit
			}
		}
	}

	void DestroyProjectile()
	{
		if (ProjectilePhotonView.owner == PhotonNetwork.player) {
			PhotonNetwork.Destroy (this.gameObject);
		}
	}
}
