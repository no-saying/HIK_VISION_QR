using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using MvCameraControl;

using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace HIK_DEMON
{
    public partial class Form1 : Form
    {
        // ---------- 海康相机 ----------
        private List<IDeviceInfo> devicesInFoList;
        private IDevice device;
        private Thread receiveThread;
        private volatile bool isGrabbing = false;
        private int calibImageCount = 0;

        // ---------- 相机内参 ----------
        private Mat cameraMatrix;
        private Mat distCoeffs;

        // ---------- 配置 ----------
        private ProjectConfig _config;

        // ---------- 标定板参数 ----------
        private int CalibBoardWidth = 9;
        private int CalibBoardHeight = 6;
        private float CalibSquareSize = 23.7f;

        // ---------- QR 控制 ----------
        private volatile bool enableQR = true;
        private float _qrSize = 20.0f;
        private readonly object _paramLock = new object();

        private float SafeQRSize
        {
            get { lock (_paramLock) return _qrSize; }
            set { lock (_paramLock) _qrSize = value; }
        }

        // ---------- 二维码跟踪 ----------
        private readonly Dictionary<string, TrackedQR> _qrDatabase = new Dictionary<string, TrackedQR>();
        private readonly object _qrLock = new object();
        private DateTime _lastListRefresh = DateTime.MinValue; //二维码时间刷新

        private DateTime _lastTextRefresh = DateTime.MinValue;// 新增：控制右侧文本框刷新

        // ========== 抗抖动参数 ==========
        private const int MAX_MISSED_FRAMES = 60;       // 连续丢失60帧才标记离线（约2秒@30fps），剧烈晃动时给足恢复时间
        private const float SMOOTHING_ALPHA = 0.50f;     // EMA 平滑系数（越大响应越快，越小越平滑）
        private const float PREDICTION_MAX_DELTA = 30f;  // 预测位置最大偏移量 (mm)，防止预测漂移
        private const float ANGLE_SMOOTHING_ALPHA = 0.15f; // 角度 EMA 平滑（角度变化慢，强平滑抗抖动）

        // ========== 光流参数 ==========
        private const int MAX_FLOW_FRAMES = 15;  // 连续光流跟踪15帧后仍无解码器校正则隐藏框（避免漂移）

        // ========== QR ID 持久化映射（Content → AssignedID） ==========
        private Dictionary<string, int> _savedQrIdMappings;

        // ========== 光流跟踪 ==========
        private Mat _prevGray;  // 上一帧灰度图

        // ========== 角度平滑 ==========
        private Dictionary<int, float> _smoothedAngles = new Dictionary<int, float>();  // angleId → EMA 平滑角度值

        // ---------- 串口 ----------
        private SerialPort _serialPort = new SerialPort();
        private volatile bool _serialEnabled = false;
        private int _serialSentCount = 0;
        private int _serialSentFrames = 0;
        private DateTime _lastSerialFpsTime = DateTime.Now;
        private double _serialCurrentFps = 0;
        private float _lastImuRoll = 0;
        private float _lastHorizontalAngleImg = 0;
        // ---------- 角度批量发送 ----------
        private volatile List<(byte id, float angle)> _latestAnglesForSend = new List<(byte id, float angle)>();
        private System.Threading.Timer _sendTimer;
        private Button btn_TestSend;

        // ---------- IMU ----------
        private DmImuDriver _imuDriver = new DmImuDriver();
        private System.Windows.Forms.Timer _imuUiTimer;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            SDKSystem.Initialize();

            // 加载配置文件，初始化内参和标定板参数
            _config = ConfigManager.Load();
            ApplyConfig();

            RefreshComPorts();
        }

        /// <summary>将配置应用到内存和 UI。</summary>
        private void ApplyConfig()
        {
            CalibBoardWidth = _config.BoardWidth;
            CalibBoardHeight = _config.BoardHeight;
            CalibSquareSize = _config.SquareSize;

            ApplyCameraMatrix();

            SafeQRSize = _config.QRSize;
            enableQR = _config.QREnabled;

            // 更新 UI
            txt_BoardWidth.Text = _config.BoardWidth.ToString();
            txt_BoardHeight.Text = _config.BoardHeight.ToString();
            txt_SquareSize.Text = _config.SquareSize.ToString("F1");
            txt_QRSize.Text = _config.QRSize.ToString("F1");
            chk_EnableQR.Checked = _config.QREnabled;

            if (_config.ExposureTime > 0)
                textBox1.Text = _config.ExposureTime.ToString("F0");
            if (_config.Gain > 0)
                textBox2.Text = _config.Gain.ToString("F1");
            if (!string.IsNullOrEmpty(_config.PixelFormat))
                comboBox2.Text = _config.PixelFormat;

            txt_CalibResult.AppendText($"配置已加载 | 上次标定: {_config.LastCalibrated}, RMS: {_config.LastReprojectionError:F3}\r\n");
        }

        /// <summary>销毁旧内参，从 _config 重建相机矩阵和畸变系数。</summary>
        private void ApplyCameraMatrix()
        {
            cameraMatrix?.Dispose();
            distCoeffs?.Dispose();

            cameraMatrix = new Mat(3, 3, MatType.CV_64FC1);
            cameraMatrix.SetArray(new double[] {
                _config.Fx, 0, _config.Cx,
                0, _config.Fy, _config.Cy,
                0, 0, 1
            });

            distCoeffs = new Mat(5, 1, MatType.CV_64FC1);
            distCoeffs.SetArray(new double[] {
                _config.K1, _config.K2, _config.P1, _config.P2, _config.K3
            });
        }

        ~Form1()
        {
            SDKSystem.Finalize();
            cameraMatrix?.Dispose();
            distCoeffs?.Dispose();
            _serialPort?.Close();
            _serialPort?.Dispose();
            _prevGray?.Dispose();
            _imuDriver?.Dispose();
            _sendTimer?.Dispose();
        }

        // ================= UI 事件 =================
        private void Form1_Load(object sender, EventArgs e)
        {
            InitImageCount();

            // 从配置恢复 UI 状态
            ApplyConfig();
            LoadQrIdMappings();
            tssl_QR.Text = enableQR ? "QR: 开启" : "QR: 关闭";

            // 按钮 ToolTip
            toolTip1.SetToolTip(button1, "扫描并列出所有连接的相机设备");
            toolTip1.SetToolTip(button2, "打开当前选中的相机");
            toolTip1.SetToolTip(button3, "断开当前相机的连接");
            toolTip1.SetToolTip(button7, "开始连续采集图像并识别二维码");
            toolTip1.SetToolTip(button6, "停止连续采集");
            toolTip1.SetToolTip(button8, "拍摄单帧静态图像");
            toolTip1.SetToolTip(button5, "读取相机的曝光/增益/帧率参数");
            toolTip1.SetToolTip(button4, "将设置的参数写入相机");
            toolTip1.SetToolTip(chk_EnableQR, "启用或禁用二维码识别功能");
            toolTip1.SetToolTip(txt_QRSize, "二维码的实际物理边长(单位:mm)");
            toolTip1.SetToolTip(btn_SaveCalibImage, "保存当前帧为棋盘格标定图片");
            toolTip1.SetToolTip(btn_RunCalibration, "使用已保存的图片执行相机标定");
            toolTip1.SetToolTip(btn_AssignID, "为选中的二维码分配一个ID编号");
            toolTip1.SetToolTip(btn_OpenPort, "打开或关闭串口连接");
            toolTip1.SetToolTip(cmb_BaudRate, "选择串口通信波特率");
            toolTip1.SetToolTip(cmb_Ports, "选择要连接的串口端口号");

            // IMU ToolTip
            toolTip1.SetToolTip(cmb_IMU_Port, "选择 IMU 连接的串口端口号");
            toolTip1.SetToolTip(cmb_IMU_Baud, "选择 IMU 通信波特率（默认 921600）");
            toolTip1.SetToolTip(btn_IMU_Connect, "打开或关闭 IMU 连接");
            toolTip1.SetToolTip(btn_EnterSetting, "进入设置模式以修改 IMU 参数");
            toolTip1.SetToolTip(btn_ExitSetting, "退出设置模式，IMU 恢复数据输出");
            toolTip1.SetToolTip(btn_SetZero, "将当前姿态设为零位");
            toolTip1.SetToolTip(btn_AccelCali, "执行加速度计校准");
            toolTip1.SetToolTip(btn_GyroCali, "执行陀螺仪校准");
            toolTip1.SetToolTip(btn_SaveIMUParams, "将当前参数保存到 IMU Flash");
            toolTip1.SetToolTip(btn_RebootIMU, "重启 IMU 模块");

            // 串口刷新（同时刷新 IMU 端口列表）
            var portTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            portTimer.Tick += (s, args) => { RefreshComPorts(); RefreshIMUPorts(); };
            portTimer.Start();

            // IMU UI 刷新定时器（50ms ≈ 20fps）
            _imuUiTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _imuUiTimer.Tick += ImuUiTimer_Tick;
            _imuUiTimer.Start();

            // 初始刷新 IMU 端口
            RefreshIMUPorts();

            // 50ms (20Hz) 角度批量发送定时器
            _sendTimer = new System.Threading.Timer(SendAngleBatch, null, 0, 50);

            // 测试按钮: 发送 ID=0, angle=-90°
            btn_TestSend = new Button
            {
                Text = "测试发送",
                Location = new System.Drawing.Point(400, 155),
                Size = new System.Drawing.Size(120, 40),
                Font = new System.Drawing.Font("微软雅黑", 10F)
            };
            btn_TestSend.Click += Btn_TestSend_Click;
            groupBox8.Controls.Add(btn_TestSend);
        }

        private async void RefreshComPorts()
        {
            var portNames = await Task.Run(() => GetPortNamesWithDescription());
            bool changed = !portNames.SequenceEqual(cmb_Ports.Items.Cast<string>());
            if (changed)
            {
                cmb_Ports.BeginUpdate();
                cmb_Ports.Items.Clear();
                cmb_Ports.Items.AddRange(portNames.ToArray());
                if (cmb_Ports.Items.Count > 0 && cmb_Ports.SelectedIndex == -1)
                    cmb_Ports.SelectedIndex = 0;
                cmb_Ports.EndUpdate();
            }
        }

        private List<string> GetPortNamesWithDescription()
        {
            List<string> list = new List<string>();
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            var match = Regex.Match(name, @"(COM\d+)");
                            if (match.Success)
                            {
                                string comPort = match.Groups[1].Value;
                                string desc = name.Replace(match.Value, "").Trim('(', ')', ' ');
                                list.Add($"{comPort} ({desc})");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WMI查询失败: " + ex.Message);
                list.AddRange(SerialPort.GetPortNames());
            }
            if (list.Count == 0)
                list.AddRange(SerialPort.GetPortNames());
            return list.Distinct().OrderBy(s => s).ToList();
        }

        private void InitImageCount()
        {
            try
            {
                string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CalibImages");
                if (Directory.Exists(folderPath))
                {
                    string[] files = Directory.GetFiles(folderPath, "calib_*.bmp");
                    if (files.Length > 0)
                    {
                        int maxNum = files.Select(f =>
                        {
                            string fileName = Path.GetFileNameWithoutExtension(f);
                            int.TryParse(fileName.Replace("calib_", ""), out int num);
                            return num;
                        }).Max();
                        calibImageCount = maxNum + 1;
                    }
                }
            }
            catch { calibImageCount = 0; }
        }

        // ---------- 相机控制 ----------
        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();
            int ret = DeviceEnumerator.EnumDevices(DeviceTLayerType.MvGigEDevice | DeviceTLayerType.MvUsbDevice, out devicesInFoList);
            if (ret != MvError.MV_OK || devicesInFoList.Count == 0)
            {
                MessageBox.Show("未找到相机");
                return;
            }

            foreach (IDeviceInfo device in devicesInFoList)
            {
                comboBox1.Items.Add(string.IsNullOrEmpty(device.UserDefinedName) ? device.ManufacturerName : device.UserDefinedName);
            }
            comboBox1.SelectedIndex = 0;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                return;
            try
            {
                device = DeviceFactory.CreateDevice(devicesInFoList[comboBox1.SelectedIndex]);
                device.Open();
                if (device is IGigEDevice eDevice)
                {
                    eDevice.GetOptimalPacketSize(out int packetSize);
                    eDevice.Parameters.SetIntValue("GevSCPSPacketSize", packetSize);
                }
                device.Parameters.SetEnumValueByString("AcquisitionMode", "Continuous");
                device.Parameters.SetEnumValueByString("TriggerMode", "Off");
                tssl_Camera.Text = "● 相机已连接";
                MessageBox.Show("相机打开成功！");
            }
            catch (Exception ex) { MessageBox.Show("打开失败:" + ex.Message); }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (device == null)
                return;
            isGrabbing = false;
            if (receiveThread != null && receiveThread.IsAlive)
                receiveThread.Join(500);
            device.StreamGrabber.StopGrabbing();
            device.Close();
            device.Dispose();
            device = null;
            tssl_Camera.Text = "○ 相机未连接";
            tssl_Grab.Text = "○ 未采集";
            MessageBox.Show("已关闭");
        }

        // ---------- 图像转换 ----------
        private bool _pixelFormatLogged = false;

        private Mat ConvertToBgrMat(IFrameOut frameOut)
        {
            int w = (int)frameOut.Image.Width;
            int h = (int)frameOut.Image.Height;
            MvGvspPixelType type = frameOut.Image.PixelType;
            IntPtr ptr = frameOut.Image.PixelDataPtr;

            if (!_pixelFormatLogged)
            {
                _pixelFormatLogged = true;
                string msg = $"[相机] 像素格式: {type} | 分辨率: {w}x{h}\r\n";
                Debug.WriteLine(msg);
                BeginInvoke(new Action(() => txt_Debug.AppendText(msg)));
            }

            Mat bgrMat = new Mat();

            switch (type)
            {
                case MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                    using (Mat src = Mat.FromPixelData(h, w, MatType.CV_8UC3, ptr))
                        src.CopyTo(bgrMat);
                    break;
                case MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                    using (Mat raw = Mat.FromPixelData(h, w, MatType.CV_8UC1, ptr))
                        Cv2.CvtColor(raw, bgrMat, ColorConversionCodes.BayerBG2BGR);
                    break;
                case MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                    using (Mat raw = Mat.FromPixelData(h, w, MatType.CV_8UC1, ptr))
                        Cv2.CvtColor(raw, bgrMat, ColorConversionCodes.BayerGB2BGR);
                    break;
                case MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                    using (Mat raw = Mat.FromPixelData(h, w, MatType.CV_8UC1, ptr))
                        Cv2.CvtColor(raw, bgrMat, ColorConversionCodes.BayerGR2BGR);
                    break;
                case MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                    using (Mat raw = Mat.FromPixelData(h, w, MatType.CV_8UC1, ptr))
                        Cv2.CvtColor(raw, bgrMat, ColorConversionCodes.BayerBG2BGR);
                    break;
                case MvGvspPixelType.PixelType_Gvsp_Mono8:
                    using (Mat gray = Mat.FromPixelData(h, w, MatType.CV_8UC1, ptr))
                        Cv2.CvtColor(gray, bgrMat, ColorConversionCodes.GRAY2BGR);
                    break;
                default:
                    try
                    {
                        using (Mat src = Mat.FromPixelData(h, w, MatType.CV_8UC3, ptr))
                            Cv2.CvtColor(src, bgrMat, ColorConversionCodes.BGR2RGB);
                    }
                    catch
                    {
                        using (Mat gray = Mat.FromPixelData(h, w, MatType.CV_8UC1, ptr))
                            Cv2.CvtColor(gray, bgrMat, ColorConversionCodes.GRAY2BGR);
                    }
                    break;
            }

            if (_config.SwapColorChannels)
            {
                Cv2.CvtColor(bgrMat, bgrMat, ColorConversionCodes.BGR2RGB);
            }

            return bgrMat;
        }

        // ---------- 采集线程 ----------
        private void button7_Click(object sender, EventArgs e)
        {
            if (device == null || isGrabbing)
                return;
            isGrabbing = true;
            receiveThread = new Thread(() =>
            {
                using (var decoder = new WeChatQRCodeDecoder())
                {
                    ReceiveThreadProcess(decoder);
                }
            })
            { IsBackground = true };
            receiveThread.Start();
            tssl_Grab.Text = "● 采集中";
            device.StreamGrabber.StartGrabbing();
        }

        private void ReceiveThreadProcess(WeChatQRCodeDecoder decoder)
        {
            int frameCount = 0;
            int successCount = 0;
            DateTime lastReport = DateTime.Now;
            Stopwatch uiUpdateTimer = Stopwatch.StartNew();

            while (isGrabbing && device != null)
            {
                IFrameOut frame;
                if (device.StreamGrabber.GetImageBuffer(1000, out frame) == MvError.MV_OK)
                {
                    try
                    {
                        using (Mat displayMat = ConvertToBgrMat(frame))
                        {
                            frameCount++;

                            // ================= 1. 核心后台处理：全速运行 =================
                            if (enableQR)
                            {
                                var sw = Stopwatch.StartNew();
                                var res = decoder.DetectAndDecode(displayMat);
                                sw.Stop();

                                if (res.texts != null && res.texts.Length > 0)
                                {
                                    // ===== 检测成功：正常绘制 + 更新数据库 + 保存角点 =====
                                    successCount++;
                                    var detectedQRs = DrawQRResults(displayMat, res.texts, res.points);
                                    UpdateQRDatabaseSync(detectedQRs);

                                    lock (_qrLock)
                                    {
                                        for (int i = 0; i < res.texts.Length; i++)
                                        {
                                            string content = res.texts[i];
                                            if (_qrDatabase.TryGetValue(content, out var existing))
                                            {
                                                float cx = 0, cy = 0;
                                                for (int k = 0; k < 4; k++)
                                                { cx += res.points[i][k].X; cy += res.points[i][k].Y; }
                                                existing.PrevCenter = new Point2f(cx / 4f, cy / 4f);
                                                float sideSum = 0;
                                                for (int k = 0; k < 4; k++)
                                                {
                                                    float dx = res.points[i][(k + 1) % 4].X - res.points[i][k].X;
                                                    float dy = res.points[i][(k + 1) % 4].Y - res.points[i][k].Y;
                                                    sideSum += (float)Math.Sqrt(dx * dx + dy * dy);
                                                }
                                                existing.BoxSize = sideSum / 4f;
                                                existing.FlowFrames = 0;
                                                existing.CornerOffsets = new Point2f[4];
                                                for (int k = 0; k < 4; k++)
                                                {
                                                    existing.CornerOffsets[k] = new Point2f(
                                                        res.points[i][k].X - existing.PrevCenter.X,
                                                        res.points[i][k].Y - existing.PrevCenter.Y);
                                                }
                                            }
                                        }
                                    }

                                    var hasIdQRs = detectedQRs.Where(q => q.AssignedID > 0).ToList();
                                    if (hasIdQRs.Count >= 2)
                                        DrawFingerLines(displayMat, hasIdQRs, anyPredicted: false);
                                }
                                else
                                {
                                    // ===== 解码失败 → 光流跟踪 =====
                                    bool flowTracked = false;
                                    if (_prevGray != null && !_prevGray.Empty())
                                    {
                                        using (Mat currGray = new Mat())
                                        {
                                            Cv2.CvtColor(displayMat, currGray, ColorConversionCodes.BGR2GRAY);
                                            flowTracked = FlowTrackQRs(_prevGray, currGray);
                                        }
                                    }

                                    if (flowTracked)
                                    {
                                        if (frameCount % 5 == 0)
                                        {
                                            using (Mat gray = new Mat())
                                            using (Mat thresh = new Mat())
                                            using (Mat threshBgr = new Mat())
                                            {
                                                Cv2.CvtColor(displayMat, gray, ColorConversionCodes.BGR2GRAY);
                                                Cv2.AdaptiveThreshold(gray, thresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 21, 2);
                                                Cv2.CvtColor(thresh, threshBgr, ColorConversionCodes.GRAY2BGR);

                                                var fb = decoder.DetectAndDecode(threshBgr, skipPreprocess: true);
                                                if (fb.texts != null && fb.texts.Length > 0)
                                                {
                                                    successCount++;
                                                    lock (_qrLock)
                                                    {
                                                        for (int i = 0; i < fb.texts.Length; i++)
                                                        {
                                                            string content = fb.texts[i];
                                                            if (_qrDatabase.TryGetValue(content, out var existing))
                                                            {
                                                                float cx = 0, cy = 0;
                                                                for (int k = 0; k < 4; k++)
                                                                { cx += fb.points[i][k].X; cy += fb.points[i][k].Y; }
                                                                existing.PrevCenter = new Point2f(cx / 4f, cy / 4f);
                                                                float sideSum = 0;
                                                                for (int k = 0; k < 4; k++)
                                                                {
                                                                    float dx = fb.points[i][(k + 1) % 4].X - fb.points[i][k].X;
                                                                    float dy = fb.points[i][(k + 1) % 4].Y - fb.points[i][k].Y;
                                                                    sideSum += (float)Math.Sqrt(dx * dx + dy * dy);
                                                                }
                                                                existing.BoxSize = sideSum / 4f;
                                                                existing.FlowFrames = 0;
                                                                existing.CornerOffsets = new Point2f[4];
                                                                for (int k = 0; k < 4; k++)
                                                                {
                                                                    existing.CornerOffsets[k] = new Point2f(
                                                                        fb.points[i][k].X - existing.PrevCenter.X,
                                                                        fb.points[i][k].Y - existing.PrevCenter.Y);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        DrawFlowOverlay(displayMat);
                                    }
                                    else
                                    {
                                        using (Mat gray = new Mat())
                                        using (Mat thresh = new Mat())
                                        using (Mat threshBgr = new Mat())
                                        {
                                            Cv2.CvtColor(displayMat, gray, ColorConversionCodes.BGR2GRAY);
                                            Cv2.AdaptiveThreshold(gray, thresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 21, 2);
                                            Cv2.CvtColor(thresh, threshBgr, ColorConversionCodes.GRAY2BGR);

                                            var fb = decoder.DetectAndDecode(threshBgr, skipPreprocess: true);
                                            if (fb.texts != null && fb.texts.Length > 0)
                                            {
                                                successCount++;
                                                var fg = DrawQRResults(displayMat, fb.texts, fb.points);
                                                UpdateQRDatabaseSync(fg);

                                                lock (_qrLock)
                                                {
                                                    for (int i = 0; i < fb.texts.Length; i++)
                                                    {
                                                        string content = fb.texts[i];
                                                        if (_qrDatabase.TryGetValue(content, out var existing))
                                                        {
                                                            float cx = 0, cy = 0;
                                                            for (int k = 0; k < 4; k++)
                                                            { cx += fb.points[i][k].X; cy += fb.points[i][k].Y; }
                                                            existing.PrevCenter = new Point2f(cx / 4f, cy / 4f);
                                                            float sideSum = 0;
                                                            for (int k = 0; k < 4; k++)
                                                            {
                                                                float dx = fb.points[i][(k + 1) % 4].X - fb.points[i][k].X;
                                                                float dy = fb.points[i][(k + 1) % 4].Y - fb.points[i][k].Y;
                                                                sideSum += (float)Math.Sqrt(dx * dx + dy * dy);
                                                            }
                                                            existing.BoxSize = sideSum / 4f;
                                                            existing.FlowFrames = 0;
                                                            existing.CornerOffsets = new Point2f[4];
                                                            for (int k = 0; k < 4; k++)
                                                            {
                                                                existing.CornerOffsets[k] = new Point2f(
                                                                    fb.points[i][k].X - existing.PrevCenter.X,
                                                                    fb.points[i][k].Y - existing.PrevCenter.Y);
                                                            }
                                                        }
                                                    }
                                                }

                                                var hasId = fg.Where(q => q.AssignedID > 0).ToList();
                                                if (hasId.Count >= 2)
                                                    DrawFingerLines(displayMat, hasId, anyPredicted: false);
                                            }
                                            else
                                            {
                                                UpdateQRDatabaseSync(new List<TrackedQR>());
                                                if (frameCount % 30 == 0)
                                                {
                                                    double successRate = 100.0 * successCount / frameCount;
                                                    BeginInvoke(new Action(() =>
                                                    {
                                                        txt_Debug.AppendText($"[统计] 已处理 {frameCount} 帧, 成功 {successCount} 次, 成功率 {successRate:F1}%\r\n");
                                                    }));
                                                }
                                            }
                                        }
                                    }
                                }

                                // ===== 更新 _prevGray =====
                                using (Mat currGray = new Mat())
                                {
                                    Cv2.CvtColor(displayMat, currGray, ColorConversionCodes.BGR2GRAY);
                                    _prevGray?.Dispose();
                                    _prevGray = currGray.Clone();
                                }

                                if ((DateTime.Now - _lastListRefresh).TotalMilliseconds >= 500)
                                {
                                    _lastListRefresh = DateTime.Now;
                                    BeginInvoke(new Action(RefreshQRListBox));
                                }

                                UpdatePoseTextDisplay();

                                if ((DateTime.Now - lastReport).TotalSeconds >= 5)
                                {
                                    lastReport = DateTime.Now;
                                    frameCount = 0;
                                    successCount = 0;
                                    BeginInvoke(new Action(() =>
                                    {
                                        string imuInfo = _imuDriver.IsConnected
                                            ? $"IMU Roll: {_lastImuRoll:F2} | horizontalAngleImg: {_lastHorizontalAngleImg:F2}\r\n"
                                            : "IMU: 未连接 (默认 Roll=180)\r\n";
                                        string serialInfo = _serialEnabled && _serialPort.IsOpen
                                            ? $"[串口] ● {_serialPort.PortName} @ {_serialPort.BaudRate} bps | 已发送 {_serialSentCount} 帧\r\n"
                                            : "[串口] ○ 未连接\r\n";
                                        txt_Debug.Clear();
                                        txt_Debug.AppendText(imuInfo + serialInfo);
                                    }));
                                }
                            }

                            // ================= 2. 界面显示渲染：降频处理 =================
                            if (uiUpdateTimer.ElapsedMilliseconds > 33)
                            {
                                uiUpdateTimer.Restart();
                                Bitmap bmp = BitmapConverter.ToBitmap(displayMat);
                                BeginInvoke(new Action(() =>
                                {
                                    Image oldImage = pictureBox1.Image;
                                    pictureBox1.Image = bmp;
                                    if (oldImage != null)
                                        oldImage.Dispose();
                                }));
                            }
                        }
                    }
                    catch (Exception ex) { Debug.WriteLine($"处理帧异常: {ex.Message}"); }
                    finally { device.StreamGrabber.FreeImageBuffer(frame); }
                }
            }
        }

        private List<TrackedQR> DrawQRResults(Mat img, string[] texts, Point2f[][] allPoints)
        {
            var detected = new List<TrackedQR>();
            var seenInFrame = new HashSet<string>();

            float h = SafeQRSize / 2.0f;
            Point3f[] objPoints = {
                new Point3f(-h, h, 0),
                new Point3f(h, h, 0),
                new Point3f(h, -h, 0),
                new Point3f(-h, -h, 0)
            };

            for (int i = 0; i < texts.Length; i++)
            {
                string content = texts[i];
                if (seenInFrame.Contains(content))
                    continue;
                seenInFrame.Add(content);

                Point2f[] pts = allPoints[i];
                float cx = 0, cy = 0;
                for (int j = 0; j < 4; j++)
                {
                    var p1 = new OpenCvSharp.Point((int)pts[j].X, (int)pts[j].Y);
                    var p2 = new OpenCvSharp.Point((int)pts[(j + 1) % 4].X, (int)pts[(j + 1) % 4].Y);
                    Cv2.Line(img, p1, p2, Scalar.Red, 3);
                    cx += pts[j].X;
                    cy += pts[j].Y;
                }
                cx /= 4.0f;
                cy /= 4.0f;

                using (Mat rvec = new Mat())
                using (Mat tvec = new Mat())
                {
                    Cv2.SolvePnP(InputArray.Create(objPoints), InputArray.Create(pts), cameraMatrix, distCoeffs, rvec, tvec);
                    if (!tvec.Empty())
                    {
                        double tx = tvec.At<double>(0, 0);
                        double ty = tvec.At<double>(1, 0);
                        double tz = tvec.At<double>(2, 0);

                        var txtPos = new OpenCvSharp.Point((int)pts[0].X, (int)pts[0].Y - 15);
                        Cv2.PutText(img, $"X:{tx:F1} Y:{ty:F1} Z:{tz:F1}", txtPos, HersheyFonts.HersheySimplex, 0.7, Scalar.Green, 2);

                        detected.Add(new TrackedQR
                        {
                            Content = content,
                            X = (float)tx,
                            Y = (float)ty,
                            Z = (float)tz,
                            PixelX = cx,
                            PixelY = cy,
                            LastSeen = DateTime.Now,
                            IsVisible = true
                        });
                    }
                }
            }

            return detected;
        }

        private void UpdatePoseTextDisplay()
        {
            if ((DateTime.Now - _lastTextRefresh).TotalMilliseconds >= 500)
            {
                _lastTextRefresh = DateTime.Now;
                string poseText;
                lock (_qrLock)
                {
                    var visible = _qrDatabase.Values.Where(q => q.IsVisible).ToList();
                    poseText = visible.Count > 0
                        ? string.Join("\r\n", visible.Select(d => $"{d.Content} -> X:{d.SmoothedX:F1}, Y:{d.SmoothedY:F1}, Z:{d.SmoothedZ:F1}"))
                        : "未检测到二维码";
                }
                BeginInvoke(new Action(() => { txt_PoseResult.Text = poseText; }));
            }
        }

        // ---------- 手指连线及夹角计算（集成串口发送） ----------
        private void DrawFingerLines(Mat img, List<TrackedQR> detected, bool anyPredicted = false)
        {
            var qrDict = detected
                .Where(q => q.AssignedID > 0)
                .GroupBy(q => q.AssignedID)
                .ToDictionary(g => g.Key, g => g.First());

            if (qrDict.Count < 2)
                return;

            var anglesForSend = new List<(byte id, float angle)>();
            Scalar lineColor = Scalar.Blue;
            Scalar distColor = Scalar.Yellow;
            Scalar angleColor = Scalar.Magenta;

            // ---------------- 遍历各个 ID 处理 ----------------
            foreach (var id in qrDict.Keys)
            {
                var cur = qrDict[id];

                // ---------------- A. 连线与距离 (ID -> ID+1) ----------------
                if (qrDict.TryGetValue(id + 1, out var next))
                {
                    var p1 = new OpenCvSharp.Point((int)cur.PixelX, (int)cur.PixelY);
                    var p2 = new OpenCvSharp.Point((int)next.PixelX, (int)next.PixelY);
                    Cv2.Line(img, p1, p2, lineColor, 3, LineTypes.AntiAlias);

                    double dist3D = Math.Sqrt(Math.Pow(next.SmoothedX - cur.SmoothedX, 2) +
                                              Math.Pow(next.SmoothedY - cur.SmoothedY, 2) +
                                              Math.Pow(next.SmoothedZ - cur.SmoothedZ, 2));

                    var textPos = new OpenCvSharp.Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                    Cv2.PutText(img, $"{dist3D:F1}mm", textPos, HersheyFonts.HersheySimplex, 0.8, distColor, 2);
                }

                // ---------------- B. 夹角结算 (ID-1 -> ID -> ID+1 左侧夹角) ----------------
                if (qrDict.TryGetValue(id - 1, out var prev) && qrDict.TryGetValue(id + 1, out var nextPt))
                {
                    float ax = prev.PixelX - cur.PixelX;
                    float ay = prev.PixelY - cur.PixelY;
                    float bx = nextPt.PixelX - cur.PixelX;
                    float by = nextPt.PixelY - cur.PixelY;

                    if (Math.Abs(ax) > 0.1f || Math.Abs(ay) > 0.1f)
                    {
                        float angleA = (float)(Math.Atan2(ay, ax) * 180.0 / Math.PI);
                        float angleB = (float)(Math.Atan2(by, bx) * 180.0 / Math.PI);
                        // 左侧角度: 从左邻(prev)逆时针到右邻(next)
                        float ccwAngle = (angleB - angleA + 720) % 360;

                        int angleKey = id;
                        _smoothedAngles.TryGetValue(angleKey, out float prevSmoothed);
                        float smoothed;
                        if (_smoothedAngles.ContainsKey(angleKey))
                        {
                            float diff = ccwAngle - prevSmoothed;
                            if (diff > 180)
                                diff -= 360;
                            else if (diff < -180)
                                diff += 360;
                            smoothed = prevSmoothed + ANGLE_SMOOTHING_ALPHA * diff;
                            if (smoothed < 0)
                                smoothed += 360;
                            else if (smoothed >= 360)
                                smoothed -= 360;
                        }
                        else
                        {
                            smoothed = ccwAngle;
                        }
                        _smoothedAngles[angleKey] = smoothed;

                        float drawStart = angleA;
                        if (drawStart < 0)
                            drawStart += 360;

                        Cv2.Ellipse(img, new OpenCvSharp.Point((int)cur.PixelX, (int)cur.PixelY),
                                    new OpenCvSharp.Size(35, 35), 0, drawStart, drawStart + smoothed, angleColor, 2);

                        var angleTextPos = new OpenCvSharp.Point((int)cur.PixelX + 15, (int)cur.PixelY + 25);
                        Cv2.PutText(img, $"ID{id}: {smoothed:F1} deg", angleTextPos, HersheyFonts.HersheySimplex, 0.7, angleColor, 2);

                        anglesForSend.Add(((byte)id, smoothed));
                    }
                }
            }

            // ---------------- C. ID1-ID2 连线与水平参考线夹角 (以ID0输出串口) ----------------
            if (qrDict.TryGetValue(1, out var qr1) && qrDict.TryGetValue(2, out var qr2))
            {
                // 水平仪: 始终平行于地面的绝对水平线
                // 相机CCW翻滚 → 画面顺时针转 → 水平线需逆时针补偿
                float imuRoll = _imuDriver.IsConnected ? _imuDriver.GetLatestData().Roll : 180f;
                // roll∈[-180,180], 水平=±180
                // roll>0(CW转): 补偿为负, roll<0(CCW转): 补偿为正
                float horizontalAngleImg = imuRoll >= 0f ? (imuRoll - 180f) : (imuRoll + 180f);
                _lastImuRoll = imuRoll;
                _lastHorizontalAngleImg = horizontalAngleImg;

                float dx = qr2.PixelX - qr1.PixelX;
                float dy = qr2.PixelY - qr1.PixelY;
                float lineAngle = (float)(Math.Atan2(dy, dx) * 180.0 / Math.PI);

                // 左侧角度: 从 ID1-ID2 连线逆时针转到水平参考左方向
                float angleH = horizontalAngleImg + 180f;
                float ccwAngle = (lineAngle - angleH + 720) % 360;

                int angleKey = 0;
                _smoothedAngles.TryGetValue(angleKey, out float prevSmoothed);
                float smoothedAngle;
                if (_smoothedAngles.ContainsKey(angleKey))
                {
                    float diff = ccwAngle - prevSmoothed;
                    if (diff > 180)
                        diff -= 360;
                    else if (diff < -180)
                        diff += 360;
                    smoothedAngle = prevSmoothed + ANGLE_SMOOTHING_ALPHA * diff;
                    if (smoothedAngle < 0)
                        smoothedAngle += 360;
                    else if (smoothedAngle >= 360)
                        smoothedAngle -= 360;
                }
                else
                {
                    smoothedAngle = ccwAngle;
                }
                _smoothedAngles[angleKey] = smoothedAngle;

                float midX = (qr1.PixelX + qr2.PixelX) / 2;
                float midY = (qr1.PixelY + qr2.PixelY) / 2;
                var midPt = new OpenCvSharp.Point((int)midX, (int)midY);

                float hLen = 150;
                float hAngleRad = horizontalAngleImg * (float)(Math.PI / 180.0);
                float cosR = (float)Math.Cos(hAngleRad);
                float sinR = (float)Math.Sin(hAngleRad);
                float rx = hLen * cosR;
                float ry = hLen * sinR;

                var hLeft = new OpenCvSharp.Point(midPt.X - rx, midPt.Y - ry);
                var hRight = new OpenCvSharp.Point(midPt.X + rx, midPt.Y + ry);

                // 画出维持真实物理水平的参考线 (黄色)
                Cv2.Line(img, hLeft, hRight, new Scalar(0, 255, 255), 2, LineTypes.AntiAlias);
                // 使用箭头标出它的指向性 (朝向左方作为基准边)
                Cv2.ArrowedLine(img, midPt, hLeft, new Scalar(0, 255, 255), 2, tipLength: 0.1);

                // 圆弧渲染: 从 angleH 逆时针画到 lineAngle
                float angleHDeg = angleH;
                if (angleHDeg < 0)
                    angleHDeg += 360;
                Cv2.Ellipse(img, midPt, new OpenCvSharp.Size(45, 45), 0, angleHDeg, angleHDeg + smoothedAngle, new Scalar(0, 255, 255), 2);

                string label = $"Base: {smoothedAngle:F1} deg";
                var labelPos = new OpenCvSharp.Point(midPt.X + 10, midPt.Y - 15);
                Cv2.PutText(img, label, labelPos, HersheyFonts.HersheySimplex, 0.7, new Scalar(0, 255, 255), 2);

                // 添加至队列串口发送
                anglesForSend.Add((0, smoothedAngle));
            }

            _latestAnglesForSend = anglesForSend;
        }

        // ---------- 二维码数据库更新（含抗抖动平滑跟踪） ----------
        private void UpdateQRDatabaseSync(List<TrackedQR> detected)
        {
            var now = DateTime.Now;
            var detectedContents = new HashSet<string>(detected.Select(d => d.Content));

            lock (_qrLock)
            {
                // ========== Step 1: 处理未检测到的 QR ==========
                foreach (var qr in _qrDatabase.Values)
                {
                    if (!detectedContents.Contains(qr.Content))
                    {
                        qr.MissedCount++;
                        if (qr.MissedCount <= MAX_MISSED_FRAMES && qr.IsVisible)
                        {
                            double dt = (now - qr.LastUpdateTime).TotalSeconds;
                            if (dt > 0.001 && dt < 0.2)
                            {
                                float px = qr.SmoothedX + qr.VelocityX * (float)dt;
                                float py = qr.SmoothedY + qr.VelocityY * (float)dt;
                                float pz = qr.SmoothedZ + qr.VelocityZ * (float)dt;

                                float dx = px - qr.SmoothedX;
                                float dy = py - qr.SmoothedY;
                                float dz = pz - qr.SmoothedZ;
                                float dist = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);

                                if (dist < PREDICTION_MAX_DELTA)
                                {
                                    qr.SmoothedX = px;
                                    qr.SmoothedY = py;
                                    qr.SmoothedZ = pz;
                                }
                            }
                            qr.LastUpdateTime = now;
                        }
                        else
                        {
                            qr.IsVisible = false;
                        }
                    }
                }

                // ========== Step 2: 处理本次检测到的 QR ==========
                foreach (var det in detected)
                {
                    if (_qrDatabase.TryGetValue(det.Content, out var existing))
                    {
                        int prevMissed = existing.MissedCount;
                        existing.MissedCount = 0;
                        existing.IsVisible = true;

                        double dt = (now - existing.LastUpdateTime).TotalSeconds;
                        if (dt > 0.001 && dt < 1.0)
                        {
                            existing.VelocityX = (float)((det.X - existing.X) / dt);
                            existing.VelocityY = (float)((det.Y - existing.Y) / dt);
                            existing.VelocityZ = (float)((det.Z - existing.Z) / dt);
                        }
                        else
                        {
                            existing.VelocityX = 0;
                            existing.VelocityY = 0;
                            existing.VelocityZ = 0;
                        }

                        existing.X = det.X;
                        existing.Y = det.Y;
                        existing.Z = det.Z;
                        existing.PixelX = det.PixelX;
                        existing.PixelY = det.PixelY;
                        existing.LastSeen = now;
                        existing.LastUpdateTime = now;

                        if (existing.IsFirstFrame)
                        {
                            existing.SmoothedX = det.X;
                            existing.SmoothedY = det.Y;
                            existing.SmoothedZ = det.Z;
                            existing.IsFirstFrame = false;
                        }
                        else
                        {
                            float alpha = prevMissed > 0 ? 0.6f : SMOOTHING_ALPHA;
                            existing.SmoothedX = alpha * det.X + (1f - alpha) * existing.SmoothedX;
                            existing.SmoothedY = alpha * det.Y + (1f - alpha) * existing.SmoothedY;
                            existing.SmoothedZ = alpha * det.Z + (1f - alpha) * existing.SmoothedZ;
                        }

                        det.SmoothedX = existing.SmoothedX;
                        det.SmoothedY = existing.SmoothedY;
                        det.SmoothedZ = existing.SmoothedZ;
                        det.AssignedID = existing.AssignedID;

                        if (existing.AssignedID == 0 && existing.LastUsedID.HasValue)
                            existing.AssignedID = existing.LastUsedID.Value;
                    }
                    else
                    {
                        det.MissedCount = 0;
                        det.SmoothedX = det.X;
                        det.SmoothedY = det.Y;
                        det.SmoothedZ = det.Z;
                        det.VelocityX = 0;
                        det.VelocityY = 0;
                        det.VelocityZ = 0;
                        det.LastUpdateTime = now;
                        det.IsFirstFrame = false;

                        if (_savedQrIdMappings != null && _savedQrIdMappings.TryGetValue(det.Content, out int savedId))
                        {
                            det.AssignedID = savedId;
                            det.LastUsedID = savedId;
                        }
                        else
                        {
                            det.AssignedID = 0;
                        }
                        _qrDatabase[det.Content] = det;
                    }
                }
            }
        }

        private List<TrackedQR> GetVisibleQRsForDrawing()
        {
            lock (_qrLock)
            {
                return _qrDatabase.Values
                    .Where(q => q.IsVisible && q.AssignedID > 0)
                    .Select(q => new TrackedQR
                    {
                        Content = q.Content,
                        AssignedID = q.AssignedID,
                        X = q.X,
                        Y = q.Y,
                        Z = q.Z,
                        SmoothedX = q.SmoothedX,
                        SmoothedY = q.SmoothedY,
                        SmoothedZ = q.SmoothedZ,
                        PixelX = q.PixelX,
                        PixelY = q.PixelY,
                        IsVisible = true,
                        LastSeen = q.LastSeen,
                        MissedCount = q.MissedCount
                    })
                    .ToList();
            }
        }

        private void RefreshQRListBox()
        {
            if (listBox_AllQRs.InvokeRequired || listBox_VisibleQRs.InvokeRequired)
            {
                listBox_AllQRs.BeginInvoke(new Action(RefreshQRListBox));
                return;
            }

            listBox_AllQRs.BeginUpdate();
            listBox_VisibleQRs.BeginUpdate();

            string selectedContent = (listBox_AllQRs.SelectedItem as TrackedQR)?.Content;

            lock (_qrLock)
            {
                var allQRs = _qrDatabase.Values
                    .OrderByDescending(q => q.IsVisible)
                    .ThenBy(q => q.AssignedID == 0 ? int.MaxValue : q.AssignedID)
                    .ThenBy(q => q.Content)
                    .ToList();

                var currentQRs = allQRs.Where(q => q.IsVisible).ToList();

                listBox_AllQRs.Items.Clear();
                listBox_AllQRs.Items.AddRange(allQRs.ToArray());

                if (selectedContent != null)
                {
                    for (int i = 0; i < listBox_AllQRs.Items.Count; i++)
                    {
                        if ((listBox_AllQRs.Items[i] as TrackedQR)?.Content == selectedContent)
                        {
                            listBox_AllQRs.SelectedIndex = i;
                            break;
                        }
                    }
                }

                listBox_VisibleQRs.Items.Clear();
                foreach (var q in currentQRs)
                {
                    listBox_VisibleQRs.Items.Add(q);
                }
            }

            listBox_AllQRs.EndUpdate();
            listBox_VisibleQRs.EndUpdate();
        }

        // ========== 光流角点跟踪 ==========
        private bool FlowTrackQRs(Mat prevGray, Mat currGray)
        {
            bool anyTracked = false;
            lock (_qrLock)
            {
                foreach (var qr in _qrDatabase.Values)
                {
                    if (!qr.IsVisible)
                        continue;

                    try
                    {
                        Point2f[] prevCenter = new Point2f[] { qr.PrevCenter };
                        Point2f[] nextCenter = new Point2f[1];
                        byte[] status = new byte[1];
                        float[] err = new float[1];

                        Cv2.CalcOpticalFlowPyrLK(prevGray, currGray,
                            prevCenter, ref nextCenter, out status, out err,
                            new OpenCvSharp.Size(31, 31), 3,
                            new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 30, 0.01),
                            OpticalFlowFlags.None, 0.001);

                        if (status[0] == 1)
                        {
                            float dx = nextCenter[0].X - qr.PrevCenter.X;
                            float dy = nextCenter[0].Y - qr.PrevCenter.Y;
                            if (dx * dx + dy * dy < 6400)
                            {
                                qr.PrevCenter = nextCenter[0];
                                qr.PixelX = nextCenter[0].X;
                                qr.PixelY = nextCenter[0].Y;
                                qr.FlowFrames++;
                                qr.MissedCount = 0;
                                qr.IsVisible = true;
                                anyTracked = true;
                            }
                            else
                            {
                                qr.FlowFrames = MAX_FLOW_FRAMES + 1;
                            }
                        }
                        else
                        {
                            qr.FlowFrames = MAX_FLOW_FRAMES + 1;
                        }
                    }
                    catch
                    {
                        qr.FlowFrames = MAX_FLOW_FRAMES + 1;
                    }
                }
            }
            return anyTracked;
        }

        private void DrawFlowOverlay(Mat img)
        {
            List<TrackedQR> flowQRs;
            lock (_qrLock)
            {
                flowQRs = _qrDatabase.Values
                    .Where(q => q.IsVisible && q.BoxSize > 0
                        && q.FlowFrames <= MAX_FLOW_FRAMES)
                    .Select(q => new TrackedQR
                    {
                        Content = q.Content,
                        AssignedID = q.AssignedID,
                        PrevCenter = q.PrevCenter,
                        CornerOffsets = q.CornerOffsets,
                        PixelX = q.PixelX,
                        PixelY = q.PixelY,
                        SmoothedX = q.SmoothedX,
                        SmoothedY = q.SmoothedY,
                        SmoothedZ = q.SmoothedZ,
                        BoxSize = q.BoxSize,
                        IsVisible = true,
                    })
                    .ToList();
            }

            float halfQR = SafeQRSize / 2.0f;
            Point3f[] objPoints = {
                new Point3f(-halfQR, halfQR, 0),
                new Point3f(halfQR, halfQR, 0),
                new Point3f(halfQR, -halfQR, 0),
                new Point3f(-halfQR, -halfQR, 0)
            };

            foreach (var qr in flowQRs)
            {
                if (qr.CornerOffsets == null || qr.CornerOffsets.Length < 4)
                    continue;

                Point2f[] corners = new Point2f[4];
                for (int k = 0; k < 4; k++)
                {
                    corners[k] = new Point2f(
                        qr.PrevCenter.X + qr.CornerOffsets[k].X,
                        qr.PrevCenter.Y + qr.CornerOffsets[k].Y);
                }

                using (Mat rvec = new Mat())
                using (Mat tvec = new Mat())
                {
                    try
                    {
                        Cv2.SolvePnP(InputArray.Create(objPoints), InputArray.Create(corners),
                            cameraMatrix, distCoeffs, rvec, tvec);
                        if (!tvec.Empty())
                        {
                            qr.SmoothedX = (float)tvec.At<double>(0, 0);
                            qr.SmoothedY = (float)tvec.At<double>(1, 0);
                            qr.SmoothedZ = (float)tvec.At<double>(2, 0);
                        }
                    }
                    catch { }
                }
            }

            foreach (var qr in flowQRs)
            {
                if (qr.BoxSize <= 0)
                    continue;
                float half = qr.BoxSize / 2f;
                var p1 = new OpenCvSharp.Point((int)(qr.PixelX - half), (int)(qr.PixelY - half));
                var p2 = new OpenCvSharp.Point((int)(qr.PixelX + half), (int)(qr.PixelY + half));
                Cv2.Rectangle(img, p1, p2, Scalar.Yellow, 2);

                if (qr.AssignedID > 0)
                {
                    var labelPos = new OpenCvSharp.Point((int)(qr.PixelX - half), (int)(qr.PixelY - half - 5));
                    Cv2.PutText(img, $"ID:{qr.AssignedID}", labelPos, HersheyFonts.HersheySimplex, 0.6, Scalar.Yellow, 1);
                }
            }

            var withId = flowQRs.Where(q => q.AssignedID > 0)
                .OrderBy(q => q.AssignedID)
                .ToList();
            if (withId.Count >= 2)
                DrawFingerLines(img, withId, anyPredicted: false);
        }

        // ---------- QR ID 持久化：加载 ----------
        private void LoadQrIdMappings()
        {
            _savedQrIdMappings = new Dictionary<string, int>();
            if (_config.QrIdAssignments != null)
            {
                foreach (var mapping in _config.QrIdAssignments)
                {
                    if (!string.IsNullOrEmpty(mapping.Content) && mapping.AssignedId > 0)
                    {
                        _savedQrIdMappings[mapping.Content] = mapping.AssignedId;
                    }
                }
            }
        }

        // ---------- QR ID 持久化：保存到 config.json ----------
        private void SaveQrIdMappings()
        {
            lock (_qrLock)
            {
                _config.QrIdAssignments = _qrDatabase.Values
                    .Where(q => q.AssignedID > 0)
                    .Select(q => new QrIdAssignment { Content = q.Content, AssignedId = q.AssignedID })
                    .ToList();
            }
            ConfigManager.Save(_config);
        }

        // ---------- 分配 ID ----------
        private void btn_AssignID_Click(object sender, EventArgs e)
        {
            if (listBox_AllQRs.SelectedItem is TrackedQR qr && int.TryParse(txt_AssignID.Text, out int id) && id > 0)
            {
                lock (_qrLock)
                {
                    if (_qrDatabase.Values.Any(v => v.AssignedID == id && v.Content != qr.Content))
                    {
                        MessageBox.Show("该 ID 已被占用，请使用其他 ID。");
                        return;
                    }
                    qr.AssignedID = id;
                    qr.LastUsedID = id;
                }
                SaveQrIdMappings();
                RefreshQRListBox();
                txt_Debug.AppendText($"✔ ID 分配已保存：{qr.Content} → ID {id}\r\n");
            }
        }

        // ---------- IMU 控制 ----------
        private async void RefreshIMUPorts()
        {
            var portNames = await Task.Run(() => GetPortNamesWithDescription());
            bool changed = !portNames.SequenceEqual(cmb_IMU_Port.Items.Cast<string>());
            if (changed)
            {
                cmb_IMU_Port.BeginUpdate();
                cmb_IMU_Port.Items.Clear();
                cmb_IMU_Port.Items.AddRange(portNames.ToArray());
                if (cmb_IMU_Port.Items.Count > 0 && cmb_IMU_Port.SelectedIndex == -1)
                    cmb_IMU_Port.SelectedIndex = 0;
                cmb_IMU_Port.EndUpdate();
            }
        }

        private void btn_IMU_Connect_Click(object sender, EventArgs e)
        {
            if (_imuDriver.IsConnected)
            {
                _imuDriver.Disconnect();
                btn_IMU_Connect.Text = "连接 IMU";
                lbl_IMU_Status.Text = "○ 未连接";
            }
            else
            {
                try
                {
                    if (cmb_IMU_Port.SelectedItem == null)
                    {
                        MessageBox.Show("请选择 IMU 串口");
                        return;
                    }
                    string portStr = cmb_IMU_Port.SelectedItem.ToString();
                    string portName = portStr.Split(' ')[0];
                    int baud = int.Parse(cmb_IMU_Baud.SelectedItem.ToString());
                    if (_imuDriver.Connect(portName, baud))
                    {
                        btn_IMU_Connect.Text = "断开 IMU";
                        lbl_IMU_Status.Text = "● 已连接";
                        txt_Debug.AppendText($"✔ IMU 已连接: {portName} @ {baud}\r\n");
                    }
                    else
                    {
                        MessageBox.Show("IMU 连接失败: " + _imuDriver.LastError);
                    }
                }
                catch (Exception ex) { MessageBox.Show("IMU 连接异常: " + ex.Message); }
            }
        }

        private void btn_EnterSetting_Click(object sender, EventArgs e)
        {
            _imuDriver.EnterSettingMode();
            txt_Debug.AppendText("→ IMU 进入设置模式\r\n");
        }

        private void btn_ExitSetting_Click(object sender, EventArgs e)
        {
            _imuDriver.ExitSettingMode();
            txt_Debug.AppendText("→ IMU 退出设置模式\r\n");
        }

        private void btn_SetZero_Click(object sender, EventArgs e)
        {
            _imuDriver.EnterSettingMode();
            System.Threading.Thread.Sleep(50);
            _imuDriver.ExitSettingMode();
            txt_Debug.AppendText("→ IMU 置零\r\n");
        }

        private void btn_AccelCali_Click(object sender, EventArgs e)
        {
            _imuDriver.AccelCalibration();
            txt_Debug.AppendText("→ 加速度校准指令已发送\r\n");
        }

        private void btn_GyroCali_Click(object sender, EventArgs e)
        {
            _imuDriver.GyroCalibration();
            txt_Debug.AppendText("→ 陀螺仪校准指令已发送\r\n");
        }

        private void btn_SaveIMUParams_Click(object sender, EventArgs e)
        {
            _imuDriver.SaveParams();
            txt_Debug.AppendText("→ IMU 参数已保存\r\n");
        }

        private void btn_RebootIMU_Click(object sender, EventArgs e)
        {
            _imuDriver.Reboot();
            txt_Debug.AppendText("→ IMU 重启指令已发送\r\n");
        }

        private DateTime _lastImuDiagLog = DateTime.MinValue;

        private void ImuUiTimer_Tick(object sender, EventArgs e)
        {
            if (!_imuDriver.IsConnected)
            {
                txt_AccelX.Text = "0.00";
                txt_AccelY.Text = "0.00";
                txt_AccelZ.Text = "0.00";
                txt_GyroX.Text = "0.00";
                txt_GyroY.Text = "0.00";
                txt_GyroZ.Text = "0.00";
                txt_Roll.Text = "0.00";
                txt_Pitch.Text = "0.00";
                txt_Yaw.Text = "0.00";
                txt_Q0.Text = "0.0000";
                txt_Q1.Text = "0.0000";
                txt_Q2.Text = "0.0000";
                txt_Q3.Text = "0.0000";
                return;
            }
            var data = _imuDriver.GetLatestData();

            txt_AccelX.Text = data.AccelX.ToString("F2");
            txt_AccelY.Text = data.AccelY.ToString("F2");
            txt_AccelZ.Text = data.AccelZ.ToString("F2");
            txt_GyroX.Text = data.GyroX.ToString("F2");
            txt_GyroY.Text = data.GyroY.ToString("F2");
            txt_GyroZ.Text = data.GyroZ.ToString("F2");
            txt_Roll.Text = data.Roll.ToString("F2");
            txt_Pitch.Text = data.Pitch.ToString("F2");
            txt_Yaw.Text = data.Yaw.ToString("F2");
            txt_Q0.Text = data.Q0.ToString("F4");
            txt_Q1.Text = data.Q1.ToString("F4");
            txt_Q2.Text = data.Q2.ToString("F4");
            txt_Q3.Text = data.Q3.ToString("F4");

            if (data.FrameCount == 0 && (DateTime.Now - _lastImuDiagLog).TotalSeconds >= 3)
            {
                _lastImuDiagLog = DateTime.Now;
                var dump = _imuDriver.RawDump;
                string hex = dump != null ? BitConverter.ToString(dump) : "(无数据)";
                string msg = $"[IMU诊断] 已连接 {_imuDriver.TotalBytesRead} 字节, " +
                    $"有效帧 {_imuDriver.ValidFrames}, CRC失败 {_imuDriver.FailedCrcFrames}\r\n" +
                    $"原始数据前{((dump?.Length ?? 0))}字节: {hex}\r\n";
                Debug.WriteLine(msg);
                txt_Debug.AppendText(msg);
            }
        }

        // ---------- 串口控制 ----------
        private void btn_OpenPort_Click(object sender, EventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialEnabled = false;
                btn_OpenPort.Text = "打开串口";
                tssl_Serial.Text = "○ 串口关闭";
            }
            else
            {
                try
                {
                    if (cmb_Ports.SelectedItem == null || cmb_BaudRate.SelectedItem == null)
                    {
                        MessageBox.Show("请选择串口和波特率");
                        return;
                    }
                    string portStr = cmb_Ports.SelectedItem.ToString();
                    string portName = portStr.Split(' ')[0];
                    _serialPort.PortName = portName;
                    _serialPort.BaudRate = int.Parse(cmb_BaudRate.SelectedItem.ToString());
                    _serialPort.DataBits = 8;
                    _serialPort.Parity = Parity.None;
                    _serialPort.StopBits = StopBits.One;
                    _serialPort.Open();
                    _serialEnabled = true;
                    _serialSentCount = 0;
                    _serialSentFrames = 0;
                    _lastSerialFpsTime = DateTime.Now;
                    _serialCurrentFps = 0;
                    btn_OpenPort.Text = "关闭串口";
                    tssl_Serial.Text = $"● 串口已开 | {portName} @ {_serialPort.BaudRate} | 发送: 0 fps";
                }
                catch (Exception ex) { MessageBox.Show("串口打开失败: " + ex.Message); }
            }
        }

        // ---------- 测试发送 ----------
        private void Btn_TestSend_Click(object sender, EventArgs e)
        {
            if (!_serialEnabled || _serialPort == null || !_serialPort.IsOpen)
            {
                MessageBox.Show("请先打开串口");
                return;
            }

            byte id = 0;
            float angle = -90f;
            short val = (short)Math.Round(angle * 10);

            // 格式: [0xAA][len][count=1][id][angle_s16 LE][0xFF]
            int frameLen = 7; // 0xAA + len + count + id + angle(2B) + 0xFF
            byte[] buffer = new byte[frameLen];
            buffer[0] = 0xAA;
            buffer[1] = (byte)frameLen;
            buffer[2] = 1; // count = 1
            buffer[3] = id;
            buffer[4] = (byte)(val & 0xFF);
            buffer[5] = (byte)((val >> 8) & 0xFF);
            buffer[6] = 0xFF;

            _serialPort.Write(buffer, 0, buffer.Length);
            txt_Debug.AppendText($"测试发送: ID={id}, angle={angle}° (val={val}, 0x{(ushort)val:X4})\r\n");
        }

        // ---------- 角度批量发送 (1kHz 定时回调) ----------
        private void SendAngleBatch(object state)
        {
            if (!_serialEnabled || _serialPort == null || !_serialPort.IsOpen)
                return;

            var angles = _latestAnglesForSend;
            if (angles.Count == 0)
                return;

            try
            {
                // 格式: [0xAA:1B] [len:1B] [count:1B] [id1:1B][angle1_10x:2B LE] ... [0xFF:1B]
                int count = angles.Count;
                int frameLen = 4 + count * 3; // 帧头 + len + count + N组数据 + 包尾
                byte[] buffer = new byte[frameLen];
                buffer[0] = 0xAA;
                buffer[1] = (byte)frameLen;
                buffer[2] = (byte)count;

                for (int i = 0; i < count; i++)
                {
                    short val = (short)Math.Round(angles[i].angle * 10);
                    buffer[3 + i * 3] = angles[i].id;
                    buffer[3 + i * 3 + 1] = (byte)(val & 0xFF);
                    buffer[3 + i * 3 + 2] = (byte)((val >> 8) & 0xFF);
                }

                buffer[frameLen - 1] = 0xFF;

                _serialPort.Write(buffer, 0, buffer.Length);
                _serialSentCount += count;
                _serialSentFrames++;

                // 每秒统计一次发送帧率
                var now = DateTime.Now;
                double elapsed = (now - _lastSerialFpsTime).TotalSeconds;
                if (elapsed >= 1.0)
                {
                    _serialCurrentFps = _serialSentFrames / elapsed;
                    _serialSentFrames = 0;
                    _lastSerialFpsTime = now;

                    string fpsStr = _serialCurrentFps.ToString("F1");
                    BeginInvoke(new Action(() =>
                    {
                        tssl_Serial.Text = $"● 串口已开 | {_serialPort.PortName} @ {_serialPort.BaudRate} | 发送: {fpsStr} fps";
                    }));
                }
            }
            catch { }
        }

        // ---------- 相机其余控制 ----------
        private void button6_Click(object sender, EventArgs e)
        {
            isGrabbing = false;
            if (receiveThread != null)
                receiveThread.Join(500);
            device?.StreamGrabber.StopGrabbing();
            tssl_Grab.Text = "○ 未采集";
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (device == null)
                return;
            button6_Click(null, null);
            device.StreamGrabber.StartGrabbing();
            if (device.StreamGrabber.GetImageBuffer(1000, out IFrameOut frame) == MvError.MV_OK)
            {
                using (Mat bgrMat = ConvertToBgrMat(frame))
                {
                    Bitmap bmp = BitmapConverter.ToBitmap(bgrMat);
                    var old = pictureBox1.Image;
                    pictureBox1.Image = bmp;
                    old?.Dispose();
                }
                device.StreamGrabber.FreeImageBuffer(frame);
            }
            device.StreamGrabber.StopGrabbing();
        }

        private async void btn_SaveCalibImage_Click(object sender, EventArgs e)
        {
            if (device == null)
            { MessageBox.Show("请先打开相机"); return; }

            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CalibImages");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            bool wasGrabbing = isGrabbing;
            if (isGrabbing)
            {
                isGrabbing = false;
                if (receiveThread != null)
                    receiveThread.Join(500);
                device.StreamGrabber.StopGrabbing();
            }

            device.StreamGrabber.StartGrabbing();
            int ret = device.StreamGrabber.GetImageBuffer(1000, out IFrameOut frameOut);
            if (ret == MvError.MV_OK)
            {
                try
                {
                    using (Mat bgrMat = ConvertToBgrMat(frameOut))
                    {
                        Bitmap bmp = BitmapConverter.ToBitmap(bgrMat);
                        var old = pictureBox1.Image;
                        pictureBox1.Image = bmp;
                        old?.Dispose();

                        string fileName = Path.Combine(folderPath, $"calib_{calibImageCount:D2}.bmp");
                        Cv2.ImWrite(fileName, bgrMat);
                        txt_CalibResult.AppendText($"保存成功: calib_{calibImageCount:D2}.bmp\r\n");
                        calibImageCount++;
                    }
                }
                catch (Exception ex) { MessageBox.Show("保存图片异常:" + ex.Message); }
                finally { device.StreamGrabber.FreeImageBuffer(frameOut); }
            }
            device.StreamGrabber.StopGrabbing();

            txt_CalibResult.AppendText("暂停2秒后自动恢复...\r\n");
            await Task.Delay(2000);
            if (wasGrabbing)
                button7_Click(null, null);
        }

        private void btn_RunCalibration_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txt_BoardWidth.Text, out int boardW) || boardW < 2)
            { MessageBox.Show("棋盘格宽度无效"); return; }
            if (!int.TryParse(txt_BoardHeight.Text, out int boardH) || boardH < 2)
            { MessageBox.Show("棋盘格高度无效"); return; }
            if (!float.TryParse(txt_SquareSize.Text, out float squareSize) || squareSize <= 0)
            { MessageBox.Show("方格边长无效"); return; }

            CalibBoardWidth = boardW;
            CalibBoardHeight = boardH;
            CalibSquareSize = squareSize;

            txt_CalibResult.Clear();
            OpenCvSharp.Size patternSize = new OpenCvSharp.Size(boardW, boardH);
            List<Point3f[]> objectPointsList = new List<Point3f[]>();
            List<Point2f[]> imagePointsList = new List<Point2f[]>();
            List<string> fileNames = new List<string>();

            Point3f[] baseObjPts = new Point3f[CalibBoardWidth * CalibBoardHeight];
            for (int i = 0; i < CalibBoardHeight; i++)
                for (int j = 0; j < CalibBoardWidth; j++)
                    baseObjPts[i * CalibBoardWidth + j] = new Point3f(j * CalibSquareSize, i * CalibSquareSize, 0);

            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CalibImages");
            if (!Directory.Exists(folderPath))
                return;
            string[] files = Directory.GetFiles(folderPath, "*.bmp");

            txt_CalibResult.AppendText("正在提取角点...\r\n");
            OpenCvSharp.Size imgSize = new OpenCvSharp.Size();

            foreach (string file in files)
            {
                using (Mat img = Cv2.ImRead(file, ImreadModes.Grayscale))
                {
                    if (img.Empty())
                        continue;
                    imgSize = img.Size();
                    if (Cv2.FindChessboardCorners(img, patternSize, out Point2f[] corners))
                    {
                        Cv2.CornerSubPix(img, corners, new OpenCvSharp.Size(11, 11), new OpenCvSharp.Size(-1, -1),
                            new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, 30, 0.1));
                        imagePointsList.Add(corners);
                        objectPointsList.Add(baseObjPts);
                        fileNames.Add(Path.GetFileName(file));
                    }
                }
            }

            if (imagePointsList.Count < 5)
            { MessageBox.Show("有效标定图不足5张！"); return; }

            double[,] calibCameraMatrix = new double[3, 3];
            double[] calibDistCoeffs = new double[5];
            Vec3d[] rvecs, tvecs;

            double rms = Cv2.CalibrateCamera(objectPointsList, imagePointsList, imgSize, calibCameraMatrix, calibDistCoeffs, out rvecs, out tvecs);

            List<Point3f[]> filteredObj = new List<Point3f[]>();
            List<Point2f[]> filteredImg = new List<Point2f[]>();
            double threshold = 0.8;

            txt_CalibResult.AppendText($"初始RMS: {rms:F4}, 开始剔除坏图...\r\n");

            for (int i = 0; i < imagePointsList.Count; i++)
            {
                double[] rvecArr = new double[] { rvecs[i].Item0, rvecs[i].Item1, rvecs[i].Item2 };
                double[] tvecArr = new double[] { tvecs[i].Item0, tvecs[i].Item1, tvecs[i].Item2 };

                Point2f[] projected;
                double[,] jacobian;
                Cv2.ProjectPoints(objectPointsList[i], rvecArr, tvecArr, calibCameraMatrix, calibDistCoeffs, out projected, out jacobian);

                double err = 0;
                for (int j = 0; j < projected.Length; j++)
                {
                    double dx = projected[j].X - imagePointsList[i][j].X;
                    double dy = projected[j].Y - imagePointsList[i][j].Y;
                    err += Math.Sqrt(dx * dx + dy * dy);
                }
                err /= projected.Length;

                if (err < threshold)
                {
                    filteredObj.Add(objectPointsList[i]);
                    filteredImg.Add(imagePointsList[i]);
                }
                else
                {
                    txt_CalibResult.AppendText($"[剔除] {fileNames[i]} 误差:{err:F4}\r\n");
                }
            }

            if (filteredImg.Count < imagePointsList.Count && filteredImg.Count >= 5)
            {
                rms = Cv2.CalibrateCamera(filteredObj, filteredImg, imgSize, calibCameraMatrix, calibDistCoeffs, out rvecs, out tvecs);
                txt_CalibResult.AppendText($"优化后最终RMS: {rms:F4} (保留{filteredImg.Count}张)\r\n");
            }

            txt_CalibResult.AppendText($"\r\n焦距 fx:{calibCameraMatrix[0, 0]:F2} fy:{calibCameraMatrix[1, 1]:F2}\r\n");
            txt_CalibResult.AppendText($"光心 cx:{calibCameraMatrix[0, 2]:F2} cy:{calibCameraMatrix[1, 2]:F2}\r\n");
            txt_CalibResult.AppendText($"畸变 k1:{calibDistCoeffs[0]:F5} k2:{calibDistCoeffs[1]:F5}");

            if (rms < 1.0)
            {
                _config.Fx = calibCameraMatrix[0, 0];
                _config.Fy = calibCameraMatrix[1, 1];
                _config.Cx = calibCameraMatrix[0, 2];
                _config.Cy = calibCameraMatrix[1, 2];
                _config.K1 = calibDistCoeffs[0];
                _config.K2 = calibDistCoeffs[1];
                _config.P1 = calibDistCoeffs[2];
                _config.P2 = calibDistCoeffs[3];
                _config.K3 = calibDistCoeffs[4];
                _config.LastReprojectionError = rms;
                _config.LastCalibrated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                ConfigManager.Save(_config);
                ApplyCameraMatrix();

                txt_CalibResult.AppendText($"\r\n✔ RMS={rms:F3}<1.0，标定结果已保存至 config.json，相机内参已更新。\r\n");
            }
            else
            {
                txt_CalibResult.AppendText($"\r\n⚠ RMS={rms:F3}≥1.0，精度不足，标定结果未保存。请增加标定图或调整参数。\r\n");
            }
        }

        private void btn_SaveConfig_Click(object sender, EventArgs e)
        {
            _config.QRSize = SafeQRSize;
            _config.QREnabled = enableQR;

            if (device != null)
            {
                try
                {
                    device.Parameters.GetFloatValue("ExposureTime", out IFloatValue exp);
                    _config.ExposureTime = exp.CurValue;
                    device.Parameters.GetFloatValue("Gain", out IFloatValue gain);
                    _config.Gain = gain.CurValue;
                }
                catch { }
                _config.PixelFormat = comboBox2.Text;
            }
            else
            {
                if (double.TryParse(textBox1.Text, out double exp))
                    _config.ExposureTime = exp;
                if (double.TryParse(textBox2.Text, out double gain))
                    _config.Gain = gain;
                _config.PixelFormat = comboBox2.Text;
            }

            lock (_qrLock)
            {
                _config.QrIdAssignments = _qrDatabase.Values
                    .Where(q => q.AssignedID > 0)
                    .Select(q => new QrIdAssignment { Content = q.Content, AssignedId = q.AssignedID })
                    .ToList();
            }

            ConfigManager.Save(_config);
            txt_CalibResult.AppendText($"✔ 配置已保存至 config.json ({DateTime.Now:HH:mm:ss})\r\n");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (device == null)
                return;
            try
            {
                device.Parameters.GetFloatValue("ExposureTime", out IFloatValue exp);
                textBox1.Text = exp.CurValue.ToString("F0");

                device.Parameters.GetFloatValue("Gain", out IFloatValue gain);
                textBox2.Text = gain.CurValue.ToString("F1");

                device.Parameters.GetFloatValue("ResultingFrameRate", out IFloatValue fps);
                textBox3.Text = fps.CurValue.ToString("F1");

                int ret = device.Parameters.GetEnumValue("PixelFormat", out IEnumValue p);
                if (ret == MvError.MV_OK)
                {
                    comboBox2.Items.Clear();
                    Type enumType = p.GetType();
                    Type paramType = device.Parameters.GetType();
                    string currentValue = "";
                    try
                    {
                        var getEnumValStrMethod = paramType.GetMethod("GetEnumValueString");
                        if (getEnumValStrMethod != null)
                        {
                            object[] args = new object[] { "PixelFormat", "" };
                            getEnumValStrMethod.Invoke(device.Parameters, args);
                            currentValue = args[1] as string;
                        }
                        else
                        {
                            var strValProp = enumType.GetProperty("strValue");
                            if (strValProp != null)
                                currentValue = strValProp.GetValue(p) as string;
                        }
                    }
                    catch { }

                    if (!string.IsNullOrEmpty(currentValue))
                        comboBox2.Text = currentValue;

                    try
                    {
                        string[] supportPropNames = new string[] { "SupportValues", "SupportValue", "EnumEntrySymbolicStr" };
                        foreach (string propName in supportPropNames)
                        {
                            var prop = enumType.GetProperty(propName);
                            if (prop != null)
                            {
                                var values = prop.GetValue(p);
                                if (values is string[] strArray)
                                {
                                    comboBox2.Items.AddRange(strArray);
                                    break;
                                }
                                else if (values is long[] longArray)
                                {
                                    foreach (long index in longArray)
                                    {
                                        try
                                        {
                                            string strName = "";
                                            string[] methodNames = new string[] { "GetEnumEntrySymbolicStr", "GetEnumEntrySymbolic", "GetEnumValueString" };
                                            foreach (string methodName in methodNames)
                                            {
                                                var method = paramType.GetMethod(methodName);
                                                if (method != null && methodName != "GetEnumValueString")
                                                {
                                                    object[] args = new object[] { "PixelFormat", index, "" };
                                                    method.Invoke(device.Parameters, args);
                                                    strName = args[2] as string;
                                                    if (!string.IsNullOrEmpty(strName))
                                                    { comboBox2.Items.Add(strName); break; }
                                                }
                                            }
                                        }
                                        catch { }
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch { }

                    if (comboBox2.Items.Count == 0)
                    {
                        string[] commonFormats = new string[] { "Mono8", "Mono10", "BayerRG8", "BayerGR8", "RGB8Packed", "BGR8Packed" };
                        foreach (string format in commonFormats)
                        { try { comboBox2.Items.Add(format); } catch { } }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("获取参数失败: " + ex.Message); }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (device == null)
                return;
            try
            {
                device.Parameters.SetEnumValueByString("ExposureAuto", "Off");
                device.Parameters.SetEnumValueByString("GainAuto", "Off");
                device.Parameters.SetFloatValue("ExposureTime", float.Parse(textBox1.Text));
                device.Parameters.SetFloatValue("Gain", float.Parse(textBox2.Text));
                if (comboBox2.SelectedItem != null)
                {
                    device.Parameters.SetEnumValueByString("PixelFormat", comboBox2.SelectedItem.ToString());
                }
                MessageBox.Show("设置成功！");
            }
            catch (Exception ex) { MessageBox.Show("设置失败: " + ex.Message); }
        }

        private void chk_EnableQR_CheckedChanged(object sender, EventArgs e)
        {
            enableQR = chk_EnableQR.Checked;
            tssl_QR.Text = chk_EnableQR.Checked ? "QR: 开启" : "QR: 关闭";
        }
        private void txt_QRSize_TextChanged(object sender, EventArgs e) { if (float.TryParse(txt_QRSize.Text, out float newSize)) SafeQRSize = newSize; }
    }

    public class TrackedQR
    {
        public string Content { get; set; }
        public int AssignedID { get; set; }
        public int? LastUsedID { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public float PixelX { get; set; }
        public float PixelY { get; set; }

        public DateTime LastSeen { get; set; }
        public bool IsVisible { get; set; }

        public int MissedCount { get; set; }
        public float SmoothedX { get; set; }
        public float SmoothedY { get; set; }
        public float SmoothedZ { get; set; }
        public float VelocityX { get; set; }
        public float VelocityY { get; set; }
        public float VelocityZ { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public bool IsFirstFrame { get; set; } = true;

        public Point2f PrevCenter;
        public float BoxSize { get; set; }
        public Point2f[] CornerOffsets { get; set; }
        public int FlowFrames { get; set; }

        public override string ToString()
        {
            string statusStr = IsVisible ? "[在线]" : "[离线]";
            string idStr = AssignedID > 0 ? $"[ID: {AssignedID}]" : "[未分配ID]";

            return $"{statusStr} {idStr} {SmoothedX:F1},{SmoothedY:F1},{SmoothedZ:F1} | {Content}";
        }
    }
}