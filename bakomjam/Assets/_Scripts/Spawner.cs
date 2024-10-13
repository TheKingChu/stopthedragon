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

    [Header("Health Effects")]
    public GameObject smokeEffectPrefab;
    public GameObject flameEffectPrefab;
    public Transform[] spawnPoints;

    [Header("Acid Slider")]
    public Slider acidSlider;

    [Header("Stalactite")]
    public GameObject stalactitePrefab;
    public float stalactiteSpawnInterval = 1.0f;
    public Transform[] stalSpawns;
    public Image redOverlayImage;
    private float stalactiteSpawnRateMultiplier = 1.0f;
    private bool isStalactiteEventActive = false;

    [Header("Ice item")]
    public GameObject iceItemPrefab;
    private float iceItemSpawnRate = 5f;
    private float lastIceSpawnTime;

    private Bounds bounds;
    private List<Vector3> occupiedPositions = new List<Vector3>();
    public SceneLoader sceneLoader;
    private CameraShake cameraShake;

    private bool isSpawningPause = false;

    [Header("Sound")]
    public AudioClip[] sfx;
    private AudioSource audioSource;

    // Arrays to track active effects
    private GameObject[] activeSmokeEffects;
    private GameObject[] activeFlameEffects;

    // Start is called before the first frame update
    void Start()
    {
        bounds = GetComponent<BoxCollider2D>().bounds;
        StartCoroutine(RandomSpawnTimer());

        lokeHealthSlider.maxValue = lokeMaxHealth;
        lokeHealthSlider.value = lokeMaxHealth;

        activeSmokeEffects = new GameObject[spawnPoints.Length];
        activeFlameEffects = new GameObject[spawnPoints.Length];

        audioSource = GetComponent<AudioSource>();
        cameraShake = FindObjectOfType<CameraShake>();
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
            if (attempts > 99999999)
            {
                Debug.LogWarning("Failed to find unique spawn position after 99999999 attempts.");
                return spawnPosition; // Fallback to the last generated position
            }
        } while (IsPositionOccupied(spawnPosition));

        occupiedPositions.Add(spawnPosition); // Add the new position to the list
        return spawnPosition;
    }

    private void SpawnObject(GameObject obj)
    {
        Vector3 spawnPosition = GetUniqueSpawnPosition();
        if(spawnPosition != Vector3.zero)
        {
            GameObject fallingObject = Instantiate(obj);
            fallingObject.transform.position = bounds.center + spawnPosition; // Set the spawn position
        }
    }


    private IEnumerator RandomSpawnTimer()
    {
        while (true)
        {
            if (!isSpawningPause)
            {
                SpawnObject(fallingObjects[0]);
                yield return new WaitForSeconds(Random.Range(0.3f, 1f));
                SpawnObject(fallingObjects[1]);
                yield return new WaitForSeconds(Random.Range(0.3f, 2f));
                SpawnObject(fallingObjects[2]);
                yield return new WaitForSeconds(Random.Range(0.3f, 2f));
            }
            else
            {
                yield return null; //skip spawning when paused
            }
        }
    }

    public void AcidDropMissed()
    {
        lokeHealthSlider.value -= acidDropDamage;
        audioSource.PlayOneShot(sfx[1]);
        cameraShake.TriggerShake();

        CheckHealthEffects();

        if (lokeHealthSlider.value <= 0)
        {
            audioSource.PlayOneShot(sfx[3]);
            FindObjectOfType<TimeManager>().StopTimer();
            float finalTime = FindObjectOfType<TimeManager>().GetFinalTime();
            LeaderboardManager.Instance.SubmitNewTime(finalTime);
            LeaderboardManager.Instance.ShowLeaderboardUI();
            sceneLoader.ShowLeaderboard();
        }
    }

    private void CheckHealthEffects()
    {
        if(lokeHealthSlider.value <= 75 && lokeHealthSlider.value > 50)
        {
            //check if smoke has not been spawned already
            if (!smokeEffectPrefab.activeInHierarchy)
            {
                SpawnSmokeEffect();
            }
        }
        else if(lokeHealthSlider.value <= 50 && lokeHealthSlider.value > 0)
        {
            //check if flames have not been spawned already
            if (!flameEffectPrefab.activeInHierarchy)
            {
                SpawnFlameEffect();
            }
        }

        // Start the stalactite event when health reaches 25%
        if (lokeHealthSlider.value <= 25 && !isStalactiteEventActive)
        {
            StartCoroutine(StartStalactiteEvent());
        }
        // Stop the stalactite event when health is above 25%
        else if (lokeHealthSlider.value > 25 && isStalactiteEventActive)
        {
            StopStalactiteEvent();
        }

        // Spawn Ice Item when health is below 50% if not already spawned
        if (lokeHealthSlider.value < 50) // Assuming you have a boolean iceItemSpawned
        {
            if(Time.time >= lastIceSpawnTime + iceItemSpawnRate)
            {
                SpawnIceItem(); // Call the method to spawn the ice item
                lastIceSpawnTime = Time.time;
            }
        }
    }

    private void SpawnIceItem()
    {
        Instantiate(iceItemPrefab, new Vector3(Random.Range(-bounds.extents.x, bounds.extents.x), bounds.center.y, 0), Quaternion.identity);
    }

    private IEnumerator StartStalactiteEvent()
    {
        //step 1 trigger screen shake
        cameraShake.TriggerEventShake();
        //step 2 red screen pulse effect
        StartCoroutine(ScreenPulseRed());
        //step 3 sound
        audioSource.PlayOneShot(sfx[5]);
        audioSource.PlayOneShot(sfx[6]);
        //wait before spawning the stalactites
        yield return new WaitForSeconds(1.5f);
        //step 4 spawning
        isStalactiteEventActive = true;
        StartCoroutine(SpawnStalactites());
    }

    private void StopStalactiteEvent()
    {
        isStalactiteEventActive = false;
        StopCoroutine(SpawnStalactites());
    }

    private IEnumerator SpawnStalactites()
    {
        while (isStalactiteEventActive)
        {
            int spawnPointIndex = Random.Range(0, stalSpawns.Length);
            Instantiate(stalactitePrefab, stalSpawns[spawnPointIndex].position, Quaternion.identity);
            yield return new WaitForSeconds(stalactiteSpawnInterval / stalactiteSpawnRateMultiplier);
        }
    }

    public void SetStalactiteSpawnRate(float multiplier)
    {
        stalactiteSpawnRateMultiplier = multiplier; // Set the multiplier for spawn rate
    }

    private IEnumerator ScreenPulseRed()
    {
        float pulseDuration = 1.5f;
        float pulseSpeed = 2f;
        float elapsedTim = 0f;

        while(elapsedTim < pulseDuration)
        {
            elapsedTim += Time.deltaTime;
            float alpha = Mathf.PingPong(Time.time * pulseSpeed, 0.5f);
            Color newColor = new Color(1f, 0f, 0f, alpha);
            redOverlayImage.color = newColor;
            yield return null;
        }

        //resetting the red overlay to transparent after the pulse
        redOverlayImage.color = new Color(1f, 0f, 0f, 0f);
    }

    private void SpawnSmokeEffect()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // Only spawn smoke if there is no active smoke effect at this spawn point
            if (activeSmokeEffects[i] == null)
            {
                activeSmokeEffects[i] = Instantiate(smokeEffectPrefab, spawnPoints[i].position, Quaternion.identity);
            }
        }

        // Remove flames if health goes above 75
        if (lokeHealthSlider.value > 75)
        {
            RemoveFlameEffects();
        }
    }

    private void SpawnFlameEffect()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            // Only spawn flames if there is no active flame effect at this spawn point
            if (activeFlameEffects[i] == null)
            {
                activeFlameEffects[i] = Instantiate(flameEffectPrefab, spawnPoints[i].position, Quaternion.identity);
            }
        }

        // Remove smoke effects if health goes above 50
        if (lokeHealthSlider.value > 50)
        {
            RemoveSmokeEffects();
        }
    }

    private void RemoveSmokeEffects()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (activeSmokeEffects[i] != null)
            {
                Destroy(activeSmokeEffects[i]);
                activeSmokeEffects[i] = null; // Reset the reference
            }
        }
    }

    private void RemoveFlameEffects()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            if (activeFlameEffects[i] != null)
            {
                Destroy(activeFlameEffects[i]);
                activeFlameEffects[i] = null; // Reset the reference
            }
        }
    }

    public void AcidDropCaught()
    {
        acidSlider.value += acidDropDamage;
        if (acidSlider.value >= acidSlider.maxValue)
        {
            StartCoroutine(EmptyingBasketEvent());
        }
    }

    private IEnumerator EmptyingBasketEvent()
    {
        isSpawningPause = true; //stop the spawning

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if(player != null)
        {
            player.animator.SetTrigger("Disappear");
        }

        audioSource.PlayOneShot(sfx[4]);

        yield return new WaitForSeconds(2f);

        if(player != null)
        {
            player.animator.SetTrigger("Reappear");
        }
        acidSlider.value = 0;
        isSpawningPause = false;
    }

    public void HealthAppleCaught()
    {
        audioSource.PlayOneShot(sfx[0]);
        lokeHealthSlider.value = Mathf.Min(lokeHealthSlider.value + healthIncreaseAmount, lokeMaxHealth);

        if(lokeHealthSlider.value > 50)
        {
            RemoveSmokeEffects();
        }
        
        if(lokeHealthSlider.value > 75)
        {
            RemoveFlameEffects();
        }
    }

    public void SpeedBoostCaught()
    {
        PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            audioSource.PlayOneShot(sfx[2]);
            playerMovement.ApplySpeedBoost();
        }
    }
}
