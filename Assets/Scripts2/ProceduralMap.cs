using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMap : MonoBehaviour
{
    [Header("Map Size")]
    public int width = 80;
    public int length = 80;
    public float heightScale = 6f;
    public float noiseScale = 0.08f;

    [Header("Paths")]
    public int numberOfPaths = 3;       
    public float pathWidth = 3f;        
    public int pathResolution = 30;     

    [Header("Placement Nodes")]
    public int nodesPerPath = 4;             
    public float nodeDistanceFromPath = 2.5f;
    public GameObject defenderNodePrefab;    
    public GameObject miningNodePrefab;      

   
    [HideInInspector] public Vector3 centerPoint;
    [HideInInspector] public List<List<Vector3>> paths = new List<List<Vector3>>();
    [HideInInspector] public List<Vector3> spawnPoints = new List<Vector3>();
    [HideInInspector] public List<DefenderNode> defenderNodes = new List<DefenderNode>();
    [HideInInspector] public List<MiningNode> miningNodes = new List<MiningNode>();

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        centerPoint = new Vector3(width * 0.5f, 0f, length * 0.5f);
        BuildBaseMesh();
        CreatePaths();
        ApplyMesh();
    }

  
    void BuildBaseMesh()
    {
        mesh = new Mesh();
        vertices = new Vector3[(width + 1) * (length + 1)];
        triangles = new int[width * length * 6];

        for (int i = 0, z = 0; z <= length; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                float y = Mathf.PerlinNoise((x + Time.time) * noiseScale,
                                            (z + Time.time * 2f) * noiseScale) * heightScale;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        int vert = 0, tri = 0;
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[tri + 0] = vert + 0;
                triangles[tri + 1] = vert + width + 1;
                triangles[tri + 2] = vert + 1;
                triangles[tri + 3] = vert + 1;
                triangles[tri + 4] = vert + width + 1;
                triangles[tri + 5] = vert + width + 2;

                vert++;
                tri += 6;
            }
            vert++;
        }
    }

    
    void CreatePaths()
    {
        paths.Clear();
        spawnPoints.Clear();
        defenderNodes.Clear();
        miningNodes.Clear();

        float edge = Mathf.Max(width, length);

        for (int p = 0; p < numberOfPaths; p++)
        {
            float angle = ((float)p / numberOfPaths) * Mathf.PI * 2f + Random.Range(-0.3f, 0.3f);
            Vector3 anchor = centerPoint + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (edge * 0.5f);
            anchor.x = Mathf.Clamp(anchor.x, 1f, width - 1f);
            anchor.z = Mathf.Clamp(anchor.z, 1f, length - 1f);
            anchor.y = GetHeightAt(anchor.x, anchor.z);

          
            List<Vector3> rawPath = new List<Vector3>();
            for (int i = 0; i <= pathResolution; i++)
            {
                float t = (float)i / pathResolution;
                float s = t * t * (3f - 2f * t); // smoothstep
                Vector3 point = Vector3.Lerp(anchor, centerPoint, s);
                Vector3 perp = Vector3.Cross(Vector3.up, (centerPoint - anchor).normalized);
                float jitter = Mathf.PerlinNoise(p * 10 + i * 0.1f, Time.time * 0.1f) - 0.5f;
                point += perp * jitter * 1.2f;
                point.y = GetHeightAt(point.x, point.z);
                rawPath.Add(point);
            }

            List<Vector3> smoothedPath = SmoothPath(rawPath, 8);
            paths.Add(smoothedPath);
            spawnPoints.Add(anchor);

           
            CarvePathIntoHeightmap(smoothedPath);

           
            GenerateNodesNearPath(smoothedPath, nodesPerPath, nodeDistanceFromPath);
        }
    }

   
    List<Vector3> SmoothPath(List<Vector3> rawPoints, int smoothFactor)
    {
        List<Vector3> smoothed = new List<Vector3>();

        for (int i = 0; i < rawPoints.Count - 1; i++)
        {
            Vector3 p0 = rawPoints[Mathf.Max(i - 1, 0)];
            Vector3 p1 = rawPoints[i];
            Vector3 p2 = rawPoints[Mathf.Min(i + 1, rawPoints.Count - 1)];
            Vector3 p3 = rawPoints[Mathf.Min(i + 2, rawPoints.Count - 1)];

            for (int j = 0; j < smoothFactor; j++)
            {
                float t = j / (float)smoothFactor;
                Vector3 pos = CatmullRom(p0, p1, p2, p3, t);
                pos.y = GetHeightAt(pos.x, pos.z);
                smoothed.Add(pos);
            }
        }

        smoothed.Add(rawPoints[rawPoints.Count - 1]);
        return smoothed;
    }

   
    void CarvePathIntoHeightmap(List<Vector3> singlePath)
    {
        float half = pathWidth * 0.5f;

        for (int vi = 0; vi < vertices.Length; vi++)
        {
            Vector3 v = vertices[vi];
            Vector2 v2 = new Vector2(v.x, v.z);
            float minDist = float.MaxValue;
            Vector3 closest = v;

            foreach (var p in singlePath)
            {
                float d = Vector2.Distance(new Vector2(p.x, p.z), v2);
                if (d < minDist)
                {
                    minDist = d;
                    closest = p;
                }
            }

            if (minDist <= half)
            {
                float targetY = closest.y;
                float t = 1f - (minDist / half);
                vertices[vi].y = Mathf.Lerp(v.y, targetY, t);
            }
        }
    }

   
    void GenerateNodesNearPath(List<Vector3> pathList, int count, float distanceFromPath)
    {
        if (pathList == null || pathList.Count == 0) return;
        int step = Mathf.Max(1, pathList.Count / (count + 1));

        for (int i = step; i < pathList.Count - step; i += step)
        {
            Vector3 p = pathList[i];
            Vector3 dir = (i < pathList.Count - 1)
                ? (pathList[i + 1] - pathList[i - 1]).normalized
                : (centerPoint - pathList[i]).normalized;

            Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
            float side = (Random.value > 0.5f) ? 1f : -1f;
            Vector3 nodePos = p + perp * side * distanceFromPath;
            nodePos.y = GetHeightAt(nodePos.x, nodePos.z) + 0.1f;

            
            if (Random.value > 0.5f && defenderNodePrefab != null)
            {
                GameObject go = Instantiate(defenderNodePrefab, nodePos, Quaternion.identity, transform);
                DefenderNode dn = go.GetComponent<DefenderNode>();
                if (dn != null) defenderNodes.Add(dn);
            }
            else if (miningNodePrefab != null)
            {
                GameObject go = Instantiate(miningNodePrefab, nodePos, Quaternion.identity, transform);
                MiningNode mn = go.GetComponent<MiningNode>();
                if (mn != null) miningNodes.Add(mn);
            }
        }
    }

    
    public float GetHeightAt(float x, float z)
    {
        int xi = Mathf.Clamp(Mathf.RoundToInt(x), 0, width);
        int zi = Mathf.Clamp(Mathf.RoundToInt(z), 0, length);
        int idx = zi * (width + 1) + xi;
        if (vertices != null && idx >= 0 && idx < vertices.Length)
            return vertices[idx].y;
        return 0f;
    }

    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }

    void ApplyMesh()
    {
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    
    public Vector3 GetTowerSpawnPoint()
    {
        if (paths == null || paths.Count == 0) return centerPoint;

        Vector3 sum = Vector3.zero;
        int count = 0;
        foreach (var path in paths)
        {
            if (path.Count > 0)
            {
                sum += path[path.Count - 1];
                count++;
            }
        }

        Vector3 avg = sum / count;
        avg.y = GetHeightAt(avg.x, avg.z);
        return avg;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (paths != null)
        {
            foreach (var path in paths)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Gizmos.DrawLine(path[i], path[i + 1]);
                }
            }
        }

        Gizmos.color = Color.blue;
        if (spawnPoints != null)
        {
            foreach (var s in spawnPoints)
                Gizmos.DrawSphere(s, 0.5f);
        }

        Gizmos.color = Color.green;
        if (defenderNodes != null)
        {
            foreach (var dn in defenderNodes)
                if (dn != null) Gizmos.DrawCube(dn.transform.position, Vector3.one * 0.4f);
        }

        Gizmos.color = Color.yellow;
        if (miningNodes != null)
        {
            foreach (var mn in miningNodes)
                if (mn != null) Gizmos.DrawSphere(mn.transform.position, 0.3f);
        }
    }
#endif
}
