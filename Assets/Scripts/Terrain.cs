using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Terrain : MonoBehaviour
{
    public int width = 50;
    public int length = 50;
    public float scale = 5f;

    void Start()
    {
        GenerateTerrain();
    }

    void GenerateTerrain()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(width + 1) * (length + 1)];
        int[] triangles = new int[width * length * 6];

        // Generate vertices with Perlin Noise
        for (int i = 0, z = 0; z <= length; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float y = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * scale;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        // Generate triangles
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
