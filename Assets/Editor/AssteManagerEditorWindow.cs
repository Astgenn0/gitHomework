using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class AssetManagerEditorWindow : EditorWindow
{
    public static GUIStyle TitleTextStyle;

    public static GUIStyle VersionTextstyle;
    public void Awake()
    {
        TitleTextStyle = new GUIStyle();
        TitleTextStyle.fontSize = 26;
        TitleTextStyle.normal.textColor = Color.red;
        TitleTextStyle.alignment = TextAnchor.MiddleCenter;

        VersionTextstyle = new GUIStyle();
        VersionTextstyle.fontSize = 20;
        VersionTextstyle.normal.textColor = Color.gray;
        VersionTextstyle.alignment = TextAnchor.MiddleRight;
    }

    private void OnEnable()
    {
        AssetManagerEditor.GetCurrentDeirectoryAllAssets();
    }
    //�����������ÿ����Ⱦ֡����,����������Ⱦui����
    private void OnGUI()
    {
        //Ĭ�ϴ�ֱ�Ű�
        GUILayout.Space(20);

        #region Title��������

        GUILayout.Label(nameof(AssetManagerEditor),TitleTextStyle);

        #endregion

        GUILayout.Label(AssetManagerEditor.AssetManagerVersion, VersionTextstyle);

        GUILayout.Space(20);
        AssetManagerEditor.BuildingPattern = (AssetBundlePattern)EditorGUILayout.EnumPopup("���ģʽ",AssetManagerEditor.BuildingPattern);

        GUILayout.Space(20);
        AssetManagerEditor.CompressionPattern = (AssetBundleCompresionPattern)EditorGUILayout.EnumPopup("ѹ����ʽ", AssetManagerEditor.CompressionPattern);

        //�����Դѡ��
        GUILayout.Space(20);
        AssetManagerEditor.AssetBundleDirectory = EditorGUILayout.ObjectField(AssetManagerEditor.AssetBundleDirectory, typeof(DefaultAsset), true) as DefaultAsset;

        if (AssetManagerEditor.CurrentAllAssets != null)
        {
            for(int i = 0; i < AssetManagerEditor.CurrentAllAssets.Count; i++)
            {
                AssetManagerEditor.CurrentSelectAssets[i] = EditorGUILayout.ToggleLeft(AssetManagerEditor.CurrentAllAssets[i], AssetManagerEditor.CurrentSelectAssets[i]);
            }
        }


        if (GUILayout.Button("���AssetBundle"))
        {
            Debug.Log("��ť����");
            AssetManagerEditor.BuildAssetBundleFormSets();
            //AssetManagerEditor.BuildAssetBundleFromDirectory();
        }
        
    }
}
