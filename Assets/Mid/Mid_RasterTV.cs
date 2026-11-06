using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_RasterTV : MonoBehaviour
{
    public int canvasWidth = 192;
    public int canvasHeight = 144;
    public Color backgroundColor = new Color(0.08f, 0.09f, 0.24f);
    public Color furColor = new Color(1f, 0.96f, 0.9f);
    public Color pinkColor = new Color(1f, 0.62f, 0.71f);
    public Color darkColor = new Color(0.1f, 0.1f, 0.1f);
    public Color starColor = new Color(1f, 0.95f, 0.7f);
    public int starCount = 18;
    public int ellipseSegments = 24;
    public int seed = 4;
    public float blinkInterval = 3f;
    public float blinkDuration = 0.15f;
    public bool addScreenLight = true;

    Texture2D canvas;
    Mesh mesh, screenMesh;
    Transform screen;
    MaterialPropertyBlock mpb;
    bool dirty = true;
    bool eyesClosed;
    float nextBlink;

    List<Vector3> verts = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> tris = new List<int>();

    void OnEnable() { dirty = true; }
    void OnValidate() { dirty = true; }

    void Update()
    {
        if (dirty) { BuildTV(); Paint(false); dirty = false; nextBlink = Time.time + blinkInterval; }
        if (!Application.isPlaying) return;

        if (Time.time >= nextBlink)
        {
            eyesClosed = !eyesClosed;
            Paint(eyesClosed);
            nextBlink = Time.time + (eyesClosed ? blinkDuration : blinkInterval);
        }
    }

    Vector2 S(float x, float y) { return new Vector2(x * canvasWidth / 192f, y * canvasHeight / 144f); }

    void Paint(bool blink)
    {
        if (canvas == null || canvas.width != canvasWidth || canvas.height != canvasHeight)
        {
            canvas = new Texture2D(canvasWidth, canvasHeight);
            canvas.filterMode = FilterMode.Point;
            canvas.wrapMode = TextureWrapMode.Clamp;
        }

        float k = canvasWidth / 192f;
        FillBackground(backgroundColor);

        var rnd = new System.Random(seed);
        for (int i = 0; i < starCount; i++)
        {
            float x = (float)rnd.NextDouble() * 192f, y = (float)rnd.NextDouble() * 144f;
            float r = 2.5f + (float)rnd.NextDouble() * 2f;
            if (Vector2.Distance(new Vector2(x, y), new Vector2(96, 75)) < 62) continue;
            DrawStar(S(x, y), r * k, starColor);
        }

        DrawEllipse(S(74, 108), 12 * k, 32 * k, 12, furColor);
        DrawEllipse(S(118, 108), 12 * k, 32 * k, -12, furColor);
        DrawEllipse(S(74, 110), 6 * k, 22 * k, 12, pinkColor);
        DrawEllipse(S(118, 110), 6 * k, 22 * k, -12, pinkColor);

        DrawEllipse(S(96, 62), 48 * k, 40 * k, 0, furColor);

        if (blink)
        {
            DrawEllipse(S(80, 66), 7 * k, 1.5f * k, 0, darkColor);
            DrawEllipse(S(112, 66), 7 * k, 1.5f * k, 0, darkColor);
        }
        else
        {
            DrawEllipse(S(80, 66), 6 * k, 7 * k, 0, darkColor);
            DrawEllipse(S(112, 66), 6 * k, 7 * k, 0, darkColor);
            DrawEllipse(S(82, 69), 2 * k, 2 * k, 0, Color.white);
            DrawEllipse(S(114, 69), 2 * k, 2 * k, 0, Color.white);
        }

        DrawEllipse(S(66, 52), 8 * k, 5 * k, 0, pinkColor);
        DrawEllipse(S(126, 52), 8 * k, 5 * k, 0, pinkColor);

        DrawTriangle(S(92, 58), S(100, 58), S(96, 53), pinkColor);
        DrawTriangle(S(89, 49), S(93, 45), S(96, 49), darkColor);
        DrawTriangle(S(96, 49), S(99, 45), S(103, 49), darkColor);

        canvas.Apply();

        if (screen != null)
        {
            if (mpb == null) mpb = new MaterialPropertyBlock();
            var sr = screen.GetComponent<MeshRenderer>();
            sr.GetPropertyBlock(mpb);
            mpb.SetTexture("_BaseMap", canvas);
            mpb.SetTexture("_MainTex", canvas);
            sr.SetPropertyBlock(mpb);
        }
    }

    void FillBackground(Color color)
    {
        Color[] fill = new Color[canvasWidth * canvasHeight];
        for (int i = 0; i < fill.Length; i++) fill[i] = color;
        canvas.SetPixels(fill);
    }

    bool IsInsideTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float denom = a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y);
        if (Mathf.Abs(denom) < 1e-6f) return false;

        float w1 = (p.x * (b.y - c.y) + b.x * (c.y - p.y) + c.x * (p.y - b.y)) / denom;
        float w2 = (a.x * (p.y - c.y) + p.x * (c.y - a.y) + c.x * (a.y - p.y)) / denom;
        float w3 = 1f - w1 - w2;

        return w1 >= 0f && w2 >= 0f && w3 >= 0f;
    }

    void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Color color)
    {
        int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
        int maxX = Mathf.Min(canvasWidth - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
        int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
        int maxY = Mathf.Min(canvasHeight - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2 pixelCenter = new Vector2(x + 0.5f, y + 0.5f);
                if (IsInsideTriangle(pixelCenter, a, b, c))
                    canvas.SetPixel(x, y, color);
            }
        }
    }

    void DrawEllipse(Vector2 c, float rx, float ry, float angleDeg, Color color)
    {
        float ang = angleDeg * Mathf.Deg2Rad;
        float ca = Mathf.Cos(ang), sa = Mathf.Sin(ang);
        Vector2 prev = Vector2.zero;
        for (int i = 0; i <= ellipseSegments; i++)
        {
            float t = 2f * Mathf.PI * i / ellipseSegments;
            float lx = Mathf.Cos(t) * rx, ly = Mathf.Sin(t) * ry;
            Vector2 p = c + new Vector2(lx * ca - ly * sa, lx * sa + ly * ca);
            if (i > 0) DrawTriangle(c, prev, p, color);
            prev = p;
        }
    }

    void DrawStar(Vector2 c, float r, Color color)
    {
        float w = r * 0.3f;
        DrawTriangle(c + new Vector2(0, r), c + new Vector2(-w, 0), c + new Vector2(w, 0), color);
        DrawTriangle(c + new Vector2(0, -r), c + new Vector2(-w, 0), c + new Vector2(w, 0), color);
        DrawTriangle(c + new Vector2(r, 0), c + new Vector2(0, w), c + new Vector2(0, -w), color);
        DrawTriangle(c + new Vector2(-r, 0), c + new Vector2(0, w), c + new Vector2(0, -w), color);
    }

    void BuildTV()
    {
        verts.Clear(); uvs.Clear(); tris.Clear();

        AddBox(new Vector3(0, 0.45f, 0), new Vector3(0.9f, 0.7f, 0.55f), Quaternion.identity);

        for (int sx = -1; sx <= 1; sx += 2)
            for (int sz = -1; sz <= 1; sz += 2)
                AddBox(new Vector3(0.35f * sx, 0.05f, 0.2f * sz), new Vector3(0.05f, 0.1f, 0.05f), Quaternion.identity);

        for (int s = -1; s <= 1; s += 2)
        {
            Quaternion rot = Quaternion.Euler(0, 0, 25f * s);
            AddBox(new Vector3(0, 0.8f, 0) + rot * new Vector3(0, 0.2f, 0), new Vector3(0.02f, 0.4f, 0.02f), rot);
            AddBox(new Vector3(0, 0.8f, 0) + rot * new Vector3(0, 0.4f, 0), new Vector3(0.05f, 0.05f, 0.05f), rot);
        }

        if (mesh == null)
        {
            mesh = new Mesh { name = "Mid_RasterTV" };
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

        BuildScreen();
        BuildLight();
    }

    void BuildScreen()
    {
        screen = transform.Find("Screen");
        if (screen == null)
        {
            screen = new GameObject("Screen").transform;
            screen.SetParent(transform, false);
            screen.gameObject.AddComponent<MeshFilter>();
            screen.gameObject.AddComponent<MeshRenderer>();
        }
        screen.localPosition = new Vector3(0, 0.45f, 0.276f);
        screen.localRotation = Quaternion.identity;

        float w = 0.72f, h = 0.54f;
        Vector3 a = new Vector3(-w / 2, -h / 2, 0), b = new Vector3(w / 2, -h / 2, 0);
        Vector3 c = new Vector3(w / 2, h / 2, 0), d = new Vector3(-w / 2, h / 2, 0);

        var sv = new List<Vector3> { a, b, c, d };
        var su = new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        var st = new List<int>();
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), Vector3.forward) >= 0)
        { st.Add(0); st.Add(1); st.Add(2); st.Add(0); st.Add(2); st.Add(3); }
        else
        { st.Add(0); st.Add(2); st.Add(1); st.Add(0); st.Add(3); st.Add(2); }

        if (screenMesh == null) screenMesh = new Mesh { name = "Mid_RasterTV_Screen" };
        screenMesh.Clear();
        screenMesh.SetVertices(sv);
        screenMesh.SetUVs(0, su);
        screenMesh.SetTriangles(st, 0);
        screenMesh.RecalculateNormals();
        screenMesh.RecalculateBounds();
        screen.GetComponent<MeshFilter>().sharedMesh = screenMesh;

        var sr = screen.GetComponent<MeshRenderer>();
        if (sr.sharedMaterial == null)
            sr.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
    }

    void BuildLight()
    {
        Transform l = transform.Find("ScreenLight");
        if (!addScreenLight)
        {
            if (l != null) l.gameObject.SetActive(false);
            return;
        }
        if (l == null)
        {
            l = new GameObject("ScreenLight").transform;
            l.SetParent(transform, false);
            var light = l.gameObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.7f, 0.75f, 1f);
            light.intensity = 0.8f;
            light.range = 2.5f;
            light.shadows = LightShadows.None;
        }
        l.gameObject.SetActive(true);
        l.localPosition = new Vector3(0, 0.45f, 0.8f);
    }

    void AddBox(Vector3 c, Vector3 size, Quaternion rot)
    {
        Vector3 h = size * 0.5f;
        Vector3 P(float x, float y, float z) => c + rot * new Vector3(x * h.x, y * h.y, z * h.z);
        Vector3 px = rot * Vector3.right, py = rot * Vector3.up, pz = rot * Vector3.forward;
        AddQuad(P(-1, 1, -1), P(1, 1, -1), P(1, 1, 1), P(-1, 1, 1), py);
        AddQuad(P(-1, -1, -1), P(1, -1, -1), P(1, -1, 1), P(-1, -1, 1), -py);
        AddQuad(P(1, -1, -1), P(1, -1, 1), P(1, 1, 1), P(1, 1, -1), px);
        AddQuad(P(-1, -1, -1), P(-1, -1, 1), P(-1, 1, 1), P(-1, 1, -1), -px);
        AddQuad(P(-1, -1, 1), P(1, -1, 1), P(1, 1, 1), P(-1, 1, 1), pz);
        AddQuad(P(-1, -1, -1), P(1, -1, -1), P(1, 1, -1), P(-1, 1, -1), -pz);
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