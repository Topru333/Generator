using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Режимы отрисовки
public enum DrawMode { NoiseMap, ColourMap, Mesh }

public class MapGenerator : MonoBehaviour {
    // Режим отрисовки
    public DrawMode drawMode;

    // Размер куска карты
    public const int mapChunkSize = 241;
    
    [Range(0,6)]
    // Уровень детализации
    public int levelOfDetail;

    // Размер шума.
    public float noiseScale;

    // Количество октав.
    public int octaves;
    [Range(0,1)]
    // 
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier = 1;
    public AnimationCurve meshHeightCurve;
    public bool autoUpdate;

    public TerrainType[] regions;

    public void GenerateMap() {
        NoiseParameters np = new NoiseParameters()
        {
            mapSize = mapChunkSize,
            seed = seed,
            scale = noiseScale,
            octaves = octaves,
            persistance = persistance,
            lacunarity = lacunarity
        };
        float[,] noiseMap = Noise.GenerateNoiseMap(np);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for(int y = 0; y < mapChunkSize; y++) {
            for(int x = 0; x < mapChunkSize; x++) {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++) {
                    if(currentHeight <= regions[i].height) {
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        switch (drawMode) {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGeneraitor.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.ColourMap:
                display.DrawTexture(TextureGeneraitor.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap , meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGeneraitor.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
                break;
        }
    }

    // При изменении параметров
    void OnValidate () {
        if (noiseScale <= 0.209) noiseScale = 0.21f;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;
    }
}


[System.Serializable]
public struct TerrainType {
    public string name;
    public float height; // Слой элемента от 0 до 1
    public Color colour;

}
