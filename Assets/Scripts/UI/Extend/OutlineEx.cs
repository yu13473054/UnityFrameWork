using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/OutlineEx")]
public class OutlineEx : BaseMeshEffect
{
    public Color OutlineColor = Color.black;
    [Range(0.1f, 6)]
    public float OutlineWidth = 1;

    private List<UIVertex> _vetexList = new List<UIVertex>();
    Dictionary<UIVertex, int> _dic = new Dictionary<UIVertex, int>();

    protected override void Awake()
    {
        base.Awake();

        //设置材质球
        string uiResMoudle = "";
        UIMod uiSystem = GetComponentInParent<UIMod>();
        if (uiSystem)
            uiResMoudle = uiSystem.resModule;
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            base.graphic.material = ResMgr.Inst.LoadMaterial("UI_OutlineEx", uiResMoudle);
        }
        else
        {
            base.graphic.material = new Material(Shader.Find("UI/OutlineEx"));
        }
#else
        base.graphic.material = ResMgr.Inst.LoadMaterial("UI_Mat_SpriteGray", uiResMoudle);
#endif
    }

    protected override void Start()
    {
        //UIVertex的非必须成员的数据默认不会被传递进Shader。修改additionalShaderChannels，让tangent传入Shader。
        var v1 = base.graphic.canvas.additionalShaderChannels;
        var v2 = AdditionalCanvasShaderChannels.Tangent;
        if ((v1 & v2) != v2)
        {
            base.graphic.canvas.additionalShaderChannels |= v2;
        }
        Refresh();
    }


#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (base.graphic.material != null)
        {
            Refresh();
        }
    }
#endif


    private void Refresh()
    {
        base.graphic.material.SetColor("_OutlineColor", OutlineColor);
        base.graphic.material.SetFloat("_OutlineWidth", OutlineWidth);
        base.graphic.SetVerticesDirty();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        _vetexList.Clear();
        vh.GetUIVertexStream(_vetexList);
        vh.Clear();

        for (int i = 0, count = _vetexList.Count - 3; i <= count; i += 3)
        {
            var v1 = _vetexList[i];
            var v2 = _vetexList[i + 1];
            var v3 = _vetexList[i + 2];
            // 计算原顶点坐标中心点
            var minX = Min(v1.position.x, v2.position.x, v3.position.x);
            var minY = Min(v1.position.y, v2.position.y, v3.position.y);
            var maxX = Max(v1.position.x, v2.position.x, v3.position.x);
            var maxY = Max(v1.position.y, v2.position.y, v3.position.y);
            var posCenter = new Vector2(minX + maxX, minY + maxY) * 0.5f;
            // 计算原始顶点坐标和UV的方向
            Vector2 triX, triY, uvX, uvY;
            Vector2 pos1 = v1.position;
            Vector2 pos2 = v2.position;
            Vector2 pos3 = v3.position;
            //找出与x轴夹角最小的一边作为X轴
            if (Mathf.Abs(Vector2.Dot((pos2 - pos1).normalized, Vector2.right))
                > Mathf.Abs(Vector2.Dot((pos3 - pos2).normalized, Vector2.right)))
            {
                triX = pos2 - pos1;
                triY = pos3 - pos2;
                uvX = v2.uv0 - v1.uv0;
                uvY = v3.uv0 - v2.uv0;
            }
            else
            {
                triX = pos3 - pos2;
                triY = pos2 - pos1;
                uvX = v3.uv0 - v2.uv0;
                uvY = v2.uv0 - v1.uv0;
            }
            // 计算原始UV框
            var uvMin = Min(v1.uv0, v2.uv0, v3.uv0);
            var uvMax = Max(v1.uv0, v2.uv0, v3.uv0);
            var uvOrigin = new Vector4(uvMin.x, uvMin.y, uvMax.x, uvMax.y);
            // 为每个顶点设置新的Position和UV，并传入原始UV框
            v1 = SetNewPosAndUV(v1, OutlineWidth, posCenter, triX, triY, uvX, uvY, uvOrigin);
            v2 = SetNewPosAndUV(v2, OutlineWidth, posCenter, triX, triY, uvX, uvY, uvOrigin);
            v3 = SetNewPosAndUV(v3, OutlineWidth, posCenter, triX, triY, uvX, uvY, uvOrigin);

            // 应用设置后的UIVertex
            int id0, id1, id2;
            if (!_dic.ContainsKey(v1))
            {
                id0 = vh.currentVertCount;
                _dic.Add(v1, vh.currentVertCount);
                vh.AddVert(v1);
            }
            else
            {
                id0 = _dic[v1];
            }
            if (!_dic.ContainsKey(v2))
            {
                id1 = vh.currentVertCount;
                _dic.Add(v2, vh.currentVertCount);
                vh.AddVert(v2);
            }
            else
            {
                id1 = _dic[v2];
            }
            if (!_dic.ContainsKey(v3))
            {
                id2 = vh.currentVertCount;
                _dic.Add(v3, vh.currentVertCount);
                vh.AddVert(v3);
            }
            else
            {
                id2 = _dic[v3];
            }
            vh.AddTriangle(id0, id1, id2);
        }
        _dic.Clear();
    }

    private static UIVertex SetNewPosAndUV(UIVertex pVertex, float pOutLineWidth,
        Vector2 pPosCenter,
        Vector2 pTriangleX, Vector2 pTriangleY,
        Vector2 pUVX, Vector2 pUVY,
        Vector4 pUVOrigin)
    {
        // 扩大pos
        var pos = pVertex.position;
        var posXOffset = pos.x > pPosCenter.x ? pOutLineWidth : -pOutLineWidth;
        var posYOffset = pos.y > pPosCenter.y ? pOutLineWidth : -pOutLineWidth;
        pos.x += posXOffset;
        pos.y += posYOffset;
        pVertex.position = pos;
        // 扩大UV
        var uv = pVertex.uv0;
        uv += pUVX / pTriangleX.magnitude * posXOffset * (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1);
        uv += pUVY / pTriangleY.magnitude * posYOffset * (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1);
        pVertex.uv0 = uv;
        // 原始UV框
        pVertex.tangent = pUVOrigin;

        return pVertex;
    }


    private float Min(float pA, float pB, float pC)
    {
        return Mathf.Min(Mathf.Min(pA, pB), pC);
    }


    private float Max(float pA, float pB, float pC)
    {
        return Mathf.Max(Mathf.Max(pA, pB), pC);
    }


    private Vector2 Min(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(Min(pA.x, pB.x, pC.x), Min(pA.y, pB.y, pC.y));
    }


    private Vector2 Max(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(Max(pA.x, pB.x, pC.x), Max(pA.y, pB.y, pC.y));
    }
}
