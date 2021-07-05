using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class NoiseNode : Node
{
    // The value of an output node field is not used for anything, but could be used for caching output results
    [HideInInspector]
    public Texture2D noise;
    [Output] public Color[] pixels;

    // The value of 'mathType' will be displayed on the node in an editable format, similar to the inspector
    public NoiseType noiseType = NoiseType.Perlin;
    public enum NoiseType { Random, RawPerlin, Perlin, Simplex, Voronoi }
    
    int width = 100;
    int height = 100;

    [HideInInspector]
    public float xOrg = 0.0f;
    [HideInInspector]
    public float yOrg = 0.0f;
    [HideInInspector]
    public float zOrg = 0.0f;
    [HideInInspector]
    public float scale = 1.0f;

    [HideInInspector]
    public int seed = 10;
    [HideInInspector]
    public int octaves = 10;
   
    [HideInInspector]
    public float persistance = 1;
   
    [HideInInspector]
    public float lacunarity = 1;

    [HideInInspector]
    public float amplitude = 1;

    [HideInInspector]
    public float frequency = 1;

    [HideInInspector]
    public VoronoiNoise.VORONOI_DISTANCE distanceMode;

    [HideInInspector]
    public VoronoiNoise.VORONOI_COMBINATION combinationMode;

    private void OnValidate()
    {
        switch (noiseType)
        {
            case NoiseType.Random:
                {
                    RandomNoise();
                    break;
                }
            case NoiseType.RawPerlin:
                {
                    RawPerlinNoise();
                    break;
                }
            case NoiseType.Perlin:
                {
                    PerlinNoise();
                    break;
                }
            case NoiseType.Simplex:
                {
                    SimplexNoiseGenerator();
                    break;
                }
            case NoiseType.Voronoi:
                {
                    VoronoiNoiseGenerator();
                    break;
                }

        }
        noise.SetPixels(pixels);
        noise.Apply();
    }

    // GetValue should be overridden to return a value for any specified output port
    public override object GetValue(NodePort port)
    {


        // After you've gotten your input values, you can perform your calculations and return a value
        if (port.fieldName == "pixels") { 
            switch (noiseType)
            {
                case NoiseType.Random: default:
                    {
                        RandomNoise();
                        break;
                    }
                case NoiseType.RawPerlin:
                    {
                        RawPerlinNoise();
                        break;
                    }
                case NoiseType.Perlin:
                    {
                        PerlinNoise();
                        break;
                    }
                case NoiseType.Simplex:
                    {
                        SimplexNoiseGenerator();
                        break;
                    }
                case NoiseType.Voronoi:
                    {
                        VoronoiNoiseGenerator();
                        break;
                    }

            }
        noise.SetPixels(pixels);
        noise.Apply();
        return pixels;
        }
        //else if (port.fieldName == "sum") return a + b;
        else return 0f;
    }

    public void RandomNoise()
    {
        noise = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels = new Color[width * height];
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                float sample = Random.Range(0f, 1f);
                pixels[y * noise.width + x] = new Color(sample, sample, sample);
            }
        }
    }

    public void RawPerlinNoise()
    {
        noise = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels = new Color[width * height];
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                float xCoord = xOrg + (float)x / noise.width * scale;
                float yCoord = yOrg + (float)y / noise.height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);
                pixels[y * noise.width + x] = new Color(sample, sample, sample);

            }
        }
    }

    public void PerlinNoise()
    {
        pixels = new Color[width * height];
        noise = new Texture2D(width, height, TextureFormat.RGB24, false);
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float tempAmplitude = amplitude;
        float tempFrequency = frequency;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + xOrg;
            float offsetY = prng.Next(-100000, 100000) - yOrg;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += tempAmplitude;
            tempAmplitude *= persistance;
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {

                tempAmplitude = 1;
                tempFrequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * tempFrequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * tempFrequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * tempAmplitude;

                    tempAmplitude *= persistance;
                    tempFrequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                pixels[y * noise.width + x] = new Color(noiseHeight, noiseHeight, noiseHeight);


            }
        }

        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {

                float normalizedHeight = (pixels[y * noise.width + x].b + 1) / (2f * maxPossibleHeight / 2f);
                float value = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                pixels[y * noise.width + x] = new Color(value, value, value);


            }
        }
    }

    public void SimplexNoiseGenerator()
    {
        noise = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels = new Color[width * height];
        float lowest = 0;
        float highest = 0;
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                float sample = SimplexNoise.SimplexNoise.CalcPixel2D(x+(int)Mathf.Round(xOrg),y+ (int)Mathf.Round(yOrg), scale)/240f;
                if (sample > highest)
                {
                    highest = sample;
                }
                if (sample < lowest)
                {
                    lowest = sample;
                }
                pixels[y * noise.width + x] = new Color(sample, sample, sample);
            }
        }
        Debug.Log(lowest);
        Debug.Log(highest);
    }

    public void VoronoiNoiseGenerator()
    {
        noise = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels = new Color[width * height];
        VoronoiNoise vn = new VoronoiNoise(seed, frequency, amplitude, distanceMode, combinationMode);
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                float sample = vn.Sample2D(x,y);
                pixels[y * noise.width + x] = new Color(sample, sample, sample);
            }
        }
    }

    
}
