using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScrollSlider : Slider
{
    public UIMod uiMod;
    public int controlID = 0;
    public int audioId = 0; // 迅速在声音表里预留一个Sn，填到这里成默认值
    
    public ScrollRect scrollRect;
    public enum ScrollRectDirection
    {
        vertical = 0,
        horizontal = 1
    }
    public ScrollRectDirection scrollRectDirection;

    protected override void Awake()
    {
        base.Awake();
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChange);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScrollRectValueChange);
        }
    }

    protected override void Start()
    {
        base.Start();
        if (scrollRect != null)
        {
            OnScrollRectValueChange(scrollRect.normalizedPosition);
        }


#if UNITY_EDITOR
        // 挂载uiMod
        if (uiMod == null)
        {
            uiMod = gameObject.GetComponentInParent<UIMod>();
        }
#endif
    }

    private bool CanDrag(PointerEventData eventData)
    {
        return this.IsActive() && this.IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (!CanDrag(eventData) || uiMod == null) return;
        uiMod.OnEvent(UIEVENT.UISLIDER_PRESS, controlID, 0);
        // 播放按钮音效
        if (audioId > 0)
        {
            AudioMgr.Inst.Play(audioId, uiMod.resModule);
        }

    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (eventData.button != PointerEventData.InputButton.Left || uiMod == null) return;
        uiMod.OnEvent(UIEVENT.UISLIDER_PRESS, controlID, 1);
    }


    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        if (!CanDrag(eventData) || uiMod == null) return;
        uiMod.OnEvent(UIEVENT.UISLIDER_DRAG, controlID, 1);

        //控制scrolView的滑动
        if (scrollRect != null)
        {
            if (scrollRectDirection == UIScrollSlider.ScrollRectDirection.vertical)
            {
                scrollRect.verticalNormalizedPosition = this.value;
            }

            else
            {
                scrollRect.horizontalNormalizedPosition = this.value;
            }
        }
    }

    public void OnScrollRectValueChange(Vector2 normalizedPosition)
    {
        if (scrollRectDirection == UIScrollSlider.ScrollRectDirection.vertical)
        {
            this.value = normalizedPosition.y;
        }

        else
        {
            this.value = normalizedPosition.x;
        }
        
        //如果内容小于等于视窗大小，则隐藏handle
        if (scrollRect.content.rect.height - scrollRect.viewport.rect.height <= 0)
        {
            this.handleRect.gameObject.SetActive(false);
        }
        else
        {
            this.handleRect.gameObject.SetActive(true);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (fillRect)
        {
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }
    }
#endif
}
