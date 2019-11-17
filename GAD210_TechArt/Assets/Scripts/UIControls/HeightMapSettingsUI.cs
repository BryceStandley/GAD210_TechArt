using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HeightMapSettingsUI : MonoBehaviour
{
    private NoiseSettings noiseSettings = new NoiseSettings();
    private int _scale = 50;
    private int _octaves = 4;
    private float _persistance = 0.5f;
    private float _lacunarity = 2f;
    private int _seed;
    private float _heightMultiplier = 50f;
    public HeightCurveScriptableObject heightCurve;

    public HeightMapSettings heightMapSettings;

    private MapPreview _mapPreview;


    private void Awake() 
    {
        _mapPreview = FindObjectOfType<MapPreview>();
        ApplyNewHeightMap();
        _mapPreview.heightMapSettings = heightMapSettings;
        
    }
    public void UpdateNormalizeMode(TMP_Dropdown dropdown)
    {
        if(dropdown.value == 0)
        {
            noiseSettings.normalizeMode = Noise.NormalizeMode.Local;
            ApplyNewHeightMap();
        }
        else if(dropdown.value == 1)
        {
            noiseSettings.normalizeMode = Noise.NormalizeMode.Global;
            ApplyNewHeightMap();
        }
    }


    public void UpdateScale(float newScale)
    {
        _scale = (int)newScale;
        ApplyNewHeightMap();
    }

    public void UpdateOctaves(float newOctaves)
    {
        _octaves = (int)newOctaves;
        ApplyNewHeightMap();
    }

    public void UpdatePersistance(float newPersistance)
    {
        _persistance = newPersistance;
        ApplyNewHeightMap();
    }

    public void UpdateLacunarity(float newLacunarity)
    {
        _lacunarity = newLacunarity;
        ApplyNewHeightMap();
    }

    public void UpdateSeed(TMP_InputField newSeed)
    {
        int textToSeed = 0;
        for (int i = 0; i < newSeed.text.Length; i++)
        {
            char c = newSeed.text[i];
            textToSeed += c;
        }
        _seed = textToSeed;
        ApplyNewHeightMap();
    }

    public void UpdateHeightMultiplier(float newMulti)
    {
        _heightMultiplier = newMulti;
        ApplyNewHeightMap();
    }

    private void ApplyNewHeightMap()//Apply new settings to heightmap
    {
        noiseSettings.scale = _scale;
        noiseSettings.octaves = _octaves;
        noiseSettings.persistance = _persistance;
        noiseSettings.lacunatity =_lacunarity;
        noiseSettings.seed = _seed;
        heightMapSettings.heightMultiplier = _heightMultiplier;
        heightMapSettings.heightCurve = heightCurve;
        heightMapSettings.noiseSettings = noiseSettings;
        noiseSettings.ValidateValues();
        if(_mapPreview.autoUpdate)
        {
            _mapPreview.DrawMapInEditor();
        }
    }

    public void UpdateHeightMap()
    {
        ApplyNewHeightMap();
        _mapPreview.DrawMapInEditor();
    }
}
