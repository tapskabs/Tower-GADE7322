using UnityEngine;

public class BuildSpot : MonoBehaviour
{
    public bool occupied;
    public Renderer previewRenderer;
    public Color freeColor = new Color(0f, 1f, 0.3f, 0.35f);
    public Color blockedColor = new Color(1f, 0f, 0f, 0.35f);


    private void Start()
    {
        if (previewRenderer != null) previewRenderer.material.color = freeColor;
    }


    public bool CanBuild => !occupied;


    public void SetOccupied(bool value)
    {
        occupied = value;
        if (previewRenderer != null)
            previewRenderer.material.color = occupied ? blockedColor : freeColor;
    }
}