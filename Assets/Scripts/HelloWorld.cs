using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;



public enum AssetBundlePattern
{
    //�༭��ģ����أ�Ӧ��ʹ��AssetDatabase������Դ���أ������ý��д��
    EditorSimulation,
    //���ؼ���ģʽ��Ӧ���������·����StreamingAssets·����,�Ӹ�·������
    Local,
    //Զ�˼���ģʽ��Ӧ������������Դ��������ַ��Ȼ��ͨ�������������
    //���ص�ɳ��·��persistentDataPath���ٽ��м���
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

    //����İ�����Ӧ����Editor���������Ϊ��Դ������Ҳ��Ҫ����
    //���Է�����Դ��������
    public static string MainAssetBundleName = "SampleAssetBundle";

    //���������⣬ʵ�ʰ���������ȫ��Сд
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
            //�������ֽ���
            Debug.Log(webRequest.downloadedBytes);
            //���ؽ���
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
            //��Ŀ������ж��Ƿ�Ϊ��
            callBack?.Invoke();
        }
    }


    IEnumerator SaveFile(string savePath, byte[] bytes, Action callBack)
    {

        //���е�system.IO��������ֻ����window������
        //�����Ҫ��ƽ̨�����ļ���Ӧÿ��ƽ̨���ò�ͬ��API
        FileStream fileStream = File.Open(savePath, FileMode.OpenOrCreate);

        yield return fileStream.WriteAsync(bytes, 0, bytes.Length);

        //�ͷ��ļ����������ļ�һֱ���ڱ���ȡ״̬�����ܱ��������̶�ȡ
        fileStream.Flush();
        fileStream.Close();
        fileStream.Dispose();

        callBack?.Invoke();
        Debug.Log($"{savePath}�ļ��������");

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
        //ͨ���ⲿ·������AB��
        //persistentDataPath���ƶ��˿ɶ���д
        //Զ�����ص�AB�������Է����ڸ�·����

        //�����嵥�����
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //manifest�ļ����������Ĵ���������߿���
        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));

        foreach (var depAssetBundleName in assetBundleManifest.GetAllDependencies("0"))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(AssetBundleLoadPath, depAssetBundleName);
            //�����ʹ������������Դ�����Բ��ñ�������,���Ի�����ڴ���
            AssetBundle.LoadFromFile(assetBundlePath);
        }

        assetBundlePath = Path.Combine(AssetBundleLoadPath, "0");
        //AB�����ؿ���������ع���·�����·��
        CubeBundle = AssetBundle.LoadFromFile(assetBundlePath);

        assetBundlePath = Path.Combine(AssetBundleLoadPath, "1");
        //AB�����ؿ���������ع���·�����·��
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
        //��ǰ֡����
        DestroyImmediate(SampleGameobject);
        CubeBundle.Unload(isTrue);

        //�����ƻ���ǰ����ʱ��Ч��
        Resources.UnloadUnusedAssets();
    }

}
