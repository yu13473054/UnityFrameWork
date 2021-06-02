using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using UnityEngine.EventSystems;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
/// <summary>
/// underline <material=underline c=#ffffff h=1 f=1 p=***>blablabla</material>
/// 图片 <quad img=资源名 size=20 hs=1 f=1 p=***/>
/// 
/// 注意：
/// 1，在点击apply之前，需要检查Text下是否有预览生成的Image，建议手动删除，或者只保留预使用的个数
/// 2，Text下，不能放置其他的image控件
/// </summary>
public class UIRichText : UIText, IPointerClickHandler
{
    class ImageInfo
    {
        public string name;       //名字(路径)
        public Vector2 size;      //宽高
        public Vector2 position;  //位置
        public Color color = Color.white;       //颜色
    }

    public UIMod uiMod;
    private string _resModule;
    private List<Image> _pools;
    private List<ImageInfo> _imgInfoList = new List<ImageInfo>();
    private static readonly Regex _PairRegex = new Regex(@"(\w+)=([^\s]+)");//(key)=(value)
    private bool _isDirty = false;
#if UNITY_EDITOR
    public bool showClickArea = false;
#endif

    protected override void Awake()
    {
        base.Awake();
        uiMod = GetComponentInParent<UIMod>();
        if (uiMod)
            _resModule = uiMod.resUtility.moduleName;
        //获取child中所有的image，缓存
        _pools = new List<Image>();
        Image[] collection = GetComponentsInChildren<Image>();
        for(int i = 0; i<collection.Length; i++)
        {
            Image img = collection[i];
            if (img.transform != transform)
            {
                img.gameObject.SetActive(false);
                _pools.Add(img);
            }
        }
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        base.OnPopulateMesh(toFill);
        if (font == null)
            return;
        _isDirty = true;
        _eventList.Clear();
        IList<UIVertex> verts = cachedTextGenerator.verts;
        //处理图片
        DealImgTag(text, verts);
        //处理下划线
        DealUnderlineTag(base.text, verts);
    }

    protected void Update()
    {
        if (_isDirty)
        {
            _isDirty = false;

            //显示图片
            int showCount = _imgInfoList.Count;
            for (int i = 0; i < showCount; i++)
            {
                ImageInfo imageInfo = _imgInfoList[i];
                Image image = null;
                if (i >= _pools.Count)
                {
                    image = NewImage();
                    _pools.Add(image);
                }
                else
                {
                    image = _pools[i];
                }
                image.gameObject.SetActive(true);
                image.sprite = LoadSprite(imageInfo.name);
                image.rectTransform.anchoredPosition = imageInfo.position;
                image.rectTransform.sizeDelta = imageInfo.size;
                image.color = imageInfo.color;
            }
            _imgInfoList.Clear();

#if UNITY_EDITOR
            if (showClickArea)
            {
                showCount += _eventList.Count;
                //点击区域
                for (int i = 0; i < _eventList.Count; i++)
                {
                    EventInfo e = _eventList[i];

                    Image image = null;
                    if (i >= _pools.Count)
                    {
                        image = NewImage();
                        _pools.Add(image);
                    }
                    else
                    {
                        image = _pools[i];
                    }
                    image.gameObject.SetActive(true);
                    image.sprite = null;
                    image.rectTransform.anchoredPosition = e.rect.center;
                    image.rectTransform.sizeDelta = new Vector2(e.rect.width, e.rect.height);
                    image.color = new Color(1,0,0,0.3f);
                }
            }
#endif

            //回收
            for (int i = showCount; i < _pools.Count; i++)
            {
                _pools[i].gameObject.SetActive(false);
            }
        }
    }

    private Image NewImage()
    {
        Image image;
        GameObject go = new GameObject("Image");
        go.layer = LayerMask.NameToLayer("UI");
        image = go.AddComponent<Image>();
        image.raycastTarget = false;
        image.transform.SetParent(rectTransform);
        image.transform.localScale = Vector3.one;
        return image;
    }

    private Sprite LoadSprite(string resName)
    {
        if (Application.isPlaying)
            return ResMgr.Inst.LoadAsset<Sprite>(resName, 1, _resModule); 
        else
            return null;
    }

    #region 处理图片 <quad img=资源名 size=20 hs=1 f=*** p=***/>
    private static readonly Regex imgRegex = new Regex(@"<quad img=([^>\s]+)([^>]*)/>");//(名字)(属性)

    protected void DealImgTag(string richText, IList<UIVertex> verts)
    {
        Match match = null;
        match = imgRegex.Match(richText);
        while (match.Success)
        {
            ImageInfo imageInfo = new ImageInfo();
            imageInfo.name = match.Groups[1].Value;
            //标签参数
            string paras = match.Groups[2].Value;
            float size = fontSize;
            float heightScale = 1f;//高度缩放
            int controlID = -9999;
            string parameter = null;//参数
            if (!string.IsNullOrEmpty(paras))
            {
                var keyValueCollection = _PairRegex.Matches(paras);
                for (int i = 0; i < keyValueCollection.Count; i++)
                {
                    string key = keyValueCollection[i].Groups[1].Value;
                    string value = keyValueCollection[i].Groups[2].Value;
                    switch (key)
                    {
                        case "size":
                            {
                                float.TryParse(value, out size);
                                break;
                            }
                        case "hs":
                            {
                                float.TryParse(value, out heightScale);
                                break;
                            }
                        case "f":
                            {
                                int.TryParse(value, out controlID);
                                break;
                            }
                        case "p":
                            {
                                parameter = value;
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
            imageInfo.size = new Vector2(size, size * heightScale);
            UIVertex uiVertex = verts[match.Index * 4 + 3]; //左下角的顶点坐标
            Vector2 pos = uiVertex.position;
            pos /= pixelsPerUnit;
            pos += new Vector2(imageInfo.size.x * 0.5f, imageInfo.size.y * 0.5f);
            pos += new Vector2(rectTransform.rect.size.x * (rectTransform.pivot.x - 0.5f), rectTransform.rect.size.y * (rectTransform.pivot.y - 0.5f));
            imageInfo.position = pos;
            imageInfo.color = Color.white;

            if (controlID!=-9999)
            {
                EventInfo e = new EventInfo();
                e.controlID = controlID;
                e.parameter = parameter;
                e.rect = new Rect(pos - imageInfo.size / 2, imageInfo.size);
                _eventList.Add(e);
            }
            _imgInfoList.Add(imageInfo);
            _isDirty = true;

            match = match.NextMatch();
        }
    }
    #endregion

    #region 处理underline <material=underline c=#ffffff h=1 f=*** p=***>blablabla</material>
    class UnderlineInfo
    {
        public string content;
        public int startIndex;
    }

    private static readonly Regex regex = new Regex(@"</*material[^>]*>");//<material xxx> or </material>
    private const string endStr = "</material>";
    private static readonly Regex tagRegex = new Regex(@"<material=([^>\s]+)([^>]*)>");//(标签类型)(标签参数)
    private Stack<UnderlineInfo> tagHeadStack = new Stack<UnderlineInfo>(); //存储标签头，因为嵌套

    private void DealUnderlineTag(string richText, IList<UIVertex> verts)
    {
        if (richText.Length * 4 > verts.Count) return; //在标签输入的过程中，会出现
        tagHeadStack.Clear();
        Match match = regex.Match(richText);
        while (match.Success)
        {
            if (match.Value == endStr) //匹配到标签尾
            {
                if (tagHeadStack.Count == 0)
                {
                    match = match.NextMatch();
                    continue;
                }
                UnderlineInfo tagParser = tagHeadStack.Pop();
                int startIndex = tagParser.startIndex;
                int endIndex = match.Index;
                if (endIndex <= startIndex) //没有囊括内容
                {
                    match = match.NextMatch();
                    continue;
                }
                //标签类型
                Match headMatch = tagRegex.Match(tagParser.content);
                if (!headMatch.Success)
                {
                    match = match.NextMatch();
                    continue;
                }
                string tagName = headMatch.Groups[1].Value;
                if (tagName != "underline")
                {
                    match = match.NextMatch();
                    continue;
                }

                //解析标签参数
                Color color = Color.white;//颜色
                float height = 1f;//线段高度
                int controlID = -99999;
                string parameter = "";//参数
                var keyValueCollection = _PairRegex.Matches(headMatch.Groups[2].Value);
                for (int i = 0; i < keyValueCollection.Count; i++)
                {
                    string key = keyValueCollection[i].Groups[1].Value;
                    string value = keyValueCollection[i].Groups[2].Value;
                    switch (key)
                    {
                        case "c":
                            {
                                ColorUtility.TryParseHtmlString(value, out color);
                                break;
                            }
                        case "h":
                            {
                                float.TryParse(value, out height);
                                break;
                            }
                        case "f":
                            {
                                int.TryParse(value, out controlID);
                                break;
                            }
                        case "p":
                            {
                                parameter = value;
                                break;
                            }
                        default:
                            break;
                    }
                }

                float unitsPerPixel = 1 / pixelsPerUnit;

                //0 1|4 5|8  9 |12 13
                //3 2|7 6|11 10|14 15
                //<material=underline c=#ffffff h=1 n=1 p=2>下划线</material>
                //以上面为例:
                //tag.start为42，对应“>” | start对应“下”的左上角顶点
                //tag.end为44，对应“划”  | end对应“线”下一个字符的左上角顶点
                int start = startIndex * 4;
                int end = Mathf.Min(endIndex * 4, verts.Count);
                UIVertex vt1 = verts[start + 3];
                float minY = vt1.position.y;
                float maxY = verts[start].position.y;
                //换行处理，如需换行，则将一条下划线分割成几条
                //顶点取样分布，如上图的2，6，10，其中end - 2表示最后一个取样点，即10
                //对应例子中的下、划、线的右下角顶点
                for (int i = start + 2; i <= end - 2; i += 4)
                {
                    UIVertex vt2 = verts[i];
                    bool newline = Mathf.Abs(vt2.position.y - vt1.position.y) > fontSize;
                    if (newline || i == end - 2)
                    {
                        ImageInfo imageInfo = new ImageInfo();

                        //计算宽高
                        int tailIndex = !newline && i == end - 2 ? i : i - 4;
                        vt2 = verts[tailIndex];
                        minY = vt2.position.y;
                        imageInfo.size = new Vector2((vt2.position.x - vt1.position.x) * unitsPerPixel, height);

                        //计算位置
                        Vector2 vertex = new Vector2(vt1.position.x, minY);
                        vertex *= unitsPerPixel;
                        vertex += new Vector2(imageInfo.size.x * 0.5f, -height * 0.5f);
                        vertex += new Vector2(rectTransform.rect.size.x * (rectTransform.pivot.x - 0.5f), rectTransform.rect.size.y * (rectTransform.pivot.y - 0.5f));
                        imageInfo.position = vertex;

                        imageInfo.color = color;
                        _imgInfoList.Add(imageInfo);

                        if (controlID != -99999)
                        {
                            EventInfo e = new EventInfo();
                            e.controlID = controlID;
                            e.parameter = parameter;
                            Vector2 pos = new Vector2(vt1.position.x, minY);
                            pos *= unitsPerPixel;
                            pos += new Vector2(rectTransform.rect.size.x * (rectTransform.pivot.x - 0.5f), rectTransform.rect.size.y * (rectTransform.pivot.y - 0.5f));
                            e.rect = new Rect(pos, new Vector2(imageInfo.size.x, (maxY - minY) * unitsPerPixel));
                            _eventList.Add(e);
                        }
                        maxY = minY;
                        vt1 = verts[i + 1];
                        minY = vt1.position.y;
                        if (newline && i == end - 2) i -= 4;
                    }
                }
            }
            else //匹配到标签头
            {
                UnderlineInfo tagParser = new UnderlineInfo();
                tagParser.content = match.Value;
                tagParser.startIndex = match.Index + match.Length; //匹配位置+标签内容到长度
                tagHeadStack.Push(tagParser);
            }
            match = match.NextMatch();
        }
    }

    #endregion

    #region 点击
    class EventInfo
    {
        public Rect rect;//触发事件的判定区域
        public int controlID;//事件名
        public string parameter;//事件参数
    }
    private List<EventInfo> _eventList = new List<EventInfo>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (uiMod == null)
            return;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPos);
        for (int i = _eventList.Count - 1; i >= 0; i--)
        {
            EventInfo e = _eventList[i];
            if (e.rect.Contains(localPos))
            {
                uiMod.OnEvent(UIEVENT.UIRICHREXT_CLICK, e.controlID, e.parameter);
                break;
            }
        }
    }
    #endregion

}