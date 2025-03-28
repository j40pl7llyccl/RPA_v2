namespace PluginTestScript
{
    class Program
    {
        static void Main(string[] args)
        {   /*--
            // 1. 設定測試影片檔案的完整路徑
            string testVideoPath = @"C:\Users\jordan.liu\Videos\LSTM_test(Python).mp4";

            double intervalSeconds = 20.0;

            // 2. 取得目前執行檔所在目錄，並在其中建立輸出資料夾 (Video_output)
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string outputFolder = Path.Combine(currentDir, "Video_output");
            if (!Directory.Exists(outputFolder))
                Directory.CreateDirectory(outputFolder);

            // 3. 設定子資料夾 (videoOutputFolder)，用於最終影格與 ini
            //    videoName 為影片檔名（不含副檔名）
            string videoName = Path.GetFileNameWithoutExtension(testVideoPath);
            string videoOutputFolder = Path.Combine(outputFolder, videoName);

            // 【關鍵】先清空該資料夾，確保不會殘留上次切好的影格
            if (Directory.Exists(videoOutputFolder))
            {
                Directory.Delete(videoOutputFolder, true);
            }
            Directory.CreateDirectory(videoOutputFolder);

            // 4. 使用 Plugin 提供的方法，根據「原影片路徑」與「切割秒數」建立初始化 UDataCarrier
            //UDataCarrier initData = uMProvidVideoStreamToFrame.MakeVideoStreamInitData(testVideoPath, intervalSeconds);
            if (initData == null)
            {
                Console.WriteLine("初始化影片流資料失敗，請確認影片檔案存在且可開啟。");
                return;
            }

            // 5. 建立 Plugin 實例並初始化
            uMProvidVideoStreamToFrame plugin = new uMProvidVideoStreamToFrame();
            plugin.Initialize(null);

            // 6. 建立一個虛擬的 Macro 實例，並把初始化資料掛在其 MutableInitialData
            UMacro macro = new UMacro(
                null,
                "VideoStreamToFrame",
                "VideoDev_ExtractNextFrame",
                null,
                null,
                null,
                null,
                null
            );
            macro.MutableInitialData = initData;

            // 7. 準備呼叫 ExtractNextFrame 所需的參數
            UDataCarrier[] prevPropagation = null;
            var historyResultCarriers = new List<UMacroProduceCarrierResult>();
            var historyPropagationCarriers = new List<UMacroProduceCarrierPropagation>();
            var historyDrawingCarriers = new List<UMacroProduceCarrierDrawingResult>();
            var historyCarrier = new List<UScriptHistoryCarrier>();

            bool bStatusCode = false;
            string strStatusMessage = "";
            UDataCarrier[] currPropagationCarrier = null;
            UDrawingCarriers currDrawingCarriers = null;
            fpUDataCarrierSetResHandler propagationCarrierHandler = null;
            fpUDataCarrierSetResHandler resultCarrierHandler = null;

            // 8. 透過反射取得私有方法 ExtractNextFrame
            MethodInfo extractMethod = typeof(uMProvidVideoStreamToFrame)
                .GetMethod("ExtractNextFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            if (extractMethod == null)
            {
                Console.WriteLine("無法找到 ExtractNextFrame 方法。");
                return;
            }

            Console.WriteLine("開始提取影格：");
            int count = 0;
            // 用來記錄每次提取的 (秒數, 檔案名稱)，後續寫入 ini
            var mappingList = new List<(int seconds, string fileName)>();

            // 9. 連續呼叫 ExtractNextFrame，直到結束或錯誤
            while (true)
            {
                object[] parameters = new object[]
                {
                    macro,
                    prevPropagation,
                    historyResultCarriers,
                    historyPropagationCarriers,
                    historyDrawingCarriers,
                    historyCarrier,
                    bStatusCode,
                    strStatusMessage,
                    currPropagationCarrier,
                    currDrawingCarriers,
                    propagationCarrierHandler,
                    resultCarrierHandler
                };

                // 呼叫私有方法
                object result = extractMethod.Invoke(plugin, parameters);

                // 更新 ref 參數的值
                bStatusCode = (bool)parameters[6];
                strStatusMessage = (string)parameters[7];
                currPropagationCarrier = (UDataCarrier[])parameters[8];

                // 若 bStatusCode 為 false，表示無法抽取影格或影片已結束
                if (!bStatusCode)
                {
                    Console.WriteLine("提取結束: " + strStatusMessage);
                    break;
                }

                if (currPropagationCarrier != null && currPropagationCarrier.Length > 0)
                {
                    // Plugin 原本回傳的影格路徑 (可能在影片資料夾下的 ExtractedFramesStream)
                    string originalFramePath = currPropagationCarrier[0].Data as string;
                    count++;

                    // 重新命名/移動路徑 => 放到 videoOutputFolder 下
                    string newFramePath = Path.Combine(videoOutputFolder, $"{videoName}_{count}.jpg");
                    try
                    {
                        // 若新檔已存在，先刪除
                        if (File.Exists(newFramePath))
                            File.Delete(newFramePath);

                        if (!string.IsNullOrEmpty(originalFramePath) && File.Exists(originalFramePath))
                        {
                            File.Move(originalFramePath, newFramePath);
                            Console.WriteLine($"提取到第 {count} 張影格：{newFramePath}");
                        }
                        else
                        {
                            Console.WriteLine($"提取到第 {count} 張影格，但無法找到檔案：{originalFramePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("重新命名/移動影格檔案失敗：" + ex.Message);
                    }

                    // 計算秒數標記 (假設第一張影格對應 intervalSeconds 秒)
                    int secondsMark = count * intervalSeconds;
                    mappingList.Add((secondsMark, $"{videoName}_{count}.jpg"));
                }
                else
                {
                    Console.WriteLine("未獲得任何影格輸出，可能影片已結束。");
                    break;
                }
            }

            // 10. 呼叫 MacroShellDoneCall 進行清理（例如釋放 VideoCapture）
            MethodInfo doneMethod = typeof(uMProvidVideoStreamToFrame)
                .GetMethod("MacroShellDoneCall_ExtractNextFrame", BindingFlags.NonPublic | BindingFlags.Instance);
            if (doneMethod != null)
            {
                object doneResult = doneMethod.Invoke(plugin, new object[] { macro.MethodName, macro });
                Console.WriteLine("完成 MacroShellDoneCall: " + doneResult);
            }

            // 11. 寫出 ini 檔 (config_video.ini)
            // 格式：
            // [System Section]
            // intervalSeconds=2
            //
            // [原始檔名]
            // 2s=影片檔名_1.jpg
            // 4s=影片檔名_2.jpg
            string iniPath = Path.Combine(videoOutputFolder, "config.ini");
            using (StreamWriter sw = new StreamWriter(iniPath))
            {
                sw.WriteLine("[System Section]");
                sw.WriteLine($"intervalSeconds={intervalSeconds}");
                sw.WriteLine();
                sw.WriteLine($"[{videoName}]");
                for (int i = 0; i < mappingList.Count; i++)
                {
                    var (seconds, fileName) = mappingList[i];
                    sw.WriteLine($"{seconds}s={fileName}");
                }
            }
            Console.WriteLine("已寫出 ini 檔至：" + iniPath);


            Console.WriteLine("測試完成，按任意鍵退出。");
            Console.ReadKey();
        }--*/
        }
    }
}

