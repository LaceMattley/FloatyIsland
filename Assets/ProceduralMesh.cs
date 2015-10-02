
//#define USE_TEX

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEditor;

public class SVert
{
    public Vector3 position;
    public int xIndex;
    public int yIndex;
    public int arrayIndex;
    public bool endPoint;
    public float distanceRatio;
   public void setEndPoint()
    {
        endPoint = true;
        distanceRatio = 0.0f;
    }
}

[ExecuteInEditMode]
public class ProceduralMesh : MonoBehaviour {

    public bool randomSeed = false;
    public bool withModifiers = false;

    public float xSize = 100.0f;
    public float ySize = 100.0f;

    public float distanceBetweenPoints = 1.0f;

    public float islandShapeStrength = 2;
    List<SVert> sVertList = new List<SVert>();  

    public int PointsNum1D = 100;
    public float minimumIslandWidthRatio = 0.7f;

    float PointySize { get { return (1.0f/ PointsNum1D); } }

#if USE_TEX
    public Texture2D shapeTexture;
#endif

    public int seed;
    public bool floodFill = false;
    public Vector3 tuckPoint;
    public BottomProceduralMesh bottomPlane;

    int totalPointCount = 0;
    
    public float lakeShapeStrenght = 0.1f;
    public float lakeSize = 20.0f;

    [NonSerialized]
    public bool[] arrayValidData;
    float[] heightModifierValueData;

    bool listFinished = false;
    String generationString ="";

    
    public bool getHeightForPosition(float x, float y, out float heightFloat)
    {
        float posX = transform.position.x + x;
        float posY = transform.position.z + y;

        posX += (xSize * 0.5f);
        posY += (ySize * 0.5f);

        float tileSize = xSize / PointsNum1D;
        posX /= tileSize;
        posY /= tileSize;

        int xIndex = Mathf.FloorToInt(posX);
        int yIndex = Mathf.FloorToInt(posY);

        float diff = ((float)1 / (PointsNum1D - 1) - .5f) * xSize;
        diff -= ((float)2 / (PointsNum1D - 1) - .5f) * xSize;
        diff = 1.0f;
        float normalisedX = (posX - xIndex)/diff;
        float normalisedY = (posY - yIndex)/diff;
        float bottomLeft = 0.0f;
        float bottomRight = 0.0f;
        float topLeft = 0.0f;
        float topRight = 0.0f;

        if (heightMap == null)
        {
            //Debug.LogError("Height map is null, regenerate the terrain");
            heightFloat = 0.0f;
            return false;
        }
        bool gotHeightMap = false;
        if (xIndex >= 0 && xIndex < PointsNum1D && yIndex >= 0 && yIndex < PointsNum1D)
        {
            bottomLeft = transform.position.y + heightMap[xIndex, yIndex];
            gotHeightMap = true;
        }

        if (xIndex >= 0 && xIndex < PointsNum1D && yIndex >= 0 && yIndex < PointsNum1D)
        {
            bottomRight = transform.position.y + heightMap[xIndex + 1, yIndex];
        }

        if (xIndex >= 0 && xIndex < PointsNum1D && yIndex >= 0 && yIndex < PointsNum1D)
        {
            topRight = transform.position.y + heightMap[xIndex + 1, yIndex + 1];
        }

        if (xIndex >= 0 && xIndex < PointsNum1D && yIndex >= 0 && yIndex < PointsNum1D)
        {
            topLeft = transform.position.y + heightMap[xIndex, yIndex + 1];
        }

        float r1 = (1 - normalisedX) * bottomLeft + (normalisedX) * bottomRight;
        float r2 = (1 - normalisedX) * topLeft + (normalisedX) * topRight;

        heightFloat = (1.0f - normalisedY) * r1 + normalisedY * r2;


        return gotHeightMap;
    }
    public Vector3 getIndexPosition(int i, int j)
    {
         float zPos = ((float)j / (PointsNum1D - 1) - .5f) * xSize;
        float xPos = ((float)i / (PointsNum1D - 1) - .5f) * ySize;
        return this.transform.position + new Vector3(xPos, heightMap[i, j], zPos);
    }
    public float[,] heightMap;

    void OnDrawGizmos()
    {

        Handles.color = Color.blue;
        Handles.Label(this.transform.position, generationString);
    }

    void setTerrain(int x, int y, bool _isTerrain)
    {
        arrayValidData[x + y*PointsNum1D] = _isTerrain;
    }
    void setTerrainHeightMod(int x, int y, float _heightMod)
    {
        heightModifierValueData[x + y * PointsNum1D] = _heightMod;

    }

    public bool getTerrainPresence(int x, int y)
    {
        try
        {
            return arrayValidData[x + y * PointsNum1D];
        }
        catch
        {
            return false;
        }
    }
    
    float getTerrainHeightMod(int x, int y)
    {
        return heightModifierValueData[x + y * PointsNum1D];
    }

    void Start()
    {
        if (heightMap == null)
        {
            StartGeneration();
        }
    }
    public Vector3 getPositionForIndex(int x, int y)
    {
        float zPos = ((float)y / (PointsNum1D - 1) - .5f) * xSize;
        float yPos = heightMap[x,y];
        float xPos = ((float)x / (PointsNum1D - 1) - .5f) * ySize;
        return new Vector3(xPos, yPos, zPos);
    }
    [ContextMenu("Generate")]
    void StartGeneration()
    {
        StopCoroutine(GenerateMesh());
        IEnumerator coroutine = GenerateMesh();
        generationString = "Started Generating"; 
        EditorApplication.update += delegate { coroutine.MoveNext(); };
    }

    IEnumerator GenerateMesh()
    {
        arrayValidData = new bool[PointsNum1D* PointsNum1D];
        heightModifierValueData = new float[PointsNum1D * PointsNum1D];
        int halfPointsNum1D = PointsNum1D / 2;

        //clear the texture.
        for (int z = 0; z < PointsNum1D; z++)
        {            
            for (int i=0;i<PointsNum1D;i++)
            {
                //Color thisColor = i < heightVal * PointsNum1D ? Color.red : Color.red;               
                setTerrain(i,z, false);
            }                            
        }

        if (randomSeed)
        {
            seed = System.DateTime.Now.Minute + System.DateTime.Now.Millisecond + System.DateTime.Now.Second;
        }
        UnityEngine.Random.seed = seed;
        float perlinStart = UnityEngine.Random.Range(0.0f, 1000.0f);

        float radius = 50.0f;
        float startVal = -0.5f * Mathf.PI;
        int previousY = halfPointsNum1D + Mathf.FloorToInt(radius * Mathf.Sin(startVal));
        
        generationString = "Generating island";

        for (float theta = startVal; theta < startVal+Mathf.PI*2.0f; theta += 0.0001f)
        {
            float heightVal = Mathf.PerlinNoise(((theta - startVal) * islandShapeStrength), 0.0f);

            float radiusRatio = minimumIslandWidthRatio + heightVal*(1.0f-minimumIslandWidthRatio);
            float adjustedRadius = PointsNum1D * radiusRatio * 0.5f;
            int x = halfPointsNum1D + Mathf.FloorToInt(adjustedRadius * Mathf.Cos(theta));
            int y = halfPointsNum1D + Mathf.FloorToInt(adjustedRadius * Mathf.Sin(theta));
            setTerrain(x, y, true);
            setTerrain(x+1, y+1, true);            
        }

        //bool floodFill = false;
        if (floodFill)
        {
            FloodFillArea<bool>(arrayValidData, halfPointsNum1D, halfPointsNum1D, true);

        }
        yield return new WaitForSeconds(.1f);

        generationString = "Smoothing";
        int smoothingIters =3;               
        for (int iter = 0; iter < smoothingIters; iter++)
        {
            for (int x = 1; x < PointsNum1D - 1; x++)
            {
                for (int z = 1; z < PointsNum1D - 1; z++)
                {
                    float cumulativeVal = 0.0f;
                    for (int i = x - 1; i < x + 2; i++)
                    {

                        for (int j = z - 1; j < z + 2; j++)
                        {
                            cumulativeVal += heightModifierValueData[i+j*PointsNum1D];
                        }
                    }
                    if (cumulativeVal > 0.0f)
                    {
                        setTerrainHeightMod(x,z,cumulativeVal/9.0f);      
                    }
                }
            }
        }
        listFinished = false;
        
        generationString = "Prepping the list";
        GenerateList(false);

                        
        generationString = "Gathering vertex data";
        int vertCount = 3 * 360;
        Vector3[] verticesArray = new Vector3[sVertList.Count];
        Vector2[] uvs = new Vector2[sVertList.Count];
        int?[,] arrayIndices = new int?[PointsNum1D, PointsNum1D];
        for (int i = 0; i < sVertList.Count;i++ )
        {
            arrayIndices[sVertList[i].xIndex, sVertList[i].yIndex] = sVertList[i].arrayIndex;
            verticesArray[i] = sVertList[i].position;
            uvs[i] = new Vector2((float)sVertList[i].xIndex / (PointsNum1D - 1), (float)sVertList[i].yIndex / (PointsNum1D - 1));
            if ((i % vertCount) == 0)
            {
                yield return new WaitForSeconds(0.0f); ;
            }
        }
        
        List<int> triangleArray = new List<int>();

        int num = 0;
        foreach (SVert vert in sVertList)
        {
            int indexX = vert.xIndex;
            int indexY = vert.yIndex;

            int? upVert = arrayIndices[indexX, indexY + 1];
            int? RightVert = arrayIndices[indexX + 1, indexY];
            //time to add a triangle
            if (upVert.HasValue && RightVert.HasValue)
            {
                triangleArray.Add(vert.arrayIndex);
                triangleArray.Add(upVert.Value);
                triangleArray.Add(RightVert.Value);
            }


            int? leftVert = arrayIndices[indexX - 1, indexY];
            int? downVert = arrayIndices[indexX, indexY - 1];
            if (leftVert.HasValue && downVert.HasValue)
            {
                triangleArray.Add(leftVert.Value);
                triangleArray.Add(vert.arrayIndex);
                triangleArray.Add(downVert.Value);
            }
            num++;

            if ((triangleArray.Count % vertCount) == 0)
            {
                generationString = "Getting all the triangles." + num + "/" + sVertList.Count;
                yield return new WaitForSeconds(0.0f);
            }
        }

        int nbFaces = (PointsNum1D - 1) * (PointsNum1D - 1);

       
        generationString = "Copying data to mesh";
        int[] triangles = new int[triangleArray.Count];
        triangleArray.CopyTo(triangles);

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = verticesArray;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        generationString = "Calculating stuff";
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.Optimize();
        totalPointCount = sVertList.Count;
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter)
        {
            if (GetComponent<MeshCollider>() == null)
            {
                gameObject.AddComponent<MeshCollider>();
            }
            GetComponent<MeshCollider>().sharedMesh = mesh;
            filter.sharedMesh = mesh;            
        }
        

        generationString = "";
        yield return new WaitForSeconds(.1f);
    }


    void GenerateList(bool fillMesh)
    {
        int halfPointsNum1D = PointsNum1D / 2;

        if (randomSeed)
        {
        seed = System.DateTime.Now.Minute + System.DateTime.Now.Millisecond + System.DateTime.Now.Second;        
       
        }
        UnityEngine.Random.seed = seed;
        float perlinStart = UnityEngine.Random.Range(0.0f, 1000.0f);

        sVertList.Clear();        

        heightMap = new float[PointsNum1D,PointsNum1D];
        for (int z = 0; z < PointsNum1D; z++)
        {       
            for (int x = 0; x < PointsNum1D; x++)
            {
          
                if (getTerrainPresence(x,z))
                {                   
                    heightMap[x,z] = 0.0f;                    
                }
            }            
        }
        
        if (withModifiers)
        {
            foreach (HeightMapFeature feature in GetComponents<HeightMapFeature>())
            {
                if (feature.isEnabled)
                {
                    feature.generateOnHeightMap(ref heightMap, true);
                }
            }
        }
        for (int z = 0; z < PointsNum1D; z++)
        {
            // [ -xSize / 2, xSize / 2 ]
            float zPos = (z-PointsNum1D*0.5f) * distanceBetweenPoints;
            for (int x = 0; x < PointsNum1D; x++)
            {
                if (getTerrainPresence(x, z))
                {
                    float xPos = (x-PointsNum1D*0.5f) * distanceBetweenPoints ;
                    Vector3 newPos = new Vector3(xPos, heightMap[x, z], zPos);
                    float closenessToEdgeX = Mathf.Abs(((float)x - (float)halfPointsNum1D) / (float)halfPointsNum1D);
                    float closenessToEdgeZ = Mathf.Abs(((float)z - (float)halfPointsNum1D) / (float)halfPointsNum1D);

                    SVert newVert = new SVert();
                    newVert.position = newPos;
                    newVert.xIndex = x;
                    newVert.yIndex = z;
                    newVert.arrayIndex = sVertList.Count;

                    bool isEdgePoint = false;
                    if (!getTerrainPresence(x - 1, z)
                        || !getTerrainPresence(x + 1, z)
                        || !getTerrainPresence(x, z - 1)
                        || !getTerrainPresence(x, z + 1)
                        || !getTerrainPresence(x - 1, z - 1)
                        || !getTerrainPresence(x + 1, z + 1)
                        || !getTerrainPresence(x + 1, z - 1)
                        || !getTerrainPresence(x - 1, z + 1)
                        )
                    {
                        isEdgePoint = true;
                    }
                    newVert.endPoint = isEdgePoint;
                    newVert.distanceRatio = 1.0f - Mathf.Clamp01(Mathf.Sqrt(closenessToEdgeX * closenessToEdgeX + closenessToEdgeZ * closenessToEdgeZ));
                    sVertList.Add(newVert);
                }
            }
        }


        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter && fillMesh)
        {
            Vector3[] vecs = new Vector3[sVertList.Count];
            for (int i = 0; i < sVertList.Count; i++)
            {
                vecs[i] = sVertList[i].position;
            }
            Mesh mesh = filter.sharedMesh;
            mesh.vertices = vecs;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.Optimize();
            filter.sharedMesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }
        listFinished = true;
    }

    [ContextMenu("RegenerateTerrain")]
    void regenerateTerrain()
    {

        GenerateList(true);
    }
    [ContextMenu("Generate All")]
    void generateAll()
    {
        GenerateMesh();
        setBottomMesh();
    }
    [ContextMenu("Generate Bottom")]
    void setBottomMesh()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter)
        {
            if (bottomPlane)
            {

                bottomPlane.GetComponent<BottomProceduralMesh>().setMesh(filter.sharedMesh, totalPointCount,sVertList);
            }
        }
    }

    void GetVertData()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter)
        {
            Mesh mesh = filter.sharedMesh;
            Vector3[] vec = mesh.vertices;
        }
    }
	// Update is called once per frame
	void Update () {
	
	}
    SVert findVert(int x, int y,List<SVert> vertList)
    {
        foreach(SVert vert in vertList)
        {
            if (vert.xIndex == x && vert.yIndex == y)
            {
                return vert;
            }
        }
        return null;
    }
    public struct Point
    {
        public short x;
        public short y;
        public Point(short aX, short aY) { x = aX; y = aY; }
        public Point(int aX, int aY) : this((short)aX, (short)aY) { }
    }

    void Awake()
    {

    }
    public void FloodFillArea<T>(T[] values, int aX, int aY, T fillVal) where T : System.IComparable<T>
    {
        int w = PointsNum1D;
        int h = PointsNum1D;
        T[] colors = values;
        T refCol = colors[aX + aY * w];
        Queue<Point> nodes = new Queue<Point>();
        nodes.Enqueue(new Point(aX, aY));
        while (nodes.Count > 0)
        {
            Point current = nodes.Dequeue();
            for (int i = current.x; i < w; i++)
            {
                T C = colors[i + current.y * w];
                if (C.CompareTo(refCol)!=0 || C.CompareTo(fillVal)==0)
                    break;
                colors[i + current.y * w] = fillVal;
                if (current.y + 1 < h)
                {
                    C = colors[i + current.y * w + w];
                    if (C.CompareTo(refCol)==0 && C.CompareTo(fillVal)!=0)
                        nodes.Enqueue(new Point(i, current.y + 1));
                }
                if (current.y - 1 >= 0)
                {
                    C = colors[i + current.y * w - w];
                    if (C.CompareTo(refCol)==0 && C.CompareTo(fillVal)!=0)
                        nodes.Enqueue(new Point(i, current.y - 1));
                }
            }
            for (int i = current.x - 1; i >= 0; i--)
            {
                T C = colors[i + current.y * w];
                if (C.CompareTo(refCol)!=0 || C.CompareTo(fillVal)==0)
                    break;
                colors[i + current.y * w] = fillVal;
                if (current.y + 1 < h)
                {
                    C = colors[i + current.y * w + w];
                    if (C.CompareTo(refCol)==0 && C.CompareTo(fillVal)!=0)
                        nodes.Enqueue(new Point(i, current.y + 1));
                }
                if (current.y - 1 >= 0)
                {
                    C = colors[i + current.y * w - w];
                    if (C.CompareTo(refCol)==0 && C.CompareTo(fillVal)!=0)
                        nodes.Enqueue(new Point(i, current.y - 1));
                }
            }
        }

        //aTex.SetPixels(colors);
    }

}
