using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BiomeGenerator))]
public class MeshGenerator : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] verticies;
    private int[] triangles;
    private int[] mountainTriangles;

    [Header("Object Settings")]
    public Transform playerTransform;
    public GameObject waterPlane;
    public BiomeGenerator biomes;

    [Header("Terrain Settings")]
    public int terrainSize = 20;
    public float waterLevel;

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
        biomes.Init(playerTransform, terrainSize);
        GenerateMesh();
        UpdateMesh();
        UpdateWater();
    }

    private void GenerateMesh()
    {
        for (int i = 0, x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                float y = biomes.GetBiomeHeight(x, z);
                float cameraCenterX = terrainSize / 2;
                float cameraCenterZ = terrainSize / 4;

                verticies[i] = new Vector3(playerTransform.position.x + (x - cameraCenterX), this.transform.position.y + y, playerTransform.position.z + (z - cameraCenterZ));
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
                if (biomes.IsMountainXAxis(x,z)&& biomes.IsMountainZAxis(x, z))
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

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    private void UpdateWater()
    {
        waterPlane.transform.position = new Vector3(playerTransform.transform.position.x, this.transform.position.y + waterLevel, playerTransform.transform.position.z + (terrainSize / 4));
        waterPlane.transform.localScale = new Vector3(terrainSize, terrainSize, 1f);
    }
    

    
}