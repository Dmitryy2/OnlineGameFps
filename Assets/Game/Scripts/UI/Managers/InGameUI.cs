using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour {

	[HideInInspector]public static InGameUI instance;
	[HideInInspector]public bool ShowScoreBoard; //This variable is used to determine if the scoreboard is currently being displayed
	[Header("Player Loadout References")]
	public GameObject WeaponLoadoutPanel;
	public Button[] LoadoutSelectionButtons;
	public Image WeaponPrimaryPreview;
	public Text WeaponPrimaryName;
	public Image WeaponSecondaryPreview;
	public Text WeaponSecondaryName;
	[Space(10)]
	public Image PrimarySelectPreview;
	public Text PrimarySelectNameText;
	public Text PrimarySelectDescText;
	[Space(10)]
	public Image SecondarySelectPreview;
	public Text SecondarySelectNameText;
	public Text SecondarySelectDescText;
	[Space(10)]
	[Header("Team DeathMatch References")]
	public GameObject TDMSpawnMenu;
	public GameObject[] TeamScoreTexts;
	public Text RedTeamScoreText;
	public Text BlueTeamScoreText;
	public GameObject TeamScoreBoardPanel;
	public Transform BlueTeamScoreBoardPanel;
	public Transform RedTeamScoreBoardPanel;
	public GameObject BlueTeamScoreBoardEntry;
	public GameObject RedTeamScoreBoardEntry;
	[Space(10)]
	[Header("DeathMatch References")]
	public GameObject DMSpawnMenu;
	public Text WelcomeText;
	public GameObject DeathMatchScoreBoardPanel;
	public Text GameTypeNameText;
	public Transform DeatMatchScoreBoardEntryAnchor;
	public GameObject DeathMatchScoreBoardEntry;
	[Space(10)]
	[Header("Player Base HUD References")]
	public GameObject PlayerHUDPanel;
	public GameObject Crosshair;
	public Text WeaponNameText;
	public Text WeaponCurrentMagazineAmmoText;
	public Text WeaponReserveClipsText;
	public Text PlayerHealthText;
	public Text PlayerUseText;
	[Space(10)]
	[Header("Game Timer / Team Score References")]
	public GameObject TimerScoreHolder;
	public GameObject TimerHolder;
	public GameObject EndGamePanel;
	public Text EndGameReasonText;
	public Text IngameTimerText;
	[Space(10)]
	[Header("ServerInfo Panel References")]
	public GameObject ServerInfoPanel;
	public Text ServerNameText;
	public Text ServerGameTypeText;
	public Text CurrentMapText;
	[Space(10)]
	[Header("Pause Menu References")]
	public GameObject PauseMenuPanel;
	public bool Paused;
	public bool CanPause = false;
	[Space(10)]
	[Header("Killfeed References")]
	public GameObject KillfeedPanel;
	public GameObject KillFeedEntryPrefab;
	[Space(10)]
	[Header("Chat References")]
	public GameObject ChatPanel;
	public GameObject ChatEntryPrefab;
	public GameObject ChatTypePanel;
	public InputField ChatInputfield;
	public bool WasTyping;
	[Space(10)]
	[Header("Hitmarker & Hitscreen References")]
	public Image Hitmarker;
	public Image HitmarkerKill;
	public Image Hitscreen;
	[Space(10)]
	[Header("Misc References")]
	public Image LevelFadeInImage;

	void Awake()
	{
		instance = this;
	}

	void Update()
	{
		PauseMenuToggle ();
		ChatActionHandler ();
	}

	#region Spawn Menu
	public void JoinRedTeam()
	{
		if (GameManager.instance.CanSpawn) {
			GameManager.instance.SpawnPlayer ("red");
			foreach (GameObject ScoreText in InGameUI.instance.TeamScoreTexts) {
				ScoreText.SetActive (true);
			}
		}
	}

	public void JoinBlueTeam()
	{
		if (GameManager.instance.CanSpawn) {
			GameManager.instance.SpawnPlayer ("blue");
			foreach (GameObject ScoreText in InGameUI.instance.TeamScoreTexts) {
				ScoreText.SetActive (true);
			}
		}
	}

	public void SpawnButton()
	{
		if (GameManager.instance.CanSpawn) {
			GameManager.instance.SpawnPlayer ("none"); 
		}
	}

	public void AutoAssign()
	{
		if (PunTeams.PlayersPerTeam [PunTeams.Team.red].Count != 0 && PunTeams.PlayersPerTeam [PunTeams.Team.blue].Count != 0) { //If both teams are currently empty
			if (PunTeams.PlayersPerTeam [PunTeams.Team.red].Count > PunTeams.PlayersPerTeam [PunTeams.Team.blue].Count) { //If there are more players in the red team than the blue team
				GameManager.instance.SpawnPlayer ("blue");																  //Assign us to the blue team
			}
			if (PunTeams.PlayersPerTeam [PunTeams.Team.blue].Count > PunTeams.PlayersPerTeam [PunTeams.Team.red].Count) { //If there are more players in the blue team than the red team
				GameManager.instance.SpawnPlayer ("red");																  //Assign us to the red team
			}
		} else if(PunTeams.PlayersPerTeam [PunTeams.Team.red].Count == 0){ 												  
			GameManager.instance.SpawnPlayer ("red"); 																	 
		} else if(PunTeams.PlayersPerTeam [PunTeams.Team.blue].Count == 0){ 											
			GameManager.instance.SpawnPlayer ("blue"); 																	
		}
	}

	public void LeaveGame()
	{
		if (GameManager.instance.InVehicle && GameManager.instance.CurrentVehicle != null) {
			GameManager.instance.CurrentVehicle.GetComponent<VehicleStats> ().VehiclePhotonView.RPC ("OnPlayerExit", PhotonTargets.AllBuffered, PhotonNetwork.player, GameManager.instance.CurrentVehicle.GetComponent<VehicleStats> ().GetPlayerCurrentSeatIndex ());
		}
		GameManager.instance.Ingame = false;
		GameManager.instance.ClearMatchSettings ();
		PhotonNetwork.DestroyPlayerObjects (PhotonNetwork.player.ID);
		PhotonNetwork.Disconnect (); //Disconnect from the game
		SceneManager.LoadScene (0); //Load the main menu scene
	}
	#endregion

	#region PauseMenu
	void PauseMenuToggle()
	{
		if (PlayerInputManager.instance.Pause && GameManager.instance.MatchActive && GameManager.instance.IsAlive && CanPause) {
			if (Paused) {
				ResumeGame ();
			} else {
				PauseGame ();
			}
		}
	}

	public void ResumeGame()
	{
		Paused = false;
		PauseMenuPanel.SetActive (false);
		GameManager.instance.SetCursorLock (true);
		if (!@GameManager.instance.InVehicle) {
			PlayerInputManager.instance.FreezePlayerControls = false;
		}
		ServerInfoPanel.SetActive (false);
	}

	public void PauseGame()
	{
		Paused = true;
		PauseMenuPanel.SetActive (true);
		GameManager.instance.SetCursorLock (false);
		PlayerInputManager.instance.FreezePlayerControls = true;
		ServerInfoPanel.SetActive (true);
	}
	#endregion

	#region Loadout
	public void OpenLoadoutMenu(string previousmenu)
	{
		if (GameManager.instance.AllowLoadout) {
			WeaponLoadoutPanel.SetActive (true);
			if (previousmenu == "TDM") {
				TDMSpawnMenu.SetActive (false);
			} else if (previousmenu == "DM") {
				DMSpawnMenu.SetActive (false);
			} else if (previousmenu == "Pause") {
				PauseMenuPanel.SetActive (false);
			}
		}
	}

	public void OnLoadoutMenu()
	{
		if (GameManager.instance.AllowLoadout) 
			CanPause = false;
	}

	public void OnLoadOutMenuExit()
	{
		CanPause = true;
		if (!GameManager.instance.IsAlive && !InGameUI.instance.Paused) {
			EventManager.TriggerEvent ("ShowSpawnMenu");
		} else if (InGameUI.instance.Paused) {
			PauseGame ();
		}
	}

	public void SetLoadOutSelectionButton(bool Toggle)
	{
		foreach (Button LoadoutSelectionButton in LoadoutSelectionButtons) {
			LoadoutSelectionButton.interactable = Toggle;
		}
	}

	public void ShowPrimarySelectPreview(int WeaponID)
	{
		PrimarySelectPreview.sprite = GameManager.instance.AllGameWeapons [WeaponID].WeaponIcon;
		PrimarySelectNameText.text = GameManager.instance.AllGameWeapons [WeaponID].WeaponName;
		PrimarySelectDescText.text = GameManager.instance.AllGameWeapons [WeaponID].WeaponDesc;
	}

	public void ShowSecondarySelectPreview(int WeaponID)
	{
		SecondarySelectPreview.sprite = GameManager.instance.AllGameWeapons [WeaponID].WeaponIcon;
		SecondarySelectNameText.text = GameManager.instance.AllGameWeapons [WeaponID].WeaponName;
		SecondarySelectDescText.text = GameManager.instance.AllGameWeapons [WeaponID].WeaponDesc;
	}

	public void SelectPrimaryWeapon(int WeaponID)
	{
		InGameUI.instance.WeaponPrimaryPreview.sprite = GameManager.instance.AllGameWeapons [WeaponID].WeaponIcon;
		InGameUI.instance.WeaponPrimaryName.text = GameManager.instance.AllGameWeapons [WeaponID].WeaponName;
		GameManager.instance.PlayerPrimaryWeapon = WeaponID;
	}

	public void SelectSecondaryWeapon(int WeaponID)
	{
		InGameUI.instance.WeaponSecondaryPreview.sprite = GameManager.instance.AllGameWeapons [WeaponID].WeaponIcon;
		InGameUI.instance.WeaponSecondaryName.text = GameManager.instance.AllGameWeapons [WeaponID].WeaponName;
		GameManager.instance.PlayerSecondaryWeapon = WeaponID;
	}
	#endregion

	#region InGameChat
	void ChatActionHandler()
	{
		if (Input.GetKeyDown (KeyCode.T)) { //If we press the T key
			EnableChatInputfield (true);	//Show the chat inputfield
		}
		if (Input.GetKeyDown (KeyCode.Return) && WasTyping) { //If we hit return and we were currently using the chat inputfield
			if (ChatInputfield.text.Length < 1) {			  //If the length if the typed text is below 1 character
				EnableChatInputfield (false);				  //Disable the chat inputfield
				return;										  
			}
			GameManager.instance.gameObject.GetComponent<PhotonView> ().RPC ("AddChatMessage", PhotonTargets.All, ChatInputfield.text, PhotonNetwork.player); //Send the message that we just typed to all players
			EnableChatInputfield (false);					 //Disable the chat inputfield
		}
		if (Paused && WasTyping) {							 //if we were typing in chat and we hit the pause button
			EnableChatInputfield (false);					 //Disable the chat inputfield
		} 
	}

	public void EnableChatInputfield(bool Toggle)
	{
		WasTyping = Toggle;									
		PlayerInputManager.instance.FreezePlayerControls = Toggle;
		if (!Paused && !DMSpawnMenu.activeInHierarchy || !Paused && !TDMSpawnMenu.activeInHierarchy) { //If we arent paused or when any of the spawn menu is currently open, dont alter the cursorlock
			GameManager.instance.SetCursorLock (!Toggle);
		}
		ChatTypePanel.SetActive (Toggle);
		if (Toggle == true) {
			ChatInputfield.ActivateInputField ();
		} else {
			ChatInputfield.text = "";
		}
	}
	#endregion

	#region Hitmarker & Hitscreen
	public void DoHitMarker()
	{
		//StopCoroutine (ShowTimedImageFade (Hitmarker, 255f, 0f));
		//Hitmarker.gameObject.SetActive (true);
		StartCoroutine (ShowTimedImageFadeOut (Hitmarker, 1f ,0.1f));
	}

	public void DoHitscreen(float TimeToShow)
	{
		//StopCoroutine (ShowTimedImageFade (0f, Hitscreen));
		//Hitscreen.gameObject.SetActive (true);
		StartCoroutine (ShowTimedImageFadeOut (Hitscreen, 1f ,0.1f));
	}

	public void LevelFadeIn()
	{
		StartCoroutine (ShowTimedImageFadeOut (LevelFadeInImage, 0.1f ,0.01f));
	}

	IEnumerator ShowTimedImageFadeOut(Image FadeImage, float TimeBeforeStartFade ,float TimeStep)
	{
		Color ColorWithAlpha = FadeImage.color;
		ColorWithAlpha.a = 1f;
		FadeImage.color = ColorWithAlpha;
		yield return new WaitForSeconds (TimeBeforeStartFade);
		for (float f = 1f; f >= -TimeStep; f -= TimeStep) {
			Color ImageColorAlpha = FadeImage.color;
			ImageColorAlpha.a = f;
			FadeImage.color = ImageColorAlpha;
			yield return new WaitForSeconds (TimeStep);
		}
	}

	IEnumerator ShowTimedImageFadeInt(Image FadeImage, float TimeBeforeStartFade ,float TimeStep)
	{
		Color NoAlpha = FadeImage.color;
		NoAlpha.a = 0f;
		FadeImage.color = NoAlpha;
		yield return new WaitForSeconds (TimeBeforeStartFade);
		for (float f = 0f; f <= TimeStep; f += TimeStep) {
			Color ImageColorAlpha = FadeImage.color;
			ImageColorAlpha.a = f;
			FadeImage.color = ImageColorAlpha;
			yield return new WaitForSeconds (TimeStep);
		}
	}
	#endregion
}
