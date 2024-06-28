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

        AssetManagerEditor.AssetBundleDirectory = EditorGUILayout.ObjectField(AssetManagerEditor.AssetBundleDirectory,typeof(DefaultAsset),true) as DefaultAsset;


        if (GUILayout.Button("���AssetBundle"))
        {

            AssetManagerEditor.BuildAssetBundleFromDirectory();
        }
        
    }
}
