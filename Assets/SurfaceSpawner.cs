using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SurfaceSpawner : MonoBehaviour {

    public GameObject parentObject;

    public GameObject instantiateObject;
    public Vector3 offset = new Vector3(0.0f, 0.0f, 0.0f);

	public int count = 20;
	public int groupCount = 1;

	public float maxAltitude = float.MaxValue;
	public float minAltitude = float.MinValue;

	public float maxDistance = float.MaxValue;

	public float minGroupDistance = float.MaxValue;

	List<Vector3> groupPositions = new List<Vector3> ();

    [ContextMenu("Generate")]
    public void spawnOnSurface()
    {
		for (int j = 0; j < groupCount; ++j) {
			List<GameObject> spawned = new List<GameObject> ();
			for (int i = 0; i < count; i++) {
				Mesh surfaceMesh = GetComponent<MeshFilter> ().sharedMesh; 
				bool valid = false;
				Vector3 randVertex = Vector3.zero;
				while (!valid) {
					valid = true;
					randVertex = surfaceMesh.vertices [Random.Range (0, surfaceMesh.vertexCount)];
					if (maxDistance != float.MaxValue) {
						if (spawned.Any (s => Mathf.Abs (Vector3.Distance (s.transform.position, randVertex)) > maxDistance)) {
							valid = false;
							continue;
						}
					}
					if (maxAltitude != float.MaxValue || minAltitude != float.MinValue) {
						if (randVertex.y > maxAltitude || randVertex.y < minAltitude) {
							valid = false;
							continue;
						}
					}
					if(minGroupDistance != float.MinValue)
					{
						if (groupPositions.Any (s => Mathf.Abs (Vector3.Distance (s, randVertex)) < minGroupDistance)) 
						{
							valid = false;
							continue;
						}
					}
				}
				GameObject newObject = GameObject.Instantiate<GameObject> (instantiateObject);
				newObject.transform.parent = parentObject.transform;
				newObject.transform.localPosition = randVertex + offset;
				spawned.Add (newObject);
            
			}
			Vector3 sum = Vector3.zero;
			foreach( var spawnPos in spawned.Select( s => s.transform.position ) )
				sum += spawnPos;
			groupPositions.Add( sum / spawned.Count );  
		}
    }
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
