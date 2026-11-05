using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_SpiralStairs : MonoBehaviour
{
    [Header("계단 모양")]
    public int stepCount = 72;          // 계단 칸 수 (24칸 = 한 바퀴)
    public float anglePerStep = 15f;    // 한 칸마다 도는 각도
    public float stepHeight = 0.2f;     // 한 칸 높이 (Step Offset 0.3보다 작게)
    public float innerRadius = 1.0f;    // 안쪽 반지름
    public float outerRadius = 2.2f;    // 바깥쪽 반지름
    public float thickness = 0.12f;     // 발판 두께
    public float endAngle = 0f;         // 마지막 칸의 각도

    [Header("떨어짐 방지 투명 벽")]
    public bool addWalls = true;        // 투명 벽 만들기
    public float wallHeight = 1.5f;     // 벽 높이 (발판 위로)
    public bool showWalls = false;      // 확인용: 체크하면 벽이 보임

    Mesh mesh, wallMesh;
    bool dirty = true;

    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    void OnEnable() { dirty = true; }
    void OnValidate() { dirty = true; }

    void Update()
    {
        if (dirty) { Build(); BuildWalls(); dirty = false; }
    }

    // 칸 i의 시작 각도, 끝 각도, 윗면 높이
    void StepInfo(int i, out float a0, out float a1, out float top)
    {
        float centerDeg = endAngle - (stepCount - 1 - i) * anglePerStep;
        a0 = (centerDeg - anglePerStep * 0.5f) * Mathf.Deg2Rad;
        a1 = (centerDeg + anglePerStep * 0.5f) * Mathf.Deg2Rad;
        top = (i + 1) * stepHeight;
    }

    void Build()
    {
        verts.Clear(); tris.Clear(); uvs.Clear();

        // 반복문: 칸마다 각도와 높이를 바꿔 발판 하나씩
        for (int i = 0; i < stepCount; i++)
        {
            StepInfo(i, out float a0, out float a1, out float top);
            AddStep(a0, a1, top - thickness, top);
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_SpiralStairs" };
            GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
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

    // 계단 안쪽, 바깥쪽 가장자리를 따라 세우는 투명 벽 (자식 오브젝트 "Walls")
    void BuildWalls()
    {
        Transform child = transform.Find("Walls");
        if (!addWalls)
        {
            if (child != null) child.gameObject.SetActive(false);
            return;
        }
        if (child == null)
        {
            child = new GameObject("Walls").transform;
            child.SetParent(transform, false);
        }
        child.gameObject.SetActive(true);

        var wv = new List<Vector3>();
        var wt = new List<int>();

        // 반복문: 칸마다 안쪽 벽 조각 + 바깥쪽 벽 조각
        for (int i = 0; i < stepCount; i++)
        {
            StepInfo(i, out float a0, out float a1, out float top);
            AddWall(wv, wt, innerRadius, a0, a1, top, top + wallHeight);
            AddWall(wv, wt, outerRadius, a0, a1, top, top + wallHeight);
        }

        if (wallMesh == null) wallMesh = new Mesh { name = "Mid_SpiralStairs_Walls" };
        wallMesh.Clear();
        wallMesh.SetVertices(wv);
        wallMesh.SetTriangles(wt, 0);
        wallMesh.RecalculateNormals();
        wallMesh.RecalculateBounds();

        var wc = child.GetComponent<MeshCollider>();
        if (wc == null) wc = child.gameObject.AddComponent<MeshCollider>();
        wc.sharedMesh = null;
        wc.sharedMesh = wallMesh;

        // 확인용 표시 (평소엔 꺼둠 → 안 보이는 벽)
        var wf = child.GetComponent<MeshFilter>();
        var wr = child.GetComponent<MeshRenderer>();
        if (showWalls)
        {
            if (wf == null) wf = child.gameObject.AddComponent<MeshFilter>();
            if (wr == null) wr = child.gameObject.AddComponent<MeshRenderer>();
            wf.sharedMesh = wallMesh;
            if (wr.sharedMaterial == null)
                wr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            wr.enabled = true;
        }
        else if (wr != null)
        {
            wr.enabled = false;
        }
    }

    // 세로 벽 조각 하나 (양면: 안에서든 밖에서든 막힘)
    void AddWall(List<Vector3> wv, List<int> wt, float r, float a0, float a1, float y0, float y1)
    {
        int s = wv.Count;
        wv.Add(P(r, a0, y0)); wv.Add(P(r, a1, y0));
        wv.Add(P(r, a1, y1)); wv.Add(P(r, a0, y1));
        wt.Add(s); wt.Add(s + 1); wt.Add(s + 2);   // 한쪽 면
        wt.Add(s); wt.Add(s + 2); wt.Add(s + 3);
        wt.Add(s); wt.Add(s + 2); wt.Add(s + 1);   // 반대쪽 면
        wt.Add(s); wt.Add(s + 3); wt.Add(s + 2);
    }

    Vector3 P(float r, float a, float y)
    {
        return new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r);
    }

    void AddStep(float a0, float a1, float y0, float y1)
    {
        float r0 = innerRadius, r1 = outerRadius;
        Vector3 i0b = P(r0, a0, y0), i1b = P(r0, a1, y0), o0b = P(r1, a0, y0), o1b = P(r1, a1, y0);
        Vector3 i0t = P(r0, a0, y1), i1t = P(r0, a1, y1), o0t = P(r1, a0, y1), o1t = P(r1, a1, y1);

        float am = (a0 + a1) * 0.5f;
        Vector3 radial = new Vector3(Mathf.Cos(am), 0, Mathf.Sin(am));
        Vector3 tan0 = new Vector3(-Mathf.Sin(a0), 0, Mathf.Cos(a0));
        Vector3 tan1 = new Vector3(-Mathf.Sin(a1), 0, Mathf.Cos(a1));

        AddQuad(i0t, o0t, o1t, i1t, Vector3.up);
        AddQuad(i0b, o0b, o1b, i1b, Vector3.down);
        AddQuad(o0b, o1b, o1t, o0t, radial);
        AddQuad(i0b, i1b, i1t, i0t, -radial);
        AddQuad(i0b, o0b, o0t, i0t, -tan0);
        AddQuad(i1b, o1b, o1t, i1t, tan1);
    }

    void AddQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 outward)
    {
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), outward) < 0)
        {
            Vector3 t = b; b = d; d = t;
        }
        int s = verts.Count;
        verts.Add(a); verts.Add(b); verts.Add(c); verts.Add(d);
        uvs.Add(new Vector2(0, 0)); uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1)); uvs.Add(new Vector2(0, 1));
        tris.Add(s); tris.Add(s + 1); tris.Add(s + 2);
        tris.Add(s); tris.Add(s + 2); tris.Add(s + 3);
    }
}