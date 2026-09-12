using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S04_DiamondMesh : MonoBehaviour
{
    void Start()
    {
        // 다이아몬드(정팔면체) 6개 정점
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0.5f, 1f, 0.5f), // 0: 위쪽 꼭짓점
            new Vector3(0.5f, 0f, 0.5f), // 1: 아래쪽 꼭짓점
            new Vector3(0f, 0.5f, 0f),   // 2: 허리 (x0,z0)
            new Vector3(1f, 0.5f, 0f),   // 3: 허리 (x1,z0)
            new Vector3(1f, 0.5f, 1f),   // 4: 허리 (x1,z1)
            new Vector3(0f, 0.5f, 1f),   // 5: 허리 (x0,z1)
        };

        int[] triangles = new int[]
        {
            // 위쪽 4면 (정점 0 사용, 허리띠 역순으로 연결)
            0, 3, 2,
            0, 4, 3,
            0, 5, 4,
            0, 2, 5,
            
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}