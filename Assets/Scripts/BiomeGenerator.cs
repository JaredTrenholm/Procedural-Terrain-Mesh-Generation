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
    private Transform playerTransform; 
    [Header("Biome Settings")]
    public BiomeWeights[] weights;
    public float minimumMountainHeight;
    public float riverDepthMultiplier;
    public float requiredNeighbors;

    [Header("Perlin Settings")]
    [Range(0.001f, .999f)]
    public float frequency = .3f;
    [Range(1f, 10f)]
    public float amplitude = 2f;
    public enum Biomes
    {
        Plains,
        River,
        Mountain
    }
    [Serializable]
    public struct BiomeWeights
    {
        [Range(0, 100)]
        public float weight;
        public Biomes biome;
        public Biomes removeBiome;
    }
    public void Init(Transform transform, float size)
    {
        playerTransform = transform;
        terrainSize = size;
        biomes = new Biomes[(int)terrainSize + 1, (int)terrainSize + 1];
        GenerateBiomes();
    }
    public float GetBiomeHeight(float x, float z)
    {
        if (biomes[(int)x, (int)z] == Biomes.Mountain)
        {
            float perlin = this.transform.position.y + PerlinNoise(x + playerTransform.position.x, z + playerTransform.position.z) + minimumMountainHeight;
            if (GetSameBiomeNeighborCount((int)x, (int)z, Biomes.Mountain) == 8)
                perlin += PerlinNoise(x + playerTransform.position.x, z + playerTransform.position.z);
            return perlin;
        }
        else if (biomes[(int)x, (int)z] == Biomes.River)
        {
            return this.transform.position.y - PerlinNoise(x + playerTransform.position.x, z + playerTransform.position.z) * riverDepthMultiplier;
        }
        else
        {
            return this.transform.position.y + PerlinNoise(x + playerTransform.position.x, z + playerTransform.position.z);
        }
    }
    public bool IsMountainXAxis(float x, float z)
    {
        bool isMountain = false;
        
        if(biomes[(int)x, (int)z] == Biomes.Mountain)
        {
            if (x + 1 < terrainSize)
            {
                if (biomes[(int)x+1, (int)z] == Biomes.Mountain && GetSameBiomeNeighborCount((int)x, (int)z,Biomes.Mountain) >= requiredNeighbors)
                {
                    isMountain = true;  
                }
                else
                {
                    isMountain = false;
                }
            } else
            {
                isMountain = false;
            }
        }

        return isMountain;
    }
    public bool IsMountainZAxis(float x, float z)
    {
        bool isMountain = false;

        if (biomes[(int)x, (int)z] == Biomes.Mountain)
        {
            if (z + 1 < terrainSize)
            {
                if (biomes[(int)x , (int)z + 1] == Biomes.Mountain && GetSameBiomeNeighborCount((int)x, (int)z, Biomes.Mountain) >= requiredNeighbors)
                {
                    isMountain = true;
                }
                else
                {
                    isMountain = false;
                }
            }
            else
            {
                isMountain = false;
            }
        }

        return isMountain;
    }
    private void GenerateBiomes()
    {
        for (int x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                foreach (BiomeWeights biomeWeight in weights)
                {
                    float perlin = Mathf.PerlinNoise((playerTransform.position.x + x) * frequency, (playerTransform.position.z + z) * frequency) * 100;
                    if (perlin <= biomeWeight.weight && biomes[x, z] == biomeWeight.removeBiome)
                        biomes[x, z] = biomeWeight.biome;
                }
            }
        }
        SmoothBiomes();
    }
    private void SmoothBiomes()
    {
        for (int x = 0; x <= terrainSize; x++)
        {
            for (int z = 0; z <= terrainSize; z++)
            {
                for (int i = 0; i < weights.Length; i++) { 
                    if (biomes[x, z] == weights[i].biome)
                        if (GetSameBiomeNeighborCount(x, z, weights[i].biome) < requiredNeighbors)
                        {
                            biomes[x, z] = weights[i].removeBiome;
                        }
                }
            }
        }
    }

    private int GetSameBiomeNeighborCount(int x, int z, Biomes biomeType)
    {
        int biomeCount = 0;
        if (x > 0)
            if (biomes[x - 1, z] == biomeType) biomeCount++;
        if (x < terrainSize)
            if (biomes[x + 1, z] == biomeType) biomeCount++;
        if (z > 0)
            if (biomes[x, z - 1] == biomeType) biomeCount++;
        if (z < terrainSize)
            if (biomes[x, z + 1] == biomeType) biomeCount++;
        if (x > 0 && z > 0)
            if (biomes[x - 1, z - 1] == biomeType) biomeCount++;
        if (x < terrainSize && z > 0)
            if (biomes[x + 1, z - 1] == biomeType) biomeCount++;
        if (x > 0 && z < terrainSize)
            if (biomes[x - 1, z + 1] == biomeType) biomeCount++;
        if (x < terrainSize && z < terrainSize)
            if (biomes[x + 1, z + 1] == biomeType) biomeCount++;
        return biomeCount;
    }
    private float PerlinNoise(float x, float y)
    {
        return Mathf.Abs(Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude);
    }
}
