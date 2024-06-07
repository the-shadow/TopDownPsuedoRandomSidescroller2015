/*
 * Code by Ryan Jackson
 * */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction
{
    Up = 0,
    Down,
    Left,
    Right,
    TOTAL
}

public enum LandTileBorder
{
    EMPTY = 0,
    North = 1,
    South = 2,
    East = 4,
    West = 8
}

public enum TileType
{
    Grass = 0,
    Snow,
    Water,
    RuggedGrass,
    Sand,
    TOTAL
}

public enum TileContents
{
    EMPTY = 0,
    Mountain,
    MountainInterior,
    Forest,
    Water,
    Dungeon,
    Flower,
    Cactus
}

public enum RegionType
{
    Grasslands = 0,
    Snowy,
    Rugged,
    Desert,
    TOTAL
}

public struct TileData
{
    TileType tileType;
    TileContents tileContents;
    LandTileBorder landTileBorderMask;

    public TileType TileType
    {
        get { return tileType; }
        set { tileType = value; }
    }

    public TileContents TileContents
    {
        get { return tileContents; }
        set { tileContents = value; }
    }

    public LandTileBorder LandTileBorderMask
    {
        get { return landTileBorderMask; }
        set { landTileBorderMask = value; }
    }
};

public class WorldMap : MonoBehaviour {

    TileData[,] WorldTileArray;
    BoxCollider2D bottomBoundary;
    BoxCollider2D topBoundary;
    Vector3 bottomCenter;
    Vector3 topCenter;
    Vector3 firstTile;

    //BottomLeft Tile
    public Vector3 FirstTile
    {
        get { return firstTile; }
        set { firstTile = value; }
    }

    [Header("Darkness")]
    public IndoorDarknessTrigger darknessTrigger;

    [Header("Region")]
    public RegionType regionType;
    public RegionTrigger regionTrigger;
    public int RegionID = 0;

    [Header("Map Attributes")]
    public int Height = 50;
    public int Width = 500;

    public float TileSize = 1f;

    public int MapEdgeRows = 5;
    public int RegionBuffer = 3;

    //[Header("Prefabs")]
    //[Header("Grass")]
    //public GameObject GrassTile_1;
    //public GameObject GrassTile_2;
    //public GameObject GrassTile_Dirt1;
    //public GameObject GrassTile_Dirt2;
    //public GameObject GrassTile_Dirt3;
    //public GameObject GrassEdge_North;
    //public GameObject GrassEdge_South;
    //public GameObject GrassEdge_East;
    //public GameObject GrassEdge_West;
    //public GameObject GrassEdge_NorthWest;
    //public GameObject GrassEdge_NorthEast;
    //public GameObject GrassEdge_SouthWest;
    //public GameObject GrassEdge_SouthEast;
    //public GameObject GrassEdge_NorthSouth;
    //public GameObject GrassEdge_EastWest;
    //public GameObject GrassEdge_NorthEastSouth;
    //public GameObject GrassEdge_NorthEastWest;
    //public GameObject GrassEdge_NorthWestSouth;
    //public GameObject GrassEdge_SouthEastWest;
    //public GameObject GrassEdge_NorthSouthEastWest;
    //[Header("Flowers")]
    //public GameObject Flower_1;
    //public GameObject Flower_2;
    //public GameObject Flower_3;
    //[Header("Water")]
    //public GameObject WaterTile;
    //[Header("Mountains")]
    //public GameObject Mountain;
    //public GameObject MountainHigh;
    //[Header("Trees")]
    //public GameObject Tree;

    //[Header("POI Prefabs")]
    //public GameObject StartingSanctuary;

    [Header("Scenery Chances")]
    [Range(20, 80)]
    public int ForestSpawnChance;
    [Range(30, 70)]
    public int MountainSpawnChance;
    [Range(0, 100)]
    public int MountainInteriorChance;
    [Range(0, 100)]
    public int WaterSpawnChance;

    [Header("Grass")]
    [Header("Land Tile Chances")]
    [Range(0, 100)]
    public int grass1 = 83;
    [Range(0, 100)]
    public int grass2 = 14;
    [Range(0, 100)]
    public int grassdirt1 = 1;
    [Range(0, 100)]
    public int grassdirt2 = 1;
    [Range(0, 100)]
    public int grassdirt3 = 1;
    [Header("Rugged")]
    [Range(0, 100)]
    public int rugged1 = 50;
    [Range(0, 100)]
    public int rugged2 = 50;
    [Header("Rugged")]
    [Range(0, 100)]
    public int sand1 = 50;
    [Range(0, 100)]
    public int sand2 = 50;
    [Header("Snow")]
    [Range(0, 100)]
    public int snow1 = 83;
    [Range(0, 100)]
    public int snow2 = 14;
    [Range(0, 100)]
    public int snow3 = 3;

    [Header("River Stats")]
    public int MinRivers = 0;
    public int MaxRivers = 30;
    public int MinRiverSteps = 10;
    public int MaxRiverSteps = 27;
    public int TotalRivers = 0;

    [Header("Lake Stats")]
    public int MinLakes = 0;
    public int MaxLakes = 15;
    public int MinLakeSteps = 1;
    public int MaxLakeSteps = 4;
    public int TotalLakes = 0;

    [Header("Decoration Stats")]
    public int DecorationMax = 100;

    int SpawnDelay;

    //string seed;
    //System.Random prng;

    [Header("World Name")]
    public string WorldName;
    // Use this for initialization
    void Start()
    {
        //Init();
    }

    public void InitMap(Vector3 startingTile, int regionID, RegionType region, int tilesAcross, int tilesHigh)
    {
        Width = tilesAcross;
        Height = tilesHigh;
        regionType = region;
        RegionID = regionID;
        regionTrigger = WorldManager.Instance.regionTrigger;
        darknessTrigger = WorldManager.Instance.darknessTrigger;

        firstTile = startingTile;

        Init();
    }

    void Init()
    {
        WorldTileArray = new TileData[Width, Height];

        //InitRng(WorldName);

        SetMap();

        SetMapBoundaries();

        CreateMountainClusters();

        if(regionType != RegionType.Desert)
            CreateForestClusters();

        if (regionType == RegionType.Desert)
            ScatterCacti();

        if(regionType != RegionType.Desert)
            CreateMountainInteriorCluster();

        if (regionType != RegionType.Snowy && regionType != RegionType.Desert)
        {
            CreateLakes();

            CreateRivers();
        }

        CreatePointsOfInterest();

        //Before we instantiate, remove scenery that is colliding with Points of Interest
        StampOutScenery();

        if(regionType != RegionType.Snowy)
            CreateWaterBorders();

        if(regionType != RegionType.Desert)
            CreateDecorations();

        SetRegionTrigger();

        SetDarknessTrigger();

        InstantiateMap();

        InstantiateScenery();

        
    }
	
    //void InitRng(string stringToSeed)
    //{
    //    seed = stringToSeed;
    //    prng = new System.Random(seed.GetHashCode());
    //}

    void CreatePointsOfInterest()
    {
        //first one is the starting sanctuary which is not instantiated....
        Vector3 SanctuarySpawnLocation = new Vector3(30 * TileSize,
           (Height * .5f) * TileSize,
           0f);

        WorldManager.Instance.StartingSanctuary.transform.position = SanctuarySpawnLocation;
    }

    void SetRegionTrigger()
    {
        //if this is the first region, that has the starting sanctuary, the region trigger is placed in front of the starting sanctuary.
        
        regionTrigger.regionType = regionType;

        if (RegionID == 0)
            regionTrigger.transform.position = new Vector3(WorldManager.Instance.StartingSanctuary.transform.position.x + 28f, Height * .5f, 0f);
        else
            regionTrigger.transform.position = new Vector3(0f, Height * .5f, 0f);
        
    }

    void SetDarknessTrigger()
    {
        //starting region
        if(RegionID == 0)
        {
            darknessTrigger.setToDark = false;
            darknessTrigger.transform.position = new Vector3(WorldManager.Instance.StartingSanctuary.transform.position.x + 28f, Height * .5f, 0f);
        }
    }

    void StampOutScenery()
    {
        //for each point of interest...
        //go through each set of dimensions and set the tilecontents to empty
        Dimensions currentDimensions = WorldManager.Instance.StartingSanctuary.GetComponent<Dimensions>();
        //find the tile relative start point
        Vector3 startPoint = currentDimensions.transform.position;
        for (int nArea = 0; nArea < currentDimensions.Areas.Count; nArea++)
        {
            int xCoord = (int)(currentDimensions.Areas[nArea].TopLeftCoords.x + startPoint.x);
            int yCoord = (int)(currentDimensions.Areas[nArea].TopLeftCoords.y + startPoint.y);

           //for each area, go to the topleft tile and set tilecontents to empty up to width and height
            for (int nRow = 0; nRow < currentDimensions.Areas[nArea].Width; nRow++)
                for(int nColumn = 0; nColumn < currentDimensions.Areas[nArea].Height; nColumn++)
                {
                    WorldTileArray[xCoord + nRow, yCoord - nColumn].TileContents = TileContents.EMPTY;
                    WorldTileArray[xCoord + nRow, yCoord - nColumn].TileType = RegionBaseTileHelper();
                }
        }


    }
    
    TileData CreateTileData(TileType tileType, TileContents tileContents)
    {
        TileData tileData = new TileData();

        tileData.TileType = tileType;
        tileData.TileContents = tileContents;

        return tileData;
    }

    void SetMap()
    {
        firstTile = Vector3.zero;

        TileData tileDataToAdd;
        for (int _width = 0; _width < Width; _width++)
            for (int _height = 0; _height < Height; _height++)
            {
                tileDataToAdd = CreateTileData(RegionBaseTileHelper(), TileContents.EMPTY);

                WorldTileArray[_width, _height] = tileDataToAdd;
            }
    }

    TileType RegionBaseTileHelper()
    {
        switch(regionType)
        {
            case RegionType.Grasslands:
                {
                    return TileType.Grass;
                }
            case RegionType.Snowy:
                {
                    return TileType.Snow;
                }
            case RegionType.Rugged:
                {
                    return TileType.RuggedGrass;
                }
            case RegionType.Desert:
                {
                    return TileType.Sand;
                }

        }

        return TileType.Grass;
    }
    void InstantiateMap()
    {
        firstTile = Vector3.zero;

        GameObject tileToCreate;

        for (int _width = 0; _width < Width; _width++)
            for (int _height = 0; _height < Height; _height++)
            {
                tileToCreate = CreateTile(WorldTileArray[_width, _height].TileType, _width, _height);

                if (tileToCreate == null)
                    continue;

                tileToCreate.transform.parent = transform;
            }
    }

    GameObject CreateTile(TileType type, int width, int height)
    {
        switch (type)
        {
            case TileType.Grass:
                {
                    int grassTile = WorldManager.Instance.PRNG.Next(0, 100);

                    if (WorldTileArray[width, height].TileContents != TileContents.EMPTY || grassTile <= grass1)
                        return Instantiate(WorldManager.Instance.GrassTile_1, new Vector3(
                            firstTile.x + width * TileSize,
                            firstTile.y + height * TileSize,
                            0f), Quaternion.identity) as GameObject;
                    else if (grassTile > grass1 && grassTile <= grass1 + grass2)
                        return Instantiate(WorldManager.Instance.GrassTile_2, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;
                    else if (grassTile > grass1 + grass2 && grassTile <= grass1 + grass2 + grassdirt1)
                        return Instantiate(WorldManager.Instance.GrassTile_Dirt1, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;
                    else if (grassTile > grass1 + grass2 + grassdirt1 && grassTile <= grass1 + grass2 + grassdirt1 + grassdirt2)
                        return Instantiate(WorldManager.Instance.GrassTile_Dirt2, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;
                    else if (grassTile > grass1 + grass2 + grassdirt1 + grassdirt2 && grassTile <= grass1 + grass2 + grassdirt1 + grassdirt2 + grassdirt3)
                        return Instantiate(WorldManager.Instance.GrassTile_Dirt3, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;


                    break;
                }
            case TileType.RuggedGrass:
                {
                    int ruggedTile = WorldManager.Instance.PRNG.Next(0, 100);

                    if (WorldTileArray[width, height].TileContents != TileContents.EMPTY || ruggedTile <= rugged1)
                        return Instantiate(WorldManager.Instance.RuggedGrassTile_1, new Vector3(
                            firstTile.x + width * TileSize,
                            firstTile.y + height * TileSize,
                            0f), Quaternion.identity) as GameObject;
                    else if (ruggedTile > rugged1 && ruggedTile <= rugged1 + rugged2)
                        return Instantiate(WorldManager.Instance.RuggedGrassTile_2, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                    break;
                }
            case TileType.Sand:
                {
                    int sandTile = WorldManager.Instance.PRNG.Next(0, 100);

                    if (WorldTileArray[width, height].TileContents != TileContents.EMPTY || sandTile <= sand1)
                        return Instantiate(WorldManager.Instance.SandTile_1, new Vector3(
                            firstTile.x + width * TileSize,
                            firstTile.y + height * TileSize,
                            0f), Quaternion.identity) as GameObject;
                    else if (sandTile > sand1 && sandTile <= sand1 + sand2)
                        return Instantiate(WorldManager.Instance.SandTile_2, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                    break;
                }
            case TileType.Water:
                {
                    GameObject border = GetRegionBasedLandTileBorder(width, height);
                    if (border)
                        border.transform.parent = transform;

                    return Instantiate(WorldManager.Instance.WaterTile, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;
                    //break;
                }
            case TileType.Snow:
                {
                    int snowTile = WorldManager.Instance.PRNG.Next(0, 100);

                    if (WorldTileArray[width, height].TileContents != TileContents.EMPTY || snowTile <= snow1)
                        return Instantiate(WorldManager.Instance.SnowTile_1, new Vector3(
                            firstTile.x + width * TileSize,
                            firstTile.y + height * TileSize,
                            0f), Quaternion.identity) as GameObject;
                    else if (snowTile > grass1 && snowTile <= grass1 + grass2)
                        return Instantiate(WorldManager.Instance.SnowTile_2, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;
                    else if (snowTile > snow1 + snow2 && snowTile <= snow1 + snow2 + snow3)
                        return Instantiate(WorldManager.Instance.SnowTile_3, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;
                   
                    break;
                }
        }
        return null;
    }
    GameObject GetRegionBasedLandTileBorder(int width, int height)
    {
        LandTileBorder landTileBorderMask = WorldTileArray[width, height].LandTileBorderMask;

        switch (regionType)
        {
            case RegionType.Grasslands:
                {
                    switch(landTileBorderMask)
                    {
                        case LandTileBorder.North:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_North, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.South:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_South, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.East:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_East, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_West, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_NorthWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.East:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_NorthEast, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.South | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_SouthWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.South | LandTileBorder.East:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_SouthEast, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.South:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_NorthSouth, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.East | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_EastWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.East | LandTileBorder.South:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_NorthEastSouth, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.East | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_NorthEastWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.West | LandTileBorder.South:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_NorthWestSouth, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.South | LandTileBorder.East | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_SouthEastWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.South | LandTileBorder.East | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.GrassEdge_NorthSouthEastWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                    }
                    break;
                }
            case RegionType.Rugged:
                {
                    switch (landTileBorderMask)
                    {
                        case LandTileBorder.North:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_North, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.South:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_South, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.East:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_East, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_West, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_NorthWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.East:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_NorthEast, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.South | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_SouthWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.South | LandTileBorder.East:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_SouthEast, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.South:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_NorthSouth, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.East | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_EastWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.East | LandTileBorder.South:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_NorthEastSouth, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.East | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_NorthEastWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.West | LandTileBorder.South:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_NorthWestSouth, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.South | LandTileBorder.East | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_SouthEastWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }
                        case LandTileBorder.North | LandTileBorder.South | LandTileBorder.East | LandTileBorder.West:
                            {
                                return Instantiate(WorldManager.Instance.RuggedGrassEdge_NorthSouthEastWest, new Vector3(
                                    firstTile.x + width * TileSize,
                                    firstTile.y + height * TileSize,
                                    0f), Quaternion.identity) as GameObject;

                            }

                    }
                    break;
                }
        }
        return null;
    }

    GameObject CreateLandTileBorder(int width, int height)
    {
        switch(WorldTileArray[width, height].LandTileBorderMask)
        {
            case LandTileBorder.North:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_North, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.South:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_South, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.East:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_East, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.West:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_West, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.North | LandTileBorder.West:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_NorthWest, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.North | LandTileBorder.East:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_NorthEast, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.South | LandTileBorder.West:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_SouthWest, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.South | LandTileBorder.East:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_SouthEast, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.North | LandTileBorder.South:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_NorthSouth, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.East | LandTileBorder.West:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_EastWest, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.North | LandTileBorder.East | LandTileBorder.South:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_NorthEastSouth, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.North | LandTileBorder.East | LandTileBorder.West:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_NorthEastWest, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.North | LandTileBorder.West | LandTileBorder.South:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_NorthWestSouth, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.South | LandTileBorder.East | LandTileBorder.West:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_SouthEastWest, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
            case LandTileBorder.North | LandTileBorder.South | LandTileBorder.East | LandTileBorder.West:
                {
                    return Instantiate(WorldManager.Instance.GrassEdge_NorthSouthEastWest, new Vector3(
                        firstTile.x + width * TileSize,
                        firstTile.y + height * TileSize,
                        0f), Quaternion.identity) as GameObject;

                }
        }
        return null;
    }
    
    void SetMapBoundaries()
    {
        bottomCenter = new Vector3(firstTile.x + Width * .5f * TileSize, firstTile.y + 0f, 0f);
        topCenter = new Vector3(firstTile.x + Width * .5f * TileSize, firstTile.y + (Height - 1) * TileSize, 0f);

        topBoundary = gameObject.AddComponent<BoxCollider2D>();
        topBoundary.size = new Vector2(Width * TileSize + TileSize * 2, TileSize);
        topBoundary.offset = topCenter;
        topBoundary.sharedMaterial = (PhysicsMaterial2D)Resources.Load("Materials/Basic");

        bottomBoundary = gameObject.AddComponent<BoxCollider2D>();
        bottomBoundary.size = new Vector3(Width * TileSize + TileSize * 2, TileSize);
        bottomBoundary.offset = bottomCenter;
        bottomBoundary.sharedMaterial = (PhysicsMaterial2D)Resources.Load("Materials/Basic");
    }

    bool TilesAreEmpty(int startX, int startY, int endX, int endY)
    {
        for (int x = startX; x <= endX; x++)
            for (int y = startY; y <= endY; y++)
                if (WorldTileArray[x, y].TileContents != TileContents.EMPTY)
                    return false;
        
        return true;
    }

    //void SetWater()
    //{
    //    //Decide how many total pools of water to spawn between 0 and 30
    //    int WaterToSpawn = prng.Next(0, MaxRivers);

    //    //roll map coords within our bounds
    //    int xMinBound = 10;
    //    int xMaxBound = Width - 10;

    //    int yMinBound = 10;
    //    int yMaxBound = Height - 10;

    //    int xCoord;
    //    int yCoord;

    //    for(int WaterObjectsToCreate = 0; WaterObjectsToCreate < WaterToSpawn; WaterObjectsToCreate++)
    //    {
    //        xCoord = prng.Next(xMinBound, xMaxBound);
    //        yCoord = prng.Next(yMinBound, yMaxBound);

    //        if (TotalRivers < WaterToSpawn &&
    //                prng.Next(0, 100) < WaterSpawnChance &&
    //                TilesAreEmpty(xCoord - 2, yCoord - 2, xCoord + 2, yCoord + 2)
    //                )
    //        {
    //            AddPOI(POI_Water, TileContents.Water,
    //                xCoord,
    //                yCoord,
    //                5,
    //                5);

    //            TotalRivers++;
    //        }
    //    }

        //for (int _width = 10; _width < Width-10; _width++)
        //    for (int _height = 10; _height < Height - 10; _height++)
        //    {
        //        if (TotalWaterPools < WaterToSpawn &&
        //            Random.Range(0, 100) < WaterSpawnChance &&
        //            TilesAreEmpty(_width - 2, _height - 2, _width + 2, _height + 2)
        //            )
        //        {
        //            AddPOI(POI_Water, TileContents.Water,
        //                _width,
        //                _height,
        //                5,
        //                5);

        //            TotalWaterPools++;

        //            SetSpawnDelay(20, 175);
        //        }
        //    }
    //}

    void SetSpawnDelay(int min, int max)
    {
        SpawnDelay = Random.Range(min, max);
    }

    void SetForests()
    {
        for (int _width = 0; _width < Width; _width++)
            for(int _height = 3; _height < Height-3; _height++)
            {
                if (WorldTileArray[_width, _height].TileContents == TileContents.EMPTY &&
                        Random.Range(0, 100) < ForestSpawnChance)
                    AddScenery(WorldManager.Instance.Tree, TileContents.Forest,
                        _width,
                        _height,
                        _width * TileSize,
                        _height * TileSize);
            }
    }

    void CreateDecorations()
    {
        int x, y;
        for(int decorationLimit = 0; decorationLimit < DecorationMax; decorationLimit++)
        {
            x = WorldManager.Instance.PRNG.Next(MapEdgeRows, Width - MapEdgeRows);
            y = WorldManager.Instance.PRNG.Next(MapEdgeRows, Height - MapEdgeRows);

            if (WorldTileArray[x, y].TileContents == TileContents.EMPTY)
                WorldTileArray[x, y].TileContents = TileContents.Flower;
        }
    }

    GameObject GetRegionBasedMountainPrefab()
    {
        switch(regionType)
        {
            case RegionType.Desert:
                {
                    return WorldManager.Instance.RedMountain;
                }
            case RegionType.Grasslands:
                {
                    return WorldManager.Instance.Mountain;
                }
            case RegionType.Rugged:
                {
                    return WorldManager.Instance.Mountain;
                }
            case RegionType.Snowy:
                {
                    return WorldManager.Instance.Mountain;
                }
        }
        return null;
    }

    void InstantiateScenery()
    {
        for (int _width = 0; _width < Width; _width++)
            for (int _height = 0; _height < Height; _height++)
            {
                switch(WorldTileArray[_width,_height].TileContents)
                {
                    case TileContents.Mountain:
                        {
                            AddScenery(GetRegionBasedMountainPrefab(), TileContents.Mountain,
                                _width,
                                _height,
                                firstTile.x + _width * TileSize,
                                firstTile.y + _height * TileSize);
                            break;
                        }
                    case TileContents.MountainInterior:
                        {
                            AddScenery(GetRegionBasedMountainPrefab(), TileContents.MountainInterior,
                                _width,
                                _height,
                                firstTile.x + _width * TileSize,
                                firstTile.y + _height * TileSize);
                            break;
                        }
                    case TileContents.Forest:
                        {
                            AddScenery(GetCorrectForestPrefab(), TileContents.Forest,
                                _width,
                                _height,
                                firstTile.x + _width * TileSize,
                                firstTile.y + _height * TileSize);

                            break;
                        }
                    case TileContents.Flower:
                        {
                            int flowerChance = WorldManager.Instance.PRNG.Next(0, 2);

                            if (flowerChance == 0)
                                AddScenery(WorldManager.Instance.Flower_1, TileContents.Flower,
                                    _width,
                                    _height,
                                    firstTile.x + _width * TileSize,
                                    firstTile.y + _height * TileSize);
                            else if (flowerChance == 1)
                                AddScenery(WorldManager.Instance.Flower_2, TileContents.Flower,
                                    _width,
                                    _height,
                                    firstTile.x + _width * TileSize,
                                    firstTile.y + _height * TileSize);
                            else if (flowerChance == 2)
                                AddScenery(WorldManager.Instance.Flower_3, TileContents.Flower,
                                    _width,
                                    _height,
                                    firstTile.x + _width * TileSize,
                                    firstTile.y + _height * TileSize);
                            break;
                        }
                    case TileContents.Cactus:
                        {
                            int cactusChance = WorldManager.Instance.PRNG.Next(0, 2);

                            if (cactusChance == 0)
                                AddScenery(WorldManager.Instance.Cactus_1, TileContents.Cactus,
                                    _width,
                                    _height,
                                    firstTile.x + _width * TileSize,
                                    firstTile.y + _height * TileSize);
                            else if (cactusChance == 1)
                                AddScenery(WorldManager.Instance.Cactus_2, TileContents.Cactus,
                                    _width,
                                    _height,
                                    firstTile.x + _width * TileSize,
                                    firstTile.y + _height * TileSize);
                            else if (cactusChance == 2)
                                AddScenery(WorldManager.Instance.Cactus_3, TileContents.Cactus,
                                    _width,
                                    _height,
                                    firstTile.x + _width * TileSize,
                                    firstTile.y + _height * TileSize);
                            break;
                        }
                }
            }
    }

    GameObject GetCorrectForestPrefab()
    {
        switch (regionType)
        {
            case RegionType.Grasslands:
                {
                    return WorldManager.Instance.Tree;
                }
            case RegionType.Rugged:
                {
                    return WorldManager.Instance.Tree;
                }
            case RegionType.Snowy:
                {
                    return WorldManager.Instance.SnowTree;
                }
            case RegionType.Desert:
                {
                    return WorldManager.Instance.Tree;
                }

        }

        return null;
    }
    void CreateLakes()
    {
        //flood fill algorithm
        int floodSteps;
        int startX, startY; 

        int LakesToCreate = WorldManager.Instance.PRNG.Next(MinLakes, MaxLakes);

        for(int currentLake = 0; currentLake < LakesToCreate; currentLake++)
        {
            floodSteps = WorldManager.Instance.PRNG.Next(MinLakeSteps, MaxLakeSteps);
            startX = WorldManager.Instance.PRNG.Next(MapEdgeRows, Width - MapEdgeRows);
            startY = WorldManager.Instance.PRNG.Next(MapEdgeRows, Height - MapEdgeRows);

            FloodFill(floodSteps, startX, startY);
        }
    }

    void FloodFill(int steps, int width, int height)
    {
        if (steps < 0)
            return;

        if (width < 0)
            width = 0;
        if (width > Width - MapEdgeRows)
            width = Width - MapEdgeRows;
        if (height < 0)
            height = 0;
        if (height > Height - MapEdgeRows)
            height = Height - MapEdgeRows;

        WorldTileArray[width, height].TileType = TileType.Water;
        WorldTileArray[width, height].TileContents = TileContents.EMPTY;

        //north
        FloodFill(steps - 1, width, height + 1);
        //south
        FloodFill(steps - 1, width, height - 1);
        //east
        FloodFill(steps - 1, width + 1, height);
        //west
        FloodFill(steps - 1, width - 1, height);
        //northeast
        FloodFill(steps - 1, width + 1, height + 1);
        //northwest
        FloodFill(steps - 1, width - 1, height + 1);
        //southeast
        FloodFill(steps - 1, width + 1, height - 1);
        //southwest
        FloodFill(steps - 1, width - 1, height - 1);
    }

    void CreateRivers()
    {
        //drunkard walk algorithm
        //pick number of bodies of water to create
        int WaterBodiesToCreate = WorldManager.Instance.PRNG.Next(MinRivers, MaxRivers);
        //for each body of water
        int WaterSteps;
        int startX, startY;
        Direction direction;
        int maxDirection = (int)Direction.TOTAL;

        for(int currentWaterBody = 0; currentWaterBody < WaterBodiesToCreate; currentWaterBody++)
        {
            //pick a number of steps for that body
            WaterSteps = WorldManager.Instance.PRNG.Next(MinRiverSteps, MaxRiverSteps);

            //pick a start point
            startX = WorldManager.Instance.PRNG.Next(0, Width - 1);
            startY = WorldManager.Instance.PRNG.Next(0, Height - 1);

            if (startX < MapEdgeRows)
                startX = MapEdgeRows;
            if (startY >= Height - MapEdgeRows)
                startY = Height - MapEdgeRows - 1;
            if (startY < MapEdgeRows)
                startY = MapEdgeRows;

            WorldTileArray[startX, startY].TileType = TileType.Water;
            WorldTileArray[startX, startY].TileContents = TileContents.EMPTY;
            //loop the following till out of steps:
            for (int Step = 0; Step < WaterSteps; Step++)
            {                
                direction = (Direction)WorldManager.Instance.PRNG.Next(0, maxDirection - 1);

                switch(direction)
                {
                    case Direction.Down:
                        {
                            startY -= 1;
                            break;
                        }
                    case Direction.Left:
                        {
                            startX -= 1;
                            break;
                        }
                    case Direction.Right:
                        {
                            startX++;
                            break;
                        }
                    case Direction.Up:
                        {
                            startY++;
                            break;
                        }

                }
                //clamp
                if (startX < 0)
                    startX = 0;
                if (startX >= Width)
                    startX = Width - 1;

                if (startY < 0)
                    startY = 0;
                if (startY >= Height)
                    startY = Height - 1;

                if (startX < MapEdgeRows)
                    startX = MapEdgeRows;
                if (startY >= Height - MapEdgeRows)
                    startY = Height - MapEdgeRows - 1;
                if (startY < MapEdgeRows)
                    startY = MapEdgeRows;
                //set tile to water
                WorldTileArray[startX, startY].TileType = TileType.Water;
                WorldTileArray[startX, startY].TileContents = TileContents.EMPTY;

            }
        }     
    }

    void CreateWaterBorders()
    {
        for (int _width = MapEdgeRows; _width < Width - MapEdgeRows; _width++)
            for (int _height = MapEdgeRows; _height < Height - MapEdgeRows; _height++)
            {
                if(WorldTileArray[_width,_height].TileType == TileType.Water)
                {
                    //check to see which neighbors are not water

                    LandTileBorder neighborMask = BitMaskNeighborCheck(_width, _height);

                    SetWaterBorder(neighborMask, _width, _height);
                }
            }
    }

    void SetWaterBorder(LandTileBorder neighborMask, int tileX, int tileY)
    {
        WorldTileArray[tileX, tileY].LandTileBorderMask = neighborMask;
    }

    /// <summary>
    /// Check All directions and use bitMask
    /// </summary>
    /// <returns></returns>
    LandTileBorder BitMaskNeighborCheck(int tileX, int tileY)
    {
        LandTileBorder directionMask = LandTileBorder.EMPTY;

        //check north
        if (WorldTileArray[tileX, tileY + 1].TileType != TileType.Water)
            directionMask = directionMask | LandTileBorder.North;

        //check south
        if (WorldTileArray[tileX, tileY - 1].TileType != TileType.Water)
            directionMask = directionMask | LandTileBorder.South;

        //check east
        if (WorldTileArray[tileX + 1, tileY].TileType != TileType.Water)
            directionMask = directionMask | LandTileBorder.East;

        //check west
        if (WorldTileArray[tileX - 1, tileY].TileType != TileType.Water)
            directionMask = directionMask | LandTileBorder.West;

        return directionMask;
    }

    void ScatterCacti()
    {
        int CactiToCreate = 75;

        int xCoord, yCoord;
        for(int nCacti = 0; nCacti < CactiToCreate; nCacti++)
        {
            xCoord = WorldManager.Instance.PRNG.Next(MapEdgeRows, Width - MapEdgeRows);
            yCoord = WorldManager.Instance.PRNG.Next(MapEdgeRows, Height - MapEdgeRows);

            WorldTileArray[xCoord, yCoord].TileContents = TileContents.Cactus;

        }
    }

    void CreateForestClusters()
    {
        for (int _width = RegionBuffer; _width < Width - RegionBuffer; _width++)
            for (int _height = MapEdgeRows; _height < Height - MapEdgeRows; _height++)
            {
                if (WorldTileArray[_width, _height].TileContents == TileContents.EMPTY &&
                         WorldManager.Instance.PRNG.Next(0, 100) < ForestSpawnChance)
                    WorldTileArray[_width, _height].TileContents = TileContents.Forest;                
            }

        SmoothSceneryPlacement(TileContents.Forest, 3);

        //invert our fill
        //InvertSceneryContent(TileContents.Forest);
    }

    void CreateMountainClusters()
    {
        for (int _width = 0; _width < Width; _width++)
            for (int _height = 0; _height < Height; _height++)
            {
                if (_height < MapEdgeRows || _height >= Height - MapEdgeRows)
                    WorldTileArray[_width, _height].TileContents = TileContents.Mountain;
                else if (WorldTileArray[_width, _height].TileContents == TileContents.EMPTY &&
                         WorldManager.Instance.PRNG.Next(0, 100) < MountainSpawnChance)
                    WorldTileArray[_width, _height].TileContents = TileContents.Mountain;
                //if (_height < 3 || _height >= Height - 3)
                //    AddScenery(Mountain, TileContents.Mountain,
                //        _width,
                //        _height,
                //        _width * TileSize,
                //        _height * TileSize);
                //else if (WorldTileArray[_width, _height].TileContents == TileContents.EMPTY &&
                //        prng.Next(0, 100) < MountainSpawnChance)
                //    AddScenery(Mountain, TileContents.Mountain,
                //        _width,
                //        _height,
                //        _width * TileSize,
                //        _height * TileSize);
            }

        SmoothSceneryPlacement(TileContents.Mountain, 4);

        //invert our fill
        //InvertSceneryContent(TileContents.Mountain);

        AddMountainBorders();
    }

    void CreateMountainInteriorCluster()
    {
        for (int _width = RegionBuffer; _width < Width - RegionBuffer; _width++)
            for (int _height = 0; _height < Height; _height++)
            {
                if (WorldTileArray[_width, _height].TileContents == TileContents.EMPTY &&
                                         WorldManager.Instance.PRNG.Next(0, 100) < MountainInteriorChance)
                    WorldTileArray[_width, _height].TileContents = TileContents.MountainInterior;

            }

        SmoothSceneryPlacement(TileContents.MountainInterior, 3);

        //invert our fill
        //InvertSceneryContent(TileContents.MountainInterior);
    }

    void SmoothSceneryPlacement(TileContents contentType, int countRule)
    {
        for (int _width = 0; _width < Width; _width++)
            for (int _height = 0; _height < Height; _height++)
            {
                if (WorldTileArray[_width, _height].TileContents != contentType && WorldTileArray[_width, _height].TileContents != TileContents.EMPTY)
                    continue;

                int neighborTypeTiles = GetSurroundingTypeCount(contentType, _width, _height);

                if (neighborTypeTiles > countRule)
                    WorldTileArray[_width, _height].TileContents = contentType;
                else if (neighborTypeTiles <= countRule && WorldTileArray[_width,_height].TileContents == contentType)
                    WorldTileArray[_width, _height].TileContents = TileContents.EMPTY;
            }
    }

    int GetSurroundingTypeCount(TileContents type, int xTile, int yTile)
    {
        int typeCount = 0;

        for(int neighborX = xTile - 1; neighborX <= xTile + 1; neighborX++)
        {
            for(int neighborY = yTile - 1; neighborY <= yTile + 1; neighborY++)
            {
                //bounds check
                if (neighborX >= 0 && neighborX < Width && neighborY >= 0 && neighborY < Height)
                {
                    //not looking at input tile
                    if (neighborX != xTile || neighborY != yTile)
                    {
                        if (WorldTileArray[neighborX, neighborY].TileContents == type)
                            typeCount++;
                    }
                }
                else
                    typeCount++;//encourage spawning around edges
            }
        }

        return typeCount;
    }

    void InvertSceneryContent(TileContents type)
    {
        for (int _width = 0; _width < Width; _width++)
            for (int _height = 0; _height < Height; _height++)
            {
                if (WorldTileArray[_width, _height].TileContents == TileContents.EMPTY)
                    WorldTileArray[_width, _height].TileContents = type;
                else if (WorldTileArray[_width, _height].TileContents == type)
                    WorldTileArray[_width, _height].TileContents = TileContents.EMPTY;
            }
    }

    void AddMountainBorders()
    {
        for (int _width = 0; _width < Width; _width++)
            for (int _height = 0; _height < Height; _height++)
            {
                if (_height < MapEdgeRows || _height >= Height - MapEdgeRows)
                    WorldTileArray[_width, _height].TileContents = TileContents.Mountain;
            }
    }

    void SetInteriorMountains()
    {
        for (int _width = 50; _width < Width - 10; _width++)
            for (int _height = 4; _height < Height - 4; _height++)
            {
                if (WorldTileArray[_width, _height].TileContents == TileContents.EMPTY &&
                        Random.Range(0, 100) < MountainSpawnChance)
                    AddScenery(WorldManager.Instance.Mountain, TileContents.Mountain,
                        _width,
                        _height,
                        _width * TileSize,
                        _height * TileSize);
            }
    }

    void SetMountains()
    {
        //GameObject mountainToCreate;
        //first just 3 rows along the edges
        for (int _width = 0; _width < Width; _width++)
        {
            //mountainToCreate = Instantiate(Mountain, new Vector3(
            //        _width * TileSize,
            //        0,
            //        0f), Quaternion.identity) as GameObject;

            //mountainToCreate = Instantiate(Mountain, new Vector3(
            //        _width * TileSize,
            //        1,
            //        0f), Quaternion.identity) as GameObject;

            //mountainToCreate = Instantiate(Mountain, new Vector3(
            //        _width * TileSize,
            //        2,
            //        0f), Quaternion.identity) as GameObject;

            //mountainToCreate = Instantiate(Mountain, new Vector3(
            //        _width * TileSize,
            //        (Height - 1) * TileSize,
            //        0f), Quaternion.identity) as GameObject;

            //mountainToCreate = Instantiate(Mountain, new Vector3(
            //        _width * TileSize,
            //        (Height - 2) * TileSize,
            //        0f), Quaternion.identity) as GameObject;

            //mountainToCreate = Instantiate(Mountain, new Vector3(
            //        _width * TileSize,
            //        (Height - 3) * TileSize,
            //        0f), Quaternion.identity) as GameObject;

            AddScenery(WorldManager.Instance.Mountain, TileContents.Mountain,
                _width,
                0,
                _width * TileSize,
                0f);

            AddScenery(WorldManager.Instance.Mountain, TileContents.Mountain,
               _width,
               1,
               _width * TileSize,
               1f * TileSize);

            AddScenery(WorldManager.Instance.Mountain, TileContents.Mountain,
               _width,
               2,
               _width * TileSize,
               2f * TileSize);

            //top section
            AddScenery(WorldManager.Instance.Mountain, TileContents.Mountain,
               _width,
               Height - 1,
               _width * TileSize,
               (Height - 1f) * TileSize);

            AddScenery(WorldManager.Instance.Mountain, TileContents.Mountain,
                _width,
                Height - 2,
                _width * TileSize,
                (Height - 2f) * TileSize);

            AddScenery(WorldManager.Instance.Mountain, TileContents.Mountain,
                _width,
                Height - 3,
                _width * TileSize,
                (Height - 3f) * TileSize);
        }
    }

    void AddScenery(GameObject prefab, TileContents contentType, int arrayX, int arrayY, float xCoord, float yCoord)
    {
        GameObject store = Instantiate(prefab, 
            new Vector3(
                    xCoord,
                    yCoord,
                    0f), 
            Quaternion.identity) as GameObject;

        WorldTileArray[arrayX, arrayY].TileContents = contentType;

        store.transform.parent = transform;
    }

    void AddPOI(GameObject prefab, TileContents contentType, int arrayX, int arrayY, int Rows, int Columns)
    {
        GameObject store = Instantiate(prefab,
            new Vector3(
                    arrayX * TileSize,
                    arrayY * TileSize,
                    0f),
            Quaternion.identity) as GameObject;

        int nStartX = arrayX - Mathf.FloorToInt(Rows * .5f);
        int nStartY = arrayY - Mathf.FloorToInt(Columns * .5f);

        for (int nRow = nStartX; nRow < nStartX + Rows; nRow++)
            for(int nCol = nStartY; nCol < nStartY + Columns; nCol++)
                WorldTileArray[nRow, nCol].TileContents = contentType;

        store.transform.parent = transform;
    }

    // Update is called once per frame
    void Update () {
	
	}
}
