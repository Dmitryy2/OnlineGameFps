using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Hashtable = ExitGames.Client.Photon.Hashtable;
//This script is a addition to the normal PunPLayerScore script, so kills and deaths can be set and accessed during the match for use scoreboard
public class PunPlayerKillsDeaths : MonoBehaviour
{
    public const string PlayerKillProp = "kills";
	public const string PlayerDeathProp = "deaths";
}

public static class KillsDeathsExtensions
{
    public static void SetKills(this PhotonPlayer player, int newKills)
    {
        Hashtable kills = new Hashtable();  // using PUN's implementation of Hashtable
		kills[PunPlayerKillsDeaths.PlayerKillProp] = newKills;

		player.SetCustomProperties(kills);  // this locally sets the kills and will sync it in-game asap.
    }

    public static void AddKill(this PhotonPlayer player, int killToAddToCurrent)
    {
		int current = player.GetKills();
		current = current + killToAddToCurrent;

        Hashtable kills = new Hashtable();  // using PUN's implementation of Hashtable
		kills[PunPlayerKillsDeaths.PlayerKillProp] = current;

		player.SetCustomProperties(kills);  // this locally sets the kills and will sync it in-game asap.
    }

    public static int GetKills(this PhotonPlayer player)
    {
        object kills;
		if (player.CustomProperties.TryGetValue(PunPlayerKillsDeaths.PlayerKillProp, out kills))
        {
			return (int) kills;
        }
        return 0;
    }

	public static void SetDeaths(this PhotonPlayer player, int newDeaths)
	{
		Hashtable Deaths = new Hashtable();  // using PUN's implementation of Hashtable
		Deaths[PunPlayerKillsDeaths.PlayerDeathProp] = newDeaths;

		player.SetCustomProperties(Deaths);  // this locally sets the kills and will sync it in-game asap.
	}

	public static void AddDeath(this PhotonPlayer player, int deathToAddToCurrent)
	{
		int current = player.GetDeaths();
		current = current + deathToAddToCurrent;

		Hashtable deaths = new Hashtable();  // using PUN's implementation of Hashtable
		deaths[PunPlayerKillsDeaths.PlayerDeathProp] = current;

		player.SetCustomProperties(deaths);  // this locally sets the kills and will sync it in-game asap.
	}

	public static int GetDeaths(this PhotonPlayer player)
	{
		object deaths;
		if (player.CustomProperties.TryGetValue(PunPlayerKillsDeaths.PlayerDeathProp, out deaths))
		{
			return (int) deaths;
		}
		return 0;
	}
}