using UnityEngine;
using System.Collections;
using System.Collections.Generic;

    [ExecuteInEditMode()]
public class RoadMesh : MonoBehaviour {

    public ProceduralMesh proceduralMesh;

    public float tileSize = 2.0f;
    public int width = 10;
    public int height = 10;
	// Use this for initialization
	void Start () {
	
	}

    [ContextMenu("Gen")]
    void spawnMesh()
    {
        GameObject block = this.gameObject;
        Mesh newMesh = new Mesh();
        List<Vector3> verticeList = new List<Vector3>();
        List<Vector2> uvList = new List<Vector2>();
        List<int> triList = new List<int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * tileSize - (width * 0.5f * tileSize), 0f, y * tileSize - (height * 0.5f * tileSize));
                Vector3 adjustedPos = this.transform.position + pos;
                float heightVal = 0.0f;

                if (proceduralMesh.getHeightForPosition(adjustedPos.x, adjustedPos.z, out heightVal))
                {
                    pos.y =  heightVal;
                }
                
                verticeList.Add(pos);
                uvList.Add(new Vector2(x/width, y/height));
                //Skip if a new square on the plane hasn't been formed
                if (x == 0 || y == 0)
                    continue;
                //Adds the index of the three vertices in order to make up each of the two tris
                triList.Add(width * x + y); //Top right
                triList.Add(width * x + y - 1); //Bottom right
                triList.Add(width * (x - 1) + y - 1); //Bottom left - First triangle
                triList.Add(width * (x - 1) + y - 1); //Bottom left 
                triList.Add(width * (x - 1) + y); //Top left
                triList.Add(width * x + y); //Top right - Second triangle
            }
        }
        newMesh.vertices = verticeList.ToArray();
        newMesh.uv = uvList.ToArray();
        newMesh.triangles = triList.ToArray();
        newMesh.RecalculateNormals();
        block.GetComponent<MeshFilter>().sharedMesh = newMesh;

        
        
    }

	// Update is called once per frame
	void Update () {
        /*
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh newMesh = filter.sharedMesh;
        for (int i=0;i<newMesh.vertexCount;i++)
        {
            Vector3 vert = transform.position+newMesh.vertices[i];
            float heightVal = 0.0f;
            if (proceduralMesh.getHeightForPosition(vert.x, vert.y, out heightVal))
            {
                vert = new Vector3(vert.x, heightVal, vert.z);
                newMesh.vertices[i] = vert;
            }
        }
         * */
        spawnMesh();
        //filter.sharedMesh = newMesh;
	}
}
