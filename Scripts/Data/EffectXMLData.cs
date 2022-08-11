using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Xml;
using System.IO; //input out 줄임말

public class EffectXMLData : DefaultData
{
    //이펙트 속성 만들기
    public EffectAttr[] effectAttrs = new EffectAttr[0];

    public string effectAttrPath = "Resources/Prefab/Effects";

    private string xmlPath = "";
    private string xmlName = "effectXmlData.xml";
    private string dataPath = "Data/effectData";

    private const string EFFECT = "effect";
    private const string DEFAULT = "default";

    /// <summary>
    /// xml 쓰기
    /// </summary>
    public void writeXMLData()
    {
        // using() {} = try catch 를 자동으로 해줌
        using (XmlTextWriter xmlinfo =
            new XmlTextWriter(xmlPath + xmlName, System.Text.Encoding.Unicode))
        {
            xmlinfo.WriteStartDocument();

                xmlinfo.WriteStartElement(EFFECT);
                    xmlinfo.WriteStartElement("length", getDataCnt().ToString());
                    for(int i = 0; i < this.idx.Length; i++)
                    {
                        EffectAttr attr = this.effectAttrs[i];
                        xmlinfo.WriteStartElement(default);
                            xmlinfo.WriteElementString("code", i.ToString());
                            xmlinfo.WriteElementString("idx", this.idx[i]);
                            xmlinfo.WriteElementString("effectAttrType", attr.effectAttrType.ToString());
                            xmlinfo.WriteElementString("effectObjName", attr.effectObjName);
                            xmlinfo.WriteElementString("effectObjPath", attr.effectObjPath);
                        xmlinfo.WriteEndElement();
                    }
                    xmlinfo.WriteEndElement();        
                xmlinfo.WriteEndElement(); //여기에서는 안써줘도 알아서 해줌 EFFECT

            xmlinfo.WriteEndDocument();
        }
        
    }

    /// <summary>
    /// xml 읽기
    /// </summary>
    public void LoadData()
    {
        this.xmlPath = Application.dataPath + dataPath;

        TextAsset dataAsset = (TextAsset)ResourceManager.Load(xmlPath);
        if(dataAsset == null || dataAsset.text == null)
        {
            this.constructionData("Hello World");
            return;
        }

        using (XmlTextReader xmlinfo = new XmlTextReader(new StringReader(dataAsset.text)))
        {
            int nowCode = 0;
            while(xmlinfo.Read())
            {
                if(xmlinfo.IsStartElement())
                {
                    switch(xmlinfo.Name)
                    {
                        case "length":
                            int length = int.Parse(xmlinfo.ReadString());
                            this.idx = new string[length];
                            this.effectAttrs = new EffectAttr[length];
                            break;

                        case "code":
                            nowCode = int.Parse(xmlinfo.ReadString());
                            this.effectAttrs[nowCode] = new EffectAttr();
                            this.effectAttrs[nowCode].code = nowCode;
                            break;

                        case "idx":
                            this.idx[nowCode] = xmlinfo.ReadString();
                            break;

                        case "effectAttrType":
                            this.effectAttrs[nowCode].effectAttrType = (EffectAtrrType)Enum.Parse(typeof(EffectAtrrType), xmlinfo.ReadString());
                            break;

                        case "effectObjName":
                            this.effectAttrs[nowCode].effectObjName = xmlinfo.ReadString();
                            break;

                        case "effectObjPath":
                            this.effectAttrs[nowCode].effectObjPath = xmlinfo.ReadString();
                            break;
                    }
                }
            }
        }
    }

    public override int constructionData(string _dataidx)
    {
        if(this.idx == null)
        {
            this.idx = new string[] { name };
            this.effectAttrs = new EffectAttr[]
            {
                new EffectAttr()
            };
        }
        else
        {
            ArrayList _tmpList = new ArrayList();
            foreach(string _val in this.idx)
            {
                _tmpList.Add(_val);
            }
            _tmpList.Add(_dataidx);
            this.idx = (string[])_tmpList.ToArray(typeof(string));


            foreach (EffectAttr _val in this.effectAttrs)
            {
                _tmpList.Add(_val);
            }
            _tmpList.Add(_dataidx);
            this.effectAttrs = (EffectAttr[])_tmpList.ToArray(typeof(EffectAttr[]));
        }

        return getDataCnt();
    }

    public static T[] CreateAttr<T>(T _name, T[] idx)
    {
        ArrayList _tmpList = new ArrayList();
        foreach(T _val in idx)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(_name);
        return (T[])_tmpList.ToArray(typeof(T));
    }


    public override void deleteData(int _pid)
    {
        ArrayList _tmpList = new ArrayList();
        foreach (string _val in this.idx)
        {
            _tmpList.Add(_val);
        }
        _tmpList.RemoveAt(_pid);
        this.idx = (string[])_tmpList.ToArray(typeof(string));

        if (this.idx.Length == 0)
        {
            this.idx = null;
        }

        foreach (EffectAttr _val in this.effectAttrs)
        {
            _tmpList.Add(_val);
        }
        _tmpList.RemoveAt(_pid);
        this.effectAttrs = (EffectAttr[])_tmpList.ToArray(typeof(EffectAttr[]));
  
    }

    public override void defulicateData(int _pid)
    {
        ArrayList _tmpList = new ArrayList();
        foreach (string _val in this.idx)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(this.idx[_pid]);
        this.idx = (string[])_tmpList.ToArray(typeof(string));

        foreach (EffectAttr _val in this.effectAttrs)
        {
            _tmpList.Add(_val);
        }
        _tmpList.Add(GetCopy(_pid));
        this.effectAttrs = (EffectAttr[])_tmpList.ToArray(typeof(EffectAttr[]));
    }

    public EffectAttr GetCopy(int _pid)
    {
        if(_pid < 0 || _pid >= this.effectAttrs.Length)
        {
            return null;
        }
        EffectAttr beforeAttr = this.effectAttrs[_pid];
        EffectAttr attr = new EffectAttr();
        attr.effectObjFullPath = beforeAttr.effectObjFullPath;
        attr.effectObjName = beforeAttr.effectObjName;
        attr.effectAttrType = beforeAttr.effectAttrType;
        attr.effectObjPath = beforeAttr.effectObjPath;

        attr.code = this.effectAttrs.Length;
        return attr;
    }

    public EffectAttr getAttr(int _pid)
    {
        if (_pid < 0 || _pid >= this.effectAttrs.Length)
        {
            return null;
        }
        effectAttrs[_pid].effectPreLoad();
        return effectAttrs[_pid];
    }

    public void ClearData()
    {
        //게임 종료 직전에 모든 데이터 삭제하고 다시 저장
        foreach(EffectAttr attr in this.effectAttrs)
        {
            attr.deleteEffect();
        }
        this.effectAttrs = null;
        this.idx = null;
    }

}
