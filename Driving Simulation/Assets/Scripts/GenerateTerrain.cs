using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTerrain : MonoBehaviour
{
    public GameObject[] terrainTiles;
    private int tileCount = 35;
    public GameObject myCar;
    private float spawnZ = 0.0f;
    private float tileLength = 60f;
    private float tileWidth = 600f;
    private List<GameObject> tileList;
    private float safeZone = 120;
    
    public GameObject[] trees;
    public GameObject[] smallFauna;
    public GameObject[] birdAreas;

    float grassSize; // the width of each grass part (between terrain edge and road) 

    void Start()
    {
        grassSize = tileWidth / 2 - 10; //road is centered and has a width of 20
        tileList = new List<GameObject>();
        safeZone = tileLength * 2;
        for (int i = 0; i < tileCount; i++)
        {
            SpawnTile();
        }
    }


    void Update()
    {
        if (myCar.transform.position.z - safeZone > (spawnZ - tileCount * tileLength))
        {
            SpawnTile();
            DespawnTile();
        }
    }

    private void SpawnTile()
    {
        GameObject tile;
        tile = Instantiate(terrainTiles[0]) as GameObject;
        tile.transform.SetParent(transform);
        tile.transform.localPosition = Vector3.zero;
        tile.transform.position += Vector3.forward * spawnZ;
        tileList.Add(tile);
        GenerateObjects(tile);
        spawnZ += tileLength;
    }

    private void DespawnTile()
    {
        Destroy(tileList[0]);
        tileList.RemoveAt(0);
    }

    void GenerateObjects(GameObject tile)
    {
        List<GameObject> objects = new List<GameObject>();

        //Trees
        for (int i = 0; i < 50; i++) 
        {
            int spawn = Random.Range(0, 100);
            if (spawn > 60)
            {
                int index = Random.Range(0, trees.Length - 1);
                GameObject newTree = Instantiate(trees[index]) as GameObject;
                newTree.transform.SetParent(tile.transform);
                newTree.transform.localPosition = CalculatePosition();
                newTree.transform.localScale = CalculateScale(newTree.transform.localScale);
                objects.Add(newTree);
            }
        }


        //bushes & mushrooms
        for (int i = 0; i < 100; i++) 
        {
            int spawn = Random.Range(0, 100);
            if (spawn > 30)
            {
                int index = Random.Range(0, smallFauna.Length - 1);
                GameObject newFauna = Instantiate(smallFauna[index]) as GameObject;
                newFauna.transform.SetParent(tile.transform);
                newFauna.transform.localPosition = CalculatePosition();
                newFauna.transform.localScale = CalculateScale(newFauna.transform.localScale);
            }
        }

        //birdAreas
        
        for (int i = 0; i < 20; i++) 
        {
            GameObject newArea = Instantiate(birdAreas[0]) as GameObject;
            newArea.transform.SetParent(tile.transform);
            newArea.transform.localPosition = CalculatePosition();
        }
    }

    Vector3 CalculatePosition() 
    {
        Vector3 pos;
        if (Random.Range(0, 100) > 50) //spread items evenly to both sides of the road
        {
            pos = new Vector3(Random.Range(0, grassSize), 0, Random.Range(0, tileLength));
        }
        else
        {
            pos = new Vector3(Random.Range(tileWidth - grassSize, tileWidth), 0, Random.Range(0, tileLength));
        }
        return pos;
    }


    Vector3 CalculateScale(Vector3 scale) 
    {
        float factor = Random.Range(0.9f, 1.1f);
        return new Vector3(factor * scale.x, factor * scale.y, factor * scale.z); 
    }

}
