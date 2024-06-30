using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public class SampleXML : MonoBehaviour
{
    public SampleScriptableObject Sample;
    // Start is called before the first frame update
    void Start()
    {
        //LoadXML();

        /*Sample = ScriptableObject.CreateInstance<SampleScriptableObject>();
        Sample.index++;
        Debug.Log(Sample.index);*/
    }

    void SaveXML()
    {
        XmlDocument xmlDoc = new XmlDocument();

        XmlElement xmlRootElement = xmlDoc.CreateElement("示例");

        xmlRootElement.InnerText = "江西财经大学";
        xmlRootElement.SetAttribute("时间", DateTime.Now.ToShortDateString());

        XmlElement xmlChildElement = xmlDoc.CreateElement("学生");
        xmlChildElement.InnerText = "111";

        xmlRootElement.AppendChild(xmlChildElement);


        xmlDoc.AppendChild(xmlRootElement);

        string xmlPath = Path.Combine(Application.dataPath, "SampleXML");
        xmlDoc.Save(xmlPath);
    }

    void LoadXML()
    {
        //XML.LOAD方法最好只在Windows平台上使用
        string xmlPath = Path.Combine(Application.dataPath, "SampleXML");
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);


        /*string xmlString = File.ReadAllText(xmlPath);

        TextAsset textAsset = Resources.Load<TextAsset>(xmlPath);
        xmlString = textAsset.text;

        xmlDoc.LoadXml(xmlString);*/

        //Node是节点读取的类型，但部分设置无法使用
        foreach(XmlNode node in xmlDoc.ChildNodes)
        {
            Debug.Log(node.Name);
            XmlElement element = (XmlElement)node;

            element.SetAttribute("动态添加","运行时");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
