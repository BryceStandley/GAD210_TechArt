using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TerrainChunk
    {
        const float colliderGenerationDistanceThreshold = 5f;
        public event System.Action<TerrainChunk, bool> onVisibilityChanged;
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
        HeightMap heightMap;
        bool heightMapReceived;
        int previousLODIndex = -1;
        bool hasSetCollider;
        float maxViewDistance;
        HeightMapSettings heightMapSettings;
        MeshSettings meshSettings;
        Transform viewer;
        public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
        {
            this.coord = coord;
            this.detailLevels = detailLevels;
            this.colliderLODIndex = colliderLODIndex;
            this.heightMapSettings = heightMapSettings;
            this.meshSettings = meshSettings;
            this.viewer = viewer;

            sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
            Vector2 position = coord *  meshSettings.meshWorldSize;
            bounds = new Bounds(sampleCentre, Vector2.one *  meshSettings.meshWorldSize);
            
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

            maxViewDistance = detailLevels[detailLevels.Length-1].visibleDistanceThreshold;
            
        }

        public void Load()
        {
            ThreadedDataRequester.RequestData(GenerateHeightMap, OnHeightMapReceived);
        }

        object GenerateHeightMap()
        {
            return HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre);
        }

        void OnHeightMapReceived(object heightMapObject)
        {
            this.heightMap = (HeightMap)heightMapObject;
            heightMapReceived = true;
            UpdateTerrainChunk();
        }

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }
        public void UpdateTerrainChunk() 
        {
            {
                if(heightMapReceived)
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
                                lodMesh.RequestMesh(heightMap, meshSettings);
                            }
                        }
                        
                    }
                    if(wasVisible != visible)
                    {
                        SetVisable(visible);
                        if(onVisibilityChanged != null)
                        {
                            onVisibilityChanged(this,visible);
                        }
                        
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
                        LODMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
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

        void OnMeshDataReceived(object meshData)
        {
            mesh = ((MeshData)meshData).CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
        {
            hasRequestedMesh = true;
            ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
        }


    }