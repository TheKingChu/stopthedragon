using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private void Start()
    {
        LeaderboardManager.Instance.HideLeaderboardUI();
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowLeaderboard()
    {
        SceneManager.LoadScene(2);
    }
}
