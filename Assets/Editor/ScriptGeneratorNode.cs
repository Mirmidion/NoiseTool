using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class ScriptGeneratorNode : Node {

	// Use this for initialization
	protected override void Init() {
		base.Init();
		
	}

	// Return the correct value of an output port when requested
	public override object GetValue(NodePort port)
    {
		

        return null;
    }

    public NodeTree<NodeConfig> GenerateConfig()
    {
        NodeTree<NodeConfig> output = new NodeTree<NodeConfig>(new NodeConfig());
        foreach (Node node in this.graph.nodes)
        {
            if (node.GetType() == typeof(NoiseNode))
            {

            }
            else if (node.GetType() == typeof(ProcessNode))
            {

            }
        }



        return output;
    }

    public List<NodeConfig> GetChildren(Node node)
    {
        List<NodeConfig> children = new List<NodeConfig>();
        foreach (NodePort port in node.Inputs)
        {
            Node connectedNode = port.Connection.node;
            children.Add(GetConfig(connectedNode));
        }
    }

    public NodeConfig GetConfig(Node node)
    {
        NodeConfig output = new NodeConfig();
        if (node.GetType() == typeof(NoiseNode))
        {
            NoiseNode noiseNode = node as NoiseNode;
            output.Type = (NodeType) noiseNode.noiseType;
            switch (noiseNode.noiseType)
            {
                case NoiseNode.NoiseType.Random:
                {
                    
                    break;
                }
                case NoiseNode.NoiseType.RawPerlin:
                {
                    output.
                    break;
                }
                case NoiseNode.NoiseType.Perlin:
                {

                    break;
                }
                case NoiseNode.NoiseType.Rigid:
                {

                    break;
                }
                case NoiseNode.NoiseType.Simplex:
                {

                    break;
                }
                case NoiseNode.NoiseType.Voronoi:
                {

                    break;
                }
            }
        }
        else if (node.GetType() == typeof(ProcessNode))
        {
            ProcessNode noiseNode = node as ProcessNode;
            output.Type = (NodeType)(noiseNode.selectedMode+6);
            switch (noiseNode.selectedMode)
            {
                case ProcessNode.mode.Add:
                {

                    break;
                }
                case ProcessNode.mode.Shift:
                {

                    break;
                }
                case ProcessNode.mode.MinMax:
                {

                    break;
                }
                case ProcessNode.mode.Scale:
                {

                    break;
                }
                case ProcessNode.mode.Maximize:
                {

                    break;
                }
                case ProcessNode.mode.Interpolate:
                {

                    break;
                }
            }
        }

        return output;
    }

    public enum NodeType
    {
        Random, 
        RawPerlin, 
        Perlin, 
        Rigid, 
        Simplex, 
        Voronoi,
        Add, 
        Shift, 
        MinMax, 
        Scale, 
        Maximize, 
        Interpolate
    }

    public class NodeTree<T>
    {
        private T data;
        private LinkedList<NodeTree<T>> children;

        public NodeTree(T data)
        {
            this.data = data;
            children = new LinkedList<NodeTree<T>>();
        }

        public void AddChild(T data)
        {
            children.AddFirst(new NodeTree<T>(data));
        }

        public NodeTree<T> GetChild(int i)
        {
            foreach (NodeTree<T> n in children)
                if (--i == 0)
                    return n;
            return null;
        }
    }

    public struct NodeConfig
    {
        public NodeType Type;

    }
}