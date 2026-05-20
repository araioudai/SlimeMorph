using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CSVLoader : MonoBehaviour
{
    public List<string[]> LoadCSV(string filePath)
    {
        List<string[]> csvData = new List<string[]>();
        TextAsset csvFile = Resources.Load<TextAsset>(filePath);
        if (csvFile != null)
        {
            string[] lines = csvFile.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] values = line.Split(',');
                // ここでvalues配列を使用してデータを処理します
                // Debug.Log("CSVの行: " + line);
                csvData.Add(values);
            }
        }
        else
        {
            Debug.LogWarning("CSVファイルが見つかりません: " + filePath);
        }
        return csvData;
    }
}
