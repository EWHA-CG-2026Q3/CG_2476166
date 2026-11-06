using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_FlowerField : MonoBehaviour
{
    public int flowerCount = 120;
    public float fieldRadius = 9f;
    public float clearRadius = 2.8f;
    public int petals = 6;
    public float petalLength = 0.12f;
    public float petalWidth = 0.05f;
    public Vector2 stemHeight = new Vector2(0.2f, 0.45f);
    public int colorCount = 3;
    public int seed = 11;
    public Vector3 avoidCenter = new Vector3(-5f, 0f, 3f);
    public float avoidRadius = 1.8f;

    Mesh mesh;
    bool dirty = true;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> stemTris = new List<int>();
    List<int>[] petalTris;

    void OnEnable() { dirty = true; }
    void OnValidate() { dirty = true; }

    void Update()
    {
        if (dirty) { Build(); dirty = false; }
    }

    float R(System.Random rnd) { return (float)rnd.NextDouble(); }

    void Build()
    {
        verts.Clear(); uvs.Clear(); stemTris.Clear();
        colorCount = Mathf.Max(1, colorCount);
        petalTris = new List<int>[colorCount];
        for (int c = 0; c < colorCount; c++) petalTris[c] = new List<int>();

        var rnd = new System.Random(seed);
        int placed = 0, tries = 0;
        while (placed < flowerCount && tries < flowerCount * 20)
        {
            tries++;
            float ang = R(rnd) * Mathf.PI * 2f;
            float rr = Mathf.Sqrt(R(rnd)) * fieldRadius;
            if (rr < clearRadius) continue;
            if (Vector3.Distance(new Vector3(Mathf.Cos(ang) * rr, 0f, Mathf.Sin(ang) * rr), avoidCenter) < avoidRadius) continue;

            Vector3 p = new Vector3(Mathf.Cos(ang) * rr, 0f, Mathf.Sin(ang) * rr);
            float h = Mathf.Lerp(stemHeight.x, stemHeight.y, R(rnd));
            float s = Mathf.Lerp(0.8f, 1.3f, R(rnd));
            float spin = R(rnd) * 360f;
            int col = rnd.Next(colorCount);

            AddFlower(p, h, s, spin, col);
            placed++;
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_FlowerField" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 1 + colorCount;
        mesh.SetTriangles(stemTris, 0);
        for (int c = 0; c < colorCount; c++) mesh.SetTriangles(petalTris[c], c + 1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mr = GetComponent<MeshRenderer>();
        var mats = mr.sharedMaterials;
        bool missing = mats.Length < 1 + colorCount;
        for (int i = 0; !missing && i < mats.Length; i++) if (mats[i] == null) missing = true;
        if (missing)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var arr = new Material[1 + colorCount];
            for (int i = 0; i < arr.Length; i++) arr[i] = (i < mats.Length && mats[i] != null) ? mats[i] : new Material(lit);
            mr.sharedMaterials = arr;
        }
    }

    void AddFlower(Vector3 p, float h, float s, float spin, int col)
    {
        AddBox(stemTris, p + Vector3.up * h * 0.5f, new Vector3(0.015f, h, 0.015f));

        Vector3 c = p + Vector3.up * h;
        float pl = petalLength * s, pw = petalWidth * s;

        for (int j = 0; j < petals; j++)
        {
            float a = spin * Mathf.Deg2Rad + 2f * Mathf.PI * j / petals;
            Vector3 dir = new Vector3(Mathf.Cos(a), 0.35f, Mathf.Sin(a)).normalized;
            Vector3 perp = new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a));
            Vector3 tip = c + dir * pl;
            Vector3 mid = c + dir * pl * 0.5f;
            AddDoubleQuad(petalTris[col], c, mid + perp * pw, tip, mid - perp * pw);
        }

        AddBox(petalTris[(col + 1) % colorCount], c, new Vector3(0.04f, 0.04f, 0.04f) * s);
    }

    void AddDoubleQuad(List<int> target, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        int s = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
        uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));
        target.Add(s); target.Add(s + 1); target.Add(s + 2);
        target.Add(s); target.Add(s + 2); target.Add(s + 3);
        target.Add(s); target.Add(s + 2); target.Add(s + 1);
        target.Add(s); target.Add(s + 3); target.Add(s + 2);
    }

    void AddBox(List<int> target, Vector3 c, Vector3 size)
    {
        Vector3 h = size * 0.5f;
        Vector3 P(float x, float y, float z) => c + new Vector3(x * h.x, y * h.y, z * h.z);

        AddQuad(target, P(-1, 1, -1), P(1, 1, -1), P(1, 1, 1), P(-1, 1, 1), Vector3.up);
        AddQuad(target, P(-1, -1, -1), P(1, -1, -1), P(1, -1, 1), P(-1, -1, 1), Vector3.down);
        AddQuad(target, P(1, -1, -1), P(1, -1, 1), P(1, 1, 1), P(1, 1, -1), Vector3.right);
        AddQuad(target, P(-1, -1, -1), P(-1, -1, 1), P(-1, 1, 1), P(-1, 1, -1), Vector3.left);
        AddQuad(target, P(-1, -1, 1), P(1, -1, 1), P(1, 1, 1), P(-1, 1, 1), Vector3.forward);
        AddQuad(target, P(-1, -1, -1), P(1, -1, -1), P(1, 1, -1), P(-1, 1, -1), Vector3.back);
    }

    void AddQuad(List<int> target, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 outward)
    {
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), outward) < 0)
        {
            Vector3 t = b; b = d; d = t;
        }
        int s = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
        uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));
        target.Add(s); target.Add(s + 1); target.Add(s + 2);
        target.Add(s); target.Add(s + 2); target.Add(s + 3);
    }
}