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

        XmlElement xmlRootElement = xmlDoc.CreateElement("ʾ��");

        xmlRootElement.InnerText = "�����ƾ���ѧ";
        xmlRootElement.SetAttribute("ʱ��", DateTime.Now.ToShortDateString());

        XmlElement xmlChildElement = xmlDoc.CreateElement("ѧ��");
        xmlChildElement.InnerText = "111";

        xmlRootElement.AppendChild(xmlChildElement);


        xmlDoc.AppendChild(xmlRootElement);

        string xmlPath = Path.Combine(Application.dataPath, "SampleXML");
        xmlDoc.Save(xmlPath);
    }

    void LoadXML()
    {
        //XML.LOAD�������ֻ��Windowsƽ̨��ʹ��
        string xmlPath = Path.Combine(Application.dataPath, "SampleXML");
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlPath);


        /*string xmlString = File.ReadAllText(xmlPath);

        TextAsset textAsset = Resources.Load<TextAsset>(xmlPath);
        xmlString = textAsset.text;

        xmlDoc.LoadXml(xmlString);*/

        //Node�ǽڵ��ȡ�����ͣ������������޷�ʹ��
        foreach(XmlNode node in xmlDoc.ChildNodes)
        {
            Debug.Log(node.Name);
            XmlElement element = (XmlElement)node;

            element.SetAttribute("��̬���","����ʱ");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
