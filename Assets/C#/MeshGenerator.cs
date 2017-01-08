using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {

    public static MeshData GenerateTerrainMesh (float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0)?1:levelOfDetail * 2; // Упрощение нашей сетки
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for(int y = 0; y < height; y += meshSimplificationIncrement) {
            for(int x = 0; x < width; x += meshSimplificationIncrement) {

                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height);

                // Добавление последних треугольников в ряде и в столбике по формуле
                // i, i + width + 1, i + width;      i + width + 1, i, i + 1;
                if(x<width-1 && y < height - 1) {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }
        return meshData;
    }

    
}

public class MeshData {
    public Vector3[] vertices; //Массив из вершин
    public int[]     triangles; //Массив из треугольников (каждые три элемента 1 треугольник)
    public Vector3[] normals=null;
    public Vector2[] uvs;

    int triangleIndex=0;

    public MeshData (int meshWidth, int meshHeight) {
        vertices    = new Vector3[meshWidth * meshHeight]; //Координаты вершин
        uvs         = new Vector2[meshWidth * meshHeight];
        triangles   = new int[(meshWidth - 1) * (meshHeight - 1) * 6]; //Кол во вершин для треугольников
    }

    /// <summary>
    /// Добавление новго треугольника
    /// </summary>
    /// <param name="a">индекс вершины А в массиве</param>
    /// <param name="b">индекс вершины B в массиве</param>
    /// <param name="c">индекс вершины C в массиве</param>
    public void AddTriangle (int a, int b, int c) {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh () {
        Mesh mesh = new Mesh()
        {
            vertices    = vertices,
            triangles   = triangles,
            uv = uvs
        };
        //if (normals != null)
        //    mesh.normals = normals;
        //else
            mesh.RecalculateNormals();
        return mesh;
    }

}