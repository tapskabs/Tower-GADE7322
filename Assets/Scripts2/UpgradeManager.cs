using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private bool isUpgradeMode = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // Exit upgrade mode if player presses Escape
        if (isUpgradeMode && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelUpgradeMode();
        }

        // Detect click on defender node
        if (isUpgradeMode && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleClick();
        }
    }

    public void EnterUpgradeMode()
    {
        isUpgradeMode = true;
        Debug.Log("Upgrade Mode: Click a defender to upgrade");
    }

    private void CancelUpgradeMode()
    {
        isUpgradeMode = false;
        Debug.Log("Upgrade Mode cancelled");
    }

    private void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            DefenderUpgrade defenderUpgrade = hit.collider.GetComponentInParent<DefenderUpgrade>();

            if (defenderUpgrade != null)
            {
                defenderUpgrade.Upgrade();
                CancelUpgradeMode();
            }
            else
            {
                Debug.Log("No upgradeable defender at this location.");
            }
        }
    }
}
