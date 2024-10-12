using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LeaderboardScene : MonoBehaviour
{
    public LeaderboardManager manager;
    public TMP_Text finalTimeText;

    // Start is called before the first frame update
    void Start()
    {
        float finaltime = PlayerPrefs.GetFloat("FinalTime", 0);

        if(finaltime > 0)
        {
            finalTimeText.text = string.Format("Your Time: {0:00}:{1:00}:{2:000}",
                Mathf.FloorToInt(finaltime / 60),
                Mathf.FloorToInt(finaltime % 60),
                (finaltime % 1) * 1000);

            manager.SubmitNewTime(finaltime);
        }
        else
        {
            finalTimeText.text = "Invalid time";
        }
    }
}
