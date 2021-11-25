using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshGenerator : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] verticies;
    private int[] triangles;
    private int[] mountainTriangles;
    private Biomes[,] biomes;

    [Header("Object Transform Settings")]
    public Transform playerTransform;
    public GameObject waterPlane;

    [Header("Terrain Settings")]
    public int terrainSize = 20;
    public BiomeWeights[] weights;

    [Header("Biome Settings")]
    public float additionalMountainHeight;
    public float riverDepthMultiplier;
    public float waterLevel;
    public float requiredNeighbors;

    [Header("Perlin Settings")]
    [Range(0.001f, .999f)]
    public float frequency = .3f;
    [Range(1f, 10f)]
    public float amplitude = 2f;

    public enum Biomes
    {
        Default,
        River,
        Mountain
    }

    [Serializable]
    public struct BiomeWeights
    {
        [Range(0, 100)]
        public float weight;
        public Biomes biome;
    }
    private void Start()
    {
        CreateTerrain();
    }
    private void Update()
    {
        CreateTerrain();
    }

    public void CreateTerrain()
    {
        verticies = new Vector3[(terrainSize + 1) * (terrainSize + 1)];
        biomes = new Biomes[terrainSize + 1, terrainSize + 1];
        triangles = new int[terrainSize * terrainSize * 6];
        GenerateBiomes();
        SmoothBiomes();
        GenerateMesh();
        UpdateMesh();
        UpdateWater();
    }
    private void UpdateWater()
    {
        waterPlane.transform.position = new Vector3(playerTransform.transform.position.x, this.transform.position.y-waterLevel, playerTransform.transform.position.z + (terrainSize / 4));
        waterPlane.transform.localScale = new Vector3(terrainSize, terrainSize, 1f);
    }

    private void GenerateBiomes()
    {
        for (int x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                foreach (BiomeWeights biomeWeight in weights)
                {
                    float perlin = Mathf.PerlinNoise((playerTransform.position.x + x) * frequency, (playerTransform.position.z + z) * frequency) * 100;
                    if (perlin <= biomeWeight.weight && biomes[x,z] == Biomes.Default)
                        biomes[x, z] = biomeWeight.biome;
                }
            }
        }
    }
    private void SmoothBiomes()
    {
        for (int x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                if(biomes[x,z] == Biomes.Default)
                    if (GetSameBiomeNeighborCount(x, z, Biomes.Mountain) >= requiredNeighbors)
                    {
                        biomes[x, z] = Biomes.Mountain;
                        if (GetSameBiomeNeighborCount(x, z, Biomes.River) >= requiredNeighbors)
                            biomes[x, z] = Biomes.River;
                    }
                if (biomes[x, z] == Biomes.Mountain)
                    if (GetSameBiomeNeighborCount(x, z, Biomes.Mountain) < requiredNeighbors)
                    {
                        biomes[x, z] = Biomes.Default;
                        if (GetSameBiomeNeighborCount(x, z, Biomes.River) >= requiredNeighbors)
                            biomes[x, z] = Biomes.River;
                    }
                if (biomes[x, z] == Biomes.River)
                    if (GetSameBiomeNeighborCount(x, z, Biomes.River) < requiredNeighbors)
                    {
                        biomes[x, z] = Biomes.Default;
                        if (GetSameBiomeNeighborCount(x, z, Biomes.Mountain) >= requiredNeighbors)
                            biomes[x, z] = Biomes.Mountain;
                    }
            }
        }
    }
    private int GetSameBiomeNeighborCount(int x, int z, Biomes biomeType)
    {
        int biomeCount = 0;
        if (x > 0)
            if (biomes[x - 1, z] == biomeType) biomeCount++;
        if (x < terrainSize)
            if (biomes[x + 1, z] == biomeType) biomeCount++;
        if (z > 0)
            if (biomes[x, z - 1] == biomeType) biomeCount++;
        if (z < terrainSize)
            if (biomes[x, z + 1] == biomeType) biomeCount++;
        if (x > 0 && z > 0)
            if (biomes[x - 1, z - 1] == biomeType) biomeCount++;
        if (x < terrainSize && z > 0)
            if (biomes[x + 1, z - 1] == biomeType) biomeCount++;
        if (x > 0 && z < terrainSize)
            if (biomes[x - 1, z + 1] == biomeType) biomeCount++;
        if (x <terrainSize && z < terrainSize)
            if (biomes[x + 1, z + 1] == biomeType) biomeCount++;
        return biomeCount;
    }

    private void GenerateMesh()
    {
        for (int i = 0, x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                float y = GetBiomeHeight(x, z);
                verticies[i] = new Vector3(playerTransform.position.x + (x - terrainSize / 2), this.transform.position.y + y, playerTransform.position.z + (z - terrainSize / 4));
                i++;
            }
        }

        int vert = 0, tris = 0;
        triangles = new int[terrainSize * terrainSize * 6];
        mountainTriangles = new int[terrainSize * terrainSize * 6];
        for (int x = 0; x < terrainSize; x++)
        {
            for (int z = 0; z < terrainSize; z++)
            {
                if (biomes[x,z] == Biomes.Mountain)
                {
                    AddTriangle(mountainTriangles, tris, vert);
                }
                else
                {
                    AddTriangle(triangles, tris, vert);
                }
                vert++;
                tris += 6;
            }
            vert++;
        }
        Array.Reverse(triangles); // so that they face upwards
        Array.Reverse(mountainTriangles); 

    }

    private void AddTriangle(int[] triArray, int tris, int vert)
    {
        triArray[tris + 0] = vert + 0;
        triArray[tris + 1] = vert + terrainSize + 1;
        triArray[tris + 2] = vert + 1;

        triArray[tris + 3] = vert + 1;
        triArray[tris + 4] = vert + terrainSize + 1;
        triArray[tris + 5] = vert + terrainSize + 2;
    }
    private void UpdateMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh
            {
                name = "Generated Terrain Mesh"
            };
        }
        mesh.vertices = verticies;
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles, 0);
        mesh.SetTriangles(mountainTriangles, 1);

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    private float PerlinNoise(float x, float y)
    {
        return Mathf.Abs(Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude);
    }

    private float GetBiomeHeight(float x, float z)
    {
        if (biomes[(int)x, (int)z] == Biomes.Mountain)
        {
            return this.transform.position.y + PerlinNoise(x + playerTransform.position.x, z + playerTransform.position.z);
        }
        else if(biomes[(int)x, (int)z] == Biomes.River)
        {
            return -PerlinNoise(x + playerTransform.position.x, z + playerTransform.position.z);
        } else
        {
            return this.transform.position.y + PerlinNoise(x + playerTransform.position.x, z + playerTransform.position.z);
        }
    }
}