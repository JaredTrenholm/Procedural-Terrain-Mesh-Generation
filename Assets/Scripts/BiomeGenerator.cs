using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshGenerator))]
public class BiomeGenerator : MonoBehaviour
{
    private Biomes[,] biomes;
    private float terrainSize;
    [Header("Biome Settings")]
    public float requiredNeighbors;
    public float mountainHeight;

    public enum Biomes
    {
        Grass,
        River,
        Mud,
        Mountain,
        Length
    }
    public void Init(float size)
    {
        terrainSize = size;
        biomes = new Biomes[(int)terrainSize + 1, (int)terrainSize + 1];
    }
    public void GenerateBiome(int x, float y, int z)
    {
        if (y >= this.transform.position.y + mountainHeight)
        {
            biomes[x, z] = Biomes.Mountain;
        } else if (y <= this.transform.position.y)
        {
            biomes[x, z] = Biomes.River;
        }
        else
        {
            biomes[x, z] = Biomes.Grass;
        }


        for (int i = 0; i < (int)Biomes.Length; i++)
        {
            SmoothBiomes((Biomes)i);
        }
    }
    public bool IsMountain(int x, int z){ return biomes[x, z] == Biomes.Mountain; }
    public bool IsRiver(int x, int z) { return biomes[x, z] == Biomes.River; }
    private void SmoothBiomes(Biomes biome)
    {
        int defaultBiome = 0;
        for (int x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                if (biomes[x, z] == biome)
                    if (GetSameBiomeNeighborCount(x, z, biome) < requiredNeighbors)
                    {
                        biomes[x, z] = (Biomes)defaultBiome;
                    }
            }
        }
    }

    private int GetSameBiomeNeighborCount(int x, int z, Biomes biomeType)
    {
        int biomeCount = 0;
        if (x > 0)
        {
            if (biomes[x - 1, z] == biomeType) biomeCount++;
        }
        else return (int)requiredNeighbors;
        if (x < terrainSize) { 
            if (biomes[x + 1, z] == biomeType) biomeCount++;
        }
        else return (int)requiredNeighbors;
        if (z > 0) { 
            if (biomes[x, z - 1] == biomeType) biomeCount++;
        }
        else return (int)requiredNeighbors;
        if (z < terrainSize) { 
            if (biomes[x, z + 1] == biomeType) biomeCount++;
        }
        else return (int)requiredNeighbors;
        if (x > 0 && z > 0) { 
            if (biomes[x - 1, z - 1] == biomeType) biomeCount++;
        }
        else return (int)requiredNeighbors;
        if (x < terrainSize && z > 0) { 
            if (biomes[x + 1, z - 1] == biomeType) biomeCount++;
        }
        else return (int)requiredNeighbors;
        if (x > 0 && z < terrainSize) { 
            if (biomes[x - 1, z + 1] == biomeType) biomeCount++;
        }
        else return (int)requiredNeighbors;
        if (x < terrainSize && z < terrainSize) { 
            if (biomes[x + 1, z + 1] == biomeType) biomeCount++;
        }
        else return (int)requiredNeighbors;
        return biomeCount;
    }   
}
