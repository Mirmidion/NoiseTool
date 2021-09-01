using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class NoiseNode : Node
{
    // The value of an output node field is not used for anything, but could be used for caching output results
    [Output] public Texture2D noise;

    [Input] public Vector3 inputPoint;
    [Output] public Vector4[] outputPoint;

    Color[] pixels;

    // The value of 'mathType' will be displayed on the node in an editable format, similar to the inspector
    public NoiseType noiseType = NoiseType.Perlin;
    public enum NoiseType { Random, RawPerlin, Perlin, Rigid, Simplex, Voronoi }
    
    int width = 150;
    int height = 150;

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

    public void GenerateValueAt(Vector2 coordinates)
    {
        SetPositionForAllNoiseNodes(coordinates.x, coordinates.y);
        float noiseValue;
    }

    public void GenerateValueAt()
    {

    }

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
            case NoiseType.Random:
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

    public void Generate()
    {
        GenerateNoise();
        noise.SetPixels(pixels);
        noise.Apply();
    }

    // GetValue should be overridden to return a value for any specified output port
    public override object GetValue(NodePort port)
    {


        // After you've gotten your input values, you can perform your calculations and return a value
        if (port.fieldName == "pixels") {
            Generate();
            return pixels;
        }
        else if (port.fieldName.Equals("noise"))
        {
            Generate();
            return noise;
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
                float amplitude = 1;
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

                noiseValue = noiseValue - minValue;
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

    
}
