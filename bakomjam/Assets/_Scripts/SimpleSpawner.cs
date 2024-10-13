using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;

public class SimpleSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] fallingObjects; // Assign in inspector
    [SerializeField] Transform[] fallingTransforms; // Assign in inspector

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

    [Header("Sound")]
    public AudioClip[] sfx;
    private AudioSource audioSource;

    // Arrays to track active effects
    private GameObject[] activeSmokeEffects;
    private GameObject[] activeFlameEffects;

    public SceneLoader sceneLoader;
    private CameraShake cameraShake;

    private bool isSpawningPause = false;
    private List<int> availableTransformIndices;
    private Dictionary<int, GameObject> lastSpawnedObjects;

    // Start is called before the first frame update
    void Start()
    {
        lastSpawnedObjects = new Dictionary<int, GameObject>();
        Debug.Log($"Falling Objects Count: {fallingObjects.Length}, Falling Transforms Count: {fallingTransforms.Length}");

        Debug.Log("SimpleSpawner script started"); // Log when the script starts
        StartCoroutine(RandomSpawnTimer()); // Start the spawning coroutine

        lokeHealthSlider.maxValue = lokeMaxHealth;
        lokeHealthSlider.value = lokeMaxHealth;

        activeSmokeEffects = new GameObject[spawnPoints.Length];
        activeFlameEffects = new GameObject[spawnPoints.Length];

        audioSource = GetComponent<AudioSource>();
        cameraShake = FindObjectOfType<CameraShake>();

    }

    private IEnumerator RandomSpawnTimer()
    {
        Debug.Log("RandomSpawnTimer coroutine started");

        while (true)
        {
            if (!isSpawningPause)
            {

                // Check for valid falling objects and transforms
                if (fallingObjects.Length == 0 || fallingTransforms.Length == 0)
                {
                    Debug.LogWarning("No fallingObjects or fallingTransforms assigned!");
                    yield return null; // Skip if nothing to spawn
                }
                else
                {
                    // Create a list of available transform indices
                    List<int> availableTransformIndices = new List<int>(Enumerable.Range(0, fallingTransforms.Length));

                    // Shuffle the available indices
                    ShuffleList(availableTransformIndices);

                    foreach (int transformIndex in availableTransformIndices)
                    {
                        // Randomly select an object
                        int randomObjIndex = Random.Range(0, fallingObjects.Length);
                        GameObject objToSpawn = fallingObjects[randomObjIndex];

                        // Get the spawn position
                        Vector3 spawnPosition = fallingTransforms[transformIndex].position;

                        // Check if the object is the same as the last one spawned at this position
                        if (lastSpawnedObjects.ContainsKey(transformIndex) && lastSpawnedObjects[transformIndex] == objToSpawn)
                        {
                            Debug.Log("Same object and position selected; skipping spawn.");
                            continue; // Skip this spawn if it's the same object
                        }

                        // Log the spawn
                        Debug.Log($"Spawning object: {objToSpawn.name} at {spawnPosition} (Object Index: {randomObjIndex}, Transform Index: {transformIndex})");

                        // Instantiate the object at the spawn position
                        Instantiate(objToSpawn, spawnPosition, Quaternion.identity);

                        // Track the last spawned object at this position
                        lastSpawnedObjects[transformIndex] = objToSpawn;

                        // Wait before the next spawn
                        yield return new WaitForSeconds(Random.Range(0.3f, 1f)); // Wait before spawning again

                        // Break after one successful spawn to avoid rapid-fire spawns at the same position
                        break; // Break out of the loop after spawning one object
                    }
                }
            }

            // Reset the available transforms for the next cycle
            yield return new WaitForSeconds(Random.Range(0.3f, 1f)); // Wait before the next spawn cycle
        }
    }

    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            // Swap
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
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
        if (lokeHealthSlider.value <= 75 && lokeHealthSlider.value > 50)
        {
            //check if smoke has not been spawned already
            if (!smokeEffectPrefab.activeInHierarchy)
            {
                SpawnSmokeEffect();
            }
        }
        else if (lokeHealthSlider.value <= 50 && lokeHealthSlider.value > 0)
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
            if (Time.time >= lastIceSpawnTime + iceItemSpawnRate)
            {
                SpawnIceItem(); // Call the method to spawn the ice item
                lastIceSpawnTime = Time.time;
            }
        }
    }

    private void SpawnIceItem()
    {
        int randomIndex = Random.Range(0, fallingTransforms.Length);
        Vector3 spawnPosition = fallingTransforms[randomIndex].position;
        Instantiate(iceItemPrefab, spawnPosition, Quaternion.identity);
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

        while (elapsedTim < pulseDuration)
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
        float newAcidValue = acidSlider.value + acidDropDamage;
        StartCoroutine(SmoothFillSlider(acidSlider, newAcidValue, 5f));
        if (newAcidValue >= acidSlider.maxValue)
        {
            StartCoroutine(EmptyingBasketEvent());
        }
    }

    private IEnumerator EmptyingBasketEvent()
    {
        isSpawningPause = true; //stop the spawning

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.animator.SetTrigger("Disappear");
        }

        audioSource.PlayOneShot(sfx[4]);

        yield return new WaitForSeconds(2f);

        if (player != null)
        {
            player.animator.SetTrigger("Reappear");
        }
        acidSlider.value = 0;
        isSpawningPause = false;
    }

    public void HealthAppleCaught()
    {
        audioSource.PlayOneShot(sfx[0]);
        float newHealthValue = Mathf.Min(lokeHealthSlider.value + healthIncreaseAmount, lokeMaxHealth);
        StartCoroutine(SmoothFillSlider(lokeHealthSlider, newHealthValue, 5f));
        if (newHealthValue > 50)
        {
            RemoveSmokeEffects();
        }

        if (newHealthValue > 75)
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

    private IEnumerator SmoothFillSlider(Slider slider, float targetValue, float fillSpeed)
    {
        while (Mathf.Abs(slider.value - targetValue) > 0.01f)
        {
            slider.value = Mathf.MoveTowards(slider.value, targetValue, fillSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
