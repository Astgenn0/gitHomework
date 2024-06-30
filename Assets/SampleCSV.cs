using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using UnityEngine;



public class SampleCSV : MonoBehaviour
{

    string SeparateSymbol = ",";

    string LineFeedSymbol = "\r\n";

    // Start is called before the first frame update
    void Start()
    {
        SaveCSV();
        LoadCSV();
    }

    void LoadCSV()
    {
        string csvPath= Path.Combine(Application.dataPath, "Resrouces", "PlayerPropertyData.csv");
        string csvString = File.ReadAllText(csvPath);

        string[] csvRowDates = csvString.Split(LineFeedSymbol);
        
        //��Ϊ��һ���Ǳ�ͷ������Ҫ���
        for(int i = 1; i < csvRowDates.Length; i++)
        {

            string[] csvColnumDatas = csvRowDates[i].Split(SeparateSymbol);

            for(int j = 0; j < csvColnumDatas.Length; j++)
            {
                Debug.Log(csvColnumDatas[j]);
            }
        }
    }

    void SaveCSV()
    {

        DataTable csvSample = new DataTable("ʾ��CSV");
        csvSample.Columns.Add("�ȼ�");
        csvSample.Columns.Add("����");
        csvSample.Columns.Add("�ٶ�");

        DataRow dataRow = csvSample.NewRow();

        float speed = 1;

        for (int i = 0; i < 10; i++)
        {
            dataRow = csvSample.NewRow();
            dataRow[0] = i;
            dataRow[1] = Random.Range(1, 5);
            dataRow[2] = speed*(i+1)*1.13f;

            csvSample.Rows.Add(dataRow);
        }

        StringBuilder csvString = new StringBuilder();



        //������ӱ�ͷ
        for (int j = 0; j < csvSample.Columns.Count; j++)
        {
            csvString.Append(csvSample.Columns[j].ColumnName);

            if (j < csvSample.Columns.Count - 1)
            {
                csvString.Append(SeparateSymbol);
            }
        }

        //����ÿһ��ÿһ�У��������ݴ��뵽�ַ�����
        for (int i = 0; i < csvSample.Rows.Count; i++)
        {
            csvString.Append(LineFeedSymbol);

            for (int j = 0; j < csvSample.Columns.Count; j++)
            {
                csvString.Append(csvSample.Rows[i][j].ToString());

                if (j < csvSample.Columns.Count - 1)
                {
                    csvString.Append(SeparateSymbol);
                }
            }
        }

        string csvPath = Path.Combine(Application.dataPath,"Resrouces", "PlayerPropertyData.csv");

        File.WriteAllText(csvPath, csvString.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
