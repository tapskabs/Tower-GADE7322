using UnityEngine;
using UnityEngine.UI;

public class TowerHealthOverlay : MonoBehaviour
{
    [Header("UI Settings")]
    public RawImage overlayImage;             // The red overlay image
    [Range(0f, 1f)] public float fadeSpeed = 3f;       // How fast the overlay fades
    [Range(0f, 1f)] public float healthThreshold = 0.3f; // Health % to start showing overlay
    [Range(0f, 1f)] public float maxAlpha = 0.5f;      // Maximum overlay opacity

    [Header("Tower Settings")]
    public Tower tower; // Reference your Tower script here

    void Start()
    {
        if (overlayImage != null)
        {
            Color c = overlayImage.color;
            overlayImage.color = new Color(c.r, c.g, c.b, 0f); // start invisible
        }
    }

    void Update()
    {
        if (tower == null || overlayImage == null) return;

        float healthPercent = (float)tower.currentHealth / tower.maxHealth;

        // Determine how strong the overlay should be
        float targetAlpha = healthPercent < healthThreshold ?
            Mathf.Lerp(0f, maxAlpha, 1f - (healthPercent / healthThreshold)) : 0f;

        // Smoothly update the overlay alpha
        Color c = overlayImage.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * fadeSpeed);
        overlayImage.color = c;
    }
}
