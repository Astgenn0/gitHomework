using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetManagerEditor
{
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
}
