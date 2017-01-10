using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ProceduralNoiseLib
{
    [DllImport("ProceduralNoiseLib")]
    public static extern int RandomTest();
    [DllImport("ProceduralNoiseLib")]
    public static extern void InitLib();
    [DllImport("ProceduralNoiseLib")]
    public static extern void DeInitLib();
    [DllImport("ProceduralNoiseLib")]
    public static extern float PerlinNoiseAtPoint(float x, float y, float z);

    public static Vector3[] octaveOffsets;

    public static void InitOctaveOffsets(NoiseParameters np)
    {
        System.Random prng = new System.Random(np.seed);
        octaveOffsets = new Vector3[np.octaves];
        //Генерация сдвигов для октав шума
        for (int i = 0; i < np.octaves; i++)
            octaveOffsets[i] = new Vector3(prng.Next(-10000, 10000), prng.Next(-10000, 10000), prng.Next(-10000, 10000));
    }

    public static float GetTerrainNoise(Vector3 pos, NoiseParameters np)
    {
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Сдвиг в середину
        float halfSize = 0;//np.mapSize / 2f;

        // Подсчет частоты
        float amplitude, frequency, noiseHeight;
       
        amplitude = frequency = 1;
        noiseHeight = 0;
        Vector3 samplePos;
        for (int i = 0; i < octaveOffsets.Length; i++)
        {
            samplePos = new Vector3((pos.x - halfSize) / np.scale * frequency + octaveOffsets[i].x, 
                                    (pos.y - halfSize) / np.scale * frequency + octaveOffsets[i].y, 
                                    (pos.z - halfSize) / np.scale * frequency + octaveOffsets[i].z);

            float perlinValue = PerlinNoiseAtPoint(samplePos.x, samplePos.y, samplePos.z) * 2 - 1;
            noiseHeight += perlinValue * amplitude;
            amplitude *= np.persistance;
            frequency *= np.lacunarity;
        }
        if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
        else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

        // Нормализация от 0 до 1
        return Math.Abs(noiseHeight);
    }
}

/// <summary>
/// Планета
/// </summary>
public class Planet : MonoBehaviour
{

    Sector[] sectors;
    GameObject[] go_Sectors;

    public NoiseParameters noiseParameters;
    public Material[]       planetSideMaterials;
    public float Radius = 5000f;
    [Range(2,255)]
    public int tesselationFactor = 32;

    public void Start()
    {
        // 6 карт шумов для 
        float[][][] noiseMap = new float[6][][];
        
        ProceduralNoiseLib.InitLib();
        ProceduralNoiseLib.InitOctaveOffsets(noiseParameters);
        sectors = new Sector[6];
        float fNMin=float.MaxValue, fNMax=float.MinValue;
        sectors[0] = new Sector(Vector3.up, Vector3.right, ref noiseMap[0], noiseParameters,ref fNMax,ref fNMin, Radius, tesselationFactor);
        sectors[1] = new Sector(Vector3.down, Vector3.right, ref noiseMap[1], noiseParameters, ref fNMax, ref fNMin, Radius, tesselationFactor);
        sectors[2] = new Sector(Vector3.right, Vector3.up, ref noiseMap[2], noiseParameters, ref fNMax, ref fNMin, Radius, tesselationFactor);
        sectors[3] = new Sector(Vector3.left, Vector3.up, ref noiseMap[3], noiseParameters, ref fNMax, ref fNMin, Radius, tesselationFactor);
        sectors[4] = new Sector(Vector3.forward, Vector3.up, ref noiseMap[4], noiseParameters, ref fNMax, ref fNMin, Radius, tesselationFactor);
        sectors[5] = new Sector(Vector3.back, Vector3.up, ref noiseMap[5], noiseParameters, ref fNMax, ref fNMin, Radius, tesselationFactor);
        
        for (int i = 0; i < 6; i++) {
            for (int x = 0; x < tesselationFactor; x++)            
                for (int y = 0; y < tesselationFactor; y++)                
                    noiseMap[i][x][y] = Mathf.InverseLerp(fNMin, fNMax, noiseMap[i][x][y]);

            planetSideMaterials[i].mainTexture = TextureGeneraitor.TextureFromHeightMap(noiseMap[i]);
            sectors[i].ApplyNoise(noiseParameters, noiseMap[i], tesselationFactor, Radius);
        }
        InstanseSectors();
        ProceduralNoiseLib.DeInitLib();
    }

    public void InstanseSectors()
    {
        go_Sectors = new GameObject[6];
        for (int i = 0; i < 6; i++)
        {
            go_Sectors[i] = new GameObject("Sector " + i);
            go_Sectors[i].AddComponent<MeshFilter>().mesh = sectors[i].GetMeshData();
            go_Sectors[i].AddComponent<MeshRenderer>().material = planetSideMaterials[i];
            go_Sectors[i].transform.parent = transform;
        }
    }

    public Sector[] Sectors
    {
        get
        {
            return sectors;
        }
    }
}

/// <summary>
/// Сектор планеты.
/// </summary>
public class Sector
{
    // Данные сектора.
    MeshData meshData;
    Mesh    unityMesh=null;
    // Подсекторы.
    Sector[] subSectors;

    public Sector(Vector3 direction, Vector3 right, ref float[][] noiseMap, NoiseParameters np,ref float fNMax,ref float fNMin, float size = 1f, int tessFactor = 2, bool hasSubSectors = false)
    {
        noiseMap = new float[tessFactor][];
        for(int i =0;i< tessFactor; i++)
            noiseMap[i] = new float[tessFactor];
        // Если поставлен флаг, создаем подсекторы.
        if (hasSubSectors)
            subSectors = new Sector[4];
        else
            subSectors = null;

        // Создаем сетку с n*n вершин, где (n-1) - колво прямоугольников
        meshData = new MeshData(tessFactor, tessFactor);
        meshData.normals = new Vector3[meshData.vertices.Length];
        float sizeMultiplier = size / tessFactor;

        Vector3 forward     = Vector3.Cross(direction, right);
        Vector3[] vertices  = meshData.vertices, normals = meshData.normals;
        Vector2[] uvs       = meshData.uvs;
        // Забиваем массив вершин внутри сетки спроецированными на сферу вершинами.
        for (int x = 0; x < tessFactor; x++)
        {
            for (int y = 0; y < tessFactor; y++)
            {
                int vertexIndex = x * tessFactor + y;

                vertices[vertexIndex] = ProjectOnSphere(direction * (size - sizeMultiplier) * 0.5f +
                                                            forward * (x * sizeMultiplier - (size - sizeMultiplier) / 2f) +
                                                            right *   (y * sizeMultiplier - (size - sizeMultiplier) / 2f), size);
                normals[vertexIndex] = vertices[vertexIndex].normalized;
                // 
                uvs[vertexIndex] = new Vector2((float)x / tessFactor, (float)y / tessFactor);
                
                // Двигаем вершину относительно нормалей
                int nx = (int)(uvs[vertexIndex].x * noiseMap.GetLength(0)), 
                    ny = (int)(uvs[vertexIndex].y * noiseMap[0].GetLength(0));
                noiseMap[nx][ny] = ProceduralNoiseLib.GetTerrainNoise(normals[vertexIndex] * tessFactor, np);
                fNMax = Mathf.Max(fNMax, noiseMap[nx][ny]);
                fNMin = Mathf.Min(fNMin, noiseMap[nx][ny]);
            }
        }
        int a, b, c, d;
        // Забиваем массив треугольников данными.
        for (int i = 0; i < tessFactor * (tessFactor - 1); i++)
        {
            if (i % tessFactor != tessFactor - 1)
            {
                a = i; b = i + 1; c = i + tessFactor; d = i + tessFactor + 1;
                meshData.AddTriangle(a, b, c);
                meshData.AddTriangle(c, b, d);
            }
        }
    }

    public void ApplyNoise(NoiseParameters np,float[][] noiseMap, int tessFactor = 2, float size = 1f)
    {
        Vector3[] vertices = meshData.vertices, normals = meshData.normals;
        Vector2[] uvs = meshData.uvs;
        for (int x = 0; x < tessFactor; x++)
        {
            for (int y = 0; y < tessFactor; y++)
            {
                int vertexIndex = x * tessFactor + y;
                int nx = (int)(uvs[vertexIndex].x * noiseMap.GetLength(0)),
                    ny = (int)(uvs[vertexIndex].y * noiseMap[0].GetLength(0));
                float noise = np.heightCurve.Evaluate(noiseMap[nx][ny])* size*np.heightScale;
                vertices[vertexIndex] += normals[vertexIndex] * noise;
            }
        }
    }

    // Получает координаты сферической развертки
    private Vector2 GetSphericalUVs(Vector3 pos)
    {
        Vector3 unitVec = pos.normalized;
        float u = 0.5f + Mathf.Atan2(unitVec.y,unitVec.x) / (2 * Mathf.PI), //Mathf.Atan2(Mathf.Sqrt(pos.y * pos.y + pos.x * pos.x), pos.z)
              v = 0.5f - Mathf.Asin(unitVec.z) / Mathf.PI;//Mathf.Atan2(pos.y, pos.x) + Mathf.PI;
        return new Vector2(u, v);
    }

    // Проекция позиции на сферу радиусом R
    Vector3 ProjectOnSphere(Vector3 pos, float r)
    {
        return pos.normalized * r;
    }
    // Возвращает данные сектора.
    public Mesh GetMeshData(bool reCreate=false)
    {
        if (!unityMesh || reCreate)
        {
            unityMesh = meshData.CreateMesh();
            return meshData.CreateMesh();
        }
        else
            return unityMesh;
    }
}