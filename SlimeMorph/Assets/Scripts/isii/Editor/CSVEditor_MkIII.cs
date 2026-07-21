using System.Collections.Generic;
using System.IO;
using common;
using UnityEditor;
using UnityEngine;

public class CSVEditor_MkIII : EditorWindow
{
    const int StageIdColumn = 0;
    const int LaneColumn = 1;
    const int SeasonColumn = 2;
    const int FirstBlockColumn = 3;

    #region Variables

    //==========================
    // CSVデータ
    private List<List<string>> csvData = new();
    private List<int> inData = new();
    string path;

    //==========================
    // StageObjectDatabase
    StageObjectDatabase stageObjectDatabase;

    //==========================
    // スクロール位置
    Vector2 scrollPos;
    Vector2 scrollPosName;
    Vector3 scrollPosButtonX;

    //==========================
    // 表示設定
    private int stageID;
    int viewWidth = 30;
    int maxWidth = 100;
    int minWidth = 15;

    //==========================
    // 選択中のオブジェクトIDと名前
    private int selectedObjectId = 0;
    private int selectNoneId = -99;
    private string selectedObjectName = "None";
    List<Color> colorList = new();


    //==========================
    // 選択中のオブジェクトIDと名前
    private int coinValue = 0;



    #endregion

    #region Menu

    [MenuItem("Tools/CSV Editor MkIII")]
    private static void OpenWindow()
    {
        GetWindow<CSVEditor_MkIII>("CSV Editor MkIII");
    }

    #endregion
    #region GUI
    private void OnGUI()
    {
        InitializeVariables();

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
        if (csvData.Count == 0)
        {
            EditorGUILayout.HelpBox("CSVが読み込まれていません", MessageType.Warning);
            return;
        }

        CSVView();
        IdPalletteView();
        PreLoad();
        if (GUILayout.Button("Add Object to All Rows")) { AddObjectToAllRows(); }
        StageDataCheck();


        // Coin数表示
        EditorGUILayout.LabelField($"Coin数: {coinValue}", EditorStyles.boldLabel);



    }

    #region 変数初期化
    void InitializeVariables()
    {
        coinValue = 0;
    }
    #endregion




    #region CSV表示
    void CSVView()
    {
        inData.Clear();

        stageID = EditorGUILayout.IntField("ステージID", stageID);
        viewWidth = EditorGUILayout.IntField("表示幅", viewWidth);
        viewWidth = Mathf.Clamp(viewWidth, minWidth, maxWidth);

        // スクロール開始
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(120));
        try
        {
            // 行数
            if (viewWidth > 1)
            {
                EditorGUILayout.BeginHorizontal();
                try
                {
                    EditorGUILayout.LabelField("", GUILayout.Width(40));

                    for (int col = minWidth + 1; col < viewWidth; col++)
                    {
                        EditorGUILayout.LabelField(col.ToString(), GUILayout.Width(80));
                    }
                }
                finally
                {
                    EditorGUILayout.EndHorizontal();
                }
            }

            // CSVデータ
            int maxRows = Mathf.Min(viewWidth, csvData.Count);
            for (int row = 0; row < maxRows; row++)
            {
                List<string> rowData = csvData[row];
                if (rowData == null || rowData.Count == 0)
                    continue;

                if (int.TryParse(rowData[StageIdColumn], out int parsed))
                    DataIn(parsed);

                if (!int.TryParse(rowData[StageIdColumn], out int id) || id != stageID)
                    continue;

                EditorGUILayout.BeginHorizontal();
                try
                {
                    GUILayout.Label((row + 1).ToString(), GUILayout.Width(20));

                    int lane = 0;
                    if (rowData.Count > LaneColumn)
                        int.TryParse(rowData[LaneColumn], out lane);

                    string laneLabel = lane switch
                    {
                        0 => "中",
                        1 => "左",
                        2 => "右",
                        _ => "?"
                    };

                    EditorGUILayout.LabelField(laneLabel, GUILayout.Width(20));
                    DrawSeasonField(rowData);

                    int maxCols = Mathf.Min(viewWidth, rowData.Count);
                    for (int col = Mathf.Max(FirstBlockColumn, minWidth + 1); col < maxCols; col++)
                    {
                        DrawPlacementCell(row, col);
                    }
                }
                finally
                {
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        finally
        {
            EditorGUILayout.EndScrollView();
        }
        // DataCheck();
    }

    void DataIn(int x)
    {
        if (!inData.Contains(x)) // 重複チェック
        {
            inData.Add(x);
        }
    }

    void DrawSeasonField(List<string> rowData)
    {
        EnsureSeasonColumn(rowData);

        int seasonValue = ParseSeasonValue(rowData[SeasonColumn]);
        int newSeasonValue = EditorGUILayout.IntPopup(
            seasonValue,
            new[] { "春", "夏", "秋", "冬" },
            new[] { 1, 2, 3, 4 },
            GUILayout.Width(50));

        if (newSeasonValue != seasonValue)
        {
            rowData[SeasonColumn] = newSeasonValue.ToString();
        }
    }

    void EnsureSeasonColumn(List<string> rowData)
    {
        while (rowData.Count <= SeasonColumn)
        {
            rowData.Add(string.Empty);
        }

        if (string.IsNullOrWhiteSpace(rowData[SeasonColumn]))
        {
            rowData[SeasonColumn] = "1";
        }
    }

    int ParseSeasonValue(string rawSeason)
    {
        if (int.TryParse(rawSeason, out int seasonValue) && seasonValue is >= 1 and <= 4)
        {
            return seasonValue;
        }

        return 1;
    }

    void DrawPlacementCell(int row, int col)
    {
        if (row < 0 || row >= csvData.Count)
            return;

        List<string> rowData = csvData[row];
        if (rowData == null || col < 0 || col >= rowData.Count)
        {
            GUILayout.Label("", GUILayout.Width(80), GUILayout.Height(20));
            return;
        }

        string rawValue = rowData[col];
        int currentId = selectNoneId;

        if (!string.IsNullOrWhiteSpace(rawValue) && float.TryParse(rawValue, out _))
        {
            currentId = Parse(rawValue);
        }

        string label = GetObjectLabel(currentId);

        GUI.backgroundColor = currentId == selectNoneId ? Color.gray : Color.white;

        if (GUILayout.Button(label, GUILayout.Width(80), GUILayout.Height(20)))
        {
            string previousId;

            if (selectedObjectId == selectNoneId)
                previousId = "";
            else
                previousId = selectedObjectId.ToString();

            rowData[col] = previousId;
            GUI.FocusControl(null);
            Repaint();
        }

        // Coin数の計算
        if (currentId == 1) // CoinのIDが1の場合
        {
            coinValue++;
        }

        // GUI.backgroundColor = Color.white;
    }

    string GetObjectLabel(int id)
    {
        if (id == selectNoneId) return "Empty";

        var data = stageObjectDatabase.GetAll().Find(x => x.id == id);
        if (data != null)
            return data.name;

        return $"Invalid({id})";
    }
    #endregion

    #region IDパレット表示
    void IdPalletteView()
    {
        EditorGUILayout.LabelField("配置するオブジェクトを選択", EditorStyles.boldLabel);

        scrollPosName = EditorGUILayout.BeginScrollView(scrollPosName, GUILayout.Height(150));

        // None
        GUI.backgroundColor = selectedObjectId == selectNoneId ? Color.cyan : Color.white;
        if (GUILayout.Button($"None"))
        {
            selectedObjectId = selectNoneId;
            selectedObjectName = "None";
        }

        foreach (var data in stageObjectDatabase.GetAll())
        {
            GUI.backgroundColor = selectedObjectId == data.id ? Color.cyan : Color.white;

            if (GUILayout.Button($"{data.name}"))
            {
                selectedObjectId = data.id;
                selectedObjectName = data.name;
            }
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndScrollView();

        EditorGUILayout.HelpBox($"選択中: {selectedObjectId} / {selectedObjectName}", MessageType.Info);
    }
    #endregion

    #region ステージロードボタン
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
    #endregion

    #region 一括追加
    void AddObjectToAllRows()
    {
        if (selectedObjectId == selectNoneId)
        {
            EditorUtility.DisplayDialog("Error", "配置するオブジェクトを選択してください。", "OK");
            return;
        }

        for (int row = 0; row < csvData.Count; row++)
        {
            List<string> rowData = csvData[row];
            if (rowData == null || rowData.Count <= LaneColumn)
                continue;

            if (!int.TryParse(rowData[StageIdColumn], out int id) || id != stageID)
                continue;

            // 空のセルにのみ追加
            for (int col = FirstBlockColumn; col < rowData.Count; col++)
            {
                if (string.IsNullOrWhiteSpace(rowData[col]))
                {
                    rowData[col] = selectedObjectId.ToString();
                    break; // 1行につき1つだけ追加
                }
            }
        }
    }


    #endregion

    #region データ操作
    void StageDataCheck()
    {
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
            newRow.Add(i.ToString()); // Lane
            newRow.Add("1"); // Season

            for (int y = FirstBlockColumn; y < maxWidth; y++)
            {
                if (y <= 15)
                {
                    newRow.Add("0");
                }
                else
                {
                    newRow.Add("");
                }
            }

            csvData.Add(newRow);
        }
    }

    void DataDelete()
    {
        // csvDataからステージIDに一致する行を削除
        csvData.RemoveAll(row => int.TryParse(row[StageIdColumn], out int id) && id == stageID);
    }
    #endregion

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