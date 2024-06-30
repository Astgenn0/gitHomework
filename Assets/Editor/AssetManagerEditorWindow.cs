using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class AssetManagerEditorWindow : EditorWindow
{

    public string VersionString;

    public AssetManagetEditorWindowConfigSO WindowConfig;
    public void Awake()
    {
        AssetManagerEditor.LoadConfig(this);
        AssetManagerEditor.LoadWindowConfig(this);
    }

    //每当工程发生修改时会调用该方法
    public void OnValidate()
    {
        AssetManagerEditor.LoadConfig(this);
        AssetManagerEditor.LoadWindowConfig(this);
    }

    public void OnInspectorUpdate()
    {
        AssetManagerEditor.LoadConfig(this);
        AssetManagerEditor.LoadWindowConfig(this);
    }
    private void OnEnable()
    {
        AssetManagerEditor.AssetManagerConfig.GetCurrentDeirectoryAllAssets();
    }
    public DefaultAsset editorWindowDirectory=null;
    //这个方法会在每个渲染帧调用,可以用来渲染ui界面
    private void OnGUI()
    {
        //默认垂直排版
        GUILayout.Space(20);

        #region Title文字内容

        GUILayout.Label(nameof(AssetManagerEditor), WindowConfig.TitleTextStyle);

        #endregion

        GUILayout.Label(VersionString, WindowConfig.VersionTextstyle);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig.BuildingPattern = (AssetBundlePattern)EditorGUILayout.EnumPopup("打包模式",AssetManagerEditor.AssetManagerConfig.BuildingPattern);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig._IncrementalBuildMode = (IncrementalBuildMode)EditorGUILayout.EnumPopup("增量打包", AssetManagerEditor.AssetManagerConfig._IncrementalBuildMode);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig.CompressionPattern = (AssetBundleCompresionPattern)EditorGUILayout.EnumPopup("压缩格式", AssetManagerEditor.AssetManagerConfig.CompressionPattern);

        //打包资源选择
        GUILayout.Space(20);
        editorWindowDirectory = EditorGUILayout.ObjectField(editorWindowDirectory, typeof(DefaultAsset), true) as DefaultAsset;

        if (AssetManagerEditor.AssetManagerConfig.AssetBundleDirectory != editorWindowDirectory)
        {
            if (editorWindowDirectory == null)
            {
                AssetManagerEditor.AssetManagerConfig.CurrentAllAssets.Clear();
            }
            AssetManagerEditor.AssetManagerConfig.AssetBundleDirectory = editorWindowDirectory;
            AssetManagerEditor.AssetManagerConfig.GetCurrentDeirectoryAllAssets();
        }

        if (AssetManagerEditor.AssetManagerConfig.CurrentAllAssets != null)
        {
            for(int i = 0; i < AssetManagerEditor.AssetManagerConfig.CurrentAllAssets.Count; i++)
            {
                AssetManagerEditor.AssetManagerConfig.CurrentSelectedAssets[i] = EditorGUILayout.ToggleLeft(AssetManagerEditor.AssetManagerConfig.CurrentAllAssets[i], AssetManagerEditor.AssetManagerConfig.CurrentSelectedAssets[i]);
            }
        }


        if (GUILayout.Button("打包AssetBundle"))
        {
            Debug.Log(AssetManagerEditor.AssetManagerConfig.CurrentBuildVersion);
            AssetManagerEditor.BuildAssetBundleFromDiredGraph();
            //AssetManagerEditor.BuildAssetBundleFromDirectory();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("保存config文件"))
        {
            Debug.Log("按钮按下");
            AssetManagerEditor.SaveConfigToJson();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("读取config文件"))
        {
            Debug.Log("按钮按下");
            AssetManagerEditor.LoadConfigFromJson();
        }
    }
}
