using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIText : Text 
{
    private Material _defaultMat;
    private ResModuleUtility _resModuleUtility;
    public string textID = "";
    [SerializeField]
    private bool _multiRes = true;

    [SerializeField]
    private float _colorByGrayFactor = 1;
    private bool _isGray;

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        if (!Localization.isInited)
        {
            Localization.Init();
            Localization.isInited = true;
        }
#endif

        if (textID != "")
        {
            string dataText = Localization.Get(textID);
		    if (string.IsNullOrEmpty(dataText))
		    {
		        Debug.Log("<UIText> 文本错误！",this);
		    }
		    text = dataText;
        }
    }

    protected override void Start()
    {
        base.Start();
        if (Application.isPlaying && _multiRes) //指定了字体
        {
            ResModuleUtility utility = GetComponentInParent<ResModuleUtility>();
            string moduleName;
            if (utility)
                moduleName = utility.moduleName;
            else
            {
                Debug.LogErrorFormat(this, "<UIText> 缺少资源管理器ResModuleUtility, 名称：{0}", name);
                return;
            }
            Font mFont = GetComponentInParent<ResModuleUtility>().LoadFont(font.name, true);
            if(mFont)
                font = mFont;
        }
    }


    // 设置文本，所有Lua中的文字设置，都调用此方法
    public void SetText(string textID, params object[] args)
    {
        if( string.IsNullOrEmpty(textID) )
        {
            Debugger.LogError( "<UIText> textID为空，请检查数据表！" );
            return;
        }

        string result = Localization.Get( textID, args );
        if (string.IsNullOrEmpty(result))
        {
            Debug.Log("<UIText> 文本错误！", this);
        }
        text = result;
    }
    public bool isGray
    {
        set
        {
            if (_isGray == value) return; //防止反复设置
            _isGray = value;
            if (value)
            {
                if (!defaultMaterial) _defaultMat = material;
                if (!_resModuleUtility)
                {
                    _resModuleUtility = GetComponentInParent<ResModuleUtility>();
                    if (!_resModuleUtility)
                    {
                        Debug.LogErrorFormat(this, "<UIImage> 缺少资源管理器ResModuleUtility, 名称：{0}", name);
                        return;
                    }
                }

                material = _resModuleUtility.LoadMaterial("UI_Mat_SpriteGray");
                color = new Color(color.r * _colorByGrayFactor, color.g * _colorByGrayFactor,
                    color.b * _colorByGrayFactor, color.a);
            }
            else
            {
                if (defaultMaterial) material = _defaultMat;
                color = new Color(color.r / _colorByGrayFactor, color.g / _colorByGrayFactor,
                    color.b / _colorByGrayFactor, color.a);
            }
        }
    }


    private readonly UIVertex[] m_TempVerts = new UIVertex[4];
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);

        if (!font)
            return;
        m_DisableFontTextureRebuiltCallback = true;

        BoldEx boldEx = GetComponent<BoldEx>();
        if (boldEx && boldEx.CheckRichtext(text)) //检查是否有富文本标签，如果有，就去掉标签，记录加粗字体的位置
        {
            string resultTxt = text;
            resultTxt = resultTxt.Replace("<b>", "");
            resultTxt = resultTxt.Replace("</b>", "");
            cachedTextGenerator.PopulateWithErrors(resultTxt, GetGenerationSettings(rectTransform.rect.size), gameObject);
        }
        else
        {
            cachedTextGenerator.PopulateWithErrors(text, GetGenerationSettings(rectTransform.rect.size), gameObject);
        }

        IList<UIVertex> verts = this.cachedTextGenerator.verts;
        float num1 = 1f / this.pixelsPerUnit;
        int num2 = verts.Count - 4;
        if (num2 <= 0)
        {
            toFill.Clear();
        }
        else
        {
            Vector2 point = new Vector2(verts[0].position.x, verts[0].position.y) * num1;
            Vector2 vector2 = this.PixelAdjustPoint(point) - point;
            toFill.Clear();
            if (vector2 != Vector2.zero)
            {
                for (int index1 = 0; index1 < num2; ++index1)
                {
                    int index2 = index1 & 3;
                    this.m_TempVerts[index2] = verts[index1];
                    this.m_TempVerts[index2].position *= num1;
                    this.m_TempVerts[index2].position.x += vector2.x;
                    this.m_TempVerts[index2].position.y += vector2.y;
                    if (index2 == 3)
                        toFill.AddUIVertexQuad(this.m_TempVerts);
                }
            }
            else
            {
                for (int index1 = 0; index1 < num2; ++index1)
                {
                    int index2 = index1 & 3;
                    this.m_TempVerts[index2] = verts[index1];
                    this.m_TempVerts[index2].position *= num1;
                    if (index2 == 3)
                        toFill.AddUIVertexQuad(this.m_TempVerts);
                }
            }

            this.m_DisableFontTextureRebuiltCallback = false;

        }
    }
}
