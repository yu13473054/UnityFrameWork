using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using LuaInterface;
using UnityEditor;
using BindType = ToLuaMenu.BindType;
using System.Reflection;
using UnityEngine.UI;
using Animation = UnityEngine.Animation;
using AnimationState = UnityEngine.AnimationState;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

public static class WrapList
{
    public static string saveDir = Application.dataPath + "/ToLua/Source/Generate/";
    public static string toluaBaseType = Application.dataPath + "/ToLua/BaseType/";
    public static string injectionFilesPath = Application.dataPath + "/ToLua/Injection/";

    //导出时强制做为静态类的类型(注意customTypeList 还要添加这个类型才能导出)
    //unity 有些类作为sealed class, 其实完全等价于静态类
    public static List<Type> staticClassTypes = new List<Type>
    {        
        typeof(UnityEngine.Application),
        typeof(UnityEngine.Time),
        typeof(UnityEngine.Screen),
        typeof(UnityEngine.SleepTimeout),
        typeof(UnityEngine.Input),
        typeof(UnityEngine.Resources),
        typeof(UnityEngine.Physics),
        typeof(UnityEngine.RenderSettings),
        typeof(UnityEngine.GL),
        typeof(UnityEngine.Graphics),
    };

    //附加导出委托类型(在导出委托时, customTypeList 中牵扯的委托类型都会导出， 无需写在这里)
    public static DelegateType[] customDelegateList = 
    {        
        _DT(typeof(Action)),                
        _DT(typeof(UnityEngine.Events.UnityAction)),
        _DT(typeof(System.Predicate<int>)),
        _DT(typeof(System.Action<int>)),
        _DT(typeof(System.Comparison<int>)),
        _DT(typeof(System.Func<int, int>)),
    };

    //在这里添加你要导出注册到lua的类型列表
    public static BindType[] customTypeList =
    {
#if USING_DOTWEENING
        _GT(typeof(Component)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Transform)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Light)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Material)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Rigidbody)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Camera)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(AudioSource)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        //_GT(typeof(LineRenderer)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        //_GT(typeof(TrailRenderer)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),    
#else
//        _GT(typeof(DG.Tweening.DOTween)).SetNameSpace(null),
//        _GT(typeof(DG.Tweening.Tween)).SetBaseType(typeof(System.Object)).AddExtendType(typeof(DG.Tweening.TweenExtensions)).SetNameSpace(null),
        //_GT(typeof(DG.Tweening.DOTweenAnimation)).SetNameSpace(null),
        //_GT(typeof(DG.Tweening.Core.ABSAnimationComponent)).SetNameSpace(null),
        //_GT(typeof(DG.Tweening.TweenSettingsExtensions)).SetNameSpace(null),
        //_GT(typeof(DG.Tweening.Ease)).SetNameSpace(null),

//        _GT(typeof(DG.Tweening.Sequence)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
//        _GT(typeof(DG.Tweening.Tweener)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
//        _GT(typeof(DG.Tweening.LoopType)).SetNameSpace(null),
//        _GT(typeof(DG.Tweening.PathMode)).SetNameSpace(null),
//        _GT(typeof(DG.Tweening.PathType)).SetNameSpace(null),
//        _GT(typeof(DG.Tweening.RotateMode)).SetNameSpace(null),

        _GT(typeof(Component)).SetNameSpace(null),
        _GT(typeof(Transform)).SetNameSpace(null),
        _GT(typeof(RectTransform)).SetNameSpace(null),
        // _GT(typeof(Material)).SetNameSpace(null),
        _GT(typeof(Rigidbody)).SetNameSpace(null),
        // _GT(typeof(Camera)).SetNameSpace(null),
        _GT(typeof(AudioSource)).SetNameSpace(null),
        _GT(typeof(LineRenderer)).SetNameSpace(null),
        //_GT(typeof(TrailRenderer))
#endif
      
        _GT(typeof(Behaviour)).SetNameSpace(null),
        _GT(typeof(MonoBehaviour)).SetNameSpace(null),        
        _GT(typeof(GameObject)).SetNameSpace(null),
        _GT(typeof(TrackedReference)).SetNameSpace(null),
        _GT(typeof(Application)).SetNameSpace(null),
        _GT(typeof(Physics)).SetNameSpace(null),
        _GT(typeof(Collider)).SetNameSpace(null),
        _GT(typeof(PolygonCollider2D)).SetNameSpace(null),
        _GT(typeof(BoxCollider2D)).SetNameSpace(null),
        _GT(typeof(Time)),        
        _GT(typeof(Texture)).SetNameSpace(null),
        _GT(typeof(Texture2D)).SetNameSpace(null),
        // _GT(typeof(Shader)).SetNameSpace(null),        
        _GT(typeof(Renderer)).SetNameSpace(null),
        // _GT(typeof(Screen)).SetNameSpace(null),        
        _GT(typeof(CameraClearFlags)).SetNameSpace(null),
        _GT(typeof(AudioClip)).SetNameSpace(null),        
        _GT(typeof(AssetBundle)).SetNameSpace(null),
        // _GT(typeof(ParticleSystemRenderer)).SetNameSpace(null),
        // _GT(typeof(ParticleSystem.MainModule)).SetNameSpace(null),
        _GT(typeof(AsyncOperation)).SetBaseType(typeof(System.Object)).SetNameSpace(null),        
        _GT(typeof(LightType)).SetNameSpace(null),
        _GT(typeof(SleepTimeout)).SetNameSpace(null),
        _GT(typeof(Animator)).SetNameSpace(null),
        _GT(typeof(AnimatorOverrideController)).SetNameSpace(null),
        _GT(typeof(AnimatorUpdateMode)).SetNameSpace(null),
        _GT(typeof(Input)).SetNameSpace(null),
        _GT(typeof(KeyCode)),
        _GT(typeof(SkinnedMeshRenderer)).SetNameSpace(null),
        _GT(typeof(Space)).SetNameSpace(null),
        _GT(typeof(BoxCollider)).SetNameSpace(null),
        _GT(typeof(MeshCollider)).SetNameSpace(null),
        _GT(typeof(SphereCollider)).SetNameSpace(null),        
        _GT(typeof(CharacterController)).SetNameSpace(null),
        _GT(typeof(CapsuleCollider)).SetNameSpace(null),
        _GT(typeof(Rect)).SetNameSpace(null),
        
        _GT(typeof(Animation)).SetNameSpace(null),        
        _GT(typeof(AnimationClip)).SetBaseType(typeof(UnityEngine.Object)).SetNameSpace(null),        
        _GT(typeof(AnimationState)).SetNameSpace(null),
        _GT(typeof(AnimationBlendMode)).SetNameSpace(null),
        _GT(typeof(QueueMode)).SetNameSpace(null),  
        _GT(typeof(PlayMode)).SetNameSpace(null),
        _GT(typeof(WrapMode)).SetNameSpace(null),

        _GT(typeof(RenderSettings)).SetNameSpace(null),                                                   
        _GT(typeof(SkinWeights)).SetNameSpace(null),           
        _GT(typeof(RenderTexture)).SetNameSpace(null),
        _GT(typeof(Resources)).SetNameSpace(null),
        _GT(typeof(Sprite)).SetNameSpace(null),
        _GT(typeof(SpriteRenderer)).SetNameSpace(null),
        _GT(typeof(SortingGroup)).SetNameSpace(null),
        _GT(typeof(LuaProfiler)),

        _GT(typeof(List<GameObject>)).SetNameSpace(null),
        _GT(typeof(Stopwatch)).SetNameSpace(null),

        // TimeLine        
        _GT(typeof(PlayableDirector)).SetNameSpace(null),
        _GT(typeof(PlayableAsset)).SetNameSpace(null),
        _GT(typeof(TimelineAsset)).SetNameSpace(null),
        _GT(typeof(List<PlayableAsset>)).SetNameSpace(null),
        _GT(typeof(List<string>)).SetNameSpace(null),


        // 自定义从此开始
        _GT(typeof(Localization)),
        _GT(typeof(TableHandler)),
        _GT(typeof(ConfigHandler)),
        _GT(typeof(GameMain)),
        _GT(typeof(UIMgr)),
        _GT(typeof(ResModuleUtility)),
        _GT(typeof(NetworkMgr)),
        _GT(typeof(AudioMgr)),
        _GT(typeof(CommonUtils)),
        _GT(typeof(FullScreen)),
        _GT(typeof(Debugger)),

        //UI
        _GT(typeof(UISystem)),
        _GT(typeof(UIMod)),
        _GT(typeof(UIItem)),
        _GT(typeof(LayoutRebuilder)).SetNameSpace(null),
        _GT(typeof(RectTransformUtility)).SetNameSpace(null),
        _GT(typeof(VerticalLayoutGroup)).SetNameSpace(null),
        _GT(typeof(HorizontalLayoutGroup)).SetNameSpace(null),
        _GT(typeof(GridLayoutGroup)).SetNameSpace(null),
        _GT(typeof(ToggleGroup)).SetNameSpace(null),
        _GT(typeof(Canvas)).SetNameSpace(null),
        _GT(typeof(CanvasGroup)).SetNameSpace(null),
        _GT(typeof(RectOffset)).SetNameSpace(null),
        _GT(typeof(UIImage)),
        _GT(typeof(UIRawImage)),
        _GT(typeof(UIText)),
        _GT(typeof(UIButton)),
        _GT(typeof(UIToggle)),
        _GT(typeof(UISlider)),
        _GT(typeof(UIScrollView)),
        _GT(typeof(UIScrollbar)),
        _GT(typeof(UIInputField)),
        _GT(typeof(UIRaycast)),
        _GT(typeof(UIPolygonRaycast)),
        
        // Tween      
         


        //Spine
        //_GT(typeof(SkeletonRenderer)),
        //_GT(typeof(SkeletonAnimation)).SetNameSpace(null),
        //_GT(typeof(SkeletonGraphic)).SetNameSpace(null),
        //_GT(typeof(Spine.SkeletonData)),
        //_GT(typeof(Spine.Animation)),
        //_GT(typeof(Spine.AnimationState)),
        //_GT(typeof(Spine.AnimationStateData)),
        //_GT(typeof(Spine.Skin)),
        //_GT(typeof(Spine.Event)),
        //_GT(typeof(Spine.ExposedList<Spine.Animation>)),
        //_GT(typeof(Spine.ExposedList<Spine.Skin>)),
        //_GT(typeof(Spine.Skeleton)),
        //_GT(typeof(Spine.Bone)),
        //_GT(typeof(SkeletonDataAsset)),
        //_GT(typeof(TrackEntry)),
        //_GT(typeof(SkeletonGhost)).SetNameSpace(null),
        //_GT(typeof(SpineFXBinder)),
        //_GT(typeof(SpineMatReplacer)),
        //_GT(typeof(BoneFollower)).SetNameSpace( null )
    };

    public static List<Type> dynamicList = new List<Type>()
    {
        typeof(BoxCollider),
        typeof(MeshCollider),
        typeof(SphereCollider),
        typeof(CharacterController),
        typeof(CapsuleCollider),
        typeof(Animation),
        typeof(AnimationClip),
        typeof(AnimationState),
        typeof(SkinWeights),
        typeof(RenderTexture),
        typeof(Rigidbody),

    };

    //重载函数，相同参数个数，相同位置out参数匹配出问题时, 需要强制匹配解决
    //使用方法参见例子14
    public static List<Type> outList = new List<Type>()
    {

    };
        
    //ngui优化，下面的类没有派生类，可以作为sealed class
    public static List<Type> sealedList = new List<Type>()
    {
        /*typeof(Transform),
        typeof(UIRoot),
        typeof(UICamera),
        typeof(UIViewport),
        typeof(UIPanel),
        typeof(UILabel),
        typeof(UIAnchor),
        typeof(UIAtlas),
        typeof(UIFont),
        typeof(UITexture),
        typeof(UISprite),
        typeof(UIGrid),
        typeof(UITable),
        typeof(UIWrapGrid),
        typeof(UIInput),
        typeof(UIScrollView),
        typeof(UIEventListener),
        typeof(UIScrollBar),
        typeof(UICenterOnChild),
        typeof(UIScrollView),        
        typeof(UIButton),
        typeof(UITextList),
        typeof(UIPlayTween),
        typeof(UIDragScrollView),
        typeof(UISpriteAnimation),
        typeof(UIWrapContent),
        typeof(TweenWidth),
        typeof(TweenAlpha),
        typeof(TweenColor),
        typeof(TweenRotation),
        typeof(TweenPosition),
        typeof(TweenScale),
        typeof(TweenHeight),
        typeof(TypewriterEffect),
        typeof(UIToggle),
        typeof(Localization),*/
    };

    public static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    public static DelegateType _DT(Type t)
    {
        return new DelegateType(t);
    }    


//    [MenuItem("Lua/Attach Profiler", false, 151)]
//    static void AttachProfiler()
//    {
//        if (!Application.isPlaying)
//        {
//            EditorUtility.DisplayDialog("警告", "请在运行时执行此功能", "确定");
//            return;
//        }

//        LuaClient.Instance.AttachProfiler();
//    }

//    [MenuItem("Lua/Detach Profiler", false, 152)]
//    static void DetachProfiler()
//    {
//        if (!Application.isPlaying)
//        {            
//            return;
//        }

//        LuaClient.Instance.DetachProfiler();
//    }
}
