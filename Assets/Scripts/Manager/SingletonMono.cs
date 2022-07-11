using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _inst;
    public static T Inst
    {
        get
        {
            if (!_inst)
            {
                _inst = Object.FindObjectOfType<T>();
                if (!_inst)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _inst = go.AddComponent<T>();
                }
            }
            return _inst;
        }
    }

    protected virtual void Awake()
    {
        _inst = this as T;
    }

    protected virtual void OnDestroy()
    {
        _inst = null;
        Debug.Log($"<{typeof(T).Name}> OnDestroy!");
    }

}
