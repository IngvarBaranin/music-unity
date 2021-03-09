using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class TerrainGenerator : MonoBehaviour
{

    public Transform playerTransform;
    private float playerPosX;
    
    private int[,] mapArray;
    private Tilemap tilemap;

    private TileBase currentTile;
    public TileBase TileBiome1;
    public TileBase TileBiome2;
    public TileBase TileBiome3;
    
    public int terrainWidth, terrainHeight;
    
    public int lastX = 0;
    public int lastY = 0;


    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        currentTile = TileBiome1;

        for (int i = 0; i < 10; i++)
        {
            GenerateTerrain();
        }
    }

    private void Update()
    {
        if (playerTransform.position.x > (lastX - (terrainWidth / 2)))
        {
            GenerateTerrain();
        }
    }

    void GenerateTerrain()
    {
        float seed = Random.Range(.1f, 1f);
        
        mapArray = GenerateArray(terrainWidth, terrainHeight, true);
        mapArray = RandomWalkTopSmoothed(mapArray, seed, 5);
        RenderMap(mapArray);

        lastX += terrainWidth;
    }
    
    public int[,] GenerateArray(int width, int height, bool empty)
    {
        int[,] map = new int[width, height];
        for (int x = 0; x < map.GetUpperBound(0); x++)
        {
            for (int y = 0; y < map.GetUpperBound(1); y++)
            {
                if (empty)
                {
                    map[x, y] = 0;
                }
                else
                {
                    map[x, y] = 1;
                }
            }
        }
        return map;
    }
    
    public void RenderMap(int[,] map)
    {
        //Clear the map (ensures we dont overlap)
        //tilemap.ClearAllTiles(); 
        //Loop through the width of the map

        int currentBiomeWidth = 0;
        int allowedBiomeWidth = 10;

        Vector3Int[] positions = new Vector3Int[terrainWidth * terrainHeight];
        TileBase[] tileArray = new TileBase[terrainWidth * terrainHeight];
        
        for (int x = 0; x < map.GetUpperBound(0) ; x++) 
        {

            if (currentBiomeWidth > allowedBiomeWidth)
            {
                PickRandomBiome();
                currentBiomeWidth = 0;
                allowedBiomeWidth = Random.Range(allowedBiomeWidth - 2, allowedBiomeWidth + 2);
            }
            
            //Loop through the height of the map
            for (int y = 0; y < map.GetUpperBound(1); y++) 
            {
                positions[terrainWidth*y + x] = new Vector3Int(x + lastX, y, 0);
                // 1 = tile, 0 = no tile
                if (map[x, y] == 1) 
                {
                    tileArray[terrainWidth*y + x] = currentTile;
                }
            }
            currentBiomeWidth += 1;
        }
        
        tilemap.SetTiles(positions, tileArray);
    }

    public void PickRandomBiome()
    {
        switch (Random.Range(0, 3))
        {
            case 0:
                currentTile = TileBiome1;
                break;
            case 1:
                currentTile = TileBiome2;
                break;
            case 2:
                currentTile = TileBiome3;
                break;
        }
    }
    
    public int[,] RandomWalkTopSmoothed(int[,] map, float seed, int minSectionWidth)
    {
        //Seed our random
        System.Random rand = new System.Random(seed.GetHashCode());

        //Determine the start position
        int lastHeight = lastY;

        //Used to determine which direction to go
        int nextMove = 0;
        //Used to keep track of the current sections width
        int sectionWidth = 0;
        
        bool drewPlatform = false;

        //Work through the array width
        for (int x = 0; x <= map.GetUpperBound(0); x++)
        {
            
            if (drewPlatform && rand.Next(6) == 0)
            {
                drewPlatform = false;
                continue;
            }

            //Determine the next move
            nextMove = rand.Next(2);

            //Only change the height if we have used the current height more than the minimum required section width
            if (nextMove == 0 && lastHeight > 0 && sectionWidth > minSectionWidth)
            {
                lastHeight--;
                sectionWidth = 0;
            }
            else if (nextMove == 1 && lastHeight < map.GetUpperBound(1) && sectionWidth > minSectionWidth)
            {
                lastHeight++;
                sectionWidth = 0;
            }
            //Increment the section width
            sectionWidth++;

            //Work our way from the height down to 0
            for (int y = lastHeight; y >= 0; y--)
            {
                if (y != lastHeight && rand.Next(0, 4) == 0)
                {
                    continue;
                }
                
                map[x, y] = 1;
            }
            
            drewPlatform = true;
        }

        lastY = lastHeight;
        //Return the modified map
        return map;
    }
    
}
