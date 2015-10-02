using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingPlacer : MonoBehaviour {

    public GameObject pointerObject;
    public float granularity = 2.0f;
    public float spawnHeight = 2.0f;
    Vector3? lastPosition;
    public List<Vector3> baseOffsetList = new List<Vector3>();
    public bool oddWidth = false;
    public bool oddHeight = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetButtonDown("Fire1")) 
        { 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray,out hit))
            {
                
                float xPos = Mathf.Round(hit.point.x/granularity)*granularity;
                float zPos = Mathf.Round(hit.point.z/granularity)*granularity;
                Vector3 pos = new Vector3(xPos, spawnHeight, zPos);
                pos += new Vector3(oddWidth ? -0.5f : 0.0f, 0.0f, oddHeight ? 0.5f : 0.0f);
               
                bool canSpawn = true;
                foreach (RoadMesh roadMesh in GameObject.FindObjectsOfType<RoadMesh>())
                {
                    foreach (Vector3 offsetVec in baseOffsetList)
                    {
                        Vector3 offsetPos = pos + offsetVec;                        
                        Vector3 distance = roadMesh.transform.position - offsetPos;
                        if (distance.sqrMagnitude < 1.0f)
                        {
                            canSpawn = false;
                        }
                    }
                }

                if (canSpawn)
                {
                    foreach (Vector3 offsetVec in baseOffsetList)
                    {
                        Vector3 offsetPos = pos + offsetVec * granularity;
                        GameObject gameObject = GameObject.Instantiate<GameObject>(pointerObject);
                        gameObject.transform.position = offsetPos;
                            
                    }

                    //pos = TrySpawnAtPosition(pos);
                }                               
            }
        }
        else
        {
            lastPosition = null; ;
        }
	}
}
