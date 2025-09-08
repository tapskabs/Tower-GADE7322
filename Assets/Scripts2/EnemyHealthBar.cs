using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        if (slider != null)
        {
            slider.minValue = 0;
        }
    }

    public void SetHealth(int current, int max)
    {
        if (slider != null)
        {
            slider.maxValue = max;
            slider.value = Mathf.Clamp(current, 0, max);
        }
    }
}
