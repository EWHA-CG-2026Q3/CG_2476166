using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_PondRocks : MonoBehaviour
{
    public float ringRadius = 2.65f;
    public int rockCount = 30;
    public Vector2 rockSize = new Vector2(0.25f, 0.45f);
    public float heightScale = 0.7f;
    public float bumpiness = 0.25f;
    public int lat = 4;
    public int lon = 7;
    public int seed = 3;
    public float uvScale = 1f;

    public bool scatter = false;
    public float scatterMin = 3f;
    public float scatterMax = 9f;
    public Vector3[] avoids = { new Vector3(-5f, 3f, 3.4f), new Vector3(7f, 0f, 1.8f) };

    Mesh mesh;
    bool dirty = true;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();

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
        verts.Clear(); uvs.Clear(); tris.Clear();
        var rnd = new System.Random(seed);

        int placed = 0, tries = 0;
        while (placed < rockCount && tries < rockCount * 30)
        {
            tries++;
            float a, rr;
            if (scatter)
            {
                a = R(rnd) * Mathf.PI * 2f;
                rr = Mathf.Sqrt(Mathf.Lerp(scatterMin * scatterMin, scatterMax * scatterMax, R(rnd)));
                if (Blocked(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr)) continue;
            }
            else
            {
                a = 2f * Mathf.PI * placed / rockCount + (R(rnd) - 0.5f) * 0.1f;
                rr = ringRadius + (R(rnd) - 0.5f) * 0.15f;
            }
            float s = Mathf.Lerp(rockSize.x, rockSize.y, R(rnd));
            Vector3 c = new Vector3(Mathf.Cos(a) * rr, s * heightScale * 0.3f, Mathf.Sin(a) * rr);
            AddRock(c, s, R(rnd) * 360f, rnd);
            placed++;
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_Rocks" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mc = GetComponent<MeshCollider>();
        if (mc == null) mc = gameObject.AddComponent<MeshCollider>();
        mc.sharedMesh = null;
        mc.sharedMesh = mesh;

        var mr = GetComponent<MeshRenderer>();
        if (mr.sharedMaterial == null)
            mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }

    void AddRock(Vector3 c, float s, float spinDeg, System.Random rnd)
    {
        float spin = spinDeg * Mathf.Deg2Rad;
        Vector3[,] p = new Vector3[lat + 1, lon];

        for (int i = 0; i <= lat; i++)
        {
            float phi = Mathf.PI * i / lat;
            for (int j = 0; j < lon; j++)
            {
                float th = 2f * Mathf.PI * j / lon + spin;
                float bump = 1f + (R(rnd) - 0.5f) * 2f * bumpiness;
                if (i == 0 || i == lat) bump = 1f;
                Vector3 d = new Vector3(Mathf.Sin(phi) * Mathf.Cos(th),
                                        Mathf.Cos(phi) * heightScale,
                                        Mathf.Sin(phi) * Mathf.Sin(th));
                p[i, j] = c + d * s * bump;
            }
        }

        for (int i = 0; i < lat; i++)
        {
            for (int j = 0; j < lon; j++)
            {
                int jn = (j + 1) % lon;
                Vector3 a = p[i, j], b = p[i, jn], cc = p[i + 1, jn], d = p[i + 1, j];
                if (i > 0) AddTri(a, b, cc, c);
                if (i < lat - 1) AddTri(a, cc, d, c);
            }
        }
    }

    void AddTri(Vector3 a, Vector3 b, Vector3 c, Vector3 center)
    {
        Vector3 outward = (a + b + c) / 3f - center;
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), outward) < 0)
        {
            Vector3 t = b; b = c; c = t;
        }
        int s = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c);
        uvs.Add(new Vector2(a.x + a.y, a.z) * uvScale);
        uvs.Add(new Vector2(b.x + b.y, b.z) * uvScale);
        uvs.Add(new Vector2(c.x + c.y, c.z) * uvScale);
        tris.Add(s); tris.Add(s + 1); tris.Add(s + 2);
    }
}