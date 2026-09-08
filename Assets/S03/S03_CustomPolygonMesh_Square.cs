  using UnityEngine;

  [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
  public class S03_CustomPolygonMesh_Square : MonoBehaviour
  {
      void Start()
      {
          // TODO 1: 원하는 다각형의 정점 좌표를 채우세요 (최소 4개)
          Vector3[] vertices = new Vector3[]
          {
              // 예: new Vector3(0f, 1f, 0f),
              new Vector3(0f, 0f, 0f),

            new Vector3(0f, 2f, 0f),

            new Vector3(0.47f, 0.65f, 0f),

            new Vector3(1.9f, 0.62f, 0f),

            new Vector3(0.76f, -0.25f, 0f),

            new Vector3(1.18f, -1.62f, 0f),

            new Vector3(0f, -0.8f, 0f),

            new Vector3(-1.18f, -1.62f, 0f),

            new Vector3(-0.76f, -0.25f, 0f),

            new Vector3(-1.9f, 0.62f, 0f),

            new Vector3(-0.47f, 0.65f, 0f),
          };

          // TODO 2: 정점 3개씩 묶어 삼각형들을 구성하세요
          int[] triangles = new int[]
          {
              // 예: 0, 1, 2,
                  0, 1, 2,
                  0, 2, 3,
          };

          Mesh mesh = new Mesh();
          mesh.vertices = vertices;
          mesh.triangles = triangles;
          mesh.RecalculateNormals();

          GetComponent<MeshFilter>().mesh = mesh;
          GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
      }
  }