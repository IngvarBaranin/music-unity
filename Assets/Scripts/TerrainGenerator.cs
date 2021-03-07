using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerrainGenerator : MonoBehaviour
{
    private int[,] mapArray;
    private Tilemap tilemap;

    public TileBase tile;
    public int terrainWidth, terrainHeight;
    
    public int lastX, lastY = 0;
    
    
    // Start is called before the first frame update
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Generating");
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
        for (int x = 0; x < map.GetUpperBound(0) ; x++) 
        {
            //Loop through the height of the map
            for (int y = 0; y < map.GetUpperBound(1); y++) 
            {
                // 1 = tile, 0 = no tile
                if (map[x, y] == 1) 
                {
                    tilemap.SetTile(new Vector3Int(x + lastX, y, 0), tile); 
                }
            }
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
                Debug.Log(x + " " + y);
                map[x, y] = 1;
            }
            
            drewPlatform = true;
        }

        lastY = lastHeight;
        //Return the modified map
        return map;
    }
    
}
