using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DefaultData�� ���콺�� ������ ���� �� �۾��� ��
/// 1. �������� �� ����
/// 2. �������� ���
/// 3. �߰� ���� ���� ���
/// </summary>
public class DefaultData : ScriptableObject //SciptableObject : �뷮�� �����͸� �����ϴµ� ����� �� �ִ� ������ �����̳�
{
    public const string pathData = "/Resources/Data";

    //���� ���̵� �Ӽ�
    // null�� ���ִ� ������ ������ ī��Ʈ�Ҷ� �ʿ��ؼ�
    public string[] idx = null;

    public DefaultData() { } 

    //�������� �� ���� ��������
    public int getDataCnt()
    {
        int _retCnt = 0;

        //this�� Ȥ�� �� ��Ÿ���� ����� ���� ��
        if(this.idx != null)
        {
            _retCnt = this.idx.Length;
        }

        return _retCnt;
    }

    
    public string[] getDataIdxList(bool flagID, string strFilter = "") //strFilter�� �Ű������� ���Ե� ��Ʈ���� ������ (�˻���)
    {
        string[] retList = new string[0];

        if(this.idx == null)
        {
            return retList;
        }

        retList = new string[this.idx.Length];

        for(int i = 0; i < this.idx.Length; i++)
        {
            if(strFilter != "")
            {
                if(idx[i].ToLower()/*�ҹ��ڷ�*/.Contains/*���ϱ�*/(strFilter.ToLower()) == false)
                {
                    continue;
                }
            }

            //flagId�� true��� [0] [1] ������ ��������
            if (flagID)
            {
                retList[i] = "[" + i.ToString() + "]" + this.idx[i];
            } else
            {
                retList[i] = this.idx[i];
            }
        }

        return retList;
    }


    public virtual int constructionData(string _dataidx) //����
    {
        return getDataCnt();
    }

    public virtual void deleteData(int _pid) //����
    {

    }

    public virtual void defulicateData(int _pid) //����
    {

    }

}
