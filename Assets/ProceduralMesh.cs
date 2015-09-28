
//#define USE_TEX

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

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
public class ProceduralMesh : MonoBehaviour {

    public bool randomSeed = false;
    public bool withModifiers = false;

    public float xSize = 100.0f;
    public float ySize = 100.0f;    

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

    public Vector3 getIndexPosition(int i, int j)
    {
         float zPos = ((float)j / (PointsNum1D - 1) - .5f) * xSize;
        float xPos = ((float)i / (PointsNum1D - 1) - .5f) * ySize;
        return this.transform.position + new Vector3(xPos, heightMap[i, j], zPos);
    }
    public float[,] heightMap;
	// Use this for initialization
	void Start () {
	
	}
	
    void setTerrain(int x, int y, bool _isTerrain)
    {
        arrayValidData[x + y*PointsNum1D] = _isTerrain;
#if USE_TEX
        shapeTexture.SetPixel(x, y, _isTerrain ? Color.red : Color.blue);
#endif
    }
    void setTerrainHeightMod(int x, int y, float _heightMod)
    {
        heightModifierValueData[x + y * PointsNum1D] = _heightMod;
#if USE_TEX
        shapeTexture.SetPixel(x, y, _isTerrain ? Color.red : Color.blue);
#endif
    }

    bool getTerrainPresence(int x, int y)
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
    [ContextMenu("Generate")]
    void GenerateMesh()
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
#if USE_TEX
            //shapeTexture.FloodFillArea(halfPointsNum1D, halfPointsNum1D, Color.blue);
#endif
        }

#if USE_TEX
        shapeTexture.Apply();
#endif
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
#if USE_TEX
                            cumulativeVal += shapeTexture.GetPixel(i, j).g;
#else
                            cumulativeVal += heightModifierValueData[i+j*PointsNum1D];
#endif
                        }
                    }
                    if (cumulativeVal > 0.0f)
                    {
                        setTerrainHeightMod(x,z,cumulativeVal/9.0f);      
                    }
                }
            }
        }
        #if USE_TEX
        shapeTexture.Apply();
#endif

        GenerateList();

        List<int> triangleArray = new List<int>();
        foreach (SVert vert in sVertList)
        {
            int indexX = vert.xIndex;
            int indexY = vert.yIndex;

            SVert upVert = findVert(indexX, indexY + 1, sVertList);
            SVert RightVert = findVert(indexX + 1, indexY, sVertList);
            //time to add a triangle
            if (upVert!=null && RightVert!= null)
            {
                triangleArray.Add(vert.arrayIndex);
                triangleArray.Add(upVert.arrayIndex);
                triangleArray.Add(RightVert.arrayIndex);
            }


            SVert leftVert = findVert(indexX - 1, indexY, sVertList);
            SVert downVert = findVert(indexX, indexY - 1, sVertList);
            if (leftVert != null && downVert != null)
            {
                triangleArray.Add(leftVert.arrayIndex);
                triangleArray.Add(vert.arrayIndex);
                triangleArray.Add(downVert.arrayIndex);
            }

        }
        
        Vector3[] verticesArray = new Vector3[sVertList.Count];
        Vector2[] uvs = new Vector2[sVertList.Count];
        for (int i = 0; i < sVertList.Count;i++ )
        {
            verticesArray[i] = sVertList[i].position;
            uvs[i] = new Vector2((float)sVertList[i].xIndex / (PointsNum1D - 1), (float)sVertList[i].yIndex / (PointsNum1D - 1));       
        }
        
        int nbFaces = (PointsNum1D - 1) * (PointsNum1D - 1);

       
        int[] triangles = new int[triangleArray.Count];
        triangleArray.CopyTo(triangles);

        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = verticesArray;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.Optimize();
        totalPointCount = sVertList.Count;
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter)
        {
            filter.sharedMesh = mesh;            
        }

    }


    private void GenerateList()
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
                feature.generateOnHeightMap(ref heightMap,true);
            }
        }
        for (int z = 0; z < PointsNum1D; z++)
        {
            // [ -xSize / 2, xSize / 2 ]
            float zPos = ((float)z / (PointsNum1D - 1) - .5f) * xSize;
            for (int x = 0; x < PointsNum1D; x++)
            {
                if (getTerrainPresence(x, z))
                {
                    float xPos = ((float)x / (PointsNum1D - 1) - .5f) * ySize;
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
        if (filter)
        {
            Mesh mesh = filter.sharedMesh;
            Vector3[] vecs = new Vector3[sVertList.Count];
            for (int i = 0; i < sVertList.Count; i++)
            {
                vecs[i] = sVertList[i].position;            
            }
            mesh.vertices = vecs;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.Optimize();
            filter.sharedMesh = mesh;
        }
    }

    [ContextMenu("RegenerateTerrain")]
    void regenerateTerrain()
    {

        GenerateList();
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
