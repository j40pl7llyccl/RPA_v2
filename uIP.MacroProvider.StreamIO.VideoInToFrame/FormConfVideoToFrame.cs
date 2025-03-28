using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using uIP.Lib;
using uIP.Lib.Script;

namespace uIP.MacroProvider.StreamIO.VideoInToFrame
{
    public partial class FormConfVideoToFrame : Form
    {

        internal UMacro MacroInstance { get; set; } = null;

        public FormConfVideoToFrame()
        {
            InitializeComponent();
        }

        internal FormConfVideoToFrame UpdateToUI()
        {
            if (MacroInstance == null) return this;
            textBoxVideo.Text = UDataCarrier.GetDicKeyStrOne(MacroInstance.MutableInitialData, VideoStreamIndex.VideoPath.ToString(), "");
            textBoxIntervalDouble.Text = UDataCarrier.GetDicKeyStrOne(MacroInstance.MutableInitialData, VideoStreamIndex.FrameInterval.ToString(), "10.0");
            return this;
        }
        //選取影片按鈕
        private void bt_Path_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                // Set file dialog properties
                openFileDialog.Title = "Select Video File";
                openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mov;*.wmv;*.mkv|All Files|*.*";
                openFileDialog.Multiselect = false;

                // Show the dialog and check if user selected a file
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Find the TextBox by name and set its text to the selected file path
                    textBoxVideo.Text = openFileDialog.FileName;
                }
            }
        }
        //選取輸入秒數

        //執行ok存入參數的事件
        private void bt_Ok_Click(object sender, EventArgs e)
        {
            if (MacroInstance == null) return;

            // 安全地存儲參數
            UDataCarrier.SetDicKeyStrOne(
                MacroInstance.MutableInitialData,
                VideoStreamIndex.VideoPath.ToString(),
                textBoxVideo.Text
            );
            UDataCarrier.SetDicKeyStrOne(
                MacroInstance.MutableInitialData,
                VideoStreamIndex.FrameInterval.ToString(),
                textBoxIntervalDouble.Text
            );

            if (UDataCarrier.GetDicKeyStrOne<Form>(MacroInstance.MutableInitialData, VideoStreamIndex.Form.ToString(), null, out var frm))
            {
                frm.DialogResult = DialogResult.OK;
                frm.Close();
            }
            else
            {
                // 如果没有找到Form，直接关闭当前窗体
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

        }
        private void ApplyVideoSettings(UMacro m)
        {
            // 類似 ApplyFolderSettings 的邏輯
            if (!UDataCarrier.Get<Dictionary<string, UDataCarrier>>(m.MutableInitialData, null, out var dic))
                return;

            // 驗證視頻路徑
            var videoReady =
                UDataCarrier.Get(dic, VideoStreamIndex.VideoPath.ToString(), "", out var videoPath) &&
                !string.IsNullOrEmpty(videoPath) && File.Exists(videoPath);

            // 驗證切割間隔
            var intervalValid =
                UDataCarrier.Get(dic, VideoStreamIndex.FrameInterval.ToString(), "", out var intervalStr) &&
                double.TryParse(intervalStr, out double interval) &&
                interval > 0 && interval <= 3600;

        }

        private void intervalLabel_Click(object sender, EventArgs e)
        {
            // 不需處理
        }

        private void label_progress_Click(object sender, EventArgs e)
        {
            // 不需處理
        }

        private void intervalTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxPickedDir_TextChanged(object sender, EventArgs e)
        {
            label_progress.Text = "";
        }

        private void textBoxImage_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
