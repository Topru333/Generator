using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Планета
/// </summary>
public class Planet : MonoBehaviour
{

    Sector[] sectors;
    GameObject[] go_Sectors;
    public NoiseParameters noiseParameters;
    public Material planetBaseMaterial;

    public void Start()
    {
        float size = 100; int tessFactor = 32;
        float[,] noiseMap = Noise.GenerateNoiseMap(noiseParameters);
        sectors = new Sector[6];

        sectors[0] = new Sector(Vector3.up, Vector3.right, noiseMap, size, tessFactor);
        sectors[1] = new Sector(Vector3.down, Vector3.right, noiseMap, size, tessFactor);
        sectors[2] = new Sector(Vector3.right, Vector3.up, noiseMap, size, tessFactor);
        sectors[3] = new Sector(Vector3.left, Vector3.up, noiseMap, size, tessFactor);
        sectors[4] = new Sector(Vector3.forward, Vector3.up, noiseMap, size, tessFactor);
        sectors[5] = new Sector(Vector3.back, Vector3.up, noiseMap, size, tessFactor);
        InstanseSectors();
        planetBaseMaterial.mainTexture = TextureGeneraitor.TextureFromHeightMap(noiseMap);
    }

    public void InstanseSectors()
    {
        go_Sectors = new GameObject[6];
        for (int i = 0; i < 6; i++)
        {
            go_Sectors[i] = new GameObject("Sector " + i);
            go_Sectors[i].AddComponent<MeshFilter>().mesh = Sectors[i].GetMeshData();
            go_Sectors[i].AddComponent<MeshRenderer>().material = planetBaseMaterial;
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
    Mesh meshData;
    // Подсекторы.
    Sector[] subSectors;

    public Sector(Vector3 direction, Vector3 right, float[,] noiseMap, float size = 1f, int tessFactor = 2, bool hasSubSectors = false)
    {
        // Если поставлен флаг, создаем подсекторы.
        if (hasSubSectors)
            subSectors = new Sector[4];
        else
            subSectors = null;

        // Создаем сетку с n*n вершин, где (n-1) - колво прямоугольников
        MeshData md = new MeshData(tessFactor, tessFactor);
        md.normals = new Vector3[md.vertices.Length];
        float sizeMultiplier = size / tessFactor;

        Vector3 forward = Vector3.Cross(direction, right);
        // Забиваем массив вершин внутри сетки спроецированными на сферу вершинами.
        for (int x = 0; x < tessFactor; x++)
        {
            for (int y = 0; y < tessFactor; y++)
            {
                int vertexIndex = x * tessFactor + y;

                md.vertices[vertexIndex] = ProjectOnSphere(direction * (size - sizeMultiplier) * 0.5f +
                                                            forward * (x * sizeMultiplier - (size - sizeMultiplier) / 2f) +
                                                            right * (y * sizeMultiplier - (size - sizeMultiplier) / 2f), size);
                md.normals[vertexIndex] = md.vertices[vertexIndex].normalized;
                // 
                float theta = Mathf.Atan2(Mathf.Sqrt(md.vertices[vertexIndex].y * md.vertices[vertexIndex].y + md.vertices[vertexIndex].x * md.vertices[vertexIndex].x), md.vertices[vertexIndex].z),
                      phi = Mathf.Atan2(md.vertices[vertexIndex].y, md.vertices[vertexIndex].x) + Mathf.PI;
                md.uvs[vertexIndex] = new Vector2(phi / (2 * Mathf.PI), theta / (Mathf.PI));
                //if (md.uvs[vertexIndex].x >= 1)
                //    md.uvs[vertexIndex].x = 0;
                //if (md.uvs[vertexIndex].y >= 1)
                //    md.uvs[vertexIndex].y = 0;
                // Двигаем вершину относительно нормалей
                int nx = (int)(md.uvs[vertexIndex].x * noiseMap.GetLength(1)), ny = (int)(md.uvs[vertexIndex].y * noiseMap.GetLength(0));
                float noise = noiseMap[nx, ny] * size / 4;
                md.vertices[vertexIndex] += md.normals[vertexIndex] * noise;
            }
        }
        int a, b, c, d;
        // Забиваем массив треугольников данными.
        for (int i = 0; i < tessFactor * (tessFactor - 1); i++)
        {
            if (i % tessFactor != tessFactor - 1)
            {
                a = i; b = i + 1; c = i + tessFactor; d = i + tessFactor + 1;
                md.AddTriangle(a, b, c);
                md.AddTriangle(c, b, d);
            }
        }
        meshData = md.CreateMesh();
    }
    // Проекция позиции на сферу радиусом R
    Vector3 ProjectOnSphere(Vector3 pos, float r)
    {
        return pos.normalized * r;
    }
    // Возвращает данные сектора.
    public Mesh GetMeshData()
    {
        return meshData;
    }
    Vector2 GetSC(Vector3 pos)
    {
        float theta = Mathf.Atan2(Mathf.Sqrt(pos.y * pos.y + pos.x * pos.x), pos.z),
              phi = Mathf.Atan2(pos.y, pos.x) + Mathf.PI;
        return new Vector2(phi / (2 * Mathf.PI), theta / (Mathf.PI));
    }
}