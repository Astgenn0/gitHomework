using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public enum AssetBundleCompresionPattern
{
    LZMA,
    LZ4,
    None
}
public class AssetManagerEditor
{
    public static string AssetManagerVersion = "1.0.0";


    //�༭��ģ���½��д��
    //����ģʽ�������streamingAssets
    //Զ��ģʽ���������Զ��·�����ڸ�ʾ����ΪpersistentDataPath
    public static AssetBundlePattern BuildingPattern;

    public static AssetBundleCompresionPattern CompressionPattern;
    
    private static DefaultAsset _AssetBundleDirectory;
    //��Ҫ������ļ���
    public static DefaultAsset AssetBundleDirectory
    {
        get
        {
            return _AssetBundleDirectory;
        }
        set
        {
            //value�ؼ�����ָ����ǵ��øñ�����ֵʱ��ֵ

            if (_AssetBundleDirectory != value)
            {
                _AssetBundleDirectory = value;

                GetCurrentDeirectoryAllAssets(); 
            }
        }
    }

    

    public static string AssetBundleOutputPath;

    

    public static List<string> CurrentAllAssets = new List<string>();
    public static bool[] CurrentSelectAssets;
    //ͨ��MenuItem������Editor�����˵�
    [MenuItem(nameof(AssetManagerEditor) + "/" + nameof(BuildAssetBundle))]
    static void BuildAssetBundle()
    {
        CheckBuildingOutputPath();
        //���ַ������Զ�����б��    
        

        if (!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }

        //��ͬƽ̨���AssetBundle����ͨ�� 
        //�÷����������й��������øð����İ�
        //optionsΪnoneʱʹ��LZMAѹ��
        //ΪUncompressedAssetBundle������ѹ��
        //ChunkBasedCompression����LZ4��ѹ��
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, CheckCompressionPattern(), BuildTarget.StandaloneWindows);

        Debug.Log("AB����������");
    }

    public static List<string> GetAllSelectedAssets()
    {

        List<string> selectedAssets = new List<string>();
        if (CurrentAllAssets == null||CurrentSelectAssets.Length==0)
        {
            return null;
        }
        //��ֵΪtrue�Ķ�Ӧ�����ļ�����ӵ�Ҫ�������Դ�б���
        for (int i = 0; i < CurrentSelectAssets.Length; i++)
        {
            if (CurrentSelectAssets[i])
            {
                selectedAssets.Add(CurrentAllAssets[i]);
            }
        }
        return selectedAssets;
    }

    public static List<string> GetSelectedAssetDependencies()
    {
        List<string> dependiencies = new List<string>();

        List<string> selectedAsset =GetAllSelectedAssets();

        for (int i = 0;i< selectedAsset.Count; i++)
        {
            //����ͨ���÷�����ȡ�������飬������Ϊ�����б�L�е�һ��Ԫ��
            string[] deps = AssetDatabase.GetDependencies(selectedAsset[i],true);
            foreach(string depName in deps)
            {
                Debug.Log(depName);
            }

        }

        return dependiencies;
    }

    public static void GetCurrentDeirectoryAllAssets()
    {
        if (_AssetBundleDirectory == null)
        {
            return;
        }

        string directoryPath = AssetDatabase.GetAssetPath(_AssetBundleDirectory);

        CurrentAllAssets = FindAllAssetFromDirectory(directoryPath);

        CurrentSelectAssets = new bool[CurrentAllAssets.Count];
    }

    static BuildAssetBundleOptions CheckCompressionPattern()
    {
        BuildAssetBundleOptions option = new BuildAssetBundleOptions();
        switch (CompressionPattern)
        {
            case AssetBundleCompresionPattern.LZMA:
                option = BuildAssetBundleOptions.None;
                break;
            case AssetBundleCompresionPattern.LZ4:
                option = BuildAssetBundleOptions.ChunkBasedCompression;
                break;
            case AssetBundleCompresionPattern.None:
                option = BuildAssetBundleOptions.UncompressedAssetBundle;
                break;

        }
        return option;
    }



    [MenuItem(nameof(AssetManagerEditor) + "/" + nameof(OpenAssetManagerWindow))]
    static void OpenAssetManagerWindow()
    {
        AssetManagerEditorWindow window = (AssetManagerEditorWindow)EditorWindow.GetWindow(typeof(AssetManagerEditorWindow), true);
    }


    static void CheckBuildingOutputPath()
    {
        switch (BuildingPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                break;
            case AssetBundlePattern.Local:
                AssetBundleOutputPath = Path.Combine(Application.streamingAssetsPath, HelloWorld.MainAssetBundleName);
                break;
            case AssetBundlePattern.Remote:
                AssetBundleOutputPath = Path.Combine(Application.persistentDataPath, HelloWorld.MainAssetBundleName);
                break;
        }
        if (string.IsNullOrEmpty(AssetBundleOutputPath))
        {
            Debug.LogError("���·��Ϊ��");
            return;
        }
        else if (!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }
    }


    //��Ϊlist���������ͣ������ڷ����ж��ڲ������޸ģ��ᷴӦ����������ı�����
    //��Ϊ������ֻ�������˱�����ָ�룬���������޸ĵ���ͬһ�������ֵ
    public static List<GUID> ContrastDependenciesFromGUID(List<GUID> setsA,List<GUID> setsB)
    {
        List<GUID> newDependencies = new List<GUID>();

        //ȡ������λ
        foreach(var assetGUID in setsA)
        {
            if (setsB.Contains(assetGUID))
            {
                newDependencies.Add(assetGUID);
            }
        }

        //ȡ��ֵ
        foreach(var asserGUID in newDependencies)
        {
            if (setsA.Contains(asserGUID))
            {
                setsA.Remove(asserGUID);
            }

            if (setsB.Contains(asserGUID))
            {
                setsB.Remove(asserGUID);
            }
        }

        return newDependencies;
    }

    public static void BuildAssetBundleFormSets()
    {
        CheckBuildingOutputPath();
        if (AssetBundleDirectory == null)
        {
            Debug.LogError("���Ŀ¼������");
            return;
        }
        //��ѡ�н�Ҫ�������Դ�б� �����б�A
        List<string> selectedAssets = GetAllSelectedAssets();

        //�����б�L
        List<List<GUID>> selectedAssetsDependencies = new List<List<GUID>>();

        //��������ѡ���SourceAsset�Լ���������ü��ϱ�L
        foreach(string selectedAsset in selectedAssets)
        {
            //��ȡSourceAsset��DerivedAsset
            string[] assetDeps = AssetDatabase.GetDependencies(selectedAsset, true);

            List<GUID> assetGUIDs = new List<GUID>();

            foreach(string assetdep in assetDeps)
            {
                GUID assetGUID = AssetDatabase.GUIDFromAssetPath(assetdep);
                assetGUIDs.Add(assetGUID);
            }

            //��������SourceAsset�Լ�DrivedAsset�ļ�����ӵ������б�L��
            selectedAssetsDependencies.Add(assetGUIDs);

            for(int i = 0; i < selectedAssetsDependencies.Count; i++)
            {
                int nextIndex = i + 1;
                if (nextIndex >= selectedAssetsDependencies.Count)
                {
                    break;
                }
                Debug.Log($"�Ա�֮ǰ{selectedAssetsDependencies[i].Count}");
                Debug.Log($"�Ա�֮ǰ{selectedAssetsDependencies[nextIndex].Count}");

                for(int j = 0; j <= i; j++)
                {
                    List<GUID> newDependencies = ContrastDependenciesFromGUID(selectedAssetsDependencies[j], selectedAssetsDependencies[nextIndex]);

                    //��Snew������ӵ������б���
                    if (newDependencies != null && newDependencies.Count > 0)
                    {
                        selectedAssetsDependencies.Add(newDependencies);
                    }
                }
               
            }            
        }
        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[selectedAssetsDependencies.Count];

        for (int i = 0; i < assetBundleBuilds.Length; i++)
        {


            assetBundleBuilds[i].assetBundleName = i.ToString();

            string[] assetNames = new string[selectedAssetsDependencies[i].Count];

            List<GUID> assetGUIDs = selectedAssetsDependencies[i];
            for (int j = 0; j < assetNames.Length; j++)
            {
                string assetName = AssetDatabase.GUIDToAssetPath(assetGUIDs[j]);
                if (assetName.Contains(".cs"))
                {
                    continue;
                }
                assetNames[j] = assetName;
            }

            assetBundleBuilds[i].assetNames =assetNames;
        }
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckCompressionPattern(), BuildTarget.StandaloneWindows);

        AssetDatabase.Refresh();
    }

    public static void BuildAssetBundleFromEditorWindow()
    {
        CheckBuildingOutputPath();
        if (AssetBundleDirectory == null)
        {
            Debug.LogError("���Ŀ¼������");
            return;
        }

        //��ѡ�н�Ҫ�������Դ�б� 
        List<string> selectedAssets = GetAllSelectedAssets();

        //ѡ���˶��ٸ���Դ�������ٸ�ab��
        AssetBundleBuild[] assetBundleBuilds=new AssetBundleBuild[selectedAssets.Count];

        string directoryPath = AssetDatabase.GetAssetPath(_AssetBundleDirectory);

        for (int i = 0; i < assetBundleBuilds.Length; i++)
        {
            string bundleName = selectedAssets[i].Replace($@"{directoryPath}\", string.Empty);            

            //unity�ڵ���.prefab�ļ�ʱ���Զ�ʹ��Ԥ���嵼�������룬��assetBundle����Ԥ���壬���Իᵼ�±���
            bundleName = bundleName.Replace(".prefab", string.Empty);

            assetBundleBuilds[i].assetBundleName = bundleName;

            assetBundleBuilds[i].assetNames = new string[] { selectedAssets[i] };
        }

        
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckCompressionPattern(), BuildTarget.StandaloneWindows);

        Debug.Log(AssetBundleOutputPath);

        //ˢ��object���棬������Ǵ��������������Ҫִ��
        AssetDatabase.Refresh();

    }

    //����ļ�����������ԴΪAB��
    public static void BuildAssetBundleFromDirectory()
    {
        CheckBuildingOutputPath();
        if (AssetBundleDirectory == null)
        {
            Debug.Log("���Ŀ¼������");
            return;
        }

        AssetBundleBuild[] assetBundleBuild = new AssetBundleBuild[1];


        //��Ҫ����ľ������������������
        assetBundleBuild[0].assetBundleName = HelloWorld.ObjectAssetBundleName;

        //��Ҫ��Դ�ڹ����µ�·��
        assetBundleBuild[0].assetNames = CurrentAllAssets.ToArray();

        if (string.IsNullOrEmpty(AssetBundleOutputPath))
        {
            Debug.LogError("���·��Ϊ��");
            return;
        }
        else if (!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }

        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuild, CheckCompressionPattern(), BuildTarget.StandaloneWindows);
    }

    

    public static List<string> FindAllAssetFromDirectory(string directoryPath)
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
