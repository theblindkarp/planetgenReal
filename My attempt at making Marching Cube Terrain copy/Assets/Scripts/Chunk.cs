using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{

    public int chunkNumber;
    public int[] ownedXUnits = new int[2];
    public int[] ownedYUnits = new int[2];
    public int[] ownedZUnits = new int[2];

    MeshGenerator meshGenerator;

    public Mesh mesh;
    public MeshCollider meshCollider;

    public Color[] colors;

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();

    bool isLoaded;

    void Start()
    {
        meshGenerator = FindObjectOfType<MeshGenerator>();

        //UpdateMesh();

        BoxCollider bc = gameObject.AddComponent(typeof(BoxCollider)) as BoxCollider;

        //int xChunkAmount = Mathf.CeilToInt(meshGenerator.xSize / meshGenerator.chunkSize);
        //int yChunkAmount = Mathf.CeilToInt(meshGenerator.xSize / meshGenerator.chunkSize);
        //int zChunkAmount = Mathf.CeilToInt(meshGenerator.xSize / meshGenerator.chunkSize);

        //int zLoc = 0;
        //if(chunkNumber > 0)
        //{
        //    zLoc = Mathf.FloorToInt((xChunkAmount * yChunkAmount) / chunkNumber);
        //}
        //int yLoc = Mathf.FloorToInt(chunkNumber / xChunkAmount);
        //int xLoc = chunkNumber - yLoc;

        //bc.center = (Vector3.one * (meshGenerator.radius - (meshGenerator.radius / xChunkAmount))) - (new Vector3(xLoc, yLoc, zLoc) * meshGenerator.chunkSize);
        bc.center = new Vector3(ownedXUnits[0], ownedYUnits[0], ownedZUnits[0]) + ((Vector3.one * meshGenerator.chunkSize) / 2);
        bc.size = new Vector3(1, 1, 1) * meshGenerator.chunkSize;
        bc.isTrigger = true;
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Viewer" && other is SphereCollider)
        {
            if(isLoaded == true)
            {
                UnloadChunk();
                //Debug.Log("Unloaded Chunk");

                isLoaded = false;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Viewer" && other is SphereCollider)
        {
            if(isLoaded == false)
            {
                GenerateChunk(ownedXUnits[0], ownedXUnits[1], ownedYUnits[0], ownedYUnits[1], ownedZUnits[0], ownedZUnits[1], chunkNumber);
                //Debug.Log("Reload Chunk");

                isLoaded = true;
            }
        }
    }

    public void CreateChunkMesh()
    {
        if(meshGenerator == null)
            meshGenerator = FindObjectOfType<MeshGenerator>();

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = meshGenerator.material;
        meshCollider = GetComponent<MeshCollider>();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //increase max vertices per mesh
        mesh.name = "ProceduralMesh: #" + chunkNumber;

        if(isLoaded == true)
            GenerateChunk(ownedXUnits[0], ownedXUnits[1], ownedYUnits[0], ownedYUnits[1], ownedZUnits[0], ownedZUnits[1], chunkNumber);
    }

    public void GenerateChunk(int startingChunkX, int endingChunkX, int startingChunkY, int endingChunkY, int startingChunkZ, int endingChunkZ, int chunkID)
    {
        float startTime = Time.realtimeSinceStartup;

        meshGenerator.Clear();
        Clear(); //Clear the mesh triangles and verts

        //MakeGrid();
        meshGenerator.Generate(startingChunkX, endingChunkX, startingChunkY, endingChunkY, startingChunkZ, endingChunkZ);


        //Give it the Vertices and Triangles
        vertices = MeshGenerator.vertices;
        triangles = MeshGenerator.triangles;

        GenerateColor(startingChunkX, endingChunkX, startingChunkY, endingChunkY, startingChunkZ, endingChunkZ);

        UpdateMesh();

        //Debug.Log("Chunk Loaded in " + (Time.realtimeSinceStartup - startTime) + " Seconds.");
    }

    public void GenerateColor(int startingChunkX, int endingChunkX, int startingChunkY, int endingChunkY, int startingChunkZ, int endingChunkZ)
    {
        colors = new Color[vertices.Count];

        meshGenerator.CaveMap(startingChunkX, endingChunkX, startingChunkY, endingChunkY, startingChunkZ, endingChunkZ);
        colors = meshGenerator.planetGradientColors;

        UpdateMesh();
    }

    public void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors;

        mesh.RecalculateNormals();
        meshCollider.sharedMesh = mesh;
    }

    void Clear()
    {
        vertices.Clear();
        triangles.Clear();
    }

    void UnloadChunk()
    {
        mesh.Clear();
        Clear();
    }
}
