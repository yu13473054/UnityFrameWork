using System.Collections.Generic;
using UnityEngine;

//public enum SPValueType
//{
//    INT, INT_EN, LONG, LONG_EN, FLOAT, FLOAT_EN, BOOL, string, string_EN
//}

public class PrefMgr : MonoBehaviour
{
    #region 初始化
    private static PrefMgr _inst;
    public static PrefMgr Inst
    {
        get { return _inst; }
    }
    #endregion

    void Awake()
    {
        _inst = this;
        DontDestroyOnLoad(gameObject);

        PlayerPrefs.SetInt("aa",1);
        PlayerPrefs.Save();
    }
//    private PlayerPrefs sp;
//    private bool dirty;//是否需要提交
//    private Dictionary<string, Data> dataMap = new Dictionary<string, Data>();
//
//    /**
//	 * 初始化sp
//	 * @param datas 必须是3的倍数个元素
//	 */
//    public void init(Object[] datas)
//    {
//        init("data", datas);
//    }
//    public void init(string spName, Object[] datas)
//    {
//        sp = Gdx.app.getPreferences(spName);
//        //1.注册：数组中得数据
//        for (int i = 0; i < datas.Length; i += 3)
//        {
//            string name = (string)datas[i];
//            dataMap.put(name, new Data(name, (SPValueType)datas[i + 1], datas[i + 2]));
//        }
//    }
//
//    /**
//	 * 从sp中返回一个数据
//	 * @param key sp中的key值
//	 * @param type 值的类型
//	 * @param defValue 如果未注册过改该值，返回的默认值
//	 * @return
//	 */
//    public <T> T getData(string key, SPValueType type, Object defValue)
//    {
//        Data resultData = dataMap.get(key);
//        if (resultData == null)
//        {
//            Data data = new Data(key, type, defValue);
//            dataMap.put(key, data);
//            return data.getValue();
//        }
//        else
//        {
//            return resultData.getValue();
//        }
//    }
//
//    /**
//	 * 直接从sp中获得对应的key的值，可能为null
//	 * @param key
//	 * @return
//	 */
//    public <T> T getData(string key)
//    {
//        return dataMap.get(key).getValue();
//    }
//
//    /**
//	 * 提交数据
//	 * @param key 
//	 * @param value
//	 */
//    public void commit(string key, Object value)
//    {
//        dirty = true;
//        dataMap.get(key).setValue(value);
//    }
//
//    /**
//	 * 提交数据
//	 * @param key
//	 * @param type 未注册时，用于定义其类型
//	 * @param value
//	 */
//    public void commit(string key, SPValueType type, Object value)
//    {
//        dirty = true;
//        Data resultData = dataMap.get(key);
//        if (resultData == null)
//        {
//            Data data = new Data(key, type, value);
//            dataMap.put(key, data);
//            data.setValue(value);
//        }
//        else
//        {
//            resultData.setValue(value);
//        }
//    }
//
//    /**
//	 * 后台数据防篡改机制
//	 * @return
//	 */
//    public boolean isModify()
//    {
//        boolean flag = false;
//        Values<Data> values = dataMap.values();
//        for (Data data : values)
//        {
//            flag = data.isChange();
//            if (flag)
//            {
//                break;
//            }
//        }
//        return flag;
//    }
//
//    public void commitAll()
//    {
//        if (dirty)
//        {
//            sp.flush();
//        }
//    }
//
//    private class Data
//    {
//        Object value, valueEn;
//        string key;
//        SPValueType type;
//
//        Object oldVEN, oldV;//旧的加密至和该加密值对应的解密后的值
//
//        private Data(string key, SPValueType type, Object defValue)
//        {
//            this.key = key;
//            this.type = type;
//            switch (type)
//            {
//                case BOOL:
//                    value = sp.getBoolean(key, (Boolean)defValue);
//                    break;
//                case FLOAT:
//                    value = sp.getFloat(key, (Float)defValue);
//                    break;
//                case FLOAT_EN:
//                    valueEn = sp.getFloat(key, DecipherRes.getInstance().dataDeal((Float)defValue, false));
//                    value = DecipherRes.getInstance().dataDeal((Float)valueEn, true);
//                    break;
//                case INT:
//                    value = sp.getInteger(key, (Integer)defValue);
//                    break;
//                case INT_EN:
//                    valueEn = sp.getInteger(key, DecipherRes.getInstance().dataDeal((Integer)defValue));
//                    value = DecipherRes.getInstance().dataDeal((Integer)valueEn);
//                    break;
//                case LONG:
//                    value = sp.getLong(key, (Long)defValue);
//                    break;
//                case LONG_EN:
//                    valueEn = sp.getLong(key, DecipherRes.getInstance().dataDeal((Long)defValue));
//                    value = DecipherRes.getInstance().dataDeal((Long)valueEn);
//                    break;
//                case string:
//                    value = sp.getstring(key, (string)defValue);
//                    break;
//                case string_EN:
//                    valueEn = sp.getstring(key, DecipherRes.getInstance().dataDeal((string)defValue, false));
//                    value = DecipherRes.getInstance().dataDeal((string)valueEn, true);
//                    break;
//                default:
//                    break;
//            }
//        }
//
//
//        @SuppressWarnings("unchecked")
//
//        public <T> T getValue()
//        {
//            return (T)value;
//        }
//
//        /**
//		 * 判断原始值是否修改过，如果修改过，就进行解密操作
//		 * @return
//		 */
//        public boolean isChange()
//        {
//            boolean flag = false;
//            switch (type)
//            {
//                case INT_EN:
//                    //加密数据未变化时，直接判断,变化后，进行如下操作
//                    if (oldVEN == null || !oldVEN.equals(valueEn))
//                    {
//                        //记录加密值、记录解密值
//                        oldVEN = valueEn;
//                        oldV = DecipherRes.getInstance().dataDeal((Integer)valueEn);
//                    }
//                    flag = !value.equals(oldV);
//                    break;
//                case FLOAT_EN:
//                    //加密数据未变化时，直接判断,变化后，进行如下操作
//                    if (oldVEN == null || !oldVEN.equals(valueEn))
//                    {
//                        //记录加密值、记录解密值
//                        oldVEN = valueEn;
//                        oldV = DecipherRes.getInstance().dataDeal((Float)valueEn, true);
//                    }
//                    flag = !value.equals(oldV);
//                    break;
//                case LONG_EN:
//                    //加密数据未变化时，直接判断,变化后，进行如下操作
//                    if (oldVEN == null || !oldVEN.equals(valueEn))
//                    {
//                        //记录加密值、记录解密值
//                        oldVEN = valueEn;
//                        oldV = DecipherRes.getInstance().dataDeal((Long)valueEn);
//                    }
//                    flag = !value.equals(oldV);
//                    break;
//                case string_EN:
//                    //加密数据未变化时，直接判断,变化后，进行如下操作
//                    if (oldVEN == null || !oldVEN.equals(valueEn))
//                    {
//                        //记录加密值、记录解密值
//                        oldVEN = valueEn;
//                        oldV = DecipherRes.getInstance().dataDeal((string)valueEn, true);
//                    }
//                    flag = !value.equals(oldV);
//                    break;
//                default:
//                    break;
//            }
//            return flag;
//        }
//
//        public void setValue(Object value)
//        {
//            this.value = value;
//            switch (type)
//            {
//                case BOOL:
//                    sp.putBoolean(key, (Boolean)value);
//                    break;
//                case FLOAT:
//                    sp.putFloat(key, (Float)value);
//                    break;
//                case LONG:
//                    sp.putLong(key, (Long)value);
//                    break;
//                case INT:
//                    sp.putInteger(key, (Integer)value);
//                    break;
//                case string:
//                    sp.putstring(key, (string)value);
//                    break;
//                case FLOAT_EN:
//                    valueEn = DecipherRes.getInstance().dataDeal((Float)value, false);
//                    sp.putFloat(key, (Float)valueEn);
//                    break;
//                case string_EN:
//                    valueEn = DecipherRes.getInstance().dataDeal((string)value, false);
//                    sp.putstring(key, (string)valueEn);
//                    break;
//                case INT_EN:
//                    valueEn = DecipherRes.getInstance().dataDeal((Integer)value);
//                    sp.putInteger(key, (Integer)valueEn);
//                    break;
//                case LONG_EN:
//                    valueEn = DecipherRes.getInstance().dataDeal((Long)value);
//                    sp.putLong(key, (Long)valueEn);
//                    break;
//                default:
//                    break;
//            }
//        }
//    }


    void OnDestroy()
    {
        _inst = null;
    }
}
