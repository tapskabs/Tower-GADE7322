using UnityEngine;

public class ProceduralMusicManager : MonoBehaviour
{
    [Header("Audio Layers")]
    public AudioSource ambientSource;
    public AudioSource melodySource;
    public AudioSource percussionSource;

    [Header("Clips")]
    public AudioClip[] ambientClips;
    public AudioClip[] melodyClips;
    public AudioClip[] percussionClips;

    [Header("Tower Reference (for intensity)")]
    public Tower tower; // Updated to use Tower instead of DefenderBase

    [Range(0.1f, 1f)]
    public float volumeChangeSpeed = 0.5f;

    private float targetVolume = 0.5f;

    void Start()
    {
        // Start playing random loops from each category
        PlayRandomClip(ambientSource, ambientClips);
        PlayRandomClip(melodySource, melodyClips);
        PlayRandomClip(percussionSource, percussionClips);
    }

    void Update()
    {
        // Re-randomize when clips finish
        if (!ambientSource.isPlaying) PlayRandomClip(ambientSource, ambientClips);
        if (!melodySource.isPlaying) PlayRandomClip(melodySource, melodyClips);
        if (!percussionSource.isPlaying) PlayRandomClip(percussionSource, percussionClips);

        // Change volume dynamically based on tower health
        if (tower != null)
        {
            // Calculate health percentage
            float healthPercent = (float)GetTowerHealth() / tower.maxHealth;
            // As health decreases, music intensity rises
            targetVolume = Mathf.Lerp(1f, 0.2f, healthPercent);
        }

        // Smoothly adjust volume
        ambientSource.volume = Mathf.Lerp(ambientSource.volume, targetVolume, Time.deltaTime * volumeChangeSpeed);
        melodySource.volume = Mathf.Lerp(melodySource.volume, targetVolume, Time.deltaTime * volumeChangeSpeed);
        percussionSource.volume = Mathf.Lerp(percussionSource.volume, targetVolume, Time.deltaTime * volumeChangeSpeed);
    }

    void PlayRandomClip(AudioSource source, AudioClip[] clips)
    {
        if (clips.Length == 0) return;
        source.clip = clips[Random.Range(0, clips.Length)];
        source.Play();
    }

    int GetTowerHealth()
    {
        // Use reflection to read private field safely
        var field = typeof(Tower).GetField("currentHealth",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
            return (int)field.GetValue(tower);
        return tower.maxHealth;
    }
}
