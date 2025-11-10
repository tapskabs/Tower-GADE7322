using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TowerGlow : MonoBehaviour
{
    private Renderer rend;
    private Color baseColor;
    public Color glowColor = Color.cyan;
    private bool glowing = false;
    private float lerpSpeed = 5f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        baseColor = rend.material.color;
    }

    public void SetGlow(bool active)
    {
        glowing = active;
    }

    void Update()
    {
        Color target = glowing ? glowColor : baseColor;
        rend.material.color = Color.Lerp(rend.material.color, target, Time.deltaTime * lerpSpeed);
    }
}
