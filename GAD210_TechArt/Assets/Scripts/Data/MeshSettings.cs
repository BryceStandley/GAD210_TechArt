using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MeshSettings : UpdatableData
{
    public const int numSupportedLOD = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = {48, 72, 96, 120, 144, 168, 192, 216, 240};

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;

    [Range(0, numSupportedFlatChunkSizes - 1)]
    public int flatChunkSizeIndex;

    public float meshScale = 2.5f;
    public bool useFlatShading;

    //Number of vertices per line of mesh rendered at LOD = 0. Includes the 2 extra vertices that are excluded from final mesh but used with calculating normals
    public int numVertsPerLine
    {
        get{
            if(useFlatShading)
            {
                return supportedChunkSizes[flatChunkSizeIndex] + 1;
            }
            else 
            {
                return supportedChunkSizes[chunkSizeIndex] + 1;
            }
        }
    }

    public float meshWorldSize
    {
        get
        {
            return(numVertsPerLine - 3) * meshScale;
        }
    }

}
