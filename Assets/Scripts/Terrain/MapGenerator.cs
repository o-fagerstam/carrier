using System;
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

        public void GenerateMap() {
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

            MapDisplay display = FindObjectOfType<MapDisplay>();

            switch (drawMode) {
                case DrawMode.NoiseMap: {
                    display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                    break;
                }
                case DrawMode.ColorMap: {
                    display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
                    break;
                }
                case DrawMode.Mesh: {
                    display.DrawMesh(
                        MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail),
                        TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize)
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
}

[Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}