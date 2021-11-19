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
    private int[] mountainTris;

    [Header("Object Transform Settings")]
    public Transform playerTransform;

    [Header("Terrain Settings")]
    public int terrainSize = 20;
    [Range(1, 10)]
    public int elevationChanges;

    [Header("Perlin Settings")]
    [Range(0.001f, .999f)]
    public float frequency = .3f;
    [Range(1f, 10f)]
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
                verticies[i] = new Vector3(playerTransform.position.x + x - terrainSize/2, this.transform.position.y + y, playerTransform.position.z + z - terrainSize / 4);
                i++;
            }
            zLoopCompletes += terrainSize;
        }

        zLoopCompletes = 0;
        int vert = 0, tris = 0;
        triangles = new int[terrainSize * terrainSize * 6];
        mountainTris = new int[terrainSize * terrainSize * 6];
        for (int z = 0; z < terrainSize; z++)
        {
            for (int x = 0; x < terrainSize; x++)
            {
                if (IsPartOfMountain(x,z,zLoopCompletes))
                {
                    AddTriangle(mountainTris, tris, vert);
                }
                else
                {
                    AddTriangle(triangles, tris, vert);
                }

                vert++;
                tris += 6;
            }
            zLoopCompletes += terrainSize;
            vert++;
        }

        Array.Reverse(triangles); // so that they face upwards
        Array.Reverse(mountainTris);

    }

    private bool IsPartOfMountain(int x, int z, int zLoopCompletes)
    {
        bool isPartOfMountain = false;

        if (biomes[x + z + zLoopCompletes] == Biome.Mountain)
        {
            isPartOfMountain = true;
        }
        else if(x + z + zLoopCompletes - 1 > 0)
        {
            if (biomes[x - 1 + z + zLoopCompletes] == Biome.Mountain)
            {
                isPartOfMountain = true;
            }
        }
        else if (x + z + zLoopCompletes - terrainSize > 0)
        {
            if (biomes[x - 1 + z + zLoopCompletes - terrainSize] == Biome.Mountain)
            {
                isPartOfMountain = true;
            }
        }
        else if (x + z + zLoopCompletes + terrainSize < biomes.Length)
        {
            if (biomes[x + z + zLoopCompletes + terrainSize] == Biome.Mountain)
            {
                isPartOfMountain = true;
            }
        }
        else if (x + z + zLoopCompletes + 1 < biomes.Length)
        {
            if (biomes[x + z + zLoopCompletes + 1] == Biome.Mountain)
            {
                isPartOfMountain = true;
            }
        }        
        return isPartOfMountain;
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
            mesh = new Mesh();
            mesh.name = "Generated Terrain Mesh";
        }
        mesh.vertices = verticies;
        mesh.subMeshCount = 2;
        mesh.SetTriangles(triangles, 0);
        mesh.SetTriangles(mountainTris, 1);
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private void GenerateBiome(int x, int y, int zLoopCompletes)
    {
        float biomeWeights = PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + y)*100;

        if (biomeWeights <= mountainsWeight)
        {
            biomes[x + y + zLoopCompletes] = Biome.Mountain;
        }
        else if (biomeWeights <= mountainsWeight+valleyWeight)
        {
            biomes[x + y + zLoopCompletes] = Biome.Valley;
        }
        else
        {
            biomes[x + y + zLoopCompletes] = Biome.Plains;
        }
    }
    private float GetBiomeHeight(int x, int y, int zLoopCompletes)
    {
        if (biomes[x+y+zLoopCompletes] == Biome.Mountain)
        {
            
            return PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + y) * elevationChanges;
        } else if (biomes[x + y + zLoopCompletes] == Biome.Valley)
        {
            
            return -PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + y) * elevationChanges;
        } else
        {
            
            return 0;
        }
    }
    private float PerlinNoise(float x, float y)
    {
        return Mathf.PerlinNoise(x * frequency, y * frequency)*amplitude;
    }
}