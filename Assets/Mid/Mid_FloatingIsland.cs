using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_FloatingIsland : MonoBehaviour
{
    [Header("섬 모양")]
    public int segments = 28;            // 둘레를 나누는 수
    public float radius = 5f;            // 윗면 반지름
    public float rimDepth = 0.6f;        // 윗면 가장자리 흙층 두께
    public float depth = 5f;             // 아래로 뾰족한 깊이
    public int coneRings = 4;            // 원뿔을 나누는 층 수
    [Range(0f, 0.4f)] public float roughness = 0.18f;  // 울퉁불퉁한 정도
    public int seed = 7;                 // 바꾸면 모양이 달라짐
    public float uvScale = 0.25f;        // 텍스처 크기

    Mesh mesh;
    bool dirty = true;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> topTris = new List<int>();    // submesh 0: 윗면 (풀)
    List<int> sideTris = new List<int>();   // submesh 1: 옆, 아래 (바위)

    void OnEnable() { dirty = true; }
    void OnValidate() { dirty = true; }

    void Update()
    {
        if (dirty) { Build(); dirty = false; }
    }

    // 울퉁불퉁하게 만드는 값 (1 근처)
    float Jitter(int i, int k)
    {
        float n = Mathf.PerlinNoise(seed * 10f + i * 0.37f, k * 1.3f + seed);
        return 1f + roughness * (n - 0.5f) * 2f;
    }

    void Build()
    {
        verts.Clear(); uvs.Clear(); topTris.Clear(); sideTris.Clear();

        // 1. 반복문으로 링(둘레의 점들) 만들기: 0 = 윗면 가장자리, 1 = 흙층 아래, 2~ = 원뿔
        int ringCount = coneRings + 2;
        Vector3[][] rings = new Vector3[ringCount][];
        for (int k = 0; k < ringCount; k++)
        {
            float y, r;
            if (k == 0) { y = 0f; r = radius; }
            else if (k == 1) { y = -rimDepth; r = radius * 1.02f; }
            else
            {
                float t = (k - 1f) / (coneRings + 1f);       // 0 → 1로 갈수록 아래
                y = -rimDepth - depth * t;
                r = radius * Mathf.Pow(1f - t, 1.2f);        // 아래로 갈수록 좁아짐
            }

            rings[k] = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float a = 2f * Mathf.PI * i / segments;      // 점 i의 각도 = 360° / n × i
                float rr = r * Jitter(i, k);
                float yy = (k >= 2) ? y + roughness * (Jitter(i + 50, k) - 1f) * 2f : y;
                rings[k][i] = new Vector3(Mathf.Cos(a) * rr, yy, Mathf.Sin(a) * rr);
            }
        }
        Vector3 center = Vector3.zero;
        Vector3 tip = new Vector3(0, -rimDepth - depth, 0);

        // 2. 윗면: 가운데 점 + 이웃한 두 점으로 삼각형 (부채꼴)
        for (int i = 0; i < segments; i++)
        {
            int j = (i + 1) % segments;
            AddTri(topTris, center, rings[0][i], rings[0][j], Vector3.up);
        }

        // 3. 옆면: 위 링과 아래 링 사이를 사각형으로 이음
        for (int k = 0; k < ringCount - 1; k++)
        {
            for (int i = 0; i < segments; i++)
            {
                int j = (i + 1) % segments;
                Vector3 a = rings[k][i], b = rings[k][j], c = rings[k + 1][j], d = rings[k + 1][i];
                Vector3 outward = Flat((a + b + c + d) * 0.25f);
                AddTri(sideTris, a, b, c, outward);
                AddTri(sideTris, a, c, d, outward);
            }
        }

        // 4. 맨 아래: 마지막 링 + 뾰족한 끝점
        int last = ringCount - 1;
        for (int i = 0; i < segments; i++)
        {
            int j = (i + 1) % segments;
            Vector3 a = rings[last][i], b = rings[last][j];
            AddTri(sideTris, a, b, tip, Flat((a + b) * 0.5f));
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_FloatingIsland" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(topTris, 0);
        mesh.SetTriangles(sideTris, 1);
        mesh.RecalculateNormals();   // 삼각형마다 정점을 나눴으므로 각진 로우폴리 (S19)
        mesh.RecalculateBounds();

        var mc = GetComponent<MeshCollider>();
        if (mc == null) mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = mesh;

        // 재질 칸 2개가 비어 있을 때만 기본 재질 (Inspector에서 넣은 재질은 그대로)
        var mr = GetComponent<MeshRenderer>();
        var mats = mr.sharedMaterials;
        if (mats.Length < 2 || mats[0] == null || mats[1] == null)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var grass = new Material(lit) { color = new Color(0.25f, 0.45f, 0.25f) };
            var rock = new Material(lit) { color = new Color(0.35f, 0.32f, 0.3f) };
            mr.sharedMaterials = new Material[] { grass, rock };
        }
    }

    Vector3 Flat(Vector3 p) { return new Vector3(p.x, 0, p.z).normalized; }

    // 삼각형 하나: 정점 3개를 새로 추가 (면마다 정점 분리), 바깥을 보도록 감기 순서 자동 맞춤
    void AddTri(List<int> target, Vector3 a, Vector3 b, Vector3 c, Vector3 outward)
    {
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), outward) < 0)
        {
            Vector3 t = b; b = c; c = t;
        }
        int s = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c);
        uvs.Add(UV(a)); uvs.Add(UV(b)); uvs.Add(UV(c));
        target.Add(s); target.Add(s + 1); target.Add(s + 2);
    }

    Vector2 UV(Vector3 p) { return new Vector2(p.x * uvScale, p.z * uvScale + p.y * uvScale); }
}