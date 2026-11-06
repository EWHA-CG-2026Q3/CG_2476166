using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_StairLanterns : MonoBehaviour
{
    public Mid_SpiralStairs stairs;
    public int every = 6;
    public float inset = 0.15f;
    public float postHeight = 0.9f;
    public float postWidth = 0.06f;
    public float lampSize = 0.18f;
    public bool addLights = true;
    public Color lightColor = new Color(1f, 0.75f, 0.45f);
    public float lightIntensity = 1.5f;
    public float lightRange = 2.5f;

    Mesh mesh;
    bool dirty = true;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> postTris = new List<int>();
    List<int> lampTris = new List<int>();
    List<Vector3> lampPositions = new List<Vector3>();

    void OnEnable() { dirty = true; }
    void OnValidate() { dirty = true; }

    void Update()
    {
        if (stairs == null) stairs = GetComponentInParent<Mid_SpiralStairs>();
        if (dirty && stairs != null)
        {
            Build();
            BuildLights();
            dirty = false;
        }
    }

    void Build()
    {
        verts.Clear(); uvs.Clear(); postTris.Clear(); lampTris.Clear(); lampPositions.Clear();

        int n = stairs.stepCount;
        for (int i = 0; i < n; i++)
        {
            if ((i + 1) % every != 0) continue;

            float centerDeg = stairs.endAngle - (n - 1 - i) * stairs.anglePerStep;
            float a = centerDeg * Mathf.Deg2Rad;
            float top = (i + 1) * stairs.stepHeight;
            float r = stairs.outerRadius - inset;

            Vector3 basePos = new Vector3(Mathf.Cos(a) * r, top, Mathf.Sin(a) * r);
            Quaternion rot = Quaternion.Euler(0, -centerDeg, 0);

            AddBox(postTris, basePos + Vector3.up * postHeight * 0.5f,
                   new Vector3(postWidth, postHeight, postWidth), rot);

            Vector3 lampCenter = basePos + Vector3.up * (postHeight + lampSize * 0.5f);
            AddBox(lampTris, lampCenter, new Vector3(lampSize, lampSize, lampSize), rot);

            AddBox(postTris, basePos + Vector3.up * (postHeight + lampSize + 0.02f),
                   new Vector3(lampSize * 1.4f, 0.04f, lampSize * 1.4f), rot);

            lampPositions.Add(lampCenter);
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_StairLanterns" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.subMeshCount = 2;
        mesh.SetTriangles(postTris, 0);
        mesh.SetTriangles(lampTris, 1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mr = GetComponent<MeshRenderer>();
        var mats = mr.sharedMaterials;
        if (mats.Length < 2 || mats[0] == null || mats[1] == null)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            mr.sharedMaterials = new Material[] { new Material(lit), new Material(lit) };
        }
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
            var go = new GameObject("LanternLight");
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

    void AddBox(List<int> target, Vector3 c, Vector3 size, Quaternion rot)
    {
        Vector3 h = size * 0.5f;
        Vector3 P(float x, float y, float z) => c + rot * new Vector3(x * h.x, y * h.y, z * h.z);

        Vector3 px = rot * Vector3.right, py = Vector3.up, pz = rot * Vector3.forward;

        AddQuad(target, P(-1, 1, -1), P(1, 1, -1), P(1, 1, 1), P(-1, 1, 1), py);
        AddQuad(target, P(-1, -1, -1), P(1, -1, -1), P(1, -1, 1), P(-1, -1, 1), -py);
        AddQuad(target, P(1, -1, -1), P(1, -1, 1), P(1, 1, 1), P(1, 1, -1), px);
        AddQuad(target, P(-1, -1, -1), P(-1, -1, 1), P(-1, 1, 1), P(-1, 1, -1), -px);
        AddQuad(target, P(-1, -1, 1), P(1, -1, 1), P(1, 1, 1), P(-1, 1, 1), pz);
        AddQuad(target, P(-1, -1, -1), P(1, -1, -1), P(1, 1, -1), P(-1, 1, -1), -pz);
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