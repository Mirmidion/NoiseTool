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

                int seedMin = 1;
                int seedMax = 20;
                int tempSeed = EditorGUILayout.IntField("Seed", node.seed);
                if (tempSeed != node.seed)
                {
                    node.TriggerOnValidate();
                }
                node.seed = Mathf.Min(Mathf.Max(tempSeed, seedMin), seedMax);

                int octaveMin = 1;
                int octaveMax = 20;
                int tempOctaves = EditorGUILayout.IntField("Octaves", node.octaves);
                if (tempOctaves != node.octaves)
                {
                    node.TriggerOnValidate();
                }
                node.octaves = Mathf.Min(Mathf.Max(tempOctaves, octaveMin), octaveMax);

                float persistanceMin = 0.6f;
                float persistanceMax = 0.9f;
                float tempPersistance = EditorGUILayout.FloatField("Persistance", node.persistance);
                if (tempPersistance != node.persistance)
                {
                    node.TriggerOnValidate();
                }
                node.persistance = Mathf.Min(Mathf.Max(tempPersistance, persistanceMin), persistanceMax);

                float lacunarityMin = 0f;
                float lacunarityMax = 100f;
                float tempLacunarity = EditorGUILayout.FloatField("Lacunarity", node.lacunarity);
                if (tempLacunarity != node.lacunarity)
                {
                    node.TriggerOnValidate();
                }
                node.lacunarity = Mathf.Min(Mathf.Max(tempLacunarity, lacunarityMin), lacunarityMax);

                float amplitudeMin = 0f;
                float amplitudeMax = 100f;
                float tempAmplitude = EditorGUILayout.FloatField("Amplitude", node.amplitude);
                if (tempAmplitude != node.amplitude)
                {
                    node.TriggerOnValidate();
                }
                node.amplitude = Mathf.Min(Mathf.Max(tempAmplitude, amplitudeMin), amplitudeMax);

                float frequencyMin = 0f;
                float frequencyMax = 100f;
                float tempFrequency = EditorGUILayout.FloatField("Frequency", node.frequency);
                if (tempFrequency != node.frequency)
                {
                    node.TriggerOnValidate();
                }
                node.frequency = Mathf.Min(Mathf.Max(tempFrequency, frequencyMin), frequencyMax);




                NoisePreview(node, new Vector2(30, 275));


            }
            else if (node.noiseType == NoiseNode.NoiseType.Random)
            {
                if (GUILayout.Button("Generate"))
                {
                    node.TriggerOnValidate();
                }
                NoisePreview(node, new Vector2(30, 115));
            }
            else if (node.noiseType == NoiseNode.NoiseType.RawPerlin)
            {

                BasePerlinAttributes(node);
                NoisePreview(node, new Vector2(30, 155));

            }
            else if (node.noiseType == NoiseNode.NoiseType.Simplex)
            {
                float tempzOffset = EditorGUILayout.FloatField("Offset Z", node.zOrg);
                if (tempzOffset != node.zOrg)
                {
                    node.TriggerOnValidate();
                }
                node.zOrg = tempzOffset;
                BasePerlinAttributes(node);
                
                NoisePreview(node, new Vector2(30, 155));

            }
            else if (node.noiseType == NoiseNode.NoiseType.Voronoi)
            {
                
                VoronoiNoise.VORONOI_COMBINATION tempEnum = (VoronoiNoise.VORONOI_COMBINATION) EditorGUILayout.EnumPopup(node.combinationMode);
                if (!tempEnum.Equals(node.combinationMode))
                {
                    node.combinationMode = tempEnum;
                    node.TriggerOnValidate();
                }
                

                VoronoiNoise.VORONOI_DISTANCE tempEnum2 = (VoronoiNoise.VORONOI_DISTANCE)EditorGUILayout.EnumPopup(node.distanceMode);
                if (!tempEnum2.Equals(node.distanceMode))
                {
                    node.distanceMode = tempEnum2;
                    node.TriggerOnValidate();
                }
                

                int seedMin = 1;
                int seedMax = 20;
                int tempSeed = EditorGUILayout.IntField("Seed", node.seed);
                if (tempSeed != node.seed)
                {
                    node.TriggerOnValidate();
                }
                node.seed = Mathf.Min(Mathf.Max(tempSeed, seedMin), seedMax);

                float amplitudeMin = 0f;
                float amplitudeMax = 100f;
                float tempAmplitude = EditorGUILayout.FloatField("Amplitude", node.amplitude);
                if (tempAmplitude != node.amplitude)
                {
                    node.TriggerOnValidate();
                }
                node.amplitude = Mathf.Min(Mathf.Max(tempAmplitude, amplitudeMin), amplitudeMax);

                float frequencyMin = 0f;
                float frequencyMax = 100f;
                float tempFrequency = EditorGUILayout.FloatField("Frequency", node.frequency);
                if (tempFrequency != node.frequency)
                {
                    node.TriggerOnValidate();
                }
                node.frequency = Mathf.Min(Mathf.Max(tempFrequency, frequencyMin), frequencyMax);


                NoisePreview(node, new Vector2(30, 195));

            }
        }
        else if (baseNode.GetType() == typeof(AddNode))
        {
            AddNode node = (AddNode) baseNode;
            
            if (node.selectedMode == AddNode.mode.Add)
            {
                int weight1Min = 0;
                int weight1Max = 100;
                int tempWeight1 = EditorGUILayout.IntField("Noise 1 Weight", node.noise1Weight);
                if (tempWeight1 != node.noise1Weight)
                {
                    node.TriggerOnValidate();
                }


                int weight2Min = 0;
                int weight2Max = 100;
                int tempWeight2 = EditorGUILayout.IntField("Noise 2 Weight", node.noise2Weight);
                if (tempWeight2 != node.noise2Weight)
                {
                    node.TriggerOnValidate();
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
                    EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 155), new Vector2(150, 150)), node.noise);
                }
            }
            else if (node.selectedMode == AddNode.mode.Shift)
            {
                float offsetMin = 0f;
                float offsetMax = 1f;
                float tempOffset = EditorGUILayout.FloatField("Offset", node.offset);
                if (tempOffset != node.offset)
                {
                    node.TriggerOnValidate();
                }
                node.offset = Mathf.Min(Mathf.Max(tempOffset, offsetMin), offsetMax);

                if (node.noise != null)
                {
                    for (int spaces = 0; spaces < 30; spaces++)
                    {
                        EditorGUILayout.Space();
                    }
                    EditorGUI.DrawPreviewTexture(new Rect(new Vector2(30, 155), new Vector2(150, 150)), node.noise);
                }
            }
            AddDynamicPorts(node);
        }
    }

    public void CreateFloatField(AddNode node, string fieldName, float min, float max)
    {
        float preValue = (float) node.GetType().GetProperty(fieldName).GetValue(node);
        float tempOffset = EditorGUILayout.FloatField("Offset", preValue);
        if (tempOffset != (float) node.GetType().GetProperty(fieldName).GetValue(node))
        {
            node.TriggerOnValidate();
        }
        node.GetType().GetProperty(fieldName).SetValue(node, Mathf.Min(Mathf.Max(tempOffset, min), max));
    }

   void AddDynamicPorts(AddNode node)
    {
        
        
        bool noise = false;
        bool noise1 = false;
        bool noise2 = false;
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
            }
        }

        switch (node.selectedMode)
        {
            case AddNode.mode.Add:
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
                    if (!noise1)
                    {
                        NodePort temp = node.AddDynamicInput(typeof(Color[]), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Noise1");
                        inputList.Add(temp);
                    }
                    if (!noise2)
                    {
                        NodePort temp = node.AddDynamicInput(typeof(Color[]), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Noise2");
                        inputList.Add(temp);
                    }
                    break;
                }
            case AddNode.mode.Shift:
                {
                    if (!noise)
                    {
                        NodePort temp = node.AddDynamicInput(typeof(Color[]), Node.ConnectionType.Multiple, Node.TypeConstraint.None, "Noise");
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
                    if (noise2)
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
                    break;
                }
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
}
