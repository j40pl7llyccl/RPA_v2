using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;

namespace uIP.MacroProvider.StreamIO.VideoInToFrame
{
    public static class VideoFrameExtractor
    {
        public static string[] ExtractFrames(string videoPath, int intervalSeconds, string outputDir, Action<int> progressCallback)
        {
            List<string> extractedFiles = new List<string>();
            List<string> iniMappings = new List<string>();

            // 根據影片檔名取得不含副檔名的名稱
            string videoName = Path.GetFileNameWithoutExtension(videoPath);

            using (var capture = new VideoCapture(videoPath))
            {
                if (!capture.IsOpened())
                {
                    throw new Exception("無法開啟影片檔案");
                }
                double fps = capture.Fps;
                int frameInterval = (int)Math.Round(intervalSeconds * fps);
                if (frameInterval <= 0) frameInterval = 1;
                int totalFrames = (int)capture.FrameCount;
                int frameIndex = 0;
                int fileIndex = 0;
                Mat frame = new Mat();
                while (true)
                {
                    bool success = capture.Read(frame);
                    if (!success || frame.Empty())
                        break;

                    // 修改判斷：第一張必須定格在 intervalSeconds 時間（跳過 0 秒影格）
                    if (frameIndex != 0 && frameIndex % frameInterval == 0)
                    {
                        // 依照格式產生檔名，例如 "myvideo_1.jpg"
                        string outputFileName = Path.Combine(outputDir, $"{videoName}_{fileIndex + 1}.jpg");
                        // 儲存影格（以 JPEG 格式）
                        Cv2.ImWrite(outputFileName, frame);
                        extractedFiles.Add(outputFileName);
                        // 以拆解順序記錄時間（例如 30s、60s 等）與檔名對應
                        int timeSec = (fileIndex + 1) * intervalSeconds;
                        iniMappings.Add($"{timeSec}s={videoName}_{fileIndex + 1}.jpg");
                        fileIndex++;
                    }
                    frameIndex++;

                    // 回報進度（依照讀取的 frameIndex 與總幀數計算百分比）
                    int progress = (int)((frameIndex / (double)totalFrames) * 100);
                    progressCallback?.Invoke(progress);
                }
            }

            // 撰寫 ini 檔：包含 [System Section] 與 [原始檔名] 區塊
            string iniPath = Path.Combine(outputDir, "config_video.ini");
            List<string> iniLines = new List<string>();
            iniLines.Add("[System Section]");
            iniLines.Add($"intervalSeconds={intervalSeconds}");
            iniLines.Add("");
            iniLines.Add($"[{videoName}]");
            iniLines.AddRange(iniMappings);
            File.WriteAllLines(iniPath, iniLines);

            return extractedFiles.ToArray();
        }
    }
}
