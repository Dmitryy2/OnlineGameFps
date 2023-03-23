using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TDM : MonoBehaviour {
/*
	Team DeathMatch
	Objective: Eliminate enemy team players till the time limit or score limit has been reached!
	Respawn: Auto-respawn 5 seconds after being killed
*/
	void  OnEnable()
	{
		EventManager.StartListening ("OnPlayerRespawn", OnPlayerRespawn);
		EventManager.StartListening ("OnPlayerSpawn", OnPlayerSpawn);
		EventManager.StartListening ("DisableGameType", DisableGameType);
		EventManager.StartListening ("ShowSpawnMenu", ShowSpawnMenu);
		EventManager.StartListening ("AddRedTeamScore", AddRedTeamScore);
		EventManager.StartListening ("AddBlueTeamScore", AddBlueTeamScore);
		EventManager.StartListening ("GameTypeEndGame", GameTypeEndGame);
		StartGameLogic ();
	}

	void DisableGameType()
	{
		EventManager.StopListening ("OnPlayerRespawn", OnPlayerRespawn);
		EventManager.StopListening ("OnPlayerSpawn", OnPlayerSpawn);
		EventManager.StopListening ("DisableGameType", DisableGameType);
		EventManager.StopListening ("ShowSpawnMenu", ShowSpawnMenu);
		EventManager.StopListening ("AddRedTeamScore", AddRedTeamScore);
		EventManager.StopListening ("AddBlueTeamScore", AddBlueTeamScore);
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
		UpdateTeamScore ();
	}	

	void StartGameLogic()
	{
		InGameUI.instance.LevelFadeIn ();
		GameManager.instance.Teambased = true;
		PlayerInputManager.instance.FreezePlayerControls = false;
		GameManager.instance.HasTeam = false;
		GameManager.instance.SetCursorLock (false);
		GameManager.instance.CanSpawn = true;
		GameManager.instance.InVehicle = false;
		GameManager.instance.GameScoreLimit = int.Parse(PhotonNetwork.room.CustomProperties ["sl"].ToString ());
		InGameUI.instance.ServerNameText.text = PhotonNetwork.room.Name;
		for (int i = 0; i < GameManager.instance.MapList.Length; i++) {
			if (GameManager.instance.MapList [i].MapName == PhotonNetwork.room.CustomProperties ["map"].ToString ()) {
				GameManager.instance.CurrentMap = GameManager.instance.MapList [i];
			}
		}
		InGameUI.instance.ServerGameTypeText.text = GameManager.instance.CurrentGameType.GameTypeFullName;
		if (PhotonNetwork.isMasterClient) {
			ExitGames.Client.Photon.Hashtable ht = new ExitGames.Client.Photon.Hashtable();
			ht.Add("StartTime", PhotonNetwork.ServerTimestamp);
			PhotonNetwork.room.SetCustomProperties(ht);
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
		InGameUI.instance.TDMSpawnMenu.SetActive (true); //Enable the TDM spawn menu
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
		InGameUI.instance.TDMSpawnMenu.SetActive(false);
		InGameUI.instance.ServerInfoPanel.SetActive (false);
		InGameUI.instance.TimerScoreHolder.SetActive (true);
		InGameUI.instance.PlayerHUDPanel.SetActive (true);
		GameManager.instance.IsAlive = true;
		GameManager.instance.HasTeam = true;
		InGameUI.instance.CanPause = true;
		GameManager.instance.CurrentTeam = PhotonNetwork.player.GetTeam ();
		if (!InGameUI.instance.Paused) {
			GameManager.instance.SetCursorLock (true);
			PlayerInputManager.instance.FreezePlayerControls = false;
		}
		Invoke("SetPlayerWeapon", 0.1f); //Gives the player his weapon after 0.1 seconds
	}

	public void AddRedTeamScore()
	{
		PunTeams.Team.red.AddTeamScore (1);
		ScoreLimitCheck ();
	}

	public void AddBlueTeamScore()
	{
		PunTeams.Team.blue.AddTeamScore (1);
		ScoreLimitCheck ();
	}

	void ScoreLimitCheck()
	{
		if (PunTeams.Team.red.GetTeamScore () == GameManager.instance.GameScoreLimit) {
			GameManager.instance.GameManagerPhotonView.RPC ("EndGame", PhotonTargets.AllBuffered, "Red team wins!");	//Send out a rpc that tells all players the match has ended
		}
		if (PunTeams.Team.blue.GetTeamScore () == GameManager.instance.GameScoreLimit) {
			GameManager.instance.GameManagerPhotonView.RPC ("EndGame", PhotonTargets.AllBuffered, "Blue team wins!");	//Send out a rpc that tells all players the match has ended
		}
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
		//PhotonNetwork.RemoveRPCs (GetComponent<PhotonView>());
		if (PhotonNetwork.isMasterClient) {
			GameManager.instance.ResetTeamScores ();
		}
		GameManager.instance.GameManagerPhotonView.RPC ("LoadNextMap", PhotonTargets.AllBuffered, "TestMap");
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
				InGameUI.instance.IngameTimerText.text = string.Format ("{0:0}:{1:00}", minutes, seconds);	//Set the time limit to a digital clock like notation
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
		InGameUI.instance.TeamScoreBoardPanel.SetActive (true);
		if (PunTeams.PlayersPerTeam [PunTeams.Team.red].Count != 0) {
			foreach (PhotonPlayer RedTeamPlayer in PunTeams.PlayersPerTeam[PunTeams.Team.red].ToArray()) {
				GameObject RedTeamScoreEntry = Instantiate (InGameUI.instance.RedTeamScoreBoardEntry, InGameUI.instance.RedTeamScoreBoardPanel);
				ScoreBoardEntry RedScoreEntryProps = RedTeamScoreEntry.GetComponent<ScoreBoardEntry> ();
				RedScoreEntryProps.PlayerNameText.text = RedTeamPlayer.NickName;
				RedScoreEntryProps.PlayerScoreText.text = RedTeamPlayer.GetScore ().ToString();
				RedScoreEntryProps.PlayerKillsText.text = RedTeamPlayer.GetKills ().ToString();
				RedScoreEntryProps.PlayerDeathsText.text = RedTeamPlayer.GetDeaths ().ToString();
			}
		}
		if (PunTeams.PlayersPerTeam [PunTeams.Team.blue].Count != 0) {
			foreach (PhotonPlayer BlueTeamPlayer in PunTeams.PlayersPerTeam[PunTeams.Team.blue].ToArray()) {
				GameObject BlueTeamScoreEntry = Instantiate (InGameUI.instance.BlueTeamScoreBoardEntry, InGameUI.instance.BlueTeamScoreBoardPanel);
				ScoreBoardEntry BlueScoreEntryProps = BlueTeamScoreEntry.GetComponent<ScoreBoardEntry> ();
				BlueScoreEntryProps.PlayerNameText.text = BlueTeamPlayer.NickName;
				BlueScoreEntryProps.PlayerScoreText.text = BlueTeamPlayer.GetScore ().ToString ();
				BlueScoreEntryProps.PlayerKillsText.text = BlueTeamPlayer.GetKills ().ToString ();
				BlueScoreEntryProps.PlayerDeathsText.text = BlueTeamPlayer.GetDeaths ().ToString ();
			}
		}
	}

	void HideScoreBoard()
	{
		GameObject[] ScoreBoardEntries = GameObject.FindGameObjectsWithTag("ScoreBoardEntry");
		foreach (GameObject ScoreEntry in ScoreBoardEntries) {
			Destroy (ScoreEntry);
		}
		InGameUI.instance.TeamScoreBoardPanel.SetActive (false);
	}

	public void UpdateTeamScore()
	{
		InGameUI.instance.RedTeamScoreText.text = PunTeams.Team.red.GetTeamScore ().ToString();
		InGameUI.instance.BlueTeamScoreText.text = PunTeams.Team.blue.GetTeamScore ().ToString();
	}
	#endregion
}