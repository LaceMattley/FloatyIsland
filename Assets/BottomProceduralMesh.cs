using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BottomProceduralMesh : MonoBehaviour {

    public int smoothintIterations = 1;
    public float smoothingStrenght = 0.1f;
    public float depth = -10.0f;
    public void setMesh(Mesh copyMesh, int verticesCount, List<SVert> vertList)
    {
        Mesh mesh = new Mesh();
        mesh.Clear();
        Vector3[] verts = new Vector3[verticesCount];
        for (int i = 0; i < verticesCount;i++)
        {
            verts[i] = copyMesh.vertices[i];
            float distRatio = vertList[i].endPoint ? 0.0f : vertList[i].distanceRatio;
            verts[i] += new Vector3(0.0f, depth * distRatio, 0.0f);
            
        }

        
        int numTriangles = copyMesh.triangles.GetLength(0);
        int[] triangles = new int[numTriangles];
        copyMesh.triangles.CopyTo(triangles, 0);
        for (int i = 0; i < numTriangles; i+=3)
        {
            int temp = triangles[i];
            triangles[i] = triangles[i + 2];
            triangles[i+2] = temp;

        }
        for (int smoothIter = 0; smoothIter < smoothintIterations; smoothIter++)
        {
            for (int i = 0; i < numTriangles; i += 3)
            {
                SVert vert1 = vertList[triangles[i]];
                SVert vert2 = vertList[triangles[i + 1]];
                SVert vert3 = vertList[triangles[i + 2]];

                Vector3 pos1 = verts[triangles[i]];
                Vector3 pos2 = verts[triangles[i + 1]];
                Vector3 pos3 = verts[triangles[i + 2]];

                float avHeight = (pos1.y + pos2.y + pos3.y) / 3;
                if (!vert1.endPoint)
                {
                    Vector3 thisVec = verts[triangles[i]];
                    verts[triangles[i]] = new Vector3(thisVec.x, thisVec.y + (( avHeight - thisVec.y) * smoothingStrenght), thisVec.z);
                }
                if (!vert2.endPoint)
                {
                    Vector3 thisVec = verts[triangles[i + 1]];
                    verts[triangles[i + 1]] = new Vector3(thisVec.x, thisVec.y +(( avHeight - thisVec.y) * smoothingStrenght), thisVec.z);
                }
                if (!vert3.endPoint)
                {
                    Vector3 thisVec = verts[triangles[i + 2]];
                    verts[triangles[i + 2]] = new Vector3(thisVec.x, + thisVec.y + (( avHeight - thisVec.y) * smoothingStrenght), thisVec.z);
                }
            }
        }
        mesh.vertices = verts;
        mesh.uv = copyMesh.uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.Optimize();
        GetComponent<MeshFilter>().sharedMesh = mesh;

    }

    
}


