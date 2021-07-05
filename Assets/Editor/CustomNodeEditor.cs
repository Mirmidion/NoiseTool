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
               
                CreateFloatField(node, "zOrg", "Z Origin");
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
                
                CreateIntField(node, "seed", "seed", 1,20);
                CreateFloatField(node, "amplitude", "amplitude",1f, 100f);
                CreateFloatField(node, "frequency", "frequency");

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
                CreateFloatField(node, "offset", "offset",0f, 1f);

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

    public void CreateFloatField(Node node, string fieldName, string diplayName, float min = -1000f, float max = 1000f)
    {
        float preValue = (float) node.GetType().GetField(fieldName).GetValue(node);
        float tempOffset = EditorGUILayout.FloatField(fieldName.Substring(0,1).ToUpper() + fieldName.Substring(1), preValue);
        if (tempOffset != (float)node.GetType().GetField(fieldName).GetValue(node))
        {
            node.TriggerOnValidate();
        }
        node.GetType().GetField(fieldName).SetValue(node, Mathf.Min(Mathf.Max(tempOffset, min), max));
    }

    public void CreateIntField(Node node, string fieldName, string diplayName, int min = -1000, int max = 1000)
    {
        int preValue = (int)node.GetType().GetField(fieldName).GetValue(node);
        int tempOffset = EditorGUILayout.IntField(fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1), preValue);
        if (tempOffset != (int)node.GetType().GetField(fieldName).GetValue(node))
        {
            node.TriggerOnValidate();
        }
        node.GetType().GetField(fieldName).SetValue(node, Mathf.Min(Mathf.Max(tempOffset, min), max));
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
