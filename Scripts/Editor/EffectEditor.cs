using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

public class EffectEditor : EditorWindow
{
    public int sizeLargeWidth = 400;
    public int sizeLargeHeight = 200;

    private int nowCode = 0;
    private Vector2 posScroll_1 = Vector2.zero;
    private Vector2 posScroll_2 = Vector2.zero;

    private GameObject effectObject = null;

    private static EffectXMLData effectXMLData;

    [MenuItem("ToolEffect/Editor")] 
    static void init()
    {
        effectXMLData = ScriptableObject.CreateInstance<EffectXMLData>();
        effectXMLData.LoadData();

        EffectEditor window = GetWindow<EffectEditor>(false, "Effect Editor");
        window.Show();
    }

    void OnGUI()
    {
        if(effectXMLData == null)
        {
            return;
        }

        EditorGUILayout.BeginVertical();
        {
            //top
            UnityEngine.Object editObj = effectObject;
            EditorLib.setTopLayer(effectXMLData, ref nowCode, ref editObj, this.sizeLargeWidth);
            effectObject = (GameObject)editObj;

            //middle
            EditorGUILayout.BeginHorizontal();
            {
                //list(left)
                EditorLib.setListLayer(ref posScroll_1, effectXMLData,ref nowCode, ref editObj, this.sizeLargeWidth);
                effectObject = (GameObject)editObj;

                //content(right) 이건 에디터마다 달라서 editorLib에 템플릿을 안만들어둔거
                EditorGUILayout.BeginVertical();
                {
                    posScroll_2 = EditorGUILayout.BeginScrollView(this.posScroll_2);
                    {
                        if(effectXMLData.getDataCnt() > 0)
                        {
                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.Separator();

                                EditorGUILayout.LabelField("코드", nowCode.ToString(), GUILayout.Width(sizeLargeWidth));

                                effectXMLData.idx[nowCode] = EditorGUILayout.
                                    TextField("고유코드", effectXMLData.idx[nowCode], GUILayout.Width(sizeLargeWidth + 1.5f));

                                effectXMLData.effectAttrs[nowCode].effectAttrType = (EffectAtrrType)EditorGUILayout
                                    .EnumPopup("이펙트 타입", effectXMLData.effectAttrs[nowCode].effectAttrType, GUILayout.Width(sizeLargeWidth));

                                EditorGUILayout.Separator();

                                if(effectObject == null && effectXMLData.effectAttrs[nowCode].effectObjName != string.Empty)
                                {
                                    effectXMLData.effectAttrs[nowCode].effectPreLoad();
                                    effectObject = (GameObject)ResourceManager.
                                        Load(effectXMLData.effectAttrs[nowCode].effectObjPath + effectXMLData.effectAttrs[nowCode].effectObjName);
                                }

                                effectObject = (GameObject)EditorGUILayout.
                                    ObjectField("이펙트", this.effectObject, typeof(GameObject), false, GUILayout.Width(sizeLargeWidth));

                                string _tmpEffectObjPath = string.Empty;
                                string _tmpEffectObjName = string.Empty;
                                if(effectObject != null)
                                {
                                    _tmpEffectObjName = effectObject.name;
                                    _tmpEffectObjPath = EditorLib.getAssetPath(this.effectObject);
                                }
                                else
                                {
                                    effectXMLData.effectAttrs[nowCode].effectObjName = _tmpEffectObjName;
                                    effectXMLData.effectAttrs[nowCode].effectObjPath = _tmpEffectObjPath;
                                    effectObject = null;
                                }

                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    EditorGUILayout.EndScrollView();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        {
            if(GUILayout.Button("Reload"))
            {
                effectXMLData = CreateInstance<EffectXMLData>();
                effectXMLData.LoadData();
                nowCode = 0;
                this.effectObject = null;
            }

            if(GUILayout.Button("Save"))
            {
                EffectEditor.effectXMLData.writeXMLData();
                CreateEnumStructure();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            }

        }
        EditorGUILayout.EndHorizontal();
    }

    public void CreateEnumStructure()
    {
        string enumName = "EffectList";
        StringBuilder builder = new StringBuilder();
        builder.AppendLine();

        for(int i = 0; i < effectXMLData.idx.Length; i++)
        {
            builder.AppendLine("    " + effectXMLData.idx[i] + "=" + i + ",");
        }

        EditorLib.makeEnumClass(enumName, builder);
    }
}
