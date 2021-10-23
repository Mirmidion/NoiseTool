using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using Random = UnityEngine.Random;

public class NoiseNode : Node
{
    // The value of an output node field is not used for anything, but could be used for caching output results
    [Output] public Texture2D noise;

    [Input] public Vector3 inputPoint;
    [Output] public Vector4[] outputPoint;

    Color[] pixels;
    private Vector4[] output;

    // The value of 'mathType' will be displayed on the node in an editable format, similar to the inspector
    public NoiseType noiseType = NoiseType.Perlin;
    public enum NoiseType { Random, RawPerlin, Perlin, Rigid, Simplex, Voronoi }
    
    int width = 150;
    int height = 150;

    [HideInInspector]
    public float xOrg = 0.0f, yOrg = 0.0f, zOrg = 0.0f, scale = 1.0f;

    [HideInInspector]
    public int seed = 10;
    [HideInInspector]
    public int octaves = 10;
   
    [HideInInspector]
    public float persistance = 1, lacunarity = 1, amplitude = 1, frequency = 1;
    [HideInInspector]
    public float weightMultiplier = .8f, strength = 1, baseRoughness = 1, roughness = 2, minValue;
    [Range(1, 8), HideInInspector]
    public int numLayers = 1;
    

    [HideInInspector]
    public VoronoiNoise.VORONOI_DISTANCE distanceMode;

    [HideInInspector]
    public VoronoiNoise.VORONOI_COMBINATION combinationMode;

    float delay = 0;

    /* ----- || ----- \\
    
    Variables for getting a value when generating a planet

    \\ ----- || ----- */


    [HideInInspector]
    public float posX, posY;



    /* ----- || ----- \\
    


    \\ ----- || ----- */

    

    public void SetPositionForAllNoiseNodes(float posX, float posY)
    {
        foreach(Node node in graph.nodes)
        {
            if (node.GetType() == typeof(NoiseNode))
            {
                NoiseNode noiseNode = (NoiseNode) node;
                noiseNode.posX = posX;
                noiseNode.posY = posY;
            }
        }
    }

    private void OnValidate()
    {
        if (Time.time - delay > 0.02f)
        {
            Generate();
            delay = Time.time;
        }
    }

    public void TriggerValidate()
    {
        OnValidate();
    }

    public void RegenerateBranch()
    {
        foreach (NodePort childPort in GetOutputPort("noise").GetConnections())
        {
            if (childPort.IsInput)
            {
                Node node = childPort.node;

                if (node.GetType() == typeof(ProcessNode))
                {
                    ProcessNode node1 = (ProcessNode)node;
                    node1.TriggerValidate();
                    node1.RegenerateBranch();
                }
                else if (node.GetType() == typeof(NoiseNode))
                {
                    NoiseNode node1 = (NoiseNode)node;
                    node1.TriggerValidate();
                    node1.RegenerateBranch();
                }
            }
        }
    }

    public void GenerateNoise()
    {
        switch (noiseType)
        {
            default:
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
            case NoiseType.Rigid:
                {
                    RigidNoise();
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
    }

    public void Generate3DNoise()
    {
        switch (noiseType)
        {
            default:
            {
                Random3DNoise();
                break;
            }
            case NoiseType.Simplex:
            case NoiseType.RawPerlin:
            {
                Simplex3DNoise();
                break;
            }
            case NoiseType.Perlin:
            {
                Perlin3DNoise();
                break;
            }
            case NoiseType.Rigid:
            {
                Rigid3DNoise();
                break;
            }
            case NoiseType.Voronoi:
            {
                Voronoi3DNoise();
                break;
            }

        }
    }

    public void Generate()
    {
        GenerateNoise();
        noise.SetPixels(pixels);
        noise.Apply();
    }

    public void Generate3D()
    {
        SetPointArray();
        Generate3DNoise();
    }

    // GetValue should be overridden to return a value for any specified output port
    public override object GetValue(NodePort port)
    {


        // After you've gotten your input values, you can perform your calculations and return a value
        if (port.fieldName == "pixels") {
            Generate();
            return pixels;
        }

        if (port.fieldName.Equals("noise"))
        {
            Generate();
            return noise;
        }

        if (port.fieldName.Equals("outputPoint"))
        {
            Generate3D();
            return output;
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

    public void RigidNoise()
    {
        noise = new Texture2D(width, height, TextureFormat.RGB24, false);
        pixels = new Color[width * height];
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                float noiseValue = 0;
                float frequency = baseRoughness;
                float amplitude = this.amplitude;
                float weight = 1;

                for (int i = 0; i < numLayers; i++)
                {
                    Noise noiseFunction = new Noise();
                    float v = 1 - Mathf.Abs(noiseFunction.Evaluate(new Vector3(x+xOrg,y+yOrg,0) * frequency));
                    v *= v;
                    v *= weight;
                    weight = Mathf.Clamp01(v * weightMultiplier);

                    noiseValue += v * amplitude;
                    frequency *= roughness;
                    amplitude *= persistance;
                }

                noiseValue -= minValue;
                float value = noiseValue * strength;
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

    public void SetPointArray()
    {
        Vector3 point = GetInputValue<Vector3>("inputPoint");
        point = new Vector3(Mathf.Round(point.x), Mathf.Round(point.y), Mathf.Round(point.z));
        output = new Vector4[7];
        output[0] =  new Vector4(point.x, point.y, point.z, 0);
        output[1] = new Vector4(point.x + 1, point.y, point.z, 0); //pos x
        output[2] = new Vector4(point.x, point.y + 1, point.z, 0); //pos y
        output[3] = new Vector4(point.x , point.y, point.z + 1, 0); //pos z
        output[4] = new Vector4(point.x - 1, point.y, point.z, 0); //neg x
        output[5] = new Vector4(point.x, point.y - 1, point.z, 0); //neg y
        output[6] = new Vector4(point.x, point.y, point.z - 1, 0); //neg z
    }

    public void Random3DNoise()
    {
        for (int i = 0; i < 7; i++)
        {
            output[i].w = Random.Range(0f, 1f);
        }
    }

    public void Simplex3DNoise()
    {
        for (int i = 0; i < 7; i++)
        {
            output[i].w = SimplexNoise.SimplexNoise.CalcPixel3D((int)output[i].x, (int)output[i].y, (int)output[i].z, 1);
        }
    }

    public void Voronoi3DNoise()
    {
        VoronoiNoise vn = new VoronoiNoise(seed, frequency, amplitude, distanceMode, combinationMode);
        for (int i = 0; i < 7; i++)
        {
            output[i].w = vn.Sample3D((int)output[i].x, (int)output[i].y, (int)output[i].z);
        }
    }

    public void Rigid3DNoise()
    {
        for (int a = 0; a < 7; a++){
            float noiseValue = 0;
            float frequency = baseRoughness;
            float amplitude = this.amplitude;
            float weight = 1;
            for (int i = 0; i < numLayers; i++)
            {
                Noise noiseFunction = new Noise();
                float v = 1 - Mathf.Abs(noiseFunction.Evaluate(output[a] * frequency));
                v *= v;
                v *= weight;
                weight = Mathf.Clamp01(v * weightMultiplier);

                noiseValue += v * amplitude;
                frequency *= roughness; 
                amplitude *= persistance;
            }
            noiseValue -= minValue;
            float value = noiseValue * strength;
            output[a].w = value;
        }
    }

    public void Perlin3DNoise()
    {
        System.Random prng = new System.Random(seed);
        Vector3[] octaveOffsets = new Vector3[octaves];

        float maxPossibleHeight = 0;
        float tempAmplitude = amplitude;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + xOrg;
            float offsetY = prng.Next(-100000, 100000) - yOrg;
            float offsetZ = prng.Next(-100000, 100000) - zOrg;
            octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);

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

        for (int a = 0; a < 7; a++)
        { 
            tempAmplitude = amplitude;
            float tempFrequency = frequency;
            float noiseHeight = 0;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (output[a].x - halfWidth + octaveOffsets[i].x) / scale * tempFrequency;
                float sampleY = (output[a].y - halfHeight + octaveOffsets[i].y) / scale * tempFrequency;
                float sampleZ = (output[a].z - halfHeight + octaveOffsets[i].z) / scale * tempFrequency;

                float perlinValue = SimplexNoise.SimplexNoise.CalcPixel3D((int)sampleX, (int)sampleY, (int)sampleZ, 1) * 2 - 1;
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
            output[a].w = noiseHeight;
        }

        for (int a = 0; a < 7; a++)
        {
            float normalizedHeight = (output[a].w + 1) / (2f * maxPossibleHeight / 2f);
            float value = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
            output[a].w = value;
        }
    }
}
