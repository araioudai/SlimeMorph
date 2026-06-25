using UnityEngine;
using System.Collections.Generic;

public class SkinManager : MonoBehaviour
{
    [Header("スキンプレファブの配列")]
    [SerializeField] private GameObject[] skinPrefabs;

    //PlayerPrefsで使用する保存用のキー名
    private const string SelectedSkinKey = "SavedSelectedSkinIndex";

    //生成したキャラクターを管理するリスト
    private List<GameObject> spawnedSkins = new List<GameObject>();

    //二重初期化の防止フラグ
    private bool isInitialized = false;

    void Start()
    {
        InitializeSkins();
    }

    /// <summary>
    /// 全スキンをサブカメラの前に生成し、保存されたスキンだけを表示する
    /// </summary>
    private void InitializeSkins()
    {
        if (isInitialized) return; //既に初期化済みならスキップ

        if (skinPrefabs == null || skinPrefabs.Length == 0) return;

        //インデックスは「0始まり」
        int savedIndex = PlayerPrefs.GetInt(SelectedSkinKey, 0);

        for (int i = 0; i < skinPrefabs.Length; i++)
        {
            if (skinPrefabs[i] == null) continue;

            //キャラクターを所定の位置に生成
            GameObject spawnedCharacter = Instantiate(skinPrefabs[i], transform);
            spawnedSkins.Add(spawnedCharacter);

            //ロードされたインデックスと一致するものだけを表示、他は非表示
            spawnedCharacter.SetActive(i == savedIndex);
        }

        isInitialized = true;
    }

    /// <summary>
    /// 外部からスキンをリアルタイムに切り替えるためのメソッド
    /// </summary>
    /// <param name="targetIndex">表示したいスキンのインデックス</param>
    public void ChangeSkin(int targetIndex)
    {
        //UI側の初期化が先に入ってしまった場合、ここで強制初期化
        if (!isInitialized)
        {
            InitializeSkins();
        }

        //インデックスが配列の範囲外なら処理しない
        if (targetSkinsIndexInvalid(targetIndex)) return;

        transform.rotation = Quaternion.Euler(3.121f, -178.718f, 0.15f);

        for (int i = 0; i < spawnedSkins.Count; i++)
        {
            //対象のインデックスだけを true にし、それ以外を false にする
            spawnedSkins[i].SetActive(i == targetIndex);
        }
    }

    /// <summary>
    /// スキン画面から待機画面に戻る時
    /// </summary>
    public void PushBack()
    {
        transform.rotation = Quaternion.Euler(3.121f, -178.718f, 0.15f);
    }

    /// <summary>
    /// 選択されたスキンが範囲内か判定
    /// </summary>
    /// <param name="index">選択されたスキン番号</param>
    /// <returns></returns>
    private bool targetSkinsIndexInvalid(int index)
    {
        return index < 0 || index >= spawnedSkins.Count;
    }
}