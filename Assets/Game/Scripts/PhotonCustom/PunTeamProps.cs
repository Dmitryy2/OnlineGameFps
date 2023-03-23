using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script adds extra properties for the punteams.team class without editing the original Punteams script. 
public class PunTeamProps : MonoBehaviour {

	public const string TeamBlueScoreProp = "teambluescore";
	public const string TeamRedScoreProp = "teamredscore";
}

public static class PunTeamsExtensions
{
	public static void SetTeamScore(this PunTeams.Team team, int newTeamScore)
	{
		if (team == PunTeams.Team.red) {
			ExitGames.Client.Photon.Hashtable teamredscore = new ExitGames.Client.Photon.Hashtable();  // using PUN's implementation of Hashtable
			teamredscore[PunTeamProps.TeamRedScoreProp] = newTeamScore;

			PhotonNetwork.room.SetCustomProperties(teamredscore);  // this locally sets the team score and will sync it in-game asap.
		}
		if (team == PunTeams.Team.blue) {
			ExitGames.Client.Photon.Hashtable teambluescore = new ExitGames.Client.Photon.Hashtable();  // using PUN's implementation of Hashtable
			teambluescore[PunTeamProps.TeamBlueScoreProp] = newTeamScore;

			PhotonNetwork.room.SetCustomProperties(teambluescore);  // this locally sets the team score and will sync it in-game asap.
		}
	}

	public static void AddTeamScore(this PunTeams.Team team, int teamScoreToAdd)
	{
		if (team == PunTeams.Team.red) {
			int current =  team.GetTeamScore();
			current = current + teamScoreToAdd;

			ExitGames.Client.Photon.Hashtable teamredscore = new ExitGames.Client.Photon.Hashtable();  // using PUN's implementation of Hashtable
			teamredscore[PunTeamProps.TeamRedScoreProp] = current;

			PhotonNetwork.room.SetCustomProperties(teamredscore);  // this locally sets the teamscore and will sync it in-game asap.
		}
		if (team == PunTeams.Team.blue) {
			int current =  team.GetTeamScore();
			current = current + teamScoreToAdd;

			ExitGames.Client.Photon.Hashtable teambluescore = new ExitGames.Client.Photon.Hashtable();  // using PUN's implementation of Hashtable
			teambluescore[PunTeamProps.TeamBlueScoreProp] = current;

			PhotonNetwork.room.SetCustomProperties(teambluescore);  // this locally sets the teamscore and will sync it in-game asap.
		}
	}

	public static int GetTeamScore(this PunTeams.Team team)
	{
		object teamscore;
		if (team == PunTeams.Team.red) {
			if (PhotonNetwork.room.CustomProperties.TryGetValue(PunTeamProps.TeamRedScoreProp, out teamscore))
			{
				return (int) teamscore;
			}
		}
		if (team == PunTeams.Team.blue) {
			if (PhotonNetwork.room.CustomProperties.TryGetValue(PunTeamProps.TeamBlueScoreProp, out teamscore))
			{
				return (int) teamscore;
			}
		}
		return 0;
	}

	public static Color GetTeamColor(this PunTeams.Team team)
	{
		if (team == PunTeams.Team.red) {
			return Color.red; 
		}
		if (team == PunTeams.Team.blue) {
			return Color.blue; 
		}
		if (team == PunTeams.Team.none) {
			return Color.red; 
		}
		return Color.white;
	}
}
