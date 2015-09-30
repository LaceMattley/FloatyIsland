using UnityEngine;
using System.Collections;

public class BuildingPlacer : MonoBehaviour {

    public GameObject pointerObject;
    public float granularity = 2.0f;
    public float spawnHeight = 2.0f;
    Vector3? lastPosition;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetButton("Fire1")) 
        { 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit))
            {
                
                float xPos = Mathf.Round(hit.point.x/granularity)*granularity;
                float zPos = Mathf.Round(hit.point.z/granularity)*granularity;
                Vector3 pos = new Vector3(xPos, spawnHeight, zPos);

                if (!lastPosition.HasValue)
                {
                    pos = TrySpawnAtPosition(pos);
                }
                else
                {
                    float xDiff = Mathf.Abs(pos.x - lastPosition.Value.x);
                    float yDiff = Mathf.Abs(pos.z - lastPosition.Value.z);


                    if (xDiff < Mathf.Epsilon)
                    {
                        float increments = granularity;
                        float start = lastPosition.Value.z;
                        float end = zPos;
                        if (start > end)
                        {
                            start = zPos;
                            end = lastPosition.Value.z;
                        }
                        for (float i = start; i <= end; i += increments)
                        {
                            TrySpawnAtPosition(new Vector3(xPos, spawnHeight, i));
                        }
                    }
                    if (yDiff < Mathf.Epsilon)
                    {
                        float increments = granularity;
                        float start = lastPosition.Value.x;
                        float end = xPos;
                        if (start > end)
                        {
                            start = xPos;
                            end = lastPosition.Value.x;
                        }
                        for (float i = start; i <= end; i += increments)
                        {
                            TrySpawnAtPosition(new Vector3(i, spawnHeight, zPos));
                        }
                    }
                }
            }
        }
        else
        {
            lastPosition = null; ;
        }
	}

    private Vector3 TrySpawnAtPosition(Vector3 pos)
    {
        bool shouldSpawn = true;
        foreach (RoadMesh roadMesh in GameObject.FindObjectsOfType<RoadMesh>())
        {
            Vector3 distance = roadMesh.transform.position - pos;
            distance = new Vector3(distance.x, 0.0f, distance.z);
            if (distance.sqrMagnitude < 1.0f)
            {
                shouldSpawn = false;
            }
        }
        if (shouldSpawn)
        {
            GameObject newObject = GameObject.Instantiate<GameObject>(pointerObject);
            newObject.transform.position = pos;
            lastPosition = pos;
        }
        return pos;
    }
}
