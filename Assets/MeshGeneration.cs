using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshGeneration : MonoBehaviour
{
    public int width;
    public int length;
    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Biome biomeType;

    private enum Biome
    {
        Plains,
        Pond,
        ForestHigh,
        Swamp
    }
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        CreateShape();
        UpdateMesh();
    }

    void CreateShape()
    {
        vertices = new Vector3[(width + 1) * (length + 1)];

        int vertexCount = 0;
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                vertices[vertexCount] = new Vector3((this.transform.position.x - width / 2) + x, PerlinNoise(x, z), (this.transform.position.z - length / 2) + z);
                vertexCount++;
            }
        }

        triangles = new int[length * width * 6];
        int verticeBeingUsed = 0;
        int triangle = 0;
        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {
                triangles[triangle] = verticeBeingUsed;
                triangles[triangle + 1] = verticeBeingUsed + 1 + width;
                triangles[triangle + 2] = verticeBeingUsed + 1;
                triangles[triangle + 3] = verticeBeingUsed + 1;
                triangles[triangle + 4] = verticeBeingUsed + 1 + width;
                triangles[triangle + 5] = verticeBeingUsed + 2 + width;
                verticeBeingUsed++;
                triangle += 6;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }

    private float PerlinNoise(float x, float y)
    {
        float perlin = Mathf.PerlinNoise(x / width, y / width);
        perlin = Mathf.Round(perlin);

        if (biomeType == Biome.Pond || biomeType == Biome.Swamp)
        {
            return -perlin;
        }
        else
        {
            return perlin;
        }
    }
}