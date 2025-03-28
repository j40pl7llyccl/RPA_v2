using System;
using System.Windows.Forms;

namespace uIP.MacroProvider.StreamIO.VideoInToFrame
{
    partial class FormConfVideoToFrame : Form
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.label_progress = new System.Windows.Forms.Label();
            this.bt_Ok = new System.Windows.Forms.Button();
            this.bt_Path = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxVideo = new System.Windows.Forms.TextBox();
            this.textBoxIntervalDouble = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 23);
            this.label3.TabIndex = 0;
            // 
            // label_progress
            // 
            this.label_progress.AllowDrop = true;
            this.label_progress.AutoEllipsis = true;
            this.label_progress.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_progress.Location = new System.Drawing.Point(142, 154);
            this.label_progress.Name = "label_progress";
            this.label_progress.Size = new System.Drawing.Size(347, 20);
            this.label_progress.TabIndex = 6;
            // 
            // bt_Ok
            // 
            this.bt_Ok.Location = new System.Drawing.Point(145, 128);
            this.bt_Ok.Name = "bt_Ok";
            this.bt_Ok.Size = new System.Drawing.Size(75, 23);
            this.bt_Ok.TabIndex = 7;
            this.bt_Ok.Text = "Ok";
            this.bt_Ok.UseVisualStyleBackColor = true;
            this.bt_Ok.Click += new System.EventHandler(this.bt_Ok_Click);
            // 
            // bt_Path
            // 
            this.bt_Path.Location = new System.Drawing.Point(12, 27);
            this.bt_Path.Name = "bt_Path";
            this.bt_Path.Size = new System.Drawing.Size(98, 32);
            this.bt_Path.TabIndex = 8;
            this.bt_Path.Text = "SelectVideoPath";
            this.bt_Path.Click += new System.EventHandler(this.bt_Path_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(12, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 20);
            this.label4.TabIndex = 9;
            this.label4.Text = "切割間隔(秒):";
            // 
            // textBoxVideo
            // 
            this.textBoxVideo.Location = new System.Drawing.Point(145, 37);
            this.textBoxVideo.Name = "textBoxVideo";
            this.textBoxVideo.Size = new System.Drawing.Size(283, 22);
            this.textBoxVideo.TabIndex = 10;
            this.textBoxVideo.TextChanged += new System.EventHandler(this.textBoxImage_TextChanged);
            // 
            // textBoxIntervalDouble
            // 
            this.textBoxIntervalDouble.Location = new System.Drawing.Point(145, 85);
            this.textBoxIntervalDouble.Name = "textBoxIntervalDouble";
            this.textBoxIntervalDouble.Size = new System.Drawing.Size(134, 22);
            this.textBoxIntervalDouble.TabIndex = 11;
            // 
            // FormConfVideoToFrame
            // 
            this.ClientSize = new System.Drawing.Size(443, 171);
            this.Controls.Add(this.textBoxIntervalDouble);
            this.Controls.Add(this.textBoxVideo);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.bt_Path);
            this.Controls.Add(this.bt_Ok);
            this.Controls.Add(this.label_progress);
            this.Controls.Add(this.label3);
            this.Name = "FormConfVideoToFrame";
            this.Text = "Video cutting tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Button button_pickFolder;
        private Label label1;
        private TextBox textBox_intervalSecond;
        private Button Extract;
        private TextBox textBox_pickedDir;
        private Button button1;
        private TextBox textBox1;
        private Label label2;
        private TextBox textBox2;
        private Button button_extract;
        private Label label3;
        private Label label_progress;
        private Button bt_Ok;
        private Button bt_Path;
        private Label label4;
        private TextBox textBoxVideo;
        private TextBox textBoxIntervalDouble;
    }
}