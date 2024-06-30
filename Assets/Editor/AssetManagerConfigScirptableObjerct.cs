using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetManagerConfig", menuName = "AssetManager/CreateManagerConfig")]
public class AssetManagerConfigScirptableObjerct : ScriptableObject
{
    //��Դ�������汾
    public int AssetManagerVersion = 100;

    //��Դ����İ汾
    public int CurrentBuildVersion = 100;

    //�༭��ģ���½��д��
    //����ģʽ�������streamingAssets
    //Զ��ģʽ���������Զ��·�����ڸ�ʾ����ΪpersistentDataPath
    public AssetBundlePattern BuildingPattern;

    //�Ƿ�Ӧ���������
    public IncrementalBuildMode _IncrementalBuildMode;

    //AssetBundleѹ����ʽ
    public AssetBundleCompresionPattern CompressionPattern;



    //��Ҫ������ļ���
    [SerializeField]
    public DefaultAsset AssetBundleDirectory;

    //���ļ��б�����ֵʱ�����ڴ�����ļ�����������Դ·��
    public List<string> CurrentAllAssets = new List<string>();

    //��Editor����ѡ�����Դ����������������Ӧ
    public bool[] CurrentSelectedAssets;

    
    public void GetCurrentDeirectoryAllAssets()
    {
        if (AssetBundleDirectory == null)
        {
            return;
        }
        string directoryPath = AssetDatabase.GetAssetPath(AssetBundleDirectory);

        CurrentAllAssets = FindAllAssetNameFromDirectory(directoryPath);

        CurrentSelectedAssets = new bool[CurrentAllAssets.Count];
    }

    public List<string> FindAllAssetNameFromDirectory(string directoryPath)
    {
        List<string> assetPaths = new List<string>();

        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
        {
            Debug.Log("�ļ���·��������");
            return null;
        }

        //window�Դ��Ķ��ļ��н��в�������
        //���ƶ��˲�����
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

        //��ȡ�����ļ���Ϣ
        //directory�������ļ����ͣ����Բ����ȡ���ļ���
        FileInfo[] fileInfos = directoryInfo.GetFiles();

        //���з�Ԫ�����ļ�·������ӵ��б������ڴ���ļ�
        foreach (FileInfo info in fileInfos)
        {
            if (!isValidExtentionName(info.Extension))
            {
                continue;
            }
            //Assetbundle ���ֻ��Ҫ�ļ���
            string assetPath = Path.Combine(directoryPath, info.Name);
            assetPaths.Add(assetPath);
        }

        return assetPaths;
    }

    //��Ҫ�ų���Asset��չ��
    public string[] InvalidExtensionNames = new string[] { ".meta", ".cs" };

    //���������չ���İ��������ں���Ч��չ������жԱ�
    public bool isValidExtentionName(string filename)
    {
        bool isValid = true;
        foreach(string invalidName in InvalidExtensionNames)
        {
            if (filename.Contains(invalidName))
            {
                isValid = false;
                return isValid;
            }
        }
        return isValid;
    }


}
