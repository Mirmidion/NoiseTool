using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class AddNode : Node
{
    //[Input] public Color[] Noise1;
    //[Input] public Color[] Noise2;

    [Output] public Color[] pixels;

    public enum mode { Add, Shift}
    public mode selectedMode;


    [HideInInspector]
     public Texture2D noise;

    [Range(0,100), HideInInspector]
    public int noise1Weight = 50;
    [Range(0, 100), HideInInspector]
    public int noise2Weight = 50;
    [Range(0,100), HideInInspector]
    public float offset = 0f;

    int width = 100;
    int height = 100;


    public override object GetValue(NodePort port)
    {
        if (port.fieldName == "pixels")
        {
            pixels = new Color[width * height];
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
            }

            Texture2D tempNoise = new Texture2D(100, 100, TextureFormat.ARGB32, false);
            tempNoise.SetPixels(pixels);
            tempNoise.Apply();
            noise = tempNoise;
            return pixels;
        }
        else
        {
            return new Color[2];
        }
    }

    private void OnValidate()
    {
        pixels = new Color[width * height];
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
        }

        Texture2D tempNoise = new Texture2D(100, 100, TextureFormat.RGB24, false);
        tempNoise.SetPixels(pixels);
        tempNoise.Apply();
        noise = tempNoise;
    }

    public void AddNoises()
    {
        Color[] a = GetInputValue<Color[]>("Noise1");
        Color[] b = GetInputValue<Color[]>("Noise2");
        for (int y = 0; y < width; y++)
        {
            for (int x = 0; x < height; x++)
            {
                float sample = (a[y * width + x].r * (noise1Weight / 100f)) + (b[y * width + x].r * (noise2Weight / 100f));
                pixels[y * width + x] = new Color(sample, sample, sample);
            }
        }
    }

    public void ShiftNoise()
    {
        Color[] a = GetInputValue<Color[]>("Noise");
        if (a != null)
        {
            float maxValue = 0;
            float originalMaxValue = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {

                    float sample = a[y * width + x].r;
                    if (sample > originalMaxValue)
                    {
                        originalMaxValue = sample;
                    }
                    sample = Mathf.Abs(sample - offset);
                    if (sample > maxValue)
                    {
                        maxValue = sample;
                    }


                }
            }
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    float sample = a[y * width + x].r;
                    sample = Mathf.Abs(sample - offset);
                    pixels[y * width + x] = new Color(sample, sample, sample);
                }
            }
        }
    }
}
