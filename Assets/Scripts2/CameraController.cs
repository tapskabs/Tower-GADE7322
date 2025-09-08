using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;   // movement speed
    public float fastMultiplier = 2f; // hold shift to move faster
    public float zoomSpeed = 200f; // scrollwheel zoom speed
    public float minY = 10f; // minimum zoom height
    public float maxY = 60f; // maximum zoom height

    void Update()
    {
        Vector3 direction = Vector3.zero;

      
        if (Input.GetKey(KeyCode.W)) direction += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) direction += Vector3.back;
        if (Input.GetKey(KeyCode.A)) direction += Vector3.left;
        if (Input.GetKey(KeyCode.D)) direction += Vector3.right;

       
        direction = direction.normalized;

        
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastMultiplier : 1f);

        
        transform.position += direction * speed * Time.deltaTime;

        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * zoomSpeed * Time.deltaTime;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.position = pos;
        }
    }
}
