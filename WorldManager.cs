/*
 * Code by Ryan Jackson
 * */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class WorldManager : MonoBehaviour {

    string seed;
    System.Random prng;

    public System.Random PRNG
    {
        get { return prng; }
    }

    public List<string> region_adjectives;
    public List<string> indoorArea_nouns;

    [Header("World Name")]
    public string WorldName;

    [Header("Darkness")]
    public IndoorDarknessTrigger darknessTrigger;

    [Header("Region")]
    public RegionTrigger regionTrigger;

    [Header("Prefabs")]

    [Header("WorldMap")]
    public GameObject WorldMapPrefab;
    [Header("Grass")]
    public GameObject GrassTile_1;
    public GameObject GrassTile_2;
    public GameObject GrassTile_Dirt1;
    public GameObject GrassTile_Dirt2;
    public GameObject GrassTile_Dirt3;
    public GameObject GrassEdge_North;
    public GameObject GrassEdge_South;
    public GameObject GrassEdge_East;
    public GameObject GrassEdge_West;
    public GameObject GrassEdge_NorthWest;
    public GameObject GrassEdge_NorthEast;
    public GameObject GrassEdge_SouthWest;
    public GameObject GrassEdge_SouthEast;
    public GameObject GrassEdge_NorthSouth;
    public GameObject GrassEdge_EastWest;
    public GameObject GrassEdge_NorthEastSouth;
    public GameObject GrassEdge_NorthEastWest;
    public GameObject GrassEdge_NorthWestSouth;
    public GameObject GrassEdge_SouthEastWest;
    public GameObject GrassEdge_NorthSouthEastWest;
    [Header("Snow")]
    public GameObject SnowTile_1;
    public GameObject SnowTile_2;
    public GameObject SnowTile_3;
    [Header("Sand")]
    public GameObject SandTile_1;
    public GameObject SandTile_2;
    [Header("Rugged")]
    public GameObject RuggedGrassTile_1;
    public GameObject RuggedGrassTile_2;
    public GameObject RuggedGrassEdge_North;
    public GameObject RuggedGrassEdge_South;
    public GameObject RuggedGrassEdge_East;
    public GameObject RuggedGrassEdge_West;
    public GameObject RuggedGrassEdge_NorthWest;
    public GameObject RuggedGrassEdge_NorthEast;
    public GameObject RuggedGrassEdge_SouthWest;
    public GameObject RuggedGrassEdge_SouthEast;
    public GameObject RuggedGrassEdge_NorthSouth;
    public GameObject RuggedGrassEdge_EastWest;
    public GameObject RuggedGrassEdge_NorthEastSouth;
    public GameObject RuggedGrassEdge_NorthEastWest;
    public GameObject RuggedGrassEdge_NorthWestSouth;
    public GameObject RuggedGrassEdge_SouthEastWest;
    public GameObject RuggedGrassEdge_NorthSouthEastWest;
    [Header("Flowers")]
    public GameObject Flower_1;
    public GameObject Flower_2;
    public GameObject Flower_3;
    [Header("Water")]
    public GameObject WaterTile;
    [Header("Mountains")]
    public GameObject Mountain;
    public GameObject RedMountain;
    [Header("Trees")]
    public GameObject Tree;
    public GameObject SnowTree;
    public GameObject Cactus_1;
    public GameObject Cactus_2;
    public GameObject Cactus_3;

    [Header("POI Prefabs")]
    public GameObject StartingSanctuary;

    List<WorldMap> WorldMapRegions;

    Vector3 startingTilePosition;

    public Vector3 StartingTilePosition
    {
        get { return startingTilePosition; }
        set { startingTilePosition = value; }
    }

    private static WorldManager worldManager;
    public static WorldManager Instance
    {
        get
        {
            if (!worldManager)
            {
                worldManager = FindObjectOfType(typeof(WorldManager)) as WorldManager;

                if (!worldManager)
                {
                    Debug.LogError("WorldManager Script must be on a GameObject and Active");
                }
                else
                {
                    worldManager.Init();
                }
            }

            return worldManager;
        }
    }

    void Init()
    {
        WorldMapRegions = new List<WorldMap>();
        region_adjectives = new List<string>();
        indoorArea_nouns = new List<string>();
    }

    //This allows the generation to be Pseudo-Random.
    //stringToSeed = World Name
    //A specific string "seed" will always produce the exact same outcome for map generation
    //This allows us revisit previous map layouts by using the exact same "World Name" or generate an endless number of new map layouts
    void InitRng(string stringToSeed)
    {
        seed = stringToSeed;
        prng = new System.Random(seed.GetHashCode());
    }

    //
    public static void StartNewWorld(string worldName)
    {
        //Load in Word Dictionaries
        Instance.LoadDictionary(Application.dataPath+"/Resources/Dictionaries/fantasy_adjectives.txt", Instance.region_adjectives);
        Instance.LoadDictionary(Application.dataPath + "/Resources/Dictionaries/fantasy_nouns.txt", Instance.indoorArea_nouns);

        Instance.FirstLetterToUpper(Instance.region_adjectives);

        Instance.WorldName = worldName;

        Instance.InitRng(worldName);

        BuildRegions(1);      
    }

    public static void BuildRegions(int regionsToBuild)
    {
        //create regions one after the other, in order and insert them into the list
        GameObject regionToCreate;
        for(int nRegion = 0; nRegion < regionsToBuild; nRegion++)
        {
            regionToCreate = Instantiate(Instance.WorldMapPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            regionToCreate.transform.parent = Instance.transform;

            WorldMap map = regionToCreate.GetComponent<WorldMap>();

            //need to decide region type
            //map.regionType = map.getregio
            map.InitMap(Instance.CalculateStartingTile(), Instance.WorldMapRegions.Count, RegionType.Rugged, 500, 50);
            
            Instance.WorldMapRegions.Add(map);
        }
    }
    
    RegionType RollRegionType()
    {
        return (RegionType)PRNG.Next((int)RegionType.Grasslands, (int)(RegionType.TOTAL - 1));
    }

    Vector3 CalculateStartingTile()
    {
        //what's the current region count
        //if it's zero, first tile is vector3.zero
        //if it's more than 0
        //get the region at the end of the list, and it's width
        if (WorldMapRegions.Count == 0)
            return Vector3.zero;

        float xCoord = WorldMapRegions[WorldMapRegions.Count - 1].Width * WorldMapRegions[WorldMapRegions.Count - 1].TileSize;

        return new Vector3(xCoord, 0f, 0f);
    }

    /// <summary>
    /// Read in Word Dictionaries.  These dictionaries are used to psuedo-randomly assign names to playable world segments (environments) that the player traverses through
    /// One dictionary is a text file with a single adjective per line
    /// The second dicctionary is a text file with a single noun per line
    /// One Adjective and One Noun is used to form a region name
    /// Examples: Parched Highlands, Abandoned Wastelands
    /// </summary>
    /// <returns></returns>
    private void LoadDictionary(string fileName, List<string> storeHere)
    {
        string line;
        StreamReader reader = new StreamReader(fileName, Encoding.Default);

        while (!reader.EndOfStream)
        {
            line = reader.ReadLine();

            if (line != null)
                storeHere.Add(line);
        }

        reader.Close();
    }

    void FirstLetterToUpper(List<string> stringList)
    {
        for(int i = 0; i < stringList.Count; i++)
        {
            string charToUpper = stringList[i][0].ToString();

            charToUpper = charToUpper.ToUpper();

            stringList[i] = stringList[i].Replace(stringList[i][0], charToUpper.ToCharArray()[0]);
        }
    }
}



