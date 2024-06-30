using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using Unity.Plastic.Newtonsoft.Json;

public enum AssetBundleCompresionPattern
{
    LZMA,
    LZ4,
    None
}

//�κ�BuildOption���ڷ�ForceRebuildѡ����
//��Ĭ��Ϊ�������
public enum IncrementalBuildMode
{
    None,
    IncrementalBuild,
    ForceRebuild
}
public class AssetBundleEdge
{
    //�������������������ϵ��Node
    //һ��Node�������ö��Node
    //���NodeҲ��������һ��Node

    public List<AssetBundleNode> Nodes = new List<AssetBundleNode>();
}

public class AssetBundleNode
{
    //ÿ��Node��Ӧֻ��һ��Asset
    //�˴�string���͸�ΪGUIDҲ��һ����
    public string assetName;

    //�����ж�һ����Դ�Ƿ�ΪSourceAsset
    //���Ϊ-1˵����
    public int SourceIndex = -1;

    //��ǰNode��index�б�
    //���������outedge���д���
    public List<int> SourceIndices = new List<int>();

    //��ǰNode�����õ�Nodes
    public AssetBundleEdge OutEdge;

    //���õ�ǰNode��Nodes
    public AssetBundleEdge InEdge;

}

public class AssetBundleVersionDiffererce
{
    //������Դ��
    public List<string> AdditionAssetBundles=new List<string>();
    //�Ƴ���Դ��
    public List<string> ReducedAssetBundles=new List<string>();
}
public class AssetManagerEditor
{
    public static AssetManagerConfigScirptableObjerct AssetManagerConfig;

    public static string AssetBundleOutputPath;


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


    public static void BuildAssetBundleFromDiredGraph ()
    {
        CheckBuildingOutputPath();

        List<string> selectedAssets = GetAllSelectedAssets();

        List<AssetBundleNode> allNodes = new List<AssetBundleNode>();

        //��ǰ��ѡ�е���Դ������SourceAsset
        //�����������SourceAsset��Node
        for(int i = 0; i < selectedAssets.Count; i++)
        {
            AssetBundleNode currentNode = new AssetBundleNode();
            currentNode.assetName = selectedAssets[i];
            currentNode.SourceIndex = i;
            currentNode.SourceIndices = new List<int>() { i };
            currentNode.InEdge = new AssetBundleEdge();
            allNodes.Add(currentNode);

            GetNodesFromDependencies(currentNode, allNodes);
        }

        Dictionary<List<int>, List<AssetBundleNode>> assetBundleNodeDic = new Dictionary<List<int>, List<AssetBundleNode>>();

        foreach(AssetBundleNode node in allNodes)
        {
            bool isEquals = false;
            List<int> keylist = new List<int>();
            //�������е�key
            //ͨ�������ķ�ʽȷ����ͬlist֮��������Ƿ�һ��
            foreach(List<int> key in assetBundleNodeDic.Keys)
            {
                //�жϵ�ǰkey�ĳ����Ƿ�͵�ǰnode��SourceIndices�������
                isEquals = node.SourceIndices.Count == key.Count && node.SourceIndices.All(p => key.Any(k => k.Equals(p)));

                if (isEquals)
                {
                    keylist = key;
                    break;
                }

            }
            if (!isEquals)
            {
                keylist = node.SourceIndices;
                assetBundleNodeDic.Add(node.SourceIndices, new List<AssetBundleNode>());
            }

            assetBundleNodeDic[keylist].Add(node);
        }

        AssetBundleBuild[] assetBundleBuilds = new AssetBundleBuild[assetBundleNodeDic.Count];
        int buildindex = 0;

        foreach(List<int> key in assetBundleNodeDic.Keys)
        {
            assetBundleBuilds[buildindex].assetBundleName = buildindex.ToString();

            List<string> assetNames = new List<string>();
            //��һ��ѭ�����Ǵ�ͬһ����ֵ���л�ȡNode
            //Ҳ���Ǵ�sourseIndices��ͬ�ļ����У���ȡ��Ӧ��Node�����Asset
            foreach(AssetBundleNode node in assetBundleNodeDic[key])
            {
                assetNames.Add(node.assetName);
            }

            assetBundleBuilds[buildindex].assetNames = assetNames.ToArray();

            buildindex++;
        }

        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckCompressionPattern(), BuildTarget.StandaloneWindows);

        BuildAssetBundleHashTable(assetBundleBuilds);
        CopyAssetBundleToVersionFolder();

        AssetManagerConfig.CurrentBuildVersion++;

        AssetDatabase.Refresh();
    }

    public static void GetNodesFromDependencies(AssetBundleNode lastNode,List<AssetBundleNode> currentAllNodes)
    {
        string[] assetNames = AssetDatabase.GetDependencies(lastNode.assetName, false);
        if (assetNames.Length > 0)
        {
            lastNode.OutEdge = new AssetBundleEdge();
        }
    }
    public static void LoadConfig(AssetManagerEditorWindow window)
    {
        if (AssetManagerConfig == null)
        {
            //ʹ��AssetDataBase������Դ��ֻ��Ҫ����AssetĿ¼�µ�·������
            AssetManagerConfig = AssetDatabase.LoadAssetAtPath<AssetManagerConfigScirptableObjerct>("Assets/Editor/AssetManagerConfig.asset");
            window.VersionString = AssetManagerConfig.AssetManagerVersion.ToString();

            for(int i = window.VersionString.Length; i >= 1; i--)
            {
                window.VersionString = window.VersionString.Insert(i,".");
            }

            window.editorWindowDirectory = AssetManagerConfig.AssetBundleDirectory;
        }
    }
    public static void LoadWindowConfig(AssetManagerEditorWindow window)
    {
        if (window.WindowConfig == null)
        {
            //ʹ��AssetDataBase������Դ��ֻ��Ҫ����AssetĿ¼�µ�·������
            window.WindowConfig = AssetDatabase.LoadAssetAtPath<AssetManagetEditorWindowConfigSO>("Assets/Editor/AssetManagerEditorWindowConfig.asset");
            window.VersionString = AssetManagerConfig.AssetManagerVersion.ToString();

            window.WindowConfig.TitleTextStyle = new GUIStyle();
            window.WindowConfig.TitleTextStyle.fontSize = 26;
            window.WindowConfig.TitleTextStyle.normal.textColor = Color.red;
            window.WindowConfig.TitleTextStyle.alignment = TextAnchor.MiddleCenter;

            window.WindowConfig.VersionTextstyle = new GUIStyle();
            window.WindowConfig.VersionTextstyle.fontSize = 20;
            window.WindowConfig.VersionTextstyle.normal.textColor = Color.gray;
            window.WindowConfig.VersionTextstyle.alignment = TextAnchor.MiddleRight;
        }
    }

    public static void LoadConfigFromJson()
    {
        string configPath= Path.Combine(Application.dataPath, "Editor/AssetManagerConfig.amc");

        string configString = File.ReadAllText(configPath);

        JsonUtility.FromJsonOverwrite(configString,AssetManagerConfig);


    }
    public static void SaveConfigToJson()
    {
        if (AssetManagerConfig!=null)
        {
            string configString = JsonUtility.ToJson(AssetManagerConfig);
            string outputPath = Path.Combine(Application.dataPath, "Editor/AssetManagerConfig.amc");
            Debug.Log(outputPath);

            File.WriteAllText(outputPath, configString);

            AssetDatabase.Refresh();
        }
    }

    
    [MenuItem(nameof(AssetManagerEditor) + "/" + nameof(CreateConfig))]
    static void CreateConfig()
    {
        //����scriptable���͵�ʵ��
        //ScriptableObject���͵���������������Json�н�ĳ����ʵ�����Ĺ���
        AssetManagerConfigScirptableObjerct config = ScriptableObject.CreateInstance<AssetManagerConfigScirptableObjerct>();

        AssetDatabase.CreateAsset(config, "Assets/Editor/AssetManagerConfig.asset");

        AssetDatabase.SaveAssets();

        AssetDatabase.Refresh();
    }


    //������һ���������е�GUID�б���MD5�㷨���ܹ���hash��
    //���GUI�б������仯���Լ������㷨�Ͳ���û�б仯
    //�����ܵõ���ͬ���ַ���
    static string ComputeAssetSetSignature(IEnumerable<string> assetNames)
    {
        var assetGUIDs = assetNames.Select(AssetDatabase.AssetPathToGUID);

        MD5 currentMD5 = MD5.Create();
        foreach(var assetDUID in assetGUIDs.OrderBy(x=>x))
        {
            byte[] bytes = Encoding.ASCII.GetBytes(assetDUID);

            //ʹ��MD5�㷨�����ֽ�����
            currentMD5.TransformBlock(bytes,0,bytes.Length,null,0);
        }

        currentMD5.TransformFinalBlock(new byte[0], 0, 0);

        return BytesToHexString(currentMD5.Hash);
    }


    //byteתΪ16�����ַ���
    static string BytesToHexString(byte[] bytes)
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach(var aByte in bytes){
            stringBuilder.Append(aByte.ToString("x2"));
        }

        return stringBuilder.ToString();
    }

    static string[] BuildAssetBundleHashTable(AssetBundleBuild[] assetBundleBuilds)
    {
        //��ĳ�����AssetBundle����������һ��
        string[] assetBundleHashs = new string[assetBundleBuilds.Length];

        for(int i = 0; i < assetBundleBuilds.Length; i++)
        {
            string assetBundlePath = Path.Combine(AssetBundleOutputPath, assetBundleBuilds[i].assetBundleName);

            FileInfo info = new FileInfo(assetBundlePath);

            //���м�¼����AssetBundle�ļ��ĳ����Լ������ݵ�MD5Hashֵ
            assetBundleHashs[i] = $"{info.Length}_{assetBundleBuilds[i].assetBundleName}";
        }

        string hashString = JsonConvert.SerializeObject(assetBundleHashs);
        string hashFilePath = Path.Combine(AssetBundleOutputPath, "AssetBundleHashs");

        File.WriteAllText(hashFilePath, hashString);

        return assetBundleHashs;
    }

    public static List<string> GetAllSelectedAssets()
    {
        List<string> selectedAssets = new List<string>();
        if (AssetManagerConfig.CurrentAllAssets == null|| AssetManagerConfig.CurrentSelectedAssets.Length==0)
        {
            return null;
        }
        //��ֵΪtrue�Ķ�Ӧ�����ļ�����ӵ�Ҫ�������Դ�б���
        for (int i = 0; i < AssetManagerConfig.CurrentSelectedAssets.Length; i++)
        {
            if (AssetManagerConfig.CurrentSelectedAssets[i])
            {
                selectedAssets.Add(AssetManagerConfig.CurrentAllAssets[i]);
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

    static BuildAssetBundleOptions CheckCompressionPattern()
    {
        BuildAssetBundleOptions option = new BuildAssetBundleOptions();
        switch (AssetManagerConfig.CompressionPattern)
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

        

        switch (AssetManagerConfig.BuildingPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                break;
            case AssetBundlePattern.Local:
                AssetBundleOutputPath = Path.Combine(Application.streamingAssetsPath, "Local", HelloWorld.MainAssetBundleName);
                break;
            case AssetBundlePattern.Remote:
                AssetBundleOutputPath = Path.Combine(Application.persistentDataPath,"Remote", HelloWorld.MainAssetBundleName);
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

    static AssetBundleVersionDiffererce ContrastAssetBundleVersion(string[] oldVersionAssets,string[] newVersionAssets)
    {
        AssetBundleVersionDiffererce differerce = new AssetBundleVersionDiffererce();

        foreach(var assetName in oldVersionAssets)
        {
            if (!newVersionAssets.Contains(assetName))
            {
                differerce.ReducedAssetBundles.Add(assetName);
            }
        }

        foreach(var assetName in newVersionAssets)
        {
            if (!oldVersionAssets.Contains(assetName))
            {
                differerce.AdditionAssetBundles.Add(assetName);
            }
        }

        return differerce;
    }
    
    public static void BuildAssetBundleFormSets()
    {

        CheckBuildingOutputPath();
        if (AssetManagerConfig.AssetBundleDirectory == null)
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
            string[] assetNamesArray = assetNames.ToArray();

            assetBundleBuilds[i].assetBundleName = ComputeAssetSetSignature(assetNamesArray);

            assetBundleBuilds[i].assetNames =assetNamesArray;
        }
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckIncrementalBuildMode(), BuildTarget.StandaloneWindows);

        string[] currentVersionAssetHashs= BuildAssetBundleHashTable(assetBundleBuilds);

        CopyAssetBundleToVersionFolder();
        //GetVersionDifference(currentVersionAssetHashs);
        
        AssetManagerConfig.CurrentBuildVersion++;

        AssetDatabase.Refresh();
    }

    static void CopyAssetBundleToVersionFolder()
    {
        string versionString = AssetManagerConfig.CurrentBuildVersion.ToString();
        for (int i = versionString.Length - 1; i >= 1; i--)
        {
            versionString = versionString.Insert(i, ".");
        }

        string assetBundleVersionPath = Path.Combine(Application.streamingAssetsPath, versionString, HelloWorld.MainAssetBundleName);
        if (!Directory.Exists(assetBundleVersionPath))
        {
            Directory.CreateDirectory(assetBundleVersionPath);
        }

        string[] assetNames = ReadAssetBundleHashTable(AssetBundleOutputPath);

        //���ƹ�ϣ��
        string hashTableOriginPath = Path.Combine(AssetBundleOutputPath, "AssetBundleHashs");
        string hashTableVersionPath = Path.Combine(assetBundleVersionPath, "AssetBundleHashs");
        File.Copy(hashTableOriginPath, hashTableVersionPath);

        //��������
        string mainBundleOriginPath = Path.Combine(AssetBundleOutputPath, HelloWorld.MainAssetBundleName);
        string mainBundleVersionPath = Path.Combine(assetBundleVersionPath, HelloWorld.MainAssetBundleName);
        File.Copy(mainBundleOriginPath, mainBundleVersionPath);

        foreach(var assetName in assetNames)
        {
            string assetHashName = assetName.Substring(assetName.IndexOf("_")+1);

            string assetOriginPath = Path.Combine(AssetBundleOutputPath,assetHashName);
            //fileinfo.Name�ǰ�������չ�����ļ���
            string assetVersionPath = Path.Combine(assetBundleVersionPath, assetHashName);
            //fileInfo.FullName�ǰ�����Ŀ¼���ļ������ļ�����·��
            File.Copy(assetOriginPath, assetVersionPath,true);
        }
    }

    static BuildAssetBundleOptions CheckIncrementalBuildMode()
    {
        BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

        switch (AssetManagerConfig._IncrementalBuildMode)
        {
            case IncrementalBuildMode.None:
                options = BuildAssetBundleOptions.None;
                break;
            case IncrementalBuildMode.IncrementalBuild:
                options = BuildAssetBundleOptions.DeterministicAssetBundle;
                break;
            case IncrementalBuildMode.ForceRebuild:
                options = BuildAssetBundleOptions.ForceRebuildAssetBundle;
                break;
        }
        return options;
    }

    static string[] ReadAssetBundleHashTable(string outputPath)
    {

        string VersionHashTablePath = Path.Combine(outputPath, "AssetBundleHashs");

        string VersionHashString = File.ReadAllText(VersionHashTablePath);

        string[] VersionAssetHashs = JsonConvert.DeserializeObject<string[]>(VersionHashString);

        return VersionAssetHashs;
    }

    static void GetVersionDifference(string[] currentAssetHashs)
    {
        if (AssetManagerConfig.CurrentBuildVersion >= 101)
        {
            int lastVersion = AssetManagerConfig.CurrentBuildVersion - 1;
            string versionString = lastVersion.ToString();
            for (int i = versionString.Length - 1; i >= 1; i--)
            {
                versionString = versionString.Insert(i, ".");
            }

            var lastOutputPath = Path.Combine(Application.streamingAssetsPath, versionString, HelloWorld.MainAssetBundleName);

            string[] lastVersionAssetHashs = ReadAssetBundleHashTable(lastOutputPath);

            AssetBundleVersionDiffererce differerce = ContrastAssetBundleVersion(lastVersionAssetHashs, currentAssetHashs);

            foreach (var assetName in differerce.AdditionAssetBundles)
            {
                Debug.Log($"��ǰ�汾������Դ{assetName}");
            }
            foreach (var assetName in differerce.ReducedAssetBundles)
            {
                Debug.Log($"��ǰ�汾������Դ{assetName}");
            }
        }
    }



    public static void BuildAssetBundleFromEditorWindow()
    {
        CheckBuildingOutputPath();
        if (AssetManagerConfig.AssetBundleDirectory == null)
        {
            Debug.LogError("���Ŀ¼������");
            return;
        }

        //��ѡ�н�Ҫ�������Դ�б� 
        List<string> selectedAssets = GetAllSelectedAssets();

        //ѡ���˶��ٸ���Դ�������ٸ�ab��
        AssetBundleBuild[] assetBundleBuilds=new AssetBundleBuild[selectedAssets.Count];

        string directoryPath = AssetDatabase.GetAssetPath(AssetManagerConfig.AssetBundleDirectory);

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
        if (AssetManagerConfig.AssetBundleDirectory == null)
        {
            Debug.Log("���Ŀ¼������");
            return;
        }

        AssetBundleBuild[] assetBundleBuild = new AssetBundleBuild[1];


        //��Ҫ����ľ������������������
        assetBundleBuild[0].assetBundleName = HelloWorld.ObjectAssetBundleName;

        //��Ҫ��Դ�ڹ����µ�·��
        assetBundleBuild[0].assetNames = AssetManagerConfig.CurrentAllAssets.ToArray();

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


    //���������չ���İ��������ں���Ч��չ������жԱ�
    public bool isValidExtentionName(string filename)
    {
        bool isValid = true;
        foreach (string invalidName in AssetManagerConfig.InvalidExtensionNames)
        {
            if (filename.Contains(invalidName))
            {
                isValid = false;
                return isValid;
            }
        }
        return isValid;
    }

    public List<string> FindAllAssetNameFromDirectory(string directoryPath)
    {
        List<string> assetPaths = new List<string>();

        if (!string.IsNullOrEmpty(directoryPath) || !Directory.Exists(directoryPath))
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
                //Assetbundle ���ֻ��Ҫ�ļ���
                string assetPath = Path.Combine(directoryPath, info.Name);
                assetPaths.Add(assetPath);
            }
        }

        return assetPaths;
    }

}
