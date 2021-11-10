using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeEditor(typeof(Node))]
public class CustomNodeEditor : NodeEditor
{
    private NoiseNode simpleNode;
    private List<NodePort> inputList = new List<NodePort>();

    public override void OnBodyGUI()
    {
        base.OnBodyGUI();

        Node baseNode = target as Node;
        if (baseNode.GetType() == typeof(NoiseNode)) {
            NoiseNode node = (NoiseNode) baseNode;
            if (node.noiseType == NoiseNode.NoiseType.Perlin)
            {
                BasePerlinAttributes(node);

                CreateIntField(node, "seed", "seed", 1, 20);
                CreateIntField(node, "octaves", "octaves", 1, 20);
                CreateFloatField(node, "persistance", "persistance", 0.6f, 0.9f);
                CreateFloatField(node, "lacunarity", "lacunarity", 0f, 100f);
                CreateFloatField(node, "amplitude", "amplitude", 0f, 100f);
                CreateFloatField(node, "frequency", "frequency", 0f, 100f);

                NoisePreview(node, new Vector2(30, 330));
            }
            else if (node.noiseType == NoiseNode.NoiseType.Random)
            {
                if (GUILayout.Button("Generate"))
                {
                    node.TriggerOnValidate();
                }
                NoisePreview(node, new Vector2(30, 165));
            }
            else if (node.noiseType == NoiseNode.NoiseType.RawPerlin)
            {

                BasePerlinAttributes(node);
                NoisePreview(node, new Vector2(30, 210));

            }
            else if (node.noiseType == NoiseNode.NoiseType.Simplex)
            {
               
                //CreateFloatField(node, "zOrg", "Z Origin");
                
                BasePerlinAttributes(node);
                
                NoisePreview(node, new Vector2(30, 210));

            }
            else if (node.noiseType == NoiseNode.NoiseType.Rigid)
            {
                BasePerlinAttributes(node);
                CreateFloatField(node, "minValue", "minValue");
                CreateFloatField(node, "roughness", "roughness");
                CreateFloatField(node, "baseRoughness", "baseRoughness");
                CreateFloatField(node, "strength", "strength");
                CreateFloatField(node, "weightMultiplier", "weightMultiplier");
                CreateFloatField(node, "persistance", "persistance");
                CreateIntField(node, "numLayers", "numLayers", 1, 8);

                NoisePreview(node, new Vector2(30, 350));
            }
            else if (node.noiseType == NoiseNode.NoiseType.Voronoi)
            {
                
                VoronoiNoise.VORONOI_COMBINATION tempEnum = (VoronoiNoise.VORONOI_COMBINATION) EditorGUILayout.EnumPopup(node.combinationMode);
                if (!tempEnum.Equals(node.combinationMode))
                {
                    node.combinationMode = tempEnum;
                    Revalidate(node);
                }
                

                VoronoiNoise.VORONOI_DISTANCE tempEnum2 = (VoronoiNoise.VORONOI_DISTANCE)EditorGUILayout.EnumPopup(node.distanceMode);
                if (!tempEnum2.Equals(node.distanceMode))
                {
                    node.distanceMode = tempEnum2;
                    Revalidate(node);
                }
                
                CreateIntField(node, "seed", "seed", 1,20);
                CreateFloatField(node, "amplitude", "amplitude",1f, 100f);
                CreateFloatField(node, "frequency", "frequency");

                NoisePreview(node, new Vector2(30, 250));

            }
            if(GUILayout.Button("Regenerate branch"))
            {
                node.RegenerateBranch();
            }
        }
        
        else if (baseNode.GetType() == typeof(ProcessNode))
        {
            ProcessNode node = (ProcessNode) baseNode;
            
            if (node.selectedMode == ProcessNode.mode.Add)
            {
                int weight1Min = 0;
                int weight1Max = 100;
                int tempWeight1 = EditorGUILayout.IntField("Noise 1 Weight", node.noise1Weight);
                if (tempWeight1 != node.noise1Weight)
                {
                    Revalidate(node);
                }


                int weight2Min = 0;
                int weight2Max = 100;
                int tempWeight2 = EditorGUILayout.IntField("Noise 2 Weight", node.noise2Weight);
                if (tempWeight2 != node.noise2Weight)
                {
                    Revalidate(node);
                }

                if (tempWeight1 + tempWeight2 != 100)
                {
                    if (tempWeight2 != node.noise2Weight)
                    {
                        tempWeight1 = 100 - tempWeight2;
                    }
                    else if (tempWeight1 != node.noise1Weight)
                    {
                        tempWeight2 = 100 - tempWeight1;
                    }
                }

                node.noise1Weight = Mathf.Min(Mathf.Max(tempWeight1, weight1Min), weight1Max);
                node.noise2Weight = Mathf.Min(Mathf.Max(tempWeight2, weight2Min), weight2Max);

                if (node.noise != null)
                {
                    for (int spaces = 0; spaces < 30; spaces++)
                    {
                        EditorGUILayout.Space();
                    }
                    EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 230), new Vector2(150, 150)), node.noise);
                }
            }
            else if (node.selectedMode == ProcessNode.mode.Shift)
            {
                CreateFloatField(node, "offset", "offset",0f, 1f);

                if (node.noise != null)
                {
                    for (int spaces = 0; spaces < 30; spaces++)
                    {
                        EditorGUILayout.Space();
                    }
                    EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 190), new Vector2(150, 150)), node.noise);
                }
            }
            else if (node.selectedMode == ProcessNode.mode.MinMax)
            {
                CreateFloatField(node, "min", "min",0f,1f);
                CreateFloatField(node, "max", "max", 0f, 1f);

                if (node.noise != null)
                {
                    for (int spaces = 0; spaces < 30; spaces++)
                    {
                        EditorGUILayout.Space();
                    }
                    EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 210), new Vector2(150, 150)), node.noise);
                }
            }
            else if (node.selectedMode == ProcessNode.mode.Scale)
            {
                CreateFloatField(node, "scaleFirstHalf", "scaleFirstHalf", 0f, 2f);
                CreateFloatField(node, "scaleSecondHalf", "scaleSecondHalf", 0f, 2f);

                if (GUILayout.Button("Reset"))
                {
                    node.scaleFirstHalf = 1f;
                    node.scaleSecondHalf = 1f;
                    Revalidate(node);
                }

                if (node.noise != null)
                {
                    for (int spaces = 0; spaces < 30; spaces++)
                    {
                        EditorGUILayout.Space();
                    }
                    EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 230), new Vector2(150, 150)), node.noise);
                }
            }
            else if (node.selectedMode == ProcessNode.mode.Maximize)
            {
                if (node.noise != null)
                {
                    for (int spaces = 0; spaces < 30; spaces++)
                    {
                        EditorGUILayout.Space();
                    }
                    EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 170), new Vector2(150, 150)), node.noise);
                }
            }
            else if (node.selectedMode == ProcessNode.mode.Interpolate)
            {
                    if (node.noise != null)
                    {
                        for (int spaces = 0; spaces < 30; spaces++)
                        {
                            EditorGUILayout.Space();
                        }
                        EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 170), new Vector2(150, 150)), node.noise);
                    }
            }
            //else if (node.selectedMode == AddNode.mode.Multiply)
            //{
            //    CreateIntField(node, "size", "size", 0,10);
            //    AddNode.Multiplier[] preValue = (AddNode.Multiplier[])node.GetType().GetField("multipliers").GetValue(node);
            //    AddNode.Multiplier[] outputValue = new AddNode.Multiplier[(int)node.GetType().GetField("size").GetValue(node)];
            //    int counter = 0;
            //    if (preValue != null)
            //    {
            //        foreach (AddNode.Multiplier multiplier in preValue)
            //        {
            //            if (counter == outputValue.Length)
            //            {
            //                break;
            //            }
            //            outputValue[counter] = new AddNode.Multiplier();
            //            if (multiplier != null)
            //            {
            //                outputValue[counter].multiplier = multiplier.multiplier;
            //                outputValue[counter].from = multiplier.from;
            //                outputValue[counter].to = multiplier.to;
            //            }
            //            EditorGUILayout.LabelField("Multiplier " + (counter+1));
            //            outputValue[counter].multiplier = EditorGUILayout.FloatField("Multiplier:", outputValue[counter].multiplier);
            //            outputValue[counter].from = EditorGUILayout.FloatField("From:",outputValue[counter].from);
            //            outputValue[counter].to = EditorGUILayout.FloatField("To:", outputValue[counter].to);
            //            counter++;
            //        }
            //    }
            //    if (outputValue != (AddNode.Multiplier[])node.GetType().GetField("multipliers").GetValue(node))
            //    {
            //        Revalidate(node);
            //    }
            //    node.GetType().GetField("multipliers").SetValue(node, outputValue);

            //    if (node.noise != null)
            //    {
            //        for (int spaces = 0; spaces < 30; spaces++)
            //        {
            //            EditorGUILayout.Space();
            //        }
            //        EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 175+(counter*45)), new Vector2(150, 150)), node.noise);
            //    }
            //}
            AddDynamicPorts(node);
            if (GUILayout.Button("Regenerate branch"))
            {
                node.RegenerateBranch();
            }
        }
        else if (baseNode.GetType() == typeof(SerializeNode))
        {
            SerializeNode node = (SerializeNode)baseNode;
            if (GUILayout.Button("Serialize"))
            {
                node.Serialize();
            }
            if (node.noise != null)
            {
                for (int spaces = 0; spaces < 30; spaces++)
                {
                    EditorGUILayout.Space();
                }
                EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 95), new Vector2(150, 150)), node.noise);
            }
        }
        else if (baseNode.GetType() == typeof(OutputNode))
        {
            OutputNode node = (OutputNode)baseNode;
            if (GUILayout.Button("Get Point"))
            {
                node.GetPoint();
            }
        }
    }

    public void SinglePointInput(Node node, bool single, bool dual)
    {
        if (!single && !node.HasPort("Input Port"))
        {
            NodePort temp = node.AddDynamicInput(typeof(Vector4[]), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Input Port");
            
            inputList.Add(temp);
        }
        if (dual)
        {
            node.RemoveDynamicPort("Input Port 1");
            int index = -1;
            foreach (NodePort i in inputList)
            {
                if (i.fieldName.Equals("Input Port 1"))
                {
                    index = inputList.IndexOf(i);
                }
            }
            if (index != -1)
            {
                inputList.RemoveAt(index);
            }

            node.RemoveDynamicPort("Input Port 2");
            index = -1;
            foreach (NodePort i in inputList)
            {
                if (i.fieldName.Equals("Input Port 2"))
                {
                    index = inputList.IndexOf(i);
                }
            }
            if (index != -1)
            {
                inputList.RemoveAt(index);
            }
        }
    }

    public void DoublePointInput(Node node, bool single, bool first, bool second)
    {
        if (single)
        {
            node.RemoveDynamicPort("Input Port");
            int index = -1;
            foreach (NodePort i in inputList)
            {
                if (i.fieldName.Equals("Input Port"))
                {
                    index = inputList.IndexOf(i);
                }
            }
            if (index != -1)
            {
                inputList.RemoveAt(index);
            }
        }
        if (!first && !node.HasPort("Input Port 1") )
        {
            NodePort temp = node.AddDynamicInput(typeof(Vector4[]), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Input Port 1");
            inputList.Add(temp);
        }
        if (!second &&  !node.HasPort("Input Port 2"))
        {
            NodePort temp1 = node.AddDynamicInput(typeof(Vector4[]), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Input Port 2");
            inputList.Add(temp1);
        }
    }

    public void SingleNoiseInput(Node node, bool noise, bool noise1, bool noise2)
    {
        if (!noise && !node.HasPort("Noise"))
        {
            NodePort temp = node.AddDynamicInput(typeof(Texture2D), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Noise");
            inputList.Add(temp);
        }
        if (noise1)
        {
            node.RemoveDynamicPort("Noise1");
            int index = -1;
            foreach (NodePort i in inputList)
            {
                if (i.fieldName.Equals("Noise1"))
                {
                    index = inputList.IndexOf(i);
                }
            }
            if (index != -1)
            {
                inputList.RemoveAt(index);
            }
        }
        else if (noise2)
        {
            node.RemoveDynamicPort("Noise2");
            int index = -1;
            foreach (NodePort i in inputList)
            {
                if (i.fieldName.Equals("Noise2"))
                {
                    index = inputList.IndexOf(i);
                }
            }
            if (index != -1)
            {
                inputList.RemoveAt(index);
            }
        }
    }

    public void TwoNoiseInputs(Node node, bool noise, bool noise1, bool noise2)
    {
        if (noise)
        {
            node.RemoveDynamicPort("Noise");
            int index = -1;
            foreach (NodePort i in inputList)
            {
                if (i.fieldName.Equals("Noise"))
                {
                    index = inputList.IndexOf(i);
                }
            }
            if (index != -1)
            {
                inputList.RemoveAt(index);
            }
        }
        if (!noise1 && !node.HasPort("Noise1"))
        {
            NodePort temp = node.AddDynamicInput(typeof(Texture2D), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Noise1");
            inputList.Add(temp);
        }
        if (!noise2 && !node.HasPort("Noise2"))
        {
            NodePort temp = node.AddDynamicInput(typeof(Texture2D), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Noise2");
            inputList.Add(temp);
        }
    }

    public void CreateFloatField(Node node, string fieldName, string diplayName, float min = -1000f, float max = 1000f)
    {
        float preValue = (float) node.GetType().GetField(fieldName).GetValue(node);
        float tempOffset = EditorGUILayout.FloatField(fieldName.Substring(0,1).ToUpper() + fieldName.Substring(1), preValue);
        if (tempOffset != (float)node.GetType().GetField(fieldName).GetValue(node))
        {
            Revalidate(node);
        }
        node.GetType().GetField(fieldName).SetValue(node, Mathf.Min(Mathf.Max(tempOffset, min), max));
    }

    public void CreateIntField(Node node, string fieldName, string diplayName, int min = -1000, int max = 1000)
    {
        int preValue = (int)node.GetType().GetField(fieldName).GetValue(node);
        int tempOffset = EditorGUILayout.IntField(fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1), preValue);
        if (tempOffset != (int)node.GetType().GetField(fieldName).GetValue(node))
        {
            Revalidate(node);
        }
        node.GetType().GetField(fieldName).SetValue(node, Mathf.Min(Mathf.Max(tempOffset, min), max));
    }

    void AddDynamicPorts(ProcessNode node)
    {
        
        bool noise = false;
        bool noise1 = false;
        bool noise2 = false;
        bool input = false;
        bool input1 = false;
        bool input2 = false;
        foreach (NodePort port in inputList)
        {
            switch (port.fieldName)
            {
                case "Noise":
                    {
                        noise = true;
                        break;
                    }
                case "Noise1":
                    {
                        noise1 = true;
                        break;
                    }
                case "Noise2":
                    {
                        noise2 = true;
                        break;
                    }
                case "Input Port":
                {
                    input = true;
                    break;
                }
                case "Input Port 1":
                {
                    input1 = true;
                    break;
                }
                case "Input Port 2":
                {
                    input2 = true;
                    break;
                }
            }
        }

        switch (node.selectedMode)
        {
            case ProcessNode.mode.Add:
                {
                    TwoNoiseInputs(node, noise,noise1,noise2);
                    DoublePointInput(node, input, input1, input2);
                    break;
                }
            case ProcessNode.mode.Shift:
                {
                    SingleNoiseInput(node, noise, noise1, noise2);
                    SinglePointInput(node, input, input1 && input2);
                    break;
                }
            case ProcessNode.mode.MinMax:
                {
                    SingleNoiseInput(node, noise, noise1, noise2);
                    SinglePointInput(node, input, input1 && input2);
                    break;
                }
            case ProcessNode.mode.Scale:
                {
                    SingleNoiseInput(node, noise, noise1, noise2);
                    SinglePointInput(node, input, input1 && input2);
                    break;
                }
            case ProcessNode.mode.Maximize:
                {
                    SingleNoiseInput(node, noise, noise1, noise2);
                    SinglePointInput(node, input, input1 && input2);
                    break;
                }
            case ProcessNode.mode.Interpolate:
                {
                    SingleNoiseInput(node, noise, noise1, noise2);
                    SinglePointInput(node, input, input1 && input2);
                    break;
                }
                //case AddNode.mode.Multiply:
                //    {
                //        if (!noise && !node.HasPort("Noise"))
                //        {
                //            NodePort temp = node.AddDynamicInput(typeof(Color[]), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Noise");
                //            inputList.Add(temp);
                //        }
                //        if (noise1)
                //        {
                //            node.RemoveDynamicPort("Noise1");
                //            int index = -1;
                //            foreach (NodePort i in inputList)
                //            {
                //                if (i.fieldName.Equals("Noise1"))
                //                {
                //                    index = inputList.IndexOf(i);
                //                }
                //            }
                //            if (index != -1)
                //            {
                //                inputList.RemoveAt(index);
                //            }
                //        }
                //        else if (noise2)
                //        {
                //            node.RemoveDynamicPort("Noise2");
                //            int index = -1;
                //            foreach (NodePort i in inputList)
                //            {
                //                if (i.fieldName.Equals("Noise2"))
                //                {
                //                    index = inputList.IndexOf(i);
                //                }
                //            }
                //            if (index != -1)
                //            {
                //                inputList.RemoveAt(index);
                //            }
                //        }
                //        break;
                //    }
        }
       
        
    }

    public override void OnRename()
    {
        base.OnRename();
        NoiseNode node = target as NoiseNode;
        if (node.noiseType == NoiseNode.NoiseType.Perlin)
        {
            node.octaves = EditorGUILayout.IntSlider("Octaves", node.octaves, 1, 100);
        }
        else if (node.noiseType == NoiseNode.NoiseType.Random)
        {

        }
        else if (node.noiseType == NoiseNode.NoiseType.RawPerlin)
        {

        }
    }

    public void BasePerlinAttributes(NoiseNode node)
    {
        
        float tempXOffset = EditorGUILayout.FloatField("Offset X", node.xOrg);
        if (tempXOffset != node.xOrg)
        {
            node.TriggerOnValidate();
        }
        node.xOrg = tempXOffset;

        
        float tempYOffset = EditorGUILayout.FloatField("Offset Y", node.yOrg);
        if (tempYOffset != node.yOrg)
        {
            node.TriggerOnValidate();
        }
        node.yOrg = tempYOffset;

       
        float tempScale = EditorGUILayout.FloatField("Scale", node.scale);
        if (tempScale != node.scale)
        {
            node.TriggerOnValidate();
        }
        node.scale = tempScale;
    }

    public void NoisePreview(NoiseNode node, Vector2 position)
    {
        if (node.noise != null)
        {
            for (int spaces = 0; spaces < 30; spaces++)
            {
                EditorGUILayout.Space();
            }
            EditorGUI.DrawPreviewTexture(new Rect(position, new Vector2(150, 150)), node.noise);
        }
    }

    public void Revalidate(Node node)
    {
        node.TriggerOnValidate();
        foreach (NodePort child in node.DynamicOutputs)
        {
            
        }
    }
}
