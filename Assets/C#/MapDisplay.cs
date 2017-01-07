using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRender;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    
    /// <summary>
    /// Функция прорисовки масива распределения
    /// </summary>
    /// <param name="noiseMap"></param>
    public void DrawTexture( Texture2D texture) {
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    /// <summary>
    /// Функция прорисовки сетки из треугольников
    /// </summary>
    /// <param name="meshData">Объект с данными сетки</param>
    /// <param name="texture">Наша вариация текстуры</param>
    public void DrawMesh (MeshData meshData, Texture2D texture) {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
