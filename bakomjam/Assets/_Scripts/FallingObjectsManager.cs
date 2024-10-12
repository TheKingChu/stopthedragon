using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FallingObjectsManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject acidDropPrefab;
    public GameObject healthApplePrefab;

    [Header("Spawn boundries")]
    public Transform spawnPointLeft;
    public Transform spawnPointRight;

    [Header("Loki stuff")]
    public Slider lokeHealthSlider;
    public float acidDropDamage = 10f;
    public float healthIncreaseAmount = 15f;

    [Header("Acid Slider")]
    public Slider acidSlider;

    public float minSpawnDelay = 0.5f;
    public float maxSpawnDelay = 2.0f;

    public float minDropSpeed = 2f;
    public float maxDropSpeed = 5f;

    private float lokeMaxHealth = 100f;

    public SceneLoader sceneLoader;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnFallingObjects());

        lokeHealthSlider.maxValue = lokeMaxHealth;
        lokeHealthSlider.value = lokeMaxHealth;
    }

    private IEnumerator SpawnFallingObjects()
    {
        while (true)
        {
            GameObject objectToSpawn;
            if(Random.value > 0.8f) //20% chance to spawn health apple
            {
                objectToSpawn = healthApplePrefab;
            }
            else
            {
                objectToSpawn = acidDropPrefab;
            }

            //random position between left and right bounds
            float randomX = Random.Range(spawnPointLeft.position.x, spawnPointRight.position.x);
            Vector3 spawnPosition = new Vector3(randomX, spawnPointLeft.position.y, 0);

            GameObject fallingObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

            float randomSpeed = Random.Range(minDropSpeed, maxDropSpeed);
            fallingObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -randomSpeed);

            float randomDelay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(randomDelay);
        }
    }

    public void AcidDropMissed()
    {
        lokeHealthSlider.value -= acidDropDamage;

        if(lokeHealthSlider.value <= 0)
        {
            FindObjectOfType<TimeManager>().StopTimer();
            float finalTime = FindObjectOfType<TimeManager>().GetFinalTime();
            LeaderboardManager.Instance.SubmitNewTime(finalTime);
            LeaderboardManager.Instance.ShowLeaderboardUI();
            sceneLoader.ShowLeaderboard();
        }
    }

    public void AcidDropCaught()
    {
        acidSlider.value += acidDropDamage;
        if(acidSlider.value >= acidSlider.maxValue)
        {
            acidSlider.value = 0;
        }
    }

    public void HealthAppleCaught()
    {
        lokeHealthSlider.value = Mathf.Min(lokeHealthSlider.value + healthIncreaseAmount, lokeMaxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
