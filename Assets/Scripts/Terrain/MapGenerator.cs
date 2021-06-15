using System;
using UnityEngine;

namespace Terrain {
    public class MapGenerator : MonoBehaviour {
        public enum DrawMode {
            NoiseMap,
            ColorMap,
            Mesh
        }

        public bool autoUpdate;

        public DrawMode drawMode;
        public float lacunarity;
        public int mapHeight;
        public int mapWidth;
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
                mapWidth,
                mapHeight,
                noiseScale,
                octaves,
                persistence,
                lacunarity,
                offset
            );

            Color[] colorMap = new Color[mapWidth * mapHeight];

            for (int y = 0; y < mapHeight; y++) {
                for (int x = 0; x < mapWidth; x++) {
                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < terrainTypes.Length; i++) {
                        if (currentHeight <= terrainTypes[i].height) {
                            colorMap[y * mapWidth + x] = terrainTypes[i].color;
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
                    display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight));
                    break;
                }
                case DrawMode.Mesh: {
                    display.DrawMesh(
                        MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier),
                        TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight)
                    );
                    break;
                }
            }
        }

        private void OnValidate() {
            if (mapWidth < 1) {
                mapWidth = 1;
            }

            if (mapHeight < 1) {
                mapHeight = 1;
            }

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