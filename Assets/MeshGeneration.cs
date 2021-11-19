using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshGeneration : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] verticies;
    private int[] triangles;

    [Header("Object Transform Settings")]
    public Transform playerTransform;

    [Header("Terrain Settings")]
    public int terrainSize = 20;

    [Header("Perlin Settings")]
    [Range(0.001f, .999f)]
    public float frequency = .3f;
    [Range(0.001f, 10f)]
    public float amplitude = 2f;

    public int mountainsWeight = 0;
    public int valleyWeight = 0;

    private Biome[] biomes;
    private enum Biome
    {
        Plains,
        Valley,
        Mountain,
        BiomeCount
    }

    private void Start()
    {
        verticies = new Vector3[(terrainSize + 1) * (terrainSize + 1)];
        triangles = new int[terrainSize * terrainSize * 6];
        CreateTerrain();
    }
    private void Update()
    {
       CreateTerrain();
    }

    public void CreateTerrain()
    {
        biomes = new Biome[(terrainSize + 1) * (terrainSize + 1)];
        GenerateMesh();
        UpdateMesh();
    }

    private void GenerateMesh()
    {
        

        int zLoopCompletes = 0;
        for (int i = 0, x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                GenerateBiome(x, z, zLoopCompletes);
                float y = GetBiomeHeight(x, z, zLoopCompletes);
                verticies[i] = new Vector3(playerTransform.position.x + x - terrainSize/2, this.transform.position.y + y, playerTransform.position.z + z - terrainSize / 2);
                i++;
            }
            zLoopCompletes += terrainSize;
        }

        int vert = 0, tris = 0;
        triangles = new int[terrainSize * terrainSize * 6];
        for (int z = 0; z < terrainSize; z++)
        {
            for (int x = 0; x < terrainSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + terrainSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + terrainSize + 1;
                triangles[tris + 5] = vert + terrainSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        Array.Reverse(triangles); // so that they face upwards

    }

    private void UpdateMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
            mesh.name = "Generated Terrain Mesh";
        }
        mesh.Clear();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void GenerateBiome(float x, float y, int zLoopCompletes)
    {
        float biomeWeights = PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + y)*100;

        if (biomeWeights <= mountainsWeight)
        {
            biomes[(int)x + (int)y + zLoopCompletes] = Biome.Mountain;
        }
        else if (biomeWeights <= mountainsWeight+valleyWeight)
        {
            biomes[(int)x + (int)y + zLoopCompletes] = Biome.Valley;
        }
        else
        {
            biomes[(int)x + (int)y + zLoopCompletes] = Biome.Plains;
        }
    }
    private float GetBiomeHeight(float x, float y, int zLoopCompletes)
    {
        if (biomes[(int)x+(int)y+zLoopCompletes] == Biome.Mountain)
        {
            
            return PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + y) * 2;
        } else if (biomes[(int)x + (int)y + zLoopCompletes] == Biome.Valley)
        {
            
            return -PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + y);
        } else
        {
            
            return PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + y);
        }
    }
    private float PerlinNoise(float x, float y)
    {
        return Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
    }
}