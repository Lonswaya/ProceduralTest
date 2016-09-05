using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public class ProceduralLandGenerator : MonoBehaviour {
    Mesh mesh;
    Vector3[] originalVerts;


    /** Basically something I am trying out to generate a landscape mesh at runtime
     *  Unoptimized, trying out new things
     *  
     *  #Requirements: NEEDS TO BE A SQUARE MESH
     */
    public int seed;
    [Space(20)]

    [Header("Terrain Texturizer")]
    public bool texturizesTerrain = true;
    [Range(0.0f, 1.0f)]
    public float heightFactor = 1;

    [Space(20)]

    [Header("Mountain Settings")]
    public bool createsMountains = true;
    [Range(0, 50)]
    public int mountains = 4;
    [Range(1, 10)]
    public int mountainWidth = 5;
    [Range(0.0f, 2.0f)]
    public float mountainHeight = 1;
    [Range(1f, 20f)]
    public float maxMountainHeight = 10;
    [Range(0f, 1f)]
    public float mountainVariation = .5f;

    [Space(20)]

    [Header("Ravine Settings")]
    public bool createsRavines = true;
    [Range(0, 5)]
    public int ravines = 1;
    [Range(0, 200)]
    public int ravineLength;
    [Range(0, 20)]
    public int ravineWidth;
    [Range(0f, 1f)]
    public float ravineDiversity = .5f;


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
        if (texturizesTerrain) TexturizeTerrain(newVertsGrid);

        //Create several mountains, and raise the land around each
        if (createsMountains) GenerateMountains(newVertsGrid);

        if (createsRavines) GenerateRavines(newVertsGrid);
        
        mesh.vertices = ToArray(newVertsGrid);
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    private void GenerateRavines(Vector3[][] newVertsGrid)
    {

        /* TODO find a way to fix this
         * Right now it creates comb-like indentations in the ground, which looks kind of awkward
         * 
         */
        Vector3[][] originalVertsGrid = DeepCopy(newVertsGrid);

        newVertsGrid[0][0] = new Vector2(1, 1);
        print(originalVertsGrid[0][0]);

        for (int ravineCount = 0; ravineCount < ravines; ravineCount++)
        {
            Vector2 randomStart = new Vector2(Random.Range(0, newVertsGrid.Length - 1), Random.Range(0, newVertsGrid.Length - 1));
            //the overall direction that we want to go
            int directionBias = Random.Range(0, 3);

            int lastDirection = -1;
            Vector2 lastPosition = randomStart;

            //I want to call random.range 10 times, just to ensure seeds will stay the same if mountain width is the same
            ArrayList randomSet = new ArrayList();
            for (int ind = 0; ind < 10; ind++)
            {
                randomSet.Add(Random.Range(0, 1.0f));
            }

            int randIndex = 0;

            for (int i = 0; i < ravineLength; i++)
            {
                print(lastPosition);
                newVertsGrid[Mathf.RoundToInt(lastPosition.x)][Mathf.RoundToInt(lastPosition.y)] =
                    originalVertsGrid[Mathf.RoundToInt(lastPosition.x)][Mathf.RoundToInt(lastPosition.y)] + -1 * Vector3.up / transform.localScale.z;

                int direction = Random.Range(0, 3);

                //all this mumbo jumbo means that it searches for all parts that are within ravineWidth, or a ravineWidth * ravineWidth size of a square 
                for (int k = (Mathf.RoundToInt(lastPosition.x) - (ravineWidth) < 0 ? 0 : Mathf.RoundToInt(lastPosition.x) - (ravineWidth));
                             k < (Mathf.RoundToInt(lastPosition.x) + (ravineWidth) >= newVertsGrid[Mathf.RoundToInt(lastPosition.x)].Length ? newVertsGrid[Mathf.RoundToInt(lastPosition.x)].Length - 1 : Mathf.RoundToInt(lastPosition.x) + (ravineWidth));
                        k++)
                {
                    //print(k);
                    for (int f = (Mathf.RoundToInt(lastPosition.y) - (ravineWidth) < 0 ? 0 : Mathf.RoundToInt(lastPosition.y) - (ravineWidth));
                             f < (Mathf.RoundToInt(lastPosition.y)  + (ravineWidth) >= newVertsGrid[Mathf.RoundToInt(lastPosition.y)].Length ? newVertsGrid[Mathf.RoundToInt(lastPosition.y)].Length - 1 : Mathf.RoundToInt(lastPosition.y) + (ravineWidth));
                        f++)
                    {
                        //print(f);
                        float distance = Vector2.Distance(new Vector2(k, f), new Vector2(Mathf.RoundToInt(lastPosition.x), Mathf.RoundToInt(lastPosition.y)));
                        //print(distance);
                        if (!(k == Mathf.RoundToInt(lastPosition.x) && f == Mathf.RoundToInt(lastPosition.y)))
                        {
                            //print(k + " " + f);

                            //if distance == mountain width, then we will not change anything. 
                            newVertsGrid[k][f] = originalVertsGrid[k][f] + (newVertsGrid[Mathf.RoundToInt(lastPosition.x)][Mathf.RoundToInt(lastPosition.y)].y * (float)randomSet[randIndex] * (ravineWidth - distance > 0 ? Mathf.Sqrt(ravineWidth - distance) : 0))  *  Vector3.up;
                            randIndex = (randIndex + 1) % 10;
                        }
                    }
                }


                /*Vector2 lastWidth = lastPosition;
                for (int width = 0; width < ravineWidth; width++)
                {
                    //next position to the right
                    lastWidth = directionToLocation((direction + 1) % 4, lastWidth, newVertsGrid.Length);

                    newVertsGrid[Mathf.RoundToInt(lastWidth.x)][Mathf.RoundToInt(lastWidth.y)] =
                    originalVertsGrid[Mathf.RoundToInt(lastWidth.x)][Mathf.RoundToInt(lastWidth.y)] + -1 * (float)randomSet[randIndex] * (ravineWidth - width > 0 ? Mathf.Sqrt(ravineWidth - width) : 0) * Vector3.up / (transform.localScale.z);

                    randIndex = (randIndex + 1) % 10;
                }
                lastWidth = lastPosition;
                for (int width = 0; width < ravineWidth; width++)
                {
                    //next position to the right
                    lastWidth = directionToLocation((direction - 1) % 4, lastWidth, newVertsGrid.Length);

                    newVertsGrid[Mathf.RoundToInt(lastWidth.x)][Mathf.RoundToInt(lastWidth.y)] =
                    originalVertsGrid[Mathf.RoundToInt(lastWidth.x)][Mathf.RoundToInt(lastWidth.y)] + -1 * (float)randomSet[randIndex] * (ravineWidth - width > 0 ? Mathf.Sqrt(ravineWidth - width) : 0) * Vector3.up / (transform.localScale.z);

                    randIndex = (randIndex + 1) % 10;
                }*/

                if (Random.value > ravineDiversity) direction = directionBias;
                // if (direction == lastDirection) direction = (direction + 1) % 4;
                lastDirection = direction;
                Vector2 nextPosition = directionToLocation(direction, lastPosition, newVertsGrid.Length);
                lastPosition = nextPosition;

            }
        }
        //originalVertsGrid = newVertsGrid;
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
    Vector3[][] DeepCopy(Vector3[][] original)
    {
        Vector3[][] deepCopy = new Vector3[original.Length][];
        for (int x = 0; x < original.Length; x++)
        {
            Vector3[] ySet = new Vector3[original[x].Length];
            for (int y = 0; y < ySet.Length; y++)
            {
                ySet[y] = original[x][y];
            }
            deepCopy[x] = ySet;
        }
        return deepCopy;

    }
    Vector2 directionToLocation(int direction, Vector2 lastPosition, int limit)
    {

        Vector2 nextPosition = Vector2.zero;
        //print(direction);
        switch (direction)
        {
            case 0:
                if (lastPosition.x + 1 >= limit)
                    nextPosition = lastPosition;
                else
                    nextPosition = new Vector2(lastPosition.x + 1, lastPosition.y);
                break;
            case 1:
                if (lastPosition.x - 1 < 0)
                    nextPosition = lastPosition;
                else
                    nextPosition = new Vector2(lastPosition.x - 1, lastPosition.y);
                break;
            case 2:
                if (lastPosition.y + 1 >= limit)
                    nextPosition = lastPosition;
                else
                    nextPosition = new Vector2(lastPosition.x, lastPosition.y + 1);
                break;
            case 3:
                if (lastPosition.y - 1 < 0)
                    nextPosition = lastPosition;
                else
                    nextPosition = new Vector2(lastPosition.x + 1, lastPosition.y - 1);
                break;
            default:
                break;
        }
        return nextPosition;

    }

    Vector3[][] ToGrid(Vector3[] input)
    {
        //SortArray(input);

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

    private void SortArray(Vector3[] arr)
    {
        arr = arr.OrderBy(v => v.x).ToArray<Vector3>();
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
        Vector3[][] originalGrid = (Vector3[][])newVertsGrid.Clone();
        for (int i = 0; i < mountains; i++)
        {
            //find a random point inside everything
            int val = Random.Range(0, newVertsGrid[0].Length * newVertsGrid.Length);

            //find the related X and Y coordinates
            int y = val % newVertsGrid.Length;
            int x = (val - y) / newVertsGrid.Length;

            //print("|||" +val + " " + x + " " + y);

            //Randomize the peak location
            float randRange = Random.Range(heightFactor * 15, heightFactor * 20);
            //print(randRange);
            newVertsGrid[x][y] = originalGrid[x][y] + (mountainHeight * randRange * Vector3.up / transform.localScale.z) ;

            print(newVertsGrid[x][y].y);

            if (newVertsGrid[x][y].y > maxMountainHeight)
                newVertsGrid[x][y] = new Vector3(newVertsGrid[x][y].x, maxMountainHeight, newVertsGrid[x][y].z);


            //I want to call random.range 10 times, just to ensure seeds will stay the same if mountain width is the same
            ArrayList randomSet = new ArrayList();
            for (int ind = 0; ind < mountainWidth + Random.Range(0, 5); ind++)
            {
                randomSet.Add(Random.Range(1 - mountainVariation, 1.0f));
            }

            int randIndex = 0;
            //all this mumbo jumbo means that it searches for all parts that are within mountain width, or a mountainWidth * mountanWidth size of a square 
            for (int k = (x - (mountainWidth) < 0 ? 0 : x - (mountainWidth));
                         k < (x + (mountainWidth) >= newVertsGrid[x].Length ? newVertsGrid[x].Length - 1 : x + (mountainWidth));
                    k++)
            {
                //print(k);
                for (int f = (y - (mountainWidth ) < 0 ? 0 : y - (mountainWidth ));
                         f < (y + (mountainWidth ) >= newVertsGrid[y].Length ? newVertsGrid[y].Length - 1 : y + (mountainWidth ));
                    f++)
                {
                    //print(f);
                    float distance = Vector2.Distance(new Vector2(k, f), new Vector2(x, y));
                    //print(distance);
                    if (!(k == x && f == y))
                    {
                        //print(k + " " + f);

                        //if distance == mountain width, then we will not change anything. 
                        newVertsGrid[k][f] = originalGrid[k][f] + (newVertsGrid[x][y].y * (float)randomSet[randIndex] * (mountainWidth - distance > 0?Mathf.Sqrt(mountainWidth - distance):0)) * Vector3.up;
                        randIndex = (randIndex + 1)% randomSet.Count;
                    }
                }
            }

        }
        //newVertsGrid = originalGrid;
    }

}
