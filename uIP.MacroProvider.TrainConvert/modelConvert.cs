using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net.Sockets;

namespace uIP.MacroProvider.TrainingConvert
{
    public partial class modelConvert : Form
    {
        public modelConvert()
        {
            InitializeComponent();
            bt_SelectModel.Click += new EventHandler(bt_SelectModel_Click);
            bt_Run.Click += new EventHandler(bt_Run_Click);
        }

        private void bt_SelectModel_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Model files (*.h5;*.pt;*.onnx)|*.h5;*.pt;*.onnx|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    tB_model.Text = openFileDialog.FileName;
                }
            }
        }

        private void bt_Run_Click(object sender, EventArgs e)
        {
            string pythonExe = @"C:\Users\YourUser\anaconda3\python.exe"; // 替換為你的 Python 位置
            string scriptPath = @"C:\path\to\your\script.py"; // 替換為你的 Python 轉換腳本

            string inputModel = tB_model.Text;
            string outputModel = tB_ToModel.Text;

            if (string.IsNullOrEmpty(inputModel) || string.IsNullOrEmpty(outputModel))
            {
                MessageBox.Show("請選擇輸入模型並指定輸出模型名稱!", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = pythonExe,
                    Arguments = $"\"{scriptPath}\" --input \"{inputModel}\" --output \"{outputModel}\"",
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

                    if (!string.IsNullOrEmpty(error))
                    {
                        MessageBox.Show($"轉換失敗: {error}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"轉換成功! \n{output}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"執行 Python 失敗: {ex.Message}", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
