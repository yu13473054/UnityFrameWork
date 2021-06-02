using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class BoldEx : BaseMeshEffect
{
    [Range(0, 1)] public float Alpha = 0.5f;
    [Range(1, 5)] public int Strength = 1;

    private static readonly Regex s_BoldBeginRegex = new Regex("<b>", RegexOptions.Singleline);
    private static readonly Regex s_BoldEndRegex = new Regex("</b>", RegexOptions.Singleline);

    private MatchCollection begin = null;
    private MatchCollection end = null;

    protected void ApplyShadowZeroAlloc(List<UIVertex> verts, int start, int end, float x, float y)
    {
        int num = verts.Count + end - start;
        if (verts.Capacity < num)
            verts.Capacity = num;
        for (int index = start; index < end; ++index)
        {
            UIVertex vert = verts[index];
            verts.Add(vert);
            Vector3 position = vert.position;
            position.x += x;
            position.y += y;
            vert.position = position;
            Color32 color32 = vert.color;
            color32.a = (byte)((int)color32.a * (int)verts[index].color.a / (int)byte.MaxValue);
            color32.a = (byte)(Alpha * color32.a);
            vert.color = color32;
            verts[index] = vert;
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        List<UIVertex> verts = new List<UIVertex>();
        vh.GetUIVertexStream(verts);

        if (begin != null && begin.Count > 0 && begin.Count == end.Count)
        {
            int offset = 0;
            for (int i = 0; i < begin.Count && i < end.Count; ++i)
            {
                for (int j = 0; j < Strength; ++j)
                {
                    ApplyShadowZeroAlloc(verts, (begin[i].Index - offset) * 6, (end[i].Index - offset - 3) * 6, 0, 0);
                }
                offset += 7;
            }
        }
        else
        {
            for (int i = 0; i < Strength; ++i)
            {
                ApplyShadowZeroAlloc(verts, 0, verts.Count, 0, 0);
            }
        }


        vh.Clear();
        vh.AddUIVertexTriangleStream(verts);
    }

    public bool CheckRichtext(string text)
    {
        begin = s_BoldBeginRegex.Matches(text);
        end = s_BoldEndRegex.Matches(text);
        return begin.Count > 0 && begin.Count == end.Count;
    }

}