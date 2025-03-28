namespace uIP.MacroProvider.StreamIO.VideoInToFrame
{
    internal enum VideoStreamIndex : int
    {
        VideoPath = 0,     // 影片檔案路徑
        FrameInterval,     // 以影格數計算的切割間隔 (秒數 * 幀率)
        //CurrentFrame,      // 當前讀取到的影格數 (用來追蹤進度)
        ReaderInstance,    // VideoCapture 實例 (持續開啟)
        FrameRate,         // 影片的幀率
        MaxCount,
        Form,              // 標記用，不使用
    }
}
