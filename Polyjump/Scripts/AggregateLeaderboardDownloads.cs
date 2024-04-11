using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Steamworks;

public class AggregateLeaderboardDownloads : MonoBehaviour
{
	public List<GameObject> LeaderboardsToPoll;

	Dictionary<LeaderboardFiller, bool> reportedMap;

	// Use this for initialization
	void Start()
	{
		reportedMap = new Dictionary<LeaderboardFiller, bool>();
	}

	public void ListenToFillers()
	{
		// Unlisten to everything
		foreach (LeaderboardFiller filler in new List<LeaderboardFiller>(reportedMap.Keys))
		{
			filler.LeaderboardLoaded -= fillerHandler;
			reportedMap.Remove(filler);
		}

		// Listen to selected fillers
		foreach (GameObject go in LeaderboardsToPoll)
		{
			if (go != null)
			{
				LeaderboardFiller filler = go.GetComponent<LeaderboardFiller>();
				if (filler != null) filler.LeaderboardLoaded += fillerHandler;
			}
		}
	}

	void fillerHandler(object sender, System.EventArgs e)
	{
		var filler = sender as LeaderboardFiller;
		if (filler == null) return;
		if (reportedMap.ContainsKey(filler))
			reportedMap[filler] = true;
	}

	IEnumerator doUpdateCumulativeScores()
	{
		foreach (LeaderboardFiller filler in new List<LeaderboardFiller>(reportedMap.Keys))
		{
			reportedMap[filler] = false;
			// You may want to save the filler's settings and set it to only retrieving
			// the current player's score here.
			filler.RetrieveLeaderboard();
		}

		bool waiting = true;
		while (waiting)
		{
			yield return new WaitForSeconds(0.2f);
			waiting = false;
			foreach (bool got in new List<bool>(reportedMap.Values))
			{
				if (!got) waiting = true;
			}
			// It is probably a good idea to add some sort of timeout so you won't get stuck
			// forever if something goes wrong.
		}

		// Now that we have everything, sum them up
		int cumulativeScore = 0;
		CSteamID myId = SteamUser.GetSteamID();
		foreach (LeaderboardFiller filler in new List<LeaderboardFiller>(reportedMap.Keys))
		{
			if (filler.RetrievedEntries.Count == 0) continue;
			LeaderboardEntry_t entry = filler.RetrievedEntries[0];
			if (entry.m_steamIDUser != myId) continue;
			cumulativeScore += entry.m_nScore;
		}

		// And do something with the score
	}

	public void UpdateCumulativeScore()
	{
		StartCoroutine("doUpdateCumulativeScores");
	}
} 