using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectObjectManager : SingleMonobehaviour<EffectObjectManager>
{
    private Transform effectObj = null;

    void Start()
    {
        if(effectObj == null)
        {
            effectObj = new GameObject("EffectObj").transform;
            effectObj.SetParent(transform);
        }
    }

    public GameObject EffectInstantiate(int idx, Vector3 pos)
    {
        EffectAttr attr = DataXMLManager.effectData().getAttr(idx);
        GameObject effectInstance = attr.Instantiate(pos);
        effectInstance.SetActive(true);
        return effectInstance;
    }
}
