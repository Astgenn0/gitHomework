using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AssetManagerConfig", menuName = "AssetManager/CreateManagerConfig")]
public class AssetManagerConfigScirptableObjerct : ScriptableObject
{
    //资源管理器版本
    public int AssetManagerVersion = 100;

    //资源打包的版本
    public int CurrentBuildVersion = 100;

    //编辑器模拟下进行打包
    //本地模式，打包到streamingAssets
    //远端模式打包到任意远端路径，在该示例中为persistentDataPath
    public AssetBundlePattern BuildingPattern;

    //是否应用增量打包
    public IncrementalBuildMode _IncrementalBuildMode;

    //AssetBundle压缩格式
    public AssetBundleCompresionPattern CompressionPattern;



    //需要打包的文件夹
    [SerializeField]
    public DefaultAsset AssetBundleDirectory;

    //当文件夹变量赋值时，用于储存该文件夹下所有资源路径
    public List<string> CurrentAllAssets = new List<string>();

    //在Editor界面选择的资源，以数组的索引相对应
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
            if (!isValidExtentionName(info.Extension))
            {
                continue;
            }
            //Assetbundle 打包只需要文件名
            string assetPath = Path.Combine(directoryPath, info.Name);
            assetPaths.Add(assetPath);
        }

        return assetPaths;
    }

    //需要排除的Asset拓展名
    public string[] InvalidExtensionNames = new string[] { ".meta", ".cs" };

    //传入包含拓展名的包名，用于和无效拓展名组进行对比
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
