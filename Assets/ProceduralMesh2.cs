using UnityEngine;
using System.Collections;

public class ProceduralMesh2 : MonoBehaviour {

    public float xSize = 100.0f;
    public float ySize = 100.0f;
    public float maxHeight = 5.0f;

    public int PointsNum1D = 2000;
    public float perlinStrength = 0.1f;

    public float flattenDistance =5000.0f;
    public float islandShapeStrength = 0.1f;
    int TotalXPoint { get { return Mathf.CeilToInt(xSize * PointsNum1D); } }
    int TotalYPoint { get { return Mathf.CeilToInt(ySize * PointsNum1D); } }

    float PointWidth { get { return (1.0f/ PointsNum1D); } }

    public Texture2D shapeTexture;

    public int seed;
    public bool floodFill = false;
    public Vector3 tuckPoint;
	// Use this for initialization
	void Start () {
	
	}
	
    [ContextMenu("Generate")]
    void GenerateMesh()
    {

        // You can change that line to provide another MeshFilter

        Mesh mesh = new Mesh() ;
        mesh.Clear();

        float length = xSize;
        float width = ySize;
        int resX = PointsNum1D; // 2 minimum
        int resZ = PointsNum1D;

        int halfResZ = resZ / 2;

        shapeTexture = new Texture2D(PointsNum1D, PointsNum1D);
        for (int z = 0; z < resZ; z++)
        {
            float heightVal = Mathf.PerlinNoise(z * islandShapeStrength, 0);
            for (int i=0;i<resX;i++)
            {
                Color thisColor = i < heightVal * resX ? Color.red : Color.red;
                //Color thisColor = 
                shapeTexture.SetPixel(i, z, thisColor);

            }                            
        }
        seed = System.DateTime.Now.Minute + System.DateTime.Now.Millisecond + System.DateTime.Now.Second;
        //Random.seed = seed;
        
        float radius = 50.0f;
        float startVal = -0.5f * Mathf.PI;
        int previousY = halfResZ + Mathf.FloorToInt(radius * Mathf.Sin(startVal));
        float perlinStart = 0.0f;// 10.0f;// Random.Range(0.0f, 1000.0f);
        for (float theta = startVal; theta < startVal+Mathf.PI*2.0f; theta += 0.0001f)
        {
            float heightVal = Mathf.PerlinNoise((theta - startVal) * islandShapeStrength, 0.0f) * 50.0f;
            float adjustedRadius = radius + heightVal;
            int x = halfResZ + Mathf.FloorToInt(adjustedRadius * Mathf.Cos(theta));
            int y = halfResZ + Mathf.FloorToInt(adjustedRadius * Mathf.Sin(theta));
            shapeTexture.SetPixel(x, y, Color.blue);
            shapeTexture.SetPixel(x+1, y+1, Color.blue);            
        }
        //bool floodFill = false;
        if (floodFill)
        {
            shapeTexture.FloodFillArea(halfResZ, halfResZ, Color.blue);
        }
           shapeTexture.Apply();
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("Albedo",shapeTexture);
        Random randVals = new Random(); ;
        #region Vertices
        Vector3[] vertices = new Vector3[resX * resZ];
        for (int z = 0; z < resZ; z++)
        {
            // [ -length / 2, length / 2 ]
            float zPos = ((float)z / (resZ - 1) - .5f) * length;
            for (int x = 0; x < resX; x++)
            {
                
                // [ -width / 2, width / 2 ]
                float xPos = ((float)x / (resX - 1) - .5f) * width;
                float heightVal = Mathf.PerlinNoise(x * perlinStrength, z * perlinStrength)*maxHeight;                
                Vector3 newPos = new Vector3(xPos, heightVal, zPos);
                if (shapeTexture.GetPixel(x,z).b == 0.0f)
                {
                    float closenessToEdgeX = Mathf.Abs(((float)x - (float)halfResZ) / (float)halfResZ);
                    float closenessToEdgeZ = Mathf.Abs(((float)z - (float)halfResZ) / (float)halfResZ);
                    float ratioVal = 1.0f - Mathf.Pow((closenessToEdgeX + closenessToEdgeZ), 3.0f);
                    newPos = tuckPoint;// new Vector3(0.0f, heightVal * ratioVal, 0.0f);
                }
                vertices[x + z * resX] = newPos;
                
            }
        }
        #endregion

        #region Normales
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < normales.Length; n++)
            normales[n] = Vector3.up;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int v = 0; v < resZ; v++)
        {
            for (int u = 0; u < resX; u++)
            {
                uvs[u + v * resX] = new Vector2((float)u / (resX - 1), (float)v / (resZ - 1));
            }
        }
        #endregion

        #region Triangles
        int nbFaces = (resX - 1) * (resZ - 1);
        int[] triangles = new int[nbFaces * 6];
        int t = 0;
        for (int face = 0; face < nbFaces; face++)
        {
            // Retrieve lower left corner from face ind
            int i = face % (resX - 1) + (face / (resZ - 1) * resX);

            triangles[t++] = i + resX;
            triangles[t++] = i + 1;
            triangles[t++] = i;

            triangles[t++] = i + resX;
            triangles[t++] = i + resX + 1;
            triangles[t++] = i + 1;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.Optimize();

        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter)
        {
            filter.mesh = mesh;            
        }

        //MeshRenderer rend = GetComponent<MeshRenderer>();
        
        //rend.material.SetTexture("_MainTex",shapeTexture);

    }
	// Update is called once per frame
	void Update () {
	
	}
}
