using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public static class OversizedMeshColliderFinder
{
    [MenuItem("Tools/Beta/Find Large Mesh Colliders")]
    private static void FindLargeColliders()
    {
        const ulong limit = 2097152;

        int found = 0;

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.isLoaded)

                continue;

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                MeshCollider[] colliders = root.GetComponentsInChildren<MeshCollider>(true);

                foreach (MeshCollider collider in colliders)
                {
                    Mesh mesh = collider.sharedMesh;

                    if (mesh == null)

                        continue;

                    ulong triangles = 0;

                    for (int sub = 0; sub < mesh.subMeshCount; sub++)
                    {
                        if (mesh.GetTopology(sub) == MeshTopology.Triangles)
                        {
                            triangles += mesh.GetIndexCount(sub) / 3;
                        }
                    }

                    if (triangles > limit)
                    {
                        //Debug.LogWarning($"Large Collider: {collider.name} | " + $"Triangles: {triangles}", collider.gameObject);

                        found++;
                    }
                }
            }
        }

        //Debug.Log($"Large Mesh Colliders found: {found}");
    }
}
