using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stryker_Turret : MonoBehaviour {

	[Header("Stryker Turret")]
	public PhotonView Stryker_PhotonView;
	public Transform Stryker_Turret_Base;
	public Transform Stryker_Turret_Gun;
	public Quaternion Stryker_Turret_Base_Rotation_Synced;
	public Quaternion Stryker_Turret_Gun_Rotation_Synced;

	public void Update()
	{
		HandleTurretAiming ();
	}

	public void HandleTurretAiming()
	{
		if (!Stryker_PhotonView.isMine) {
			Stryker_Turret_Base.localRotation = Quaternion.Lerp (Stryker_Turret_Base.localRotation, Stryker_Turret_Base_Rotation_Synced, 0.1f);
			Stryker_Turret_Gun.localRotation = Quaternion.Lerp (Stryker_Turret_Gun.localRotation, Stryker_Turret_Gun_Rotation_Synced, 0.1f);
		}
	}

	public void OnPhotonSerializeView (PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.isWriting) { //If we are sending this information
			stream.SendNext (Stryker_Turret_Base.localRotation); 
			stream.SendNext (Stryker_Turret_Gun.localRotation); 
		}
		else if (stream.isReading) { //If we are receiving this information
			this.Stryker_Turret_Base_Rotation_Synced = (Quaternion)stream.ReceiveNext();	
			this.Stryker_Turret_Gun_Rotation_Synced = (Quaternion)stream.ReceiveNext();	
		}
	}
}
