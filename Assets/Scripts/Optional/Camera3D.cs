using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class Camera3D : MonoBehaviour
{
    public float distance = 10;
    [HideInInspector]
    public bool moveValid;
    //缩放限制
    public float zoomInLimit = 6f;
    public float zoomOutLimit = 20f;

    public Vector4 mapLimit = new Vector4(-50, 50, -50, 50);//XZ平面，依次是左右下上
    protected Vector4 _camLimit;

    [HideInInspector]
    public Camera myCamera;

    Vector3 _oldPos1, _oldPos2;
    bool _move, _inputValid, _zoom;
    int _dragLayer;

    void Awake()
    {
        myCamera = GetComponent<Camera>();
        moveValid = true;
        _dragLayer = 1 << LayerMask.NameToLayer("Draggable");
    }

    void Start()
    {
        CalculateViewBounds();
    }

    void Update()
    {
        if (!moveValid)
        {
            _inputValid = false;
            return;
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        UpdatePC();
#else
        UpdateTouch();
#endif
    }

    void UpdatePC()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                int uiLayer = LayerMask.NameToLayer("UI");
                for(int i = 0; i < results.Count; i++)
                {
                    //点击了UI层元素
                    if (results[i].gameObject.layer == uiLayer) return;
                }
            }

            _inputValid = true;
            _oldPos1 = Input.mousePosition;
            //按下

        }
        else if (Input.GetMouseButton(0))
        {
            if (!_inputValid) return;
            RaycastHit hit, oldHit;
            if(GetHit(Input.mousePosition, out hit) && GetHit(_oldPos1, out oldHit))
            {
                Vector3 moveDir = hit.point - oldHit.point;
                if (moveDir == Vector3.zero) return;
                if(!_move && EventSystem.current)
                {
                    EventSystem.current.currentInputModule.DeactivateModule();
                }
                Move(moveDir);
                _oldPos1 = Input.mousePosition;
                _move = true;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (!_inputValid) return;
            _inputValid = false;
            if (_move)
            {
                _move = false;
                return;
            }
            //点击释放
        }

        float length = Input.GetAxis("Mouse ScrollWheel") * 200f;
        if (Mathf.Abs(length) > 0.0001f)
            Zoom(length);
    }

    void UpdateTouch()
    {
        if (Input.touchCount == 1)
        {
            var touch0 = Input.GetTouch(0);
            if(touch0.phase == TouchPhase.Began)
            {
                if (EventSystem.current)
                {
                    PointerEventData eventData = new PointerEventData(EventSystem.current);
                    eventData.position = touch0.position;
                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(eventData, results);

                    int uiLayer = LayerMask.NameToLayer("UI");
                    for (int i = 0; i < results.Count; i++)
                    {
                        //点击了UI层元素
                        if (results[i].gameObject.layer == uiLayer) return;
                    }
                }

                _inputValid = true;
                _zoom = false;
                _oldPos1 = touch0.position;
                //按下
            }
            else if (touch0.phase == TouchPhase.Moved)
            {
                if (!_inputValid || _zoom) return;
                RaycastHit hit, oldHit;
                if (GetHit(touch0.position, out hit) && GetHit(_oldPos1, out oldHit))
                {
                    Vector3 moveDir = hit.point - oldHit.point;
                    if (moveDir == Vector3.zero) return;
                    if (!_move && EventSystem.current)
                    {
                        EventSystem.current.currentInputModule.DeactivateModule();
                    }
                    Move(moveDir);
                    _oldPos1 = touch0.position;
                    _move = true;
                }
            }
            else if (touch0.phase == TouchPhase.Ended)
            {
                if (!_inputValid) return;
                _inputValid = false;
                if (_move)
                {
                    _move = false;
                    return;
                }
                //点击释放
                
            }
        }
        else if(Input.touchCount == 2) 
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);
            if(touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
            {
                _oldPos1 = touch0.position;
                _oldPos2 = touch1.position;
            }
            else if(touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                float len1 = Vector3.Distance(_oldPos1, _oldPos2);
                float len2 = Vector3.Distance(touch0.position, touch1.position);
                if (Mathf.Approximately(len1, len2)) return;
                Zoom((len2 - len1) * 0.05f);
                _oldPos1 = touch0.position;
                _oldPos2 = touch1.position;
                _zoom = true;
            }
        }
    }

    //获取射线检测点
    private bool GetHit(Vector3 screenPoint, out RaycastHit hit)
    {
        Ray ray = myCamera.ScreenPointToRay(screenPoint);
        if(Physics.Raycast(ray, out hit, 100, _dragLayer))
        {
            return true;
        }
        return false;
    }

    //缩放
    private void Zoom(float length)
    {
        if (Mathf.Approximately(zoomInLimit, zoomOutLimit)) return; //不缩放

        float oldDis = distance;
        distance -= length;
        if (distance < zoomInLimit) distance = zoomInLimit;
        if (distance > zoomOutLimit) distance = zoomOutLimit;
        length = distance - oldDis;

        myCamera.transform.position -= myCamera.transform.forward * length;
        CalculateViewBounds();//重新计算限定范围
    }

    private void Move(Vector3 delta)
    {
        delta = -delta;
        Vector3 newPos = myCamera.transform.position + delta;
        if (newPos.x < _camLimit.x)
            newPos.x = _camLimit.x;
        else if (newPos.x > _camLimit.y)
            newPos.x = _camLimit.y;

        if (newPos.z < _camLimit.z)
            newPos.z = _camLimit.z;
        else if (newPos.z > _camLimit.w)
            newPos.z = _camLimit.w;

        myCamera.transform.position = newPos;
    }

    //相机对齐到某一个点
    public void LookAtPos(Vector3 pos)
    {
        myCamera.transform.position = pos - myCamera.transform.forward * distance;
    }


    protected virtual void CalculateViewBounds()
    {
        Vector3 lb = new Vector3(mapLimit.x, 0, mapLimit.z);
        Vector3 rt = new Vector3(mapLimit.y, 0, mapLimit.w);
        lb = lb - myCamera.transform.forward * distance;
        rt = rt - myCamera.transform.forward * distance;
        _camLimit = new Vector4(lb.x, rt.x, lb.z, rt.z);
    }
}
