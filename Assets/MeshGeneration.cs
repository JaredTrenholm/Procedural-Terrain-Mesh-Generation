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
    public float TerrainResolution = 1f;
    [Range(1, 100)]
    public float vertexSpacing = 10;

    [Header("Perlin Settings")]
    [Range(0.001f, .999f)]
    public float frequency = .3f;
    [Range(0.001f, 10f)]
    public float amplitude = 2f;

    private Biome biome;
    private enum Biome
    {
        Plains,
        Valley,
        Mountain
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
        GenerateMesh();
        UpdateMesh();
    }

    private void GenerateMesh()
    {
        var vertexFieldSize = Mathf.FloorToInt(terrainSize / TerrainResolution);

        verticies = new Vector3[
            (int)Mathf.Pow(vertexFieldSize + 1, 2)
        ];

        var tSizeVector = new Vector3(vertexFieldSize / 2f, 1, vertexFieldSize / 2f) * TerrainResolution;

        for (int i = 0, x = 0; x <= vertexFieldSize; x++)
        {
            for (int z = 0; z <= vertexFieldSize; z++)
            {
                float y = PerlinNoise(this.transform.position.x + x, this.transform.position.z +z);
                verticies[i] = new Vector3(x * vertexSpacing, y, z * vertexSpacing);
                i++;
            }
        }

        int vert = 0, tris = 0;
        triangles = new int[vertexFieldSize * vertexFieldSize * 6];
        for (int z = 0; z < vertexFieldSize; z++)
        {
            for (int x = 0; x < vertexFieldSize; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + vertexFieldSize + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + vertexFieldSize + 1;
                triangles[tris + 5] = vert + vertexFieldSize + 2;

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
            mesh.name = "Generic Mesh Terrain";
        }
        mesh.Clear();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private float PerlinNoise(float x, float y)
    {
        if (biome == Biome.Mountain)
        {
            return Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
        } else if (biome == Biome.Valley)
        {
            return -Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
        } else
        {
            return Mathf.PerlinNoise(x * frequency, y * frequency) * (amplitude*10);
        }
    }
}