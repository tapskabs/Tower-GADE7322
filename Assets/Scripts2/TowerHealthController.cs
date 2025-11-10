using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TowerHealthController : MonoBehaviour
{
    public Volume volume;
    private TowerHealthPostEffect effect;
    public float towerHealth = 100f;

    void Start()
    {
        volume.profile.TryGet(out effect);
    }

    void Update()
    {
        float intensityValue = Mathf.InverseLerp(100, 0, towerHealth); // 0 health = full red
        effect.intensity.value = intensityValue;
        effect.tintColor.value = Color.red;
    }
}
