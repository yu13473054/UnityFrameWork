using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIRaycast : MaskableGraphic
{
    public bool fillColor = false;
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (!fillColor)
            toFill.Clear();
    }
}