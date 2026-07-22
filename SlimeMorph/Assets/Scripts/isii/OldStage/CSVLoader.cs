using System.Collections.Generic;
using UnityEngine;
using System.IO;
using common;
using Unity.Mathematics;

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
            if (!int.TryParse(line[2], out int season)) continue;

            for (int col = 3; col < line.Length; col++)
            {
                string raw = line[col].Trim();

                // 空白は無視
                if (string.IsNullOrEmpty(raw)) continue;

                // 数値化
                if (!float.TryParse(raw, out float objectId)) continue;

                int z = col - 3; // z座標は列番号から計算

                float amount = 0;

                // if (objectId >= 100)
                // {
                //     amount = objectId % 100;
                //     objectId = (int)(objectId / 100);
                // }

                // ScriptableObjectのparamを参照
                amount = StageManager.Instance.GetStageObjectParam((int)objectId);
                // Debug.Log($"objectId: {objectId}, amount: {amount}");

                StageCellData cell = new()
                {
                    stageId = stageId,
                    lane = lane,
                    season = (StageSeason)season,
                    z = z,
                    objectId = (int)objectId,
                    amount = amount,
                };

                result.Add(cell);
            }
        }

        return result;
    }


}
