using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudManager : MonoBehaviour {

    public GameObject CloudPrefab;

    public GameObject Cloud_0;
    public GameObject Cloud_1;
    public GameObject Cloud_2;
    public GameObject Cloud_3;
    public GameObject Cloud_4;
    public GameObject Cloud_5;
    public GameObject Cloud_6;
    public GameObject Cloud_7;
    public GameObject Cloud_8;
    public GameObject Cloud_9;
    List<GameObject> CloudPrefabs;
    public int CloudCount;
    List<GameObject> Clouds;
    List<Renderer> CloudRenderers;

    public float xMin;
    public float xMax;
    public float yMin;
    public float yMax;

    public float xVelocity = -.5f;
    public float yVelocity = -.5f;

    public float yWrap;
    public float xWrap;

    private static CloudManager cloudManager;
    public static CloudManager Instance
    {
        get
        {
            if (!cloudManager)
            {
                cloudManager = FindObjectOfType(typeof(CloudManager)) as CloudManager;

                if (!cloudManager)
                {
                    Debug.LogError("CloudManager Script must be on a GameObject and Active");
                }
                else
                {
                    cloudManager.Init();
                }
            }

            return cloudManager;
        }
    }

    void Init()
    {
        Clouds = new List<GameObject>();
        CloudPrefabs = new List<GameObject>();
        CloudRenderers = new List<Renderer>();

        CloudPrefabs.Add(Cloud_0);
        CloudPrefabs.Add(Cloud_1);
        CloudPrefabs.Add(Cloud_2);

        //create the number of clouds, each at a different start point
        InitClouds();
    }

    public static void SetCloudsAlpha(float alpha)
    {
        for (int nCloud = 0; nCloud < Instance.CloudRenderers.Count; nCloud++)
            Instance.CloudRenderers[nCloud].material.color = new Color(Instance.CloudRenderers[nCloud].material.color.r,
                Instance.CloudRenderers[nCloud].material.color.g,
                Instance.CloudRenderers[nCloud].material.color.b,
                alpha);
    }

    // Use this for initialization
    void Start () {
        Instance.Init();
    }
	
    void InitClouds()
    {
        int nCloud;
        GameObject cloudToCreate;
        for(int cloud = 0; cloud < CloudCount; cloud++)
        {
            nCloud = Random.Range(0, CloudPrefabs.Count);

            cloudToCreate = Instantiate(CloudPrefabs[nCloud],
            new Vector3(
                    Random.Range(xMin, xMax),
                    Random.Range(yMin, yMax),
                    0f),
            Quaternion.identity) as GameObject;
          
            cloudToCreate.transform.parent = transform;

            Clouds.Add(cloudToCreate);

            Renderer cloudRenderer = cloudToCreate.GetComponent<Renderer>();
            cloudRenderer.material.color = new Color(cloudRenderer.material.color.r,
                cloudRenderer.material.color.g,
                cloudRenderer.material.color.b,
                0f);

            //add reference to renderers list for alpha changing
            CloudRenderers.Add(cloudRenderer);
        }
    }

    void CloudMarch()
    {
        Vector3 velocity = new Vector3(xVelocity, yVelocity, 0f);

        for (int cloud = 0; cloud < CloudCount; cloud++)
        {
            Clouds[cloud].transform.position += velocity * Time.deltaTime;

            if (Clouds[cloud].transform.position.y < yWrap)
                Clouds[cloud].transform.position = new Vector3(Clouds[cloud].transform.position.x, 100f, Clouds[cloud].transform.position.z);

            if (Clouds[cloud].transform.position.x > xWrap)
                Clouds[cloud].transform.position = new Vector3(0f, Clouds[cloud].transform.position.y, Clouds[cloud].transform.position.z);
        }
    }

	// Update is called once per frame
	void Update () {
        CloudMarch();
	}
}
