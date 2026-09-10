/// <summary>
/// OnLineManagerとOffLineManagerのどちらを使うべきかをまとめるクラス
/// </summary>
public static class PlayerBackend
{
    /// <summary>
    /// 今使うべきバックエンドを返す
    /// DebugMode.debugModeがtrue: オンライン(OnLineManager)を使う
    /// DebugMode.debugModeがfalse: オフライン(OffLineManager)を使う
    /// </summary>
    public static IPlayerBackend Current
    {
        get
        {
            //DebugModeが無い場合はオフライン
            bool isOnline = DebugMode.Instance != null && DebugMode.Instance.GetDebugMode();

            return isOnline
                ? (IPlayerBackend)OnLineManager.Instance
                : OffLineManager.Instance;
        }
    }
}