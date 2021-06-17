using UnityEngine;

namespace Terrain {
    public class MapDisplay : MonoBehaviour {
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public Renderer textureRenderer;

        public void DrawTexture(Texture2D texture) {
            textureRenderer.sharedMaterial.mainTexture = texture;
            textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }

        public void DrawMesh(MeshData meshData, Texture2D texture) {
            meshFilter.sharedMesh = meshData.CreateMesh();
            meshRenderer.sharedMaterial.mainTexture = texture;
        }
    }
}