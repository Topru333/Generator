using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable]
public struct NoiseParameters
{
    public int mapSize, seed, octaves;
    public float scale, persistance, lacunarity, heightScale;
    public AnimationCurve heightCurve;
}

public static class Noise
{
    /// <summary>
    /// Функция генерирующая массив с коэффициентами шума перлина. 
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
    public static float[,] GenerateNoiseMap(NoiseParameters p)
    {
        if (p.mapSize <= 0) p.mapSize = 1;
        if (p.scale <= 0.209) p.scale = 0.21f;

        // Инициализируем массив значений.
        float[,] noiseMap = new float[p.mapSize, p.mapSize];

        System.Random prng = new System.Random(p.seed);
        Vector2[] octaveOffsets = new Vector2[p.octaves];
        //Псевдо рандом генерации
        for (int i = 0; i < p.octaves; i++) {
            float offsetX = prng.Next(-10000, 10000);
            float offsetY = prng.Next(-10000, 10000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Середина для центрального приблежения
        float halfWidth = p.mapSize / 2f;
        float halfHeight = p.mapSize / 2f;

        // Подсчет частоты
        float amplitude, frequency, noiseHeight;
        for (int y = 0; y < p.mapSize; y++)
        {
            for (int x = 0; x < p.mapSize; x++) {
                amplitude = frequency = 1;
                noiseHeight = 0;

                for (int i = 0; i < p.octaves; i++) {
                    float sampleX = (x - halfWidth)     / p.scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight)    / p.scale * frequency + octaveOffsets[i].y;

                    float perLinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perLinValue * amplitude;
                    amplitude *= p.persistance;
                    frequency *= p.lacunarity;
                }
                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }
        // Нормализация от 0 до 1
        for (int y = 0; y < p.mapSize; y++) {
            for (int x = 0; x < p.mapSize; x++) {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
    

}
