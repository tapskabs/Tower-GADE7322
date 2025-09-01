

using UnityEngine;

public class DefenderPlacement : MonoBehaviour
{
    public Camera cam;
    public Defender defenderPrefab;
    public LayerMask buildSpotMask;
    public int defenderCost = 50;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray r = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(r, out RaycastHit hit, 200f, buildSpotMask))
            {
                var spot = hit.collider.GetComponent<BuildSpot>();
                if (spot != null && spot.CanBuild && ResourceManager.Instance.TrySpend(defenderCost))
                {
                    var def = Instantiate(defenderPrefab, spot.transform.position, Quaternion.identity);
                    spot.SetOccupied(true);
                }
            }
        }
    }
}