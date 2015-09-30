using UnityEngine;
using System.Collections;

public class MoveAndRotate : MonoBehaviour {

    public GameObject targetObject;
    float currentT = 0.0f;
    public float radius = 100.0f;
    public float heightOffset = 50.0f;
    public float heightChange = 30.0f;
    public float heightChangeSpeed = 0.5f;
    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        currentT += Time.deltaTime;
        transform.LookAt(targetObject.transform);
        float x =  radius * Mathf.Cos(currentT);
        float y =  radius * Mathf.Sin(currentT);

        float height = heightOffset + Mathf.Sin(currentT * heightChangeSpeed) * heightChange;
        transform.position = targetObject.transform.position + new Vector3(x,height, y);
	}
}
