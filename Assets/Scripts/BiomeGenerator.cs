using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshGenerator))]
public class BiomeGenerator : MonoBehaviour
{
    [Header("Biome Settings")]
    public MeshGenerator meshGen;
    public float requiredNeighbors;
    public float mountainHeight;
    public float snowHeight;

    [Header("Detail Settings")]
    public DetailObject[] detailObjects;
    

    [Header("Perlin Settings")]
    [Range(0.001f, .999f)]
    public float frequency = .3f;
    [Range(1f, 20f)]
    public float amplitude = 2f;

    [Serializable]
    public struct DetailObject
    {
        public GameObject objectToSpawn;
        public Biomes biome;
        public float chance;
        public float spawnRadius;
    }



    private Biomes[,] biomes;
    private float terrainSize;
    private Transform playerTransform;
    private float waterLevel;
    private List<GameObject> details = new List<GameObject>();
    public enum Biomes
    {
        Grass,
        Mountain,
        Snow,
        Mud
    }
    public void Init(Transform transform, float size, float waterLevel)
    {
        playerTransform = transform;
        terrainSize = size;
        biomes = new Biomes[(int)terrainSize + 1, (int)terrainSize + 1];
        this.waterLevel = waterLevel;
    }
    public float PopulateHeight(float x, float z)
    {
        float perlin = this.transform.position.y + PerlinNoise(x + Mathf.FloorToInt(playerTransform.position.x), z + Mathf.FloorToInt(playerTransform.position.z));
        PopulateHeightBasedBiome(perlin, x, z);
        return perlin;

    }
    public bool IsMud(int x, int z){ return biomes[x, z] == Biomes.Mud; }
    public bool IsMountain(int x, int z) { return biomes[x, z] == Biomes.Mountain; }
    public bool IsSnow(int x, int z) { return biomes[x, z] == Biomes.Snow; }
    public void CreateDetails(Vector3[] vertexArray)
    {
        foreach(GameObject gameObject in details)
        {
            Destroy(gameObject);
        }
        details.Clear();
        SpawnObjects(vertexArray);
    }
    private void SpawnObjects(Vector3[] vertexArray)
    {
        foreach (DetailObject detailObject in detailObjects)
        {
            foreach (Vector3 vertex in vertexArray)
            {
                if (biomes[(int)meshGen.WorldPointToVertex(vertex).x, (int)meshGen.WorldPointToVertex(vertex).z] != detailObject.biome)
                    continue;

                float perlin = PerlinNoise(vertex.x, vertex.z) * (100 / amplitude);
                if (perlin <= detailObject.chance)
                {
                    bool skip = false;
                    foreach (GameObject detail in details)
                    {
                        if (Vector3.Distance(detail.transform.position, vertex) < detailObject.spawnRadius)
                            skip = true;
                    }
                    if (skip)
                        continue;
                    GameObject spawnedObject = GameObject.Instantiate(detailObject.objectToSpawn);
                    spawnedObject.transform.position = vertex;
                    details.Add(spawnedObject);
                }
            }
        }
    }
    private void PopulateHeightBasedBiome(float perlin, float x, float z)
    {
        float waterBorder = 1;
        if (perlin >= this.transform.position.y + mountainHeight)
            biomes[(int)x, (int)z] = Biomes.Mountain;
        if (perlin >= this.transform.position.y + snowHeight)
            biomes[(int)x, (int)z] = Biomes.Snow;
        if (perlin <= this.transform.position.y + waterLevel+waterBorder)
            biomes[(int)x, (int)z] = Biomes.Mud;
    }
    private float PerlinNoise(float x, float y)
    {
        return Mathf.Abs(Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude);
    }
}
