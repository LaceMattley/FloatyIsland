using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RiverPath : HeightMapFeature {

    public int startIndexX = 10;
    public int startIndexY = 10;

    Vector3 direction;
    ProceduralMesh proceduralMesh;
	// Use this for initialization

    List<Point> pointList = new List<Point>();

    public float heightModifer = -5.0f;

    class Point
    {
        public int x;
        public int y;
        public bool deadPoint;
        public Point nextPoint;
    }

    public void OnDrawGizmosSelected ()
    {
        Gizmos.color = Color.white;
		ProceduralMesh prodMesh = GetComponent<ProceduralMesh>();
        if (pointList.Count > 0)
        {
            
            Vector3 firstLine = prodMesh.getIndexPosition(pointList[0].x, pointList[0].y);
            Gizmos.DrawSphere(firstLine,5.0f);
            Point point = pointList[0];
            while (point!=null)            
            {
                Vector3 pointPosition = prodMesh.getIndexPosition(point.x, point.y);
                Gizmos.DrawLine(firstLine, pointPosition);
                firstLine = pointPosition;
                point = point.nextPoint;
            }
        }
    }
    [ContextMenu("Generate")]
    public void Generate()
    {
        float[,] heightMap =GetComponent<ProceduralMesh>().heightMap;
        generateOnHeightMap(ref heightMap,false);

    }
    public override void generateOnHeightMap(ref float[,] _heightmap, bool _alterHeightmap)
    {
        int width = GetComponent<ProceduralMesh>().PointsNum1D;
        pointList.Clear();
        float cumulativeHeight = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                cumulativeHeight += _heightmap[i, j];
            }
        }
        float averageHeight = cumulativeHeight / (width * width);
        float topThreeQuaters = averageHeight + (averageHeight * 0.5f);


        Point startingPoint = null;
        bool foundGoodStart = false;
        while (!foundGoodStart)
        {
            int randX = Random.Range(0,width);
            int randY = Random.Range(0,width);

            float chosenHeight = _heightmap[randX,randY];
            if (chosenHeight > topThreeQuaters && GetComponent<ProceduralMesh>().arrayValidData[randX+randY*width])
            {
                startingPoint = new Point();
                startingPoint.x = randX;
                startingPoint.y = randY;
                startingPoint.deadPoint = false;
                pointList.Add(startingPoint);
                foundGoodStart = true;
            }
        }

        bool foundEdge = false;
        
        while(!foundEdge)
        {           
            Point lastPoint = null;
            int pointIndex = pointList.Count - 1;
            for (pointIndex = pointList.Count - 1; pointIndex >= 0 && lastPoint == null; pointIndex--)
            {
                if (!pointList[pointIndex].deadPoint)
                {
                    lastPoint = pointList[pointIndex];
                }
            }
            if (lastPoint == null)
            {
                Debug.Log("Ran out of points! (index: " + pointIndex + " , count: " + pointList.Count);
                foundEdge = true;
                break;
            }
            float bestHeightDifference = 0.0f;
            Point bestPoint = null;
            for (int i = lastPoint.x-1; i <= lastPoint.x+1; i++)
            {
                for (int j =lastPoint.y-1; j <= lastPoint.y+1; j++)
                {
                    if (i >= 0 && i < width && j >= 0 &&j < width)
                    {
                        bool foundPoint = false;
                        foreach (Point point in pointList)
                        {
                            if (point.x == i && point.y == j)
                            {
                                foundPoint = true;
                            }
                        }
                        //if we haven't already got this point get the height diff.
                        if (!foundPoint)
                        {
                            float testHeight = _heightmap[i, j];
                            float currentHeight = _heightmap[lastPoint.x, lastPoint.y];
                            float heightDiff = testHeight - currentHeight;
                            if (heightDiff < 0.0 && heightDiff < bestHeightDifference )
                            {
                                bestHeightDifference = heightDiff;
                                bestPoint = new Point();
                                bestPoint.x = i;
                                bestPoint.y = j;
                                bestPoint.nextPoint = null;
                                bestPoint.deadPoint = false;                                
                            }
                        }
                    }
                }
            }
            if (bestPoint == null)
            {
                Debug.Log("Dead point at " + lastPoint.x +"," + lastPoint.y +" after iteration num: " + pointList.Count);
                foundEdge = true;
                lastPoint.deadPoint = true;
            }
            else
            {
                lastPoint.nextPoint = bestPoint;
                pointList.Add(bestPoint);
                if (bestPoint.x == 0 || bestPoint.x == width - 1 || bestPoint.y == 0 || bestPoint.y == width - 1)
                {
                    foundEdge = true;
                    Debug.Log("found edge after " + pointList.Count + "iterations");
                }
            }
        } 
     
        float[,] alterMap = new float[width,width];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                alterMap[i,j]=0.0f;
            }
        }
        if (_alterHeightmap)
        {
            Point currentPoint = pointList[0];
            Point nextPoint = currentPoint.nextPoint;
            float riverWidth = 5.0f;
            while (currentPoint!=null)
            {
                alterMap[currentPoint.x,currentPoint.y] = heightModifer;
                if (nextPoint!=null)
                {
                    Vector2 origin = new Vector2(currentPoint.x, currentPoint.y);
                    Vector2 perpDirection = new Vector2(-nextPoint.y - currentPoint.y, nextPoint.x - currentPoint.x);
                    perpDirection.Normalize();
                    for (float currentWidth = -riverWidth;currentWidth<riverWidth;currentWidth+=0.5f)
                    {
                        Vector2 position = origin+perpDirection*currentWidth;
                        int xIndex = Mathf.FloorToInt(position.x);
                        int yIndex = Mathf.FloorToInt(position.y);
                        alterMap[xIndex, yIndex] = heightModifer;
                    }
                }
                currentPoint = nextPoint;
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    _heightmap[i, j] += alterMap[i, j];
                }
            }
        }
        
    }

}
