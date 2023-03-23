using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour {

	[HideInInspector]public static MainMenuUI instance;
	[Header("Main Menu")]
	public GameObject MainMenuPanel;
	public Text ConnectionStatusText;

	[Header("Servers Menu")]
	public Transform ServerListPanel;
	public GameObject ServerListEntryPrefab;
	public bool JustLoadedServerList = false;
	private List<GameObject> ServerListEntries = new List<GameObject> ();

	[Header("Create Server Menu")]
	public InputField GameNameInputField;
	public Text SelectedGameTypeText;
	public Text SelectedMapText;
	public Text SelectedMaxPlayersText;
	public Image SelectedMapPreview;
	private int SelectedGameTypeInt = 0;
	private int SelectedMapInt = 0;
	private byte SelectedMaxPlayersByte = 0;


	[Header("GameType Settings Menu")]
	public GameObject GameTypeSettingsPanel;
	public Animation MenuAnimationComponent;
	public GameObject TimeLimitPanel;
	public Text SelectedTimeLimitText;
	private int SelectedTimeLimitInt = 0;
	public GameObject ScoreLimitPanel;
	public Text SelectedScoreLimitText;
	private int SelectedScoreLimitInt = 0;


	[Header("Settings Menu")]
	public InputField PlayerNameInputfield;
	public Dropdown ResolutionDropDown;
	private Resolution[] Resolutions;

	void Awake()
	{
		instance = this;
	}

	void Start()
	{
		IndexResolution (); //Set the resolution automaticly to the native size (comment this line when testing your game out as it can be quiete annoying when trying to run the game in multiple smaller windows)
		OnMenuStart ();
	}

	void OnMenuStart()
	{
		GameNameInputField.text = "Room:" + Random.Range (0, 9999); 							//Give the gamenameinputfield a random name
		if (PlayerPrefs.GetString ("PlayerName") != "") {
			PlayerNameInputfield.text = PlayerPrefs.GetString ("PlayerName");
		} else {
			PlayerNameInputfield.text = "Player:" + Random.Range (0, 9999); 						//Give the playernameinputfield a random name
		}
		PhotonNetwork.playerName = PlayerNameInputfield.text;									//Set the synced playername to the contents of the previously set inputfield
		SelectedGameTypeText.text = GameManager.instance.CurrentGameType.GameTypeLoadName;		//Set the selectedgametype text to the currently loaded gametype name
		SelectedMapText.text = GameManager.instance.CurrentMap.MapName;							//Set the selectedmap text to the currently loaded map name
		SelectedMapPreview.sprite = GameManager.instance.MapList[0].MapLoadImage;
	}

	void FixedUpdate()
	{
		if (GameManager.instance.Ingame == false) { 													//If we arent in the mainmenu anymore
			MainMenuUI.instance.ConnectionStatusText.text = PhotonNetwork.connectionState.ToString (); 	//Dont display our current connection status anymore
		}
	}

	#region Main Menu
	public void QuitGame()
	{
		Application.Quit ();																			//Quits out the game and closes the game window
	}

	#endregion

	#region Create Server Menu
	public void CreateServer()
	{
		GameManager.instance.GameName = GameNameInputField.text; 												//Set the Gamemanger gamename to the contents of the gamenameinputfield
		GameManager.instance.Ingame = true;			 															//Set our ingame state to true
		RoomOptions roomOptions = new RoomOptions();															//Create a new RoomOptions entry
		roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();								//Create a new photon hashtable
		roomOptions.CustomRoomProperties.Add ("map", GameManager.instance.CurrentMap.MapName);					//Create a entry for the current game map
		roomOptions.CustomRoomProperties.Add ("gm", GameManager.instance.CurrentGameType.GameTypeLoadName);		//Create a entry for the current game type
		roomOptions.CustomRoomProperties.Add ("tl", GameManager.instance.GameTimeLimit);						//Create a entry for the current game time limit
		roomOptions.CustomRoomProperties.Add ("sl", GameManager.instance.GameScoreLimit);						//Create a entry for the current game time limit
		roomOptions.CustomRoomProperties.Add ("maxveh", GameManager.instance.MaxVehicles);						//Create a entry for the maximum amount of vehicles
		roomOptions.CustomRoomPropertiesForLobby = new string[] { "map", "gm" };							    //Make the mapname and gametype name accesable from the room list so it can be displayed in the server browser
		roomOptions.MaxPlayers = GameManager.instance.MaxPlayers;												//Set the player cap for this room to 20
		PhotonNetwork.CreateRoom (GameManager.instance.GameName, roomOptions, null);							//Creates the room with the just created RoomOptions
		GameManager.instance.ToggleLoadScreen(true, GameManager.instance.CurrentMap);
		PhotonNetwork.LoadLevel (GameManager.instance.CurrentMap.MapName);											//Load the selected mapscene
	}

	public void OpenGameTypeSettingMenu()
	{
		if (MenuAnimationComponent.clip == MenuAnimationComponent.GetClip ("OpenGameTypeMenu")) {
			CloseGameTypeSettingMenu ();
			return;
		}
		UpdateGameTypeSettings ();
		MenuAnimationComponent.clip = MenuAnimationComponent.GetClip ("OpenGameTypeMenu");
		MenuAnimationComponent.Play ();
		GameTypeSettingsPanel.SetActive (true);
	}

	public void UpdateGameTypeSettings()
	{
		if (GameManager.instance.CurrentGameType.ScoreLimitEnabled == true) {
			ScoreLimitPanel.SetActive (true);
		} else {
			ScoreLimitPanel.SetActive (false);
		}
		if (GameManager.instance.CurrentGameType.TimeLimitEnabled == true) {
			TimeLimitPanel.SetActive (true);
		} else {
			TimeLimitPanel.SetActive (false);
		}
	}

	public void CloseGameTypeSettingMenu()
	{
		MenuAnimationComponent.clip = MenuAnimationComponent.GetClip ("CloseGameTypeMenu");
		MenuAnimationComponent.Play ();
		GameTypeSettingsPanel.SetActive (false);
	}

	public void CloseCreateServerMenu()
	{
		if (GameTypeSettingsPanel.activeSelf) {
			CloseGameTypeSettingMenu ();
		}
	}
	#region Select GameType
	public void NextGameType()
	{
		if (GameManager.instance.GameTypeList.Length > 1 && SelectedGameTypeInt != GameManager.instance.GameTypeList.Length - 1) { //If the length of the gametypelist is greater than 1 and the current index isnt at the length minus one
			SelectedGameTypeInt++;		//Add 1 to the index
		} else { 						//If one of the previous conditions is set the false
			SelectedGameTypeInt = 0; 	//Set the index back to zero
		}
		GameManager.instance.CurrentGameType = GameManager.instance.GameTypeList [SelectedGameTypeInt]; 		//Set the gamemanger currentgametype to the chosen index of the gametypelist
		SelectedGameTypeText.text = GameManager.instance.GameTypeList [SelectedGameTypeInt].GameTypeLoadName; 	//Set the selected gametype text to the chosen gametype
		UpdateGameTypeSettings ();
	}

	public void PrevioustGameType()
	{
		if (GameManager.instance.GameTypeList.Length > 1 && SelectedGameTypeInt != 0) { 						//If the length of the gametypelist is greater than 1 and the current index isnt zero
			SelectedGameTypeInt--; 																				//Subtract 1 to the index
		} else { 																								//If one of the previous conditions is set the false
			SelectedGameTypeInt = GameManager.instance.GameTypeList.Length - 1; 								//Set the index to the current index isnt at the length minus one
		}
		GameManager.instance.CurrentGameType = GameManager.instance.GameTypeList [SelectedGameTypeInt]; 		//Set the gamemanger currentgametype to the chosen index of the gametypelist
		SelectedGameTypeText.text = GameManager.instance.GameTypeList [SelectedGameTypeInt].GameTypeLoadName; 	//Set the selected gametype text to the chosen gametype
		UpdateGameTypeSettings ();
	}
	#endregion

	#region Select Map
	public void NextMap()
	{
		if (GameManager.instance.MapList.Length > 1 && SelectedMapInt != GameManager.instance.MapList.Length - 1) { //If the length of the maplist is greater than 1 and the current index isnt at the length minus one
			SelectedMapInt++;		//Add 1 to the index
		} else { 						//If one of the previous conditions is set the false
			SelectedMapInt = 0; 	//Set the index back to zero
		}
		GameManager.instance.CurrentMap = GameManager.instance.MapList [SelectedMapInt]; 		//Set the gamemanger currentmap to the chosen index of the maplist
		SelectedMapText.text = GameManager.instance.MapList [SelectedMapInt].MapName; 			//Set the selected map text to the chosen gametype
		SelectedMapPreview.sprite = GameManager.instance.MapList [SelectedMapInt].MapLoadImage;
	}

	public void PrevioustMap()
	{
		if (GameManager.instance.MapList.Length > 1 && SelectedMapInt != 0) { 						//If the length of the maplist is greater than 1 and the current index isnt zero
			SelectedMapInt--; 																				//Subtract 1 to the index
		} else { 																								//If one of the previous conditions is set the false
			SelectedMapInt = GameManager.instance.MapList.Length - 1; 								//Set the index to the current index isnt at the length minus one
		}
		GameManager.instance.CurrentMap = GameManager.instance.MapList [SelectedMapInt]; 			//Set the gamemanger currentmap to the chosen index of the gametypelist
		SelectedMapText.text = GameManager.instance.MapList [SelectedMapInt].MapName; 				//Set the selected map text to the chosen gametype
		SelectedMapPreview.sprite = GameManager.instance.MapList [SelectedMapInt].MapLoadImage;

	}
	#endregion

	#region Select Time Limit 
	public void NextTimeLimit()
	{
		if (GameManager.instance.TimeLimits.Length > 1 && SelectedTimeLimitInt != GameManager.instance.TimeLimits.Length - 1) { //If the length of the gametypelist is greater than 1 and the current index isnt at the length minus one
			SelectedTimeLimitInt++;	//Add 1 to the index
		} else { //If one of the previous conditions is set the false
			SelectedTimeLimitInt = 0; //Set the index back to zero
		}
		GameManager.instance.GameTimeLimit = GameManager.instance.TimeLimits[SelectedTimeLimitInt]; //Set the gamemanger currentgametype to the chosen index of the gametypelist
		if (SelectedTimeLimitInt != 0) {
			SelectedTimeLimitText.text = GameManager.instance.TimeLimits[SelectedTimeLimitInt].ToString(); //Set the selected gametype text to the chosen gametype
		}
		else {
			SelectedTimeLimitText.text = "Unlim";
		}
	}

	public void PrevioustTimeLimit()
	{
		if (GameManager.instance.TimeLimits.Length > 1 && SelectedTimeLimitInt != 0) { //If the length of the gametypelist is greater than 1 and the current index isnt zero
			SelectedTimeLimitInt--; //Subtract 1 to the index
		} else { //If one of the previous conditions is set the false
			SelectedTimeLimitInt = GameManager.instance.TimeLimits.Length - 1; //Set the index to the current index isnt at the length minus one
		}
		GameManager.instance.GameTimeLimit = GameManager.instance.TimeLimits [SelectedTimeLimitInt]; //Set the gamemanger currentgametype to the chosen index of the gametypelist
		if (SelectedTimeLimitInt != 0) {
			SelectedTimeLimitText.text = GameManager.instance.TimeLimits[SelectedTimeLimitInt].ToString(); //Set the selected gametype text to the chosen gametype
		}
		else {
			SelectedTimeLimitText.text = "Unlim";
		}
	}
	#endregion

	#region Select Score Limit 
	public void NextScoreLimit()
	{
		if (GameManager.instance.ScoreLimits.Length > 1 && SelectedScoreLimitInt != GameManager.instance.ScoreLimits.Length - 1) { //If the length of the gametypelist is greater than 1 and the current index isnt at the length minus one
			SelectedScoreLimitInt++;	//Add 1 to the index
		} else { //If one of the previous conditions is set the false
			SelectedScoreLimitInt = 0; //Set the index back to zero
		}
		GameManager.instance.GameScoreLimit = GameManager.instance.ScoreLimits[SelectedScoreLimitInt]; //Set the gamemanger currentgametype to the chosen index of the gametypelist
		if (SelectedScoreLimitInt != 0) {
			SelectedScoreLimitText.text = GameManager.instance.ScoreLimits[SelectedScoreLimitInt].ToString(); //Set the selected gametype text to the chosen gametype
		}
		else {
			SelectedScoreLimitText.text = "Unlim";
		}
	}

	public void PrevioustScoreLimit()
	{
		if (GameManager.instance.ScoreLimits.Length > 1 && SelectedScoreLimitInt != 0) { //If the length of the gametypelist is greater than 1 and the current index isnt zero
			SelectedScoreLimitInt--; //Subtract 1 to the index
		} else { //If one of the previous conditions is set the false
			SelectedScoreLimitInt = GameManager.instance.ScoreLimits.Length - 1; //Set the index to the current index isnt at the length minus one
		}
		GameManager.instance.GameScoreLimit = GameManager.instance.ScoreLimits[SelectedScoreLimitInt]; //Set the gamemanger currentgametype to the chosen index of the gametypelist
		if (SelectedScoreLimitInt != 0) {
			SelectedScoreLimitText.text = GameManager.instance.ScoreLimits[SelectedScoreLimitInt].ToString(); //Set the selected gametype text to the chosen gametype
		}
		else {
			SelectedScoreLimitText.text = "Unlim";
		}
	}
	#endregion

	#region Select Max Players
	public void IncreaseMaxPlayers()
	{
		if (SelectedMaxPlayersByte < 20) { //If the selected amout of maxplayers is below 20
			SelectedMaxPlayersByte += 2;	//Add 2 to the index
		} else { //If one of the previous conditions is set the false
			SelectedMaxPlayersByte = 4; //Set the index back to zero
		}
		GameManager.instance.MaxPlayers = SelectedMaxPlayersByte; //Set the gamemanager maxplayers to the chosen amount
		SelectedMaxPlayersText.text = SelectedMaxPlayersByte.ToString(); //Set the selected max players amout text to the chosen amount
	}

	public void DecreaseMaxPlayers()
	{
		if (SelectedMaxPlayersByte > 4) { //If the selected amout of maxplayers is below 20
			SelectedMaxPlayersByte -= 2;	//Subtract 2 of the index
		} else { //If one of the previous conditions is set the false
			SelectedMaxPlayersByte = 20; //Set the index to 20
		}
		GameManager.instance.MaxPlayers = SelectedMaxPlayersByte; //Set the gamemanager maxplayers to the chosen amount
		SelectedMaxPlayersText.text = SelectedMaxPlayersByte.ToString(); //Set the selected max players amout text to the chosen amount
	}
	#endregion
	#endregion

	#region Servers Menu
	public void DisplayServerList()
	{
		if (JustLoadedServerList == false) {
			ClearServerList ();
			RoomInfo[] ServerList = PhotonNetwork.GetRoomList ();
			foreach (RoomInfo room in ServerList) {
				GameObject ServerListEntry = Instantiate (ServerListEntryPrefab, ServerListPanel);
				ServerListEntries.Add (ServerListEntry);
				ServerListEntry.GetComponent<ServerListItem> ().Setup (room, JoinServer);
			}
		}
	}

	public void JoinServer(RoomInfo roomtojoin)
	{
		PhotonNetwork.JoinRoom (roomtojoin.Name);
		GameManager.instance.ToggleLoadScreen(true, null);
	}

	void ClearServerList()
	{
		foreach (GameObject ServerListEntry in ServerListEntries) {
			Destroy (ServerListEntry);
		}
		ServerListEntries.Clear ();
	}

	public void JoinRandomServer()
	{
		PhotonNetwork.JoinRandomRoom (); //Joins a random room
		GameManager.instance.ToggleLoadScreen(true, null);
	}
	#endregion

	#region Settings Menu
	void IndexResolution()
	{
		Resolutions = Screen.resolutions;
		ResolutionDropDown.ClearOptions ();
		List<string> ResolutionOptions = new List<string> ();
		int CurrentResolutionIndex = 0;

		for (int i = 0; i < Resolutions.Length; i++) {
			string resolutionoption = Resolutions [i].width + " x " + Resolutions [i].height;
			ResolutionOptions.Add (resolutionoption);
			if (Resolutions [i].width == Screen.currentResolution.width && Resolutions [i].height == Screen.currentResolution.height) {
				CurrentResolutionIndex = i;
			}
		}
		ResolutionDropDown.AddOptions (ResolutionOptions);
		ResolutionDropDown.value = CurrentResolutionIndex;
		ResolutionDropDown.RefreshShownValue ();
	}

	public void SetResolution(int resolutionindex)
	{
		Resolution resolution = Resolutions [resolutionindex];
		Screen.SetResolution (resolution.width, resolution.height, Screen.fullScreen);
	}

	public void SetQuality(int QualityIndex)
	{
		QualitySettings.SetQualityLevel (QualityIndex);
	}

	public void SetFullScreen(bool isFullScreen)
	{
		Screen.fullScreen = isFullScreen;
	}

	public void SetPlayername(string name)
	{
		PhotonNetwork.playerName = name;
		PlayerPrefs.SetString ("PlayerName", name);
	}
	#endregion

	#region Icon Workshop
	public void LoadIconWorkShop()
	{
		PhotonNetwork.Disconnect ();
		SceneManager.LoadScene ("Icon_Workshop");
	}
	#endregion
}
