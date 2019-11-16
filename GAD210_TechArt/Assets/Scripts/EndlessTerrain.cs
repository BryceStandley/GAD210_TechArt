using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    const float colliderGenerationDistanceThreshold = 5f;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;
    public static float maxViewDistance;
    public Transform viewer;
    public Material mapMaterial;

    static MapGenerator mapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    float meshChunkSize;
    int chunkVisableInViewDistance;

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        meshChunkSize = mapGenerator.meshSettings.meshWorldSize;
        maxViewDistance = detailLevels[detailLevels.Length-1].visibleDistanceThreshold;
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
                        terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, meshChunkSize, detailLevels,colliderLODIndex, transform, mapMaterial));
                    }
                }

            }
        }
    }


    public class TerrainChunk
    {
        public Vector2 coord;
        GameObject meshObject;
        Vector2 sampleCentre;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        LODInfo[] detailLevels;
        LODMesh[] LODMeshes;
        int colliderLODIndex;
        HeightMap mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        bool hasSetCollider;
        public TerrainChunk(Vector2 coord, float meshWorldSize, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;

            sampleCentre = coord * meshWorldSize /mapGenerator.meshSettings.meshScale;
            Vector2 position = coord * meshWorldSize;
            bounds = new Bounds(sampleCentre, Vector2.one * meshWorldSize);
            
            meshObject = new GameObject("TerrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = new Vector3(position.x, 0, position.y);
            meshObject.transform.parent = parent;
            SetVisable(false);

            LODMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++)
            {
                LODMeshes[i] = new LODMesh(detailLevels[i].lod);
                LODMeshes[i].updateCallback += UpdateTerrainChunk;
                if(i==colliderLODIndex)
                {
                    LODMeshes[i].updateCallback += UpdateCollisionMesh;
                }
            }

            mapGenerator.RequestHeightMap(sampleCentre, OnMapDataReceived);
        }

        void OnMapDataReceived(HeightMap mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;
            UpdateTerrainChunk();
        }


        public void UpdateTerrainChunk() 
        {
            {
                if(mapDataReceived)
                {
                    float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                    bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
                    bool wasVisible = IsVisible();

                    if(visible)
                    {
                        int lodIndex = 0;
                        for(int i = 0; i < detailLevels.Length; i++)
                        {
                            if(viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                            {
                                lodIndex = i+1;
                            }
                            else
                            {
                                break;
                            }
                        }
                        if(lodIndex != previousLODIndex)
                        {
                            LODMesh lodMesh = LODMeshes[lodIndex];
                            if(lodMesh.hasMesh)
                            {
                                previousLODIndex = lodIndex;
                                meshFilter.mesh = lodMesh.mesh;
                            }
                            else if(!lodMesh.hasRequestedMesh)
                            {
                                lodMesh.RequestMesh(mapData);
                            }
                        }
                        
                    }
                    if(wasVisible != visible)
                    {
                        if(visible)
                        {
                            visibleTerrainChunks.Add(this);
                        }
                        else
                        {
                            visibleTerrainChunks.Remove(this);
                        }
                        SetVisable(visible);
                    }
                }
            }
                
        }

        public void UpdateCollisionMesh()
        {
            if(!hasSetCollider)
            {
                float sqrDistanceFromViewerToEdge = bounds.SqrDistance(viewerPosition);

                if(sqrDistanceFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshold)
                {
                    if(!LODMeshes[colliderLODIndex].hasRequestedMesh)
                    {
                        LODMeshes[colliderLODIndex].RequestMesh(mapData);
                    }
                }

                if(sqrDistanceFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
                {
                    if(LODMeshes[colliderLODIndex].hasMesh)
                    {
                        meshCollider.sharedMesh = LODMeshes[colliderLODIndex].mesh;
                        hasSetCollider = true;
                    }
                }
            }
        }

        public void SetVisable(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        public event System.Action updateCallback;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(HeightMap mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData,lod, OnMeshDataReceived);
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


}
