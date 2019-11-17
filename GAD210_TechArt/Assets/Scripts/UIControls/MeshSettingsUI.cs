using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeshSettingsUI : MonoBehaviour
{
    public MeshSettings meshSettings;
    private int _size = 4;

    private int _flatSize = 2;

    private float _scale = 2.5f;

    private bool _flatShading;

    private MapPreview _mapPreview;

    private void Awake() 
    {
        _mapPreview = FindObjectOfType<MapPreview>();
        _mapPreview.meshSettings = meshSettings;
        ApplyNewMeshSettings();
    }
    public void UpdateSize(float newSize)
    {
        _size = (int)newSize;
        ApplyNewMeshSettings();
    }

    public void UpdateFlatSize(float newSize)
    {
        _flatSize = (int)newSize;
        if(_flatSize > _size)
        {
            _size = _flatSize;
        }
        ApplyNewMeshSettings();
    }

    public void UpdateScale(float newScale)
    {
        _scale = newScale;
        ApplyNewMeshSettings();
    }

    public void UpdateFlatShading(Toggle toggle)
    {
        if(toggle.isOn)
        {
            _flatShading = true;
        }
        else if(!toggle.isOn)
        {
            _flatShading = false;
        }
        ApplyNewMeshSettings();
    }

    private void ApplyNewMeshSettings()
    {
        meshSettings.chunkSizeIndex = _size;
        meshSettings.flatChunkSizeIndex = _flatSize;
        meshSettings.meshScale = _scale;
        meshSettings.useFlatShading = _flatShading;
        if(_mapPreview.autoUpdate)
        {
            _mapPreview.DrawMapInEditor();
        }
    }

    public void UpdateMeshSettings()
    {
        ApplyNewMeshSettings();
        _mapPreview.DrawMapInEditor();
    }


}
