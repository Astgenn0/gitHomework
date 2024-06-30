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
        
        //因为第一行是表头，不需要输出
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

        DataTable csvSample = new DataTable("示例CSV");
        csvSample.Columns.Add("等级");
        csvSample.Columns.Add("力量");
        csvSample.Columns.Add("速度");

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



        //单独添加表头
        for (int j = 0; j < csvSample.Columns.Count; j++)
        {
            csvString.Append(csvSample.Columns[j].ColumnName);

            if (j < csvSample.Columns.Count - 1)
            {
                csvString.Append(SeparateSymbol);
            }
        }

        //遍历每一行每一列，并将数据传入到字符串中
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
