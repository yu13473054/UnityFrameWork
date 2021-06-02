using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Text;

public class HttpClient
{
    // 委托回调
    public delegate void ResponseDelegate( int error, int msgID, string response, bool dontDecode = false, params object[] param );

    //POST请求
    public IEnumerator PostForm(int msgID, string url, Dictionary<string, string> post, ResponseDelegate responseDelegate, params object[] param )
    {
        //表单
        WWWForm form = new WWWForm();
        //从集合中取出所有参数，设置表单参数
        foreach( KeyValuePair<string, string> post_arg in post )
        {
            form.AddField( post_arg.Key, post_arg.Value );
        }

        //Post传表单
        UnityWebRequest www = UnityWebRequest.Post( url, form );
        yield return www.SendWebRequest();

        if( www.error != null )
        {//POST请求失败
            if( responseDelegate != null )
                responseDelegate.Invoke( 1, msgID, www.error, false, param );
        }
        else
        {//POST请求成功
            if( responseDelegate != null )
                responseDelegate.Invoke( 0, msgID, www.downloadHandler.text, false, param );
        }
    }


    //POST请求
    public IEnumerator PostJson( int msgID, string url, string json, ResponseDelegate responseDelegate, params object[] param )
    {
        byte[] body = Encoding.UTF8.GetBytes( json );
        UnityWebRequest www = new UnityWebRequest( url, "POST" );
        www.uploadHandler = new UploadHandlerRaw( body );
        www.SetRequestHeader( "Content-Type", "application/json" );
        www.SetRequestHeader( "Content-Type", "text/json" );
        www.downloadHandler = new DownloadHandlerBuffer();
        yield return www.SendWebRequest();

        if ( www.error != null )
        {//POST请求失败
            if ( responseDelegate != null )
                responseDelegate.Invoke( 1, msgID, www.error, false, param );
        }
        else
        {//POST请求成功
            if ( responseDelegate != null )
                responseDelegate.Invoke( 0, msgID, www.downloadHandler.text, false, param );
        }
    }

    //GET请求
    public IEnumerator Get(int msgID, string url, Dictionary<string, string> get, bool dontDecode, ResponseDelegate responseDelegate, params object[] param )
    {
        string Parameters;
        bool first;
        if( get != null && get.Count > 0 )
        {
            first = true;
            Parameters = "?";
            //从集合中取出所有参数，拼url串
            foreach( KeyValuePair<string, string> post_arg in get )
            {
                if( first )
                    first = false;
                else
                    Parameters += "&";

                Parameters += post_arg.Key + "=" + post_arg.Value;
            }
        }
        else
        {
            Parameters = "";
        }

        //直接URL传值就是get 
        string uri = url + Parameters;
#if NPC_REPORTER
        Debug.Log("<HttpClient> Get地址：" + uri);
#endif
        UnityWebRequest www = UnityWebRequest.Get( uri );
        www.timeout = 10;
        yield return www.SendWebRequest();

        if( www.error != null )
        {//GET请求失败
            if( responseDelegate != null )
                responseDelegate.Invoke( 1, msgID, www.error, dontDecode, param );

        }
        else
        {//GET请求成功
            if( responseDelegate != null )
                responseDelegate.Invoke( 0, msgID, www.downloadHandler.text, dontDecode, param );
        }
    }
}

