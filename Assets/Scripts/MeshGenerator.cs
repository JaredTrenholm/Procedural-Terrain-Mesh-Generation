using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BiomeGenerator))]
public class MeshGenerator : MonoBehaviour
{
    [Header("Object Settings")]
    public Transform playerTransform;
    public GameObject waterPlane;
    public BiomeGenerator biomes;
    public float distanceDenomintator;

    [Header("Terrain Settings")]
    public int terrainSize = 20;
    public float waterLevel;

    private Mesh mesh;
    private Vector3[] verticies;
    private Vector2[] uvs;
    private int[] triangles;
    private int[] mudTriangles;
    private int[] stoneTriangles;
    private int[] snowTriangles;
    private Vector3 midPoint;
    private int subMeshCount;
    private float cameraCenterX;
    private float cameraCenterZ;

    private void Start()
    {
        subMeshCount = GetComponent<MeshRenderer>().materials.Length;
        midPoint = playerTransform.position;
        cameraCenterX = terrainSize / 2;
        cameraCenterZ = terrainSize / 4;
        CreateTerrain();
    }
    private void Update()
    {
        if (Vector3.Distance(playerTransform.position, midPoint) > terrainSize/ distanceDenomintator)
            CreateTerrain();
    }

    public void CreateTerrain()
    {
        midPoint = playerTransform.position;
        verticies = new Vector3[(terrainSize + 1) * (terrainSize + 1)];
        uvs = new Vector2[verticies.Length];
        biomes.Init(playerTransform, terrainSize, waterLevel);
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
                float y = biomes.PopulateHeight(Mathf.FloorToInt(x), Mathf.FloorToInt(z));

                verticies[i] = new Vector3(playerTransform.position.x + (x - cameraCenterX), this.transform.position.y + y, playerTransform.position.z + (z - cameraCenterZ));
                uvs[i] = new Vector2((float)x / terrainSize, (float)z / terrainSize);
                i++;
            }
        }


        int vert = 0, tris = 0;
        triangles = new int[terrainSize * terrainSize * 6];
        mudTriangles = new int[terrainSize * terrainSize * 6];
        stoneTriangles = new int[terrainSize * terrainSize * 6];
        snowTriangles = new int[terrainSize * terrainSize * 6];
        for (int x = 0; x < terrainSize; x++)
        {
            for (int z = 0; z < terrainSize; z++)
            {
                if (biomes.IsMud(x,z))
                {
                    AddTriangle(mudTriangles, tris, vert);
                } else if (biomes.IsMountain(x, z))
                {
                    AddTriangle(stoneTriangles, tris, vert);
                } else if (biomes.IsSnow(x, z))
                {
                    AddTriangle(snowTriangles, tris, vert);
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
        Array.Reverse(mudTriangles);
        Array.Reverse(stoneTriangles);
        Array.Reverse(snowTriangles);
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
        mesh.subMeshCount = subMeshCount;
        mesh.SetTriangles(triangles, 0);
        mesh.SetTriangles(mudTriangles, 1);
        mesh.SetTriangles(stoneTriangles, 2);
        mesh.SetTriangles(snowTriangles, 3);
        mesh.uv = uvs;

        GetComponent<MeshFilter>().mesh = mesh;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    private void UpdateWater()
    {
        float defaultScale = 1f;
        waterPlane.transform.position = new Vector3(playerTransform.transform.position.x, this.transform.position.y + waterLevel, playerTransform.transform.position.z + cameraCenterZ);
        waterPlane.transform.localScale = new Vector3(terrainSize, terrainSize, defaultScale);
    }
}