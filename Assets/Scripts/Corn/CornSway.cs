using UnityEngine;

public class CornSway : MonoBehaviour
{
    public float swayAmplitude = 2.2f;   // degrees
    public float swaySpeed = 0.6f;       // Hz-ish
    public float swayTwist = 1.0f;       // extra yaw
    public int seed = 0;

    Quaternion baseRot;

    void OnEnable() { baseRot = transform.localRotation; }

    void Update()
    {
        float t = Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
        float n1 = Mathf.PerlinNoise((seed + 13) * 0.173f, t * swaySpeed);
        float n2 = Mathf.PerlinNoise((seed + 71) * 0.193f, t * (swaySpeed * 0.77f));
        float pitch = (n1 - 0.5f) * 2f * swayAmplitude;
        float yaw   = (n2 - 0.5f) * 2f * swayAmplitude * swayTwist;

        transform.localRotation = baseRot * Quaternion.Euler(pitch, yaw, 0f);
    }
}
