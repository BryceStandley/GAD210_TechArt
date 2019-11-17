using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int width, int height, HeightMapSettings settings, Vector2 sampleCenter)
    {
        float[,] values = Noise.GenerateNoiseMap(width, height, settings.noiseSettings, sampleCenter);

        AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i,j] *= settings.heightCurve.heightCurve.Evaluate(values[i,j]) * settings.heightMultiplier;

                if(values[i,j] > minValue)
                {
                    maxValue = values[i,j];
                }
                if(values[i,j] < minValue)
                {
                    minValue = values[i,j];
                }
            }
        }
        return new HeightMap(values, minValue, maxValue);
    }
}

public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float min, float max)
    {
        this.values = values;
        this.minValue = min;
        this.maxValue = max;
    }
}
    
