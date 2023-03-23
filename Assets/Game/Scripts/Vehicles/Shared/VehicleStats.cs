using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleStats : MonoBehaviour {

	public Rigidbody VehicleRigidbody;
	public Vehicle VehicleStatistics;
	public PhotonView VehiclePhotonView;
	public ParticleSystem VehicleDamageSmoke;
	public ParticleSystem VehicleDamageFire;
	public ParticleSystem VehicleDamageExplosion;
	public WheelCollider[] VehicleWheels;
	public int VehicleHealth = 500;
	public bool VehicleAlive = true;
	public int VehicleSmokeDamagePoint = 250;
	public int VehicelFireDamagePoint = 75;
	public bool CanEnter = true;
	public GameObject VehicleDeathCam;
	private int count = 0;

	void Start()
	{
		//VehiclePhotonView.RPC ("ChangeVehicleDamageState", PhotonTargets.AllBuffered, 0);
	}

	void Update()
	{
		OnPlayerVehicleExitRequest ();
		OnPlayerChangeSeatRequests ();
		if (GameManager.instance.MatchActive == false) {
			foreach (VehicleSeat Seat in VehicleStatistics.Seats) {
				foreach (MonoBehaviour SeatBehaviour in Seat.SeatBehaviours) {
					SeatBehaviour.enabled = false;
				}
			}
		}
	}

	#region VehicleEvents
	void OnPlayerVehicleEnterRequest()
	{
		for (int i = 0; i < this.VehicleStatistics.Seats.Length; i++) {	
			//Check if vehicle is not full and which seats are available at the moment
			if (this.VehicleStatistics.Seats [i].SeatOwner == null && this.VehicleStatistics.VehicleTeam == PhotonNetwork.player.GetTeam() && CanEnter || this.VehicleStatistics.Seats [i].SeatOwner == null && this.VehicleStatistics.VehicleTeam == PunTeams.Team.none && CanEnter) {
				int SeatIndex = i;
				VehiclePhotonView.RPC ("OnPlayerEnter", PhotonTargets.AllBuffered, PhotonNetwork.player, SeatIndex);
				InGameUI.instance.PlayerHUDPanel.SetActive (false);
				GameManager.instance.InVehicle = true;
				break;
			}
		}
	}

	void OnPlayerChangeSeatRequests()
	{
		if (PlayerInputManager.instance.FirstSeat && GameManager.instance.InVehicle && VehicleStatistics.Seats.Length > 1) {
			if (VehicleStatistics.Seats [0].SeatOwner != null || VehicleStatistics.Seats [0].SeatOwner == PhotonNetwork.player) {
				VehiclePhotonView.RPC ("OnPlayerSeatChange", PhotonTargets.AllBuffered, PhotonNetwork.player, 0, GetPlayerCurrentSeatIndex());
			}
		}
		if (PlayerInputManager.instance.SecondSeat && GameManager.instance.InVehicle && VehicleStatistics.Seats.Length > 1) {
			if (VehicleStatistics.Seats [1].SeatOwner != null || VehicleStatistics.Seats [1].SeatOwner == PhotonNetwork.player) {
				VehiclePhotonView.RPC ("OnPlayerSeatChange", PhotonTargets.AllBuffered, PhotonNetwork.player, 1, GetPlayerCurrentSeatIndex());
			}
		}
	}

	void OnPlayerVehicleExitRequest()
	{
		if (PlayerInputManager.instance.UseButton && GameManager.instance.InVehicle && CanEnter) {
			for (int i = 0; i < this.VehicleStatistics.Seats.Length; i++) {	
				//Check if vehicle is not full and which seats are available at the moment
				if (this.VehicleStatistics.Seats [i].SeatOwner == PhotonNetwork.player) {
					int SeatIndex = i;
					VehiclePhotonView.RPC ("OnPlayerExit", PhotonTargets.AllBuffered, PhotonNetwork.player, SeatIndex);
					InGameUI.instance.PlayerHUDPanel.SetActive (true);
					GameManager.instance.InVehicle = false;
					break;
				}
			}
		}
	}

	public bool IsVehicleFullCheck()
	{
		count = 0;
		for (int i = 0; i < this.VehicleStatistics.Seats.Length; i++) {
			if (this.VehicleStatistics.Seats [i].SeatOwner != null) {
				count++;
			}
		}
		if (count == this.VehicleStatistics.Seats.Length) {
			return true;
		} else {
			return false;
		}
	}

	public bool IsVehicleEmptyCheck()
	{
		count = 0;
		for (int i = 0; i < this.VehicleStatistics.Seats.Length; i++) {
			if (this.VehicleStatistics.Seats [i].SeatOwner != null) {
				count++;
			}
		}
		if (count == 0) {
			return true;
		} else {
			return false;
		}
	}

	public int GetPlayerCurrentSeatIndex()
	{
		for (int i = 0; i < this.VehicleStatistics.Seats.Length; i++) {
			if (this.VehicleStatistics.Seats [i].SeatOwner == PhotonNetwork.player) {
				return i;
			}
		}
		return -1;
	}

	#region Vehicle Damage Code, not yet fully usable
	//Not fully usable has major bugs
	/*
	[PunRPC]
	public void FinishVehicleDamage(int damage, string source, PhotonPlayer attacker)
	{
		if (attacker.GetTeam () == PunTeams.Team.none || VehicleStatistics.VehicleTeam != attacker.GetTeam () || attacker == null && source == "Collision" || VehicleStatistics.VehicleTeam == PunTeams.Team.none) {
			if (source == "Collision") 
			{
				if (PhotonNetwork.isMasterClient) {
					if (VehicleHealth > 0) {		
						VehiclePhotonView.RPC ("SyncVehicleDamage", PhotonTargets.AllBuffered, damage);
						if (VehicleHealth <= VehicleSmokeDamagePoint && VehicleHealth >= VehicelFireDamagePoint) {
							VehiclePhotonView.RPC ("ChangeVehicleDamageState", PhotonTargets.AllBuffered, 1);
						}
						if (VehicleHealth <= VehicelFireDamagePoint && VehicleHealth > 0) {
							VehiclePhotonView.RPC ("ChangeVehicleDamageState", PhotonTargets.AllBuffered, 2);
						}
					}
					if (VehicleHealth <= 0) {	
						if (attacker == null) {
							attacker = PhotonNetwork.player;
						}
						VehicleHealth = 0;
						VehiclePhotonView.RPC ("ChangeVehicleDamageState", PhotonTargets.AllBuffered, 3);
						VehiclePhotonView.RPC ("OnVehicleKilled", PhotonTargets.All, "VehicleExp", attacker); //Send out an rpc to let others know this vehicle dead
					}
				}
			} else 
			{
				if (PhotonNetwork.player == VehicleStatistics.Seats [0].SeatOwner) {
					if (VehicleHealth > 0) {		
						VehiclePhotonView.RPC ("SyncVehicleDamage", PhotonTargets.AllBuffered, damage);
						if (VehicleHealth <= VehicleSmokeDamagePoint && VehicleHealth >= VehicelFireDamagePoint) {
							VehiclePhotonView.RPC ("ChangeVehicleDamageState", PhotonTargets.All, 1);
						}
						if (VehicleHealth <= VehicelFireDamagePoint && VehicleHealth > 0) {
							VehiclePhotonView.RPC ("ChangeVehicleDamageState", PhotonTargets.All, 2);
						}
					}
					if (VehicleHealth <= 0) {	
						if (attacker == null) {
							attacker = PhotonNetwork.player;
						}
						VehicleHealth = 0;
						VehiclePhotonView.RPC ("ChangeVehicleDamageState", PhotonTargets.AllBuffered, 3);
						VehiclePhotonView.RPC ("OnVehicleKilled", PhotonTargets.AllBuffered, "VehicleExp" , attacker); //Send out an rpc to let others know this vehicle dead
					}
				}
			}
		}
	}

	[PunRPC]
	public void SyncVehicleDamage(int dmg)
	{
		VehicleHealth = VehicleHealth - dmg;
	}

	[PunRPC]
	public void ChangeVehicleDamageState(int State)
	{
		if (State == 0) {
			VehicleDamageFire.Stop ();
			VehicleDamageSmoke.Stop ();
			VehicleDamageExplosion.Stop ();
		}
		if (State == 1) {
			VehicleDamageFire.Stop ();
			VehicleDamageSmoke.Play ();
			VehicleDamageExplosion.Stop ();
		}
		if (State == 2) {
			VehicleDamageFire.Play ();
			VehicleDamageSmoke.Play ();
			VehicleDamageExplosion.Stop ();
		}
		if (State == 3) {
			VehicleDamageFire.Play ();
			VehicleDamageSmoke.Play ();
			VehicleDamageExplosion.Play ();
			VehicleRigidbody.drag = 2f;
			//Destroy (VehicleWheels [1]);
			//Destroy (VehicleWheels [3]);
		}
	}

	[PunRPC]
	public void OnVehicleKilled(string source, PhotonPlayer attacker)
	{
		CanEnter = false;
		VehicleAlive = false;
		for (int i = 0; i < VehicleStatistics.Seats.Length; i++) {
			Behaviour[] SeatBehaviours = VehicleStatistics.Seats [i].SeatBehaviours;
			foreach (Behaviour KilledSeatBehaviour in SeatBehaviours) {
				KilledSeatBehaviour.enabled = false;
			}
			if (PhotonNetwork.player == VehicleStatistics.Seats [0].SeatOwner) { //If we are one of the seat occupants
				Debug.Log(VehicleStatistics.Seats [i].SeatOwner.NickName.ToString());
				VehicleDeathCam.SetActive (true);
				Invoke ("TurnOffDeathCamera", GameManager.instance.RespawnDelay);
				GameObject PlayerObject = PhotonNetwork.player.TagObject as GameObject;
				if(PlayerObject.GetComponent<PhotonView>().isMine)
					PlayerObject.GetComponent<PlayerStats> ().PlayerPhotonView.RPC ("ApplyPlayerDamage", PhotonTargets.All, 999, "RoadKill", attacker, 1f);
			}
		}
		if (PhotonNetwork.player == VehiclePhotonView.owner) {
			Invoke ("DeleteVehicle", GameManager.instance.RespawnDelay + 5f);
		}
	}

	void TurnOffDeathCamera()
	{
		GameManager.instance.SceneCamera.SetActive (true);
		VehicleDeathCam.SetActive (false);
	}

	public void DeleteVehicle()
	{
		PhotonNetwork.Destroy (this.gameObject);
	}

	private void OnCollisionEnter(Collision thisCollision)
	{
		if (VehicleStatistics.Seats[0].SeatOwner == PhotonNetwork.player) {
			VehiclePhotonView.RPC ("FinishVehicleDamage", PhotonTargets.All, Mathf.RoundToInt(thisCollision.relativeVelocity.magnitude) , "Collision", PhotonNetwork.player);
		}
	}*/
	#endregion
	#endregion

	#region Vehicle Remote Calls
	[PunRPC]
	public void OnPlayerEnter(PhotonPlayer player, int SeatIndex)
	{
		GameObject vehicleplayer = GameObject.Find (player.NickName);
		CanEnter = false; //Dont allow anyone for a short amount of time to enter the vehicle
		VehicleStatistics.Seats[SeatIndex].SeatPhotonView.TransferOwnership (player.ID);
		vehicleplayer.GetComponent<PhotonView> ().TransferOwnership (0);
		VehicleStatistics.Seats[SeatIndex].SeatOwner = player;
		VehicleStatistics.Seats[SeatIndex].SeatOwnerName = player.NickName;
		VehicleStatistics.VehicleTeam = player.GetTeam ();
		VehicleStatistics.PlayersInVehicle.Add (player);
		if (PhotonNetwork.player == player) {	//If this is the player who wanted to enter this vehicle
			foreach (Behaviour SeatBehaviour in VehicleStatistics.Seats[SeatIndex].SeatBehaviours) {
				SeatBehaviour.enabled = true;
			}
			GameManager.instance.CurrentVehicle = this.gameObject;
			//vehicleplayer.GetComponent<PlayerMovementController> ().enabled = false;
			vehicleplayer.GetComponent<PlayerWeaponManager> ().enabled = false;
			GameManager.instance.SetPlayerCameraActiveState (false);
			PlayerInputManager.instance.FreezePlayerControls = true;
		}
		if (PhotonNetwork.player == player) {
			vehicleplayer.GetComponent<CharacterController> ().enabled = false;
		}
		vehicleplayer.transform.SetParent (VehicleStatistics.Seats[SeatIndex].SeatPlayerPos);
		vehicleplayer.transform.localPosition = Vector3.zero;
		StartCoroutine (PlayerEnterDelay(1f));
	}

	IEnumerator PlayerEnterDelay(float TimeToWait)
	{
		yield return new WaitForSeconds (TimeToWait);
		CanEnter = true;
	}

	[PunRPC]
	public void OnPlayerSeatChange(PhotonPlayer player, int SeatIndex, int OldSeatIndex)
	{
		VehicleStatistics.Seats[OldSeatIndex].SeatPhotonView.TransferOwnership (0);
		VehicleStatistics.Seats[SeatIndex].SeatPhotonView.TransferOwnership (player.ID);
		if (PhotonNetwork.player == player) {	//If this is the player who wanted to enter this vehicle
			foreach (Behaviour SeatBehaviour in VehicleStatistics.Seats[OldSeatIndex].SeatBehaviours) {
				SeatBehaviour.enabled = false;
			}
			foreach (Behaviour SeatBehaviour in VehicleStatistics.Seats[SeatIndex].SeatBehaviours) {
				SeatBehaviour.enabled = true;
			}
		}
		GameObject vehicleplayer = GameObject.Find (player.NickName);
		vehicleplayer.transform.SetParent (VehicleStatistics.Seats[SeatIndex].SeatPlayerPos);
		vehicleplayer.transform.localPosition = Vector3.zero;
	}

	[PunRPC]
	public void OnPlayerExit(PhotonPlayer player, int SeatIndex)
	{
		if (player == null) {
			VehicleStatistics.PlayersInVehicle.Remove (player);
			VehicleStatistics.Seats [SeatIndex].SeatOwner = null;
			VehicleStatistics.Seats[SeatIndex].SeatOwnerName = null;
			VehicleStatistics.Seats[SeatIndex].SeatPhotonView.TransferOwnership (0);
			return;
		}
		GameObject vehicleplayer = GameObject.Find (player.NickName);
		CanEnter = false;
		//vehicleplayer.GetComponent<PhotonView> ().TransferOwnership (player.ID);
		VehicleStatistics.PlayersInVehicle.Remove (player);
		if (PhotonNetwork.player == player) {
			vehicleplayer.transform.position = VehicleStatistics.Seats[SeatIndex].SeatExit.position;
		}
		vehicleplayer.transform.parent = null;
		if (PhotonNetwork.player == player) {
			vehicleplayer.GetComponent<CharacterController> ().enabled = true;
		}
		if (PhotonNetwork.player == player) {	//If this is the player who wanted to enter this vehicle
			foreach (Behaviour SeatBehaviour in VehicleStatistics.Seats[SeatIndex].SeatBehaviours) {
				SeatBehaviour.enabled = false;
			}
			GameManager.instance.CurrentVehicle = null;
			GameManager.instance.SetPlayerCameraActiveState (true);
			//vehicleplayer.GetComponent<PlayerMovementController> ().enabled = true;
			vehicleplayer.GetComponent<PlayerWeaponManager> ().enabled = true;
			PlayerInputManager.instance.FreezePlayerControls = false;
		}
		VehicleStatistics.Seats [SeatIndex].SeatOwner = null;
		VehicleStatistics.Seats[SeatIndex].SeatOwnerName = null;
		vehicleplayer.GetComponent<PhotonView> ().TransferOwnership (player.ID);
		VehicleStatistics.Seats[SeatIndex].SeatPhotonView.TransferOwnership (0);
		InGameUI.instance.PlayerUseText.text = "";
		StartCoroutine (PlayerEnterDelay(1f));
	}
	#endregion
}

[System.Serializable]
public class Vehicle
{
	public string VehicleName;
	public PunTeams.Team VehicleTeam;
	public List<PhotonPlayer> PlayersInVehicle = new List<PhotonPlayer>();
	public VehicleType Type;
	public GameObject Object;
	public VehicleSeat[] Seats;
}

[System.Serializable]
public enum VehicleType
{
	FourWheeled,
	Helicopter
}

[System.Serializable]
public class VehicleSeat
{
	public string SeatName;
	public string SeatOwnerName;
	public PhotonPlayer SeatOwner;
	public Behaviour[] SeatBehaviours;
	public Transform SeatPlayerPos;
	public Transform SeatExit;
	public PhotonView SeatPhotonView;
}
