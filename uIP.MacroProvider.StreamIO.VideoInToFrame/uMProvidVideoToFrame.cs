using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using uIP.Lib;
using uIP.Lib.DataCarrier;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.VideoInToFrame
{
    // 定義 UDataCarrier 陣列中各項的索引

    public class uMProvidVideoStreamToFrame : UMacroMethodProviderPlugin
    {
        private const string ExtractNextFrameMethodName = "VideoDev_ExtractNextFrame";

        public uMProvidVideoStreamToFrame() : base()
        {
            m_strInternalGivenName = "VideoStreamToFrame";
        }

        public override bool Initialize(UDataCarrier[] param)
        {
            m_UserQueryOpenedMethods.Add(
                new UMacro(null, m_strCSharpDefClassName, ExtractNextFrameMethodName, ExtractNextFrame,
                           null, // immutable
                           null, // variable
                           null, // 前置輸入要求
                           new UDataCarrierTypeDescription[] {
                               new UDataCarrierTypeDescription(typeof(string), "Extracted frame file path")
                           }
                )
            );
            // config variable

            // config the macro GET/SET
            // fill macro get/set
            // - list all available names
            // - if multiple methods use same name, check by UMacro MethodName
            // - if own by one method, not check the MethodName

            m_MacroControls.Add("InputVideoInterval", new UScriptControlCarrierMacro("InputVideoInterval", true, true, true,
                new UDataCarrierTypeDescription[] { new UDataCarrierTypeDescription(typeof(double), "Input VideoInterval") },
                new fpGetMacroScriptControlCarrier(IoctrlGet_intervalSeconds), new fpSetMacroScriptControlCarrier(IoctrlSet_intervalSeconds)));

            m_createMacroDoneFromMethod.Add(ExtractNextFrameMethodName, MacroShellDoneCall_ExtractNextFrame);
            m_macroMethodConfigPopup.Add(ExtractNextFrameMethodName, PopupConf_ExtractNextFrame);

            m_bOpened = true;
            return true;
        }
        #region LoadingVideoDir parameter GET/SET

        private bool IoctrlSet_intervalSeconds(UScriptControlCarrier carrier, UMacro whichMacro, UDataCarrier[] data)
        {
            if (whichMacro.MethodName == ExtractNextFrameMethodName)
            {
                var path = UDataCarrier.GetItem(data, 0, "", out var status);
                var intervalSeconds = UDataCarrier.GetItem(data, 1, 0.0, out var status2);
                whichMacro.MutableInitialData = MakeVideoStreamInitData(path, intervalSeconds);
                return true;
            }

            return false;
        }

        private UDataCarrier[] IoctrlGet_intervalSeconds(UScriptControlCarrier carrier, UMacro whichMacro, ref bool bRetStatus)
        {
            if (whichMacro.MethodName == ExtractNextFrameMethodName)
            {
                UDataCarrier[] ret = null;
                UDataCarrier[] innerD = whichMacro.MutableInitialData?.Data as UDataCarrier[] ?? null;
                if (innerD != null && innerD.Length > ((int)VideoStreamIndex.FrameInterval))
                {
                    var path = UDataCarrier.GetItem(innerD, (int)VideoStreamIndex.FrameInterval, "", out var dummy);
                    ret = UDataCarrier.MakeOneItemArray(path);
                    bRetStatus = true;
                }
                return ret;
            }

            bRetStatus = true;
            return null;
        }
        /// <summary>
        /// 初始化連續數據流用的 UDataCarrier，內含影片路徑、切割間隔、當前影格、讀取器與幀率等資料
        /// 使用 OpenCvSharp 的 VideoCapture 取代 VideoFileReader
        /// </summary>
        internal static UDataCarrier MakeVideoStreamInitData(string videoPath, double intervalSeconds)
        {
            if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
            {
                // 使用 OpenCvSharp 的 VideoCapture 開啟影片
                VideoCapture capture = new VideoCapture(videoPath);
                if (!capture.IsOpened())
                    return null;
                double frameRate = capture.Fps;
                int frameInterval = (int)Math.Round(intervalSeconds * frameRate);
                if (frameInterval <= 0)
                    frameInterval = 1;
                long currentFrame = 0;

                object[] data = new object[(int)VideoStreamIndex.MaxCount];
                data[(int)VideoStreamIndex.VideoPath] = videoPath;
                data[(int)VideoStreamIndex.FrameInterval] = frameInterval;
                //data[(int)VideoStreamIndex.CurrentFrame] = currentFrame;
                data[(int)VideoStreamIndex.ReaderInstance] = capture;
                data[(int)VideoStreamIndex.FrameRate] = frameRate;

                return UDataCarrier.MakeOne(data, true);
            }
            return null;
        }

        // 回傳設定此 Macro 的 UI 視窗 (使用者可在 UI 上選擇影片、輸入切割秒數)
        private Form PopupConf_ExtractNextFrame(string callMethodName, UMacro macroToConf)
        {
            return new FormConfVideoToFrame() { MacroInstance = macroToConf }.UpdateToUI(); ;
        }

        private bool MacroShellDoneCall_ExtractNextFrame(string callMethodName, UMacro instance)
        {
            // Macro 完成後可進行額外處理 (例如關閉影片流)
            // 此處可選擇釋放 VideoCapture 物件
            //if (instance.MutableInitialData != null && instance.MutableInitialData.Data is object[] data)
            //{
            //    VideoCapture capture = data[(int)VideoStreamIndex.ReaderInstance] as VideoCapture;
            //    capture?.Release();
            //}
            //return true;

            instance.MutableInitialData = UDataCarrier.MakeOne(new Dictionary<string, UDataCarrier>()
            {
                { VideoStreamIndex.FrameInterval.ToString(), UDataCarrier.MakeOne(0.8) }, // intervalSeconds
                //{ VideoStreamIndex.ReaderInstance.ToString(), UDataCarrier.MakeOne(new VideoCapture(), true) }, // opencv capture binstance
             });
            return true;
        }

        /// <summary>
        /// 連續數據流情境下，每次呼叫此方法抽取下一個符合切割間隔的影格
        /// 使用 OpenCvSharp 取代 VideoFileReader，並排除 0 秒影格（即第一張抽取影格必須在設定秒數後才開始）
        /// </summary>
        private UDataCarrier[] ExtractNextFrame(UMacro MacroInstance,
                                                UDataCarrier[] PrevPropagationCarrier,
                                                List<UMacroProduceCarrierResult> historyResultCarriers,
                                                List<UMacroProduceCarrierPropagation> historyPropagationCarriers,
                                                List<UMacroProduceCarrierDrawingResult> historyDrawingCarriers,
                                                List<UScriptHistoryCarrier> historyCarrier,
                                                ref bool bStatusCode, ref string strStatusMessage,
                                                ref UDataCarrier[] CurrPropagationCarrier,
                                                ref UDrawingCarriers CurrDrawingCarriers,
                                                ref fpUDataCarrierSetResHandler PropagationCarrierHandler,
                                                ref fpUDataCarrierSetResHandler ResultCarrierHandler)
        {
            if (!UDataCarrier.GetByIndex(PrevPropagationCarrier, 0, "", out var videoPath)) //public static bool GetByIndex<T>( UDataCarrier[] arr, int index, T def, out T ret )
            {
                bStatusCode = false;
                strStatusMessage = "未接收到上一個傳來的參數值";
                return null;
            }


            if (!UDataCarrier.GetDicKeyStrOne(MacroInstance.MutableInitialData, VideoStreamIndex.FrameInterval.ToString(), 0.0, out double frameInt)) //GetDicKeyStrOne<T>( UDataCarrier carr, string k, T defaultV, out T v )
            {
                strStatusMessage = "no frame interval";
                return null;
            }

            // 建立 VideoCapture實例
            var capture = new VideoCapture(videoPath);
            if (!capture.IsOpened())
            {
                bStatusCode = false;
                strStatusMessage = "VideoCapture 未初始化或影片無法開啟。";
                return null;
            }

            // 計算 FPS 與幀數間隔
            double fps = capture.Fps;
            int frameIntervalFrames = (int)Math.Round(frameInt * fps);
            if (frameIntervalFrames <= 0)
                frameIntervalFrames = 1;

            int currentFrame = 0;
            int fileIndex = 0;
            string lastOutputFile = null;

            // 用於存儲每個提取影格對應的時間和檔案名稱的映射
            List<string> mappingLines = new List<string>();

            // 建立輸出目錄
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine("currentDir: " + currentDir);
            string videoName = Path.GetFileNameWithoutExtension(videoPath);
            string outputDir = Path.Combine(currentDir, "Video_output_v2", videoName);
            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);

            // 開始批次讀取，並用 using 在執行完成後自動釋放 VideoCapture 物件
            using (capture)
            {
                while (true)
                {
                    // 建立一個新的 Mat 讀取當前 frame
                    using (Mat matFrame = new Mat())
                    {
                        bool success = capture.Read(matFrame);
                        if (!success || matFrame.Empty())
                            break; // 影片讀取完畢，離開迴圈

                        currentFrame++;

                        // 每隔 frameIntervalFrames 幀存圖一次
                        if (currentFrame % frameIntervalFrames == 0)
                        {
                            using (Bitmap bmpFrame = BitmapConverter.ToBitmap(matFrame))
                            {
                                string outputFile = Path.Combine(
                                    outputDir,
                                    $"{videoName}_{currentFrame:D5}.png"
                                );
                                bmpFrame.Save(outputFile);
                                lastOutputFile = outputFile; // 將存檔路徑加入清單
                                fileIndex++;

                                // 計算對應的時間（秒），並加入映射，取檔名部分即可
                                double timeKey = fileIndex * frameInt;
                                mappingLines.Add($"{timeKey:0.0}={Path.GetFileName(outputFile)}");
                            }
                        }
                    }
                }
            }


            // 檢查是否有擷取到任何圖片
            if (string.IsNullOrEmpty(lastOutputFile))
            {
                bStatusCode = false;
                strStatusMessage = "無法抽取影格或影片結束(未產生任何圖片)。";
                return null;
            }

            // 產生 ini 檔案，寫入相關參數
            string iniPath = Path.Combine(outputDir, "config.ini");
            var iniContent = new System.Text.StringBuilder();
            iniContent.AppendLine("[Video]");
            iniContent.AppendLine($"VideoPath={videoPath}");
            iniContent.AppendLine($"VideoName={videoName}");
            iniContent.AppendLine($"FrameIntervalSeconds={frameInt}");
            iniContent.AppendLine($"FrameIntervalFrames={frameIntervalFrames}");
            iniContent.AppendLine($"FPS={fps}");
            iniContent.AppendLine($"TotalFramesExtracted={fileIndex}");
            iniContent.AppendLine($"LastOutputFile={lastOutputFile}");
            iniContent.AppendLine();

            // 新增每個時間點的映射，格式如 "2.0=videoName_00060.png"
            foreach (var mapping in mappingLines)
            {
                iniContent.AppendLine(mapping);
            }

            try
            {
                System.IO.File.WriteAllText(iniPath, iniContent.ToString());
            }
            catch (Exception ex)
            {
                bStatusCode = false;
                strStatusMessage = "寫入 ini 檔案失敗：" + ex.Message;
                return null;
            }

            // 回傳結果：將所有圖片的路徑以陣列回傳給 Macro 系統
            bStatusCode = true;
            strStatusMessage = "成功抽取並存檔影格，且產生 ini 設定檔。";
            CurrPropagationCarrier = UDataCarrier.MakeVariableItemsArray(outputDir, iniPath);
            return null;
        }
    }
}
#endregion