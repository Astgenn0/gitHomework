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

//任何BuildOption处于非ForceRebuild选项下
//都默认为增量打包
public enum IncrementalBuildMode
{
    None,
    IncrementalBuild,
    ForceRebuild
}

public class AssetBundleVersionDiffererce
{
    //新增资源包
    public List<string> AdditionAssetBundles=new List<string>();
    //移除资源包
    public List<string> ReducedAssetBundles=new List<string>();
}
public class AssetManagerEditor
{
    public static string AssetManagerVersion = "1.0.0";


    //编辑器模拟下进行打包
    //本地模式，打包到streamingAssets
    //远端模式打包到任意远端路径，在该示例中为persistentDataPath
    public static AssetBundlePattern BuildingPattern;

    public static IncrementalBuildMode _IncrementalBuildMode;

    public static AssetBundleCompresionPattern CompressionPattern;
    
    private static DefaultAsset _AssetBundleDirectory;
    //需要打包的文件夹
    public static DefaultAsset AssetBundleDirectory
    {
        get
        {
            return _AssetBundleDirectory;
        }
        set
        {
            //value关键字所指向的是调用该变量赋值时的值

            if (_AssetBundleDirectory != value)
            {
                _AssetBundleDirectory = value;

                GetCurrentDeirectoryAllAssets(); 
            }
        }
    }   

    public static string AssetBundleOutputPath;

    
    public static List<string> CurrentAllAssets = new List<string>();
    public static bool[] CurrentSelectedAssets;

    //资源打包的版本
    public static int CurrentBuildVersion=100;
    //通过MenuItem，声明Editor顶部菜单
    [MenuItem(nameof(AssetManagerEditor) + "/" + nameof(BuildAssetBundle))]
    static void BuildAssetBundle()
    {
        CheckBuildingOutputPath();
        //在字符串中自动插入斜杠    
        

        if (!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }

        //不同平台间的AssetBundle不可通用 
        //该方法会打包所有工程内配置该包名的包
        //options为none时使用LZMA压缩
        //为UncompressedAssetBundle不进行压缩
        //ChunkBasedCompression进行LZ4块压缩
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, CheckCompressionPattern(), BuildTarget.StandaloneWindows);

        Debug.Log("AB包打包已完成");
    }


    //返回由一个包中所有的GUID列表经过MD5算法加密过的hash码
    //如果GUI列表不发生变化，以及加密算法和参数没有变化
    //总是能得到相同的字符串
    static string ComputeAssetSetSignature(IEnumerable<string> assetNames)
    {
        var assetGUIDs = assetNames.Select(AssetDatabase.AssetPathToGUID);

        MD5 currentMD5 = MD5.Create();
        foreach(var assetDUID in assetGUIDs.OrderBy(x=>x))
        {
            byte[] bytes = Encoding.ASCII.GetBytes(assetDUID);

            //使用MD5算法加密字节数组
            currentMD5.TransformBlock(bytes,0,bytes.Length,null,0);
        }

        currentMD5.TransformFinalBlock(new byte[0], 0, 0);

        return BytesToHexString(currentMD5.Hash);
    }


    //byte转为16进制字符串
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
        //表的长度与AssetBundle的数量保持一致
        string[] assetBundleHashs = new string[assetBundleBuilds.Length];

        for(int i = 0; i < assetBundleBuilds.Length; i++)
        {
            string assetBundlePath = Path.Combine(AssetBundleOutputPath, assetBundleBuilds[i].assetBundleName);

            FileInfo info = new FileInfo(assetBundlePath);

            //表中记录的是AssetBundle文件的长度以及其内容的MD5Hash值
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
        if (CurrentAllAssets == null||CurrentSelectedAssets.Length==0)
        {
            return null;
        }
        //将值为true的对应索引文件，添加到要打包的资源列表中
        for (int i = 0; i < CurrentSelectedAssets.Length; i++)
        {
            if (CurrentSelectedAssets[i])
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
            //所有通过该方法读取到的数组，可以视为集合列表L中的一个元素
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

        CurrentSelectedAssets = new bool[CurrentAllAssets.Count];
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
                AssetBundleOutputPath = Path.Combine(Application.streamingAssetsPath, "Local", HelloWorld.MainAssetBundleName);
                break;
            case AssetBundlePattern.Remote:
                AssetBundleOutputPath = Path.Combine(Application.persistentDataPath,"Remote", HelloWorld.MainAssetBundleName);
                break;
        }
        if (string.IsNullOrEmpty(AssetBundleOutputPath))
        {
            Debug.LogError("输出路径为空");
            return;
        }
        else if (!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }
    }


    //因为list是引用类型，所以在方法中对于参数的修改，会反应到传入参数的变量上
    //因为本质上只是引用了变量的指针，所以最终修改的是同一个对象的值
    public static List<GUID> ContrastDependenciesFromGUID(List<GUID> setsA,List<GUID> setsB)
    {
        List<GUID> newDependencies = new List<GUID>();

        //取交集部位
        foreach(var assetGUID in setsA)
        {
            if (setsB.Contains(assetGUID))
            {
                newDependencies.Add(assetGUID);
            }
        }

        //取差值
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
        if (AssetBundleDirectory == null)
        {
            Debug.LogError("打包目录不存在");
            return;
        }
        //被选中将要打包的资源列表 ，即列表A
        List<string> selectedAssets = GetAllSelectedAssets();

        //集合列表L
        List<List<GUID>> selectedAssetsDependencies = new List<List<GUID>>();

        //遍历所有选择的SourceAsset以及依赖，获得集合表L
        foreach(string selectedAsset in selectedAssets)
        {
            //获取SourceAsset的DerivedAsset
            string[] assetDeps = AssetDatabase.GetDependencies(selectedAsset, true);

            List<GUID> assetGUIDs = new List<GUID>();

            foreach(string assetdep in assetDeps)
            {
                GUID assetGUID = AssetDatabase.GUIDFromAssetPath(assetdep);
                assetGUIDs.Add(assetGUID);
            }

            //将包含了SourceAsset以及DrivedAsset的集合添加到集合列表L中
            selectedAssetsDependencies.Add(assetGUIDs);

            for(int i = 0; i < selectedAssetsDependencies.Count; i++)
            {
                int nextIndex = i + 1;
                if (nextIndex >= selectedAssetsDependencies.Count)
                {
                    break;
                }
                Debug.Log($"对比之前{selectedAssetsDependencies[i].Count}");
                Debug.Log($"对比之前{selectedAssetsDependencies[nextIndex].Count}");

                for(int j = 0; j <= i; j++)
                {
                    List<GUID> newDependencies = ContrastDependenciesFromGUID(selectedAssetsDependencies[j], selectedAssetsDependencies[nextIndex]);

                    //将Snew集合添加到集合列表中
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
        
        CurrentBuildVersion++;

        AssetDatabase.Refresh();
    }

    static void CopyAssetBundleToVersionFolder()
    {
        string versionString = CurrentBuildVersion.ToString();
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

        //复制哈希表
        string hashTableOriginPath = Path.Combine(AssetBundleOutputPath, "AssetBundleHashs");
        string hashTableVersionPath = Path.Combine(assetBundleVersionPath, "AssetBundleHashs");
        File.Copy(hashTableOriginPath, hashTableVersionPath);

        //复制主包
        string mainBundleOriginPath = Path.Combine(AssetBundleOutputPath, HelloWorld.MainAssetBundleName);
        string mainBundleVersionPath = Path.Combine(assetBundleVersionPath, HelloWorld.MainAssetBundleName);
        File.Copy(mainBundleOriginPath, mainBundleVersionPath);

        foreach(var assetName in assetNames)
        {
            string assetHashName = assetName.Substring(assetName.IndexOf("_")+1);

            string assetOriginPath = Path.Combine(AssetBundleOutputPath,assetHashName);
            //fileinfo.Name是包含了拓展名的文件名
            string assetVersionPath = Path.Combine(assetBundleVersionPath, assetHashName);
            //fileInfo.FullName是包含了目录和文件名的文件完整路径
            File.Copy(assetOriginPath, assetVersionPath,true);
        }
    }

    static BuildAssetBundleOptions CheckIncrementalBuildMode()
    {
        BuildAssetBundleOptions options = BuildAssetBundleOptions.None;

        switch (_IncrementalBuildMode)
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
        if (CurrentBuildVersion >= 101)
        {
            int lastVersion = CurrentBuildVersion - 1;
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
                Debug.Log($"当前版本新增资源{assetName}");
            }
            foreach (var assetName in differerce.ReducedAssetBundles)
            {
                Debug.Log($"当前版本减少资源{assetName}");
            }
        }
    }

    public static void BuildAssetBundleFromEditorWindow()
    {
        CheckBuildingOutputPath();
        if (AssetBundleDirectory == null)
        {
            Debug.LogError("打包目录不存在");
            return;
        }

        //被选中将要打包的资源列表 
        List<string> selectedAssets = GetAllSelectedAssets();

        //选中了多少个资源则打包多少个ab包
        AssetBundleBuild[] assetBundleBuilds=new AssetBundleBuild[selectedAssets.Count];

        string directoryPath = AssetDatabase.GetAssetPath(_AssetBundleDirectory);

        for (int i = 0; i < assetBundleBuilds.Length; i++)
        {
            string bundleName = selectedAssets[i].Replace($@"{directoryPath}\", string.Empty);            

            //unity在导入.prefab文件时会自动使用预制体导入器导入，而assetBundle不是预制体，所以会导致报错
            bundleName = bundleName.Replace(".prefab", string.Empty);

            assetBundleBuilds[i].assetBundleName = bundleName;

            assetBundleBuilds[i].assetNames = new string[] { selectedAssets[i] };
        }

        
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuilds, CheckCompressionPattern(), BuildTarget.StandaloneWindows);

        Debug.Log(AssetBundleOutputPath);

        //刷新object界面，如果不是打包到工程内则不需要执行
        AssetDatabase.Refresh();

    }

    //打包文件夹内所有资源为AB包
    public static void BuildAssetBundleFromDirectory()
    {
        CheckBuildingOutputPath();
        if (AssetBundleDirectory == null)
        {
            Debug.Log("打包目录不存在");
            return;
        }

        AssetBundleBuild[] assetBundleBuild = new AssetBundleBuild[1];


        //需要打包的具体包名，而非主包名
        assetBundleBuild[0].assetBundleName = HelloWorld.ObjectAssetBundleName;

        //需要资源在工程下的路径
        assetBundleBuild[0].assetNames = CurrentAllAssets.ToArray();

        if (string.IsNullOrEmpty(AssetBundleOutputPath))
        {
            Debug.LogError("输出路径为空");
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
            Debug.Log("文件夹路径不存在");
            return null;
        }

        //window自带的对文件夹进行操作的类
        //在移动端不适用
        DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

        //获取所有文件信息
        //directory不属于文件类型，所以不会获取子文件夹
        FileInfo[] fileInfos = directoryInfo.GetFiles();

        //所有非元数据文件路径都添加到列表中用于打包文件
        foreach (FileInfo info in fileInfos)
        {
            if (info.Extension.Contains(".meta"))
            {
                continue;
            }

            //Assetbundle 打包只需要文件名
            string assetPath = Path.Combine(directoryPath, info.Name);
            assetPaths.Add(assetPath);
        }

        return assetPaths;
    }
}
