using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshGenerator))]
public class MeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MeshGenerator meshGenerator = (MeshGenerator)target;

        if (GUILayout.Button("Generate"))
        {
            meshGenerator.GenerateTerrain();
        }

        if(GUILayout.Button("Generate Color"))
        {
            meshGenerator.GenerateColor();
        }

        if (GUILayout.Button("Reload Chunk " + meshGenerator.selectedChunk))
        {
            //Finds the Chunk based on the ID
            Chunk chunk = meshGenerator.chunks[meshGenerator.selectedChunk];
            //meshGenerator.chunks[meshGenerator.selectedChunk].GenerateChunk(chunk.ownedXUnits[0], chunk.ownedXUnits[1], chunk.ownedYUnits[0], chunk.ownedYUnits[1], chunk.ownedZUnits[0], chunk.ownedZUnits[1],meshGenerator.selectedChunk);
            chunk.CreateChunkMesh();
        }
    }
}
