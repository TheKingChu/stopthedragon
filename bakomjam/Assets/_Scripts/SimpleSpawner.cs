using System.Collections;
using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] fallingObjects; // Assign in inspector
    [SerializeField] Transform[] fallingTransforms; // Assign in inspector

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("SimpleSpawner script started"); // Log when the script starts
        StartCoroutine(RandomSpawnTimer()); // Start the spawning coroutine
    }

    private IEnumerator RandomSpawnTimer()
    {
        Debug.Log("RandomSpawnTimer coroutine started"); // Log when the coroutine starts

        while (true)
        {
            if (fallingTransforms.Length > 0 && fallingObjects.Length > 0)
            {
                int randomIndex = Random.Range(0, fallingTransforms.Length);
                Vector3 spawnPosition = fallingTransforms[randomIndex].position;

                Debug.Log("Spawning object: " + fallingObjects[0].name + " at " + spawnPosition); // Log the spawn
                Instantiate(fallingObjects[0], spawnPosition, Quaternion.identity); // Spawn the first object for testing

                yield return new WaitForSeconds(1f); // Wait before spawning again
            }
            else
            {
                Debug.LogWarning("No fallingTransforms or fallingObjects assigned!"); // Log warning
                yield return null; // Skip if nothing to spawn
            }
        }
    }
}
