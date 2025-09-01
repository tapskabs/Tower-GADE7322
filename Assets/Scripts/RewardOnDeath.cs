using UnityEngine;

public class RewardOnDeath : MonoBehaviour
{
    public int goldReward = 5;
    private Health health;


    private void Awake()
    {
        health = GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.AddListener(() => {
                ResourceManager.Instance.Add(goldReward);
                Destroy(gameObject);
            });
        }
    }
}