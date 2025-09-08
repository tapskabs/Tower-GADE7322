using UnityEngine;

public class Mine : MonoBehaviour
{
    [Header("Economy")]
    public int resourcePerCycle = 20;      
    public float cycleTime = 10f;         

    private float timer = 0f;

    [Header("Health")]
    public int maxHealth = 80;
    private int currentHealth;

    public GameObject healthBarPrefab;
    private DefenderHealthBar healthBar; 

    void Start()
    {
        currentHealth = maxHealth;

       
        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            hb.transform.SetParent(GameObject.Find("Canvas").transform, false);
            healthBar = hb.GetComponent<DefenderHealthBar>();
            if (healthBar != null)
                healthBar.SetHealth(currentHealth, maxHealth);
        }
    }

    void Update()
    {
       
        timer += Time.deltaTime;
        if (timer >= cycleTime)
        {
            GameManager.Instance?.AddResources(resourcePerCycle);
            timer = 0f;
        }

       
        if (healthBar != null)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            healthBar.transform.position = screenPos;
        }
    }

    public void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;

        if (healthBar != null)
            healthBar.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            if (healthBar != null)
                Destroy(healthBar.gameObject);
            Destroy(gameObject);
        }
    }
}
