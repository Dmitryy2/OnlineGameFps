using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	[HideInInspector]public static GameManager instance;	//Set an instance to access the gamemanger easily from every gameobject
	public PhotonView GameManagerPhotonView;				//Reference to our photonview attached to this gameobject
	[Space(10)]

	[Header("Game Rules")]
	public string GameName = "";							//The name of the current game or the name we are setting the mainmenu
	public byte MaxPlayers = 20;							//The max amount of players of the current game or the game we are currently setting up
	public MapSettings CurrentMap;							//The map currently being played or the one we have chosen to set up in the server settings
	public GameType CurrentGameType;						//The gametype currently being played or the one we have chosen to set up in the server settings
	public float GameTimeLimit = 0.1f; 						//Time in minutes, 0 is unlimited
	public float GameScoreLimit = 0.1f; 					//Time in points, 0 is unlimited
	//public int TimeLimit = 0;								//Time in points, 0 is unlimited
	//public int ScoreLimit = 0; 								//Score per team or player depending on the gametype, 0 is no score limit
	public int MaxVehicles = 2;								//Max vehicle available at a given moment
	public float RespawnDelay = 5f; 						//The delay before the OnplayerRespawn is called in a gametype

	[Header("Game References")]
	public GameType[] GameTypeList;							//All gametypes in which we can choose from in the server settings
	public float[] TimeLimits;								//All Timelimots we can chose in the server settings
	public int[] ScoreLimits;
	public MapSettings[] MapList;							//All maps in which we can chose from in the server settings
	public string[] ServerRotation;							//List of maps which will be played in sequence
	private int ServerRotationIndex = -1;					//At which index are we at in the server rotation?
	public Weapon[] AllGameWeapons;							//List of all weapons available in the game currently
	public List<int> NotAllowedWeapons = new List<int>();	//Weapon ID's to exclude from the game [Can be manually set in the inspector but not yet ingame]
	//public List<SurfaceHit> SurfaceHits = new List<SurfaceHit> ();	//List of all surfacehit types only used with the old playerweapon system
	public GameObject RedTeamPlayer;						//Prefab of the RedTeamPlayer
	public GameObject BlueTeamPlayer;						//Prefab of the BlueTeamPlayer
	public GameObject NoneTeamPlayer;						//Prefab of the NoneTeamPlayer
	public GameObject[] VehiclePrefabs;						//Prefab of the Vehicles
	public GameObject LoadingPanel;							//Prefab of the canvas when loading a scene
	public Image LoadImage;
	public GameObject ErrorMessagePanel;
	public Text ErrorMessageText;
	public GameObject[] IngameEffectsReferences;					

	[Header("Player Loadout References")]
	public int PlayerPrimaryWeapon = 0;
	public int PlayerSecondaryWeapon = 4;

	[Header("Ambient Sound")]
	public AudioSource AmbientAudioSource;					//Audiosource where ambient sounds are going to be played from
	public AudioClip[] AmbientAudioclips;					//All possible audioclips to be used as ambient sounds
		
	[Header("InGame References")]							//Local variables
	public bool MapChanged = false;							//Has the map just been changed?
	public bool Ingame = false;								//Are we currently ingame?
	public bool Teambased = false;							//Is the currentgametype teambased?
	public PunTeams.Team CurrentTeam;						//The team we are currently in
	public bool IsAlive = false;							//Are we alive at the moment
	public bool InVehicle = false; 							//Are we currently in a vehicle?
	public bool MatchActive = false;						//Is the match in which we are still in an active state?
	public bool CanSpawn = false;							//Is it possible for us the spawn right now?
	public bool HasTeam = false;							//Do we have a team currently?
	public bool AllowLoadout = true;
	public bool CanSpawnVehicles = true;


	//All non-visible variables and references here
	[HideInInspector]public GameObject[] DeathMatchSpawns;
	[HideInInspector]public GameObject[] RedTeamSpawns;
	[HideInInspector]public GameObject[] BlueTeamSpawns;
	[HideInInspector]public GameObject CurrentVehicle;
	[HideInInspector]public float GameTimeLeft = 1f;
	[HideInInspector]public int StartTime;
	[HideInInspector]public float EndGameTime = 15f;
	[HideInInspector]public GameObject SceneCamera;
	[HideInInspector]public string EndGameReason;

	void Awake()
	{
		if (instance == null) {
			instance = this;
			SceneManager.sceneLoaded += OnLevelWasFinishedLoading;
		} else if (instance != null) {
			DestroyImmediate (this.gameObject);
		}
	}

	void OnLevelWasFinishedLoading(Scene LoadedScene, LoadSceneMode SceneLoadMode) //When the scene has finished loading
	{
		if (LoadedScene.name == "MainMenu") { 													//If the loaded scene in the main menu
			PhotonNetwork.ConnectUsingSettings (PhotonNetworkManager.instance.GameVersion); 	//Connect to the photon services
			SetGameType (0);			 							 							//Set the gametype to the default value
			SetMap (0);				  															//Set the map to the default value
			SetTimeLimit(0);
			SetScoreLimit(0);
		}

		if (LoadedScene.name != "MainMenu" && LoadedScene.name != "Icon_Workshop") { //If the loaded scene is not the mainmenu or the workshop, assuming alls scenes other than the mainmenu scene are playable maps
			//LoadImage.gameObject.SetActive (false);
			SceneCamera = GameObject.Find ("SceneCamera"); //Find the scenecamera in the scene
			DeathMatchSpawns = GameObject.FindGameObjectsWithTag ("DeathMatchSpawn"); //Get all DeathMatchSpawns
			RedTeamSpawns = GameObject.FindGameObjectsWithTag ("RedTeamSpawn");       //Get all RedTeamSpawns
			BlueTeamSpawns = GameObject.FindGameObjectsWithTag ("BlueTeamSpawn");	   //Get all blueTeamSpawns
			InGameUI.instance.SelectPrimaryWeapon(PlayerPrimaryWeapon);
			InGameUI.instance.SelectSecondaryWeapon(PlayerSecondaryWeapon);
			if (MapChanged == true && PhotonNetwork.inRoom) {
				InitGameType ();
				MapChanged = false;
			}
		} 
	}

	public void SetCursorLock(bool Toggle)				//Determines if the cursor is locked to the screen or free to use
	{
		if (Toggle == false) {							//If the toggle is set to off
			Cursor.lockState = CursorLockMode.None;		//Free our cursor
			Cursor.visible = true;						//Make it visible for us
		} else {
			Cursor.lockState = CursorLockMode.Locked;	//Lock our cursor to the middle of the window
			Cursor.visible = false;						//Hide our cursor
		}
	}

	public void SetPlayerCameraActiveState(bool Toggle)
	{
		GameObject playerobject = PhotonNetwork.player.TagObject as GameObject; //Get the local player object
		playerobject.GetComponent<PlayerMovementController> ().PlayerCamera.gameObject.SetActive(Toggle); //Set the cameraobject active state to the be on or off
	}

	public void ClearMatchSettings()							//Called when we leave an match or when the match has been restarted
	{
		EventManager.TriggerEvent ("DisableGameType");			//Tell the gametype to deactivate itself using the EventManager
		CanSpawnVehicles = true;
		InVehicle = false;							
		AllowLoadout = true;
		Teambased = false;
		ResetPlayerStats ();
		ClearAmbience ();
		GameName = "";
		GameTimeLimit = 0;
		CanSpawn = false;
		CurrentGameType = GameTypeList [0];
		CurrentTeam = PunTeams.Team.none;
		PlayerInputManager.instance.FreezePlayerControls = false; //Dont freeze our control anymore
		SceneCamera.SetActive (true);							  //Set the scenecamera to on again  
		SetCursorLock (false);									  //Unlock our cursor
	}

	public void SetGameType(int index)							  //Sets the Currentgametype to the chosen index in the server settings menu
	{
		CurrentGameType = GameTypeList [index];					  
	}

	public void SetScoreLimit(int index)						 //Sets the Current Scorelimit to the chosen index in the server settings menu
	{
		GameScoreLimit = ScoreLimits [index];					  
	}

	public void SetMap(int index)								  //Sets the CurrentMap to the chosen index in the server settings menu
	{
		CurrentMap = MapList[index];
	}

	public void SetTimeLimit(int index)							  //Sets the Current Timelimit to the chosen index in the server settings menu
	{
		GameTimeLimit = TimeLimits [index];
	}
		
	public void DisplayErrorMessage(string ErrorText)
	{
		ErrorMessagePanel.SetActive (true);
		ErrorMessageText.text = ErrorText;
	}

	public void GetCurrentGameType()
	{
		///if(PhotonNetwork.masterClient != photon)
		for (int i = 0; i < GameTypeList.Length; i++) { 		//Loop through every gametype available
			if (GameTypeList [i].GameTypeLoadName == PhotonNetwork.room.CustomProperties ["gm"].ToString ()) { //If one of the gametypes matches with the current gametype from the server
				CurrentGameType = GameTypeList [i];				//Set the currentgametype to the specific gametype
			}
		}

		if (CurrentGameType != null) {
			InitGameType ();
			LoadingPanel.SetActive (false);
		}
	}

	void InitGameType()
	{
		if (CurrentGameType.GameTypeLoadName == "TDM") {		//If the gametypeloadname is "TDM"
			GameTypeList[0].GameTypeBehaviour.enabled = true;
		}
		if (CurrentGameType.GameTypeLoadName == "DM") {			//If the gametypeloadname is "DM"
			GameTypeList[1].GameTypeBehaviour.enabled = true;
		}
	}

	public void SpawnPlayer(string Team)
	{
		if (Team == "red") {
			//Spawns the player prefab in the game
			GameObject RedTeamPlayerObject = PhotonNetwork.Instantiate (GameManager.instance.RedTeamPlayer.name, GameManager.instance.RedTeamSpawns [Random.Range (0, GameManager.instance.RedTeamSpawns.Length)].transform.position, GameManager.instance.RedTeamSpawns [Random.Range (0, GameManager.instance.RedTeamSpawns.Length)].transform.rotation, 0);
			//RedTeamPlayerObject.GetComponent<CharacterController> ().enabled = true;	 	
			RedTeamPlayerObject.GetComponent<PlayerMovementController> ().enabled = true;	
			RedTeamPlayerObject.GetComponent<PlayerMovementController> ().controller.enabled = true;
			GameManager.instance.SceneCamera.SetActive (false);								
			RedTeamPlayerObject.GetComponent<PlayerMovementController> ().PlayerWeaponHolder.gameObject.SetActive (true);	
			RedTeamPlayerObject.GetComponent<PlayerWeaponManager> ().enabled = true;
			RedTeamPlayerObject.GetComponent<PlayerInteractables> ().enabled = true;
			PhotonNetwork.player.SetTeam (PunTeams.Team.red);
			RedTeamPlayerObject.GetComponent<PlayerStats> ().PlayerTeam = PhotonNetwork.player.GetTeam (); //Set the team on the player object
		}
		else if (Team == "blue") {
			//Spawns the player prefab in the game
			GameObject BlueTeamPlayerObject = PhotonNetwork.Instantiate (GameManager.instance.BlueTeamPlayer.name, GameManager.instance.BlueTeamSpawns [Random.Range (0, GameManager.instance.BlueTeamSpawns.Length)].transform.position, GameManager.instance.BlueTeamSpawns [Random.Range (0, GameManager.instance.BlueTeamSpawns.Length)].transform.rotation, 0);
			//BlueTeamPlayerObject.GetComponent<CharacterController> ().enabled = true;
			BlueTeamPlayerObject.GetComponent<PlayerMovementController> ().enabled = true;
			BlueTeamPlayerObject.GetComponent<PlayerMovementController> ().controller.enabled = true;
			GameManager.instance.SceneCamera.SetActive (false);
			BlueTeamPlayerObject.GetComponent<PlayerMovementController> ().PlayerWeaponHolder.gameObject.SetActive (true);
			BlueTeamPlayerObject.GetComponent<PlayerWeaponManager> ().enabled = true;
			BlueTeamPlayerObject.GetComponent<PlayerInteractables> ().enabled = true;
			PhotonNetwork.player.SetTeam (PunTeams.Team.blue);
			BlueTeamPlayerObject.GetComponent<PlayerStats> ().PlayerTeam = PhotonNetwork.player.GetTeam (); //Set the team on the player object
		}
		else if (Team == "none") {
			//Spawns the player prefab in the game
			GameObject DMPlayerObject = PhotonNetwork.Instantiate (GameManager.instance.BlueTeamPlayer.name, GameManager.instance.DeathMatchSpawns [Random.Range (0, GameManager.instance.DeathMatchSpawns.Length)].transform.position, GameManager.instance.DeathMatchSpawns [Random.Range (0, GameManager.instance.DeathMatchSpawns.Length)].transform.rotation, 0);
			//DMPlayerObject.GetComponent<CharacterController> ().enabled = true;
			DMPlayerObject.GetComponent<PlayerMovementController> ().enabled = true;
			DMPlayerObject.GetComponent<PlayerMovementController> ().controller.enabled = true;
			GameManager.instance.SceneCamera.SetActive (false);
			DMPlayerObject.GetComponent<PlayerMovementController> ().PlayerWeaponHolder.gameObject.SetActive (true);
			DMPlayerObject.GetComponent<PlayerWeaponManager> ().enabled = true;
			DMPlayerObject.GetComponent<PlayerInteractables> ().enabled = true;
			PhotonNetwork.player.SetTeam (PunTeams.Team.none);
			DMPlayerObject.GetComponent<PlayerStats> ().PlayerTeam = PhotonNetwork.player.GetTeam (); //Set the team on the player object
		}
		EventManager.TriggerEvent ("OnPlayerSpawn"); //Tell the gametype to do certain actions on the player specified by the gametype
	}

	[PunRPC]
	void SyncAmbientSoundNetwork(int index)
	{
		ClearAmbience (); //Clear the audiosource so we wont have 2 ambient track running at a given time
		AmbientPlay(index, false, 0.75f);
		AddGameMessage ("<color=green>Now playing: </color>" + AmbientAudioclips [index].name);
	}

	public void ToggleLoadScreen(bool Toggle, MapSettings MapToLoad)
	{
		Debug.Log (MapToLoad.MapName);
		LoadingPanel.SetActive (Toggle);
		//if (MapToLoad != null) {
		//	LoadImage.sprite = MapToLoad.MapLoadImage;
		//}
	}

	public void AmbientPlay(int index, bool Loop, float Volume)
	{
		AmbientAudioSource.clip = AmbientAudioclips [index];
		AmbientAudioSource.loop = Loop;
		AmbientAudioSource.volume = Volume;
		AmbientAudioSource.Play();
	}

	void ClearAmbience()
	{
		AmbientAudioSource.Stop (); //Stop the current track the ambient source is playing
		if (AmbientAudioSource.isPlaying) {
			AddGameMessage ("<color=red>Ambient sound stopped!</color>");
		}
	}

	public void ResetPlayerStats()
	{
		PhotonNetwork.player.SetKills (0);	//Set the kills to 0
		PhotonNetwork.player.SetDeaths (0);	//Set the deaths to 0
		PhotonNetwork.player.SetScore (0);	//Set the score to 0
	}

	public void ResetTeamScores()
	{
		PunTeams.Team.red.SetTeamScore (0);	//Set the red team score to 0
		PunTeams.Team.blue.SetTeamScore (0); //Set the blue team score to 0
	}

	[PunRPC]
	void SyncInteractableState(string interactable, bool state)	//Used when interactable is activated
	{
		GameObject Interactable = GameObject.Find (interactable);	//Find the specific interactable
		Interactable.GetComponent<Trigger> ().Triggered = true;		//Set the triggered state for everyone to true so it cant be activated twice
	}

	[PunRPC]
	public void EndGame(string Reason)
	{
		EndGameReason = Reason;
		EventManager.TriggerEvent ("GameTypeEndGame");
	}

	[PunRPC]
	public void LoadNextMap(string MapToLoad)
	{
		MapChanged = true;
		ClearAmbience (); //Clear the ambient track
		GameManager.instance.ResetPlayerStats (); //Clears all player stats from the scoreboard
		GameManager.instance.ResetTeamScores();  //Clears all team score variables
		EventManager.TriggerEvent ("DisableGameType"); //Tell the gametype to destroy itself
		if (PhotonNetwork.isMasterClient) {
			PhotonNetwork.LoadLevel (GetNextMapFromRotation()); //Load the next level in the rotation
		}
	}

	string GetNextMapFromRotation()
	{
		if (ServerRotation.Length > 1 && ServerRotationIndex != ServerRotation.Length - 1) { //If the length of the serverrotation is greater than 1 and the current index isnt at the length minus one
			ServerRotationIndex++;	//Add 1 to the index
		} else { //If one of the previous conditions is set the false
			ServerRotationIndex = 0; //Set the index back to zero
		}
		return ServerRotation [ServerRotationIndex];
	}

	#region Game Messages
	[PunRPC]
	public void AddKillFeedEntry(string attacker, string source, string victim)
	{
		GameObject Killfeedentry = GameObject.Instantiate (InGameUI.instance.KillFeedEntryPrefab, InGameUI.instance.KillfeedPanel.transform); //Instatiate a killfeed entry and set a reference to it.
		Killfeedentry.GetComponent<Text> ().text = attacker + " [" + source + "] " + victim; //Set the contents of the Text component to display who/what killed who 
	}

	[PunRPC]
	public void AddChatMessage(string message, PhotonPlayer sender)
	{
		GameObject Chatentry = GameObject.Instantiate (InGameUI.instance.ChatEntryPrefab, InGameUI.instance.ChatPanel.transform); //Instatiate a chat entry and set a reference to it.
		Chatentry.GetComponent<Text> ().text = sender.NickName + ": " + message; //Set the contents of the Text component to the sender name and the actual message
		Chatentry.GetComponent<Text>().color = sender.GetTeam().GetTeamColor();
	}

	public void AddGameMessage(string message)
	{
		GameObject GameMessageentry = GameObject.Instantiate (InGameUI.instance.KillFeedEntryPrefab, InGameUI.instance.KillfeedPanel.transform); //Instatiate a chat entry and set a reference to it.
		GameMessageentry.GetComponent<Text> ().text = message; //Set the contents of the Text component to the sender name and the actual message
	}
	#endregion
}

[System.Serializable]
public class MapSettings
{
	public string MapName;
	public Sprite MapLoadImage; //If you want to display a preview image of the map while in the main menu or when loading 
	public bool AllowVehicles = false;
	//public enum MapPlayType {Normal, Test}	//Normal can be indexed by all regular gametypes, test can only be indexed by the test gametype
	//public MapPlayType MapType;	//Used to exclude special maps from the normal map pool
}

[System.Serializable]
public class GameType
{
	public string GameTypeFullName;
	public string GameTypeLoadName;
	public Behaviour GameTypeBehaviour;
	public bool TimeLimitEnabled = true;
	public bool ScoreLimitEnabled = false;
}

[System.Serializable]
public class Weapon 
{
	public string WeaponName;
	public WeaponClass WeaponType;
	public GameObject FirstPersonPrefab;
	public GameObject ThirdPersonPrefab;
	public Sprite WeaponIcon;
	public string WeaponDesc;
}

[System.Serializable]
public enum WeaponClass 
{
	Rifle,
	Sniper,
	LightMachine,
	Shotgun,
	Launcher,
	SubMachine,
	Pistol,
	Special
}

[System.Serializable]
public class SurfaceHit
{
	public string SurfaceHitname;
	public GameObject SurfaceParticle;
}
