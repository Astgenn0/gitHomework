using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetManagerEditor
{
    public static string AssetManagerVersion = "1.0.0";


    //编辑器模拟下进行打包
    //本地模式，打包到streamingAssets
    //远端模式打包到任意远端路径，在该示例中为persistentDataPath
    public static AssetBundlePattern BuildingPattern;

    //需要打包的文件夹
    public static DefaultAsset AssetBundleDirectory;



    public static string AssetBundleOutputPath;

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
        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        Debug.Log("AB包打包已完成");
    }



    [MenuItem(nameof(AssetManagerEditor) + "/" + nameof(OpenAssetManagerWindow))]
    static void OpenAssetManagerWindow()
    {
        Rect windowRect = new Rect(0, 0, 500, 500);

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
        string directoryPath = AssetDatabase.GetAssetPath(AssetBundleDirectory);


        string[] assetPaths = FindAllAssetFromDirectory(directoryPath).ToArray();

        AssetBundleBuild[] assetBundleBuild = new AssetBundleBuild[1];


        //需要打包的具体包名，而非主包名
        assetBundleBuild[0].assetBundleName = HelloWorld.ObjectAssetBundleName;

        //需要资源在工程下的路径
        assetBundleBuild[0].assetNames = assetPaths;

        if (string.IsNullOrEmpty(AssetBundleOutputPath))
        {
            Debug.LogError("输出路径为空");
            return;
        }
        else if (!Directory.Exists(AssetBundleOutputPath))
        {
            Directory.CreateDirectory(AssetBundleOutputPath);
        }



        BuildPipeline.BuildAssetBundles(AssetBundleOutputPath, assetBundleBuild, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
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
