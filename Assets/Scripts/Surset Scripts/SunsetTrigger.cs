using UnityEngine;
using System.Collections;

public class SunsetTrigger : MonoBehaviour
{
    public Light sunLight;              // Assign your Directional Light here
    public float sunsetDuration = 5f;   // Time for sunset
    public Color sunsetColor = new Color(0.8f, 0.4f, 0.2f); // Orange tint
    public Color nightAmbientColor = Color.black;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            StartCoroutine(StartSunset());
        }
    }

    IEnumerator StartSunset()
    {
        float elapsed = 0f;
        float startIntensity = sunLight.intensity;
        Color startColor = RenderSettings.ambientLight;

        while (elapsed < sunsetDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / sunsetDuration;

            // Gradually dim the sun and shift its color
            sunLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
            sunLight.color = Color.Lerp(Color.white, sunsetColor, t);

            // Make ambient lighting darker
            RenderSettings.ambientLight = Color.Lerp(startColor, nightAmbientColor, t);

            yield return null;
        }

        sunLight.intensity = 0f;
        RenderSettings.ambientLight = nightAmbientColor;
    }
}
