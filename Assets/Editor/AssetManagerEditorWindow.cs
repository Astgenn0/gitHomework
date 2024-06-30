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

    //ÿ�����̷����޸�ʱ����ø÷���
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
    //�����������ÿ����Ⱦ֡����,����������Ⱦui����
    private void OnGUI()
    {
        //Ĭ�ϴ�ֱ�Ű�
        GUILayout.Space(20);

        #region Title��������

        GUILayout.Label(nameof(AssetManagerEditor), WindowConfig.TitleTextStyle);

        #endregion

        GUILayout.Label(VersionString, WindowConfig.VersionTextstyle);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig.BuildingPattern = (AssetBundlePattern)EditorGUILayout.EnumPopup("���ģʽ",AssetManagerEditor.AssetManagerConfig.BuildingPattern);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig._IncrementalBuildMode = (IncrementalBuildMode)EditorGUILayout.EnumPopup("�������", AssetManagerEditor.AssetManagerConfig._IncrementalBuildMode);

        GUILayout.Space(20);
        AssetManagerEditor.AssetManagerConfig.CompressionPattern = (AssetBundleCompresionPattern)EditorGUILayout.EnumPopup("ѹ����ʽ", AssetManagerEditor.AssetManagerConfig.CompressionPattern);

        //�����Դѡ��
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


        if (GUILayout.Button("���AssetBundle"))
        {
            Debug.Log(AssetManagerEditor.AssetManagerConfig.CurrentBuildVersion);
            AssetManagerEditor.BuildAssetBundleFromDiredGraph();
            //AssetManagerEditor.BuildAssetBundleFromDirectory();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("����config�ļ�"))
        {
            Debug.Log("��ť����");
            AssetManagerEditor.SaveConfigToJson();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("��ȡconfig�ļ�"))
        {
            Debug.Log("��ť����");
            AssetManagerEditor.LoadConfigFromJson();
        }
    }
}
