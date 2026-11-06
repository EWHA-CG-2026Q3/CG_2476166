using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_Person : MonoBehaviour
{
    public bool longHair = false;
    public float headTilt = 0f;
    public float scale = 1f;
    public int lat = 10;
    public int lon = 14;

    Mesh mesh;
    bool dirty = true;

    const int SKIN = 0, HAIR = 1, TOP = 2, PANTS = 3, SHOES = 4;

    List<Vector3> v = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int>[] sub = { new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>() };

    static readonly Vector3 neck = new Vector3(0f, 0.52f, 0f);

    void OnEnable() { dirty = true; }
    void OnValidate() { dirty = true; }

    void Update()
    {
        if (dirty) { Build(); dirty = false; }
    }

    void Build()
    {
        v.Clear(); uv.Clear();
        foreach (var s in sub) s.Clear();

        Part(PANTS, new Vector3(0f, 0.08f, 0f), new Vector3(0.16f, 0.1f, 0.14f), 0, false);
        Part(TOP, new Vector3(0f, 0.32f, 0f), new Vector3(0.17f, 0.22f, 0.13f), 0, false);

        Part(SKIN, new Vector3(0f, 0.7f, 0f), new Vector3(0.17f, 0.18f, 0.17f), 0, true);
        Part(HAIR, new Vector3(0f, 0.75f, -0.03f), new Vector3(0.185f, 0.17f, 0.185f), 0, true);
        if (longHair)
            Part(HAIR, new Vector3(0f, 0.55f, -0.1f), new Vector3(0.17f, 0.22f, 0.08f), 0, true);

        Part(TOP, new Vector3(-0.2f, 0.32f, 0.03f), new Vector3(0.055f, 0.16f, 0.055f), -10f, false);
        Part(TOP, new Vector3(0.2f, 0.32f, 0.03f), new Vector3(0.055f, 0.16f, 0.055f), 10f, false);
        Part(SKIN, new Vector3(-0.21f, 0.15f, 0.08f), new Vector3(0.04f, 0.04f, 0.04f), 0, false);
        Part(SKIN, new Vector3(0.21f, 0.15f, 0.08f), new Vector3(0.04f, 0.04f, 0.04f), 0, false);

        Part(PANTS, new Vector3(-0.08f, 0.06f, 0.18f), new Vector3(0.07f, 0.07f, 0.18f), 0, false);
        Part(PANTS, new Vector3(0.08f, 0.06f, 0.18f), new Vector3(0.07f, 0.07f, 0.18f), 0, false);
        Part(PANTS, new Vector3(-0.08f, -0.2f, 0.34f), new Vector3(0.06f, 0.2f, 0.06f), 0, false);
        Part(PANTS, new Vector3(0.08f, -0.2f, 0.34f), new Vector3(0.06f, 0.2f, 0.06f), 0, false);
        Part(SHOES, new Vector3(-0.08f, -0.42f, 0.38f), new Vector3(0.065f, 0.045f, 0.1f), 0, false);
        Part(SHOES, new Vector3(0.08f, -0.42f, 0.38f), new Vector3(0.065f, 0.045f, 0.1f), 0, false);

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_Person" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.SetVertices(v);
        mesh.SetUVs(0, uv);
        mesh.subMeshCount = sub.Length;
        for (int i = 0; i < sub.Length; i++) mesh.SetTriangles(sub[i], i);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mr = GetComponent<MeshRenderer>();
        var mats = mr.sharedMaterials;
        bool missing = mats.Length < sub.Length;
        for (int i = 0; !missing && i < sub.Length; i++) if (mats[i] == null) missing = true;
        if (missing)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var arr = new Material[sub.Length];
            for (int i = 0; i < arr.Length; i++) arr[i] = (i < mats.Length && mats[i] != null) ? mats[i] : new Material(lit);
            mr.sharedMaterials = arr;
        }
    }

    void Part(int s, Vector3 c, Vector3 r, float rollDeg, bool head)
    {
        Quaternion tilt = Quaternion.Euler(0, 0, head ? headTilt : 0f);
        Quaternion roll = Quaternion.Euler(0, 0, rollDeg);
        Vector3 center = head ? neck + tilt * (c - neck) : c;
        Quaternion q = tilt * roll;
        center *= scale;

        int start = v.Count;
        for (int i = 0; i <= lat; i++)
        {
            float phi = Mathf.PI * i / lat;
            for (int j = 0; j <= lon; j++)
            {
                float th = 2f * Mathf.PI * j / lon;
                Vector3 d = new Vector3(Mathf.Sin(phi) * Mathf.Cos(th), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(th));
                v.Add(center + q * Vector3.Scale(d, r) * scale);
                uv.Add(new Vector2((float)j / lon, (float)i / lat));
            }
        }

        for (int i = 0; i < lat; i++)
        {
            for (int j = 0; j < lon; j++)
            {
                int a = start + i * (lon + 1) + j;
                int b = a + 1;
                int c2 = a + lon + 1;
                int d = c2 + 1;
                Tri(sub[s], a, c2, b, center);
                Tri(sub[s], b, c2, d, center);
            }
        }
    }

    void Tri(List<int> tt, int a, int b, int c, Vector3 center)
    {
        Vector3 pa = v[a], pb = v[b], pc = v[c];
        Vector3 outward = (pa + pb + pc) / 3f - center;
        if (Vector3.Dot(Vector3.Cross(pb - pa, pc - pa), outward) < 0)
        {
            int t = b; b = c; c = t;
        }
        tt.Add(a); tt.Add(b); tt.Add(c);
    }
}