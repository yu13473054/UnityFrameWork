using System.Collections.Generic;
using UnityEngine;

public class Data 
{
    private static Data _inst;
    public static Data Inst
    {
        get
        {
            if (_inst == null) _inst = new Data();
            return _inst;
        }
    }

    private int[] ToIntArray(string content)
    {
        if (string.IsNullOrEmpty(content)) return new int[0];
        string[] splits = content.Split('|');
        int[] ts = new int[splits.Length];
        for(int i = 0; i < splits.Length - 1; i++)
        {
            ts[i] = int.Parse(splits[i]);
        }
        return ts;
    }
    private float[] ToFloatArray(string content)
    {
        if (string.IsNullOrEmpty(content)) return new float[0];
        string[] splits = content.Split('|');
        float[] ts = new float[splits.Length];
        for (int i = 0; i < splits.Length - 1; i++)
        {
            ts[i] = float.Parse(splits[i]);
        }
        return ts;
    }
    private double[] ToDoubleArray(string content)
    {
        if (string.IsNullOrEmpty(content)) return new double[0];
        string[] splits = content.Split('|');
        double[] ts = new double[splits.Length];
        for (int i = 0; i < splits.Length - 1; i++)
        {
            ts[i] = double.Parse(splits[i]);
        }
        return ts;
    }
    private bool[] ToBoolArray(string content)
    {
        if (string.IsNullOrEmpty(content)) return new bool[0];
        string[] splits = content.Split('|');
        bool[] ts = new bool[splits.Length];
        for (int i = 0; i < splits.Length - 1; i++)
        {
            ts[i] = splits[i].ToLower().Equals("1");
        }
        return ts;
    }
    private string[] ToStringArray(string content)
    {
        if (string.IsNullOrEmpty(content)) return new string[0];
        string[] splits = content.Split('|');
        return splits;
    }

    private Dictionary<int, AIParser> _aIDic;
    private void ParseAI()
    {
        _aIDic = new Dictionary<int, AIParser>();
        TableHandler handler = TableHandler.OpenFromResmap("AI");
        for(int i = 0; i < handler.GetRecordsNum() - 1; i++)
        {
            int key = int.Parse(handler.GetValue(i, 0));
            AIParser info = new AIParser()
            {
                sn = int.Parse(handler.GetValue(i, 0)),
				v1 = int.Parse(handler.GetValue(i, 1)),
				v2 = float.Parse(handler.GetValue(i, 2)),
				v3 = handler.GetValue( i, 3 ),
				v4 = ToIntArray(handler.GetValue(i, 4)),
				v5 = ToFloatArray(handler.GetValue(i, 5)),
				v6 = ToStringArray(handler.GetValue(i, 6)),
				v7 = handler.GetValue( i, 7 ),
            };

        }
    }
    public AIParser AI(int sn)
    {
        if(_aIDic == null)
            ParseUnit();
        AIParser parser;
        if(_aIDic.TryGetValue(sn, out parser))
        {
            return parser;
        }
        else
        {
            Debug.LogErrorFormat("<Data> 表格{0}中不存在第{1}条数据，请检查！", "AI", sn);
            return null;
        }
    }

	private Dictionary<int, UnitParser> _unitDic;
    private void ParseUnit()
    {
        _unitDic = new Dictionary<int, UnitParser>();
        TableHandler handler = TableHandler.OpenFromResmap("Unit");
        for(int i = 0; i < handler.GetRecordsNum() - 1; i++)
        {
            int key = int.Parse(handler.GetValue(i, 0));
            UnitParser info = new UnitParser()
            {
                sn = int.Parse(handler.GetValue(i, 0)),
				name = handler.GetValue( i, 1 ),
				aiType = int.Parse(handler.GetValue(i, 2)),
				skillList = ToIntArray(handler.GetValue(i, 3)),
				charSn = int.Parse(handler.GetValue(i, 4)),
				hp = int.Parse(handler.GetValue(i, 5)),
				atk = int.Parse(handler.GetValue(i, 6)),
				def = int.Parse(handler.GetValue(i, 7)),
            };

        }
    }
    public UnitParser Unit(int sn)
    {
        if(_unitDic == null)
            ParseUnit();
        UnitParser parser;
        if(_unitDic.TryGetValue(sn, out parser))
        {
            return parser;
        }
        else
        {
            Debug.LogErrorFormat("<Data> 表格{0}中不存在第{1}条数据，请检查！", "Unit", sn);
            return null;
        }
    }

	private Dictionary<int, SkillParser> _skillDic;
    private void ParseSkill()
    {
        _skillDic = new Dictionary<int, SkillParser>();
        TableHandler handler = TableHandler.OpenFromResmap("Skill");
        for(int i = 0; i < handler.GetRecordsNum() - 1; i++)
        {
            int key = int.Parse(handler.GetValue(i, 0));
            SkillParser info = new SkillParser()
            {
                sn = int.Parse(handler.GetValue(i, 0)),
				cd = int.Parse(handler.GetValue(i, 1)),
				type = int.Parse(handler.GetValue(i, 2)),
				launcherList = ToIntArray(handler.GetValue(i, 3)),
				ftlName = ToStringArray(handler.GetValue(i, 4)),
            };

        }
    }
    public SkillParser Skill(int sn)
    {
        if(_skillDic == null)
            ParseUnit();
        SkillParser parser;
        if(_skillDic.TryGetValue(sn, out parser))
        {
            return parser;
        }
        else
        {
            Debug.LogErrorFormat("<Data> 表格{0}中不存在第{1}条数据，请检查！", "Skill", sn);
            return null;
        }
    }

	private Dictionary<int, CharParser> _charDic;
    private void ParseChar()
    {
        _charDic = new Dictionary<int, CharParser>();
        TableHandler handler = TableHandler.OpenFromResmap("Char");
        for(int i = 0; i < handler.GetRecordsNum() - 1; i++)
        {
            int key = int.Parse(handler.GetValue(i, 0));
            CharParser info = new CharParser()
            {
                sn = int.Parse(handler.GetValue(i, 0)),
				res = handler.GetValue( i, 1 ),
            };

        }
    }
    public CharParser Char(int sn)
    {
        if(_charDic == null)
            ParseUnit();
        CharParser parser;
        if(_charDic.TryGetValue(sn, out parser))
        {
            return parser;
        }
        else
        {
            Debug.LogErrorFormat("<Data> 表格{0}中不存在第{1}条数据，请检查！", "Char", sn);
            return null;
        }
    }

	private Dictionary<int, Fight_BuffParser> _fight_BuffDic;
    private void ParseFight_Buff()
    {
        _fight_BuffDic = new Dictionary<int, Fight_BuffParser>();
        TableHandler handler = TableHandler.OpenFromResmap("Fight_Buff");
        for(int i = 0; i < handler.GetRecordsNum() - 1; i++)
        {
            int key = int.Parse(handler.GetValue(i, 0));
            Fight_BuffParser info = new Fight_BuffParser()
            {
                sn = int.Parse(handler.GetValue(i, 0)),
				actionType = int.Parse(handler.GetValue(i, 1)),
				buffType = int.Parse(handler.GetValue(i, 2)),
				valueList = ToIntArray(handler.GetValue(i, 3)),
				turn = int.Parse(handler.GetValue(i, 4)),
				dispelType = int.Parse(handler.GetValue(i, 5)),
				fxSn_act = int.Parse(handler.GetValue(i, 6)),
				fxSn_con = int.Parse(handler.GetValue(i, 7)),
				fxSn_des = int.Parse(handler.GetValue(i, 8)),
				sort = ToIntArray(handler.GetValue(i, 9)),
				viewType = int.Parse(handler.GetValue(i, 10)),
				icon = handler.GetValue( i, 11 ),
				desc = handler.GetValue( i, 12 ),
            };

        }
    }
    public Fight_BuffParser Fight_Buff(int sn)
    {
        if(_fight_BuffDic == null)
            ParseUnit();
        Fight_BuffParser parser;
        if(_fight_BuffDic.TryGetValue(sn, out parser))
        {
            return parser;
        }
        else
        {
            Debug.LogErrorFormat("<Data> 表格{0}中不存在第{1}条数据，请检查！", "Fight_Buff", sn);
            return null;
        }
    }

	private Dictionary<int, Fight_LauncherParser> _fight_LauncherDic;
    private void ParseFight_Launcher()
    {
        _fight_LauncherDic = new Dictionary<int, Fight_LauncherParser>();
        TableHandler handler = TableHandler.OpenFromResmap("Fight_Launcher");
        for(int i = 0; i < handler.GetRecordsNum() - 1; i++)
        {
            int key = int.Parse(handler.GetValue(i, 0));
            Fight_LauncherParser info = new Fight_LauncherParser()
            {
                sn = int.Parse(handler.GetValue(i, 0)),
				life = int.Parse(handler.GetValue(i, 1)),
				interval = int.Parse(handler.GetValue(i, 2)),
				num = int.Parse(handler.GetValue(i, 3)),
				bulletSn = int.Parse(handler.GetValue(i, 4)),
            };

        }
    }
    public Fight_LauncherParser Fight_Launcher(int sn)
    {
        if(_fight_LauncherDic == null)
            ParseUnit();
        Fight_LauncherParser parser;
        if(_fight_LauncherDic.TryGetValue(sn, out parser))
        {
            return parser;
        }
        else
        {
            Debug.LogErrorFormat("<Data> 表格{0}中不存在第{1}条数据，请检查！", "Fight_Launcher", sn);
            return null;
        }
    }

	private Dictionary<int, Fight_BulletParser> _fight_BulletDic;
    private void ParseFight_Bullet()
    {
        _fight_BulletDic = new Dictionary<int, Fight_BulletParser>();
        TableHandler handler = TableHandler.OpenFromResmap("Fight_Bullet");
        for(int i = 0; i < handler.GetRecordsNum() - 1; i++)
        {
            int key = int.Parse(handler.GetValue(i, 0));
            Fight_BulletParser info = new Fight_BulletParser()
            {
                sn = int.Parse(handler.GetValue(i, 0)),
				life = float.Parse(handler.GetValue(i, 1)),
				speed = float.Parse(handler.GetValue(i, 2)),
				checkType = int.Parse(handler.GetValue(i, 3)),
				checkValues = ToFloatArray(handler.GetValue(i, 4)),
				trigger = int.Parse(handler.GetValue(i, 5)),
				targetType = int.Parse(handler.GetValue(i, 6)),
				targetValues = ToFloatArray(handler.GetValue(i, 7)),
				effectList = ToIntArray(handler.GetValue(i, 8)),
            };

        }
    }
    public Fight_BulletParser Fight_Bullet(int sn)
    {
        if(_fight_BulletDic == null)
            ParseUnit();
        Fight_BulletParser parser;
        if(_fight_BulletDic.TryGetValue(sn, out parser))
        {
            return parser;
        }
        else
        {
            Debug.LogErrorFormat("<Data> 表格{0}中不存在第{1}条数据，请检查！", "Fight_Bullet", sn);
            return null;
        }
    }

	private Dictionary<int, Fight_EffectParser> _fight_EffectDic;
    private void ParseFight_Effect()
    {
        _fight_EffectDic = new Dictionary<int, Fight_EffectParser>();
        TableHandler handler = TableHandler.OpenFromResmap("Fight_Effect");
        for(int i = 0; i < handler.GetRecordsNum() - 1; i++)
        {
            int key = int.Parse(handler.GetValue(i, 0));
            Fight_EffectParser info = new Fight_EffectParser()
            {
                sn = int.Parse(handler.GetValue(i, 0)),
				decType = ToIntArray(handler.GetValue(i, 1)),
				decValue = ToIntArray(handler.GetValue(i, 2)),
				actionType = int.Parse(handler.GetValue(i, 3)),
				valueList = ToIntArray(handler.GetValue(i, 4)),
            };

        }
    }
    public Fight_EffectParser Fight_Effect(int sn)
    {
        if(_fight_EffectDic == null)
            ParseUnit();
        Fight_EffectParser parser;
        if(_fight_EffectDic.TryGetValue(sn, out parser))
        {
            return parser;
        }
        else
        {
            Debug.LogErrorFormat("<Data> 表格{0}中不存在第{1}条数据，请检查！", "Fight_Effect", sn);
            return null;
        }
    }

	private Dictionary<int, AudioParser> _audioDic;
    private void ParseAudio()
    {
        _audioDic = new Dictionary<int, AudioParser>();
        TableHandler handler = TableHandler.OpenFromResmap("Audio");
        for(int i = 0; i < handler.GetRecordsNum() - 1; i++)
        {
            int key = int.Parse(handler.GetValue(i, 0));
            AudioParser info = new AudioParser()
            {
                sn = int.Parse(handler.GetValue(i, 0)),
				name = handler.GetValue( i, 1 ),
				type = int.Parse(handler.GetValue(i, 2)),
				volume = float.Parse(handler.GetValue(i, 3)),
				loop = int.Parse(handler.GetValue(i, 4)),
            };

        }
    }
    public AudioParser Audio(int sn)
    {
        if(_audioDic == null)
            ParseUnit();
        AudioParser parser;
        if(_audioDic.TryGetValue(sn, out parser))
        {
            return parser;
        }
        else
        {
            Debug.LogErrorFormat("<Data> 表格{0}中不存在第{1}条数据，请检查！", "Audio", sn);
            return null;
        }
    }

/////////////////////Permanent////////////////////
	
}
