using UnityEngine;

public class FirePitEffect : MonoBehaviour
{
    [Header("Fire Light")]
    public Light fireLight;
    public float baseIntensity = 3f;
    public float flickerAmount = 0.8f;
    public float flickerSpeed = 8f;

    [Header("Flames")]
    public ParticleSystem flames;

    [Header("Embers")]
    public ParticleSystem embers;

    [Header("Fire Settings")]
    public bool fireBurning = true;

    void Start()
    {
        if (fireLight != null)
        {
            fireLight.type = LightType.Point;
            fireLight.color = new Color(1f, 0.35f, 0.08f);
            fireLight.range = 12f;
        }

        if (fireBurning)
            StartFire();
        else
            StopFire();
    }

    void Update()
    {
        if (!fireBurning) return;

        // Smooth, random firelight flickering.
        if (fireLight != null)
        {
            float noise = Mathf.PerlinNoise(
                Time.time * flickerSpeed, 0f
            );

            fireLight.intensity =
                baseIntensity +
                (noise - 0.5f) * 2f * flickerAmount;
        }
    }

    public void StartFire()
    {
        fireBurning = true;

        if (flames != null && !flames.isPlaying)
            flames.Play();

        if (embers != null && !embers.isPlaying)
            embers.Play();

        if (fireLight != null)
            fireLight.enabled = true;
    }

    public void StopFire()
    {
        fireBurning = false;

        if (flames != null)
            flames.Stop();

        if (embers != null)
            embers.Stop();

        if (fireLight != null)
            fireLight.enabled = false;
    }
}