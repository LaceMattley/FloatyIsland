using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class WorldPosToShader : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Renderer rend = GetComponent<Renderer>();
        Material mat = rend.material;
        mat.SetVector("_WorldPos",this.transform.position);
	}
}
