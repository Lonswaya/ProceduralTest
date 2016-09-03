using UnityEngine;
using System.Collections;

public class ProceduralLandGenerator : MonoBehaviour {
    Mesh mesh;

    public float heightFactor = 1;


    class VertexInfo
    {
        Vector3 ravineDirection; //null if there is no current ravine going through here
        
        float height;
        int landType; //biome basically


    }

	// Use this for initialization
	void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] newVerts = mesh.vertices;
        float lastVal = 0;
        for (int i = 0; i < newVerts.Length; i++)
        {
            float factor = findNearbyAverages(i, newVerts, lastVal);

            heightFactor += Random.Range(-heightFactor, heightFactor) / 7;

            //newVerts[i] += ((factor / transform.localScale.x) * Random.Range(-.5f * heightFactor, heightFactor) * Vector3.up); //this causes jagged lines everywhere
            newVerts[i] += (factor / transform.localScale.x) * Vector3.up + Random.Range(-1 * heightFactor, heightFactor) * Vector3.up;
            lastVal = newVerts[i].y;
        }
        mesh.vertices = newVerts;
	}
	
	float findNearbyAverages(int index, Vector3[] newVerts, float lastVal)
    {
        int hits = 1;
        float total = 1;
        int newIndex;
        //I = current position
        //O = current focus
        //X = not used at the moment

        //   X 
        //  OIX 
        //   X
        if (lastVal != 0)
        {
            total += lastVal;
            hits++;
        }
        //   X 
        //  XIO 
        //   X 
        newIndex = index + 1;
        if (newIndex < newVerts.Length)
        {
            total += newVerts[newIndex].y;
            hits++;
        }
        //   X 
        //  XIX 
        //   O
        //we assume this is a square platform
        newIndex = index - Mathf.RoundToInt(Mathf.Sqrt(newVerts.Length));
        if (newIndex >= 0)
        {
            total += newVerts[newIndex].y;
            hits++;
        }
        //   O 
        //  XIX 
        //   X
        //we assume this is a square platform
        newIndex = index + Mathf.RoundToInt(Mathf.Sqrt(newVerts.Length));
        if (newIndex < newVerts.Length)
        {
            total += newVerts[newIndex].y;
            hits++;
        }

        return total/hits;
    }
}
