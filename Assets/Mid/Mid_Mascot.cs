using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_Mascot : MonoBehaviour
{
    public int lat = 10;
    public int lon = 14;
    public float scale = 1.2f;
    public float bobSpeed = 2f;
    public float bobAmount = 0.03f;
    public float turnSpeed = 3f;
    public float greetDistance = 6f;

    Mesh mesh, armMesh;
    bool dirty = true;
    Transform arm;
    Vector3 basePos;
    bool baseSet;

    List<Vector3> v = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int>[] sub = { new List<int>(), new List<int>(), new List<int>(), new List<int>() };

    void OnEnable() { dirty = true; }
    void OnValidate() { dirty = true; }

    void Update()
    {
        if (dirty) { Build(); dirty = false; }
        if (!Application.isPlaying || arm == null) return;

        if (!baseSet) { basePos = transform.localPosition; baseSet = true; }
        transform.localPosition = basePos + Vector3.up * Mathf.Sin(Time.time * bobSpeed) * bobAmount * scale;

        float dist = 999f;
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 to = cam.transform.position - transform.position;
            to.y = 0;
            dist = to.magnitude;
            if (to.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(to), Time.deltaTime * turnSpeed);
        }

        float wave = dist < greetDistance
            ? 110f + Mathf.Sin(Time.time * 8f) * 30f
            : 20f + Mathf.Sin(Time.time * 1.5f) * 5f;
        arm.localRotation = Quaternion.Slerp(arm.localRotation,
            Quaternion.Euler(0, 0, wave), Time.deltaTime * 8f);
    }

    void Build()
    {
        v.Clear(); uv.Clear();
        foreach (var s in sub) s.Clear();

        Ellipsoid(0, new Vector3(0, 0.35f, 0), new Vector3(0.32f, 0.35f, 0.28f), Vector3.zero);
        Ellipsoid(0, new Vector3(0, 0.82f, 0.02f), new Vector3(0.3f, 0.27f, 0.27f), Vector3.zero);

        Ellipsoid(0, new Vector3(-0.12f, 1.2f, 0f), new Vector3(0.07f, 0.25f, 0.05f), new Vector3(0, 0, 12f));
        Ellipsoid(0, new Vector3(0.12f, 1.2f, 0f), new Vector3(0.07f, 0.25f, 0.05f), new Vector3(0, 0, -12f));
        Ellipsoid(3, new Vector3(-0.17f, 1.4f, 0f), new Vector3(0.05f, 0.08f, 0.04f), new Vector3(0, 0, 12f));
        Ellipsoid(3, new Vector3(0.17f, 1.4f, 0f), new Vector3(0.05f, 0.08f, 0.04f), new Vector3(0, 0, -12f));

        Ellipsoid(1, new Vector3(-0.1f, 0.86f, 0.25f), new Vector3(0.035f, 0.045f, 0.03f), Vector3.zero);
        Ellipsoid(1, new Vector3(0.1f, 0.86f, 0.25f), new Vector3(0.035f, 0.045f, 0.03f), Vector3.zero);
        Ellipsoid(2, new Vector3(-0.17f, 0.78f, 0.22f), new Vector3(0.05f, 0.03f, 0.02f), Vector3.zero);
        Ellipsoid(2, new Vector3(0.17f, 0.78f, 0.22f), new Vector3(0.05f, 0.03f, 0.02f), Vector3.zero);
        Ellipsoid(2, new Vector3(0f, 0.8f, 0.27f), new Vector3(0.025f, 0.02f, 0.02f), Vector3.zero);

        Ellipsoid(0, new Vector3(-0.13f, 0.05f, 0.08f), new Vector3(0.09f, 0.05f, 0.12f), Vector3.zero);
        Ellipsoid(0, new Vector3(0.13f, 0.05f, 0.08f), new Vector3(0.09f, 0.05f, 0.12f), Vector3.zero);
        Ellipsoid(0, new Vector3(0f, 0.25f, -0.28f), new Vector3(0.08f, 0.08f, 0.08f), Vector3.zero);
        Ellipsoid(0, new Vector3(-0.31f, 0.45f, 0.05f), new Vector3(0.07f, 0.13f, 0.07f), new Vector3(0, 0, -30f));

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_Mascot" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.SetVertices(v);
        mesh.SetUVs(0, uv);
        mesh.subMeshCount = 4;
        for (int i = 0; i < 4; i++) mesh.SetTriangles(sub[i], i);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        var mr = GetComponent<MeshRenderer>();
        var mats = mr.sharedMaterials;
        bool missing = mats.Length < 4;
        for (int i = 0; !missing && i < 4; i++) if (mats[i] == null) missing = true;
        if (missing)
        {
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            var arr = new Material[4];
            for (int i = 0; i < 4; i++) arr[i] = (i < mats.Length && mats[i] != null) ? mats[i] : new Material(lit);
            mr.sharedMaterials = arr;
        }

        BuildArm(mr.sharedMaterials[0]);
    }

    void BuildArm(Material fur)
    {
        arm = transform.Find("WaveArm");
        if (arm == null)
        {
            arm = new GameObject("WaveArm").transform;
            arm.SetParent(transform, false);
            arm.gameObject.AddComponent<MeshFilter>();
            arm.gameObject.AddComponent<MeshRenderer>();
        }
        arm.localPosition = new Vector3(0.27f, 0.55f, 0.05f) * scale;
        arm.localRotation = Quaternion.Euler(0, 0, 20f);

        var av = new List<Vector3>();
        var au = new List<Vector2>();
        var at = new List<int>();
        AddEllipsoid(av, au, at, new Vector3(0, -0.12f, 0), new Vector3(0.07f, 0.13f, 0.07f), Vector3.zero);

        if (armMesh == null) armMesh = new Mesh { name = "Mid_MascotArm" };
        armMesh.Clear();
        armMesh.SetVertices(av);
        armMesh.SetUVs(0, au);
        armMesh.SetTriangles(at, 0);
        armMesh.RecalculateNormals();
        armMesh.RecalculateBounds();

        arm.GetComponent<MeshFilter>().sharedMesh = armMesh;
        arm.GetComponent<MeshRenderer>().sharedMaterial = fur;
    }

    void Ellipsoid(int s, Vector3 c, Vector3 r, Vector3 euler)
    {
        AddEllipsoid(v, uv, sub[s], c, r, euler);
    }

    void AddEllipsoid(List<Vector3> vv, List<Vector2> uu, List<int> tt, Vector3 c, Vector3 r, Vector3 euler)
    {
        Quaternion q = Quaternion.Euler(euler);
        Vector3 center = c * scale;
        int start = vv.Count;

        for (int i = 0; i <= lat; i++)
        {
            float phi = Mathf.PI * i / lat;
            for (int j = 0; j <= lon; j++)
            {
                float th = 2f * Mathf.PI * j / lon;
                Vector3 d = new Vector3(Mathf.Sin(phi) * Mathf.Cos(th), Mathf.Cos(phi), Mathf.Sin(phi) * Mathf.Sin(th));
                vv.Add(center + q * Vector3.Scale(d, r) * scale);
                uu.Add(new Vector2((float)j / lon, (float)i / lat));
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
                Tri(vv, tt, a, c2, b, center);
                Tri(vv, tt, b, c2, d, center);
            }
        }
    }

    void Tri(List<Vector3> vv, List<int> tt, int a, int b, int c, Vector3 center)
    {
        Vector3 pa = vv[a], pb = vv[b], pc = vv[c];
        Vector3 outward = (pa + pb + pc) / 3f - center;
        if (Vector3.Dot(Vector3.Cross(pb - pa, pc - pa), outward) < 0)
        {
            int t = b; b = c; c = t;
        }
        tt.Add(a); tt.Add(b); tt.Add(c);
    }
}