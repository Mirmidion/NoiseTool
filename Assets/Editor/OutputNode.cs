using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class OutputNode : Node
{
    public Vector4[] output;
    [Input] public Vector4[] point;

    public Vector3 GetPoint()
    {
        
            if (GetInputValue<Vector4[]>("point") != null)
            {
                output = GetInputValue<Vector4[]>("point");
                return output[0];
            }
            return Vector4.zero;
        
    }
}
