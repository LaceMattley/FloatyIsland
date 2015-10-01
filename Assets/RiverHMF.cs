using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RiverHMF : HeightMapFeature 
{
	Vector3 direction;
	ProceduralMesh proceduralMesh;
	
	List<Point> pointList = new List<Point>();
	
	class Point
	{
		public int x;
		public int y;
		public bool deadPoint;
		public Point nextPoint;
	}

	struct Vec2Int
	{
		public int x, y;

		public Vec2Int(int _x = 0, int _y = 0)
		{
			x = _x;
			y = _y;
		}

		public static bool operator ==(Vec2Int c1, Vec2Int c2) 
		{
			return c1.Equals(c2);
		}
		
		public static bool operator !=(Vec2Int c1, Vec2Int c2) 
		{
			return !c1.Equals(c2);
		}
	}


	public void OnDrawGizmosSelected ()
	{
		Gizmos.color = Color.white;
		ProceduralMesh prodMesh = GetComponent<ProceduralMesh>();
		if (pointList.Count > 0)
		{
			
			Vector3 firstLine = prodMesh.getIndexPosition(pointList[0].x, pointList[0].y);
			pointList.ForEach( f => Gizmos.DrawSphere(prodMesh.getIndexPosition(f.x, f.y),1.0f));
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
		int numStarts = 3;
		float[] highPoints = new float[numStarts];
		Vec2Int[] highPointIndices = new Vec2Int[numStarts];
		float highPt = float.MinValue;
		Vec2Int highPtIdx = new Vec2Int();
		float ceil = float.MaxValue;
		for (int start = 0; start < numStarts; ++start) {
			bool foundStart = false;
			for (int i = 0; i < width; i++) {
				for (int j = 0; j < width; j++) {
					float height = _heightmap [i, j];
					if (height > highPt && height < ceil) {
					//if (height > 0.4f && Random.Range(0,100) > 80) {
						if (!GetComponent<ProceduralMesh> ().getTerrainPresence (i, j))
							continue;
						// Check how close this is to the other start points
						if (start != 0)
						{
							bool tooClose = false;
							Vector2 newV = new Vector2((float)i, (float)j);
							for(int prv = start-1; prv >= 0; prv--)
							{
								Vector2 lastV = new Vector2((float)highPointIndices[prv].x, (float)highPointIndices[prv].y);
								if(Mathf.Abs(Vector2.Distance(lastV,newV)) < 40.0f)
								{
									tooClose = true; 
									break;
								}
							}
							if(tooClose)
								continue;
						}
						highPt = height;
						highPtIdx.x = i;
						highPtIdx.y = j;
						foundStart = true;
					}
				}
			}
			ceil = highPt;
			highPoints[start] = highPt;
			highPointIndices[start] = highPtIdx;
			highPt = float.MinValue;
		}
		for (int start = 0; start < numStarts; ++start) {
			highPt = highPoints[start];
			highPtIdx = highPointIndices[start];
			int k = highPtIdx.x;
			int L = highPtIdx.y;
			// Look for the lowest neighbour and go that way and repeat until we hit a ditch or fall off the edge
			Vec2Int[] nei = new Vec2Int[8];
			List<Vec2Int> visited = new List<Vec2Int> ();
			float velocity = 0.0f;
			float lastHeight = highPt;
			while (k < width && k > 0 && L < width && L > 0) {
				visited.Add (new Vec2Int (k, L));
				/*
				 where x is k,L and 0-7 are indexes of nei
				 2 3 4
				 1 x 5
				 0 7 6
				*/

				nei [0] = new Vec2Int (k - 1, L - 1);
				nei [1] = new Vec2Int (k - 1, L);
				nei [2] = new Vec2Int (k - 1, L + 1);
				nei [3] = new Vec2Int (k, L + 1);
				nei [4] = new Vec2Int (k + 1, L + 1);
				nei [5] = new Vec2Int (k + 1, L);
				nei [6] = new Vec2Int (k + 1, L + 1);
				nei [7] = new Vec2Int (k, L - 1);
				Vec2Int lowIdxs = new Vec2Int ();
				lowIdxs.x = -1;
				lowIdxs.y = -1;
				float lowHei = lastHeight;
				int lowIdx = -1;
				List<float> lowHeis = new List<float>(); 
				List<Vec2Int> lowIndices = new List<Vec2Int>();
				for (int m = 0; m < 8; ++m) {
					if (nei [m].x >= width || nei [m].x < 0)
						continue;
					if (nei [m].y >= width || nei [m].y < 0)
						continue;
					float height = _heightmap [nei [m].x, nei [m].y];
					if ((height < lowHei)
						&& !visited.Any (v => v == nei [m])) {
						lowHeis.Add(height);
						lowIndices.Add(nei [m]);
					}
				}

				// TODO: needs to be able to get over small 
				if(lowHeis.Count() == 0)
					break;

				int idx = Random.Range(0,lowHeis.Count());
				lowIdxs = lowIndices[idx];
				lowHei = lowHeis[idx];

				if (!GetComponent<ProceduralMesh> ().getTerrainPresence (lowIdxs.x, lowIdxs.y))
					break;

				k = lowIdxs.x;
				L = lowIdxs.y;

				if (lowIdxs.x == -1 && lowIdxs.y == -1)
					break;

				/*TODO: 
				 * Velocity creates wider river
				 * Uphil turns river
				 * Random chance to turn (& slow?)
				 * Uphil slows river
				 * Flooding areas
				 	* Overflow? Merging?
				 * Turn initially? Bias 'off' straight down?
				 * VFX
				*/
				velocity += lastHeight - _heightmap [k, L];
				lastHeight = _heightmap [k, L];
				if (velocity < 0.0f)
					Debug.Assert (velocity > 0.0f);
				float heightLoss = 0.1f * velocity;
				_heightmap [k, L] -= heightLoss;
				// Lower the neighbours too
				for (int m = 0; m < 8; ++m) {
					_heightmap [nei[m].x, nei[m].y] -= heightLoss / 3.0f;
				}
			}
		}
//
//		float averageHeight = cumulativeHeight / (width * width);
//		float topThreeQuaters = averageHeight + (averageHeight * 0.5f);
//		
//		
//		Point startingPoint = null;
//		bool foundGoodStart = false;
//		while (!foundGoodStart)
//		{
//			int randX = Random.Range(0,width);
//			int randY = Random.Range(0,width);
//			
//			float chosenHeight = _heightmap[randX,randY];
//			if (chosenHeight > topThreeQuaters && GetComponent<ProceduralMesh>().arrayValidData[randX+randY*width])
//			{
//				startingPoint = new Point();
//				startingPoint.x = randX;
//				startingPoint.y = randY;
//				startingPoint.deadPoint = false;
//				pointList.Add(startingPoint);
//				foundGoodStart = true;
//			}
//		}
//		
//		bool foundEdge = false;
//		
//		while(!foundEdge)
//		{           
//			Point lastPoint = null;
//			int pointIndex = pointList.Count - 1;
//			for (pointIndex = pointList.Count - 1; pointIndex >= 0 && lastPoint == null; pointIndex--)
//			{
//				if (!pointList[pointIndex].deadPoint)
//				{
//					lastPoint = pointList[pointIndex];
//				}
//			}
//			if (lastPoint == null)
//			{
//				Debug.Log("Ran out of points! (index: " + pointIndex + " , count: " + pointList.Count);
//				foundEdge = true;
//				break;
//			}
//			float bestHeightDifference = 0.0f;
//			Point bestPoint = null;
//			for (int i = lastPoint.x-1; i <= lastPoint.x+1; i++)
//			{
//				for (int j =lastPoint.y-1; j <= lastPoint.y+1; j++)
//				{
//					if (i >= 0 && i < width && j >= 0 &&j < width)
//					{
//						bool foundPoint = false;
//						foreach (Point point in pointList)
//						{
//							if (point.x == i && point.y == j)
//							{
//								foundPoint = true;
//							}
//						}
//						//if we haven't already got this point get the height diff.
//						if (!foundPoint)
//						{
//							float testHeight = _heightmap[i, j];
//							float currentHeight = _heightmap[lastPoint.x, lastPoint.y];
//							float heightDiff = testHeight - currentHeight;
//							if (heightDiff < 0.0 && heightDiff < bestHeightDifference )
//							{
//								bestHeightDifference = heightDiff;
//								bestPoint = new Point();
//								bestPoint.x = i;
//								bestPoint.y = j;
//								bestPoint.nextPoint = null;
//								bestPoint.deadPoint = false;                                
//							}
//						}
//					}
//				}
//			}
//			if (bestPoint == null)
//			{
//				Debug.Log("Dead point at " + lastPoint.x +"," + lastPoint.y +" after iteration num: " + pointList.Count);
//				foundEdge = true;
//				lastPoint.deadPoint = true;
//			}
//			else
//			{
//				lastPoint.nextPoint = bestPoint;
//				pointList.Add(bestPoint);
//				if (bestPoint.x == 0 || bestPoint.x == width - 1 || bestPoint.y == 0 || bestPoint.y == width - 1)
//				{
//					foundEdge = true;
//					Debug.Log("found edge after " + pointList.Count + "iterations");
//				}
//			}
//		} 
//		
//		float[,] alterMap = new float[width,width];
//		for (int i = 0; i < width; i++)
//		{
//			for (int j = 0; j < width; j++)
//			{
//				alterMap[i,j]=0.0f;
//			}
//		}
//		if (_alterHeightmap)
//		{
//			Point currentPoint = pointList[0];
//			Point nextPoint = currentPoint.nextPoint;
//			float riverWidth = 5.0f;
//			while (currentPoint!=null)
//			{
//				alterMap[currentPoint.x,currentPoint.y] = heightModifer;
//				if (nextPoint!=null)
//				{
//					Vector2 origin = new Vector2(currentPoint.x, currentPoint.y);
//					Vector2 perpDirection = new Vector2(-nextPoint.y - currentPoint.y, nextPoint.x - currentPoint.x);
//					perpDirection.Normalize();
//					for (float currentWidth = -riverWidth;currentWidth<riverWidth;currentWidth+=0.5f)
//					{
//						Vector2 position = origin+perpDirection*currentWidth;
//						int xIndex = Mathf.FloorToInt(position.x);
//						int yIndex = Mathf.FloorToInt(position.y);
//						alterMap[xIndex, yIndex] = heightModifer;
//					}
//				}
//				currentPoint = nextPoint;
//			}
//			for (int i = 0; i < width; i++)
//			{
//				for (int j = 0; j < width; j++)
//				{
//					_heightmap[i, j] += alterMap[i, j];
//				}
//			}
//		}
		
	}
	
}
