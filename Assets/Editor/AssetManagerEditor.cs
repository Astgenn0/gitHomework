using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetManagerEditor
{
    //通过MenuItem，声明Editor顶部菜单
    [MenuItem(nameof(AssetManagerEditor)+"/"+nameof(BuildAssetBundle))]
   static void BuildAssetBundle()
    {
        //在字符串中自动插入斜杠    
        string outputPath = Path.Combine(Application.persistentDataPath, "Bundles");

        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        //不同平台间的AssetBundle不可通用 
        //该方法会打包所有工程内配置该包名的包
        //options为none时使用LZMA压缩
        //为UncompressedAssetBundle不进行压缩
        //ChunkBasedCompression进行LZ4块压缩
        BuildPipeline.BuildAssetBundles(outputPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

        Debug.Log("AB包打包已完成");
    }
}
