using System;
using System.Windows.Forms;

using uIP.Lib;
using uIP.Lib.Script;

namespace uIP.MacroProvider.Resulting.DrawResult
{
    public partial class FormEditDisplayFormTitle : Form
    {
        internal UMacro WorkWith { get; set; } = null;
        public FormEditDisplayFormTitle()
        {
            InitializeComponent();
        }

        internal FormEditDisplayFormTitle UpdateToUI()
        {
            if (WorkWith == null) return this;
            textBox_title.Text = UDataCarrier.GetDicKeyStrOne(WorkWith.MutableInitialData, MutableDataKey.Param_FormTitle.ToString(), "Display");
            checkBox_enableDrawing.Checked = UDataCarrier.GetDicKeyStrOne(WorkWith.MutableInitialData, MutableDataKey.Param_EnableDraw.ToString(), true);
            checkBox_showResult.Checked = UDataCarrier.GetDicKeyStrOne(WorkWith.MutableInitialData, MutableDataKey.Param_ShowResult.ToString(), true);

            return this;
        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            if (WorkWith == null) return;

            if (UDataCarrier.GetDicKeyStrOne<Form>(WorkWith.MutableInitialData, MutableDataKey.Form.ToString(), null, out var frm))
            {
                frm.Text = textBox_title.Text;
                if (!checkBox_showResult.Checked)
                    frm.Hide();
            }

            UDataCarrier.SetDicKeyStrOne(WorkWith.MutableInitialData, MutableDataKey.Param_FormTitle.ToString(), textBox_title.Text);
            UDataCarrier.SetDicKeyStrOne(WorkWith.MutableInitialData, MutableDataKey.Param_EnableDraw.ToString(), checkBox_enableDrawing.Checked);
            UDataCarrier.SetDicKeyStrOne(WorkWith.MutableInitialData, MutableDataKey.Param_ShowResult.ToString(), checkBox_showResult.Checked);
        }

        private void textBox_title_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_cancel_Click(object sender, EventArgs e)
        {

        }
    }
}
