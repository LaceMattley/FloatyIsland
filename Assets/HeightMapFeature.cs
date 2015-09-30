using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public abstract class HeightMapFeature : MonoBehaviour {

    public bool isEnabled = true;
    public abstract void generateOnHeightMap(ref float[,] _heightmap, bool _alterHeightmap);

    protected int HeightMapDimension { get { return GetComponent<ProceduralMesh>().PointsNum1D; } }
   public SVert GetVert(ref List<SVert> _vertList, int xIndex, int yIndex)
   {
       //oh god this is so inefficient
       SVert vert = _vertList.Find(x => x.xIndex == xIndex && x.yIndex == yIndex);
       return vert;
   }
}
