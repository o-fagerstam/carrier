using System;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour {

    
    private const float viewerMoveThresholdForChunkUpdate = 25f;

    private const float sqrViewerMoveThreshHoldForChunkUpdate =
        viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    
    public LODInfo[] detailLevels;
    public static float maxViewDst;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    private Vector2 viewerPositionLastUpdate;
    private static MapGenerator _mapGenerator;
    private int chunkSize;
    private int chunksVisibleInViewDst;

    private static int _collisionMinimumLOD;
    public int collisionMinimumLOD;

    private readonly Dictionary<Vector2, TerrainChunk>
        _terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    private static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start() {
        _mapGenerator = FindObjectOfType<MapGenerator>();

        _collisionMinimumLOD = collisionMinimumLOD;

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = _mapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
        
        UpdateVisibleChunks();
        viewerPositionLastUpdate = viewerPosition;
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / _mapGenerator.terrainData.uniformScale;
        
        if ((viewerPositionLastUpdate - viewerPosition).sqrMagnitude > sqrViewerMoveThreshHoldForChunkUpdate) {
            viewerPositionLastUpdate = viewerPosition;
            UpdateVisibleChunks();
        }
        
    }

    private void UpdateVisibleChunks() {
        foreach (TerrainChunk chunk in terrainChunksVisibleLastUpdate) {
            chunk.SetVisible(false);
        }

        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (_terrainChunkDictionary.ContainsKey(viewedChunkCoord)) {
                    _terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else {
                    _terrainChunkDictionary.Add(
                        viewedChunkCoord,
                        new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial)
                    );
                }
            }
        }
    }

    public class TerrainChunk {
        private readonly GameObject meshObject;
        private readonly Vector2 position;
        private Bounds bounds;

        private readonly MeshRenderer _meshRenderer;
        private readonly MeshFilter _meshFilter;
        private readonly MeshCollider _meshCollider;

        private LODInfo[] _detailLevels;
        private LODMesh[] _lodMeshes;
        private LODMesh _collisionLODMesh;

        private MapData _mapData;
        private bool mapDataReceived;

        private int previousLODIndex = -1;
        
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
            _detailLevels = detailLevels;
            
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0f, position.y);

            meshObject = new GameObject("Terrain Chunk");
            _meshRenderer = meshObject.AddComponent<MeshRenderer>();
            _meshFilter = meshObject.AddComponent<MeshFilter>();
            _meshCollider = meshObject.AddComponent<MeshCollider>();
            _meshRenderer.material = material;

            meshObject.transform.position = positionV3 * _mapGenerator.terrainData.uniformScale;
            meshObject.transform.localScale = Vector3.one * _mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            SetVisible(false);

            _lodMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) {
                _lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider) {
                    _collisionLODMesh = _lodMeshes[i];
                }
            }
            
            _mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData mapData) {
            _mapData = mapData;
            mapDataReceived = true;

            UpdateTerrainChunk();
        }

        public void UpdateTerrainChunk() {
            if (!mapDataReceived) {
                return;
            }
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (visible) {
                int lodIndex = 0;
                for (int i = 0; i < _detailLevels.Length-1; i++) {
                    if (viewerDstFromNearestEdge > _detailLevels[i].visibleDstThreshold) {
                        lodIndex++;
                    }
                    else {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex) {
                    LODMesh lodMesh = _lodMeshes[lodIndex];
                    if (lodMesh.hasMesh) {
                        previousLODIndex = lodIndex;
                        _meshFilter.mesh = lodMesh.mesh;
                    } else if (!lodMesh.hasRequestedMesh) {
                        lodMesh.RequestMesh(_mapData);
                    }
                }

                
                if (lodIndex <= _collisionMinimumLOD) {
                    if (_collisionLODMesh.hasMesh) {
                        _meshCollider.sharedMesh = _collisionLODMesh.mesh;
                    } else if (!_collisionLODMesh.hasRequestedMesh) {
                        _collisionLODMesh.RequestMesh(_mapData);
                    }
                }

                terrainChunksVisibleLastUpdate.Add(this);
            }
            
            SetVisible(visible);
        }

        public void SetVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool isVisible() {
            return meshObject.activeSelf;
        }
    }

    class LODMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;
        private Action _updateCallback;

        public LODMesh(int lod, Action updateCallback) {
            this.lod = lod;
            _updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            _updateCallback();
        }

        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            _mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }
    
    [Serializable]
    public struct LODInfo {
        public int lod;
        public float visibleDstThreshold;
        public bool useForCollider;
    }
}