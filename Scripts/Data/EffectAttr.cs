using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectAtrrType
{
    None = -1, // 기본값
    Normal, //보통
    Attack,
    Damage,
    Etc
}

public class EffectAttr 
{
    public int code = 0;
    public EffectAtrrType effectAttrType = EffectAtrrType.None;

    public GameObject effectObj = null;

    public string effectObjName = string.Empty;
    public string effectObjPath = string.Empty;
    public string effectObjFullPath = string.Empty;

    public EffectAttr() { }

    /// <summary>
    /// 사전 로딩 기능
    /// </summary>
    public void effectPreLoad()
    {
        this.effectObjFullPath = effectObjPath + effectObjName;
        if(this.effectObjFullPath != string.Empty && this.effectObj == null)
        {
            this.effectObj = ResourceManager.Load(effectObjFullPath) as GameObject;
        }
    }

    /// <summary>
    /// 프리로드 한 이펙트를 지워주는것
    /// </summary>
    public void deleteEffect()
    {
        if(this.effectObj != null)
        {
            this.effectObj = null;
        }
    }

    public GameObject Instantiate(Vector3 pos)
    {
        if(this.effectObj == null)
        {
            this.effectPreLoad();
        }

        if(this.effectObj != null)
        {
            GameObject refEffectObj = 
                GameObject.Instantiate(effectObj, pos, Quaternion.identity);
            return refEffectObj;
        }

        return null;
    }
}
