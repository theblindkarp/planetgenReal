using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{

    [Space(5)]
    [Header("General Mesh Info")]

    public int xSize = 50;
    public int ySize = 50;
    public int zSize = 50;
    public int chunkSize = 50;
    [HideInInspector] public Chunk[] chunks;
    public int selectedChunk;
    [SerializeField] GameObject chunk; //Makes the Chunk Object

    [Space(5)]
    [Header("More Complicated Generation")]

    [SerializeField] Gradient caveMap;
    [SerializeField] Gradient planetColors;
    [HideInInspector] public Color[] planetGradientColors;
    [SerializeField] Vector3 colorsOffset;
    [SerializeField, Range(0.1f, 3)]
    float gradientMultiplier = .5f;

    #region --- helpers ---
    public class GridPoint
    {
        private Vector3 _position = Vector3.zero;
        private bool _on = false;

        public Vector3 Position
        {
            get
            {
                return _position;
            }
            set
            {
                _position = new Vector3(value.x, value.y, value.z);
            }
        }
        public bool On
        {
            get
            {
                return _on;
            }
            set
            {
                _on = value;
            }
        }
        public override string ToString()
        {
            return string.Format("{0} {1}", Position, On);
        }
    }
    public static class Points
    {
        /*      E ------ F
         *      |        |
         *      | A ------- B
         *      | |      |  |
         *      G | ---- H  |
         *        |         |
         *        C ------- D
        */

        // CORNERS
        public static GridPoint A = null;
        public static GridPoint B = null;
        public static GridPoint C = null;
        public static GridPoint D = null;
        public static GridPoint E = null;
        public static GridPoint F = null;
        public static GridPoint G = null;
        public static GridPoint H = null;

        // HALF-WAY POINTS
        public static Vector3 ab
        {
            get { return C.Position + new Vector3(0.5f, 1f, 0f); }
        }
        public static Vector3 ba
        {
            get { return C.Position + new Vector3(0.5f, 1f, 0f); }
        }
        public static Vector3 bd
        {
            get { return C.Position + new Vector3(1f, 0.5f, 0f); }
        }
        public static Vector3 db
        {
            get { return C.Position + new Vector3(1f, 0.5f, 0f); }
        }
        public static Vector3 dc
        {
            get { return C.Position + new Vector3(0.5f, 0f, 0f); }
        }
        public static Vector3 cd
        {
            get { return C.Position + new Vector3(0.5f, 0f, 0f); }
        }
        public static Vector3 ca
        {
            get { return C.Position + new Vector3(0f, 0.5f, 0f); }
        }
        public static Vector3 ac
        {
            get { return C.Position + new Vector3(0f, 0.5f, 0f); }
        }

        public static Vector3 ef
        {
            get { return C.Position + new Vector3(0.5f, 1f, 1f); }
        }
        public static Vector3 fe
        {
            get { return C.Position + new Vector3(0.5f, 1f, 1f); }
        }
        public static Vector3 fh
        {
            get { return C.Position + new Vector3(1f, 0.5f, 1f); }
        }
        public static Vector3 hf
        {
            get { return C.Position + new Vector3(1f, 0.5f, 1f); }
        }
        public static Vector3 hg
        {
            get { return C.Position + new Vector3(0.5f, 0f, 1f); }
        }
        public static Vector3 gh
        {
            get { return C.Position + new Vector3(0.5f, 0f, 1f); }
        }
        public static Vector3 ge
        {
            get { return C.Position + new Vector3(0f, 0.5f, 1f); }
        }
        public static Vector3 eg
        {
            get { return C.Position + new Vector3(0f, 0.5f, 1f); }
        }

        public static Vector3 fb
        {
            get { return C.Position + new Vector3(1f, 1f, 0.5f); }
        }
        public static Vector3 bf
        {
            get { return C.Position + new Vector3(1f, 1f, 0.5f); }
        }
        public static Vector3 ae
        {
            get { return C.Position + new Vector3(0f, 1f, 0.5f); }
        }
        public static Vector3 ea
        {
            get { return C.Position + new Vector3(0f, 1f, 0.5f); }
        }
        public static Vector3 hd
        {
            get { return C.Position + new Vector3(1f, 0f, 0.5f); }
        }
        public static Vector3 dh
        {
            get { return C.Position + new Vector3(1f, 0f, 0.5f); }
        }
        public static Vector3 cg
        {
            get { return C.Position + new Vector3(0f, 0f, 0.5f); }
        }
        public static Vector3 gc
        {
            get { return C.Position + new Vector3(0f, 0f, 0.5f); }
        }
    }
    public static class Bits
    {
        // TO CHECK IF BIT IS ON OR OFF
        public static int A = (int)Mathf.Pow(2, 0);
        public static int B = (int)Mathf.Pow(2, 1);
        public static int C = (int)Mathf.Pow(2, 2);
        public static int D = (int)Mathf.Pow(2, 3);
        public static int E = (int)Mathf.Pow(2, 4);
        public static int F = (int)Mathf.Pow(2, 5);
        public static int G = (int)Mathf.Pow(2, 6);
        public static int H = (int)Mathf.Pow(2, 7);

        public static string BinaryForm(int config)
        {
            string A = ((config & (1 << 0)) != 0) ? "A" : "-";
            string B = ((config & (1 << 1)) != 0) ? "B" : "-";
            string C = ((config & (1 << 2)) != 0) ? "C" : "-";
            string D = ((config & (1 << 3)) != 0) ? "D" : "-";
            string E = ((config & (1 << 4)) != 0) ? "E" : "-";
            string F = ((config & (1 << 5)) != 0) ? "F" : "-";
            string G = ((config & (1 << 6)) != 0) ? "G" : "-";
            string H = ((config & (1 << 7)) != 0) ? "H" : "-";

            return H + G + F + E + D + C + B + A;
        }
        public static bool isBitSet(int config, string letter)
        {
            bool ret = false;

            switch (letter)
            {
                case "A":
                    ret = ((config & (1 << 0)) != 0);
                    break;
                case "B":
                    ret = ((config & (1 << 1)) != 0);
                    break;
                case "C":
                    ret = ((config & (1 << 2)) != 0);
                    break;
                case "D":
                    ret = ((config & (1 << 3)) != 0);
                    break;
                case "E":
                    ret = ((config & (1 << 4)) != 0);
                    break;
                case "F":
                    ret = ((config & (1 << 5)) != 0);
                    break;
                case "G":
                    ret = ((config & (1 << 6)) != 0);
                    break;
                case "H":
                    ret = ((config & (1 << 7)) != 0);
                    break;
            }

            return ret;
        }
    }
    public static class UVCoord
    {
        /*  A ------ B
            |        |
            |        |
            C ------ D  */
        public static Vector2 A = new Vector2(0, 1);
        public static Vector2 B = new Vector2(1, 1);
        public static Vector2 C = new Vector2(0, 0);
        public static Vector2 D = new Vector2(1, 0);
    }
    #endregion

    //grid of points
    static GridPoint[,,] grd = null;

    public float radius = 25;

    [SerializeField]
    float noiseScale = .05f;

    [SerializeField, Range(0, 1)]
    float threshold = .5f;

    [SerializeField]
    int seed = 0;

    [HideInInspector] public Mesh mesh;
    MeshCollider meshCollider;
    //int[] triangles;
    public static List<Vector3> vertices = new List<Vector3>();
    public static List<int> triangles = new List<int>();
    static List<Vector2> uv = new List<Vector2>();
    int[] verts = new int[8]; //The List of bools represented in ints
    Vector3[] allVertices = new Vector3[]
    {
        new Vector3(0, 0, 0.5f), new Vector3(0.5f, 0, 1), new Vector3(1, 0, 0.5f), new Vector3(0.5f, 0, 0),
        new Vector3(0, 1, 0.5f), new Vector3(0.5f, 1, 1), new Vector3(1, 1, 0.5f), new Vector3(0.5f, 1, 0),
        new Vector3(0, 0.5f, 0), new Vector3(0, 0.5f, 1), new Vector3(1, 0.5f, 1), new Vector3(1, 0.5f, 0),
    }; // The List of the edges

    Vector3[] corner = new Vector3[]
    {
        new Vector3(0, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(1, 0, 0),
        new Vector3(0, 1, 0), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(1, 1, 0),
    }; //The List of all the Edges/Corners

    public Material material;

    [SerializeField]
    bool sphere = false;

    // Start is called before the first frame update
    void Start()
    {
        //mesh = new Mesh();
        //GetComponent<MeshFilter>().mesh = mesh;
        //GetComponent<MeshRenderer>().material = material;
        //meshCollider = GetComponent<MeshCollider>();
        //mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //increase max vertices per mesh
        //mesh.name = "ProceduralMesh";

        //Clear();
        //MakeGrid();
        //Generate();
        //CaveMap();
        //UpdateMesh();

        GenerateTerrain();
    }

    // Update is called once per frame
    void Update()
    {

    }


    
    //void OnValidate()
    //{
        //mesh = new Mesh();
        //GetComponent<MeshFilter>().mesh = mesh;
        //GetComponent<MeshRenderer>().material = material;
        //meshCollider = GetComponent<MeshCollider>();
        //mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //increase max vertices per mesh
        //mesh.name = "ProceduralMesh";

        //Clear();
        //MakeGrid();
        //Generate();
        //CaveMap();
        //UpdateMesh();
    //}

    //Makes the Chunks
    public void MakeChunks()
    {
        //Deletes Previous Chunks
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        //Finds the number of chunks
        int xChunkAmount = Mathf.CeilToInt(xSize / chunkSize);
        int yChunkAmount = Mathf.CeilToInt(ySize / chunkSize);
        int zChunkAmount = Mathf.CeilToInt(zSize / chunkSize);

        if ((xSize / chunkSize) != xChunkAmount)
            xChunkAmount++;
        if ((ySize / chunkSize) != yChunkAmount)
            yChunkAmount++;
        if ((zSize / chunkSize) != zChunkAmount)
            zChunkAmount++;

        //Sets the ID list to the size of the combined chunks
        chunks = new Chunk[xChunkAmount * yChunkAmount * zChunkAmount];

        for (int i = 0, x = 0; x < xChunkAmount; x++)
        {
            for (int y = 0; y < yChunkAmount; y++)
            {
                for (int z = 0; z < zChunkAmount; z++)
                {
                    //Instantiates it
                    //GameObject newChunk = Instantiate(chunk, new Vector3((chunkSize / 2) + ((chunkSize / 2) * x), (chunkSize / 2) + ((chunkSize / 2) * y), (chunkSize / 2) + ((chunkSize / 2) * z)), Quaternion.identity, this.transform);
                    GameObject newChunk = Instantiate(chunk, new Vector3(0, 0, 0), Quaternion.identity, this.transform);
                    newChunk.name = "Chunk: #" + i; //Makes a name based on the ID
                    Chunk newChunkScript = newChunk.GetComponent<Chunk>();
                    newChunkScript.chunkNumber = i;

                    newChunkScript.ownedXUnits[0] = chunkSize * x; //Sets the X
                    newChunkScript.ownedYUnits[0] = chunkSize * y; //Sets the Y
                    newChunkScript.ownedZUnits[0] = chunkSize * z; //Sets the Z

                    newChunkScript.ownedXUnits[1] = (chunkSize * x) + (chunkSize - 1); //Sets the X
                    newChunkScript.ownedYUnits[1] = (chunkSize * y) + (chunkSize - 1); //Sets the Y
                    newChunkScript.ownedZUnits[1] = (chunkSize * z) + (chunkSize - 1); //Sets the Z

                    //If it is the last chunk
                    if ((x - 1 == xChunkAmount) || (y - 1 == yChunkAmount) || (z - 1 == zChunkAmount))
                    {
                        newChunkScript.ownedXUnits[1] = chunkSize - 1; //Sets the X
                        newChunkScript.ownedYUnits[1] = chunkSize - 1; //Sets the Y
                        newChunkScript.ownedZUnits[1] = chunkSize - 1; //Sets the Z
                    }

                    //Make a new List
                    vertices = new List<Vector3>();
                    triangles = new List<int>();

                    newChunkScript.CreateChunkMesh(); //Loads it in the Chunk Script




                    //Generates
                    //Generate(0, xSize, 0, ySize, 0, zSize);

                    //CaveMap(0, xSize, 0, ySize, 0, zSize);



                    //Makes the Mesh
                    //newChunkScript.mesh.Clear();

                    //newChunkScript.mesh.vertices = vertices.ToArray();
                    //newChunkScript.mesh.triangles = triangles.ToArray();
                    //newChunkScript.mesh.colors = planetGradientColors;

                    //newChunkScript.mesh.RecalculateNormals();
                    //newChunkScript.meshCollider.sharedMesh = mesh;

                    chunks[i] = newChunkScript;

                    i++;
                }
            }
        }
    }

    public void GenerateTerrain()
    {
        float startTime = Time.realtimeSinceStartup;

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = material;
        meshCollider = GetComponent<MeshCollider>();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //increase max vertices per mesh
        mesh.name = "ProceduralMesh";

        Clear();
        MakeGrid();
        Generate(0, xSize - 1, 0, ySize - 1, 0, zSize - 1);

        CaveMap(0, xSize - 1, 0, ySize - 1, 0, zSize - 1);
        UpdateMesh();

        MakeChunks();

        //Debug.Log("Planet Loaded in " + (Time.realtimeSinceStartup - startTime) + " Seconds.");
    }

    public void GenerateColor()
    {
        float startTime = Time.realtimeSinceStartup;

        //CaveMap(0, xSize, 0, ySize, 0, zSize);

        for (int i = 0; i < chunks.Length; i++)
        {
            //chunks[i].GenerateColor(chunks[i].ownedXUnits[0], chunks[i].ownedXUnits[1], chunks[i].ownedYUnits[0], chunks[i].ownedYUnits[1], chunks[i].ownedZUnits[0], chunks[i].ownedZUnits[1]);
            chunks[i].CreateChunkMesh();
        }

        //mesh.colors = planetGradientColors;
        //mesh.RecalculateNormals();

        Debug.Log("Color Loaded in " + (Time.realtimeSinceStartup - startTime) + " Seconds.");
    }

    //Makes the grid for the cubes
    void MakeGrid()
    {
        //allocate
        grd = new GridPoint[xSize + 1, ySize + 1, zSize + 1];

        // define the points
        for (int x = 0; x < xSize + 1; x++)
        {
            for (int y = 0; y < ySize + 1; y++)
            {
                for (int z = 0; z < zSize + 1; z++)
                {
                    grd[x, y, z] = new GridPoint();
                    grd[x, y, z].Position = new Vector3(x, y, z);
                    grd[x, y, z].On = false;

                    //Generates the Perlin Noise
                    Generate3DPerlinNoise(x, y, z);
                }
            }
        }
    }

    public void Generate(int startingChunkX, int endingChunkX, int startingChunkY, int endingChunkY, int startingChunkZ, int endingChunkZ)
    {
        float startTime = Time.realtimeSinceStartup;
        //int count = 0; //For Counting the amount of iterations for the amount of triangle made
        //List<Vector3> tempVertList = new List<Vector3>(); //The list version
        //List<int> tempTriList = new List<int>(); //The list version
        //int rememberLength = 0; //To remeber the length and adjust from that

        //go through each block position
        //-1 So that the Marching Cubes dont March there way off the map, hahahah ;> ok im done :|
        for (int x = startingChunkX; x < endingChunkX + 1; x++)
        {
            for (int y = startingChunkY; y < endingChunkY + 1; y++)
            {
                for (int z = startingChunkZ; z < endingChunkZ + 1; z++)
                {
                    //This is for both sides of the mesh
                    //for (int loopSides = 0; loopSides <= 1; loopSides++)
                    //{
                        //ignore this block if it's a sphere and it's outside of the radius (ex: in the corner of the chunk, outside of the sphere)
                        //distance between the current point with the center point. if it's larger than the radius, then it's not inside the sphere.
                        //if (sphere && Vector3.Distance(new Vector3(x + 5, y + 5, z + 5), Vector3.one * radius) > radius)
                        //    continue;

                        //Build it
                        //Vertice Holder Variables
                        //Getting value of the noise at given x, y, and z and setting to a boolean

                        //Reset them every time
                        //for (int i = 0; i < verts.Length; i++)
                        //{
                        //    verts[i] = 1 - loopSides; //I originally thought that 0 was nothing but now 1 is nothing, and 1 was something and now 0 is something. It is all very confusing. ＼（´Ｏ｀）／
                        //}

                        //current cube corners
                        Points.A = grd[x, y + 1, z];
                        Points.B = grd[x + 1, y + 1, z];
                        Points.C = grd[x, y, z];
                        Points.D = grd[x + 1, y, z];
                        Points.E = grd[x, y + 1, z + 1];
                        Points.F = grd[x + 1, y + 1, z + 1];
                        Points.G = grd[x, y, z + 1];
                        Points.H = grd[x + 1, y, z + 1];

                        int combination = 0;
                        //for (int vertIndex = 0; vertIndex < 8; vertIndex++)
                        //{
                        //    int powerIndex = (int)Mathf.Pow(2, vertIndex);
                        //    combination += verts[vertIndex] * powerIndex;
                        //}
                        
                        combination = GetCubeConfig();
                        IsoFaces(combination);

                        //Debug.Log(combination);

                        //Calculate It
                        //int[] triangleVerts = GetVertex(combination);
                        //List<int> trianglePlaceHolder = new List<int>();
                        //Removes the -1 Placeholder
                        //for (int i = 0; i < triangleVerts.Length; i++)
                        //{
                        //    if (triangleVerts[i] > -1)
                        //    {
                        //        trianglePlaceHolder.Add(triangleVerts[i]);
                        //    }
                        //}

                        //Sets it back to the original array
                        //triangleVerts = new int[trianglePlaceHolder.Count]; // Resets the size
                        //for (int i = 0; i < trianglePlaceHolder.Count; i++)
                        //{
                        //    triangleVerts[i] = trianglePlaceHolder[i];
                        //}

                        //foreach (int edgeIndex in triangleVerts)
                        //{
                            //Lookup the indices of the corner points making up the current edge
                        //    int indexA = GetCornerIndexFromEdge(edgeIndex, 0);
                        //    int indexB = GetCornerIndexFromEdge(edgeIndex, 1);

                            //Find midpoint of edge
                        //    Vector3 vertexPos = (corner[indexA] + corner[indexB]) / 2;

                        //    tempVertList.Add(vertexPos + new Vector3(x, y, z));
                        //}



                        //Vector3[] tempVertices = new Vector3[triangleVerts.Length]; //Sets up the array size
                        //int[] tempTriangles = new int[triangleVerts.Length]; //Sets this one up as well, becuase when I forgot to it gave me a LOT of ERRORS!!! :<ß
                        //for (int i = 0; i < triangleVerts.Length; i++)
                        //{
                            //Converts the vert numbers into actual locations
                            //tempVertices[i] = allVertices[triangleVerts[i]] + new Vector3(x, y, z);
                            //Adds it to the whole list
                            //tempVertList.Add(tempVertices[i]);

                            //Setting up the Triangle Order
                        //    tempTriangles[i] = i + rememberLength;
                            //Adds it to the whole list
                        //    tempTriList.Add(tempTriangles[i]);

                        //    count++;
                        //}

                        //rememberLength += triangleVerts.Length;
                    //}

                }
            }
        }

        //Debug.Log("Mesh Loaded in " + (Time.realtimeSinceStartup - startTime) + " Seconds.");
    }

    int GetCubeConfig()
    {
        // config code based on current cube on points
        int config = 0;

        config += Points.A.On ? Bits.A : 0;
        config += Points.B.On ? Bits.B : 0;
        config += Points.C.On ? Bits.C : 0;
        config += Points.D.On ? Bits.D : 0;
        config += Points.E.On ? Bits.E : 0;
        config += Points.F.On ? Bits.F : 0;
        config += Points.G.On ? Bits.G : 0;
        config += Points.H.On ? Bits.H : 0;

        return config;
    }
    
    //Updates the Mesh
    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = planetGradientColors;

        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    //Clears the Info
    public void Clear()
    {
        vertices.Clear();
        triangles.Clear();
        uv.Clear();
    }

    //If any changes in the Inspector are made
    //void OnValidate()
    //{
    //    Generate();
    //}

    //Generates the 3D Perlin Noise YAY! :>
    float Generate3DPerlinNoise(int x, int y, int z)
    {
        //ignore this block if it's a sphere and it's outside of the radius (ex: in the corner of the chunk, outside of the sphere)
        //distance between the current point with the center point. if it's larger than the radius, then it's not inside the sphere.
        if (sphere && Vector3.Distance(new Vector3(x - 5, y - 5, z - 5), Vector3.one * (radius - 5)) < (radius - 5))
        {
            //To account for noise scaling
            float xN = x * noiseScale;
            float yN = y * noiseScale;
            float zN = z * noiseScale;

            float xy = Mathf.PerlinNoise(xN + seed, yN + seed);
            float xz = Mathf.PerlinNoise(xN + seed, zN + seed);
            float yz = Mathf.PerlinNoise(yN + seed, zN + seed);

            float ThreeDemPerlin = (xy + xz + yz) / 3;
            //float ThreeDemPerlin;

            //if ((xN >= 0 && xN <= 5) && (yN >= 0 && yN <= 5) && (zN >= 0 && zN <= 5))
            //{
            //    ThreeDemPerlin = 1;
            //}
            //else
            //{
            //    ThreeDemPerlin = 0;
            //}

            //Turn it on if it i higher than the noise limit
            //get value of the noise at given x, y, and z
            //and is noise value above the threshold for placing a block?
            if(grd != null)
                grd[x, y, z].On = (ThreeDemPerlin >= threshold);

            return ThreeDemPerlin;
        }

        return 0;
    }

    public void CaveMap(int startingChunkX, int endingChunkX, int startingChunkY, int endingChunkY, int startingChunkZ, int endingChunkZ)
    {
        planetGradientColors = new Color[vertices.Count];

        //float multipliedRadius = radius * gradientMultiplier;
        float highestValue = Mathf.Sqrt(Mathf.Pow(xSize, 2) + Mathf.Pow(ySize, 2) + Mathf.Pow(zSize, 2));

        for (int i = 0, z = startingChunkZ; z <= endingChunkZ; z++)
        {
            for (int x = startingChunkX; x <= endingChunkX; x++)
            {
                for (int y = startingChunkY; y <= endingChunkY; y++)
                {
                    //if (sphere && Vector3.Distance(new Vector3(x, y, z), Vector3.one * radius) < radius)
                    //    continue;

                    float c = (Vector3.Distance(new Vector3(x, y, z), (Vector3.one * radius) + colorsOffset) * gradientMultiplier);
                    //float c = Mathf.Sqrt((x * x) + (y * y) + (z * z));
                    float height = Mathf.InverseLerp(0, highestValue, c);
                    //Debug.Log("c: " + c + " x: " + x + " y: " + y + " z: " + z);
                    if(i < planetGradientColors.Length) //To provent Overrides and Errors
                    {
                        planetGradientColors[i] = planetColors.Evaluate(height);
                    }
                    i++;
                }
            }
        }
        //return planetGradientColors;
    }

    //For a VISUAL REPRESENTATION WOOOOOOW
    void OnDrawGizmos()
    {
        float size = 0.25f;
        Vector3 sizeVector = new Vector3(size, size, size);

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {

                    float noiseValue = Generate3DPerlinNoise(x, y, z);//get value of the noise at given x, y, and z.
                    if (noiseValue >= threshold)
                    {//is noise value above the threshold for placing a block?

                        //ignore this block if it's a sphere and it's outside of the radius (ex: in the corner of the chunk, outside of the sphere)
                        //distance between the current point with the center point. if it's larger than the radius, then it's not inside the sphere.
                        if (sphere && Vector3.Distance(new Vector3(x, y, z), Vector3.one * radius) > radius)
                            continue;

                        Vector3 center = new Vector3(x, y, z);
                        Gizmos.DrawCube(center, sizeVector);
                    }
                }
            }
        }
    }

    //PREPARE YOUR SELF
    //Marching Cubes Vertex Table, 3 Lines Beyound This Point
    //Proceed with CATION

    int[] GetVertex(int vertexCombo)
    {
        int[] triangleConfig = new int[15];

        //The Code for the Triangle Configuration
        int[,] hi = new int[,]{
    {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
    { 8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1 },
    { 3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1 },
    { 4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
    { 4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1 },
    { 9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1 },
    { 10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1 },
    { 5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1 },
    { 5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1 },
    { 8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1 },
    { 2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
    { 2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1 },
    { 11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1 },
    { 5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1 },
    { 11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1 },
    { 11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1 },
    { 2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1 },
    { 6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
    { 3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1 },
    { 6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1 },
    { 6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1 },
    { 8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1 },
    { 7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1 },
    { 3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1 },
    { 0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1 },
    { 9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1 },
    { 8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1 },
    { 5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1 },
    { 0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1 },
    { 6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1 },
    { 10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
    { 1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1 },
    { 0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1 },
    { 3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1 },
    { 6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1 },
    { 9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1 },
    { 8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1 },
    { 3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1 },
    { 10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1 },
    { 10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
    { 2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1 },
    { 7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1 },
    { 2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1 },
    { 1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1 },
    { 11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1 },
    { 8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1 },
    { 0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1 },
    { 7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1 },
    { 7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1 },
    { 10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1 },
    { 0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1 },
    { 7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1 },
    { 6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1 },
    { 4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1 },
    { 10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1 },
    { 8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1 },
    { 1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1 },
    { 10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1 },
    { 10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1 },
    { 9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1 },
    { 7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1 },
    { 3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1 },
    { 7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1 },
    { 3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1 },
    { 6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1 },
    { 9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1 },
    { 1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1 },
    { 4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1 },
    { 7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1 },
    { 6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1 },
    { 0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1 },
    { 6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1 },
    { 0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1 },
    { 11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1 },
    { 6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1 },
    { 5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1 },
    { 9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1 },
    { 1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1 },
    { 10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1 },
    { 0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1 },
    { 11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1 },
    { 9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1 },
    { 7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1 },
    { 2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1 },
    { 9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1 },
    { 9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1 },
    { 1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1 },
    { 0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1 },
    { 10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1 },
    { 2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1 },
    { 0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1 },
    { 0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1 },
    { 9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1 },
    { 5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1 },
    { 5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1 },
    { 8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1 },
    { 9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1 },
    { 1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1 },
    { 3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1 },
    { 4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1 },
    { 9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1 },
    { 11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1 },
    { 11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1 },
    { 2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1 },
    { 9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1 },
    { 3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1 },
    { 1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1 },
    { 4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1 },
    { 0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1 },
    { 9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1 },
    { 1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { 0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 },
    { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 }
    };

        for (int i = 0; i < 15; i++)
        {
            triangleConfig[i] = hi[vertexCombo, i];
        }

        return triangleConfig;
    }

    int GetCornerIndexFromEdge(int edgeIndex, int whichIndex)
    {
        int[] cornerIndexAFromEdge = new int[]
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            0,
            1,
            2,
            3
        };

        int[] cornerIndexBFromEdge = new int[]
        {
            1,
            2,
            3,
            0,
            5,
            6,
            7,
            4,
            4,
            5,
            6,
            7
        };

        //If it is 0 then A otherwise B
        //(So if it is not 0 it will work but I really wanted it to just be 1, but idc :>)
        int chosenIndex = whichIndex == 0 ? cornerIndexAFromEdge[edgeIndex] : cornerIndexBFromEdge[edgeIndex];

        return chosenIndex;
    }

    public static int IsoFaces(int config)
    {
        // IMPORTANT GOLDEN INFORMATION!!! - the mesh triangles to make for each cube configuration

        // SANITY CHECK RULES:
        //  1. on cube corners are considered the inside of a mesh 2^8 = 256 
        //  2. connect triangle points in clockwise direction, on corners inside the mesh
        //  3. on off corners should always be separated by triangle corner
        //  4. same side corners should not have separation by triangle corner

        //vertices
        int beforeCount = vertices.Count;
        switch (config)
        {
            case 0:     // --------
                break;
            case 1:     // -------A
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                break;
            case 2:     // ------B-
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                break;
            case 3:     // ------BA
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                break;
            case 4:     // -----C--
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                break;
            case 5:     // -----C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                break;
            case 6:     // -----CB-
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 7:     // -----CBA                
                vertices.Add(Points.bf);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.bd);
                vertices.Add(Points.fb);
                vertices.Add(Points.cd);
                vertices.Add(Points.fb);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                break;
            case 8:     // ----D---
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.db);
                break;
            case 9:     // ----D--A
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.ab);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 10:    // ----D-B-
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                break;
            case 11:    // ----D-BA
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.dc);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.cd);
                break;
            case 12:    // ----DC--
                vertices.Add(Points.db);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.db);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                break;
            case 13:    // ----DC-A
                vertices.Add(Points.ba);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.ba);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.db);
                vertices.Add(Points.ba);
                break;
            case 14:    // ----DCB-
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.cg);
                vertices.Add(Points.ba);
                vertices.Add(Points.ac);
                vertices.Add(Points.cg);
                break;
            case 15:    // ----DCBA
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                break;
            case 16:    // ---E----
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                break;
            case 17:    // ---E---A
                vertices.Add(Points.ac);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                break;
            case 18:    // ---E--B-
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                break;
            case 19:    // ---E--BA
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.ac);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.ac);
                vertices.Add(Points.bf);
                break;
            case 20:    // ---E-C--
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                break;
            case 21:    // ---E-C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.cg);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                break;
            case 22:    // ---E-CB-
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                break;
            case 23:    // ---E-CBA
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.fb);
                vertices.Add(Points.gc);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                break;
            case 24:    // ---ED---
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                break;
            case 25:    // ---ED--A
                vertices.Add(Points.ab);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.ab);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 26:    // ---ED-B-
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                break;
            case 27:    // ---ED-BA
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.ac);
                vertices.Add(Points.fe);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.dh);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 28:    // ---EDC--
                vertices.Add(Points.bd);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ac);
                vertices.Add(Points.cg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                break;
            case 29:    // ---EDC-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.cg);
                vertices.Add(Points.fe);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.dh);
                break;
            case 30:    // ---EDCB-
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.cg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ba);
                vertices.Add(Points.ac);
                vertices.Add(Points.gc);
                break;
            case 31:    // ---EDCBA
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.cg);
                vertices.Add(Points.fe);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                break;
            case 32:    // --F-----
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                break;
            case 33:    // --F----A
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                break;
            case 34:    // --F---B-
                vertices.Add(Points.ba);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                break;
            case 35:    // --F---BA
                vertices.Add(Points.fh);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ac);
                vertices.Add(Points.fe);
                vertices.Add(Points.ea);
                vertices.Add(Points.ac);
                break;
            case 36:    // --F--C--
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                break;
            case 37:    // --F--C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                break;
            case 38:    // --F--CB-
                vertices.Add(Points.fe);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 39:    // --F--CBA
                vertices.Add(Points.fe);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.cd);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.bd);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                break;
            case 40:    // --F-D---
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                break;
            case 41:    // --F-D--A
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 42:    // --F-D-B-
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.dh);
                break;
            case 43:    // --F-D-BA
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.dc);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.ea);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 44:    // --F-DC--
                vertices.Add(Points.db);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.db);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                break;
            case 45:    // --F-DC-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                break;
            case 46:    // --F-DCB-
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                break;
            case 47:    // --F-DCBA
                vertices.Add(Points.fe);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                break;
            case 48:    // --FE----
                vertices.Add(Points.eg);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                break;
            case 49:    // --FE---A
                vertices.Add(Points.ac);
                vertices.Add(Points.fh);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.fh);
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.hf);
                break;
            case 50:    // --FE--B-
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fh);
                vertices.Add(Points.fh);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ea);
                vertices.Add(Points.ab);
                vertices.Add(Points.fh);
                break;
            case 51:    // --FE--BA
                vertices.Add(Points.eg);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                break;
            case 52:    // --FE-C--
                vertices.Add(Points.ea);
                vertices.Add(Points.fh);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                break;
            case 53:    // --FE-C-A
                vertices.Add(Points.cg);
                vertices.Add(Points.ab);
                vertices.Add(Points.eg);
                vertices.Add(Points.eg);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.eg);
                vertices.Add(Points.dc);
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                break;
            case 54:    // --FE-CB-
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fh);
                vertices.Add(Points.fh);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.fh);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                break;
            case 55:    // --FE-CBA
                vertices.Add(Points.hf);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                vertices.Add(Points.fh);
                vertices.Add(Points.gc);
                vertices.Add(Points.cd);
                break;
            case 56:    // --FED---
                vertices.Add(Points.ea);
                vertices.Add(Points.fh);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                break;
            case 57:    // --FED--A
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.eg);
                vertices.Add(Points.eg);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.eg);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                break;
            case 58:    // --FED-B-
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fh);
                vertices.Add(Points.fh);
                vertices.Add(Points.ba);
                vertices.Add(Points.dh);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.fh);
                vertices.Add(Points.hd);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                break;
            case 59:    // --FED-BA
                vertices.Add(Points.fh);
                vertices.Add(Points.eg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.hd);
                vertices.Add(Points.eg);
                vertices.Add(Points.dc);
                break;
            case 60:    // --FEDC--
                vertices.Add(Points.ea);
                vertices.Add(Points.fh);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.db);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                break;
            case 61:    // --FEDC-A
                vertices.Add(Points.fh);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.bd);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.hf);
                break;
            case 62:    // --FEDCB-
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.hd);
                vertices.Add(Points.ge);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.ae);
                vertices.Add(Points.gc);
                vertices.Add(Points.ge);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.gc);
                break;
            case 63:    // --FEDCBA
                vertices.Add(Points.eg);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                vertices.Add(Points.dh);
                break;
            case 64:    // -G------
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                break;
            case 65:    // -G-----A
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                break;
            case 66:    // -G----B-
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                break;
            case 67:    // -G----BA
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                break;
            case 68:    // -G---C--
                vertices.Add(Points.cd);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ge);
                break;
            case 69:    // -G---C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.gh);
                vertices.Add(Points.ae);
                vertices.Add(Points.eg);
                vertices.Add(Points.hg);
                break;
            case 70:    // -G---CB-
                vertices.Add(Points.ca);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 71:    // -G---CBA
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.eg);
                vertices.Add(Points.bf);
                vertices.Add(Points.eg);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                break;
            case 72:    // -G--D---
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                break;
            case 73:    // -G--D--A
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.ab);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 74:    // -G--D-B-
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                break;
            case 75:    // -G--D-BA
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.bf);
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                break;
            case 76:    // -G--DC--
                vertices.Add(Points.ca);
                vertices.Add(Points.dh);
                vertices.Add(Points.db);
                vertices.Add(Points.ca);
                vertices.Add(Points.ge);
                vertices.Add(Points.dh);
                vertices.Add(Points.dh);
                vertices.Add(Points.eg);
                vertices.Add(Points.gh);
                break;
            case 77:    // -G--DC-A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ae);
                vertices.Add(Points.eg);
                vertices.Add(Points.gh);
                break;
            case 78:    // -G--DCB-
                vertices.Add(Points.bf);
                vertices.Add(Points.hg);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.hg);
                vertices.Add(Points.ac);
                vertices.Add(Points.eg);
                vertices.Add(Points.hg);
                vertices.Add(Points.ba);
                vertices.Add(Points.ac);
                vertices.Add(Points.hg);
                break;
            case 79:    // -G--DCBA
                vertices.Add(Points.bf);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.gh);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                vertices.Add(Points.hg);
                break;
            case 80:    // -G-E----
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                break;
            case 81:    // -G-E---A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.cg);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                break;
            case 82:    // -G-E--B-
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                break;
            case 83:    // -G-E--BA
                vertices.Add(Points.bf);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.ac);
                break;
            case 84:    // -G-E-C--
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                break;
            case 85:    // -G-E-C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                break;
            case 86:    // -G-E-CB-
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.gh);
                vertices.Add(Points.gh);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 87:    // -G-E-CBA
                vertices.Add(Points.bf);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.cd);
                break;
            case 88:    // -G-ED---
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                break;
            case 89:    // -G-ED--A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                break;
            case 90:    // -G-ED-B-
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.ef);
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                break;
            case 91:    // -G-ED-BA
                vertices.Add(Points.fb);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ef);
                vertices.Add(Points.gc);
                vertices.Add(Points.bf);
                vertices.Add(Points.fe);
                vertices.Add(Points.ac);
                break;
            case 92:    // -G-EDC--
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.ac);
                vertices.Add(Points.ac);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ac);
                vertices.Add(Points.hg);
                vertices.Add(Points.bd);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                break;
            case 93:    // -G-EDC-A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ab);
                vertices.Add(Points.dh);
                break;
            case 94:    // -G-EDCB-
                vertices.Add(Points.fb);
                vertices.Add(Points.ba);
                vertices.Add(Points.dh);
                vertices.Add(Points.ae);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.ba);
                vertices.Add(Points.ac);
                vertices.Add(Points.ca);
                vertices.Add(Points.ae);
                vertices.Add(Points.hg);
                vertices.Add(Points.dh);
                vertices.Add(Points.ac);
                vertices.Add(Points.hg);
                break;
            case 95:    // -G-EDCBA
                vertices.Add(Points.bf);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.gh);
                break;
            case 96:    // -GF-----
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                break;
            case 97:    // -GF----A
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                break;
            case 98:    // -GF---B-
                vertices.Add(Points.fe);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                break;
            case 99:    // -GF---BA
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.fh);
                vertices.Add(Points.hg);
                vertices.Add(Points.bd);
                vertices.Add(Points.bd);
                vertices.Add(Points.cg);
                vertices.Add(Points.ac);
                vertices.Add(Points.cg);
                vertices.Add(Points.bd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.hg);
                break;
            case 100:   // -GF--C--
                vertices.Add(Points.ca);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                break;
            case 101:   // -GF--C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.gh);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                break;
            case 102:   // -GF--CB-
                vertices.Add(Points.ca);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.fe);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 103:   // -GF--CBA
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.dc);
                vertices.Add(Points.hf);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.hg);
                break;
            case 104:   // -GF-D---
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                break;
            case 105:   // -GF-D--A
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                vertices.Add(Points.ae);
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                break;
            case 106:   // -GF-D-B-
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.hd);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                break;
            case 107:   // -GF-D-BA
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.dc);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ac);
                vertices.Add(Points.dh);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                break;
            case 108:   // -GF-DC--
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ac);
                vertices.Add(Points.dh);
                vertices.Add(Points.ca);
                vertices.Add(Points.eg);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.ca);
                vertices.Add(Points.gh);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                break;
            case 109:   // -GF-DC-A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.gh);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.dh);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.fe);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                break;
            case 110:   // -GF-DCB-
                vertices.Add(Points.ef);
                vertices.Add(Points.ac);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.ef);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.hg);
                break;
            case 111:   // -GF-DCBA
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                vertices.Add(Points.ef);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.hg);
                break;
            case 112:   // -GFE----
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.gh);
                vertices.Add(Points.bf);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                break;
            case 113:   // -GFE---A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.bf);
                vertices.Add(Points.fh);
                break;
            case 114:   // -GFE--B-
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.gh);
                vertices.Add(Points.ab);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                vertices.Add(Points.ab);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                break;
            case 115:   // -GFE--BA
                vertices.Add(Points.ac);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                vertices.Add(Points.ac);
                vertices.Add(Points.gh);
                vertices.Add(Points.cg);
                break;
            case 116:   // -GFE-C--
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.gh);
                vertices.Add(Points.ae);
                vertices.Add(Points.cd);
                vertices.Add(Points.ac);
                vertices.Add(Points.gh);
                vertices.Add(Points.bf);
                vertices.Add(Points.fh);
                break;
            case 117:   // -GFE-C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.gh);
                vertices.Add(Points.bf);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                break;
            case 118:   // -GFE-CB-
                vertices.Add(Points.db);
                vertices.Add(Points.hg);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.ac);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.db);
                break;
            case 119:   // -GFE-CBA
                vertices.Add(Points.bd);
                vertices.Add(Points.gh);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                break;
            case 120:   // -GFED---
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.gh);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.fh);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                break;
            case 121:   // -GFED--A
                vertices.Add(Points.ab);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.gh);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.gh);
                vertices.Add(Points.bf);
                vertices.Add(Points.fh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                break;
            case 122:   // -GFED-B-
                vertices.Add(Points.ea);
                vertices.Add(Points.ab);
                vertices.Add(Points.fh);
                vertices.Add(Points.hg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fh);
                vertices.Add(Points.fh);
                vertices.Add(Points.ab);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ae);
                vertices.Add(Points.gh);
                vertices.Add(Points.ab);
                vertices.Add(Points.dc);
                vertices.Add(Points.hd);
                break;
            case 123:   // -GFED-BA
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.cg);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.hd);
                break;
            case 124:   // -GFEDC--
                vertices.Add(Points.ae);
                vertices.Add(Points.bd);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.bd);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.hf);
                break;
            case 125:   // -GFEDC-A
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.bd);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.hf);
                break;
            case 126:   // -GFEDCB-
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                break;
            case 127:   // -GFEDCBA
                vertices.Add(Points.hf);
                vertices.Add(Points.gh);
                vertices.Add(Points.dh);
                break;
            case 128:   // H-------
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                break;
            case 129:   // H------A
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                break;
            case 130:   // H-----B-
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 131:   // H-----BA
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 132:   // H----C--
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.hg);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                break;
            case 133:   // H----C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.hg);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                break;
            case 134:   // H----CB-
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.bf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 135:   // H----CBA
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.fb);
                vertices.Add(Points.ae);
                vertices.Add(Points.dc);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                vertices.Add(Points.dc);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.cd);
                vertices.Add(Points.cd);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                break;
            case 136:   // H---D---
                vertices.Add(Points.db);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                break;
            case 137:   // H---D--A
                vertices.Add(Points.hf);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 138:   // H---D-B-
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.fb);
                break;
            case 139:   // H---D-BA
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.fb);
                vertices.Add(Points.ea);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 140:   // H---DC--
                vertices.Add(Points.hf);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.hf);
                vertices.Add(Points.db);
                vertices.Add(Points.ca);
                vertices.Add(Points.hg);
                vertices.Add(Points.fh);
                vertices.Add(Points.gc);
                break;
            case 141:   // H---DC-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.hg);
                vertices.Add(Points.fh);
                vertices.Add(Points.bd);
                vertices.Add(Points.hg);
                break;
            case 142:   // H---DCB-
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.hg);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.gc);
                break;
            case 143:   // H---DCBA
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                break;
            case 144:   // H--E----
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                break;
            case 145:   // H--E---A
                vertices.Add(Points.ab);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                break;
            case 146:   // H--E--B-
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 147:   // H--E--BA
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.ac);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.bf);
                vertices.Add(Points.fe);
                vertices.Add(Points.ac);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                break;
            case 148:   // H--E-C--
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                vertices.Add(Points.cd);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                break;
            case 149:   // H--E-C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.cg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.cg);
                vertices.Add(Points.fe);
                vertices.Add(Points.eg);
                vertices.Add(Points.cd);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                break;
            case 150:   // H--E-CB-
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);

                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);

                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.ef);
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                break;
            case 151:   // H--E-CBA
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.cg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.fe);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                break;
            case 152:   // H--ED---
                vertices.Add(Points.hf);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                break;
            case 153:   // H--ED--A
                vertices.Add(Points.ab);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                vertices.Add(Points.ab);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 154:   // H--ED-B-
                vertices.Add(Points.bf);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.fb);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                break;
            case 155:   // H--ED-BA
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.ge);
                vertices.Add(Points.cd);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.fh);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ge);
                break;
            case 156:   // H--EDC--
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.db);
                vertices.Add(Points.db);
                vertices.Add(Points.hg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.bd);
                vertices.Add(Points.cg);
                vertices.Add(Points.eg);
                vertices.Add(Points.hf);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                break;
            case 157:   // H--EDC-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.db);
                vertices.Add(Points.hg);
                vertices.Add(Points.bd);
                vertices.Add(Points.cg);
                vertices.Add(Points.cg);
                vertices.Add(Points.db);
                vertices.Add(Points.hg);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.cg);
                vertices.Add(Points.fh);
                vertices.Add(Points.db);
                vertices.Add(Points.hg);
                vertices.Add(Points.cg);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                break;
            case 158:   // H--EDCB-
                vertices.Add(Points.fb);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.fb);
                vertices.Add(Points.ba);
                vertices.Add(Points.cg);
                vertices.Add(Points.bf);
                vertices.Add(Points.hg);
                vertices.Add(Points.fh);
                vertices.Add(Points.ba);
                vertices.Add(Points.ac);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.cg);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.cg);
                break;
            case 159:   // H--EDCBA
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.fh);
                vertices.Add(Points.ge);
                vertices.Add(Points.gh);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ge);
                break;
            case 160:   // H-F-----
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                break;
            case 161:   // H-F----A
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.ae);
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                break;
            case 162:   // H-F---B-
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.hd);
                vertices.Add(Points.ab);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 163:   // H-F---BA
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.hd);
                vertices.Add(Points.ea);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ea);
                vertices.Add(Points.ac);
                break;
            case 164:   // H-F--C--
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.hg);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                break;
            case 165:   // H-F--C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.fb);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                break;
            case 166:   // H-F--CB-
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ef);
                vertices.Add(Points.ba);
                vertices.Add(Points.hd);
                vertices.Add(Points.ab);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                break;
            case 167:   // H-F--CBA
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.ef);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dc);
                break;
            case 168:   // H-F-D---
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.dc);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                vertices.Add(Points.cd);
                break;
            case 169:   // H-F-D--A
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.dc);
                vertices.Add(Points.fb);
                vertices.Add(Points.bd);
                vertices.Add(Points.dc);
                vertices.Add(Points.ae);
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                break;
            case 170:   // H-F-D-B-
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                break;
            case 171:   // H-F-D-BA
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.dc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 172:   // H-F-DC--
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.hg);
                vertices.Add(Points.fb);
                vertices.Add(Points.bd);
                vertices.Add(Points.hg);
                vertices.Add(Points.bd);
                vertices.Add(Points.ac);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.bd);
                vertices.Add(Points.cg);
                break;
            case 173:   // H-F-DC-A
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.ef);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.gc);
                vertices.Add(Points.bf);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.ea);
                break;
            case 174:   // H-F-DCB-
                vertices.Add(Points.fe);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.cg);
                vertices.Add(Points.ba);
                vertices.Add(Points.ac);
                vertices.Add(Points.gc);
                break;
            case 175:   // H-F-DCBA
                vertices.Add(Points.fe);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.cg);
                break;
            case 176:   // H-FE----
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.hg);
                break;
            case 177:   // H-FE---A
                vertices.Add(Points.ab);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.gh);
                vertices.Add(Points.eg);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.eg);
                break;
            case 178:   // H-FE--B-
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.hd);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.gh);
                vertices.Add(Points.ab);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 179:   // H-FE--BA
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.gh);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                vertices.Add(Points.gh);
                vertices.Add(Points.ac);
                vertices.Add(Points.hd);
                break;
            case 180:   // H-FE-C--
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.hg);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                break;
            case 181:   // H-FE-C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.gh);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dc);
                break;
            case 182:   // H-FE-CB-
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.hd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.cd);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ab);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                vertices.Add(Points.hg);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.hg);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                break;
            case 183:   // H-FE-CBA
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);

                vertices.Add(Points.dc);
                vertices.Add(Points.gh);
                vertices.Add(Points.gc);
                vertices.Add(Points.dc);
                vertices.Add(Points.dh);
                vertices.Add(Points.gh);

                break;
            case 184:   // H-FED---
                vertices.Add(Points.ea);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.cd);
                vertices.Add(Points.fb);
                vertices.Add(Points.bd);
                break;
            case 185:   // H-FED--A
                vertices.Add(Points.ge);
                vertices.Add(Points.cd);
                vertices.Add(Points.gh);
                vertices.Add(Points.ge);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.cd);
                break;
            case 186:   // H-FED-B-
                vertices.Add(Points.ea);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                break;
            case 187:   // H-FED-BA
                vertices.Add(Points.eg);
                vertices.Add(Points.dc);
                vertices.Add(Points.hg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                break;
            case 188:   // H-FEDC--
                vertices.Add(Points.ae);
                vertices.Add(Points.bd);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.ae);
                vertices.Add(Points.gc);
                vertices.Add(Points.ge);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.gc);
                break;
            case 189:   // H-FEDC-A
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                break;
            case 190:   // H-FEDCB-
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.gc);
                vertices.Add(Points.ge);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.gc);
                break;
            case 191:   // H-FEDCBA
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.gh);
                break;
            case 192:   // HG------
                vertices.Add(Points.ge);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ge);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                break;
            case 193:   // HG-----A
                vertices.Add(Points.ge);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ge);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                break;
            case 194:   // HG----B-
                vertices.Add(Points.ge);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ge);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 195:   // HG----BA
                vertices.Add(Points.ge);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ge);
                vertices.Add(Points.hf);
                vertices.Add(Points.hd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.bf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                break;
            case 196:   // HG---C--
                vertices.Add(Points.ge);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ge);
                vertices.Add(Points.hf);
                vertices.Add(Points.cd);
                vertices.Add(Points.cd);
                vertices.Add(Points.fh);
                vertices.Add(Points.hd);
                break;
            case 197:   // HG---C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.hd);
                vertices.Add(Points.ae);
                vertices.Add(Points.eg);
                vertices.Add(Points.dh);
                vertices.Add(Points.eg);
                vertices.Add(Points.fh);
                vertices.Add(Points.dh);
                break;
            case 198:   // HG---CB-
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ge);
                vertices.Add(Points.cd);
                vertices.Add(Points.ge);
                vertices.Add(Points.hd);
                vertices.Add(Points.eg);
                vertices.Add(Points.fh);
                vertices.Add(Points.hd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 199:   // HG---CBA
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.bf);
                vertices.Add(Points.eg);
                vertices.Add(Points.fh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.eg);
                vertices.Add(Points.fb);
                vertices.Add(Points.dh);
                vertices.Add(Points.db);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.dh);
                break;
            case 200:   // HG--D---
                vertices.Add(Points.ge);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.ge);
                vertices.Add(Points.hf);
                vertices.Add(Points.db);
                vertices.Add(Points.gc);
                vertices.Add(Points.eg);
                vertices.Add(Points.cd);
                break;
            case 201:   // HG--D--A
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.hf);
                vertices.Add(Points.hf);
                vertices.Add(Points.gc);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.hf);
                vertices.Add(Points.dc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ge);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                break;
            case 202:   // HG--D-B-
                vertices.Add(Points.bf);
                vertices.Add(Points.eg);
                vertices.Add(Points.fh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.bf);
                vertices.Add(Points.eg);
                vertices.Add(Points.fb);
                vertices.Add(Points.gc);
                break;
            case 203:   // HG--D-BA
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.fb);
                vertices.Add(Points.eg);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.ae);
                vertices.Add(Points.eg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ca);
                break;
            case 204:   // HG--DC--
                vertices.Add(Points.hf);
                vertices.Add(Points.ca);
                vertices.Add(Points.ge);
                vertices.Add(Points.hf);
                vertices.Add(Points.db);
                vertices.Add(Points.ca);
                break;
            case 205:   // HG--DC-A
                vertices.Add(Points.ge);
                vertices.Add(Points.hf);
                vertices.Add(Points.ae);
                vertices.Add(Points.hf);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.ea);
                vertices.Add(Points.hf);
                vertices.Add(Points.ab);
                break;
            case 206:   // HG--DCB-
                vertices.Add(Points.eg);
                vertices.Add(Points.fh);
                vertices.Add(Points.fb);
                vertices.Add(Points.eg);
                vertices.Add(Points.ba);
                vertices.Add(Points.ac);
                vertices.Add(Points.fb);
                vertices.Add(Points.ba);
                vertices.Add(Points.eg);
                break;
            case 207:   // HG--DCBA
                vertices.Add(Points.bf);
                vertices.Add(Points.ge);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                break;
            case 208:   // HG-E----
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.hd);
                vertices.Add(Points.ef);
                vertices.Add(Points.fh);
                vertices.Add(Points.hd);
                break;
            case 209:   // HG-E---A
                vertices.Add(Points.ab);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.hd);
                vertices.Add(Points.dh);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                break;
            case 210:   // HG-E--B-
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.hd);
                vertices.Add(Points.ef);
                vertices.Add(Points.fh);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.hd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 211:   // HG-E--BA
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.ac);
                vertices.Add(Points.dh);
                vertices.Add(Points.cg);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.dh);
                vertices.Add(Points.fb);
                vertices.Add(Points.dh);
                vertices.Add(Points.db);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.dh);
                break;
            case 212:   // HG-E-C--
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.hd);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.cd);
                vertices.Add(Points.dh);
                vertices.Add(Points.ef);
                vertices.Add(Points.fh);
                break;
            case 213:   // HG-E-C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.hd);
                vertices.Add(Points.dc);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.dh);
                vertices.Add(Points.dh);
                vertices.Add(Points.ef);
                vertices.Add(Points.fh);
                break;
            case 214:   // HG-E-CB-
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ae);
                vertices.Add(Points.ef);
                vertices.Add(Points.fh);
                vertices.Add(Points.dh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 215:   // HG-E-CBA
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.dc);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.fb);
                vertices.Add(Points.dh);
                vertices.Add(Points.db);
                vertices.Add(Points.fb);
                vertices.Add(Points.fh);
                vertices.Add(Points.dh);
                break;
            case 216:   // HG-ED---
                vertices.Add(Points.ea);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.ae);
                vertices.Add(Points.ef);
                vertices.Add(Points.cd);
                vertices.Add(Points.dc);
                vertices.Add(Points.ef);
                vertices.Add(Points.fh);
                vertices.Add(Points.cd);
                vertices.Add(Points.hf);
                vertices.Add(Points.bd);
                break;
            case 217:   // HG-ED--A
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.ba);
                vertices.Add(Points.fh);
                vertices.Add(Points.bd);
                vertices.Add(Points.ba);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.ba);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.cd);
                break;
            case 218:   // HG-ED-B-
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.ae);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.cd);
                vertices.Add(Points.fe);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.ab);
                break;
            case 219:   // HG-ED-BA
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                break;
            case 220:   // HG-EDC--
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.db);
                vertices.Add(Points.db);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.bd);
                break;
            case 221:   // HG-EDC-A
                vertices.Add(Points.ab);
                vertices.Add(Points.hf);
                vertices.Add(Points.db);
                vertices.Add(Points.ab);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                break;
            case 222:   // HG-EDCB-
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.fh);
                vertices.Add(Points.fe);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.ab);
                break;
            case 223:   // HG-EDCBA
                vertices.Add(Points.bf);
                vertices.Add(Points.ef);
                vertices.Add(Points.hf);
                break;
            case 224:   // HGF-----
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                break;
            case 225:   // HGF----A
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.ab);
                vertices.Add(Points.ae);
                vertices.Add(Points.ac);
                vertices.Add(Points.gc);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.ae);
                vertices.Add(Points.fb);
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                break;
            case 226:   // HGF---B-
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.ab);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 227:   // HGF---BA
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.ac);
                vertices.Add(Points.dh);
                vertices.Add(Points.cg);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.dh);
                vertices.Add(Points.eg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ca);
                break;
            case 228:   // HGF--C--
                vertices.Add(Points.fe);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.ac);
                vertices.Add(Points.eg);
                vertices.Add(Points.cd);
                vertices.Add(Points.ge);
                vertices.Add(Points.ef);
                break;
            case 229:   // HGF--C-A
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.ab);
                vertices.Add(Points.dh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.bf);
                vertices.Add(Points.dh);
                vertices.Add(Points.bf);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.ea);
                break;
            case 230:   // HGF--CB-
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.fe);
                vertices.Add(Points.ac);
                vertices.Add(Points.eg);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.ac);
                vertices.Add(Points.ac);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.db);
                break;
            case 231:   // HGF--CBA
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                break;
            case 232:   // HGF-D---
                vertices.Add(Points.fe);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.fe);
                vertices.Add(Points.fb);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.cd);
                vertices.Add(Points.fb);
                vertices.Add(Points.bd);
                break;
            case 233:   // HGF-D--A
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.fe);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ca);
                vertices.Add(Points.ba);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.cd);
                break;
            case 234:   // HGF-D-B-
                vertices.Add(Points.ef);
                vertices.Add(Points.ba);
                vertices.Add(Points.gc);
                vertices.Add(Points.gc);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.gc);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                break;
            case 235:   // HGF-D-BA
                vertices.Add(Points.ef);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ca);
                vertices.Add(Points.cg);
                vertices.Add(Points.eg);
                vertices.Add(Points.ea);
                vertices.Add(Points.ca);
                break;
            case 236:   // HGF-DC--
                vertices.Add(Points.ef);
                vertices.Add(Points.fb);
                vertices.Add(Points.ac);
                vertices.Add(Points.ac);
                vertices.Add(Points.eg);
                vertices.Add(Points.ef);
                vertices.Add(Points.ac);
                vertices.Add(Points.fb);
                vertices.Add(Points.bd);
                break;
            case 237:   // HGF-DC-A
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.fe);
                vertices.Add(Points.ea);
                vertices.Add(Points.eg);
                vertices.Add(Points.bf);
                vertices.Add(Points.ea);
                vertices.Add(Points.ef);
                vertices.Add(Points.bf);
                vertices.Add(Points.ba);
                vertices.Add(Points.ea);
                break;
            case 238:   // HGF-DCB-
                vertices.Add(Points.fe);
                vertices.Add(Points.ca);
                vertices.Add(Points.ge);
                vertices.Add(Points.fe);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 239:   // HGF-DCBA
                vertices.Add(Points.fe);
                vertices.Add(Points.ae);
                vertices.Add(Points.ge);
                break;
            case 240:   // HGFE----
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                break;
            case 241:   // HGFE---A
                vertices.Add(Points.ab);
                vertices.Add(Points.bf);
                vertices.Add(Points.cg);
                vertices.Add(Points.cg);
                vertices.Add(Points.bf);
                vertices.Add(Points.dh);
                vertices.Add(Points.cg);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                break;
            case 242:   // HGFE--B-
                vertices.Add(Points.ea);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.hd);
                vertices.Add(Points.ab);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 243:   // HGFE--BA
                vertices.Add(Points.ac);
                vertices.Add(Points.hd);
                vertices.Add(Points.gc);
                vertices.Add(Points.ac);
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                break;
            case 244:   // HGFE-C--
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                vertices.Add(Points.ac);
                vertices.Add(Points.ae);
                vertices.Add(Points.dh);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.dh);
                break;
            case 245:   // HGFE-C-A
                vertices.Add(Points.ab);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.hd);
                break;
            case 246:   // HGFE-CB-
                vertices.Add(Points.ae);
                vertices.Add(Points.ab);
                vertices.Add(Points.ac);
                vertices.Add(Points.db);
                vertices.Add(Points.dh);
                vertices.Add(Points.dc);
                vertices.Add(Points.ac);
                vertices.Add(Points.db);
                vertices.Add(Points.dc);
                vertices.Add(Points.ac);
                vertices.Add(Points.ab);
                vertices.Add(Points.db);
                break;
            case 247:   // HGFE-CBA
                vertices.Add(Points.bd);
                vertices.Add(Points.hd);
                vertices.Add(Points.cd);
                break;
            case 248:   // HGFED---
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.gc);
                vertices.Add(Points.gc);
                vertices.Add(Points.fb);
                vertices.Add(Points.bd);
                vertices.Add(Points.gc);
                vertices.Add(Points.bd);
                vertices.Add(Points.dc);
                break;
            case 249:   // HGFED--A
                vertices.Add(Points.ba);
                vertices.Add(Points.bf);
                vertices.Add(Points.bd);
                vertices.Add(Points.ca);
                vertices.Add(Points.cd);
                vertices.Add(Points.cg);
                vertices.Add(Points.ba);
                vertices.Add(Points.cd);
                vertices.Add(Points.ca);
                vertices.Add(Points.ba);
                vertices.Add(Points.bd);
                vertices.Add(Points.cd);
                break;
            case 250:   // HGFED-B-
                vertices.Add(Points.ea);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.dc);
                break;
            case 251:   // HGFED-BA
                vertices.Add(Points.ac);
                vertices.Add(Points.dc);
                vertices.Add(Points.gc);
                break;
            case 252:   // HGFEDC--
                vertices.Add(Points.ea);
                vertices.Add(Points.db);
                vertices.Add(Points.ca);
                vertices.Add(Points.ea);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                break;
            case 253:   // HGFEDC-A
                vertices.Add(Points.ab);
                vertices.Add(Points.fb);
                vertices.Add(Points.db);
                break;
            case 254:   // HGFEDCB-
                vertices.Add(Points.ea);
                vertices.Add(Points.ba);
                vertices.Add(Points.ca);
                break;
            case 255:   // HGFEDCBA
                break;
            default:
                Debug.LogError(string.Format("unhandled config encountered [config]=" + config));
                break;
        }

        int addedCount = vertices.Count - beforeCount;

        //triangles, uvs (verticies added count enables to add proper triangle, uvs array items)
        switch (addedCount)
        {
            case 0:
                break;
            case 3:     // 1 triangle
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                break;
            case 6:     // 2 triangle
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                break;
            case 9:     // 3 triangle
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                break;
            case 12:    // 4 triangle
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                break;
            case 15:    // 5 triangles
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                break;
            case 18:    // 6 triangles
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                break;
            case 21: // 7 triangles
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                break;
            case 24: // 8 triangles
                triangles.Add(vertices.Count - 24);
                triangles.Add(vertices.Count - 23);
                triangles.Add(vertices.Count - 22);
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                break;
            case 27:    //9 triangles
                triangles.Add(vertices.Count - 27);
                triangles.Add(vertices.Count - 26);
                triangles.Add(vertices.Count - 25);
                triangles.Add(vertices.Count - 24);
                triangles.Add(vertices.Count - 23);
                triangles.Add(vertices.Count - 22);
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                break;
            case 30:    //10 triangles
                triangles.Add(vertices.Count - 30);
                triangles.Add(vertices.Count - 29);
                triangles.Add(vertices.Count - 28);
                triangles.Add(vertices.Count - 27);
                triangles.Add(vertices.Count - 26);
                triangles.Add(vertices.Count - 25);
                triangles.Add(vertices.Count - 24);
                triangles.Add(vertices.Count - 23);
                triangles.Add(vertices.Count - 22);
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                break;
            case 33:    //11 triangles
                triangles.Add(vertices.Count - 33);
                triangles.Add(vertices.Count - 32);
                triangles.Add(vertices.Count - 31);
                triangles.Add(vertices.Count - 30);
                triangles.Add(vertices.Count - 29);
                triangles.Add(vertices.Count - 28);
                triangles.Add(vertices.Count - 27);
                triangles.Add(vertices.Count - 26);
                triangles.Add(vertices.Count - 25);
                triangles.Add(vertices.Count - 24);
                triangles.Add(vertices.Count - 23);
                triangles.Add(vertices.Count - 22);
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                break;
            case 36:    //12 triangles
                triangles.Add(vertices.Count - 36);
                triangles.Add(vertices.Count - 35);
                triangles.Add(vertices.Count - 34);
                triangles.Add(vertices.Count - 33);
                triangles.Add(vertices.Count - 32);
                triangles.Add(vertices.Count - 31);
                triangles.Add(vertices.Count - 30);
                triangles.Add(vertices.Count - 29);
                triangles.Add(vertices.Count - 28);
                triangles.Add(vertices.Count - 27);
                triangles.Add(vertices.Count - 26);
                triangles.Add(vertices.Count - 25);
                triangles.Add(vertices.Count - 24);
                triangles.Add(vertices.Count - 23);
                triangles.Add(vertices.Count - 22);
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                break;
            case 39:    //13 triangles
                triangles.Add(vertices.Count - 39);
                triangles.Add(vertices.Count - 38);
                triangles.Add(vertices.Count - 37);
                triangles.Add(vertices.Count - 36);
                triangles.Add(vertices.Count - 35);
                triangles.Add(vertices.Count - 34);
                triangles.Add(vertices.Count - 33);
                triangles.Add(vertices.Count - 32);
                triangles.Add(vertices.Count - 31);
                triangles.Add(vertices.Count - 30);
                triangles.Add(vertices.Count - 29);
                triangles.Add(vertices.Count - 28);
                triangles.Add(vertices.Count - 27);
                triangles.Add(vertices.Count - 26);
                triangles.Add(vertices.Count - 25);
                triangles.Add(vertices.Count - 24);
                triangles.Add(vertices.Count - 23);
                triangles.Add(vertices.Count - 22);
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                break;
            case 42:    //14 triangles
                triangles.Add(vertices.Count - 42);
                triangles.Add(vertices.Count - 41);
                triangles.Add(vertices.Count - 40);
                triangles.Add(vertices.Count - 39);
                triangles.Add(vertices.Count - 38);
                triangles.Add(vertices.Count - 37);
                triangles.Add(vertices.Count - 36);
                triangles.Add(vertices.Count - 35);
                triangles.Add(vertices.Count - 34);
                triangles.Add(vertices.Count - 33);
                triangles.Add(vertices.Count - 32);
                triangles.Add(vertices.Count - 31);
                triangles.Add(vertices.Count - 30);
                triangles.Add(vertices.Count - 29);
                triangles.Add(vertices.Count - 28);
                triangles.Add(vertices.Count - 27);
                triangles.Add(vertices.Count - 26);
                triangles.Add(vertices.Count - 25);
                triangles.Add(vertices.Count - 24);
                triangles.Add(vertices.Count - 23);
                triangles.Add(vertices.Count - 22);
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                break;
            case 45:    //15 triangles
                triangles.Add(vertices.Count - 45);
                triangles.Add(vertices.Count - 44);
                triangles.Add(vertices.Count - 43);
                triangles.Add(vertices.Count - 42);
                triangles.Add(vertices.Count - 41);
                triangles.Add(vertices.Count - 40);
                triangles.Add(vertices.Count - 39);
                triangles.Add(vertices.Count - 38);
                triangles.Add(vertices.Count - 37);
                triangles.Add(vertices.Count - 36);
                triangles.Add(vertices.Count - 35);
                triangles.Add(vertices.Count - 34);
                triangles.Add(vertices.Count - 33);
                triangles.Add(vertices.Count - 32);
                triangles.Add(vertices.Count - 31);
                triangles.Add(vertices.Count - 30);
                triangles.Add(vertices.Count - 29);
                triangles.Add(vertices.Count - 28);
                triangles.Add(vertices.Count - 27);
                triangles.Add(vertices.Count - 26);
                triangles.Add(vertices.Count - 25);
                triangles.Add(vertices.Count - 24);
                triangles.Add(vertices.Count - 23);
                triangles.Add(vertices.Count - 22);
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                break;
            case 48:    //16 triangles
                triangles.Add(vertices.Count - 48);
                triangles.Add(vertices.Count - 47);
                triangles.Add(vertices.Count - 46);
                triangles.Add(vertices.Count - 45);
                triangles.Add(vertices.Count - 44);
                triangles.Add(vertices.Count - 43);
                triangles.Add(vertices.Count - 42);
                triangles.Add(vertices.Count - 41);
                triangles.Add(vertices.Count - 40);
                triangles.Add(vertices.Count - 39);
                triangles.Add(vertices.Count - 38);
                triangles.Add(vertices.Count - 37);
                triangles.Add(vertices.Count - 36);
                triangles.Add(vertices.Count - 35);
                triangles.Add(vertices.Count - 34);
                triangles.Add(vertices.Count - 33);
                triangles.Add(vertices.Count - 32);
                triangles.Add(vertices.Count - 31);
                triangles.Add(vertices.Count - 30);
                triangles.Add(vertices.Count - 29);
                triangles.Add(vertices.Count - 28);
                triangles.Add(vertices.Count - 27);
                triangles.Add(vertices.Count - 26);
                triangles.Add(vertices.Count - 25);
                triangles.Add(vertices.Count - 24);
                triangles.Add(vertices.Count - 23);
                triangles.Add(vertices.Count - 22);
                triangles.Add(vertices.Count - 21);
                triangles.Add(vertices.Count - 20);
                triangles.Add(vertices.Count - 19);
                triangles.Add(vertices.Count - 18);
                triangles.Add(vertices.Count - 17);
                triangles.Add(vertices.Count - 16);
                triangles.Add(vertices.Count - 15);
                triangles.Add(vertices.Count - 14);
                triangles.Add(vertices.Count - 13);
                triangles.Add(vertices.Count - 12);
                triangles.Add(vertices.Count - 11);
                triangles.Add(vertices.Count - 10);
                triangles.Add(vertices.Count - 9);
                triangles.Add(vertices.Count - 8);
                triangles.Add(vertices.Count - 7);
                triangles.Add(vertices.Count - 6);
                triangles.Add(vertices.Count - 5);
                triangles.Add(vertices.Count - 4);
                triangles.Add(vertices.Count - 3);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.D);
                uv.Add(UVCoord.C);
                uv.Add(UVCoord.A);
                uv.Add(UVCoord.B);
                uv.Add(UVCoord.D);
                break;
            default:
                Debug.LogError("unhandled addedCount encountered [addedCount]= " + addedCount);
                break;
        }

        return addedCount;
    }
}
