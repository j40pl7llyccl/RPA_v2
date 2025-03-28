using System;
using System.IO;
using System.Windows.Forms;
// 假設此命名空間下包含 UScriptService, UMacro 等
using uIP.Lib.Script;
using uIP.Lib.Service;

namespace uIP.MacroProvider.StreamIO.DividedData
{
    public partial class DataSetSelect : Form
    {
        // 透過外部取得 UScriptService (可藉由建構子或屬性注入)
        private UScriptService scriptService;
        public UMacro MacroInstance { get; set; }
        public ProcessPath Plugin { get; set; }

        public bool M_Flag { get; set; }

        public DataSetSelect(UScriptService scriptSrv = null)
        {
            InitializeComponent();
            scriptService = scriptSrv;
            M_Flag = true;
            // 建立 Plugin 實例，並在初始化時註冊 Macro 方法
            Plugin = new ProcessPath();
            Plugin.Initialize(null);
        }

        private void bt_directory1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox1.Text = fbd.SelectedPath;
                }
            }
        }
        private void bt_directory2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox2.Text = fbd.SelectedPath;
                }
            }
        }
        private void bt_directory3_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    textBox3.Text = fbd.SelectedPath;
                }
            }
        }

        // (其他 CheckBox 事件與輔助方法略)

        /// <summary>
        /// 讀取指定路徑的檔案數（僅限 TopDirectory），供 UI 顯示比例計算使用
        /// </summary>
        private int GetFileCount(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
                return 0;
            return Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly).Length;
        }

        /// <summary>
        /// 當使用者按下 Run 按鈕時：
        /// 1. 收集 UI 輸入（路徑與各選項）並計算檔案數更新顯示
        /// 2. 設定 Macro 參數（供 Macro 流程使用）
        /// 3. 呼叫 Plugin 的公開方法 StartConvertByCode 執行轉檔作業
        /// </summary>
        private void bt_ok_Click(object sender, EventArgs e)
        {
            if (!M_Flag)
            {
                MessageBox.Show("數據定位功能已被停用。", "通知", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (MacroInstance == null)
            {
                MessageBox.Show("MacroInstance 未設定，無法進行影片提取。", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 讀取第 1 行 CheckBox 狀態
            bool r1Train = checkBox1.Checked, r1Test = checkBox2.Checked, r1Val = checkBox3.Checked;
            // 第 2 行
            bool r2Train = checkBox4.Checked, r2Test = checkBox5.Checked, r2Val = checkBox6.Checked;
            // 第 3 行
            bool r3Train = checkBox7.Checked, r3Test = checkBox8.Checked, r3Val = checkBox9.Checked;

            // (可加入同一行/同一列檢查邏輯，若有衝突則提示並中斷，此處略)

            // 計算三個路徑檔案數，並更新 UI 顯示 (選填)
            int c1 = GetFileCount(textBox1.Text);
            int c2 = GetFileCount(textBox2.Text);
            int c3 = GetFileCount(textBox3.Text);
            int total = c1 + c2 + c3;
            if (total == 0)
            {
                MessageBox.Show("3個路徑的檔案數為 0 (或都沒勾選)。", "提醒");
                return;
            }
            double trainCount = (r1Train ? c1 : 0) + (r2Train ? c2 : 0) + (r3Train ? c3 : 0);
            double testCount = (r1Test ? c1 : 0) + (r2Test ? c2 : 0) + (r3Test ? c3 : 0);
            double valCount = (r1Val ? c1 : 0) + (r2Val ? c2 : 0) + (r3Val ? c3 : 0);
            double totalCount = trainCount + testCount + valCount;
            double trainPercent = (trainCount * 100.0) / totalCount;
            double testPercent = (testCount * 100.0) / totalCount;
            double valPercent = (valCount * 100.0) / totalCount;
            labelTrainPercent.Text = $"Train: {trainCount} 檔 (約 {trainPercent:0.0}%)";
            labelTestPercent.Text = $"Test : {testCount} 檔 (約 {testPercent:0.0}%)";
            labelValPercent.Text = $"Val  : {valCount} 檔 (約 {valPercent:0.0}%)";

            // 設定 Macro 參數，供 Macro 流程使用
            Plugin.SetPath1(MacroInstance, textBox1.Text);
            Plugin.SetR1Train(MacroInstance, r1Train);
            Plugin.SetR1Test(MacroInstance, r1Test);
            Plugin.SetR1Val(MacroInstance, r1Val);

            Plugin.SetPath2(MacroInstance, textBox2.Text);
            Plugin.SetR2Train(MacroInstance, r2Train);
            Plugin.SetR2Test(MacroInstance, r2Test);
            Plugin.SetR2Val(MacroInstance, r2Val);

            Plugin.SetPath3(MacroInstance, textBox3.Text);
            Plugin.SetR3Train(MacroInstance, r3Train);
            Plugin.SetR3Test(MacroInstance, r3Test);
            Plugin.SetR3Val(MacroInstance, r3Val);

            // 呼叫 Plugin 的公開方法 StartConvertByCode 執行所有路徑的搬檔作業
            Plugin.StartConvertByCode(
                textBox1.Text, r1Train, r1Test, r1Val,
                textBox2.Text, r2Train, r2Test, r2Val,
                textBox3.Text, r3Train, r3Test, r3Val
            );

            MessageBox.Show("資料集分配完成！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
