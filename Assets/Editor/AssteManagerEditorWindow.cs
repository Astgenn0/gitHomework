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
    //这个方法会在每个渲染帧调用,可以用来渲染ui界面
    private void OnGUI()
    {
        //默认垂直排版
        GUILayout.Space(20);

        #region Title文字内容

        GUILayout.Label(nameof(AssetManagerEditor),TitleTextStyle);

        #endregion

        GUILayout.Label(AssetManagerEditor.AssetManagerVersion, VersionTextstyle);

        GUILayout.Space(20);
        AssetManagerEditor.BuildingPattern = (AssetBundlePattern)EditorGUILayout.EnumPopup("打包模式",AssetManagerEditor.BuildingPattern);

        GUILayout.Space(20);

        AssetManagerEditor.AssetBundleDirectory = EditorGUILayout.ObjectField(AssetManagerEditor.AssetBundleDirectory,typeof(DefaultAsset),true) as DefaultAsset;


        if (GUILayout.Button("打包AssetBundle"))
        {

            AssetManagerEditor.BuildAssetBundleFromDirectory();
        }
        
    }
}
