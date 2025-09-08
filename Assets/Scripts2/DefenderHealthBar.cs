using UnityEngine;
using UnityEngine.UI;

public class DefenderHealthBar : MonoBehaviour
{
    public Image fill;   // drag the inner bar image here

    public void SetHealth(int current, int max)
    {
        if (fill != null)
        {
            fill.fillAmount = Mathf.Clamp01((float)current / max);
        }
    }
}
