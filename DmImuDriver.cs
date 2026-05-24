using System;
using System.IO.Ports;
using System.Threading;

namespace HIK_DEMON
{
    /// <summary>IMU 数据快照（线程安全，结构体避免引用拷贝问题）</summary>
    public struct ImuData
    {
        public float AccelX, AccelY, AccelZ;   // m/s²
        public float GyroX, GyroY, GyroZ;       // rad/s
        public float Roll, Pitch, Yaw;          // degrees
        public float Q0, Q1, Q2, Q3;            // quaternion
        public long FrameCount;
        public DateTime Timestamp;
    }

    /// <summary>达妙 DM-IMU-L1 串口通信驱动（USB 虚拟串口）</summary>
    public class DmImuDriver : IDisposable
    {
        // ---------- 串口 ----------
        private SerialPort _port;
        private Thread _readerThread;
        private volatile bool _running;
        private byte[] _buffer = new byte[4096];
        private int _bufLen;

        // ---------- 最新数据（线程安全） ----------
        private ImuData _latest;
        private readonly object _dataLock = new object();
        private long _frameCount;

        // ---------- 状态 ----------
        public bool IsConnected => _port != null && _port.IsOpen && _running;
        public string LastError { get; private set; }
        // ---------- 诊断 ----------
        public long TotalBytesRead { get; private set; }
        public long ValidFrames { get; private set; }
        public long FailedCrcFrames { get; private set; }
        public byte[] RawDump { get; private set; } // 最近收到的原始字节快照

        // ---------- CRC16 表 (CCITT 0x1021, init=0xFFFF) ----------
        private static readonly ushort[] CrcTable = new ushort[256]
        {
            0x0000,0x1021,0x2042,0x3063,0x4084,0x50A5,0x60C6,0x70E7,0x8108,0x9129,0xA14A,0xB16B,0xC18C,0xD1AD,0xE1CE,0xF1EF,
            0x1231,0x0210,0x3273,0x2252,0x52B5,0x4294,0x72F7,0x62D6,0x9339,0x8318,0xB37B,0xA35A,0xD3BD,0xC39C,0xF3FF,0xE3DE,
            0x2462,0x3443,0x0420,0x1401,0x64E6,0x74C7,0x44A4,0x5485,0xA56A,0xB54B,0x8528,0x9509,0xE5EE,0xF5CF,0xC5AC,0xD58D,
            0x3653,0x2672,0x1611,0x0630,0x76D7,0x66F6,0x5695,0x46B4,0xB75B,0xA77A,0x9719,0x8738,0xF7DF,0xE7FE,0xD79D,0xC7BC,
            0x48C4,0x58E5,0x6886,0x78A7,0x0840,0x1861,0x2802,0x3823,0xC9CC,0xD9ED,0xE98E,0xF9AF,0x8948,0x9969,0xA90A,0xB92B,
            0x5AF5,0x4AD4,0x7AB7,0x6A96,0x1A71,0x0A50,0x3A33,0x2A12,0xDBFD,0xCBDC,0xFBBF,0xEB9E,0x9B79,0x8B58,0xBB3B,0xAB1A,
            0x6CA6,0x7C87,0x4CE4,0x5CC5,0x2C22,0x3C03,0x0C60,0x1C41,0xEDAE,0xFD8F,0xCDEC,0xDDCD,0xAD2A,0xBD0B,0x8D68,0x9D49,
            0x7E97,0x6EB6,0x5ED5,0x4EF4,0x3E13,0x2E32,0x1E51,0x0E70,0xFF9F,0xEFBE,0xDFDD,0xCFFC,0xBF1B,0xAF3A,0x9F59,0x8F78,
            0x9188,0x81A9,0xB1CA,0xA1EB,0xD10C,0xC12D,0xF14E,0xE16F,0x1080,0x00A1,0x30C2,0x20E3,0x5004,0x4025,0x7046,0x6067,
            0x83B9,0x9398,0xA3FB,0xB3DA,0xC33D,0xD31C,0xE37F,0xF35E,0x02B1,0x1290,0x22F3,0x32D2,0x4235,0x5214,0x6277,0x7256,
            0xB5EA,0xA5CB,0x95A8,0x8589,0xF56E,0xE54F,0xD52C,0xC50D,0x34E2,0x24C3,0x14A0,0x0481,0x7466,0x6447,0x5424,0x4405,
            0xA7DB,0xB7FA,0x8799,0x97B8,0xE75F,0xF77E,0xC71D,0xD73C,0x26D3,0x36F2,0x0691,0x16B0,0x6657,0x7676,0x4615,0x5634,
            0xD94C,0xC96D,0xF90E,0xE92F,0x99C8,0x89E9,0xB98A,0xA9AB,0x5844,0x4865,0x7806,0x6827,0x18C0,0x08E1,0x3882,0x28A3,
            0xCB7D,0xDB5C,0xEB3F,0xFB1E,0x8BF9,0x9BD8,0xABBB,0xBB9A,0x4A75,0x5A54,0x6A37,0x7A16,0x0AF1,0x1AD0,0x2AB3,0x3A92,
            0xFD2E,0xED0F,0xDD6C,0xCD4D,0xBDAA,0xAD8B,0x9DE8,0x8DC9,0x7C26,0x6C07,0x5C64,0x4C45,0x3CA2,0x2C83,0x1CE0,0x0CC1,
            0xEF1F,0xFF3E,0xCF5D,0xDF7C,0xAF9B,0xBFBA,0x8FD9,0x9FF8,0x6E17,0x7E36,0x4E55,0x5E74,0x2E93,0x3EB2,0x0ED1,0x1EF0
        };

        private static ushort Crc16(byte[] data, int offset, int len)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < len; i++)
            {
                int index = ((crc >> 8) ^ data[offset + i]) & 0xFF;
                crc = (ushort)((crc << 8) ^ CrcTable[index]);
            }
            return crc;
        }

        // ========== 连接管理 ==========

        public bool Connect(string portName, int baudRate = 921600)
        {
            Disconnect();
            try
            {
                _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                {
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };
                _port.Open();
                _bufLen = 0;
                _frameCount = 0;
                _running = true;
                _readerThread = new Thread(ReaderLoop) { IsBackground = true, Name = "DM_IMU_Reader" };
                _readerThread.Start();
                // 重置诊断
                TotalBytesRead = 0;
                ValidFrames = 0;
                FailedCrcFrames = 0;
                RawDump = null;
                return true;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
                _port?.Close();
                _port = null;
                return false;
            }
        }

        public void Disconnect()
        {
            _running = false;
            if (_readerThread != null && _readerThread.IsAlive)
            {
                _readerThread.Join(500);
                _readerThread = null;
            }
            if (_port != null)
            {
                try { _port.Close(); } catch { }
                _port = null;
            }
        }

        // ========== 数据读取 ==========

        private void ReaderLoop()
        {
            while (_running && _port != null && _port.IsOpen)
            {
                try
                {
                    int available = _port.BytesToRead;
                    if (available > 0)
                    {
                        if (_bufLen + available > _buffer.Length)
                        {
                            // 缓冲溢出，丢弃旧数据
                            int keep = Math.Min(_bufLen, 256);
                            Array.Copy(_buffer, _bufLen - keep, _buffer, 0, keep);
                            _bufLen = keep;
                        }
                        int read = _port.Read(_buffer, _bufLen, Math.Min(available, _buffer.Length - _bufLen));
                        _bufLen += read;
                        TotalBytesRead += read;
                        // 保存原始数据快照（仅当还没保存过或发现新数据时）
                        if (read > 0 && RawDump == null)
                        {
                            RawDump = new byte[Math.Min(read, 64)];
                            Array.Copy(_buffer, _bufLen - read, RawDump, 0, RawDump.Length);
                        }
                        ParseFrames();
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
                catch (TimeoutException) { }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    Thread.Sleep(10);
                }
            }
        }

        private void ParseFrames()
        {
            int offset = 0;
            while (offset + 19 <= _bufLen)
            {
                // 查找帧头 0x55 0xAA
                if (_buffer[offset] != 0x55 || _buffer[offset + 1] != 0xAA)
                {
                    offset++;
                    continue;
                }

                // 判断是普通帧(19)还是扩展帧(23)
                bool isExt = _buffer[offset + 3] == 0x04;
                int frameLen = isExt ? 23 : 19;

                if (offset + frameLen > _bufLen)
                    break; // 数据不够，等下一包

                // 校验尾字节
                if (_buffer[offset + frameLen - 1] != 0x0A)
                {
                    offset++;
                    continue;
                }

                // CRC16 校验 — USB 本身有硬件校验，CRC 失败仅记录不丢弃
                ushort crcCalc = Crc16(_buffer, offset, frameLen - 3);
                ushort crcWire = (ushort)(_buffer[offset + frameLen - 3] | (_buffer[offset + frameLen - 2] << 8));
                if (crcCalc != crcWire)
                {
                    FailedCrcFrames++;
                }

                ValidFrames++;

                // 解析数据
                byte reg = _buffer[offset + 3];
                float f1 = BitConverter.ToSingle(_buffer, offset + 4);
                float f2 = BitConverter.ToSingle(_buffer, offset + 8);
                float f3 = BitConverter.ToSingle(_buffer, offset + 12);

                lock (_dataLock)
                {
                    _latest.Timestamp = DateTime.Now;
                    _latest.FrameCount = ++_frameCount;

                    switch (reg)
                    {
                        case 0x01: // 加速度
                            _latest.AccelX = f1;
                            _latest.AccelY = f2;
                            _latest.AccelZ = f3;
                            break;
                        case 0x02: // 陀螺仪
                            _latest.GyroX = f1;
                            _latest.GyroY = f2;
                            _latest.GyroZ = f3;
                            break;
                        case 0x03: // 欧拉角
                            _latest.Roll = f1;
                            _latest.Pitch = f2;
                            _latest.Yaw = f3;
                            break;
                        case 0x04: // 四元数 (扩展帧 23字节)
                            float f4 = BitConverter.ToSingle(_buffer, offset + 16);
                            _latest.Q0 = f1;
                            _latest.Q1 = f2;
                            _latest.Q2 = f3;
                            _latest.Q3 = f4;
                            break;
                    }
                }

                offset += frameLen;
            }

            // 剩余未处理的数据移到前面
            if (offset > 0)
            {
                int remaining = _bufLen - offset;
                if (remaining > 0)
                    Array.Copy(_buffer, offset, _buffer, 0, remaining);
                _bufLen = remaining;
            }
            else if (_bufLen > 3800)
            {
                // 没有找到有效帧头，丢弃大部分数据避免无限膨胀
                _bufLen = 0;
            }
        }

        // ========== 数据获取 ==========

        public ImuData GetLatestData()
        {
            lock (_dataLock) { return _latest; }
        }

        // ========== 控制指令 ==========

        /// <summary>发送原始指令（自动重发5次）</summary>
        private void SendCmd(byte[] cmd)
        {
            if (!IsConnected) return;
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    _port.Write(cmd, 0, cmd.Length);
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
        }

        public void EnterSettingMode()  => SendCmd(new byte[] { 0xAA, 0x06, 0x01, 0x0D });
        public void ExitSettingMode()   => SendCmd(new byte[] { 0xAA, 0x06, 0x00, 0x0D });
        public void Reboot()            => SendCmd(new byte[] { 0xAA, 0x00, 0x00, 0x0D });
        public void AccelCalibration()  => SendCmd(new byte[] { 0xAA, 0x01, 0x14, 0x0D });
        public void GyroCalibration()   => SendCmd(new byte[] { 0xAA, 0x01, 0x15, 0x0D });
        public void SaveParams()        => SendCmd(new byte[] { 0xAA, 0x03, 0x01, 0x0D });
        public void SetOutput1000Hz()   => SendCmd(new byte[] { 0xAA, 0x02, 0x01, 0x00, 0x0D });

        // ========== IDisposable ==========

        public void Dispose()
        {
            Disconnect();
        }
    }
}
