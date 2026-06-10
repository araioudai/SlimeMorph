using System.Collections.Generic;
using UnityEngine;
using System.IO;
using common;

public class CSVLoader : MonoBehaviour
{
    public List<string[]> LoadCSV(string filePath)
    {
        List<string[]> csvData = new();
        TextAsset csvFile = Resources.Load<TextAsset>(filePath);
        if (csvFile != null)
        {
            string[] lines = csvFile.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] values = line.Split(',');
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

    public static List<StageCellData> Parse(List<string[]> csvData)
    {
        List<StageCellData> result = new();

        foreach (var line in csvData)
        {
            if (line[0] == "stageId") continue;

            if (!int.TryParse(line[0], out int stageId)) continue;
            if (!int.TryParse(line[1], out int lane)) continue;

            for (int col = 2; col < line.Length; col++)
            {
                string raw = line[col].Trim();

                // 空白は無視
                if (string.IsNullOrEmpty(raw)) continue;

                // 数値化
                if (!int.TryParse(raw, out int objectId)) continue;

                int z = col - 2;

                StageCellData cell = new()
                {
                    stageId = stageId,
                    lane = lane,
                    z = z,
                    objectId = objectId // ← ここに0も入る
                };

                result.Add(cell);
            }
        }

        return result;
    }


}
