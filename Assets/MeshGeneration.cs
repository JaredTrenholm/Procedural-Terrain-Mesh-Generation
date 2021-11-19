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

    [Header("Terrain Settings")]
    public int terrainSize = 20;

    [Header("Perlin Settings")]
    [Range(0.001f, .999f)]
    public float frequency = .3f;
    [Range(0.001f, 10f)]
    public float amplitude = 2f;

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
        verticies = new Vector3[(terrainSize+1)*(terrainSize + 1)];

        int zLoopCompletes = 0;
        for (int i = 0, x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                GenerateBiome(this.transform.position.x + x, this.transform.position.z + z, zLoopCompletes);
                float y = GetBiomeHeight(this.transform.position.x + x, this.transform.position.z +z, zLoopCompletes);
                verticies[i] = new Vector3(this.transform.position.x + x, this.transform.position.y + y, this.transform.position.z + z);
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
        float perlin = PerlinNoise(x, y);
        if (perlin <= (int)Biome.Mountain / (int)Biome.BiomeCount)
        {
            biomes[(int)x+(int)y+zLoopCompletes] = Biome.Mountain;
        }
        else if (perlin <= (int)Biome.Valley + (int)Biome.Valley / (int)Biome.BiomeCount)
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
            return PerlinNoise(x,y)*5;
        } else if (biomes[(int)x + (int)y + zLoopCompletes] == Biome.Valley)
        {
            return -PerlinNoise(x, y);
        } else
        {
            return PerlinNoise(x, y);
        }
    }
    private float PerlinNoise(float x, float y)
    {
        return Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
    }
}