using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ice : MonoBehaviour
{
    public float effectDuration = 10f;
    private Spawner spawner;

    // Start is called before the first frame update
    void Start()
    {
        spawner = FindObjectOfType<Spawner>();
        if(spawner.lokeHealthSlider.value < 50)
        {
            ActivateIceEffect();
        }
    }

    private void ActivateIceEffect()
    {
        if(spawner != null)
        {
            spawner.SetStalactiteSpawnRate(0.5f);
            StartCoroutine(EffectDuration());
        }
    }

    private IEnumerator EffectDuration()
    {
        Camera.main.backgroundColor = Color.blue;
        yield return new WaitForSeconds(effectDuration);
        DeactivateEffect();
    }

    private void DeactivateEffect()
    {
        if(spawner != null)
        {
            spawner.SetStalactiteSpawnRate(1.0f);
            Camera.main.backgroundColor = Color.white;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Basket"))
        {
            ActivateIceEffect();
            Destroy(gameObject);
        }
    }
}
