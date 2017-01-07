using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    /// <summary>
    /// Функция распределения
    /// </summary>
    /// <param name="mapWidth"> широта </param>
    /// <param name="mapHeight"> высота </param>
    /// <param name="seed"> отбор </param>
    /// <param name="scale"> приближенность </param>
    /// <param name="octaves"> кол во типов (допустим 2 это горы и реки </param>
    /// <param name="persistance"> крепкость </param>
    /// <param name="lacunarity"> мера неоднородности </param>
    /// <param name="offset"> смещение </param>
    /// <returns></returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        if (mapHeight <= 0) mapHeight = 1;
        if (mapWidth <= 0) mapWidth = 1;
        if (scale <= 0.209) scale = 0.21f;

        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        //Псевдо рандом генерации
        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-10000, 10000) + offset.x;
            float offsetY = prng.Next(-10000, 10000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Середина для центрального приблежения
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // Подсчет частоты
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHight = 0;
                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perLinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHight += perLinValue * amplitude;
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }
                if (noiseHight > maxNoiseHeight) maxNoiseHeight = noiseHight;
                else if (noiseHight < minNoiseHeight) minNoiseHeight = noiseHight;

                noiseMap[x, y] = noiseHight; 
            }
        }

        // Нормализация от 0 до 1
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }

}
