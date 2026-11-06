using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_StoneLanterns : MonoBehaviour
{
    public Vector3 pathStart = new Vector3(7f, 0f, 0f);
    public Vector3 pathEnd = new Vector3(3.2f, 0f, 0f);
    public int pairs = 3;
    public float sideOffset = 1.2f;
    public float scale = 1f;
    public bool addLights = true;
    public Color lightColor = new Color(1f, 0.75f, 0.45f);
    public float lightIntensity = 2f;
    public float lightRange = 3f;

    Mesh mesh;
    bool dirty = true;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> stoneTris = new List<int>();
    List<int> glowTris = new List<int>();
    List<Vector3> lampPositions = new List<Vector3>();

    void OnEnable() { dirty = true; }
    void OnValidate() { dirty = true; }

    void Update()
    {
        if (dirty) { Build(); BuildLights(); dirty = false; }
    }

    void Build()
    {
        verts.Clear(); uvs.Clear(); stoneTris.Clear(); glowTris.Clear(); lampPositions.Clear();

        Vector3 dir = pathEnd - pathStart;
        Vector3 side = Vector3.Cross(Vector3.up, dir.normalized);

        for (int i = 0; i < pairs; i++)
        {
            float t = pairs == 1 ? 0.5f : (float)i / (pairs - 1);
            Vector3 p = pathStart + dir * t;
            AddLantern(p + side * sideOffset);
            AddLantern(p - side * sideOffset);
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_StoneLanterns" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(stoneTris, 0);
        mesh.SetTriangles(glowTris, 1);
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

    void AddLantern(Vector3 p)
    {
        float s = scale;
        AddBox(stoneTris, p + Vector3.up * 0.06f * s, new Vector3(0.4f, 0.12f, 0.4f) * s);
        AddBox(stoneTris, p + Vector3.up * 0.42f * s, new Vector3(0.14f, 0.6f, 0.14f) * s);
        AddBox(stoneTris, p + Vector3.up * 0.75f * s, new Vector3(0.32f, 0.06f, 0.32f) * s);

        Vector3 lamp = p + Vector3.up * 0.9f * s;
        AddBox(glowTris, lamp, new Vector3(0.22f, 0.24f, 0.22f) * s);
        lampPositions.Add(lamp);

        Vector3 roofBase = p + Vector3.up * 1.02f * s;
        float r = 0.24f * s;
        Vector3 apex = roofBase + Vector3.up * 0.22f * s;
        Vector3 a = roofBase + new Vector3(-r, 0, -r), b = roofBase + new Vector3(r, 0, -r);
        Vector3 c = roofBase + new Vector3(r, 0, r), d = roofBase + new Vector3(-r, 0, r);
        Vector3 mid = roofBase + Vector3.up * 0.08f * s;
        AddTri(stoneTris, a, b, apex, mid);
        AddTri(stoneTris, b, c, apex, mid);
        AddTri(stoneTris, c, d, apex, mid);
        AddTri(stoneTris, d, a, apex, mid);
        AddTri(stoneTris, a, b, c, apex);
        AddTri(stoneTris, a, c, d, apex);
    }

    void BuildLights()
    {
        Transform holder = transform.Find("Lights");
        if (holder != null) DestroyImmediate(holder.gameObject);
        if (!addLights) return;

        holder = new GameObject("Lights").transform;
        holder.SetParent(transform, false);
        foreach (Vector3 p in lampPositions)
        {
            var go = new GameObject("StoneLanternLight");
            go.transform.SetParent(holder, false);
            go.transform.localPosition = p;
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = lightColor;
            l.intensity = lightIntensity;
            l.range = lightRange;
            l.shadows = LightShadows.None;
        }
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

    void AddTri(List<int> target, Vector3 a, Vector3 b, Vector3 c, Vector3 inside)
    {
        Vector3 outward = (a + b + c) / 3f - inside;
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), outward) < 0)
        {
            Vector3 t = b; b = c; c = t;
        }
        int s = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c);
        uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0)); uvs.Add(new Vector2(0.5f, 1));
        target.Add(s); target.Add(s + 1); target.Add(s + 2);
    }
}