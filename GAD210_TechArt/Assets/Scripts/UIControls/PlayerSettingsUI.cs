using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class PlayerSettingsUI : MonoBehaviour
{
    public GameObject FPPrefab;
    public Camera meshViewCamera;
    public Camera textureViewCamera;
    public GameObject mapGenerator;
    public GameObject waterPlane;

    private bool _previewMode = true;
    public GameObject player;
    private GameObject _mapGen;
    private bool _postProcessing;
    public bool fpMode;
    private MapPreview _mapPreview;
    public GameObject heightMapSettingsUI;
    public GameObject meshSettingsUI;
    public GameObject menu;

    private void Awake() 
    {
        _mapPreview = FindObjectOfType<MapPreview>();
    }


    public void FirstPersonMode()
    {
        if(!fpMode)
        {
            _previewMode = false;
            fpMode = true;
            _mapPreview.gameObject.SetActive(false);
            _mapGen = Instantiate(mapGenerator);
            _mapGen.transform.position = Vector3.zero;
            player = Instantiate(FPPrefab);
            player.transform.position = new Vector3(0,100,0);
            
            TerrainGenerator terrainGenerator = _mapGen.transform.GetChild(0).gameObject.GetComponent<TerrainGenerator>();
            terrainGenerator.viewer = player.transform;
            terrainGenerator.meshSettings = _mapPreview.meshSettings;
            terrainGenerator.heightMapSettings = _mapPreview.heightMapSettings;
            _mapGen.transform.GetChild(0).gameObject.SetActive(true);
            meshViewCamera.gameObject.SetActive(false);
            textureViewCamera.gameObject.SetActive(false);
            heightMapSettingsUI.SetActive(false);
            meshSettingsUI.SetActive(false);
            menu.SetActive(false);

        }
        else
        {
            Debug.Log("PlayerSettingsUI::FirstPersonMode() Currently in FP Mode, no view change made");
        }
    }

    public void TogglePostProcessing(Toggle toggle)
    {
        if(toggle.isOn)
        {
            _postProcessing = true;
            meshViewCamera.gameObject.GetComponent<PostProcessVolume>().enabled = _postProcessing;
            if(fpMode)
            {
                player.GetComponent<PostProcessVolume>().enabled = _postProcessing;
            }
        }
        else if(!toggle.isOn)
        {
            _postProcessing = false;
            meshViewCamera.gameObject.GetComponent<PostProcessVolume>().enabled = _postProcessing;
            if(fpMode)
            {
                player.GetComponent<PostProcessVolume>().enabled = _postProcessing;
            }
        }
    }

       public void ToggleWaterPlane(Toggle toggle)
    {
        if(toggle.isOn)
        {
            if(_mapPreview.drawMode == MapPreview.DrawMode.Mesh)
            {
                waterPlane.SetActive(true);
            }
            else
            {
                toggle.isOn = false;
            }
        }
        else if(!toggle.isOn)
        {
            waterPlane.SetActive(false);
        }
    }

    public void BackToPreviewMode()
    {
        if(!_previewMode)
        {
            _previewMode = true;
            fpMode = false;
            _mapPreview.gameObject.SetActive(true);
            heightMapSettingsUI.SetActive(true);
            meshSettingsUI.SetActive(true);
            if(_mapPreview.drawMode == MapPreview.DrawMode.Mesh)
            {
                meshViewCamera.gameObject.SetActive(true);
            }
            else if(_mapPreview.drawMode == MapPreview.DrawMode.NoiseMap || _mapPreview.drawMode == MapPreview.DrawMode.Falloff)
            {
                textureViewCamera.gameObject.SetActive(true);
            }
            
            Destroy(player);
            Destroy(_mapGen);
        }
        else
        {
            Debug.Log("PlayerSettingsUI::BackToPreviewMode() Currently in Preview Mode, no view change made");
        }
    }
}
