using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject[] fallingObjects;

    [Header("Loki stuff")]
    public Slider lokeHealthSlider;
    public float acidDropDamage = 10f;
    public float healthIncreaseAmount = 15f;
    private float lokeMaxHealth = 100f;

    [Header("Acid Slider")]
    public Slider acidSlider;

    private Bounds bounds;
    private List<Vector3> occupiedPositions = new List<Vector3>();
    public SceneLoader sceneLoader;

    // Start is called before the first frame update
    void Start()
    {
        bounds = GetComponent<BoxCollider2D>().bounds;
        StartCoroutine(RandomSpawnTimer());

        lokeHealthSlider.maxValue = lokeMaxHealth;
        lokeHealthSlider.value = lokeMaxHealth;
    }

    // Get a random spawn position within the bounds
    private Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-bounds.extents.x, bounds.extents.x);
        float y = Random.Range(-bounds.extents.y, bounds.extents.y);
        return new Vector3(x, y, 0);
    }

    // Check if the position is occupied
    private bool IsPositionOccupied(Vector3 position)
    {
        float overlapThreshold = 0.5f; // Adjust based on your object's size
        foreach (Vector3 occupiedPosition in occupiedPositions)
        {
            if (Vector3.Distance(position, occupiedPosition) < overlapThreshold)
            {
                return true; // Position is occupied
            }
        }
        return false; // Position is free
    }

    private Vector3 GetUniqueSpawnPosition()
    {
        Vector3 spawnPosition;
        int attempts = 0;

        // Try to find a unique spawn position within a limited number of attempts
        do
        {
            spawnPosition = GetRandomSpawnPosition();
            attempts++;

            // Limit attempts to prevent infinite loops
            if (attempts > 10)
            {
                Debug.LogWarning("Failed to find unique spawn position after 10 attempts.");
                return spawnPosition; // Fallback to the last generated position
            }
        } while (IsPositionOccupied(spawnPosition));

        occupiedPositions.Add(spawnPosition); // Add the new position to the list
        return spawnPosition;
    }

    private void SpawnObject(GameObject obj)
    {
        Vector3 spawnPosition = GetUniqueSpawnPosition();
        GameObject fallingObject = Instantiate(obj);
        fallingObject.transform.position = bounds.center + spawnPosition; // Set the spawn position
    }


    private IEnumerator RandomSpawnTimer()
    {
        while (true)
        {
            SpawnObject(fallingObjects[0]);
            yield return new WaitForSeconds(Random.Range(0.3f, 1f));
            SpawnObject(fallingObjects[1]);
            yield return new WaitForSeconds(Random.Range(0.3f, 2f));
            SpawnObject(fallingObjects[2]);
            yield return new WaitForSeconds(Random.Range(0.3f, 2f));
        }
    }

    public void AcidDropMissed()
    {
        lokeHealthSlider.value -= acidDropDamage;

        if (lokeHealthSlider.value <= 0)
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
        if (acidSlider.value >= acidSlider.maxValue)
        {
            acidSlider.value = 0;
        }
    }

    public void HealthAppleCaught()
    {
        lokeHealthSlider.value = Mathf.Min(lokeHealthSlider.value + healthIncreaseAmount, lokeMaxHealth);
    }
}
