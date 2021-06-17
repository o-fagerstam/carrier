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
        
        private void Start() {
            chunkSize = MapGenerator.mapChunkSize - 1;
            chunkVisibleInViewDst = Mathf.RoundToInt(MaxViewDist / chunkSize);
        }

        private void Update() {
            var newViewerPosition = viewer.position;
            viewerPosition = new Vector2(newViewerPosition.x, newViewerPosition.z);
            UpdateVisibleChunks();
        }

        void UpdateVisibleChunks() {
            int currentChunkCoordX = Mathf.RoundToInt(viewer.position.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewer.position.y / chunkSize);

            for (int yOffset = -chunkVisibleInViewDst; yOffset <= chunkVisibleInViewDst; yOffset++) {
                for (int xOffset = -chunkVisibleInViewDst; xOffset < chunkVisibleInViewDst; xOffset++) {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                    if (_terrainChunkDict.ContainsKey(viewedChunkCoord)) {
                        _terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else {
                        _terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize));
                    }
                }
            }
        }

        public class TerrainChunk {
            private GameObject meshObject;
            private Vector2 position;
            private Bounds _bounds;
            public TerrainChunk(Vector2 coord, int size) {
                position = coord * size;
                _bounds = new Bounds(position, Vector2.one * size);
                Vector3 positionV3 = new Vector3(position.x, 0f, position.y);

                meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                meshObject.transform.position = positionV3;
                meshObject.transform.localScale = Vector3.one * size / 10f;
                SetVisible(false);
            }

            public void UpdateTerrainChunk() {
                float viewerDstFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= MaxViewDist;
                SetVisible(visible);
            }

            public void SetVisible(bool visible) {
                meshObject.SetActive(visible);
            }
        }
    }
}