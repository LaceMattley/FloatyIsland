using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct GridSpawnObjectRequest
{
    public GameObject spawnObject;
    public List<Index2> layoutList;
    public Vector3 worldPosition;
}

public class BuildingGrid : MonoBehaviour {

    public ProceduralMesh prodMesh;
    public float cellSize = 1.0f;
    Dictionary<Index2, GameObject> spawnedObjectDictionary;
    
	// Use this for initialization
	void Start () {
        prodMesh = GetComponentInParent<ProceduralMesh>();
	}
	
    void OnDrawGizmos()
    {
        if (prodMesh == null)
        {
            prodMesh = GetComponentInParent<ProceduralMesh>();
        }
        Vector3 islandPosition = this.transform.position;
        Vector3 startPosition = islandPosition - new Vector3(prodMesh.xSize*0.5f, 0.0f, prodMesh.ySize*0.5f);
        Vector3 endPosition = islandPosition + new Vector3(prodMesh.xSize*0.5f, 0.0f, prodMesh.ySize*0.5f);
        for(float x = startPosition.x;x<endPosition.x;x+=cellSize )
        {
            Debug.DrawLine(new Vector3(x, 5.0f, startPosition.z), new Vector3(x, 5.0f, endPosition.z));
        }
        for (float z = startPosition.z; z < endPosition.z; z += cellSize)
        {
            Debug.DrawLine(new Vector3(startPosition.x, 5.0f, z), new Vector3(endPosition.x, 5.0f, z));
        }

    }
    public Vector3 roundToNearestCellMiddle(Vector3 vec)
    {
        float xPos = Mathf.Round(vec.x / cellSize) * cellSize;
        float zPos = Mathf.Round(vec.z / cellSize) * cellSize;
        float yPos = 0.0f;
        prodMesh.getHeightForPosition(xPos,zPos,out yPos);
        return new Vector3(xPos,yPos,zPos);
    }
    public bool isOnGrid(Vector3 vec)
    {
        return true;
    }
    public Index2 getIndexForVec(Vector3 vec)
    {
        Index2 returnIndex = new Index2();
        returnIndex.X = Mathf.RoundToInt(vec.x / (cellSize * prodMesh.xSize));
        returnIndex.Y= Mathf.RoundToInt(vec.z / (cellSize * prodMesh.xSize));
        return returnIndex;
    }
    public bool trySpawnObject(GridSpawnObjectRequest spawnRequest, ref GameObject spawnedObject)
    {
        if (isOnGrid(spawnRequest.worldPosition))
        {
            Vector3 position = roundToNearestCellMiddle(spawnRequest.worldPosition);
            spawnedObject = GameObject.Instantiate<GameObject>(spawnRequest.spawnObject);
            spawnedObject.transform.position = position;
        }
        return false;
    }
}
