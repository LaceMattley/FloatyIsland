using UnityEngine;
using System.Collections;

public class SpawnSea : MonoBehaviour {

    public float seperation = 50.0f;
    public int count = 20;
    public GameObject waterObject;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
    [ContextMenu("Gen")]
	void Gen () {
        for (int i = 0; i < count; i++)
        {
            for (int j=0;j<count;j++)
            {
                Vector3 position = transform.position + new Vector3((count / 2) * seperation, 0.0f, (count / 2) * seperation);
                position += new Vector3(i * seperation, 0.0f, j * seperation);
                GameObject newGameObject = GameObject.Instantiate(waterObject as GameObject, position, Quaternion.identity) as GameObject;
                newGameObject.transform.parent = this.transform;

            }
        }
	}
}
