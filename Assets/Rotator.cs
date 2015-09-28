using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {

    public Vector3 rotationValues;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.RotateAround(new Vector3(1.0f,0.0f,0.0f),rotationValues.x*Time.deltaTime);
        //this.transform.localEulerAngles = this.transform.localEulerAngles + rotationValues * Time.deltaTime;
	}
}
