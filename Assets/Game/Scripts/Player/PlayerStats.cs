using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour {

	[Header("Player Attributes")]
	public int PlayerHealth = 100;			//Health variable on the local player | 
	public bool isAlive = true;				//Tells if this player is still alive or not
	public PunTeams.Team PlayerTeam;		//The team this player is currently on

	[Header("Player Sound Effects")]
	[Range(0f,1f)]
	public float FootStepVolume = 0.5f;
	public AudioSource FootstepAudiosource;
	public AudioClip[] FootstepSounds;
	public AudioClip FootStepLandSound;
	public float StepInterval;
	public float RunMultiplier = 1;
	public float StepTimer;
	public AudioClip PlayerWaterWadeSound;
	public AudioClip PlayerWaterEnterSound;
	[Space(10)]
	[Range(0f,1f)]
	public float GearSoundVolume = 0.4f;
	public AudioClip GearPlayerLandSound;
	public AudioSource PlayerGearAudioSource;
	[Space(10)]
	[Range(0f,1f)]
	public float BreathSoundVolume = 0.4f;
	public AudioSource PlayerBreathAudioSource;
	public AudioClip[] PlayerSprintBreathSounds;
	[Range(0f,3f)]
	public float SprintBreathInterval;
	public float SprintBreathTimer;

	[Header("Player References")]
	public Text PlayerNameText;             //Текст playername, который находится над объектом player
	public CharacterController PlayerCharacterController;   //Локальная ссылка на PlayerCharacterController
	public PlayerMovementController PlayerMovementController;   //Локальная ссылка на PlayerMovementController
	public PlayerWeaponManager PlayerWeaponManager;             //Локальная ссылка на PlayerWeaponManager
	public PlayerThirdPersonController PlayerThirdPersonController; //Локальная ссылка на PlayerThirdPersonController
	public PhotonView PlayerPhotonView;     //Photonview, прикрепленный к этому объекту проигрывателя

	void Start()
	{
		if (PlayerPhotonView.isMine) {	//Если мы являемся владельцем photonview этого playerobject
			PhotonNetwork.player.TagObject = this.gameObject;   //Получите объект localplayer и сохраните его в photonnet.player.Переменная TagObject для последующего использования
			InGameUI.instance.PlayerHealthText.text = PlayerHealth.ToString (); //Установите значение здоровья игрока в HUD
			PlayerNameText.gameObject.SetActive (false);                        //Отключите для нас текст playername над плеером
			PlayerPhotonView.RPC ("SyncPlayerTeam", PhotonTargets.AllBuffered, PlayerTeam); //Синхронизируйте название команды и установите для текста playername для других игроков правильный цвет команды
		} 
	}

	[PunRPC]
	public void SyncPlayerTeam(PunTeams.Team SyncPlayerTeam)
	{
		PlayerTeam = SyncPlayerTeam;
		SetPlayerNameText ();
	}

	public void SetPlayerNameText()
	{
		if (PlayerTeam == PunTeams.Team.red || PlayerTeam == PunTeams.Team.none) { //Если этот игрок является вражеским игроком red / dm, то покрасьте его бейдж с именем в красный цвет
			PlayerNameText.text = "<color=red>" + PlayerPhotonView.owner.NickName + "</color>";
			this.gameObject.name = PlayerPhotonView.owner.NickName;
		} else if(PlayerTeam == PunTeams.Team.blue){
			PlayerNameText.text = "<color=blue>" + PlayerPhotonView.owner.NickName + "</color>"; //Если этот игрок является игроком синей команды, то покрасьте его бейдж с именем в синий цвет
			this.gameObject.name = PlayerPhotonView.owner.NickName;
		}
	}

	[PunRPC]
	public void ApplyPlayerDamage(int dmg, string source, PhotonPlayer attacker, float dmgmod, bool SelfInflicted)
	{
		if (attacker.GetTeam() == PunTeams.Team.none || GameManager.instance.CurrentTeam != attacker.GetTeam() || attacker == PhotonNetwork.player || source == "RoadKill") { //Если игрок, нанесший нам ущерб, из команды none, а не из нашей собственной команды, или мы нанесли ущерб себе
			if (PlayerHealth > 0) {       //Если здоровье этого игрока выше 0
				PlayerHealth -= Mathf.RoundToInt(dmg * dmgmod); //Вычтите урон у игрока с учетом модификатора урона
				if (PlayerPhotonView.isMine) {   //Если поврежденный игрок является экземпляром нашего игрока
					InGameUI.instance.DoHitscreen (1f); //Покажите красный экран попадания, чтобы указать, что мы подвергаемся нападению
					if (PlayerHealth <= 0) { //Если наше здоровье ниже 0
						PlayerHealth = 0;               //Установите наше здоровье на 0
						if (source != "RoadKill") { //Если нас не убьют внутри автомобиля
							PlayerPhotonView.RPC ("OnPlayerKilled", PhotonTargets.All, source, attacker, SelfInflicted); //Отправьте rpc, чтобы другие знали, что мы мертвы
						} else {
							if (attacker == null) {
								PlayerPhotonView.RPC ("OnPlayerKilled", PhotonTargets.All, "Crashed", PhotonNetwork.player); //Отправьте rpc, чтобы другие знали, что мы разбили наш автомобиль
							} else {
								PlayerPhotonView.RPC ("OnPlayerKilled", PhotonTargets.All, source, attacker); //Отправьте rpc, чтобы другие знали, что кто-то убил нашу машину
							}
						}
					}
					InGameUI.instance.PlayerHealthText.text = PlayerHealth.ToString (); //Установите новое значение playerhealth на вычитаемую сумму
				}
			}
		}
	}

	[PunRPC]
	public void OnPlayerKilled(string source, PhotonPlayer attacker, bool SelfInflicted)
	{
		this.isAlive = false;       //	Установите для активного состояния этого игрока значение false
		if (attacker == PhotonNetwork.player && !SelfInflicted && !this.PlayerPhotonView.isMine) {
			EventManager.TriggerEvent ("OnPlayerKilled");
		}
		if (PlayerPhotonView.isMine && GameManager.instance.IsAlive) {       //Если этот игрок является нашим собственным игроком и наша переменная IsAlive в gamemanager по-прежнему имеет значение true
			InGameUI.instance.PlayerUseText.text = "";      //Скрыть текст playerusetext
			if (attacker.NickName != PhotonNetwork.player.NickName) { //Не добавляйте убийство к атакующему, когда вы являетесь атакующим
				attacker.AddKill (1);   //Добавьте убийство игроку, который убил нас
				if (GameManager.instance.CurrentGameType.GameTypeLoadName != "CTF") {
					if (attacker.GetTeam () == PunTeams.Team.red) {
						EventManager.TriggerEvent ("AddRedTeamScore");
					} else if (attacker.GetTeam () == PunTeams.Team.blue) {
						EventManager.TriggerEvent ("AddBlueTeamScore");
					}
				} 
			}
			PhotonNetwork.player.AddDeath (1);  //Добавьте смерть для нашего игрока
			PlayerThirdPersonController.EnableWeaponIK(false);  //Отключите weaponIK на этой модели игрока от третьего лица
			GameManager.instance.gameObject.GetComponent<PhotonView> ().RPC ("AddKillFeedEntry", PhotonTargets.All, attacker.NickName, source, PhotonNetwork.playerName); //Tell everyone to add a new killfeed entry with the supplied info
																																										  //Отключите функциональность этого игрового объекта

			//Эти 4 строки используются только со старым скриптом playerweapon
			//if (PlayerWeaponManager.CurrentWeapon.GetComponent<PlayerWeapon> ().IsFireLoop) {	//Если мы в данный момент стреляем из зацикленного огнестрельного оружия
			//	PlayerWeaponManager.CurrentWeapon.GetComponent<PlayerWeapon>().StopWeapon();
			//	PlayerThirdPersonController.ThirdPersonPhotonView.RPC ("ThirdPersonStopWeapon", PhotonTargets.Others, null);	//Отключите анимацию стрельбы из оружия в модели от третьего лица
			//}
			PlayerMovementController.PlayerLegs.SetActive(false);
			PlayerMovementController.enabled = false;
			//PlayerCharacterController.enabled = false;
			if (!GameManager.instance.InVehicle) {
				PlayerWeaponManager.CurrentWeapon.SetActive (false);
				PlayerWeaponManager.enabled = false;
				PlayerThirdPersonController.ThirdPersonPlayerKilled ();
			}
			InGameUI.instance.PlayerHUDPanel.SetActive (false);
			GameManager.instance.IsAlive = false;	
			GameManager.instance.InVehicle = false;
			Invoke ("PlayerRespawn", GameManager.instance.RespawnDelay);    //Подождите, пока закончится возрождение, прежде чем мы будем возрождены
		}
	}

	void PlayerRespawn()
	{
		if (GameManager.instance.MatchActive) {                     //Если матч все еще активен, значит, мы не в предыгровой или эндшпильной стадии
			EventManager.TriggerEvent ("OnPlayerRespawn");          //Отправьте сообщение gametype, чтобы позволить ему обработать возрождение
			PhotonNetwork.Destroy (this.gameObject);                //Уничтожьте этот объект игрока
		}
	}

	[PunRPC]
	public void PlayFootstepSoundNetwork(string Type)
	{
		if (Type == "Normal") {
			FootstepAudiosource.PlayOneShot (FootstepSounds [Random.Range (0, FootstepSounds.Length)], FootStepVolume); //Воспроизведение случайного звука шагов из массива footstep
		}
		if (Type == "Water") {
			FootstepAudiosource.PlayOneShot (PlayerWaterWadeSound, 0.5f); //Воспроизвести звуковой эффект the water wade
		}
	}

	#region Misc
	[PunRPC]
	public void PlayFXAtPosition(int EffectID, Vector3 Position, Vector3 EffectDirection)
	{
		Instantiate (GameManager.instance.IngameEffectsReferences[EffectID], Position, Quaternion.FromToRotation (Vector3.forward, EffectDirection));
	}
	#endregion
}
