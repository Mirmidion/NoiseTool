using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class OutputNode : Node
{
    public Vector4 output;
    [Input] public Vector4[] point;

    public Vector4 GetPoint()
    {
        
            if (GetInputValue<Vector4[]>("point") != null)
            {
                Vector4[] toInterpolate = GetInputValue<Vector4[]>("point");
                float value = 0f;
                foreach (Vector4 p in toInterpolate)
                {
                    value += p.w;
                }

                output = toInterpolate[0];
                output.w = value / 7f;
                return output;
            }
            return Vector4.zero;
        
    }
}
