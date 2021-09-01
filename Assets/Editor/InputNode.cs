using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class InputNode : Node
{
    public Vector3 input;
    [Output] public Vector3 point;

    public override object GetValue(NodePort port)
    {
        if (port.fieldName.Equals("point"))
        {
            return input;
        }
        return null;
    }
}
