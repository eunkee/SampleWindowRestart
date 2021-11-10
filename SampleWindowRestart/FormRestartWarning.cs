using System;
using System.Windows.Forms;

namespace SampleWindowRestart
{
    public partial class FormRestartWarning : Form
    {
        private new readonly Form1 ParentForm;
        public FormRestartWarning(Form1 form)
        {
            this.ParentForm = form;
            InitializeComponent();
        }

        //Mainfrm에서 라벨 시간 지정
        public void SetLabelTime(string timetext)
        {
            label3.Text = timetext;
        }

        //종료 방지
        private void FormRestartWarning_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!ParentForm.IsMainExit)
            {
                if (MessageBox.Show("취소하시겠습니까?", "Warning", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    ParentForm.IsUserDefineRestartCancel = true;
                    this.Visible = false;
                }

                e.Cancel = true;
            }
        }

        //재시작 실행
        private void ButtonOK_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("즉시 재시작 하시겠습니까?", "Warning", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                this.Visible = false;
                CommandRunWindowRestart.RunRestart();
            }
        }

        //재시작 취소
        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("취소하시겠습니까?", "Warning", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                ParentForm.IsUserDefineRestartCancel = true;
                this.Visible = false;
            }
        }
    }
}
