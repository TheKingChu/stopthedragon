using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AcidSliderEffects : MonoBehaviour
{
    public Slider acidSlider;
    public Image glowImage;
    public Color glowColor = Color.cyan;
    public float maxGlowIntensity = 1.5f;

    // Update is called once per frame
    void Update()
    {
        UpdateGlowEffect();
    }

    private void UpdateGlowEffect()
    {
        float intensity = Mathf.Lerp(0f, maxGlowIntensity, acidSlider.value / acidSlider.maxValue);
        glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, intensity);
    }
}
