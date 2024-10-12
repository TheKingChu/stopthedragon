using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FallingObjectsManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject acidDropPrefab;
    public GameObject healthApplePrefab;
    public GameObject speedBoostPrefab;

    [Header("Spawn boundries")]
    public Transform spawnPoint;

    [Header("Loki stuff")]
    public Slider lokeHealthSlider;
    public float acidDropDamage = 10f;
    public float healthIncreaseAmount = 15f;

    [Header("Acid Slider")]
    public Slider acidSlider;

    public float minSpawnDelay = 0.3f;
    public float maxSpawnDelay = 1.5f;

    public float minDropSpeed = 1.5f;
    public float maxDropSpeed = 6f;

    private float lokeMaxHealth = 100f;

    public SceneLoader sceneLoader;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnFallingObjects());

        lokeHealthSlider.maxValue = lokeMaxHealth;
        lokeHealthSlider.value = lokeMaxHealth;
    }

    private GameObject GetRandomFallingObject()
    {
        // Define weights for each object type
        List<(GameObject prefab, float weight)> objects = new List<(GameObject, float)>
        {
            (acidDropPrefab, 0.8f), // 80% chance
            (healthApplePrefab, 0.8f),
            (speedBoostPrefab, 0.8f) // 10% chance
            // Add more objects with respective weights here
        };

        // Calculate total weight
        float totalWeight = 0f;
        foreach (var obj in objects)
        {
            totalWeight += obj.weight;
        }

        // Get a random value
        float randomValue = Random.value * totalWeight;

        // Determine which object to return based on weights
        foreach (var obj in objects)
        {
            if (randomValue < obj.weight)
            {
                return obj.prefab;
            }
            randomValue -= obj.weight;
        }

        return acidDropPrefab; // Fallback in case of an error
    }



    private IEnumerator SpawnFallingObjects()
    {
        while (true)
        {
            GameObject objectToSpawn = GetRandomFallingObject();

            //Randomize X position along the spawnArea transform
            float randomX = Random.Range(spawnPoint.position.x - (spawnPoint.localScale.x / 2), spawnPoint.position.x + (spawnPoint.localScale.x / 2));
            Vector3 spawnPosition = new Vector3(randomX, spawnPoint.position.y, 0); // Keep Y fixed

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

    public void SpeedBoostCaught()
    {

    }

    public void StalagtittCaught()
    {

    }
}
