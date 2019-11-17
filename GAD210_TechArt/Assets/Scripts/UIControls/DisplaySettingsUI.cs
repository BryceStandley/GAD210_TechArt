using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DisplaySettingsUI : MonoBehaviour
{
    public Camera meshViewCamera;
    public Camera mapViewCamera;
    
    private MapPreview _mapPreview;
    private void Awake() 
    {
        _mapPreview = FindObjectOfType<MapPreview>();
    }

    public void ChangeDisplayMode(TMP_Dropdown dropdown)
    {
        if(dropdown.value == 0)//Noise Map
        {
            meshViewCamera.gameObject.SetActive(false);
            mapViewCamera.gameObject.SetActive(true);
            _mapPreview.drawMode = MapPreview.DrawMode.NoiseMap;
            _mapPreview.DrawMapInEditor();
        }
        else if (dropdown.value == 1)// Mesh View
        {
            meshViewCamera.gameObject.SetActive(true);
            mapViewCamera.gameObject.SetActive(false);
            _mapPreview.drawMode = MapPreview.DrawMode.Mesh;
            _mapPreview.DrawMapInEditor();
        }
        else if(dropdown.value == 2)//FalloffMap
        {
            meshViewCamera.gameObject.SetActive(false);
            mapViewCamera.gameObject.SetActive(true);
            _mapPreview.drawMode = MapPreview.DrawMode.Falloff;
            _mapPreview.DrawMapInEditor();
        }
    }

    public void LODSlider(float sliderVal)
    {
        _mapPreview.editorLOD = (int)sliderVal;
        if(_mapPreview.autoUpdate)
        {
            _mapPreview.DrawMapInEditor();
        }
    }

    public void AutoUpdateToggle(Toggle toggle)
    {
        if(toggle.isOn)
        {
            _mapPreview.autoUpdate = true;
        }
        else if(!toggle.isOn)
        {
            _mapPreview.autoUpdate = false;
        }
    }
}
