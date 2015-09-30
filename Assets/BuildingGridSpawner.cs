using UnityEngine;
using System.Collections;

public class BuildingGridSpawner : MonoBehaviour {

    public GameObject objectGrouper;
    public int boxWidth = 10;

    public GameObject spawnerObject;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    [ContextMenu("Gen")]
    void SpawnGrid()
    {
        ProceduralMesh prodMesh = GetComponentInParent<ProceduralMesh>();
        int width = prodMesh.PointsNum1D;
        bool[] meshData = prodMesh.arrayValidData;

        for (int i=0;i<width-boxWidth;i++)
        {
            for (int j=0;j<width-boxWidth;j++)
            {
                bool shouldSpawn = true;
                for (int x = i; x < i + boxWidth;x++ )
                {
                    for (int y = j; y < j + boxWidth;y++)
                    {
                        if (!meshData[x + y * width])
                        {
                            shouldSpawn = false;
                        }
                    }
                }
                if (shouldSpawn)
                {
                    Vector3 pos = prodMesh.getIndexPosition(i + boxWidth / 2, j + boxWidth / 2);
                    GameObject spawnedObject = GameObject.Instantiate(spawnerObject, pos, Quaternion.identity) as GameObject;
                    spawnerObject.transform.parent = objectGrouper.transform;
                }
                    
            }
        }


    }
}
