using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DefaultData를 마우스로 가져다 가면 이 글씨가 뜸
/// 1. 데이터의 총 개수
/// 2. 데이터의 목록
/// 3. 추가 삭제 복사 기능
/// </summary>
public class DefaultData : ScriptableObject //SciptableObject : 대량의 데이터를 저장하는데 사용할 수 있는 데이터 컨테이너
{
    public const string pathData = "/Resources/Data";

    //고유 아이디 속성
    // null을 써주는 이유는 개수를 카운트할때 필요해서
    public string[] idx = null;

    public DefaultData() { } 

    //데이터의 총 개수 가져오기
    public int getDataCnt()
    {
        int _retCnt = 0;

        //this는 혹시 모른 오타방지 등등을 위해 씀
        if(this.idx != null)
        {
            _retCnt = this.idx.Length;
        }

        return _retCnt;
    }

    
    public string[] getDataIdxList(bool flagID, string strFilter = "") //strFilter는 매개변수에 포함된 스트링만 가져옴 (검색어)
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
                if(idx[i].ToLower()/*소문자로*/.Contains/*비교하기*/(strFilter.ToLower()) == false)
                {
                    continue;
                }
            }

            //flagId가 true라면 [0] [1] 순서로 정리해줌
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


    public virtual int constructionData(string _dataidx) //생성
    {
        return getDataCnt();
    }

    public virtual void deleteData(int _pid) //삭제
    {

    }

    public virtual void defulicateData(int _pid) //복사
    {

    }

}
