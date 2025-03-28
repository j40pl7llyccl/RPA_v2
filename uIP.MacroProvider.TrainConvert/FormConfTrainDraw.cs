using System;
using System.Diagnostics; // ← 用於呼叫 Python & 讀取輸出
using System.Drawing;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace uIP.MacroProvider.TrainConvert
{
    public partial class FormConfTrainDraw : Form
    {
        public Process trainingProcess; // 用於保存 Python 訓練的 Process 物件

        public string SelectedModel => cB_Model.Text;
        public string SelectedDataSets => cB_DataSets.Text;
        public string SelectedWeights => cB_Weights.Text;
        public string BatchSize => tB_BatchSize.Text;
        public string Epochs => tB_Epochs.Text;
        public string Hyps => cB_Hyps.Text;

        public static FormConfTrainDraw StaticInstance { get; set; }


        /// <summary>
        /// 建構子：呼叫 InitializeComponent()，但不包含控制項的手動初始化
        /// 都假設寫在 .Designer.cs 檔裡面
        /// </summary>
        public FormConfTrainDraw()
        {
            InitializeComponent();
            //this.Load += new EventHandler(FormConfTrainDraw_Load); // 註冊 Load 事件
        }

        /// <summary>
        /// 在表單加載時初始化圖表網格
        /// </summary>
        private void FormConfTrainDraw_Load(object sender, EventArgs e)
        {
            InitializeCharts();
        }
        private void InitializeCharts()
        {
            Chart[] charts = { chart_train_box_loss, chart_train_obj_loss, chart_train_cls_loss, chart_val_box_loss, chart_val_obj_loss, chart_val_cls_loss };
            foreach (var chart in charts)
            {
                if (chart.ChartAreas.Count > 0)
                {
                    ChartArea area = chart.ChartAreas[0];

                    // 設定 X 軸的網格線
                    area.AxisX.MajorGrid.Enabled = true;
                    area.AxisX.MajorGrid.LineColor = Color.LightGray;
                    area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

                    // 設定 Y 軸的網格線
                    area.AxisY.MajorGrid.Enabled = true;
                    area.AxisY.MajorGrid.LineColor = Color.LightGray;
                    area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

                    // 設定 X 軸 & Y 軸範圍，避免圖表完全空白
                    area.AxisX.Minimum = 0;
                    area.AxisX.Maximum = 10;
                    area.AxisX.Interval = 1;

                    area.AxisY.Minimum = 0;
                    area.AxisY.Maximum = 1;
                    area.AxisY.Interval = 0.1;
                }
            }
        }
        /// <summary>
        /// [Run] 按鈕事件：啟動 Python 並開始讀取訓練過程
        /// </summary>
        private void bt_Run_Click(object sender, EventArgs e)
        {
            // 1) 取得使用者在 WinForm UI 中的參數
            string model = cB_Model.Text;
            string dataset = cB_DataSets.Text;
            string weights = cB_Weights.Text;
            string batchSize = tB_BatchSize.Text;
            string epochs = tB_Epochs.Text;
            string hyps = cB_Hyps.Text;
            Console.WriteLine("model: " + model);
            Console.WriteLine("dataset: " + dataset);
            Console.WriteLine("weights: " + weights);
            Console.WriteLine("batchSize: " + batchSize);
            Console.WriteLine("epochs: " + epochs);
            Console.WriteLine("hyps: " + hyps);
            if (string.IsNullOrEmpty(model) || string.IsNullOrEmpty(dataset) || string.IsNullOrEmpty(weights) || string.IsNullOrEmpty(batchSize) || string.IsNullOrEmpty(epochs) || string.IsNullOrEmpty(hyps))
            {
                MessageBox.Show("請確認所有參數皆已輸入！");
                return;
            }

            // 2)確認是否有安裝相關套件

            string requirementsFile = @"C:\Users\MIP4070\Desktop\yolov5-master\requirements.txt"; //C: \Users\MIP4070\Desktop\yolov5 - master\requirements.txt

            // 組成指令： python -m pip install -r "requirements.txt"
            ProcessStartInfo py_insatll = new ProcessStartInfo()
            {
                FileName = "python",
                Arguments = $"-m pip install -r \"{requirementsFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            Process installProcess = new Process() { StartInfo = py_insatll };

            installProcess.OutputDataReceived += (s, pipArgs) =>
            {
                if (!string.IsNullOrEmpty(pipArgs.Data))
                    Console.WriteLine("[PIP 輸出]: " + pipArgs.Data);
            };

            installProcess.ErrorDataReceived += (s, pipErrArgs) =>
            {
                if (!string.IsNullOrEmpty(pipErrArgs.Data))
                    Console.WriteLine("[PIP 錯誤]: " + pipErrArgs.Data);
            };

            try
            {
                installProcess.Start();
                installProcess.BeginOutputReadLine();
                installProcess.BeginErrorReadLine();
                installProcess.WaitForExit(); // 等待完成
                //MessageBox.Show("套件安裝完成!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法執行 pip install: {ex.Message}");
            }
            /*------------------------------------------------------------------------------------------------------------------------*/
            // 3) 組合成 Python 執行參數 (train.py + 相關參數)
            string pythonFile = @"C:\Users\MIP4070\Desktop\yolov5-master\train.py";
            string args = $"\"{pythonFile}\" --data C:\\Users\\MIP4070\\Desktop\\yolov5-master\\data\\{dataset} " +
                          $"--cfg  C:\\Users\\MIP4070\\Desktop\\yolov5-master\\models\\{model} " +
                          $"--weights  C:\\Users\\MIP4070\\Desktop\\yolov5-master\\weights\\{weights} " +
                          $"--hyp C:\\Users\\MIP4070\\Desktop\\yolov5-master\\data\\hyps\\{hyps}" +
                          $"--batch-size {batchSize} " +
                          $"--epochs {epochs} " +
                          $"--device cuda:0";

            ProcessStartInfo psi = new ProcessStartInfo()
            {
                FileName = "python",
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            trainingProcess = new Process { StartInfo = psi };
            trainingProcess.OutputDataReceived += Process_OutputDataReceived;

            trainingProcess.ErrorDataReceived += Process_ErrorDataReceived;

            trainingProcess.Exited += (procSender, evtArgs) =>
            {
                MessageBox.Show(" 訓練已執行完畢！", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            trainingProcess.EnableRaisingEvents = true;

            try
            {
                trainingProcess.Start();
                trainingProcess.BeginOutputReadLine();
                trainingProcess.BeginErrorReadLine();

                // 使用非同步等待，避免UI凍結
                Task.Run(() =>
                {
                    trainingProcess.WaitForExit();
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show(" 訓練程序已完成！", "完成提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"無法啟動 程序: {ex.Message}");
            }
        }
        /// <summary>
        /// 即時讀取 Python stdout，每有一行輸出便會觸發
        /// </summary>
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
                return;

            string line = e.Data.Trim();
            Debug.WriteLine($"e.Data Type: {e.Data.GetType().Name}, Value: {e.Data}");

            // 1**如果是 JSON 格式才解析**
            if (line.StartsWith("{") && line.EndsWith("}"))
            {
                try
                {
                    var dataObj = JsonSerializer.Deserialize<JsonElement>(line);

                    int epoch = dataObj.TryGetProperty("epoch", out JsonElement epochElement) && epochElement.TryGetInt32(out int epochValue) ? epochValue : -1;
                    double train_box_loss = dataObj.TryGetProperty("train_box_loss", out JsonElement trainBoxLossElement) && trainBoxLossElement.TryGetDouble(out double trainBoxLoss) ? trainBoxLoss : 0;
                    double train_obj_loss = dataObj.TryGetProperty("train_obj_loss", out JsonElement trainObjLossElement) && trainObjLossElement.TryGetDouble(out double trainObjLoss) ? trainObjLoss : 0;
                    double train_cls_loss = dataObj.TryGetProperty("train_cls_loss", out JsonElement trainClsLossElement) && trainClsLossElement.TryGetDouble(out double trainClsLoss) ? trainClsLoss : 0;
                    double val_box_loss = dataObj.TryGetProperty("val_box_loss", out JsonElement valBoxLossElement) && valBoxLossElement.TryGetDouble(out double valBoxLoss) ? valBoxLoss : 0;
                    double val_obj_loss = dataObj.TryGetProperty("val_obj_loss", out JsonElement valObjLossElement) && valObjLossElement.TryGetDouble(out double valObjLoss) ? valObjLoss : 0;
                    double val_cls_loss = dataObj.TryGetProperty("val_cls_loss", out JsonElement valClsLossElement) && valClsLossElement.TryGetDouble(out double valClsLoss) ? valClsLoss : 0;

                    this.Invoke((MethodInvoker)delegate
                    {
                        textBoxOutput.AppendText($"[Parsed Json] {line}" + Environment.NewLine);
                        UpdateChart(epoch, train_box_loss, train_obj_loss, train_cls_loss, val_box_loss, val_obj_loss, val_cls_loss);
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"JSON 解析失敗: {line}, 錯誤: {ex.Message}");
                }
            }
            // 2️ **解析非 JSON 的數據行**
            else if (line.Contains("train/box_loss") || line.Contains("val/box_loss"))
            {
                try
                {
                    // **手動解析 train/box_loss, val/box_loss 數值**
                    var parts = line.Split(':'); // 分割 ":" 前後
                    if (parts.Length == 2)
                    {
                        var values = parts[1].Trim().Split(' '); // 取出數值
                        if (values.Length >= 3)
                        {
                            int epoch = int.Parse(values[0]);
                            double train_box_loss = double.Parse(values[1]);
                            double train_obj_loss = double.Parse(values[2]);
                            double train_cls_loss = double.Parse(values[3]);
                            double val_box_loss = double.Parse(values[4]);
                            double val_obj_loss = double.Parse(values[5]);
                            double val_cls_loss = double.Parse(values[6]);

                            this.Invoke((MethodInvoker)delegate
                            {
                                textBoxOutput.AppendText($"[Parsed] {line}" + Environment.NewLine);
                                UpdateChart(epoch + 1, train_box_loss, train_obj_loss, train_cls_loss, val_box_loss, val_obj_loss, val_cls_loss);
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"數據解析失敗: {line}, 錯誤: {ex.Message}");
                }
            }
            else
            {
                // **一般日誌輸出**
                this.Invoke((MethodInvoker)delegate
                {
                    textBoxOutput.AppendText("[LOG] " + line + Environment.NewLine);
                });
            }
        }
        public void InvokeOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // 1) 基本空檔檢查
            if (e == null)
            {
                Console.WriteLine("DataReceivedEventArgs is null, skip processing.");
                return;
            }
            if (string.IsNullOrEmpty(e.Data))
            {
                // 你也可在這裡決定要不要跳出，或允許空字串流入
                // 以示例來說，跳出：
                Console.WriteLine("Data is empty, skip processing.");
                return;
            }
            if (e.Data.Length > 1000)
            {
                Console.WriteLine("Data too long, skip for safety or log it partially.");
                // 你也可截斷字串 e.Data[..500] 等
            }

            Process_OutputDataReceived(sender, e);
        }

        /// <summary>
        /// 讀取 Python stderr（錯誤輸出）時觸發，可印出 Log 以供除錯
        /// </summary>
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {

                this.Invoke(new Action(() =>
                {
                    textBoxOutput.AppendText(e.Data + Environment.NewLine);
                }));

                Debug.WriteLine("[stderr] " + e.Data);
            }
        }

        public void InvokeErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            // 1) 基本空檔檢查
            if (e == null)
            {
                Console.WriteLine("DataReceivedEventArgs is null, skip processing.");
                return;
            }
            if (string.IsNullOrEmpty(e.Data))
            {
                // 你也可在這裡決定要不要跳出，或允許空字串流入
                // 以示例來說，跳出：
                Console.WriteLine("Data is empty, skip processing.");
                return;
            }
            if (e.Data.Length > 1000)
            {
                Console.WriteLine("Data too long, skip for safety or log it partially.");
                // 你也可截斷字串 e.Data[..500] 等
            }
            Process_ErrorDataReceived(sender, e);
        }

        /// <summary>
        /// 根據 Python 輸出的結果，更新 6 張 Chart 的折線
        /// </summary>
        private void UpdateChart(int epoch,
                                 double train_box_loss, double train_obj_loss, double train_cls_loss,
                                 double val_box_loss, double val_obj_loss, double val_cls_loss)
        {
            try
            {
                // 確保所有需要的 Series 已存在
                EnsureSeriesExists(chart_train_box_loss, "train/box_loss", Color.Red);
                EnsureSeriesExists(chart_train_obj_loss, "train/obj_loss", Color.Green);
                EnsureSeriesExists(chart_train_cls_loss, "train/cls_loss", Color.Blue);
                EnsureSeriesExists(chart_val_box_loss, "val/box_loss", Color.Orange);
                EnsureSeriesExists(chart_val_obj_loss, "val/obj_loss", Color.Purple);
                EnsureSeriesExists(chart_val_cls_loss, "val/cls_loss", Color.Cyan);

                // 將資料加入對應 Series
                chart_train_box_loss.Series["train/box_loss"].Points.AddXY(epoch + 1, train_box_loss);
                ChartArea area_train_box_loss = chart_train_box_loss.ChartAreas[0];
                area_train_box_loss.AxisX.Title = "Epochs";
                area_train_box_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_train_box_loss.AxisX.TitleForeColor = Color.Black;

                chart_train_obj_loss.Series["train/obj_loss"].Points.AddXY(epoch + 1, train_obj_loss);
                ChartArea area_train_obj_loss = chart_train_obj_loss.ChartAreas[0];
                area_train_obj_loss.AxisX.Title = "Epochs";
                area_train_obj_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_train_obj_loss.AxisX.TitleForeColor = Color.Black;

                chart_train_cls_loss.Series["train/cls_loss"].Points.AddXY(epoch + 1, train_cls_loss);
                ChartArea area_train_cls_loss = chart_train_cls_loss.ChartAreas[0];
                area_train_cls_loss.AxisX.Title = "Epochs";
                area_train_cls_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_train_cls_loss.AxisX.TitleForeColor = Color.Black;

                chart_val_box_loss.Series["val/box_loss"].Points.AddXY(epoch + 1, val_box_loss);
                ChartArea area_val_box_loss = chart_val_box_loss.ChartAreas[0];
                area_val_box_loss.AxisX.Title = "Epochs";
                area_val_box_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_val_box_loss.AxisX.TitleForeColor = Color.Black;

                chart_val_obj_loss.Series["val/obj_loss"].Points.AddXY(epoch + 1, val_obj_loss);
                ChartArea area_val_obj_loss = chart_val_obj_loss.ChartAreas[0];
                area_val_obj_loss.AxisX.Title = "Epochs";
                area_val_obj_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_val_obj_loss.AxisX.TitleForeColor = Color.Black;

                chart_val_cls_loss.Series["val/cls_loss"].Points.AddXY(epoch + 1, val_cls_loss);
                ChartArea area_val_cls_loss = chart_val_cls_loss.ChartAreas[0];
                area_val_cls_loss.AxisX.Title = "Epochs";
                area_val_cls_loss.AxisX.TitleFont = new Font("Arial", 12, FontStyle.Bold);
                area_val_cls_loss.AxisX.TitleForeColor = Color.Black;

                // 更新圖表
                chart_train_box_loss.Update();
                chart_train_obj_loss.Update();
                chart_train_cls_loss.Update();
                chart_val_box_loss.Update();
                chart_val_obj_loss.Update();
                chart_val_cls_loss.Update();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("更新圖表時發生錯誤: " + ex.Message);
            }
        }

        private void EnsureSeriesExists(System.Windows.Forms.DataVisualization.Charting.Chart chart, string seriesName, Color lineColor)
        {
            chart.Series.Clear();
            /*--
            if (chart.Series.IndexOf(seriesName) >= 0)
                return;

            // 建立一個新的 Series
            var series = new System.Windows.Forms.DataVisualization.Charting.Series(seriesName);
            series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line; // 可依需求修改為其他圖表類型
            chart.Series.Add(series);
            --*/
            // 嘗試取得已存在的 Series
            var existingSeries = chart.Series.FindByName(seriesName);

            if (existingSeries == null)
            {
                // 若找不到，則建立新的 Series
                existingSeries = new System.Windows.Forms.DataVisualization.Charting.Series(seriesName);
                chart.Series.Add(existingSeries);
            }

            // 設定 Series 類型為折線圖 
            existingSeries.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

            // 設定折線顏色
            existingSeries.Color = lineColor;

            // 設定線條寬度
            existingSeries.BorderWidth = 2;

            if (chart.Legends.Count > 0)
            {
                chart.Legends[0].Font = new Font("Arial", 12, FontStyle.Bold); // 設定字體大小
                //chart.Legends[0].ForeColor = Color.Black; // 設定字體顏色
            }
        }
        // 以下是控制項的事件，若無特殊處理需求，可保持空白
        private void cB_Model_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cB_DataSets_SelectedIndexChanged(object sender, EventArgs e) { }
        private void cB_Weights_SelectedIndexChanged(object sender, EventArgs e) { }
        private void tB_BatchSize_TextChanged(object sender, EventArgs e) { }
        private void tB_Epochs_TextChanged(object sender, EventArgs e) { }
        private void label6_Click(object sender, EventArgs e) { }

        private void bt_Stop_Click(object sender, EventArgs e)
        {
            if (trainingProcess != null && !trainingProcess.HasExited)
            {
                Task.Run(() =>
                {
                    try
                    {
                        trainingProcess.Kill();
                        trainingProcess.WaitForExit();
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show("訓練已中止!", "通知", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }));
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(new Action(() =>
                        {
                            MessageBox.Show($"無法終止訓練程序: {ex.Message}");
                        }));
                    }
                });
            }
            else
            {
                MessageBox.Show("沒有正在執行的訓練程序。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public void ResetGraph()
        {
            chart_train_box_loss.Series["train/box_loss"].Points.Clear();
            chart_train_obj_loss.Series["train/obj_loss"].Points.Clear();
            chart_train_cls_loss.Series["train/cls_loss"].Points.Clear();
            chart_val_box_loss.Series["val/box_loss"].Points.Clear();
            chart_val_obj_loss.Series["val/obj_loss"].Points.Clear();
            chart_val_cls_loss.Series["val/cls_loss"].Points.Clear();
        }

        private void bt_convert_Click(object sender, EventArgs e)
        {
            //model conversion.pt=>onnx=>trt
            // 開啟檔案選擇對話框，選擇 .pt 模型檔
            /*--
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PyTorch Model (*.pt)|*.pt",
                Title = "選擇要轉換的 PyTorch 模型"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string ptPath = openFileDialog.FileName;
                string trtPath = Path.ChangeExtension(ptPath, ".trt");  // 轉換成 .trt 檔名

                // Python 執行檔的路徑 (確認 Python 安裝位置)
                string pythonPath = @"C:\Python39\python.exe";
                string scriptPath = @"C:\convert_to_trt.py";  // Python 腳本的路徑

                // 執行 Python 轉換命令
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = pythonPath,
                    Arguments = $"\"{scriptPath}\" --pt \"{ptPath}\" --trt \"{trtPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        MessageBox.Show($"模型轉換成功！\n輸出檔案: {trtPath}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"轉換失敗！\n錯誤訊息:\n{error}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }--*/
        }
    }
}
