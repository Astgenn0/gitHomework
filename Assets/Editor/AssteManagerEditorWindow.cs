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
        AssetManagerEditor._IncrementalBuildMode = (IncrementalBuildMode)EditorGUILayout.EnumPopup("增量打包", AssetManagerEditor._IncrementalBuildMode);

        GUILayout.Space(20);
        AssetManagerEditor.CompressionPattern = (AssetBundleCompresionPattern)EditorGUILayout.EnumPopup("压缩格式", AssetManagerEditor.CompressionPattern);

        //打包资源选择
        GUILayout.Space(20);
        AssetManagerEditor.AssetBundleDirectory = EditorGUILayout.ObjectField(AssetManagerEditor.AssetBundleDirectory, typeof(DefaultAsset), true) as DefaultAsset;

        if (AssetManagerEditor.CurrentAllAssets != null)
        {
            for(int i = 0; i < AssetManagerEditor.CurrentAllAssets.Count; i++)
            {
                AssetManagerEditor.CurrentSelectedAssets[i] = EditorGUILayout.ToggleLeft(AssetManagerEditor.CurrentAllAssets[i], AssetManagerEditor.CurrentSelectedAssets[i]);
            }
        }


        if (GUILayout.Button("打包AssetBundle"))
        {
            Debug.Log("按钮按下");
            AssetManagerEditor.BuildAssetBundleFormSets();
            //AssetManagerEditor.BuildAssetBundleFromDirectory();
        }
        
    }
}
