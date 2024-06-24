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
        //ͨ���ⲿ·������AB��
        //persistentDataPath���ƶ��˿ɶ���д
        //Զ�����ص�AB�������Է����ڸ�·����
        string assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);

        //�����嵥�����
        AssetBundle mainAB = AssetBundle.LoadFromFile(assetBundlePath);

        //manifest�ļ����������Ĵ���������߿���
        AssetBundleManifest assetBundleManifest = mainAB.LoadAsset<AssetBundleManifest>(nameof(AssetBundleManifest));

        foreach (var depAssetBundleName in assetBundleManifest.GetAllDependencies("prefebs"))
        {
            Debug.Log(depAssetBundleName);
            assetBundlePath = Path.Combine(Application.persistentDataPath, mainBundleName, mainBundleName);
            //�����ʹ������������Դ�����Բ��ñ�������,���Ի�����ڴ���
            AssetBundle.LoadFromFile(assetBundlePath);
        }

        assetBundlePath = Path.Combine(Application.persistentDataPath, "Bundles", "prefebs");
        //AB�����ؿ���������ع���·�����·��
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
        //��ǰ֡����
        DestroyImmediate(SampleGameobject);
        SampleBundle.Unload(isTrue);

        Resources.UnloadUnusedAssets();
    }
        
}
