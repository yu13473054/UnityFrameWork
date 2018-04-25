using UnityEngine;
using System;
using System.Collections.Generic;
using LuaInterface;
using UnityEditor;

using BindType = ToLuaMenu.BindType;
using System.Reflection;
using UnityEngine.UI;

public static class CustomSettings
{
    public static string saveDir = Application.dataPath + "/ToLua/Source/Generate/";    
    public static string toluaBaseType = Application.dataPath + "/ToLua/BaseType/";
    public static string baseLuaDir = Application.dataPath + "/Lua/ToLua/";
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
        typeof(UnityEngine.QualitySettings),
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
        _GT(typeof(LuaInjectionStation)),
        _GT(typeof(InjectType)),
        _GT(typeof(Debugger)).SetNameSpace(null),
        _GT(typeof(LuaProfiler)),        

#if USING_DOTWEENING
        _GT(typeof(DG.Tweening.DOTween)),
        _GT(typeof(DG.Tweening.Tween)).SetBaseType(typeof(System.Object)).AddExtendType(typeof(DG.Tweening.TweenExtensions)),
        _GT(typeof(DG.Tweening.Sequence)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
        _GT(typeof(DG.Tweening.Tweener)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
        _GT(typeof(DG.Tweening.LoopType)),
        _GT(typeof(DG.Tweening.PathMode)),
        _GT(typeof(DG.Tweening.PathType)),
        _GT(typeof(DG.Tweening.RotateMode)),
        _GT(typeof(Component)).SetNameSpace(null).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Transform)).SetNameSpace(null).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Light)).SetNameSpace(null).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Material)).SetNameSpace(null).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Rigidbody)).SetNameSpace(null).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Camera)).SetNameSpace(null).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(AudioSource)).SetNameSpace(null).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        //_GT(typeof(LineRenderer)).SetNameSpace(null).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        //_GT(typeof(TrailRenderer)).SetNameSpace(null).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),    
#else
                                         
        _GT(typeof(Component)).SetNameSpace(null),
        _GT(typeof(Transform)).SetNameSpace(null),
        _GT(typeof(Material)).SetNameSpace(null),
        _GT(typeof(Light)).SetNameSpace(null),
        _GT(typeof(Rigidbody)).SetNameSpace(null),
        _GT(typeof(Camera)).SetNameSpace(null),
        _GT(typeof(AudioSource)).SetNameSpace(null),
        //_GT(typeof(LineRenderer)).SetNameSpace(null),
        //_GT(typeof(TrailRenderer)).SetNameSpace(null),
#endif
      
        _GT(typeof(Behaviour)).SetNameSpace(null),
        _GT(typeof(MonoBehaviour)).SetNameSpace(null),        
        _GT(typeof(GameObject)).SetNameSpace(null),
        _GT(typeof(TrackedReference)).SetNameSpace(null),
        _GT(typeof(Application)).SetNameSpace(null),
        _GT(typeof(Physics)).SetNameSpace(null),
        _GT(typeof(Collider)).SetNameSpace(null),
        _GT(typeof(Time)).SetNameSpace(null),        
        _GT(typeof(Texture)).SetNameSpace(null),
        _GT(typeof(Texture2D)).SetNameSpace(null),
        _GT(typeof(Shader)).SetNameSpace(null),        
        _GT(typeof(Renderer)).SetNameSpace(null),
        _GT(typeof(WWW)).SetNameSpace(null),
        _GT(typeof(Screen)).SetNameSpace(null),        
        _GT(typeof(CameraClearFlags)).SetNameSpace(null),
        _GT(typeof(AudioClip)).SetNameSpace(null),        
        _GT(typeof(AssetBundle)).SetNameSpace(null),
        _GT(typeof(ParticleSystem)).SetNameSpace(null),
        _GT(typeof(AsyncOperation)).SetBaseType(typeof(System.Object)).SetNameSpace(null),        
        _GT(typeof(LightType)).SetNameSpace(null),
        _GT(typeof(SleepTimeout)).SetNameSpace(null),
        _GT(typeof(Animator)).SetNameSpace(null),
        _GT(typeof(Input)).SetNameSpace(null),
        _GT(typeof(KeyCode)).SetNameSpace(null),
        _GT(typeof(SkinnedMeshRenderer)).SetNameSpace(null),
        _GT(typeof(Space)).SetNameSpace(null),      
       

        _GT(typeof(MeshRenderer)).SetNameSpace(null),

        _GT(typeof(BoxCollider)).SetNameSpace(null),
        _GT(typeof(MeshCollider)).SetNameSpace(null),
        _GT(typeof(SphereCollider)).SetNameSpace(null),        
        _GT(typeof(CharacterController)).SetNameSpace(null),
        _GT(typeof(CapsuleCollider)).SetNameSpace(null),
        
        _GT(typeof(Animation)).SetNameSpace(null),        
        _GT(typeof(AnimationClip)).SetBaseType(typeof(UnityEngine.Object)).SetNameSpace(null),        
        _GT(typeof(AnimationState)).SetNameSpace(null),
        _GT(typeof(AnimationBlendMode)).SetNameSpace(null),
        _GT(typeof(QueueMode)).SetNameSpace(null),  
        _GT(typeof(PlayMode)).SetNameSpace(null),
        _GT(typeof(WrapMode)).SetNameSpace(null),

        _GT(typeof(QualitySettings)).SetNameSpace(null),
        _GT(typeof(RenderSettings)).SetNameSpace(null),                                                   
        _GT(typeof(BlendWeights)).SetNameSpace(null),           
        _GT(typeof(RenderTexture)).SetNameSpace(null),
        _GT(typeof(Resources)).SetNameSpace(null),     


        //自定义
        _GT(typeof(AppConst)),
        _GT(typeof(UIMod)),
        _GT(typeof(UISystem)),
        _GT(typeof(Localization)),

        //Manager
        _GT(typeof(UIMgr)),
        _GT(typeof(AudioMgr)),
        _GT(typeof(PrefMgr)),
        _GT(typeof(ResMgr)),
        _GT(typeof(DatabaseMgr)),
        _GT(typeof(TimerMgr)),
        _GT(typeof(NetworkMgr)),

        //UI
        _GT(typeof(RectTransform)),
        _GT(typeof(LayoutRebuilder)).SetNameSpace(null),
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
        _GT(typeof(WrapContent)),
    };

    public static List<Type> dynamicList = new List<Type>()
    {
        typeof(MeshRenderer),
        typeof(BoxCollider),
        typeof(MeshCollider),
        typeof(SphereCollider),
        typeof(CharacterController),
        typeof(CapsuleCollider),

        typeof(Animation),
        typeof(AnimationClip),
        typeof(AnimationState),

        typeof(BlendWeights),
        typeof(RenderTexture),
        typeof(Rigidbody),
    };

    //重载函数，相同参数个数，相同位置out参数匹配出问题时, 需要强制匹配解决
    //使用方法参见例子14
    public static List<Type> outList = new List<Type>()
    {
        
    };
        
    //ui优化，下面的类没有派生类，可以作为sealed class
    public static List<Type> sealedList = new List<Type>()
    {
        /* typeof(UIImage),
         typeof(UIImage),
         typeof(UIText),
         typeof(UIButton),
         typeof(UIToggle),
         typeof(UISlider),
         typeof(UIScrollView),
         typeof(UIScrollbar),
         typeof(UIInputField),
         typeof(UIRaycast),
         typeof(UIPolygonRaycast),
         typeof(WrapContent),*/
    };

    public static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    public static DelegateType _DT(Type t)
    {
        return new DelegateType(t);
    }    

    [MenuItem("Lua/Attach Profiler", false, 151)]
    static void AttachProfiler()
    {
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog("警告", "请在运行时执行此功能", "确定");
            return;
        }

        LuaClient.Instance.AttachProfiler();
    }

    [MenuItem("Lua/Detach Profiler", false, 152)]
    static void DetachProfiler()
    {
        if (!Application.isPlaying)
        {            
            return;
        }

        LuaClient.Instance.DetachProfiler();
    }
}
