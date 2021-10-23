using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;
using System.Linq;

public class ProcessNode : Node
{
    //[Input] public Color[] Noise1;
    //[Input] public Color[] Noise2;

    [Output] public Texture2D noise;

    [Input] public Vector4[] inputPoint;
    [Input] public Vector4[] inputPoint2;
    [Output] public Vector4[] outputPoint;

    Color[] pixels;
    private Vector4[] output;

    public enum mode { Add, Shift, MinMax, Scale, Maximize, Interpolate }
    public mode selectedMode;

    float delay = 0;


    

    [Range(0,100), HideInInspector]
    public int noise1Weight = 50;
    [Range(0, 100), HideInInspector]
    public int noise2Weight = 50, size = 1;
    [Range(0, 100), HideInInspector]
    public float offset = 0f, min = 0f, max = 1f, scaleFirstHalf = 1f, scaleSecondHalf = 1f, multiplier = 1f;
    [HideInInspector]
    public Multiplier[] multipliers; 

    int width = 150;
    int height = 150;

    public void Process()
    {
        pixels = new Color[width * height];
        noise = new Texture2D(width, height, TextureFormat.RGB24, false);
        switch (selectedMode)
        {
            case mode.Add:
            default:
                {
                    AddNoises();
                    break;
                }
            case mode.Shift:
                {
                    ShiftNoise();
                    break;
                }
            case mode.MinMax:
                {
                    MinMaxNoise();
                    break;
                }
            case mode.Scale:
                {
                    ScaleNoise();
                    break;
                }
            case mode.Maximize:
                {
                    Maximize();
                    break;
                }
            case mode.Interpolate:
                {
                    Interpolate();
                    break;
                }
                //case mode.Multiply:
                //    {
                //        MultiplyNoise();
                //        break;
                //    }
        }
    }

    public void Process3D()
    {
        switch (selectedMode)
        {
            default:
            {
                Add3DNoises();
                break;
            }
            case mode.Shift:
            {
                //ShiftNoise();
                break;
            }
            case mode.MinMax:
            {
                //MinMaxNoise();
                break;
            }
            case mode.Scale:
            {
                //ScaleNoise();
                break;
            }
            case mode.Maximize:
            {
                Maximize3DNoises();
                break;
            }
            case mode.Interpolate:
            {
                //Interpolate();
                break;
            }
        }
    }

    public void Generate()
    {
        Process();
        noise.SetPixels(pixels);
        noise.Apply();
    }

    public void Generate3D()
    {
        Process3D();
    }

    public override object GetValue(NodePort port)
    {
        if (port.fieldName == "noise")
        {
            if (noise != null)
            {
                Generate();
            }
            return noise;
        }

        if (port.fieldName == "outputPort")
        {
            Generate3D();
            return output;
        }
        
        return new Color[2];
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

    public void AddNoises()
    {
        Texture2D noiseMap = GetInputValue<Texture2D>("Noise1");
        Texture2D noiseMapToAdd = GetInputValue<Texture2D>("Noise2");
        
        if (noiseMap != null && noiseMapToAdd != null)
        {
            Color[] aColors = noiseMap.GetPixels();
            Color[] bColors = noiseMapToAdd.GetPixels();
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    float sample = (aColors[y * width + x].r * (noise1Weight / 100f)) + (bColors[y * width + x].r * (noise2Weight / 100f));
                    pixels[y * width + x] = new Color(sample, sample, sample);
                }
            }
        }
    }

    public void Add3DNoises()
    {
        Vector4[] worldSpacePoints = GetInputValue<Vector4[]>("inputPoint");
        Vector4[] worldSpacePointsToAdd = GetInputValue<Vector4[]>("inputPoint2");
        if (worldSpacePoints != null && worldSpacePointsToAdd != null)
        {
            for (int i = 0; i < worldSpacePoints.Length; i++)
            {
                output = worldSpacePoints;
                output[i].w = (output[i].w + worldSpacePointsToAdd[i].w) / 2f;
            }
        }
    }

    public void Maximize()
    {
        Texture2D noiseMap = GetInputValue<Texture2D>("Noise");
        Color[] getPixels = noiseMap.GetPixels();
        if (noiseMap != null)
        {
            float maxValue = getPixels.Max(x => x.r);
            float factor = 1f / maxValue; 

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    float sample = getPixels[y * width + x].r * factor;
                    pixels[y * width + x] = new Color(sample, sample, sample);
                }
            }
        }
    }

    public void Maximize3DNoises()
    {
        Vector4[] worldSpacePoints = GetInputValue<Vector4[]>("inputPoint");

        Texture2D noiseMap = GetInputValue<Texture2D>("Noise");
        Color[] getPixels = noiseMap.GetPixels();

        if (noiseMap != null && worldSpacePoints != null)
        {
            float maxValue = getPixels.Max(x => x.r);
            float factor = 1f / maxValue;

            for (int i = 0; i < worldSpacePoints.Length; i++)
            {
                output[i] = new Vector4(worldSpacePoints[i].x, worldSpacePoints[i].y, worldSpacePoints[i].z, worldSpacePoints[i].w * factor);
            }
        }
    }

    public void Interpolate()
    {
        Texture2D a = GetInputValue<Texture2D>("Noise");
        if (a != null)
        {
            Color[] aPixels = a.GetPixels();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float count = 0f;
                    float difference = 0;
                    float sample = aPixels[y * width + x].r;
                    if (y != 0)
                    {
                        difference += aPixels[(y - 1) * width + x].r - sample;
                        count++;
                    }
                    if (y != height - 1)
                    {
                        difference += aPixels[(y + 1) * width + x].r - sample;
                        count++;
                    }
                    if (x != 0)
                    {
                        difference += aPixels[y * width + (x - 1)].r - sample;
                        count++;
                    }
                    if (x != width - 1)
                    {
                        difference += aPixels[y * width + (x + 1)].r - sample;
                        count++;
                    }
                    if (y != height - 1 && x != width - 1)
                    {
                        difference += aPixels[(y + 1) * width + (x + 1)].r - sample;
                        count++;
                    }
                    if (y != 0 && x != width - 1)
                    {
                        difference += aPixels[(y - 1) * width + (x + 1)].r - sample;
                        count++;
                    }
                    if (y != height - 1 && x != 0)
                    {
                        difference += aPixels[(y + 1) * width + (x - 1)].r - sample;
                        count++;
                    }
                    if (y != 0 && x != 0)
                    {
                        difference += aPixels[(y - 1) * width + (x - 1)].r - sample;
                        count++;
                    }

                    difference = difference / count;
                    sample = sample + difference;
                    pixels[y * width + x] = new Color(sample, sample, sample);
                }
            }
        }


    }

    public void MinMaxNoise()
    {
        Texture2D a = GetInputValue<Texture2D>("Noise");
        if (a != null)
        {
            Color[] aPixels = a.GetPixels();
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    float sample = Mathf.Min(max, Mathf.Max(min, aPixels[y * width + x].r));
                    pixels[y * width + x] = new Color(sample, sample, sample);
                }
            }
        }
    }

    public void ScaleNoise()
    {
        Texture2D a = GetInputValue<Texture2D>("Noise");
        if (a != null)
        {
            Color[] aPixels = a.GetPixels();
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    float sample = aPixels[y * width + x].r;
                    if (sample < 0.5f)
                    {
                        sample = sample * scaleFirstHalf;
                    }
                    else if (sample >= 0.5f)
                    {
                        sample = sample * scaleSecondHalf;
                    }
                    pixels[y * width + x] = new Color(sample, sample, sample);
                }
            }
        }
    }

    public void MultiplyNoise()
    {
        Texture2D a = GetInputValue<Texture2D>("Noise");
        if (a != null)
        {
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    float sample = a.GetPixels()[y * width + x].r ;
                    if (multipliers != null)
                    {
                        foreach (Multiplier multi in multipliers)
                        {
                            if (multi != null)
                            {
                                if (multi.from < sample && sample < multi.to)
                                {
                                    sample *= multi.multiplier;
                                    break;
                                }
                            }
                        }
                    }
                    pixels[y * width + x] = new Color(sample, sample, sample);
                }
            }
        }
    }

    public void ShiftNoise()
    {
        Texture2D a = GetInputValue<Texture2D>("Noise");
        if (a != null)
        {
            //float maxValue = 0;
            Color[] pixelsGet = a.GetPixels();
            //for (int y = 0; y < width; y++)
            //{
            //    for (int x = 0; x < height; x++)
            //    {
            //        float sample = pixelsGet[y * width + x].r;

            //        if (sample < maxValue)
            //        {
            //            continue;
            //        }
                    
            //        sample -= offset;
            //        if (sample < 0)
            //        {
            //            sample *= -1;
            //        }
            //        if (sample > maxValue)
            //        {
            //            maxValue = sample;
            //        }


            //    }
            //}
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    float sample = pixelsGet[y * width + x].r;
                    sample -= offset;
                    if (sample < 0)
                    {
                        sample *= -1;
                    }
                    pixels[y * width + x] = new Color(sample, sample, sample);
                }
            }
        }
    }

    public class Multiplier
    {
        public float multiplier = 1f;
        public float from = 0f;
        public float to = 1f;
    }
}
