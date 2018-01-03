using System;
using System.Collections.Generic;
using UnityEngine;

public class UIResPathData : ScriptableObject
{
	public List<UIResPathProperty> _properties = new List<UIResPathProperty>();
}

[Serializable]
public class UIResPathProperty
{
	public string _resName;
	public string _tagName;
	public string _res_develop;
	public string _res_public;
	public string _abName;
}
