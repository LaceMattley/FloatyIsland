using UnityEngine;
using System.Collections;

public class PerlinNoiseFeature : HeightMapFeature {

    public float perlinStrength = 0.008f;
    public float maxHeight = 5.0f;
    public float power = 1.0f;
    public override void generateOnHeightMap(ref float[,] _heightmap, bool _alterHeightmap)
    {
        float perlinStart = Random.Range(0.0f, 1000.0f);
        if (_alterHeightmap)
        {
            for (int x = 0; x < HeightMapDimension; x++)
            {
                // [ -xSize / 2, xSize / 2 ]            
                for (int z = 0; z < HeightMapDimension; z++)
                {
                    //pow is so that high things become higher without making everything peaky.
                    float heightVal = Mathf.Pow(SimplexNoise.Generate(perlinStart + x * perlinStrength, perlinStart + z * perlinStrength),power) * maxHeight;
                    _heightmap[x, z] += heightVal;
                }
            }
        }                
    }
}
