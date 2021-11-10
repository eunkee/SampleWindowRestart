using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace SampleWindowRestart
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private bool IsFirstDateTimePciker = true;
        private bool IsRunRestart = false;
        public bool IsUserDefineRestartCancel = false;
        public bool IsMainExit = false;

        //레지스트리 키 경로
        private static readonly RegistryKey regKey = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\SampleWindowRestart", true);
        private FormRestartWarning formRestartWarning;

        private static bool CheckRunThisProcess()
        {
            bool rslt = false;
            int processcount = 0;
            System.Diagnostics.Process[] procs;
            procs = System.Diagnostics.Process.GetProcesses();
            foreach (System.Diagnostics.Process aProc in procs)
            {
                if (aProc.ProcessName.ToString().Equals("AutoWindowRestart"))
                {
                    processcount++;
                    if (processcount > 1)
                    {
                        rslt = true;
                        break;
                    }
                }
            }
            return rslt;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (CheckRunThisProcess())
            {
                Application.Exit();
            }

            //경고창 생성
            formRestartWarning = new(this)
            {
                Visible = false
            };

            //trayicon <= Contextmenu 적용
            notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;

            //시 분 표시 형태
            dateTimePicker1.CustomFormat = "재부팅시간 HH시 mm분";

            //default
            DateTime dateTime = new(2017, 11, 11, 23, 0, 0);
            dateTimePicker1.Value = dateTime;
            checkBox1.CheckState = CheckState.Unchecked;

            //load 레지스트리 설정
            if (regKey != null)
            {
                try
                {
                    //체크상태 반영 from 레지스트리
                    if (Convert.ToString(regKey.GetValue("SetWindowRestart", "true")) == "true")
                    {
                        checkBox1.CheckState = CheckState.Checked;
                        label3.Enabled = true;
                        label4.Enabled = true;
                    }

                    //시간 반영 from 레지스트리
                    int RestartHour = Convert.ToInt32(regKey.GetValue("RestartHour", "4"));
                    int RestartMinute = Convert.ToInt32(regKey.GetValue("RestartMinute", "0"));

                    dateTime = new DateTime(2017, 11, 11, RestartHour, RestartMinute, 0);
                    dateTimePicker1.Value = dateTime;
                }
                catch
                {
                    System.Diagnostics.Trace.WriteLine("AutoWindowRestart: Start Failed reg");
                }
            }
        }

        //Form이 보여질 때
        private void Form1_Shown(object sender, EventArgs e)
        {
            //체크가 되어 있을 경우에 처음 실행 시 트레이모드로 진행
            if (checkBox1.CheckState == CheckState.Checked)
            {
                this.Close();
            }
        }

        //트레이아이콘 컨텍스트메뉴 설정 클릭
        private void ToolStripMenuItemShow_Click(object sender, EventArgs e)
        {
            this.Show();
        }

        //트레이아이콘 컨텍스트메뉴 종료 클릭
        private void ToolStripMenuItemExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("프로그램이 종료됩니다.", "Warning", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                IsMainExit = true;
                Application.Exit();
            }
        }

        //트레이아이콘 컨텍스트메뉴 더블클릭
        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
        }

        //종료를 누를 경우 트레이 아이콘으로 진행
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                notifyIcon1.Visible = true;
                this.Hide();
                e.Cancel = true;
            }
        }

        //dateTimePicker 상태 변경 -> 레지스트리 저장
        private void DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            //처음 default 설정은 제외
            if (!IsFirstDateTimePciker)
            {
                //dateTimePicker 읽기
                int RestartHour = dateTimePicker1.Value.Hour;
                int RestartMinute = dateTimePicker1.Value.Minute;

                if (regKey != null)
                {
                    regKey.SetValue("RestartHour", RestartHour.ToString());
                    regKey.SetValue("RestartMinute", RestartMinute.ToString());
                }
            }
            else
            {
                IsFirstDateTimePciker = false;
            }
        }

        //체크박스 상태 변경 -> 레지스트리 +  시스템적용
        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.CheckState == CheckState.Unchecked)
            {
                if (MessageBox.Show("크레인 충돌방지 시스템에 영향을 줄 수 있습니다.\n계속하시겠습니까?", "Warning", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    //Uncheck 취소
                    checkBox1.CheckState = CheckState.Checked;
                }
                else //체크해제
                {
                    label3.Enabled = false;
                    label3.Text = "00:00:00";
                    formRestartWarning.SetLabelTime(label3.Text);
                    label4.Enabled = false;

                    //레지스트리 체크상태 저장
                    if (regKey != null)
                    {
                        regKey.SetValue("SetWindowRestart", "false");
                    }
                }
            }
            else //체크
            {
                label3.Enabled = true;
                label4.Enabled = true;

                //레지스트리 체크상태 저장
                if (regKey != null)
                {
                    regKey.SetValue("SetWindowRestart", "true");
                }
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            //현재 시간 저장
            DateTime CurrentDateTime = DateTime.Now;

            //오후되는지 확인 필요
            this.Text = "Auto Window Restart (현재시간 " + CurrentDateTime.Hour.ToString("D2") + ":" + CurrentDateTime.Minute.ToString("D2") + ":" + CurrentDateTime.Second.ToString("D2") + ")";

            //종료되기까지 남은 시간
            if (checkBox1.CheckState == CheckState.Checked)
            {
                //dateTimePicker 읽기
                int RestartHour = dateTimePicker1.Value.Hour;
                int RestartMinute = dateTimePicker1.Value.Minute;

                DateTime RestartDateTime = new(CurrentDateTime.Year, CurrentDateTime.Month, CurrentDateTime.Day, RestartHour, RestartMinute, 0);
                // CurrentDateTime가 더 느린 경우
                if (DateTime.Compare(CurrentDateTime, RestartDateTime) > 0) 
                {
                    //재시작시간을 다음날로 계산
                    RestartDateTime = RestartDateTime.AddDays(1);
                }

                //시간 차이 계산
                TimeSpan timeSpan = RestartDateTime - CurrentDateTime;
                //빼기 차이 1초 적용
                TimeSpan timeSpanPlus = new(0, 0, 1);
                timeSpan = timeSpan.Add(timeSpanPlus);
                //시간 차이 표시
                label3.Text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                formRestartWarning.SetLabelTime(label3.Text);

                //30초이하 종료가 임박할 때
                if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds <= 30)
                {
                    //사용자가 취소를 눌렸을 경우에는 재시작 하지 않음
                    if (!IsUserDefineRestartCancel)
                    {
                        label3.ForeColor = Color.IndianRed;
                        //경고창이 없으면 시작
                        if (!formRestartWarning.Visible)
                        {
                            formRestartWarning.Visible = true;
                        }

                        if (timeSpan.Seconds == 0)
                        {
                            if (!IsRunRestart)
                            {
                                IsRunRestart = true;

                                //재시작 실행
                                CommandRunWindowRestart.RunRestart();
                            }
                        }
                    }
                    else
                    {
                        label3.ForeColor = Color.Black;

                        //사용자 취소에 의한 종료 24시간 증가
                        label3.Text = $"{timeSpan.Hours + 24:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
                        formRestartWarning.SetLabelTime(label3.Text);
                    }
                }
                else
                {
                    //임박 범위가 벗어날 때 초기화
                    IsUserDefineRestartCancel = false;

                    label3.ForeColor = Color.Black;
                    //경고창이 있으면 종료
                    if (formRestartWarning.Visible)
                    {
                        formRestartWarning.Visible = false;
                    }
                }
            }
        }
    }
}
