using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTesting : MonoBehaviour {
    public Vector3 upVec = Vector3.up,
                   rightVec= Vector3.right;

    public const int mapChunkSize = 241;

    [Range(0, 6)]
    // Уровень детализации
    public int levelOfDetail;

    // Размер шума.
    public float noiseScale;

    // Количество октав.
    public int octaves;
    [Range(0, 1)]
    // 
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public float meshHeightMultiplier = 1;
    // Use this for initialization
    void Start () {
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
        //Sector s = new Sector(upVec, rightVec, noiseMap, 400,32);
        //GetComponent<MeshFilter>().mesh = s.GetMeshData();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
