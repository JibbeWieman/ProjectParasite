using UnityEngine;

public class CameraViewVisualizer : MonoBehaviour
{
    public Camera cam;
    public float depth = 10f; // Depth of the view area
    public Material effectMaterial; // Optional material for visual effect

    private Mesh frustumMesh;

    void Start()
    {
        frustumMesh = new Mesh();
    }

    void Update()
    {
        Vector3[] corners = new Vector3[4];
        cam.CalculateFrustumCorners(new Rect(0, 0, 1, 1), depth, Camera.MonoOrStereoscopicEye.Mono, corners);

        // Transform corners from local to world space
        Vector3 bottomLeft = cam.transform.TransformPoint(corners[0]);
        Vector3 topLeft = cam.transform.TransformPoint(corners[1]);
        Vector3 topRight = cam.transform.TransformPoint(corners[2]);
        Vector3 bottomRight = cam.transform.TransformPoint(corners[3]);

        // Update Mesh
        frustumMesh.vertices = new Vector3[] { bottomLeft, topLeft, topRight, bottomRight, cam.transform.position };
        frustumMesh.triangles = new int[]
        {
            0, 1, 4, // Bottom-left to top-left to camera position
            1, 2, 4, // Top-left to top-right to camera position
            2, 3, 4, // Top-right to bottom-right to camera position
            3, 0, 4  // Bottom-right to bottom-left to camera position
        };

        frustumMesh.RecalculateNormals();
    }

    void OnRenderObject()
    {
        if (effectMaterial)
        {
            effectMaterial.SetPass(0);
            Graphics.DrawMeshNow(frustumMesh, Matrix4x4.identity);
        }
    }

    void OnDrawGizmos()
    {
        if (cam == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.matrix = Matrix4x4.TRS(cam.transform.position, cam.transform.rotation, Vector3.one);

        Gizmos.DrawFrustum(Vector3.zero, cam.fieldOfView, cam.farClipPlane, cam.nearClipPlane, cam.aspect);
    }

}
