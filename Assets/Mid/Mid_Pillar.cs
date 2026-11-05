using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_Pillar : MonoBehaviour
{
    public int segments = 24;
    public float radius = 0.45f;
    public float height = 7.8f;
    public int bandCount = 3;
    public float bandRadius = 0.6f;
    public float bandHeight = 0.15f;
    public float uvScale = 1f;

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

    void Build()
    {
        verts.Clear(); uvs.Clear(); tris.Clear();

        AddCylinder(radius, 0f, height);

        for (int b = 1; b <= bandCount; b++)
        {
            float y = height * b / (bandCount + 1f);
            AddCylinder(bandRadius, y - bandHeight * 0.5f, y + bandHeight * 0.5f);
        }
        AddCylinder(bandRadius, height - bandHeight, height + bandHeight);
        AddCylinder(bandRadius, 0f, bandHeight * 2f);

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_Pillar" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mr = GetComponent<MeshRenderer>();
        if (mr.sharedMaterial == null)
            mr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }

    void AddCylinder(float r, float y0, float y1)
    {
        int start = verts.Count;
        for (int i = 0; i <= segments; i++)
        {
            float a = 2f * Mathf.PI * i / segments;
            float x = Mathf.Cos(a) * r, z = Mathf.Sin(a) * r;
            verts.Add(new Vector3(x, y0, z));
            verts.Add(new Vector3(x, y1, z));
            float u = (float)i / segments * uvScale * 2f;
            uvs.Add(new Vector2(u, y0 * uvScale * 0.5f));
            uvs.Add(new Vector2(u, y1 * uvScale * 0.5f));
        }
        for (int i = 0; i < segments; i++)
        {
            int b0 = start + i * 2, t0 = b0 + 1, b1 = b0 + 2, t1 = b0 + 3;
            Vector3 mid = (verts[b0] + verts[b1]) * 0.5f;
            Vector3 outward = new Vector3(mid.x, 0, mid.z);
            Tri(b0, t0, t1, outward);
            Tri(b0, t1, b1, outward);
        }

        AddCap(r, y1, Vector3.up);
        AddCap(r, y0, Vector3.down);
    }

    void AddCap(float r, float y, Vector3 n)
    {
        int c = verts.Count;
        verts.Add(new Vector3(0, y, 0));
        uvs.Add(new Vector2(0.5f, 0.5f));
        for (int i = 0; i <= segments; i++)
        {
            float a = 2f * Mathf.PI * i / segments;
            verts.Add(new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r));
            uvs.Add(new Vector2(0.5f + Mathf.Cos(a) * 0.5f, 0.5f + Mathf.Sin(a) * 0.5f));
        }
        for (int i = 0; i < segments; i++)
            Tri(c, c + 1 + i, c + 2 + i, n);
    }

    void Tri(int a, int b, int c, Vector3 outward)
    {
        Vector3 pa = verts[a], pb = verts[b], pc = verts[c];
        if (Vector3.Dot(Vector3.Cross(pb - pa, pc - pa), outward) < 0)
        {
            int t = b; b = c; c = t;
        }
        tris.Add(a); tris.Add(b); tris.Add(c);
    }
}