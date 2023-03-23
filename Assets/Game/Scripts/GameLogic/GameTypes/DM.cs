
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DM : MonoBehaviour {
	/*
	DeathMatch
	Objective: Eliminate enemy players till the time limit or score limit has been reached!
	Respawn: Auto-respawn 5 seconds after being killed
	*/
	void  OnEnable()
	{
		EventManager.StartListening ("OnPlayerRespawn", OnPlayerRespawn);
		EventManager.StartListening ("OnPlayerSpawn", OnPlayerSpawn);
		EventManager.StartListening ("DisableGameType", DisableGameType);
		EventManager.StartListening ("ShowSpawnMenu", ShowSpawnMenu);
		EventManager.StartListening ("GameTypeEndGame", GameTypeEndGame);
		StartGameLogic ();
	}

	void DisableGameType()
	{
		EventManager.StopListening ("OnPlayerRespawn", OnPlayerRespawn);
		EventManager.StopListening ("OnPlayerSpawn", OnPlayerSpawn);
		EventManager.StopListening ("DisableGameType", DisableGameType);
		EventManager.StopListening ("ShowSpawnMenu", ShowSpawnMenu);
		EventManager.StopListening ("GameTypeEndGame", GameTypeEndGame);
		this.enabled = false;
	}

	void Update()
	{
		if (GameManager.instance.MatchActive) {
			HandleIngameTimer ();
			DisplayIngameTimer ();
		}
		ToggleScoreboard ();
	}	

	void StartGameLogic()
	{
		PlayerInputManager.instance.FreezePlayerControls = false;
		GameManager.instance.HasTeam = false;
		GameManager.instance.SetCursorLock (false);
		GameManager.instance.CanSpawn = true;
		GameManager.instance.InVehicle = false;
		InGameUI.instance.ServerNameText.text = PhotonNetwork.room.Name;
		GameManager.instance.GameScoreLimit = int.Parse(PhotonNetwork.room.CustomProperties ["sl"].ToString ());
		for (int i = 0; i < GameManager.instance.MapList.Length; i++) {
			if (GameManager.instance.MapList [i].MapName == PhotonNetwork.room.CustomProperties ["map"].ToString ()) {
				GameManager.instance.CurrentMap = GameManager.instance.MapList [i];
			}
		}

		InGameUI.instance.CurrentMapText.text = GameManager.instance.CurrentMap.MapName;
		InGameUI.instance.ServerGameTypeText.text = GameManager.instance.CurrentGameType.GameTypeFullName;
		if (PhotonNetwork.isMasterClient) { //If we are the master client
			ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable(); //Create a new photon hashtable
			ht.Add("StartTime", PhotonNetwork.ServerTimestamp); 							//Saves the servertimestamp in the hashtable
			PhotonNetwork.room.SetCustomProperties(ht);										//Add the hashtable to the room properties to be accessed by other clients
		}
		ShowSpawnMenu ();
	}

	#region Game Type Events
	public void ShowSpawnMenu()
	{
		GameManager.instance.SceneCamera.SetActive (true);
		if (InGameUI.instance.Paused) {
			InGameUI.instance.ResumeGame ();	//Hides the pause screen if the spawn screen pops up when still pausede ingame
		}
		InGameUI.instance.EnableChatInputfield(false);
		InGameUI.instance.DMSpawnMenu.SetActive (true); //Enable the DM spawn menu
		InGameUI.instance.ServerInfoPanel.SetActive (true); //Show the server info on the right side of the screen
		GameManager.instance.SetCursorLock(false);
	}
	public void OnPlayerRespawn()
	{
		//Actions regarding the gametype to preform on the local player each time they want to respawn
		GameManager.instance.SpawnPlayer (GameManager.instance.CurrentTeam.ToString());
	}

	public void OnPlayerSpawn()
	{
		//Actions regarding the gametype to preform on the local player each time they spawn
		InGameUI.instance.DMSpawnMenu.SetActive(false);
		InGameUI.instance.ServerInfoPanel.SetActive (false);
		InGameUI.instance.TimerScoreHolder.SetActive (true);
		InGameUI.instance.PlayerHUDPanel.SetActive (true);
		InGameUI.instance.CanPause = true;
		GameManager.instance.IsAlive = true;
		GameManager.instance.CurrentTeam = PhotonNetwork.player.GetTeam ();
		if (!InGameUI.instance.Paused) {
			GameManager.instance.SetCursorLock (true);
			PlayerInputManager.instance.FreezePlayerControls = false;
		}
		Invoke("SetPlayerWeapon", 0.1f); //Gives the player his weapon after 0.1 seconds
	}
		
	void SetPlayerWeapon()
	{
		GameObject LocalPlayer = PhotonNetwork.player.TagObject as GameObject; 										//Get the localplayer gameobject
		LocalPlayer.GetComponent<PlayerWeaponManager> ().GiveWeapon (GameManager.instance.PlayerPrimaryWeapon);	   	//Give the player's primary weapon
		LocalPlayer.GetComponent<PlayerWeaponManager> ().GiveWeapon (GameManager.instance.PlayerSecondaryWeapon);	//Give the player's secondary weapon
		LocalPlayer.GetComponent<PlayerWeaponManager> ().EquipWeapon (0);	   										//Equip the first weapon in our inventory
	}

	public void GameTypeEndGame()
	{
		GameManager.instance.CanSpawn = false;
		GameManager.instance.IsAlive = false;
		InGameUI.instance.PlayerHUDPanel.SetActive (false);
		GameManager.instance.MatchActive = false;
		GameManager.instance.CurrentTeam =PunTeams.Team.none;
		GameManager.instance.HasTeam = false;
		GameManager.instance.InVehicle = false;
		PlayerInputManager.instance.FreezePlayerControls = true;
		if (PhotonNetwork.isMasterClient) {
			StartCoroutine (WaitToLoadNextMap ());
		}
		InGameUI.instance.EndGamePanel.SetActive (true);
	}

	IEnumerator WaitToLoadNextMap()
	{
		yield return new WaitForSeconds (GameManager.instance.EndGameTime);
		PhotonNetwork.RemoveRPCs (GameManager.instance.GameManagerPhotonView);
		GameManager.instance.GameManagerPhotonView.RPC ("LoadNextMap", PhotonTargets.All, "TestMap");
	}
	#endregion

	#region Game Type UI
	void HandleIngameTimer()
	{
		if (GameManager.instance.MatchActive && GameManager.instance.GameTimeLimit != 0) { //If the match is currently active and the time limit isnt set to unlimited
			GameManager.instance.GameTimeLeft = (GameManager.instance.GameTimeLimit * 60f) - ((PhotonNetwork.ServerTimestamp - GameManager.instance.StartTime) / 1000.0f); //Calculate the time left
		} 
		if(GameManager.instance.GameTimeLeft < 0) { //If the time left is below 0
			GameManager.instance.MatchActive = false;	//The match is not active anymore
			GameManager.instance.GameTimeLeft = 0;		//Set the time left to 0
			GameManager.instance.GameManagerPhotonView.RPC ("EndGame", PhotonTargets.AllBuffered, "Time limit Reached!");	//Send out a rpc that tells all players the match has ended
		}
	}

	void DisplayIngameTimer()
	{
		if (GameManager.instance.GameTimeLimit != 0) {
			if (!GameManager.instance.MatchActive) {
				return;
			} else {
				int minutes = Mathf.FloorToInt (GameManager.instance.GameTimeLeft / 60F);
				int seconds = Mathf.FloorToInt (GameManager.instance.GameTimeLeft - minutes * 60);
				InGameUI.instance.IngameTimerText.text = string.Format ("{0:0}:{1:00}", minutes, seconds); //Set the time limit to a digital clock like notation
			}
		} else {
			InGameUI.instance.IngameTimerText.fontSize = 50;
			InGameUI.instance.IngameTimerText.text = "Unlimited";
		}
	}

	void ToggleScoreboard()
	{
		if (PlayerInputManager.instance.Scoreboard) {
			if (!InGameUI.instance.ShowScoreBoard) {
				ShowScoreBoard ();
				InGameUI.instance.ShowScoreBoard = true;
			}
		} else {
			if (InGameUI.instance.ShowScoreBoard) {
				HideScoreBoard ();
				InGameUI.instance.ShowScoreBoard = false;
			}
		}
	}

	void ShowScoreBoard()
	{
		InGameUI.instance.DeathMatchScoreBoardPanel.SetActive (true);
		InGameUI.instance.GameTypeNameText.text = GameManager.instance.CurrentGameType.GameTypeFullName;
		if (PunTeams.PlayersPerTeam [PunTeams.Team.none].Count != 0) {
			foreach (PhotonPlayer DeathMatchPlayer in PunTeams.PlayersPerTeam[PunTeams.Team.none].ToArray()) {
				GameObject DeathMatchScoreEntry = Instantiate (InGameUI.instance.DeathMatchScoreBoardEntry, InGameUI.instance.DeatMatchScoreBoardEntryAnchor);
				ScoreBoardEntry DeathMatchScoreEntryProps = DeathMatchScoreEntry.GetComponent<ScoreBoardEntry> ();
				DeathMatchScoreEntryProps.PlayerNameText.text = DeathMatchPlayer.NickName;
				DeathMatchScoreEntryProps.PlayerScoreText.text = DeathMatchPlayer.GetScore ().ToString();
				DeathMatchScoreEntryProps.PlayerKillsText.text = DeathMatchPlayer.GetKills ().ToString();
				DeathMatchScoreEntryProps.PlayerDeathsText.text = DeathMatchPlayer.GetDeaths ().ToString();
			}
		}
	}

	void HideScoreBoard()
	{
		GameObject[] ScoreBoardEntries = GameObject.FindGameObjectsWithTag("ScoreBoardEntry");
		foreach (GameObject ScoreEntry in ScoreBoardEntries) {
			Destroy (ScoreEntry);
		}
		InGameUI.instance.DeathMatchScoreBoardPanel.SetActive (false);
	}
	#endregion
}
