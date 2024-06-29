using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



public enum AssetBundlePattern
{
    //编辑器模拟加载，应该使用AssetDatabase进行资源加载，而不用进行打包
    EditorSimulation,
    //本地加载模式，应打包到本地路径或StreamingAssets路径下,从该路径加载
    Local,
    //远端加载模式，应打包到任意的资源服务器地址，然后通过网络进行下载
    //下载到沙盒路径persistentDataPath后，再进行加载
    Remote
}
public class HelloWorld : MonoBehaviour
{
    public AssetBundlePattern LoadPattern;

    AssetBundle CubeBundle;
    AssetBundle SphereBundle;
    GameObject SampleGameobject;

    public Button LoadAssetBundleButton;
    public Button LoadAssetButton;
    public Button UnLoadFalseButton;
    public Button UnLoadTrueButton;

    //打包的包名本应该由Editor类管理，但因为资源加载类也需要访问
    //所以放在资源加载类中
    public static string MainAssetBundleName = "SampleAssetBundle";

    //除了主包外，实际包名都必须全部小写
    public static string ObjectAssetBundleName = "resourecesbundle";


    public string AssetBundleLoadPath;

    public string HTTPAdress = "http://10.255.46.126:80";

    public string HTTPAssetBundlePath;

    public string DownloadPath;
    // Start is called before the first frame update
    void Start()
    {
        CheckAssetBundleLoadPath();

        LoadAssetBundleButton.onClick.AddListener(CheckAssetBundlePattern);

        LoadAssetButton.onClick.AddListener(LoadAsset);

        UnLoadFalseButton.onClick.AddListener(() => { UnloadAssetBundle(false); });

        UnLoadTrueButton.onClick.AddListener(() => { UnloadAssetBundle(true); });
    }

    void CheckAssetBundleLoadPath()
    {
        switch (LoadPattern)
        {
            case AssetBundlePattern.EditorSimulation:
                break;
            case AssetBundlePattern.Local:
                AssetBundleLoadPath = Path.Combine(Application.streamingAssetsPath, MainAssetBundleName);
                break;
            case AssetBundlePattern.Remote:
                HTTPAssetBundlePath = Path.Combine(HTTPAdress, MainAssetBundleName);
                DownloadPath = Path.Combine(Application.persistentDataPath, "DownloadAssetBundle");
                AssetBundleLoadPath = Path.Combine(DownloadPath, MainAssetBundleName);
                Debug.Log(AssetBundleLoadPath);
                if (!Directory.Exists(AssetBundleLoadPath))
                {
                    Directory.CreateDirectory(AssetBundleLoadPath);
                }
                break;
        }
    }



    IEnumerator DownloadFile(string fileName, Action callBack, bool isSaveFile = true)
    {
        string AssetBundleDownloadPath = Path.Combine(HTTPAssetBundlePath, fileName);

        UnityWebRequest webRequest = UnityWebRequest.Get(AssetBundleDownloadPath);

        yield return webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            //下载总字节数
            Debug.Log(webRequest.downloadedBytes);
            //下载进度
            Debug.Log(webRequest.downloadProgress);
            yield return new WaitForEndOfFrame();
        }

        string fileSavePath = Path.Combine(AssetBundleLoadPath, fileName);
        Debug.Log(webRequest.downloadHandler.data.Length);
        if (isSaveFile)
        {
            yield return SaveFile(fileSavePath, webRequest.downloadHandler.data, callBack);
        }
        else
        {
            //三目运算符判断是否为空
            callBack?.Invoke();
        }
    }


    IEnumerator SaveFile(string savePath, byte[] bytes, Action callBack)
    {

        //所有的system.IO方法，都只能在window上运行
        //如果想要跨平台保存文件，应每个平台调用不同的API
        FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate);

        yield return fileStream.WriteAsync(bytes, 0, bytes.Length);

        //释放文件流，否则文件一直处于被读取状态而不能被其他进程读取
        fileStream.Flush();
        fileStream.Close();
        fileStream.Dispose();

        callBack?.Invoke();
        Debug.Log($"{savePath}文件保存完成");

    }

    void CheckAssetBundlePattern()
    {
        if (LoadPattern == AssetBundlePattern.Remote)
        {
            StartCoroutine(DownloadFile(ObjectAssetBundleName, LoadAssetBundle));
        }
        else
        {
            LoadAssetBundle();
        }
    }

    void LoadAssetBundle()
    {

        string assetBundlePath = Path.Combine(AssetBundleLoadPath, MainAssetBundleName);
        //通过外部路径加载AB包
        //persistentDataPath在移动端可读可写
        //远程下载的AB包都可以放置在该路径下

        //加载清单捆绑包
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //manifest文件本身是明文储存给开发者看的
        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));

        foreach (var depAssetBundleName in assetBundleManifest.GetAllDependencies("0"))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(AssetBundleLoadPath, depAssetBundleName);
            //如果不使用依赖包内资源，可以不用变量储存,但仍会存于内存中
            AssetBundle.LoadFromFile(assetBundlePath);
        }

        assetBundlePath = Path.Combine(AssetBundleLoadPath, "0");
        //AB包加载可以允许加载工程路径外的路径
        CubeBundle = AssetBundle.LoadFromFile(assetBundlePath);

        assetBundlePath = Path.Combine(AssetBundleLoadPath, "1");
        //AB包加载可以允许加载工程路径外的路径
        SphereBundle = AssetBundle.LoadFromFile(assetBundlePath);
    }

    void LoadAsset()
    {
        GameObject cubeObject = CubeBundle.LoadAsset<GameObject>("Cube");
        Instantiate(cubeObject);
        cubeObject = SphereBundle.LoadAsset<GameObject>("Sphere");
        Instantiate(cubeObject);
    }

    void UnloadAssetBundle(bool isTrue)
    {
        Debug.Log(isTrue);
        //当前帧回收
        DestroyImmediate(SampleGameobject);
        CubeBundle.Unload(isTrue);

        //不会破坏当前运行时的效果
        Resources.UnloadUnusedAssets();
    }

}
