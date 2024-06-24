using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetManagerEditor
{
    public static string AssetManagerVersion = "1.0.0";
    
    //��Ҫ������ļ���
    public static DefaultAsset AssetBundleDirectory;

    public static string MainAssetBundleName = "SampleAssetBundle";

    public static string AssetBundleOutputPath = Path.Combine(Application.persistentDataPath, MainAssetBundleName);

    //ͨ��MenuItem������Editor�����˵�
    [MenuItem(nameof(AssetManagerEditor)+"/"+nameof(BuildAssetBundle))]
   static void BuildAssetBundle()
    {
        //���ַ������Զ�����б��    
        string outputPath = Path.Combine(Application.persistentDataPath, "Bundles");

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        //��ͬƽ̨���AssetBundle����ͨ�� 
        //�÷����������й��������øð����İ�
        //optionsΪnoneʱʹ��LZMAѹ��
        //ΪUncompressedAssetBundle������ѹ��
        //ChunkBasedCompression����LZ4��ѹ��
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        Debug.Log("AB����������");
    }



    [MenuItem(nameof(AssetManagerEditor) + "/" + nameof(OpenAssetManagerWindow))]
    static void OpenAssetManagerWindow()
    {
        Rect windowRect = new Rect(0, 0,500, 500);

        AssetManagerEditorWindow window=(AssetManagerEditorWindow) EditorWindow.GetWindow(typeof(AssetManagerEditorWindow),true);
    }


    //����ļ�����������ԴΪAB��
    public static void BuildAssetBundleFromDirectory()
    {
        if (AssetBundleDirectory == null)
        {
            Debug.Log("���Ŀ¼������");
            return;
        }
        string directoryPath = AssetDatabase.GetAssetPath(AssetBundleDirectory);


        string[] assetPaths = FindAllAssetFromDirectory(directoryPath).ToArray();

        AssetBundleBuild[] assetBundleBuild = new AssetBundleBuild[1];


        //��Ҫ����ľ������������������
        assetBundleBuild[0].assetBundleName = "rescoresbundle";

        //��Ҫ��Դ�ڹ����µ�·��
        assetBundleBuild[0].assetNames = assetPaths;

        if (string.IsNullOrEmpty(AssetBundleOutputPath))
        {
            Debug.LogError("���·��Ϊ��");
            return;
        }
        else if (!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }

        

        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuild,BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
    }

    public  static List<string> FindAllAssetFromDirectory(string directoryPath)
    {
        List<string> assetPaths = new List<string>();

        if(!string.IsNullOrEmpty(directoryPath)&&!Directory.Exists(directoryPath))
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
        foreach(FileInfo info in fileInfos)
        {
            if (info.Extension.Contains(".meta"))
            {
                continue;
            }

            //Assetbundle ���ֻ��Ҫ�ļ���
            string assetPath = Path.Combine(directoryPath, info.Name);
            assetPaths.Add(assetPath);
        }

        return assetPaths;
    }
}
