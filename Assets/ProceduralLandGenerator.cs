using UnityEngine;
using UnityEditor;
using System.Collections;

public class ProceduralLandGenerator : MonoBehaviour {
    Mesh mesh;
    Vector3[] originalVerts;


    /** Basically something I am trying out to generate a landscape mesh at runtime
     *  Unoptimized, trying out new things
     *  
     *  #Requirements: NEEDS TO BE A SQUARE MESH
     */
    [Range(0.0f, 1.0f)]
    public float heightFactor = 1;
    [Range(0, 50)]
    public int mountains = 4;
    [Range(1, 10)]
    public int mountainWidth = 5;
    [Range(0.0f, 2.0f)]
    public float mountainHeight = 1;

    public int seed;

    // This is a method that overrides the GUI in the editor, 
    // allowing us to regenerate the terrain at a button's press.
    [CustomEditor(typeof(ProceduralLandGenerator))]
    public class ColliderCreatorEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            ProceduralLandGenerator pg = (ProceduralLandGenerator)target;
            if (GUILayout.Button("Re-Generate"))
            {
                pg.Generate();
            }
            if (GUILayout.Button("New Seed"))
            {
                //just a new random seed
                pg.seed = (int)System.DateTime.Now.Ticks;
            }
            DrawDefaultInspector();
        }
    }


	public void Start () {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVerts = mesh.vertices;
        Generate();
        
    }
	public void Generate()
    {
        Random.seed = seed;
        //EditorApplication.isPaused = true;
        Vector3[][] newVertsGrid = ToGrid(originalVerts);
        
        //Make terrain non-flat
        TexturizeTerrain(newVertsGrid);

        //Create several mountains, and raise the land around each
        GenerateMountains(newVertsGrid);
        
        mesh.vertices = ToArray(newVertsGrid);
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
	void TexturizeTerrain(Vector3[][] newVertsGrid)
    {
        for (int i = 0; i < newVertsGrid.Length; i++)
        {
            for (int k = 0; k < newVertsGrid[i].Length; k++)
            {
                newVertsGrid[i][k] += Random.Range(-1 * heightFactor, heightFactor) * Vector3.up / transform.localScale.z;

            }
        }
    }
    Vector3[][] ToGrid(Vector3[] input)
    {
        int sq = Mathf.RoundToInt(Mathf.Sqrt(input.Length));
        Vector3[][] newGrid = new Vector3[sq][];
        
        for (int i = 0; i < sq; i++) {
            newGrid[i] = new Vector3[sq];
            for (int k = 0; k < sq; k++)
            {
                newGrid[i][k] = input[(i * sq) + k];
            }
        }

        return newGrid;
    }
    Vector3[] ToArray(Vector3[][] input)
    {
      
        Vector3[] newArray = new Vector3[input.Length * input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            for (int k = 0; k < input[i].Length; k++)
            {
                newArray[(i * input.Length) + k] = input[i][k];
            }
        }

        return newArray;

    }
    void GenerateMountains(Vector3[][] newVertsGrid)
    {
        for (int i = 0; i < mountains; i++)
        {
            //find a random point inside everything
            int val = Random.Range(0, newVertsGrid[0].Length * newVertsGrid.Length);

            //find the related X and Y coordinates
            int y = val % newVertsGrid.Length;
            int x = (val - y) / newVertsGrid.Length;

            //print(val + " " + x + " " + y);

            //Randomize the higher location\
            newVertsGrid[x][y] +=  mountainHeight * Random.Range(heightFactor * 15, heightFactor * 20) * Vector3.up / transform.localScale.z;

            //all this mumbo jumbo means that it searches for all parts that are within mountain width, or a mountainWidth * mountanWidth size of a square 
            for (int k = (x - (mountainWidth/2) < 0 ? 0 : x - (mountainWidth / 2));
                         k < (x + (mountainWidth / 2) >= newVertsGrid[x].Length ? newVertsGrid[x].Length - 1 : x + (mountainWidth / 2));
                    k++)
            {
                //print(k);
                for (int f = (y - (mountainWidth / 2) < 0 ? 0 : y - (mountainWidth / 2));
                         f < (y + (mountainWidth / 2) >= newVertsGrid[y].Length ? newVertsGrid[y].Length - 1 : y + (mountainWidth / 2));
                    f++)
                {
                    //print(f);
                    float distance = Vector2.Distance(new Vector2(k, f), new Vector2(x, y));
                    print(distance);
                    if (k != x && f != y)
                    {
                        //print(k + " " + f);
                        newVertsGrid[k][f] += (newVertsGrid[x][y].y / distance) * Vector3.up;
                    }
                }
            }

        }
    }

}
