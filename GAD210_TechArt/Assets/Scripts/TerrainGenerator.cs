using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;


    public int colliderLODIndex;
    public LODInfo[] detailLevels;
    public static float maxViewDistance;
    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;
    public Transform viewer;
    public Material mapMaterial;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    float meshChunkSize;
    int chunkVisableInViewDistance;

    void Start()
    {
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        meshChunkSize = meshSettings.meshWorldSize;
        float maxViewDistance = detailLevels[detailLevels.Length-1].visibleDistanceThreshold;
        chunkVisableInViewDistance = Mathf.RoundToInt(maxViewDistance / meshChunkSize);
        
        UpdateVisableChunks();
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if(viewerPosition != viewerPositionOld)
        {
            foreach(TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisableChunks();
        }
        
    }
    void UpdateVisableChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for(int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }
        

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshChunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshChunkSize);

        for(int yOffset = -chunkVisableInViewDistance; yOffset <= chunkVisableInViewDistance; yOffset++)
        {
            for(int xOffset = -chunkVisableInViewDistance; xOffset <= chunkVisableInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if(!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if(terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord,heightMapSettings, meshSettings, detailLevels,colliderLODIndex, transform, viewer, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }

            }
        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if(isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }

}

[System.Serializable]
public struct LODInfo
{   
    [Range(0, MeshSettings.numSupportedLOD - 1)]
    public int lod;
    public float visibleDistanceThreshold;
    public float sqrVisibleDistanceThreshold
    {
        get
        {
            return visibleDistanceThreshold * visibleDistanceThreshold;
        }
    }

}