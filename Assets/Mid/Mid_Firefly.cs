using UnityEngine;

public class Mid_Firefly : MonoBehaviour
{
    public Mid_SpiralStairs stairs;
    [Range(0f, 1f)] public float phase = 0f;
    public float climbTime = 20f;
    public float floatHeight = 1.0f;
    public float wobble = 0.25f;
    public float size = 0.08f;

    float t;

    void Start() { t = phase; }

    void Update()
    {
        if (stairs == null) return;

        t += Time.deltaTime / climbTime;
        if (t > 1f) t -= 1f;

        int n = stairs.stepCount;
        float startDeg = stairs.endAngle - (n - 1) * stairs.anglePerStep;
        float angle = Mathf.Lerp(startDeg, stairs.endAngle, t) * Mathf.Deg2Rad;
        float h = t * n * stairs.stepHeight + floatHeight
                  + Mathf.Sin(Time.time * 2.3f + phase * 10f) * wobble;
        float r = (stairs.innerRadius + stairs.outerRadius) * 0.5f
                  + Mathf.Sin(Time.time * 1.7f + phase * 20f) * wobble;

        Matrix4x4 T_r = Translate(r, 0, 0);
        Matrix4x4 R = RotateY(angle);
        Matrix4x4 T_h = Translate(0, h, 0);

        Matrix4x4 local = T_h * R * T_r;
        Matrix4x4 world = stairs.transform.localToWorldMatrix * local;

        transform.position = world.MultiplyPoint3x4(Vector3.zero);

        float fade = Mathf.Clamp01(Mathf.Min(t, 1f - t) * 10f);
        transform.localScale = Vector3.one * size * fade;
    }

    Matrix4x4 Translate(float x, float y, float z)
    {
        Matrix4x4 m = Matrix4x4.identity;
        m.m03 = x; m.m13 = y; m.m23 = z;
        return m;
    }

    Matrix4x4 RotateY(float a)
    {
        float c = Mathf.Cos(a), s = Mathf.Sin(a);
        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = c; m.m02 = -s;
        m.m20 = s; m.m22 = c;
        return m;
    }
}