namespace HIK_DEMON
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.txt_Debug = new System.Windows.Forms.TextBox();
            this.panelRight = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.button8 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.txt_PoseResult = new System.Windows.Forms.TextBox();
            this.label_QRSize = new System.Windows.Forms.Label();
            this.txt_QRSize = new System.Windows.Forms.TextBox();
            this.chk_EnableQR = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txt_CalibResult = new System.Windows.Forms.TextBox();
            this.btn_RunCalibration = new System.Windows.Forms.Button();
            this.btn_SaveCalibImage = new System.Windows.Forms.Button();
            this.lbl_BoardGrid = new System.Windows.Forms.Label();
            this.txt_BoardWidth = new System.Windows.Forms.TextBox();
            this.lbl_BoardX = new System.Windows.Forms.Label();
            this.txt_BoardHeight = new System.Windows.Forms.TextBox();
            this.lbl_SquareSize = new System.Windows.Forms.Label();
            this.txt_SquareSize = new System.Windows.Forms.TextBox();
            this.lbl_SquareUnit = new System.Windows.Forms.Label();
            this.btn_SaveConfig = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.listBox_AllQRs = new System.Windows.Forms.ListBox();
            this.listBox_VisibleQRs = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.txt_AssignID = new System.Windows.Forms.TextBox();
            this.btn_AssignID = new System.Windows.Forms.Button();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmb_Ports = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.cmb_BaudRate = new System.Windows.Forms.ComboBox();
            this.btn_OpenPort = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox_IMU_Connect = new System.Windows.Forms.GroupBox();
            this.cmb_IMU_Port = new System.Windows.Forms.ComboBox();
            this.cmb_IMU_Baud = new System.Windows.Forms.ComboBox();
            this.btn_IMU_Connect = new System.Windows.Forms.Button();
            this.lbl_IMU_Status = new System.Windows.Forms.Label();
            this.groupBox_Sensor = new System.Windows.Forms.GroupBox();
            this.txt_AccelX = new System.Windows.Forms.TextBox();
            this.txt_AccelY = new System.Windows.Forms.TextBox();
            this.txt_AccelZ = new System.Windows.Forms.TextBox();
            this.txt_GyroX = new System.Windows.Forms.TextBox();
            this.txt_GyroY = new System.Windows.Forms.TextBox();
            this.txt_GyroZ = new System.Windows.Forms.TextBox();
            this.txt_Roll = new System.Windows.Forms.TextBox();
            this.txt_Pitch = new System.Windows.Forms.TextBox();
            this.txt_Yaw = new System.Windows.Forms.TextBox();
            this.groupBox_Quat = new System.Windows.Forms.GroupBox();
            this.txt_Q0 = new System.Windows.Forms.TextBox();
            this.txt_Q1 = new System.Windows.Forms.TextBox();
            this.txt_Q2 = new System.Windows.Forms.TextBox();
            this.txt_Q3 = new System.Windows.Forms.TextBox();
            this.groupBox_IMU_Ctrl = new System.Windows.Forms.GroupBox();
            this.btn_EnterSetting = new System.Windows.Forms.Button();
            this.btn_ExitSetting = new System.Windows.Forms.Button();
            this.btn_SetZero = new System.Windows.Forms.Button();
            this.btn_AccelCali = new System.Windows.Forms.Button();
            this.btn_GyroCali = new System.Windows.Forms.Button();
            this.btn_SaveIMUParams = new System.Windows.Forms.Button();
            this.btn_RebootIMU = new System.Windows.Forms.Button();
            this.lbl_AccelX = new System.Windows.Forms.Label();
            this.lbl_AccelY = new System.Windows.Forms.Label();
            this.lbl_AccelZ = new System.Windows.Forms.Label();
            this.lbl_GyroX = new System.Windows.Forms.Label();
            this.lbl_GyroY = new System.Windows.Forms.Label();
            this.lbl_GyroZ = new System.Windows.Forms.Label();
            this.lbl_Roll = new System.Windows.Forms.Label();
            this.lbl_Pitch = new System.Windows.Forms.Label();
            this.lbl_Yaw = new System.Windows.Forms.Label();
            this.lbl_Q0 = new System.Windows.Forms.Label();
            this.lbl_Q1 = new System.Windows.Forms.Label();
            this.lbl_Q2 = new System.Windows.Forms.Label();
            this.lbl_Q3 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tssl_Camera = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssl_Grab = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssl_QR = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssl_Serial = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);

            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panelRight.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox_IMU_Connect.SuspendLayout();
            this.groupBox_Sensor.SuspendLayout();
            this.groupBox_Quat.SuspendLayout();
            this.groupBox_IMU_Ctrl.SuspendLayout();
            this.SuspendLayout();

            // tabControl1
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1480, 875);

            // tabPage1
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.txt_Debug);
            this.tabPage1.Controls.Add(this.panelRight);
            this.tabPage1.Location = new System.Drawing.Point(4, 29);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(10);
            this.tabPage1.Size = new System.Drawing.Size(1472, 842);
            this.tabPage1.Text = "实时画面与控制";
            this.tabPage1.UseVisualStyleBackColor = true;

            // ================== 修正: groupBox2 相机画面 ==================
            // 使用四周 Anchor 确保画面能随窗口拉伸完美铺满左侧可用空间
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.pictureBox1);
            this.groupBox2.Location = new System.Drawing.Point(10, 10);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(600, 560);
            this.groupBox2.Text = "相机实时画面";

            // ================== 修正: pictureBox1 尺寸防坍塌 ==================
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 21);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(594, 536);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;

            // ================== 修正: txt_Debug 日志框 ==================
            // 绑定左、右、底，使其能在屏幕底部横向拉伸
            this.txt_Debug.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_Debug.Location = new System.Drawing.Point(10, 580);
            this.txt_Debug.Multiline = true;
            this.txt_Debug.Name = "txt_Debug";
            this.txt_Debug.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_Debug.Size = new System.Drawing.Size(600, 250);

            // ================== 修正: panelRight 右侧面板 ==================
            // 只绑定右侧、上方、下方，使其大小固定，始终贴靠在窗口最右侧不被过度拉长
            this.panelRight.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Right)));
            this.panelRight.AutoScroll = true;
            this.panelRight.Controls.Add(this.groupBox1);
            this.panelRight.Controls.Add(this.groupBox4);
            this.panelRight.Controls.Add(this.groupBox3);
            this.panelRight.Controls.Add(this.groupBox6);
            this.panelRight.Controls.Add(this.groupBox5);
            this.panelRight.Controls.Add(this.btn_SaveConfig);
            this.panelRight.Location = new System.Drawing.Point(620, 10);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(840, 820);

            // groupBox1 设备连接
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.comboBox1);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Location = new System.Drawing.Point(20, 20);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(400, 150);
            this.groupBox1.Text = "设备连接";

            this.button1.Location = new System.Drawing.Point(20, 30);
            this.button1.Size = new System.Drawing.Size(90, 35);
            this.button1.Text = "枚举";
            this.button1.Click += new System.EventHandler(this.button1_Click);

            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.Location = new System.Drawing.Point(120, 30);
            this.comboBox1.Size = new System.Drawing.Size(260, 28);

            this.button2.Location = new System.Drawing.Point(20, 80);
            this.button2.Size = new System.Drawing.Size(160, 40);
            this.button2.Text = "打开相机";
            this.button2.Click += new System.EventHandler(this.button2_Click);

            this.button3.Location = new System.Drawing.Point(200, 80);
            this.button3.Size = new System.Drawing.Size(160, 40);
            this.button3.Text = "关闭相机";
            this.button3.Click += new System.EventHandler(this.button3_Click);

            // groupBox4 采集控制
            this.groupBox4.Controls.Add(this.button8);
            this.groupBox4.Controls.Add(this.button7);
            this.groupBox4.Controls.Add(this.button6);
            this.groupBox4.Location = new System.Drawing.Point(20, 180);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(400, 110);
            this.groupBox4.Text = "采集控制";

            this.button8.Location = new System.Drawing.Point(20, 40);
            this.button8.Size = new System.Drawing.Size(110, 40);
            this.button8.Text = "单帧";
            this.button8.Click += new System.EventHandler(this.button8_Click);

            this.button7.Location = new System.Drawing.Point(140, 40);
            this.button7.Size = new System.Drawing.Size(110, 40);
            this.button7.Text = "连续采集";
            this.button7.Click += new System.EventHandler(this.button7_Click);

            this.button6.Location = new System.Drawing.Point(260, 40);
            this.button6.Size = new System.Drawing.Size(110, 40);
            this.button6.Text = "停止";
            this.button6.Click += new System.EventHandler(this.button6_Click);

            // groupBox3 参数配置
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBox3);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.comboBox2);
            this.groupBox3.Controls.Add(this.button5);
            this.groupBox3.Controls.Add(this.button4);
            this.groupBox3.Location = new System.Drawing.Point(20, 300);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(400, 270);
            this.groupBox3.Text = "参数配置";

            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(20, 40);
            this.label1.Text = "曝光时间:";
            this.textBox1.Location = new System.Drawing.Point(120, 37);
            this.textBox1.Size = new System.Drawing.Size(260, 26);

            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 80);
            this.label2.Text = "增益:";
            this.textBox2.Location = new System.Drawing.Point(120, 77);
            this.textBox2.Size = new System.Drawing.Size(260, 26);

            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 120);
            this.label3.Text = "实际帧率:";
            this.textBox3.Location = new System.Drawing.Point(120, 117);
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(260, 26);

            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 160);
            this.label4.Text = "像素格式:";
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(120, 157);
            this.comboBox2.Size = new System.Drawing.Size(260, 28);

            this.button5.Location = new System.Drawing.Point(20, 200);
            this.button5.Size = new System.Drawing.Size(160, 40);
            this.button5.Text = "获取参数";
            this.button5.Click += new System.EventHandler(this.button5_Click);

            this.button4.Location = new System.Drawing.Point(200, 200);
            this.button4.Size = new System.Drawing.Size(160, 40);
            this.button4.Text = "下发参数";
            this.button4.Click += new System.EventHandler(this.button4_Click);

            // groupBox6 QR码位姿
            this.groupBox6.Controls.Add(this.chk_EnableQR);
            this.groupBox6.Controls.Add(this.label_QRSize);
            this.groupBox6.Controls.Add(this.txt_QRSize);
            this.groupBox6.Controls.Add(this.txt_PoseResult);
            this.groupBox6.Location = new System.Drawing.Point(430, 20);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(400, 270);
            this.groupBox6.Text = "QR码位姿";

            this.chk_EnableQR.AutoSize = true;
            this.chk_EnableQR.Location = new System.Drawing.Point(20, 30);
            this.chk_EnableQR.Text = "启用识别";
            this.chk_EnableQR.CheckedChanged += new System.EventHandler(this.chk_EnableQR_CheckedChanged);

            this.label_QRSize.AutoSize = true;
            this.label_QRSize.Location = new System.Drawing.Point(180, 30);
            this.label_QRSize.Text = "边长(mm):";
            this.txt_QRSize.Location = new System.Drawing.Point(260, 27);
            this.txt_QRSize.Size = new System.Drawing.Size(110, 26);

            this.txt_PoseResult.Location = new System.Drawing.Point(20, 65);
            this.txt_PoseResult.Multiline = true;
            this.txt_PoseResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_PoseResult.Size = new System.Drawing.Size(360, 180);

            // groupBox5 棋盘格标定
            this.groupBox5.Controls.Add(this.btn_SaveCalibImage);
            this.groupBox5.Controls.Add(this.btn_RunCalibration);
            this.groupBox5.Controls.Add(this.lbl_BoardGrid);
            this.groupBox5.Controls.Add(this.txt_BoardWidth);
            this.groupBox5.Controls.Add(this.lbl_BoardX);
            this.groupBox5.Controls.Add(this.txt_BoardHeight);
            this.groupBox5.Controls.Add(this.lbl_SquareSize);
            this.groupBox5.Controls.Add(this.txt_SquareSize);
            this.groupBox5.Controls.Add(this.lbl_SquareUnit);
            this.groupBox5.Controls.Add(this.txt_CalibResult);
            this.groupBox5.Location = new System.Drawing.Point(430, 300);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(400, 350);
            this.groupBox5.Text = "棋盘格标定";

            this.btn_SaveCalibImage.Location = new System.Drawing.Point(20, 30);
            this.btn_SaveCalibImage.Size = new System.Drawing.Size(160, 40);
            this.btn_SaveCalibImage.Text = "采集标定图";
            this.btn_SaveCalibImage.Click += new System.EventHandler(this.btn_SaveCalibImage_Click);

            this.btn_RunCalibration.Location = new System.Drawing.Point(200, 30);
            this.btn_RunCalibration.Size = new System.Drawing.Size(160, 40);
            this.btn_RunCalibration.Text = "执行标定";
            this.btn_RunCalibration.Click += new System.EventHandler(this.btn_RunCalibration_Click);

            this.lbl_BoardGrid.AutoSize = true;
            this.lbl_BoardGrid.Location = new System.Drawing.Point(20, 90);
            this.lbl_BoardGrid.Text = "棋盘格宽:";
            this.txt_BoardWidth.Location = new System.Drawing.Point(100, 87);
            this.txt_BoardWidth.Size = new System.Drawing.Size(70, 26);
            this.txt_BoardWidth.Text = "9";

            this.lbl_BoardX.AutoSize = true;
            this.lbl_BoardX.Location = new System.Drawing.Point(180, 90);
            this.lbl_BoardX.Text = "高:";
            this.txt_BoardHeight.Location = new System.Drawing.Point(210, 87);
            this.txt_BoardHeight.Size = new System.Drawing.Size(70, 26);
            this.txt_BoardHeight.Text = "6";

            this.lbl_SquareSize.AutoSize = true;
            this.lbl_SquareSize.Location = new System.Drawing.Point(20, 130);
            this.lbl_SquareSize.Text = "方格边长:";
            this.txt_SquareSize.Location = new System.Drawing.Point(100, 127);
            this.txt_SquareSize.Size = new System.Drawing.Size(100, 26);
            this.txt_SquareSize.Text = "23.7";

            this.lbl_SquareUnit.AutoSize = true;
            this.lbl_SquareUnit.Location = new System.Drawing.Point(210, 130);
            this.lbl_SquareUnit.Text = "mm";

            this.txt_CalibResult.Location = new System.Drawing.Point(20, 170);
            this.txt_CalibResult.Multiline = true;
            this.txt_CalibResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txt_CalibResult.Size = new System.Drawing.Size(360, 160);

            // btn_SaveConfig
            this.btn_SaveConfig.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.btn_SaveConfig.Location = new System.Drawing.Point(20, 670);
            this.btn_SaveConfig.Name = "btn_SaveConfig";
            this.btn_SaveConfig.Size = new System.Drawing.Size(810, 60);
            this.btn_SaveConfig.Text = "保 存 所 有 配 置";
            this.btn_SaveConfig.Click += new System.EventHandler(this.btn_SaveConfig_Click);

            // tabPage2
            this.tabPage2.Controls.Add(this.groupBox7);
            this.tabPage2.Controls.Add(this.groupBox8);
            this.tabPage2.Location = new System.Drawing.Point(4, 29);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(10);
            this.tabPage2.Size = new System.Drawing.Size(1472, 842);
            this.tabPage2.Text = "二维码与串口";
            this.tabPage2.UseVisualStyleBackColor = true;

            // groupBox7 跟踪列表
            this.groupBox7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox7.Controls.Add(this.label8);
            this.groupBox7.Controls.Add(this.listBox_AllQRs);
            this.groupBox7.Controls.Add(this.label9);
            this.groupBox7.Controls.Add(this.listBox_VisibleQRs);
            this.groupBox7.Controls.Add(this.label5);
            this.groupBox7.Controls.Add(this.txt_AssignID);
            this.groupBox7.Controls.Add(this.btn_AssignID);
            this.groupBox7.Location = new System.Drawing.Point(15, 15);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(700, 810);
            this.groupBox7.Text = "跟踪列表";

            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(20, 30);
            this.label8.Text = "所有识别过的二维码 (选择以分配ID):";
            this.listBox_AllQRs.FormattingEnabled = true;
            this.listBox_AllQRs.Location = new System.Drawing.Point(20, 55);
            this.listBox_AllQRs.Size = new System.Drawing.Size(660, 300);

            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(20, 370);
            this.label9.Text = "当前识别到的二维码:";
            this.listBox_VisibleQRs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox_VisibleQRs.FormattingEnabled = true;
            this.listBox_VisibleQRs.Location = new System.Drawing.Point(20, 395);
            this.listBox_VisibleQRs.Size = new System.Drawing.Size(660, 300);

            this.label5.AutoSize = true;
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label5.Location = new System.Drawing.Point(20, 710);
            this.label5.Text = "分配新ID:";
            this.txt_AssignID.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.txt_AssignID.Location = new System.Drawing.Point(100, 707);
            this.txt_AssignID.Size = new System.Drawing.Size(120, 26);
            this.btn_AssignID.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_AssignID.Location = new System.Drawing.Point(230, 705);
            this.btn_AssignID.Size = new System.Drawing.Size(100, 30);
            this.btn_AssignID.Text = "分配";
            this.btn_AssignID.Click += new System.EventHandler(this.btn_AssignID_Click);

            // groupBox8 串口输出
            this.groupBox8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox8.Controls.Add(this.label6);
            this.groupBox8.Controls.Add(this.cmb_Ports);
            this.groupBox8.Controls.Add(this.label7);
            this.groupBox8.Controls.Add(this.cmb_BaudRate);
            this.groupBox8.Controls.Add(this.btn_OpenPort);
            this.groupBox8.Location = new System.Drawing.Point(730, 15);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(715, 810);
            this.groupBox8.Text = "串口输出";

            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(30, 50);
            this.label6.Text = "端口号:";
            this.cmb_Ports.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_Ports.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.cmb_Ports.FormattingEnabled = true;
            this.cmb_Ports.Location = new System.Drawing.Point(100, 47);
            this.cmb_Ports.Size = new System.Drawing.Size(580, 30);

            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(30, 100);
            this.label7.Text = "波特率:";
            this.cmb_BaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_BaudRate.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.cmb_BaudRate.FormattingEnabled = true;
            this.cmb_BaudRate.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200" });
            this.cmb_BaudRate.Location = new System.Drawing.Point(100, 97);
            this.cmb_BaudRate.Size = new System.Drawing.Size(250, 30);
            this.cmb_BaudRate.SelectedIndex = 4;

            this.btn_OpenPort.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Bold);
            this.btn_OpenPort.Location = new System.Drawing.Point(100, 150);
            this.btn_OpenPort.Size = new System.Drawing.Size(250, 50);
            this.btn_OpenPort.Text = "打 开 串 口";
            this.btn_OpenPort.Click += new System.EventHandler(this.btn_OpenPort_Click);

            // tabPage3
            this.tabPage3.Controls.Add(this.groupBox_IMU_Connect);
            this.tabPage3.Controls.Add(this.groupBox_Sensor);
            this.tabPage3.Controls.Add(this.groupBox_Quat);
            this.tabPage3.Controls.Add(this.groupBox_IMU_Ctrl);
            this.tabPage3.Location = new System.Drawing.Point(4, 29);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(10);
            this.tabPage3.Size = new System.Drawing.Size(1472, 842);
            this.tabPage3.Text = "IMU 数据";
            this.tabPage3.UseVisualStyleBackColor = true;

            // groupBox_IMU_Connect（顶部通栏）
            this.groupBox_IMU_Connect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_IMU_Connect.Controls.Add(this.cmb_IMU_Port);
            this.groupBox_IMU_Connect.Controls.Add(this.cmb_IMU_Baud);
            this.groupBox_IMU_Connect.Controls.Add(this.btn_IMU_Connect);
            this.groupBox_IMU_Connect.Controls.Add(this.lbl_IMU_Status);
            this.groupBox_IMU_Connect.Location = new System.Drawing.Point(15, 15);
            this.groupBox_IMU_Connect.Size = new System.Drawing.Size(1440, 75);
            this.groupBox_IMU_Connect.Text = "IMU 连接";
            this.cmb_IMU_Port.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_IMU_Port.Location = new System.Drawing.Point(15, 33);
            this.cmb_IMU_Port.Size = new System.Drawing.Size(400, 28);
            this.cmb_IMU_Baud.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmb_IMU_Baud.Items.AddRange(new object[] { "921600", "460800", "230400", "115200" });
            this.cmb_IMU_Baud.Location = new System.Drawing.Point(430, 33);
            this.cmb_IMU_Baud.Size = new System.Drawing.Size(150, 28);
            this.cmb_IMU_Baud.SelectedIndex = 0;
            this.btn_IMU_Connect.Location = new System.Drawing.Point(600, 30);
            this.btn_IMU_Connect.Size = new System.Drawing.Size(120, 35);
            this.btn_IMU_Connect.Text = "连接 IMU";
            this.btn_IMU_Connect.Click += new System.EventHandler(this.btn_IMU_Connect_Click);
            this.lbl_IMU_Status.AutoSize = true;
            this.lbl_IMU_Status.Font = new System.Drawing.Font("微软雅黑", 10.5F, System.Drawing.FontStyle.Bold);
            this.lbl_IMU_Status.Location = new System.Drawing.Point(740, 36);
            this.lbl_IMU_Status.Text = "○ 未连接";

            // groupBox_Sensor（左列，占大部分宽度）
            this.groupBox_Sensor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox_Sensor.Controls.Add(this.lbl_AccelX);
            this.groupBox_Sensor.Controls.Add(this.lbl_AccelY);
            this.groupBox_Sensor.Controls.Add(this.lbl_AccelZ);
            this.groupBox_Sensor.Controls.Add(this.lbl_GyroX);
            this.groupBox_Sensor.Controls.Add(this.lbl_GyroY);
            this.groupBox_Sensor.Controls.Add(this.lbl_GyroZ);
            this.groupBox_Sensor.Controls.Add(this.lbl_Roll);
            this.groupBox_Sensor.Controls.Add(this.lbl_Pitch);
            this.groupBox_Sensor.Controls.Add(this.lbl_Yaw);
            this.groupBox_Sensor.Controls.Add(this.txt_AccelX);
            this.groupBox_Sensor.Controls.Add(this.txt_AccelY);
            this.groupBox_Sensor.Controls.Add(this.txt_AccelZ);
            this.groupBox_Sensor.Controls.Add(this.txt_GyroX);
            this.groupBox_Sensor.Controls.Add(this.txt_GyroY);
            this.groupBox_Sensor.Controls.Add(this.txt_GyroZ);
            this.groupBox_Sensor.Controls.Add(this.txt_Roll);
            this.groupBox_Sensor.Controls.Add(this.txt_Pitch);
            this.groupBox_Sensor.Controls.Add(this.txt_Yaw);
            this.groupBox_Sensor.Location = new System.Drawing.Point(15, 100);
            this.groupBox_Sensor.Size = new System.Drawing.Size(900, 430);
            this.groupBox_Sensor.Text = "传感器数据";

            // 3行 × 3列 均匀分布
            this.lbl_AccelX.AutoSize = true;
            this.lbl_AccelX.Location = new System.Drawing.Point(30, 95);
            this.lbl_AccelX.Text = "X (m/s²):";
            this.txt_AccelX.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_AccelX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_AccelX.Location = new System.Drawing.Point(115, 90);
            this.txt_AccelX.ReadOnly = true;
            this.txt_AccelX.Size = new System.Drawing.Size(170, 34);
            this.txt_AccelX.Text = "0.00";

            this.lbl_GyroX.AutoSize = true;
            this.lbl_GyroX.Location = new System.Drawing.Point(320, 95);
            this.lbl_GyroX.Text = "X (rad/s):";
            this.txt_GyroX.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_GyroX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_GyroX.Location = new System.Drawing.Point(410, 90);
            this.txt_GyroX.ReadOnly = true;
            this.txt_GyroX.Size = new System.Drawing.Size(170, 34);
            this.txt_GyroX.Text = "0.00";

            this.lbl_Roll.AutoSize = true;
            this.lbl_Roll.Location = new System.Drawing.Point(615, 95);
            this.lbl_Roll.Text = "Roll (°):";
            this.txt_Roll.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_Roll.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_Roll.Location = new System.Drawing.Point(700, 90);
            this.txt_Roll.ReadOnly = true;
            this.txt_Roll.Size = new System.Drawing.Size(170, 34);
            this.txt_Roll.Text = "0.00";

            this.lbl_AccelY.AutoSize = true;
            this.lbl_AccelY.Location = new System.Drawing.Point(30, 215);
            this.lbl_AccelY.Text = "Y (m/s²):";
            this.txt_AccelY.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_AccelY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_AccelY.Location = new System.Drawing.Point(115, 210);
            this.txt_AccelY.ReadOnly = true;
            this.txt_AccelY.Size = new System.Drawing.Size(170, 34);
            this.txt_AccelY.Text = "0.00";

            this.lbl_GyroY.AutoSize = true;
            this.lbl_GyroY.Location = new System.Drawing.Point(320, 215);
            this.lbl_GyroY.Text = "Y (rad/s):";
            this.txt_GyroY.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_GyroY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_GyroY.Location = new System.Drawing.Point(410, 210);
            this.txt_GyroY.ReadOnly = true;
            this.txt_GyroY.Size = new System.Drawing.Size(170, 34);
            this.txt_GyroY.Text = "0.00";

            this.lbl_Pitch.AutoSize = true;
            this.lbl_Pitch.Location = new System.Drawing.Point(615, 215);
            this.lbl_Pitch.Text = "Pitch (°):";
            this.txt_Pitch.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_Pitch.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_Pitch.Location = new System.Drawing.Point(700, 210);
            this.txt_Pitch.ReadOnly = true;
            this.txt_Pitch.Size = new System.Drawing.Size(170, 34);
            this.txt_Pitch.Text = "0.00";

            this.lbl_AccelZ.AutoSize = true;
            this.lbl_AccelZ.Location = new System.Drawing.Point(30, 335);
            this.lbl_AccelZ.Text = "Z (m/s²):";
            this.txt_AccelZ.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_AccelZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_AccelZ.Location = new System.Drawing.Point(115, 330);
            this.txt_AccelZ.ReadOnly = true;
            this.txt_AccelZ.Size = new System.Drawing.Size(170, 34);
            this.txt_AccelZ.Text = "0.00";

            this.lbl_GyroZ.AutoSize = true;
            this.lbl_GyroZ.Location = new System.Drawing.Point(320, 335);
            this.lbl_GyroZ.Text = "Z (rad/s):";
            this.txt_GyroZ.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_GyroZ.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_GyroZ.Location = new System.Drawing.Point(410, 330);
            this.txt_GyroZ.ReadOnly = true;
            this.txt_GyroZ.Size = new System.Drawing.Size(170, 34);
            this.txt_GyroZ.Text = "0.00";

            this.lbl_Yaw.AutoSize = true;
            this.lbl_Yaw.Location = new System.Drawing.Point(615, 335);
            this.lbl_Yaw.Text = "Yaw (°):";
            this.txt_Yaw.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_Yaw.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_Yaw.Location = new System.Drawing.Point(700, 330);
            this.txt_Yaw.ReadOnly = true;
            this.txt_Yaw.Size = new System.Drawing.Size(170, 34);
            this.txt_Yaw.Text = "0.00";

            // groupBox_Quat（传感器下方）
            this.groupBox_Quat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox_Quat.Controls.Add(this.lbl_Q0);
            this.groupBox_Quat.Controls.Add(this.txt_Q0);
            this.groupBox_Quat.Controls.Add(this.lbl_Q1);
            this.groupBox_Quat.Controls.Add(this.txt_Q1);
            this.groupBox_Quat.Controls.Add(this.lbl_Q2);
            this.groupBox_Quat.Controls.Add(this.txt_Q2);
            this.groupBox_Quat.Controls.Add(this.lbl_Q3);
            this.groupBox_Quat.Controls.Add(this.txt_Q3);
            this.groupBox_Quat.Location = new System.Drawing.Point(15, 540);
            this.groupBox_Quat.Size = new System.Drawing.Size(900, 100);
            this.groupBox_Quat.Text = "四元数";

            this.lbl_Q0.AutoSize = true;
            this.lbl_Q0.Location = new System.Drawing.Point(30, 40);
            this.lbl_Q0.Text = "Q0:";
            this.txt_Q0.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_Q0.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_Q0.Location = new System.Drawing.Point(70, 37);
            this.txt_Q0.ReadOnly = true;
            this.txt_Q0.Size = new System.Drawing.Size(180, 34);
            this.txt_Q0.Text = "0.0000";

            this.lbl_Q1.AutoSize = true;
            this.lbl_Q1.Location = new System.Drawing.Point(270, 40);
            this.lbl_Q1.Text = "Q1:";
            this.txt_Q1.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_Q1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_Q1.Location = new System.Drawing.Point(310, 37);
            this.txt_Q1.ReadOnly = true;
            this.txt_Q1.Size = new System.Drawing.Size(180, 34);
            this.txt_Q1.Text = "0.0000";

            this.lbl_Q2.AutoSize = true;
            this.lbl_Q2.Location = new System.Drawing.Point(510, 40);
            this.lbl_Q2.Text = "Q2:";
            this.txt_Q2.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_Q2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_Q2.Location = new System.Drawing.Point(550, 37);
            this.txt_Q2.ReadOnly = true;
            this.txt_Q2.Size = new System.Drawing.Size(180, 34);
            this.txt_Q2.Text = "0.0000";

            this.lbl_Q3.AutoSize = true;
            this.lbl_Q3.Location = new System.Drawing.Point(750, 40);
            this.lbl_Q3.Text = "Q3:";
            this.txt_Q3.Font = new System.Drawing.Font("Consolas", 14F);
            this.txt_Q3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txt_Q3.Location = new System.Drawing.Point(790, 37);
            this.txt_Q3.ReadOnly = true;
            this.txt_Q3.Size = new System.Drawing.Size(100, 34);
            this.txt_Q3.Text = "0.0000";

            // groupBox_IMU_Ctrl（右列，和左列等高）
            this.groupBox_IMU_Ctrl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_IMU_Ctrl.Controls.Add(this.btn_SetZero);
            this.groupBox_IMU_Ctrl.Controls.Add(this.btn_SaveIMUParams);
            this.groupBox_IMU_Ctrl.Controls.Add(this.btn_AccelCali);
            this.groupBox_IMU_Ctrl.Controls.Add(this.btn_GyroCali);
            this.groupBox_IMU_Ctrl.Controls.Add(this.btn_EnterSetting);
            this.groupBox_IMU_Ctrl.Controls.Add(this.btn_ExitSetting);
            this.groupBox_IMU_Ctrl.Controls.Add(this.btn_RebootIMU);
            this.groupBox_IMU_Ctrl.Location = new System.Drawing.Point(930, 100);
            this.groupBox_IMU_Ctrl.Size = new System.Drawing.Size(540, 540);
            this.groupBox_IMU_Ctrl.Text = "IMU 控制";

            this.btn_SetZero.Location = new System.Drawing.Point(30, 45);
            this.btn_SetZero.Size = new System.Drawing.Size(220, 55);
            this.btn_SetZero.Text = "置零";
            this.btn_SaveIMUParams.Location = new System.Drawing.Point(280, 45);
            this.btn_SaveIMUParams.Size = new System.Drawing.Size(220, 55);
            this.btn_SaveIMUParams.Text = "保存参数";

            this.btn_AccelCali.Location = new System.Drawing.Point(30, 155);
            this.btn_AccelCali.Size = new System.Drawing.Size(220, 55);
            this.btn_AccelCali.Text = "加速度校准";
            this.btn_GyroCali.Location = new System.Drawing.Point(280, 155);
            this.btn_GyroCali.Size = new System.Drawing.Size(220, 55);
            this.btn_GyroCali.Text = "陀螺仪校准";

            this.btn_EnterSetting.Location = new System.Drawing.Point(30, 265);
            this.btn_EnterSetting.Size = new System.Drawing.Size(220, 55);
            this.btn_EnterSetting.Text = "进入设置模式";
            this.btn_ExitSetting.Location = new System.Drawing.Point(280, 265);
            this.btn_ExitSetting.Size = new System.Drawing.Size(220, 55);
            this.btn_ExitSetting.Text = "退出设置模式";

            this.btn_RebootIMU.Location = new System.Drawing.Point(30, 375);
            this.btn_RebootIMU.Size = new System.Drawing.Size(220, 55);
            this.btn_RebootIMU.Text = "重启 IMU";

            // statusStrip1
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.tssl_Camera, this.tssl_Grab, this.tssl_QR, this.tssl_Serial});
            this.statusStrip1.Location = new System.Drawing.Point(0, 875);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1480, 25);
            this.tssl_Camera.Text = "○ 相机未连接";
            this.tssl_Camera.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.All;
            this.tssl_Grab.Text = "○ 未采集";
            this.tssl_Grab.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.All;
            this.tssl_QR.Text = "QR: 关闭";
            this.tssl_QR.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.All;
            this.tssl_Serial.Text = "○ 串口关闭";
            this.tssl_Serial.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.All;
            this.tssl_Serial.Spring = true;

            // Form1
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1480, 900);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.statusStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 10F);
            this.MinimumSize = new System.Drawing.Size(1480, 900);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "海康机器人相机视觉终端演示系统";
            this.Load += new System.EventHandler(this.Form1_Load);

            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panelRight.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.groupBox_IMU_Connect.ResumeLayout(false);
            this.groupBox_IMU_Connect.PerformLayout();
            this.groupBox_Sensor.ResumeLayout(false);
            this.groupBox_Sensor.PerformLayout();
            this.groupBox_Quat.ResumeLayout(false);
            this.groupBox_Quat.PerformLayout();
            this.groupBox_IMU_Ctrl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // 下方变量声明
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox txt_Debug;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox txt_PoseResult;
        private System.Windows.Forms.Label label_QRSize;
        private System.Windows.Forms.TextBox txt_QRSize;
        private System.Windows.Forms.CheckBox chk_EnableQR;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox txt_CalibResult;
        private System.Windows.Forms.Button btn_RunCalibration;
        private System.Windows.Forms.Button btn_SaveCalibImage;
        private System.Windows.Forms.Label lbl_BoardGrid;
        private System.Windows.Forms.TextBox txt_BoardWidth;
        private System.Windows.Forms.Label lbl_BoardX;
        private System.Windows.Forms.TextBox txt_BoardHeight;
        private System.Windows.Forms.Label lbl_SquareSize;
        private System.Windows.Forms.TextBox txt_SquareSize;
        private System.Windows.Forms.Label lbl_SquareUnit;
        private System.Windows.Forms.Button btn_SaveConfig;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ListBox listBox_AllQRs;
        private System.Windows.Forms.ListBox listBox_VisibleQRs;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txt_AssignID;
        private System.Windows.Forms.Button btn_AssignID;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmb_Ports;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox cmb_BaudRate;
        private System.Windows.Forms.Button btn_OpenPort;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tssl_Camera;
        private System.Windows.Forms.ToolStripStatusLabel tssl_Grab;
        private System.Windows.Forms.ToolStripStatusLabel tssl_QR;
        private System.Windows.Forms.ToolStripStatusLabel tssl_Serial;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox_IMU_Connect;
        private System.Windows.Forms.ComboBox cmb_IMU_Port;
        private System.Windows.Forms.ComboBox cmb_IMU_Baud;
        private System.Windows.Forms.Button btn_IMU_Connect;
        private System.Windows.Forms.Label lbl_IMU_Status;
        private System.Windows.Forms.GroupBox groupBox_Sensor;
        private System.Windows.Forms.Label lbl_AccelX;
        private System.Windows.Forms.Label lbl_AccelY;
        private System.Windows.Forms.Label lbl_AccelZ;
        private System.Windows.Forms.Label lbl_GyroX;
        private System.Windows.Forms.Label lbl_GyroY;
        private System.Windows.Forms.Label lbl_GyroZ;
        private System.Windows.Forms.Label lbl_Roll;
        private System.Windows.Forms.Label lbl_Pitch;
        private System.Windows.Forms.Label lbl_Yaw;
        private System.Windows.Forms.TextBox txt_AccelX;
        private System.Windows.Forms.TextBox txt_AccelY;
        private System.Windows.Forms.TextBox txt_AccelZ;
        private System.Windows.Forms.TextBox txt_GyroX;
        private System.Windows.Forms.TextBox txt_GyroY;
        private System.Windows.Forms.TextBox txt_GyroZ;
        private System.Windows.Forms.TextBox txt_Roll;
        private System.Windows.Forms.TextBox txt_Pitch;
        private System.Windows.Forms.TextBox txt_Yaw;
        private System.Windows.Forms.GroupBox groupBox_Quat;
        private System.Windows.Forms.Label lbl_Q0;
        private System.Windows.Forms.Label lbl_Q1;
        private System.Windows.Forms.Label lbl_Q2;
        private System.Windows.Forms.Label lbl_Q3;
        private System.Windows.Forms.TextBox txt_Q0;
        private System.Windows.Forms.TextBox txt_Q1;
        private System.Windows.Forms.TextBox txt_Q2;
        private System.Windows.Forms.TextBox txt_Q3;
        private System.Windows.Forms.GroupBox groupBox_IMU_Ctrl;
        private System.Windows.Forms.Button btn_EnterSetting;
        private System.Windows.Forms.Button btn_ExitSetting;
        private System.Windows.Forms.Button btn_SetZero;
        private System.Windows.Forms.Button btn_AccelCali;
        private System.Windows.Forms.Button btn_GyroCali;
        private System.Windows.Forms.Button btn_SaveIMUParams;
        private System.Windows.Forms.Button btn_RebootIMU;
    }
}