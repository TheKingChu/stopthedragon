using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LokiSliderEffects : MonoBehaviour
{
    public Slider lokiSlider;
    public RectTransform sliderHandle;
    public float pulseThreshold = 0.25f;
    public float pulseScale = 1.2f;
    public float pulseSpeed = 0.5f;

    private bool isPulsing = false;

    // Update is called once per frame
    void Update()
    {
        if(lokiSlider.value / lokiSlider.maxValue <= pulseThreshold)
        {
            if (!isPulsing)
            {
                isPulsing = true;
            }
            float scale = 1 + Mathf.PingPong(Time.time * pulseSpeed, pulseScale - 1);
            sliderHandle.localScale = new Vector3(scale, scale, 1);
        }
        else
        {
            isPulsing = false;
            sliderHandle.localScale = Vector3.one;
        }
    }
}
