namespace uIP.MacroProvider.TrainingConvert
{
    partial class modelConvert
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.bt_SelectModel = new System.Windows.Forms.Button();
            this.tB_model = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tB_ToModel = new System.Windows.Forms.TextBox();
            this.bt_Run = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // bt_SelectModel
            // 
            this.bt_SelectModel.Font = new System.Drawing.Font("新細明體", 12F);
            this.bt_SelectModel.Location = new System.Drawing.Point(48, 56);
            this.bt_SelectModel.Name = "bt_SelectModel";
            this.bt_SelectModel.Size = new System.Drawing.Size(196, 46);
            this.bt_SelectModel.TabIndex = 0;
            this.bt_SelectModel.Text = "Select model";
            this.bt_SelectModel.UseVisualStyleBackColor = true;
            this.bt_SelectModel.Click += new System.EventHandler(this.bt_SelectModel_Click);
            // 
            // tB_model
            // 
            this.tB_model.Location = new System.Drawing.Point(313, 65);
            this.tB_model.Name = "tB_model";
            this.tB_model.Size = new System.Drawing.Size(256, 36);
            this.tB_model.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 12F);
            this.label1.Location = new System.Drawing.Point(65, 177);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 32);
            this.label1.TabIndex = 2;
            this.label1.Text = "ToModel:";
            // 
            // tB_ToModel
            // 
            this.tB_ToModel.Location = new System.Drawing.Point(313, 173);
            this.tB_ToModel.Name = "tB_ToModel";
            this.tB_ToModel.Size = new System.Drawing.Size(256, 36);
            this.tB_ToModel.TabIndex = 3;
            // 
            // bt_Run
            // 
            this.bt_Run.Font = new System.Drawing.Font("新細明體", 12F);
            this.bt_Run.Location = new System.Drawing.Point(471, 325);
            this.bt_Run.Name = "bt_Run";
            this.bt_Run.Size = new System.Drawing.Size(98, 40);
            this.bt_Run.TabIndex = 4;
            this.bt_Run.Text = "Run";
            this.bt_Run.UseVisualStyleBackColor = true;
            this.bt_Run.Click += new System.EventHandler(this.bt_Run_Click);
            // 
            // modelConvert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 427);
            this.Controls.Add(this.bt_Run);
            this.Controls.Add(this.tB_ToModel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tB_model);
            this.Controls.Add(this.bt_SelectModel);
            this.Name = "modelConvert";
            this.Text = "modelConvert";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bt_SelectModel;
        private System.Windows.Forms.TextBox tB_model;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tB_ToModel;
        private System.Windows.Forms.Button bt_Run;
    }
}