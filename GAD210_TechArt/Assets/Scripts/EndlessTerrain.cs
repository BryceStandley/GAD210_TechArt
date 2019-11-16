﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    public LODInfo[] detailLevels;
    public static float maxViewDistance;
    public Transform viewer;
    public Material mapMaterial;

    static MapGenerator mapGenerator;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisableLastUpdate = new List<TerrainChunk>();

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    int chunkSize;
    int chunkVisableInViewDistance;

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = mapGenerator.mapChunkSize - 1;
        maxViewDistance = detailLevels[detailLevels.Length-1].visibleDistanceThreshold;
        chunkVisableInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
        
        UpdateVisableChunks();
    }

    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;
        if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisableChunks();
        }
        
    }
    void UpdateVisableChunks()
    {
        for(int i = 0; i < terrainChunksVisableLastUpdate.Count; i++)
        {
            terrainChunksVisableLastUpdate[i].SetVisable(false);
        }
        terrainChunksVisableLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for(int yOffset = -chunkVisableInViewDistance; yOffset <= chunkVisableInViewDistance; yOffset++)
        {
            for(int xOffset = -chunkVisableInViewDistance; xOffset <= chunkVisableInViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if(terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }
    }


    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        MeshCollider meshCollider;
        LODInfo[] detailLevels;
        LODMesh[] LODMeshes;
        LODMesh collisionLODMesh;
        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;
        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            meshObject = new GameObject("TerrainChunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            SetVisable(false);

            LODMeshes = new LODMesh[detailLevels.Length];
            for(int i = 0; i < detailLevels.Length; i++)
            {
                LODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if(detailLevels[i].useForCollider)
                {
                    collisionLODMesh = LODMeshes[i];
                }
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
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

                        if(lodIndex == 0)
                        {
                            if(collisionLODMesh.hasMesh)
                            {
                                meshCollider.sharedMesh = collisionLODMesh.mesh;
                            }
                            else if(!collisionLODMesh.hasRequestedMesh)
                            {
                                collisionLODMesh.RequestMesh(mapData);
                            }
                        }
                        terrainChunksVisableLastUpdate.Add(this);
                    }
                    SetVisable(visible);
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
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData,lod, OnMeshDataReceived);
        }

    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistanceThreshold;
        public bool useForCollider;

    }


}
