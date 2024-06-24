using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HelloWorld : MonoBehaviour
{
    AssetBundle SampleBundle;
    GameObject SampleGameobject;

    public  Button LoadAssetBundleButton;
    public Button LoadAssetButton;
    public Button UnLoadFalseButton;
    public Button UnLoadTrueButton;
    // Start is called before the first frame update
    void Start()
    {
        LoadAssetBundleButton.onClick.AddListener(LoadAssetBundle);

        LoadAssetButton.onClick.AddListener(LoadAsset);

        UnLoadFalseButton.onClick.AddListener(() => { UnloadAssetBundle(false); });

        UnLoadTrueButton.onClick.AddListener(() => { UnloadAssetBundle(true); });
    }

    void LoadAssetBundle()
    {
        string mainBundleName = "Bundles";
        //通过外部路径加载AB包
        //persistentDataPath在移动端可读可写
        //远程下载的AB包都可以放置在该路径下
        string assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);

        //加载清单捆绑包
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //manifest文件本身是明文储存给开发者看的
        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));

        foreach (var depAssetBundleName in assetBundleManifest.GetAllDependencies("prefebs"))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);
            //如果不使用依赖包内资源，可以不用变量储存,但仍会存于内存中
            AssetBundle.LoadFromFile(assetBundlePath);
        }

        assetBundlePath = Path.Combine(Application.persistentDataPath, "Bundles", "prefebs");
        //AB包加载可以允许加载工程路径外的路径
        SampleBundle = AssetBundle.LoadFromFile(assetBundlePath);
    }

    void LoadAsset()
    {
        GameObject cubeObject = SampleBundle.LoadAsset<GameObject>("Cube");
        SampleGameobject=Instantiate(cubeObject); 
    }

    void UnloadAssetBundle(bool isTrue)
    {
        Debug.Log(isTrue);
        //当前帧回收
        DestroyImmediate(SampleGameobject);
        SampleBundle.Unload(isTrue);

        Resources.UnloadUnusedAssets();
    }
        
}
