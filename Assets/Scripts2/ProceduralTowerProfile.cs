using UnityEngine;

[System.Serializable]

public class ProceduralTowerProfile : MonoBehaviour
{
    [Header("Identity")]
    public string seedName = "Variant";
    public Color tint = Color.white;
    public float scaleMultiplier = 1f;

    [Header("Stat multipliers (applied multiplicatively to tower base)")]
    public float damageMultiplier = 1f;
    public float attackRateMultiplier = 1f; // multiply the tower.attackRate (use <1 to be faster)
    public float rangeMultiplier = 1f;
    public float healthMultiplier = 1f;

    [Header("Special")]
    public float slowChance = 0f;   // chance to add a slowing effect on hit (0..1)
    public float poisonChance = 0f; // chance to add poison on hit (0..1)

    [Header("VFX")]
    public GameObject particlePrefab; // optional visual effect prefab
}
