using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine.UI;

public class LeaderboardNew : MonoBehaviour
{
    public string LeaderboardID;
    public ELeaderboardDataRequest RequestType = ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobalAroundUser;
    public int RequestRangeMin = -2;
    public int RequestRangeMax = 2;
    public SteamLeaderboard_t leaderboard;

    public List<LeaderboardEntry_t> RetrievedEntries { get; private set; }

    CallResult<LeaderboardFindResult_t> findResult;
    CallResult<LeaderboardScoresDownloaded_t> downloadResult;
    CallResult<LeaderboardScoreUploaded_t> m_callResultUploadScore;

    public event EventHandler LeaderboardLoaded;
    private TimeCounter tc;
    private int realscore;
    public Text[] globalRank;
    public Text[] scoreTexts;
    public Text[] steamNames;

    void Start()
    {
        RetrievedEntries = new List<LeaderboardEntry_t>();
        for (int i = 0; i < scoreTexts.Length; i++)
        {
            globalRank[i].enabled = false;
            scoreTexts[i].enabled = false;
            steamNames[i].enabled = false;
        }
    }

    void OnEnable()
    {
        RetrieveLeaderboard();
    }

    public void RetrieveLeaderboard()
    {
        if (string.IsNullOrEmpty(LeaderboardID)) return;

        SteamAPICall_t findApiCall = SteamUserStats.FindLeaderboard(LeaderboardID);
        m_callResultUploadScore = CallResult<LeaderboardScoreUploaded_t>.Create(OnScoreUploadResult);
        findResult = CallResult<LeaderboardFindResult_t>.Create(onLeaderboardFound);
        findResult.Set(findApiCall);
    }

    void onLeaderboardFound(LeaderboardFindResult_t param, bool isIoError)
    {
        findResult = null;
        if (!isIoError && param.m_bLeaderboardFound != 0)
        {
            leaderboard = param.m_hSteamLeaderboard;
            SteamAPICall_t downloadApiCall = SteamUserStats.DownloadLeaderboardEntries(leaderboard, RequestType, RequestRangeMin, RequestRangeMax);
            downloadResult = CallResult<LeaderboardScoresDownloaded_t>.Create(onLeaderboardScoresDownloaded);
            downloadResult.Set(downloadApiCall);
        }
    }

    void onLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t param, bool isIoError)
    {
        downloadResult = null;
        if (!isIoError)
        {
            for (int i = 0; i < param.m_cEntryCount; ++i)
            {
                LeaderboardEntry_t entry;

                if (SteamUserStats.GetDownloadedLeaderboardEntry(param.m_hSteamLeaderboardEntries, i, out entry, null, 0))
                {
                    LeaderboardEntry_t score = new LeaderboardEntry_t();
                    // Update your GUI or something
                    RetrievedEntries.Add(entry);

                    globalRank[i].text = entry.m_nGlobalRank.ToString();
                    steamNames[i].text = SteamFriends.GetFriendPersonaName(entry.m_steamIDUser);
                    scoreTexts[i].text = entry.m_nScore.ToString("N0");//string.Format("{0:n0}", entry.m_nScore + " sec");
                }
            }
            var callback = LeaderboardLoaded;
            if (callback != null) callback(this, EventArgs.Empty);
        }
    }

    private void OnScoreUploadResult(LeaderboardScoreUploaded_t pCallback, bool failure)
    {
        if (pCallback.m_bSuccess != 1 || failure)
            Debug.Log("There was an error uploading the LeaderboardScoreUploaded_t.");
        else
            Debug.Log("Score uploaded and m_nScore is : " + pCallback.m_nScore);
    }

    public bool UploadScore(int score)
    {
        if (leaderboard == null)
            return false;

        SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(leaderboard,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, score, null, 0);
        m_callResultUploadScore.Set(hSteamAPICall);
        Debug.Log("Uploaded score");
        return true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            LeaderboardEntry_t entry;
            GameObject timeMgr = GameObject.Find("TimeManager");
            TimeCounter counterscript = timeMgr.GetComponent<TimeCounter>();
            realscore = counterscript.score;
            UploadScore(realscore);
            RetrieveLeaderboard();
            for (int i = 0; i < scoreTexts.Length; i++)
            {
                globalRank[i].enabled = true;
                scoreTexts[i].enabled = true;
                steamNames[i].enabled = true;
            }
            Debug.Log("Called UploadCore()");
        }
    }
}

