using UnityEngine;

public class Mid_Constellation : MonoBehaviour
{
    public Material glMaterial;
    public Transform anchor;
    public float distance = 45f;
    public float size = 2.2f;
    public Vector2 dipperOffset = new Vector2(-10f, 6f);
    public bool drawRabbit = true;
    public Vector2 rabbitOffset = new Vector2(9f, 5f);
    public Color lineColor = new Color(0.6f, 0.75f, 1f, 1f);
    public Color starColor = new Color(1f, 0.95f, 0.8f, 1f);
    public float starSize = 0.25f;
    public float twinkleSpeed = 3f;

    static readonly Vector2[] dipper =
    {
        new Vector2(0f, 0f), new Vector2(1f, 0.1f), new Vector2(2f, 0f), new Vector2(3f, -0.2f),
        new Vector2(3.2f, -1.1f), new Vector2(4.3f, -1.3f), new Vector2(4.5f, -0.3f)
    };
    static readonly int[] dipperLines = { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 3 };

    static readonly Vector2[] rabbit =
    {
        new Vector2(0f, 0f), new Vector2(-0.8f, 0.6f), new Vector2(-0.5f, 1.4f), new Vector2(-0.9f, 3f),
        new Vector2(-0.1f, 1.6f), new Vector2(0.4f, 3.1f), new Vector2(0.5f, 1.4f), new Vector2(0.9f, 0.6f)
    };
    static readonly int[] rabbitLines = { 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 0 };

    void OnRenderObject()
    {
        if (glMaterial == null || anchor == null) return;

        glMaterial.SetPass(0);

        Vector3 center = anchor.position + anchor.forward * distance;
        Vector3 right = anchor.right, up = anchor.up;
        float tw = 0.75f + 0.25f * Mathf.Sin(Time.time * twinkleSpeed);

        DrawPattern(dipper, dipperLines, center + right * dipperOffset.x + up * dipperOffset.y, right, up, tw);
        if (drawRabbit)
            DrawPattern(rabbit, rabbitLines, center + right * rabbitOffset.x + up * rabbitOffset.y, right, up, tw);
    }

    Vector3 P(Vector2 v, Vector3 origin, Vector3 right, Vector3 up)
    {
        return origin + (right * v.x + up * v.y) * size;
    }

    void DrawPattern(Vector2[] pts, int[] lines, Vector3 origin, Vector3 right, Vector3 up, float tw)
    {
        GL.Begin(GL.LINES);
        GL.Color(lineColor * tw);
        for (int i = 0; i < lines.Length; i += 2)
        {
            GL.Vertex(P(pts[lines[i]], origin, right, up));
            GL.Vertex(P(pts[lines[i + 1]], origin, right, up));
        }
        GL.End();

        GL.Begin(GL.TRIANGLES);
        GL.Color(starColor * (0.6f + 0.4f * tw));
        foreach (Vector2 pt in pts)
        {
            Vector3 c = P(pt, origin, right, up);
            Vector3 u = up * starSize, r = right * starSize;
            GL.Vertex(c + u); GL.Vertex(c + r); GL.Vertex(c - u);
            GL.Vertex(c + u); GL.Vertex(c - u); GL.Vertex(c - r);
        }
        GL.End();
    }
}