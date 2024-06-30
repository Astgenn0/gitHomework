using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using Newtonsoft.Json;
using System.IO;

public class Student
{
    public string Name;
    public int Index;
}
public class SimpleJson : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {


        LoadJson();
    }

    void SaveJson(object targetObject)
    {
        string jsonString= JsonMapper.ToJson(targetObject);
        string jsonPath = Path.Combine(Application.dataPath, "LitJson.Simple");
        File.WriteAllText(jsonPath, jsonString);

        jsonString=JsonConvert.SerializeObject(targetObject);
        jsonPath = Path.Combine(Application.dataPath, "NewTown.Simple");
        File.WriteAllText(jsonPath, jsonString);

        jsonString =JsonUtility.ToJson(targetObject);
        jsonPath = Path.Combine(Application.dataPath, "JsonUtility.Simple");
        File.WriteAllText(jsonPath, jsonString);
    }

    public void LoadJson()
    {
        string jsonPath = Path.Combine(Application.dataPath, "LitJson.Simple");
        string jsonString = File.ReadAllText(jsonPath);
        Student sampleStudent = JsonMapper.ToObject<Student>(jsonString);

        Debug.Log(sampleStudent);

        jsonPath = Path.Combine(Application.dataPath, "NewTown.Simple");
        jsonString = File.ReadAllText(jsonPath);
        sampleStudent = JsonConvert.DeserializeObject<Student>(jsonString);

        Debug.Log(sampleStudent);

        jsonPath = Path.Combine(Application.dataPath, "JsonUtility.Simple");
        jsonString = File.ReadAllText(jsonPath);
        sampleStudent = JsonUtility.FromJson<Student>(jsonString);

        Debug.Log(sampleStudent);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
