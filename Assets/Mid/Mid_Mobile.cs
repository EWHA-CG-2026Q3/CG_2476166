using UnityEngine;

[ExecuteAlways]
public class Mid_Mobile : MonoBehaviour
{
    public float spinSpeed = 20f;
    public float radius = 0.6f;
    public float hangY = -0.35f;
    public float bobAmount = 0.05f;
    public float bobSpeed = 1.5f;
    public float selfSpin = 40f;

    void Update()
    {
        if (Application.isPlaying)
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.Self);

        int n = transform.childCount;
        for (int i = 0; i < n; i++)
        {
            Transform c = transform.GetChild(i);
            float a = 2f * Mathf.PI * i / n;
            float bob = Application.isPlaying ? Mathf.Sin(Time.time * bobSpeed + i * 2f) * bobAmount : 0f;
            c.localPosition = new Vector3(Mathf.Cos(a) * radius, hangY + bob, Mathf.Sin(a) * radius);
            if (Application.isPlaying)
                c.Rotate(0f, selfSpin * Time.deltaTime, 0f, Space.Self);
        }
    }
}