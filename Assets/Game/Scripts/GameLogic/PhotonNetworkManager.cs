using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonNetworkManager : MonoBehaviour {

	[HideInInspector]public static PhotonNetworkManager instance;
	[Header("Photon Networking Settings")]
	public string GameVersion = "";					//The current gameversion we are playing at, is used for determining the server list

	void Awake()
	{
		if (instance != null) {
			DestroyImmediate (gameObject);
		} else {
			instance = this;
			DontDestroyOnLoad (gameObject);
		}	
	}

	public void OnJoinedLobby() //When we have connected to the photonservices
	{
		MainMenuUI.instance.MainMenuPanel.SetActive (true); //Show the main menu
		PhotonNetwork.automaticallySyncScene = true;		//Automatically syncs the scene only when entering a new room
	}

	public void OnJoinedRoom()	//Called on ourself when we join a room
	{
		GameManager.instance.GameTimeLimit = float.Parse(PhotonNetwork.room.CustomProperties ["tl"].ToString()); //Sync the time limit to the other clients
		foreach (PhotonPlayer player in PhotonNetwork.otherPlayers) {
			if (player.NickName == PhotonNetwork.player.NickName) {		//If there is a player already with the same name as our player
				PhotonNetwork.player.NickName = PhotonNetwork.player.NickName + Random.Range(0,99).ToString(); //Change the name of our player to something unique
			}
		}
		StartCoroutine (WaitTillGameTypeDeclared());
	}

	public void OnPhotonRandomJoinFailed()
	{
		PhotonNetwork.Disconnect ();
		SceneManager.LoadScene ("MainMenu");
		GameManager.instance.ToggleLoadScreen(false, null);
		GameManager.instance.DisplayErrorMessage ("Error: No random game could be found!");
	}

	IEnumerator WaitTillGameTypeDeclared () {
		while ( PhotonNetwork.room.CustomProperties ["gm"].ToString () == null) {
			yield return new WaitForSeconds(0.1f); //Wait 0.1 sec before retrying
		}
		GameManager.instance.GetCurrentGameType();
	}

	public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged) //This function is called when the custom properties of a room is being changed
	{
		if (propertiesThatChanged.ContainsKey("StartTime")) //If there is a property called: StartTime
		{
			GameManager.instance.MatchActive = true; //The match has been started
			GameManager.instance.StartTime = (int) propertiesThatChanged["StartTime"]; //Save the starttime for later use
		}
	}

	public void OnGameError(string reason)			//If we encountered an error during loading the game
	{
		if (GameManager.instance.InVehicle && GameManager.instance.CurrentVehicle != null) {
			GameManager.instance.CurrentVehicle.GetComponent<VehicleStats> ().VehiclePhotonView.RPC ("OnPlayerExit", PhotonTargets.AllBuffered, PhotonNetwork.player, GameManager.instance.CurrentVehicle.GetComponent<VehicleStats> ().GetPlayerCurrentSeatIndex ());
		}
		Debug.LogError (reason); //Displays a error in the console when this function is called and has specified a reason
		GameManager.instance.Ingame = false; //Set our ingame state to false
		GameManager.instance.ClearMatchSettings ();   //Clear the current match's settings
		PhotonNetwork.Disconnect (); //Disconnect from the game
	}

	public void OnLeftRoom()						//Called on ourself when we leave the current room
	{
		GameManager.instance.ResetPlayerStats ();	//Reset our player stats
	}

	public void OnPhotonPlayerConnected(PhotonPlayer player)													//Called when a player joins
	{
		GameManager.instance.AddGameMessage ("<color=green>" + player.NickName + "</color> joined the game!");	//Display a player join message
	}

	public void OnPhotonPlayerDisconnected(PhotonPlayer player)													//Called when player leaves
	{
		GameManager.instance.AddGameMessage ("<color=red>" + player.NickName + "</color> left the game!");		//Display a player leave message
	}
}
