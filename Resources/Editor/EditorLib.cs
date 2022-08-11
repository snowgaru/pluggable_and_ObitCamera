using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using System.Text;
using System.IO;

public class EditorLib
{
    //1 파일경로를 찾아주는 기능
    //2 enum structure
    //3 layout
    //  1)top
    //  2)list : content
    //  3)botton

    //1 파일경로를 찾아주는 기능
    public static string getAssetPath(UnityEngine.Object _attr)
    {
        string retStrPath = string.Empty;

        //asset / resources / ......
        retStrPath = AssetDatabase.GetAssetPath(_attr);

        string[] tmpStrPath = retStrPath.Split("/");
        bool flagRes = false;
        for(int i = 0; i < tmpStrPath.Length; i++)
        {
            if(flagRes == false)
            {
                if(tmpStrPath[i] == "Resources")
                {
                    flagRes = true;
                    retStrPath = string.Empty;
                }
            } 
            else
            {
                retStrPath += tmpStrPath[i] + "/";
            }
        }

        return retStrPath;
    }

    //2 enum structure
    public static void makeEnumClass(string enumName, StringBuilder enumData)
    {
        string _filePathTemplate = "Assets/Editor/Class/EnumClassTemplate.txt";
        string contentClassTemplate = File.ReadAllText(_filePathTemplate);
        contentClassTemplate = contentClassTemplate.Replace("$CLASSENUM$", enumName);
        contentClassTemplate = contentClassTemplate.Replace("$DATAINFO$", enumData.ToString());
        string tempFilePathTemplate = "Assets/Resources/EnumClass";
        if(Directory.Exists(tempFilePathTemplate) == false)
        {
            Directory.CreateDirectory(tempFilePathTemplate);
        }

        string retFilePathTemplate = tempFilePathTemplate + enumName + ".cs";
        if(File.Exists(retFilePathTemplate))
        {
            File.Delete(retFilePathTemplate);
        }
        File.WriteAllText(retFilePathTemplate, contentClassTemplate);
    }

    //3 layout
    //  1)top
    //  2)list : content
    //  3)botton
    public static void setTopLayer(DefaultData data, ref int nowIdx, ref UnityEngine.Object objLayer, int sizeWidth)
    {
        EditorGUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("Constructor", GUILayout.Width(sizeWidth)))
            {
                data.constructionData("NewData");
                nowIdx = data.getDataCnt() - 1;
                objLayer = null;
            }

            if (GUILayout.Button("Defulicate", GUILayout.Width(sizeWidth)))
            {
                data.defulicateData(nowIdx);
                objLayer = null;
                nowIdx = data.getDataCnt() - 1;
            }

            if(data.getDataCnt() > 0)
            {
                if (GUILayout.Button("Delete", GUILayout.Width(sizeWidth)))
                {
                    objLayer = null;
                    data.deleteData(nowIdx);
                }
            }

            if(nowIdx > data.getDataCnt() - 1)
            {
                nowIdx = data.getDataCnt() - 1;
            }

        }
        EditorGUILayout.EndHorizontal();
    }

    public static void setListLayer(ref Vector2 posScroll, DefaultData data, ref int nowIdx, ref UnityEngine.Object objLayer, int sizeWidth)
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(sizeWidth));
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginVertical("box");
            {
                posScroll = EditorGUILayout.BeginScrollView(posScroll);
                {
                    if(data.getDataCnt() > 0)
                    {
                        int lastIdx = nowIdx;
                        nowIdx = GUILayout.SelectionGrid(nowIdx, data.getDataIdxList(true), 1);
                        if(lastIdx != nowIdx)
                        {
                            objLayer = null;

                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
    }
}
