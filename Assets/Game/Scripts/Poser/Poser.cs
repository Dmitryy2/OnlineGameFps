using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Poser : MonoBehaviour {

	public Animation animationComponent;
	public AnimationClip clip;
	public float poseTime;
	public bool  pose;

	void  Update (){
		if(pose)
		{
			Pose();
			pose = false;
		}
	}

	void  Pose (){
		AnimationClip temp = animationComponent.clip;
		animationComponent.clip = clip;
		animationComponent[clip.name].time = poseTime;
		animationComponent.Play();
		animationComponent.Sample();
		animationComponent.Stop();
		animationComponent.clip = temp;
	}
}