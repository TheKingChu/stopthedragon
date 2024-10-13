using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Ice : MonoBehaviour
{
    public float effectDuration = 10f;
    private Spawner spawner;

    private Light2D globalLight;

    // Start is called before the first frame update
    void Start()
    {
        spawner = FindObjectOfType<Spawner>();
        globalLight = FindObjectOfType<Light2D>();

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
        globalLight.color = new Color(0.3089177f, 0.3266586f, 0.735849f, 1);
        yield return new WaitForSeconds(effectDuration);
        DeactivateEffect();
    }

    private void DeactivateEffect()
    {
        if(spawner != null)
        {
            spawner.SetStalactiteSpawnRate(1.0f);
            globalLight.color = new Color(0.6792453f, 0.6792453f, 0.6792453f, 1);
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
