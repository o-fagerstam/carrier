using System;
using System.Collections.Generic;
using UnityEngine;

namespace Terrain {
    public class EndlessTerrain : MonoBehaviour {
        public const float MaxViewDist = 300f;
        public Transform viewer;

        public static Vector2 viewerPosition;
        private int chunkSize;
        private int chunkVisibleInViewDst;

        Dictionary<Vector2, TerrainChunk> _terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
        List<TerrainChunk> _terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

        private static MapGenerator _mapGenerator;
        public Material mapMaterial;
        
        private void Start() {
            _mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunkVisibleInViewDst = Mathf.RoundToInt(MaxViewDist / chunkSize);
        }

        private void Update() {
            var newViewerPosition = viewer.position;
            viewerPosition = new Vector2(newViewerPosition.x, newViewerPosition.z);
            UpdateVisibleChunks();
        }

        void UpdateVisibleChunks() {

            foreach (TerrainChunk chunk in _terrainChunksVisibleLastUpdate) {
                chunk.SetVisible(false);
            }
            _terrainChunksVisibleLastUpdate.Clear();
            
            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);
            
            for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++) {
                for (int xOffset = -chunkVisibleInViewDst; xOffset <= chunkVisibleInViewDst; xOffset++) {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                    if (_terrainChunkDict.ContainsKey(viewedChunkCoord)) {
                        _terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                        if (_terrainChunkDict[viewedChunkCoord].isVisible()) {
                            _terrainChunksVisibleLastUpdate.Add(_terrainChunkDict[viewedChunkCoord]);
                        }
                    }
                    else {
                        _terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, mapMaterial));
                    }
                }
            }
        }

        public class TerrainChunk {
            private GameObject meshObject;
            private Vector2 position;
            private Bounds _bounds;

            private MeshRenderer _meshRenderer;
            private MeshFilter _meshFilter;
            public TerrainChunk(Vector2 coord, int size, Material material) {
                position = coord * size;
                _bounds = new Bounds(position, Vector2.one * size);
                Vector3 positionV3 = new Vector3(position.x, 0f, position.y);

                meshObject = new GameObject("Terrain Chunk");
                _meshRenderer = meshObject.AddComponent<MeshRenderer>();
                _meshFilter = meshObject.AddComponent<MeshFilter>();
                _meshRenderer.material = material;
                meshObject.transform.position = positionV3;
                SetVisible(false);
                
                _mapGenerator.RequestMapData(OnMapDataReceived);
            }

            void OnMapDataReceived(MapData mapData) {
                _mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            }

            void OnMeshDataReceived(MeshData meshData) {
                _meshFilter.mesh = meshData.CreateMesh();
            }

            public void UpdateTerrainChunk() {
                float viewerDstFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= MaxViewDist;
                SetVisible(visible);
            }

            public void SetVisible(bool visible) {
                meshObject.SetActive(visible);
            }

            public bool isVisible() {
                return meshObject.activeSelf;
            }
        }
    }
}