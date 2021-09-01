using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XNode;

public class SerializeNode : Node
{
    [Input] public Texture2D Input2;
    [HideInInspector]
    public Texture2D noise;

    

    private void OnValidate()
    {

    }

    public override object GetValue(NodePort port)
    {
        
        
        if (port.fieldName.Equals(""))
        {

        }
        return 0f;
    }

    public void Serialize()
    {
        
        Texture2D b = GetInputValue<Texture2D>("Input2");
        if (b != null)
        {
            noise = b;
            byte[] bytes = noise.EncodeToPNG();
            var dirPath = Application.dataPath + "/Editor/Textures/";
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            System.Random prng = new System.Random();
            File.WriteAllBytes(dirPath + "Image" + prng.Next() + ".png", bytes);
        }
        
    }
}
