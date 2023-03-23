using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractables : MonoBehaviour {

	public PhotonView PlayerPhotonView;
	public bool PlayerInTrigger = false;
	public Collider CurrentTrigger;

	void Update () {
		OnPlayerSuicide ();
		MonitorTriggers ();
	}

	#region Player Actions
	void OnPlayerSuicide()
	{
		if (PlayerInputManager.instance.Suicide && GameManager.instance.IsAlive) {				//When we press P and are currently alive
			PlayerPhotonView.RPC ("ApplyPlayerDamage", PhotonTargets.All, 999, "World", PhotonNetwork.player, 1f, true);	//Deal 999 damage to ourself with the world as source
		}
	}

	void PlayerHurtTrigger(int dmg)
	{
		if (dmg != 0) {
			PlayerPhotonView.RPC ("ApplyPlayerDamage", PhotonTargets.All, dmg, "HurtTrigger", PhotonNetwork.player, 1f, true);	//Deal 999 damage to ourself with the hurttrigger as source
		}
	}

	void PlayerLandMine()
	{
		PlayerPhotonView.RPC ("ApplyPlayerDamage", PhotonTargets.All, 999, "Mine", PhotonNetwork.player, 1f, true);	//Deal 999 damage to ourself 
		PlayerPhotonView.RPC ("PlayFXAtPosition", PhotonTargets.All, 0, this.transform.position, Vector3.zero);
	}
	#endregion

	#region Triggers
	void OnTriggerEnter(Collider other)	//When we enter a trigger
	{
		PlayerInTrigger = true;			//Set the PlayerInTrigger variable to true
		CurrentTrigger = other;			//Set the CurrentTrigger to the trigger we just walked into
	}

	void MonitorTriggers()
	{
		if (PlayerInTrigger) {
			if (CurrentTrigger.tag == "UseTrigger" && CurrentTrigger != null) { //Check is this trigger is tagged with UseTrigger
				if (!CurrentTrigger.GetComponent<Trigger> ().Triggered) { //If the trigger has not yet been triggered by the player
					InGameUI.instance.PlayerUseText.text = CurrentTrigger.GetComponent<Trigger> ().HintString; //Display the hintstring
				} else {										//If it is already triggered
					InGameUI.instance.PlayerUseText.text = "";  //Hide the hintstring again
				}
				if (PlayerInputManager.instance.UseButton && GameManager.instance.IsAlive && !CurrentTrigger.GetComponent<Trigger> ().Triggered) { //When the player presses F and is currently alive and the trigger has not yet been triggered
					PhotonNetwork.player.AddScore (5);							//Give the player some score for activating
					CurrentTrigger.GetComponent<Collider> ().SendMessage ("TriggerInteractable"); //Trigger the action linked to the trigger and send the triggered state to all other clients
					CurrentTrigger.GetComponent<Trigger> ().Triggered = true;			//Set the triggered state on the local client to true
				}
			}
			if (CurrentTrigger.tag == "VehicleTrigger" && !GameManager.instance.InVehicle && GameManager.instance.IsAlive) { //Check if this is tagged with VehicleTrigger and check if we currently aren't in a vehicle
				if (CurrentTrigger.transform.root.GetComponent<VehicleStats> ().IsVehicleFullCheck() == false && CurrentTrigger.transform.root.GetComponent<VehicleStats> ().CanEnter == true) {
					InGameUI.instance.PlayerUseText.text = VehicleManager.instance.VehicleEnterHinstring; //Display the hintstring
				}
				if (PlayerInputManager.instance.UseButton && GameManager.instance.IsAlive) {		//When the player presses F and is currently alive
					CurrentTrigger.transform.root.gameObject.SendMessage ("OnPlayerVehicleEnterRequest");    //Send a message to the vehicle object that a player requests to enter it
					InGameUI.instance.PlayerUseText.text = "";										//Hide the hinstring
				}
			}
		} else {
			InGameUI.instance.PlayerUseText.text = "";											//Hide the hinstring
		}
	}

	void OnTriggerExit(Collider other)														//When we are walking out the trigger again
	{
		PlayerInTrigger = false;															//Set the PlayerInTrigger variable to true																										
		CurrentTrigger = null;																//Empty the CurrentTrigger variable
	}
	#endregion


}
