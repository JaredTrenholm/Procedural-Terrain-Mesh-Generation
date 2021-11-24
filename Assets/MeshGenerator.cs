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
    private Biomes[] biomes;

    [Header("Object Transform Settings")]
    public Transform playerTransform;

    [Header("Terrain Settings")]
    public int terrainSize = 20;
    public float mountainHeightMultiplier;

    [Header("Perlin Settings")]
    [Range(0.001f, .999f)]
    public float frequency = .3f;
    [Range(1f, 10f)]
    public float amplitude = 2f;

    private enum Biomes
    {
        Default,
        River,
        Mountain
    }
    private void Start()
    {
        verticies = new Vector3[(terrainSize + 1) * (terrainSize + 1)];
        biomes = new Biomes[verticies.Length];
        triangles = new int[terrainSize * terrainSize * 6];
        CreateTerrain();
    }
    private void Update()
    {
        CreateTerrain();
    }

    public void CreateTerrain()
    {
        GenerateBiomes();
        SmoothBiomes(Biomes.Mountain);
        SmoothBiomes(Biomes.River);
        GenerateMesh();
        UpdateMesh();
    }

    private void GenerateBiomes()
    {
        for(int i = 0; i < biomes.Length; i++)
        {
            for (int x = 0; x <= terrainSize; x++)
            {
                for (int z = 0; z <= terrainSize; z++)
                {
                    float perlin = PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + z);
                    if(perlin > 3f)
                    {
                        biomes[i] = Biomes.Mountain;
                    }
                    else
                    {
                        biomes[i] = Biomes.Default;
                    }
                    i++;
                }
            }
        }
    }
    private void SmoothBiomes(Biomes biomeTypeToCheck)
    {
        Biomes[] biomeFilter = biomes;
        for (int i = 0; i < biomes.Length; i++)
        {
            int biomeCount = 0;
            if (biomes[i] == biomeTypeToCheck)
            {
                if (i > terrainSize)
                {
                    if (biomes[i - terrainSize] == biomeTypeToCheck) biomeCount++;
                }
                if (i+terrainSize < biomes.Length)
                {
                    if (biomes[i + terrainSize] == biomeTypeToCheck) biomeCount++;
                }
                if (i > 0)
                {
                    if (biomes[i - 1] == biomeTypeToCheck) biomeCount++;
                }
                if (i + 1 < biomes.Length)
                {
                    if (biomes[i + 1] == biomeTypeToCheck) biomeCount++;
                }
                if (i > terrainSize + 1)
                {
                    if (biomes[i - (terrainSize + 1)] == biomeTypeToCheck) biomeCount++;
                }
                if (i > terrainSize - 1)
                {
                    if (biomes[i - (terrainSize - 1)] == biomeTypeToCheck) biomeCount++;
                }

                if (i + terrainSize + 1 < biomes.Length)
                {
                    if (biomes[i + (terrainSize + 1)] == biomeTypeToCheck) biomeCount++;
                }
                if (i + terrainSize - 1 < biomes.Length)
                {
                    if (biomes[i + (terrainSize - 1)] == biomeTypeToCheck) biomeCount++;
                }
                if (biomeCount < 4)
                {
                    biomeFilter[i] = Biomes.Default;
                }
                else
                {
                    biomeFilter[i] = biomeTypeToCheck;
                }
            }
        }
        biomes = biomeFilter;
    }
    private void GenerateMesh()
    {
        for (int i = 0, x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                float y = PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + z, i);
                verticies[i] = new Vector3(playerTransform.position.x + (x - terrainSize / 2), this.transform.position.y + y, playerTransform.position.z + (z - terrainSize / 4));
                i++;
            }
        }

        int vert = 0, tris = 0;
        triangles = new int[terrainSize * terrainSize * 6];
        mountainTriangles = new int[terrainSize * terrainSize * 6];
        for (int z = 0; z < terrainSize; z++)
        {
            for (int x = 0; x < terrainSize; x++)
            {
                if (biomes[vert] == Biomes.Mountain)
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
            return Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
    }
    private float PerlinNoise(float x, float y, int index)
    {
        if (biomes[index] == Biomes.Mountain)
        {
            if(mountainHeightMultiplier > 0)
                return (Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude) * mountainHeightMultiplier;
            else
                return Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
        } else if(biomes[index] == Biomes.River)
        {
            return -(Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude);
        }
        else
        {
            return 0;
        }
    }
}