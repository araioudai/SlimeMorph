using System;

namespace common
{
    public static class Const
    {
        public const string SOUND_SETTINGS_FILE_NAME = "sound_settings.json";
    }






    [Serializable]
    public enum StageObjectType
    {
        None = 0,           // 何もない
        Increase = 1,       // 増加
        Decrease = 2,       // 減少
        Hole = 3,            // 穴
        Wall = 4,           // 壁
        Enemy = 5,          // 敵
    }

    [Serializable]
    public class StageCellData
    {
        public int stageId;   // ステージID
        public int lane;      // 0=床,1=左,2=右
        public int z;         // 奥行き
        public int objectId;  // オブジェクトID
        public float amount;    // 値
    }

    [Serializable]
    public class SoundSettings
    {
        public float masterVolume = 1.0f;
        public float bgmVolume = 0.3f;
        public float seVolume = 0.5f;
    }






    #region SoundManager
    public enum SE
    {
        Decision,
        Cancel,
        HitDamage,
        Heal,
        Max
    }





    #endregion
}