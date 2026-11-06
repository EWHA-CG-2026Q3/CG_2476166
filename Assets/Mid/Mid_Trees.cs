using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_Trees : MonoBehaviour
{
    public int treeCount = 8;
    public float fieldMin = 3.5f;
    public float fieldMax = 8.5f;
    public Vector3[] avoids = { new Vector3(-5f, 3f, 3.8f), new Vector3(7f, 0f, 2f) };
    public int seed = 5;
    public int segments = 8;
    public Vector2 trunkHeight = new Vector2(0.8f, 1.4f);
    public float trunkRadius = 0.12f;
    public int layers = 3;
    public Vector2 canopyRadius = new Vector2(0.9f, 1.3f);
    public float layerHeight = 0.9f;
    public float leafJitter = 0.15f;

    Mesh mesh;
    bool dirty = true;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> trunkTris = new List<int>();
    List<int> leafTris = new List<int>();

    void OnEnable() { dirty = true; }
    void OnValidate() { dirty = true; }

    void Update()
    {
        if (dirty) { Build(); dirty = false; }
    }

    float R(System.Random rnd) { return (float)rnd.NextDouble(); }

    bool Blocked(float x, float z)
    {
        foreach (var v in avoids)
            if (new Vector2(x - v.x, z - v.y).magnitude < v.z) return true;
        return false;
    }

    void Build()
    {
        verts.Clear(); uvs.Clear(); trunkTris.Clear(); leafTris.Clear();
        var rnd = new System.Random(seed);

        int placed = 0, tries = 0;
        while (placed < treeCount && tries < treeCount * 50)
        {
            tries++;
            float a = R(rnd) * Mathf.PI * 2f;
            float rr = Mathf.Sqrt(Mathf.Lerp(fieldMin * fieldMin, fieldMax * fieldMax, R(rnd)));
            float x = Mathf.Cos(a) * rr, z = Mathf.Sin(a) * rr;
            if (Blocked(x, z)) continue;

            float scale = Mathf.Lerp(0.8f, 1.3f, R(rnd));
            AddTree(new Vector3(x, 0, z), scale, rnd);
            placed++;
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_Trees" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(trunkTris, 0);
        mesh.SetTriangles(leafTris, 1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mc = GetComponent<MeshCollider>();
        if (mc == null) mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = mesh;

        var mr = GetComponent<MeshRenderer>();
        var mats = mr.sharedMaterials;
        if (mats.Length < 2 || mats[0] == null || mats[1] == null)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            mr.sharedMaterials = new Material[] { new Material(lit), new Material(lit) };
        }
    }

    void AddTree(Vector3 p, float s, System.Random rnd)
    {
        float th = Mathf.Lerp(trunkHeight.x, trunkHeight.y, R(rnd)) * s;
        float tr = trunkRadius * s;
        for (int i = 0; i < segments; i++)
        {
            float a0 = 2f * Mathf.PI * i / segments, a1 = 2f * Mathf.PI * (i + 1) / segments;
            Vector3 b0 = p + new Vector3(Mathf.Cos(a0) * tr, 0, Mathf.Sin(a0) * tr);
            Vector3 b1 = p + new Vector3(Mathf.Cos(a1) * tr, 0, Mathf.Sin(a1) * tr);
            Vector3 t0 = b0 + Vector3.up * th, t1 = b1 + Vector3.up * th;
            Vector3 axis = p + Vector3.up * th * 0.5f;
            AddTri(trunkTris, b0, b1, t1, axis);
            AddTri(trunkTris, b0, t1, t0, axis);
        }

        float cr = Mathf.Lerp(canopyRadius.x, canopyRadius.y, R(rnd)) * s;
        float spin = R(rnd) * Mathf.PI * 2f;
        for (int L = 0; L < layers; L++)
        {
            float y0 = p.y + th * 0.6f + L * layerHeight * 0.7f * s;
            float rad = cr * (1f - L * 0.25f);
            float h = layerHeight * 1.4f * s;
            Vector3 baseC = new Vector3(p.x, y0, p.z);
            Vector3 tip = baseC + Vector3.up * h;

            Vector3[] ring = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float a = 2f * Mathf.PI * i / segments + spin + L * 0.4f;
                float jr = rad * (1f + (R(rnd) - 0.5f) * 2f * leafJitter);
                float jy = (R(rnd) - 0.5f) * 0.15f * s;
                ring[i] = baseC + new Vector3(Mathf.Cos(a) * jr, jy, Mathf.Sin(a) * jr);
            }
            Vector3 center = baseC + Vector3.up * h * 0.3f;
            for (int i = 0; i < segments; i++)
            {
                int j = (i + 1) % segments;
                AddTri(leafTris, ring[i], ring[j], tip, center);
                AddTri(leafTris, ring[i], ring[j], baseC, center);
            }
        }
    }

    void AddTri(List<int> target, Vector3 a, Vector3 b, Vector3 c, Vector3 center)
    {
        Vector3 outward = (a + b + c) / 3f - center;
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), outward) < 0)
        {
            Vector3 t = b; b = c; c = t;
        }
        int s = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c);
        uvs.Add(new Vector2(a.x, a.y)); uvs.Add(new Vector2(b.x, b.y)); uvs.Add(new Vector2(c.x, c.y));
        target.Add(s); target.Add(s + 1); target.Add(s + 2);
    }
}