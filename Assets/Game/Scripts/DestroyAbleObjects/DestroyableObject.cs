

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is written by paultricklebank
/// Class which deals with applying damage to a destroyable object.
/// you MUST have a collider attached to the same child object as this script.
/// </summary>
/// 


//add a photon view
[RequireComponent(typeof(PhotonView))]


public class DestroyableObject : MonoBehaviour {

    
    public float health = 100;                          // The objects health
    public bool playRandomDeathEffect = false;          //if this is set to true, a death effect will be played from the
                                                        //array and played at raqndom. If it's false, they will all play.
    public float TimeToDestroyEffect = 5f;             //how long till we destroy the death effect object we instantiate?                                                    
    public GameObject[] DeathEffect = null;             //the array of death effects
    public GameObject AudioPlayer;
    public bool playdeathAudio = false;                 //Play the death audio clip?
    private AudioSource mySource = null;                 //our audioSource
    public AudioClip deathClip = null;                  //the on death audioclip.
    public float maxDistanceToHearAudio = 100f;          //how far in meters do we want the sound to carry?



    void Start()
    {
      
    }
    /// <summary>
    /// Apply damage to the GameObject this is attached to.
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    public void ApplyDamage(int damage)
    {
        health = health - damage;
        if (health < 0)
        {
            health = 0;
			DestroyObject();
        }
    }
    void DestroyObject()
    {
        if (playRandomDeathEffect == true)
        {
            int i = Random.Range(0, DeathEffect.Length);
            Instantiate(DeathEffect[i], this.transform.position, this.transform.rotation);
        }
		else 
        {
            foreach (GameObject go in DeathEffect)
            {
                Instantiate(go, this.transform.position, this.transform.rotation);
            }
        }
        //we need to instantiate a gameobject with an AudioSOurce attached otherwise we won't hear the death sound.
        if (playdeathAudio == true)
        {
            GameObject go = Instantiate(AudioPlayer, this.transform.position, this.transform.rotation);
            mySource = go.GetComponent<AudioSource>();
            mySource.spatialBlend = 1;
            mySource.rolloffMode = AudioRolloffMode.Custom;
            mySource.maxDistance = maxDistanceToHearAudio;
            mySource.PlayOneShot(deathClip, 0.5f);
            //destroy the empty gameobject after the audio clip has played.
            Destroy(go, deathClip.length);
        }
       	
		if (PhotonNetwork.isMasterClient) {
			PhotonNetwork.Destroy (this.gameObject);
		}
        //Destroy(this.gameObject);         //no need to destroy this over the network
                                            //as it wasn't instantiated over the network.
                                            //if you instantiate an object ver the network, you need to call
                                            //PhotonNetwork.Destroy()
    }
}
