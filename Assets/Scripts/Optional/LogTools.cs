using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

public class LogTools : MonoBehaviour {

	string _path = "";
	FileStream _fs;
	StreamWriter _sw;

    List<string> _threadLogs = new List<string>();
	void Awake()
	{
		CreateLogFile ();
	}

	void Start ()
    {
		DontDestroyOnLoad (this);
	}

	void OnEnable()
    {
		Application.logMessageReceivedThreaded += HandleLog;
	}

    void OnDisable()
    {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    void OnDestroy()
    {
        Update();
    }

    void OnApplicationPause()
    {
        Update();
    }

    void OnApplicationQuit()
    {
        Update();
    }

    void HandleLog(string logString, string stackTrace, LogType type) 
    {
        lock (_threadLogs)
        {
            string log = null;
            if (type == LogType.Exception || type == LogType.Error)
            {
                log = string.Format("[{0}] {1}\n===[\n{2}===]\n", type, logString, stackTrace);
            }
            else
            {
                log = string.Format("[{0}] {1}\n", type, logString);
            }
            _threadLogs.Add(log);
        }
	}

	void CreateLogFile()
	{
		string directoryPath = Application.persistentDataPath + "/LogFile";
		_path = directoryPath+ "/" + DateTime.Now.ToString ("yyyyMMdd-HHmmss") + ".txt";
		if (!Directory.Exists(directoryPath))
			Directory.CreateDirectory(directoryPath);
		_fs = new FileStream (_path,FileMode.OpenOrCreate);
		_sw = new StreamWriter (_fs);
	    _sw.AutoFlush = true;
	}

    void Update()
    {
        if (_threadLogs.Count > 0)
        {
            lock (_threadLogs)
            {
                for (int i = 0; i < _threadLogs.Count; ++i)
                {
                    _sw.Write(_threadLogs[i]);
                }

                _threadLogs.Clear();
                _sw.Flush();
            }
        }
    }
}
