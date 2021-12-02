using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BiomeGenerator))]
public class MeshGenerator : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] verticies;
    private Vector2[] uvs;
    private int[] grassTriangles;
    private int[] mudTriangles;
    private int[] stoneTriangles;
    private Vector3 midPoint;

    [Header("Object Settings")]
    public Transform playerTransform;
    public GameObject waterPlane;
    public BiomeGenerator biomes;
    public float distanceDenomintator;

    [Header("Terrain Settings")]
    public int terrainSize = 20;
    public float waterLevel;
    [Range(1f, 20f)]
    public float vertexSpacing = 1;

    [Header("Perlin Settings")]
    [Range(0.001f, .999f)]
    public float frequency = .3f;
    [Range(1f, 20f)]
    public float amplitude = 2f;

    private void Start()
    {
        midPoint = playerTransform.position;
        biomes.Init(terrainSize);
        CreateTerrain();
    }
    private void Update()
    {
       if (Vector3.Distance(playerTransform.position, midPoint) > terrainSize*vertexSpacing/ distanceDenomintator)
            CreateTerrain();
    }

    public void CreateTerrain()
    {
        midPoint = playerTransform.position;
        verticies = new Vector3[(terrainSize + 1) * (terrainSize + 1)];
        uvs = new Vector2[verticies.Length];
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
                float y = PerlinNoise(playerTransform.position.x + x, playerTransform.position.z + z);
                float cameraCenterX = terrainSize * vertexSpacing / distanceDenomintator;
                float cameraCenterZ = terrainSize * vertexSpacing / distanceDenomintator;

                verticies[i] = new Vector3(playerTransform.position.x + ((x*vertexSpacing) - cameraCenterX), this.transform.position.y + y, playerTransform.position.z + ((z * vertexSpacing) - cameraCenterZ));
                biomes.GenerateBiome(x,y,z);
                uvs[i] = new Vector2((float)x / terrainSize, (float)z / terrainSize);
                i++;
            }
        }

        int vert = 0, tris = 0;
        grassTriangles = new int[terrainSize * terrainSize * 6];
        mudTriangles = new int[terrainSize * terrainSize * 6];
        stoneTriangles = new int[terrainSize * terrainSize * 6];
        for (int x = 0; x < terrainSize; x++)
        {
            for (int z = 0; z < terrainSize; z++)
            {
                if (biomes.IsMountain(x,z))
                {
                    AddTriangle(stoneTriangles, tris, vert);
                } else if (biomes.IsRiver(x, z))
                {
                    AddTriangle(mudTriangles, tris, vert);
                }
                else
                {
                    AddTriangle(grassTriangles, tris, vert);
                }
                
                vert++;
                tris += 6;
            }
            vert++;
        }
        Array.Reverse(grassTriangles); // so that they face upwards
        Array.Reverse(mudTriangles);
        Array.Reverse(stoneTriangles);
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
        mesh.subMeshCount = 3;
        mesh.SetTriangles(grassTriangles, 0);
        mesh.SetTriangles(mudTriangles, 1);
        mesh.SetTriangles(stoneTriangles, 2);
        mesh.uv = uvs;

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    private void UpdateWater()
    {
        float centerZ = (terrainSize*vertexSpacing) / 4;
        float defaultScale = 1f;
        waterPlane.transform.position = new Vector3(playerTransform.transform.position.x, this.transform.position.y + waterLevel, playerTransform.transform.position.z + centerZ);
        waterPlane.transform.localScale = new Vector3(terrainSize*vertexSpacing, terrainSize * vertexSpacing, defaultScale);
    }
    private float PerlinNoise(float x, float y)
    {
        return Mathf.Abs(Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude);
    }



}