using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    public TMP_Text leaderboardTexts;
    public GameObject button;
    
    private List<float> bestTimes = new List<float>();
    private const int maxEntries = 5;

    public static LeaderboardManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this instance between scenes
            HideLeaderboardUI();
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadLeaderboard();
    }

    public void SubmitNewTime(float newTime)
    {
        if (newTime <= 0) return;

        if(bestTimes.Count < maxEntries || newTime > bestTimes[maxEntries - 1])
        {
            bestTimes.Add(newTime); //add the new time
            bestTimes.Sort();
            bestTimes.Reverse();

            //enusre only top 5 entries are kept
            if(bestTimes.Count > maxEntries)
            {
                bestTimes.RemoveAt(bestTimes.Count - 1); //remove the smallest time
            }

            SaveLeaderboard();
            DisplayLeaderboard();
        }
    }

    private void SaveLeaderboard()
    {
        for(int i = 0; i < bestTimes.Count; i++)
        {
            string key = "BestTime" + i;
            PlayerPrefs.SetFloat(key, bestTimes[i]);
        }

        //remove any old entries beyond the max entries
        for(int i = bestTimes.Count; i < maxEntries; i++)
        {
            string key = "BestTime" + i;
            PlayerPrefs.DeleteKey(key);
        }
        PlayerPrefs.Save(); //save the changes
    }

    private void LoadLeaderboard()
    {
        bestTimes.Clear();
        //load the saved times
        for(int i = 0; i < maxEntries; i++)
        {
            string key = "BestTime" + i;
            if (PlayerPrefs.HasKey(key))
            {
                bestTimes.Add(PlayerPrefs.GetFloat(key));
            }
        }
        bestTimes.Sort();
        bestTimes.Reverse();
    }

    public void ShowLeaderboardUI()
    {
        leaderboardTexts.enabled = true;
        button.SetActive(true);
        DisplayLeaderboard();
    }

    public void HideLeaderboardUI()
    {
        leaderboardTexts.enabled = false;
        button.SetActive(false);
    }

    private void DisplayLeaderboard()
    {
        leaderboardTexts.enabled = true;
        button.SetActive(true);
        leaderboardTexts.text = "Top 5 Survival Time:\n";

        for(int i = 0; i < bestTimes.Count; i++)
        {
            leaderboardTexts.text += string.Format("{0}. {1:00}:{2:00}:{3:000}\n",
                i + 1,
                Mathf.FloorToInt(bestTimes[i] / 60),
                Mathf.FloorToInt(bestTimes[i] % 60),
                (bestTimes[i] % 1) * 1000);
        }
        for(int i = bestTimes.Count; i < maxEntries; i++)
        {
            leaderboardTexts.text += string.Format("{0}. ---\n", i + 1);
        }
    }

    public void ClearLeaderboard()
    {
        PlayerPrefs.DeleteAll();
        bestTimes.Clear();
        DisplayLeaderboard();
    }
}
