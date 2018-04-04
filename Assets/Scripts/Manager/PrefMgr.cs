using UnityEngine;

public class PrefMgr : MonoBehaviour
{
    #region 初始化
    private static PrefMgr _inst;
    public static PrefMgr Inst
    {
        get { return _inst; }
    }
    public static void Init()
    {
        if (_inst)
        {
            return;
        }
        GameObject go = new GameObject("PrefMgr");
        go.AddComponent<PrefMgr>();
    }
    #endregion

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);

    }

    void OnDestroy()
    {
        _inst = null;
        Debug.Log("~PrefMgr was destroy!");
    }
}
