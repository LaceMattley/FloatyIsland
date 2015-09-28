using UnityEngine;
using System.Collections;

public class SurfaceSpawner : MonoBehaviour {

    public GameObject parentObject;

    public GameObject instantiateObject;
    public Vector3 offset = new Vector3(0.0f, 0.0f, 0.0f);
    int count = 20;
    [ContextMenu("Generate")]
    public void spawnOnSurface()
    {
        for (int i = 0; i < count; i++)
        {
            Mesh surfaceMesh = GetComponent<MeshFilter>().sharedMesh;
            Vector3 randVertex = surfaceMesh.vertices[Random.Range(0, surfaceMesh.vertexCount)];
            GameObject newObject = GameObject.Instantiate<GameObject>(instantiateObject);
            newObject.transform.parent = parentObject.transform;
            newObject.transform.localPosition = randVertex + offset;
            
        }

    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
