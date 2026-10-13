using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S12_CustomMesh_Heart : MonoBehaviour
{
    void Start()
    {
        // 중심점(0번) + 하트 테두리를 따라 도는 점 16개(1~16번)
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),        // 0: 중심

            new Vector3(0f, 0.5f, 0f),      // 1: 윗부분 가운데 오목한 곳
            new Vector3(0.09f, 0.77f, 0f),  // 2
            new Vector3(0.57f, 1.16f, 0f),  // 3: 오른쪽 위 봉우리
            new Vector3(1.26f, 1.04f, 0f),  // 4
            new Vector3(1.6f, 0.4f, 0f),    // 5: 오른쪽 끝
            new Vector3(1.26f, -0.33f, 0f), // 6
            new Vector3(0.57f, -0.96f, 0f), // 7
            new Vector3(0.09f, -1.48f, 0f), // 8
            new Vector3(0f, -1.7f, 0f),     // 9: 맨 아래 뾰족한 끝

            new Vector3(-0.09f, -1.48f, 0f),// 10
            new Vector3(-0.57f, -0.96f, 0f),// 11
            new Vector3(-1.26f, -0.33f, 0f),// 12
            new Vector3(-1.6f, 0.4f, 0f),   // 13: 왼쪽 끝
            new Vector3(-1.26f, 1.04f, 0f), // 14
            new Vector3(-0.57f, 1.16f, 0f), // 15: 왼쪽 위 봉우리
            new Vector3(-0.09f, 0.77f, 0f), // 16
        };

        // 중심(0)에서 테두리 점들을 순서대로 이어 부채꼴 모양으로 삼각형을 만듦
        int[] triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
            0, 6, 7,
            0, 7, 8,
            0, 8, 9,
            0, 9, 10,
            0, 10, 11,
            0, 11, 12,
            0, 12, 13,
            0, 13, 14,
            0, 14, 15,
            0, 15, 16,
            0, 16, 1,
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}