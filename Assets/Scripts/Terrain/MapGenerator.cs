using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Terrain {
    public class MapGenerator : MonoBehaviour {
        public enum DrawMode {
            NoiseMap,
            ColorMap,
            Mesh
        }

        public const int mapChunkSize = 241;
        [Range(0,6)]
        public int levelOfDetail;

        public bool autoUpdate;

        public DrawMode drawMode;
        public float lacunarity;
        public int meshHeightMultiplier;
        public AnimationCurve meshHeightCurve;
        public float noiseScale;

        public int octaves;
        public Vector2 offset;

        [Range(0, 1)] public float persistence;

        public int seed;

        public TerrainType[] terrainTypes;
        Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

        private void Update() {
            if (mapDataThreadInfoQueue.Count > 0) {
                for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                    MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }

            if (meshDataThreadInfoQueue.Count > 0) {
                for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
                    MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
        }

        private MapData GenerateMapData() {
            float[,] noiseMap = Noise.GenerateNoiseMap(
                seed,
                mapChunkSize,
                mapChunkSize,
                noiseScale,
                octaves,
                persistence,
                lacunarity,
                offset
            );

            Color[] colorMap = new Color[mapChunkSize * mapChunkSize];

            for (int y = 0; y < mapChunkSize; y++) {
                for (int x = 0; x < mapChunkSize; x++) {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < terrainTypes.Length; i++) {
                        if (currentHeight <= terrainTypes[i].height) {
                            colorMap[y * mapChunkSize + x] = terrainTypes[i].color;
                            break;
                        }
                    }
                }
            }

            return new MapData(noiseMap, colorMap);
        }

        public void RequestMapData(Action<MapData> callback) {
            ThreadStart threadStart = delegate {
                MapDataThread(callback);
            };
            
            new Thread(threadStart).Start();
        }

        public void RequestMeshData(MapData mapData, Action<MeshData> callback) {
            
        }

        void MeshDataThread(MapData mapData, Action<MeshData> callback) {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(
                mapData.heightMap,
                meshHeightMultiplier,
                meshHeightCurve,
                levelOfDetail
            );
            lock (meshDataThreadInfoQueue) {
                meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        void MapDataThread(Action<MapData> callback) {
            MapData mapData = GenerateMapData();
            lock (mapDataThreadInfoQueue) {
                mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        public void DrawMapInEditor() {
            MapData mapData = GenerateMapData();
            
            MapDisplay display = FindObjectOfType<MapDisplay>();

            switch (drawMode) {
                case DrawMode.NoiseMap: {
                    display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
                    break;
                }
                case DrawMode.ColorMap: {
                    display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize));
                    break;
                }
                case DrawMode.Mesh: {
                    display.DrawMesh(
                        MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail),
                        TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChunkSize, mapChunkSize)
                    );
                    break;
                }
            }
        }

        private void OnValidate() {
            if (lacunarity < 1) {
                lacunarity = 1;
            }

            if (octaves < 0) {
                octaves = 0;
            }
        }
    }

    struct MapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;
        public MapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public struct MapData {
    public readonly float[,] heightMap;
    public readonly Color[] colorMap;
    public MapData(float[,] heightMap, Color[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }
}