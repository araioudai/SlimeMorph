using System.Collections.Generic;
using System.IO;
using common;
using Unity.Android.Gradle.Manifest;
using UnityEditor;
using UnityEngine;

public class CSVEditor_MkII : EditorWindow
{
    #region Variables

    /// CSVデータ（2次元配列
    private List<List<string>> csvData = new();
    private List<int> inData = new();

    private List<string> idName = new();


    StageObjectDatabase stageObjectDatabase;

    string path;
    Vector2 scrollPos;

    Vector2 scrollPosName;



    private int stageID;
    #endregion

    #region Menu

    [MenuItem("Tools/CSV Editor MkII")]
    private static void OpenWindow()
    {
        GetWindow<CSVEditor_MkII>("CSV Editor MkII");
    }

    #endregion
    #region GUI
    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Load CSV")) { LoadCSV(); }
        if (GUILayout.Button("Save CSV")) { SaveCSV(); }
        if (GUILayout.Button("New Save CSV")) { SaveCSV(true); }
        EditorGUILayout.EndHorizontal();

        // StageObjectDatabaseを入れられるようにする
        stageObjectDatabase = (StageObjectDatabase)EditorGUILayout.ObjectField("Stage Object Database", stageObjectDatabase, typeof(StageObjectDatabase), false);

        if (stageObjectDatabase == null)
        {
            EditorGUILayout.HelpBox("Stage Object Databaseが設定されていません", MessageType.Warning);
            return;
        }



        CSVView();
        IdNameView();
        PreLoad();
        DataCheck();


    }

    void CSVView()
    {
        inData.Clear();

        stageID = EditorGUILayout.IntField("ステージID", stageID);

        // スクロール開始
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(100));


        if (csvData.Count > 1)
        {
            EditorGUILayout.BeginHorizontal();

            for (int col = 1; col < csvData[0].Count; col++)
            {
                EditorGUILayout.LabelField((col - 1).ToString(), GUILayout.Width(40));
            }

            EditorGUILayout.EndHorizontal();
        }

        // データ表示
        for (int row = 0; row < csvData.Count; row++)
        {
            // 1列目のIDをリストに追加
            if (int.TryParse(csvData[row][0], out int parsed))
                DataIn(parsed);

            if (!int.TryParse(csvData[row][0], out int id) || id != stageID)
                continue;

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label((row + 1).ToString(), GUILayout.Width(20));

            for (int col = 1; col < csvData[row].Count; col++)
            {
                if (col == 1)
                {
                    int lane = 0;
                    int.TryParse(csvData[row][1], out lane);

                    string laneLabel = lane switch
                    {
                        0 => "中",
                        1 => "左",
                        2 => "右",
                        _ => "?"
                    };
                    EditorGUILayout.LabelField(laneLabel, GUILayout.Width(20));
                }
                else
                {
                    GUIStyle textFieldStyle = new(EditorStyles.textField);

                    if (!float.TryParse(csvData[row][col], out float objectId))
                    {
                        textFieldStyle.normal.textColor = Color.red;
                    }
                    else
                    {
                        if (stageObjectDatabase.IDCheck(Parse(csvData[row][col])))
                            textFieldStyle.normal.textColor = Color.white;
                        else
                            textFieldStyle.normal.textColor = Color.red;
                    }


                    csvData[row][col] = EditorGUILayout.TextField(csvData[row][col], textFieldStyle, GUILayout.Width(40));
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
        // DataCheck();
    }

    void IdNameView()
    {
        if (stageObjectDatabase == null)
        {
            EditorGUILayout.HelpBox("Stage Object Databaseが設定されていません", MessageType.Warning);
            return;
        }

        idName.Clear();

        // スクロール開始
        scrollPosName = EditorGUILayout.BeginScrollView(scrollPosName, GUILayout.Height(100));

        foreach (var data in stageObjectDatabase.GetAll())
        {
            idName.Add($"ID: {data.id}, Name: {data.name}");
        }

        foreach (var name in idName)
        {
            EditorGUILayout.LabelField(name);
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("ID順に並べ替え"))
        {
            stageObjectDatabase.GetAll().Sort((a, b) => a.id.CompareTo(b.id));
            EditorUtility.SetDirty(stageObjectDatabase);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    void PreLoad()
    {
        // ステージをロードするボタン
        if (GUILayout.Button("Load Stage"))
        {
            if (EditorApplication.isPlaying)
            {
                // プレイモード中の場合、ステージをロードする処理を実行
                StageManager.Instance.debugMode = true; // デバッグモードを有効化
                StageManager.Instance.CreateStage(stageID);
            }
            else
            {
                // プレイモードにする
                EditorApplication.isPlaying = true;
                // プレイモードが開始された後にステージをロードする処理を実行
                EditorApplication.delayCall += () =>
                {
                    StageManager.Instance.debugMode = true; // デバッグモードを有効化
                    StageManager.Instance.CreateStage(stageID);
                };
            }
        }
    }





    void DataIn(int x)
    {
        if (!inData.Contains(x)) // 重複チェック
        {
            inData.Add(x);
        }
    }

    void DataCheck()
    {
        if (csvData.Count == 0)
        {
            EditorGUILayout.HelpBox("CSVが読み込まれていません", MessageType.Warning);
            return;
        }

        if (!inData.Contains(stageID))
        {
            EditorGUILayout.HelpBox("このステージIDは存在しません", MessageType.Info);

            if (GUILayout.Button("新規作成"))
            {
                DataCreate();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("このステージIDは存在します", MessageType.Info);

            if (GUILayout.Button("削除"))
            {
                DataDelete();
            }
        }
    }

    void DataCreate()
    {
        for (int i = 0; i <= 2; i++)
        {
            List<string> newRow = new();

            newRow.Add(stageID.ToString()); // StageID

            for (int y = 1; y < csvData[0].Count; y++)
            {
                switch (y)
                {
                    case 1:
                        newRow.Add(i.ToString()); // Lane
                        break;
                    case >= 2 and <= 10:
                        newRow.Add("0");
                        break;

                    case > 10:
                        newRow.Add("");
                        break;

                    default:
                        newRow.Add("");
                        break;
                }
            }

            csvData.Add(newRow);
        }
    }

    void DataDelete()
    {
        // csvDataからステージIDに一致する行を削除
        csvData.RemoveAll(row => int.TryParse(row[0], out int id) && id == stageID);
    }


    #endregion

    #region CSV処理
    void LoadCSV()
    {
        path = EditorUtility.OpenFilePanel("CSVを選択", "", "csv");

        if (!string.IsNullOrEmpty(path))
        {
            string text = File.ReadAllText(path);

            csvData.Clear();

            string[] lines = text.Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string[] values = line.Split(",");
                csvData.Add(new(values));
            }
            Debug.Log("CSV loaded from: " + path);
        }
    }

    void SaveCSV(bool newSave = false)
    {
        if (newSave)
            path = EditorUtility.SaveFilePanel("CSVを保存", "", "Stage.csv", "csv");

        if (!string.IsNullOrEmpty(path))
        {
            List<string> lines = new();

            foreach (var row in csvData)
            {
                string line = string.Join(",", row);
                lines.Add(line);
            }

            string csvText = string.Join("\n", lines);
            File.WriteAllText(path, csvText);
            Debug.Log("CSV saved to: " + path);
            AssetDatabase.Refresh();
        }
    }



    static int Parse(string objectId)
    {
        int id;
        if (float.Parse(objectId) >= 100)
        {
            id = (int)(float.Parse(objectId) / 100);
        }
        else
        {
            id = (int)float.Parse(objectId);
        }


        return id;
    }


    #endregion

}
