using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Size")]
    public int width = 80; // x
    public int depth = 80; // z
    public float scale = 12f; // noise scale
    public float height = 5f;


    [Header("Paths")]
    public int numberOfPaths = 3;
    public float pathWidth = 3.0f;
    public Transform towerPrefab;
    public Tower towerComponentPrefab; // used to add Tower + Health if spawning separate


    [Header("Prefabs")]
    public EnemySpawner spawnerPrefab;
    public Enemy enemyPrefab;
    public Defender defenderPrefab;
    public Projectile projectilePrefab;
    public BuildSpot buildSpotPrefab;


    [Header("Placement")] public LayerMask groundMask;


    private Mesh mesh;
    private float[,] heights;
    private List<List<Vector3>> pathPoints;


    private void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        Generate();
    }


    public void Generate()
    {
        CreateHeights();
        CarvePaths();
        BuildMesh();
        SpawnTowerAndSystems();
        PlaceSpawners();
        CreateBuildSpots();
    }

    private void SpawnTowerAndSystems()
    {
        Vector3 centerPos = new Vector3(width * 0.5f, 0f, depth * 0.5f);
        // Find Y from heights grid
        int cx = Mathf.RoundToInt(centerPos.x);
        int cz = Mathf.RoundToInt(centerPos.z);
        float y = heights[cx, cz];
        Vector3 place = new Vector3(centerPos.x, y, centerPos.z);


        Transform t = null;
        if (towerPrefab != null)
        {
            t = Instantiate(towerPrefab, place, Quaternion.identity);
        }
        else
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.transform.position = place + Vector3.up * 1f;
            t = go.transform;
        }


        // Ensure Tower + Health + Projectile references
        Tower tower = t.GetComponent<Tower>();
        if (tower == null) tower = t.gameObject.AddComponent<Tower>();
        var health = t.GetComponent<Health>();
        if (health == null) health = t.gameObject.AddComponent<Health>();


        if (tower.projectilePrefab == null && projectilePrefab != null) tower.projectilePrefab = projectilePrefab;
        if (tower.firePoint == null)
        {
            var fp = new GameObject("FirePoint").transform; fp.SetParent(t); fp.localPosition = new Vector3(0, 1.5f, 0.6f);
            tower.firePoint = fp;
        }
    }
    private void BuildMesh()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mf.sharedMesh = mesh;


        Vector3[] vertices = new Vector3[(width + 1) * (depth + 1)];
        int[] triangles = new int[width * depth * 6];
        Vector2[] uv = new Vector2[vertices.Length];


        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                int i = x + z * (width + 1);
                vertices[i] = new Vector3(x, heights[x, z], z);
                uv[i] = new Vector2((float)x / width, (float)z / depth);
            }
        }


        int t = 0;
        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int i = x + z * (width + 1);
                triangles[t++] = i;
                triangles[t++] = i + width + 1;
                triangles[t++] = i + 1;


                triangles[t++] = i + 1;
                triangles[t++] = i + width + 1;
                triangles[t++] = i + width + 2;
            }
        }


        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();


        // Add collider for raycasts
        var col = GetComponent<MeshCollider>();
        if (col == null) col = gameObject.AddComponent<MeshCollider>();
        col.sharedMesh = mesh;
    }

    private void CreateHeights()
    {
        heights = new float[width + 1, depth + 1];
        float ox = Random.Range(0f, 9999f);
        float oz = Random.Range(0f, 9999f);
        for (int z = 0; z <= depth; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float nx = (x + ox) / scale;
                float nz = (z + oz) / scale;
                float h = Mathf.PerlinNoise(nx, nz);
                heights[x, z] = h * height;
            }
        }
    }


    private void CarvePaths()
    {
        // Choose random edge points that lead to center
        pathPoints = new List<List<Vector3>>();
        Vector3 center = new Vector3(width * 0.5f, 0, depth * 0.5f);
        int tries = 0;
        for (int i = 0; i < numberOfPaths; i++)
        {
            // pick an edge point
            Vector3 start;
            if (i % 4 == 0) start = new Vector3(Random.Range(0, width), 0, 0);
            else if (i % 4 == 1) start = new Vector3(Random.Range(0, width), 0, depth);
            else if (i % 4 == 2) start = new Vector3(0, 0, Random.Range(0, depth));
            else start = new Vector3(width, 0, Random.Range(0, depth));
            var route = GenerateJaggyPath(start, center);
            {
                Vector3 centerPos = new Vector3(width * 0.5f, 0f, depth * 0.5f);
                // Find Y from heights grid
                int cx = Mathf.RoundToInt(centerPos.x);
                int cz = Mathf.RoundToInt(centerPos.z);
                float y = heights[cx, cz];
                Vector3 place = new Vector3(centerPos.x, y, centerPos.z);


                Transform t = null;
                if (towerPrefab != null)
                {
                    t = Instantiate(towerPrefab, place, Quaternion.identity);
                }
                else
                {
                    var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    go.transform.position = place + Vector3.up * 1f;
                    t = go.transform;
                }


                // Ensure Tower + Health + Projectile references
                Tower tower = t.GetComponent<Tower>();
                if (tower == null) tower = t.gameObject.AddComponent<Tower>();
                var health = t.GetComponent<Health>();
                if (health == null) health = t.gameObject.AddComponent<Health>();


                if (tower.projectilePrefab == null && projectilePrefab != null) tower.projectilePrefab = projectilePrefab;
                if (tower.firePoint == null)
                {
                    var fp = new GameObject("FirePoint").transform; fp.SetParent(t); fp.localPosition = new Vector3(0, 1.5f, 0.6f);
                    tower.firePoint = fp;
                }
            }
        }
    }


    private List<Vector3> GenerateJaggyPath(Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        path.Add(start);

        Vector3 current = start;
        while (Vector3.Distance(current, end) > pathWidth * 2)
        {
            // Create a point that moves towards the target with some randomness
            Vector3 direction = (end - current).normalized;
            float stepSize = pathWidth * Random.Range(1.5f, 2.5f);

            // Add some randomness perpendicular to the direction
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
            float offset = Random.Range(-pathWidth, pathWidth);

            Vector3 nextPoint = current + direction * stepSize + perpendicular * offset;

            // Ensure point stays within bounds
            nextPoint.x = Mathf.Clamp(nextPoint.x, 0, width);
            nextPoint.z = Mathf.Clamp(nextPoint.z, 0, depth);

            path.Add(nextPoint);
            current = nextPoint;

            // Flatten terrain around path
            int px = Mathf.RoundToInt(nextPoint.x);
            int pz = Mathf.RoundToInt(nextPoint.z);
            FlattenAreaAroundPoint(px, pz);
        }

        path.Add(end);
        return path;
    }

    private void FlattenAreaAroundPoint(int centerX, int centerZ)
    {
        int radius = Mathf.CeilToInt(pathWidth);
        for (int x = -radius; x <= radius; x++)
        {
            for (int z = -radius; z <= radius; z++)
            {
                int px = centerX + x;
                int pz = centerZ + z;

                if (px >= 0 && px <= width && pz >= 0 && pz <= depth)
                {
                    if (Vector2.Distance(Vector2.zero, new Vector2(x, z)) <= pathWidth)
                    {
                        heights[px, pz] = 0; // flatten to zero height
                    }
                }
            }
        }
    }


    private void PlaceSpawners()
    {
        if (spawnerPrefab == null) return;
        foreach (var route in pathPoints)
        {
            if (route.Count == 0) continue;
            Vector3 spawnPos = new Vector3(route[0].x, heights[(int)route[0].x, (int)route[0].z], route[0].z);
            var spawner = Instantiate(spawnerPrefab, spawnPos + Vector3.up * 0.1f, Quaternion.identity);
            spawner.enemyPrefab = enemyPrefab;


            // Create waypoint transforms from route
            List<Transform> wps = new List<Transform>();
            foreach (var p in route)
            {
                var wp = new GameObject("WP").transform;
                wp.position = new Vector3(p.x, heights[(int)p.x, (int)p.z], p.z) + Vector3.up * 0.1f;
                wps.Add(wp);
            }
            spawner.waypoints = wps;
            spawner.Begin();
        }
    }
    private void CreateBuildSpots()
    {
        if (buildSpotPrefab == null) return;
        // Scatter spots near (but not on) paths
        int spots = Mathf.RoundToInt((width * depth) * 0.01f); // 1% of tiles
        int created = 0; int guard = 0;
        while (created < spots && guard++ < spots * 20)
        {
            int x = Random.Range(2, width - 2);
            int z = Random.Range(2, depth - 2);
            if (IsOnPath(x, z)) continue; // keep paths clear


            Vector3 pos = new Vector3(x, heights[x, z], z);
            var spot = Instantiate(buildSpotPrefab, pos + Vector3.up * 0.05f, Quaternion.identity);
            created++;
        }
    }


    private bool IsOnPath(int x, int z)
    {
        float h = heights[x, z];
        return Mathf.Abs(h - 0f) < 0.0001f; // flattened = path
    }
}

