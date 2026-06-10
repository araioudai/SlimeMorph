using System;

namespace common
{
    [Serializable]
    public enum StageObjectType
    {
        None = 0,           // 何もない
        Increase = 1,       // 増加
        Decrease = 2,       // 減少
        Hole = 3,            // 穴
        Wall = 4,           // 壁
    }

    [Serializable]
    public class StageCellData
    {
        public int stageId;   // ステージID
        public int lane;      // 0=床,1=左,2=右
        public int z;         // 奥行き
        public int objectId;  // オブジェクトID
    }

}