using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace HIK_DEMON
{
    public class WeChatQRCodeDecoder : IDisposable
    {
        private OpenCvSharp.WeChatQRCode _wechat;
        private bool _isWeChatEnabled = false;
        private DateTime _lastCrashTime = DateTime.MinValue;
        private string _debugFolder;

        public WeChatQRCodeDecoder()
        {
            _debugFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DebugImages");
            if (!Directory.Exists(_debugFolder)) Directory.CreateDirectory(_debugFolder);

            try
            {
                string modelPath = @"H:\HIK\HIK_DEMON\model";
                string d_proto = Path.Combine(modelPath, "detect.prototxt");
                string d_caffe = Path.Combine(modelPath, "detect.caffemodel");
                string s_proto = Path.Combine(modelPath, "sr.prototxt");
                string s_caffe = Path.Combine(modelPath, "sr.caffemodel");

                if (File.Exists(d_proto) && File.Exists(d_caffe) && File.Exists(s_proto) && File.Exists(s_caffe))
                {
                    _wechat = OpenCvSharp.WeChatQRCode.Create(d_proto, d_caffe, s_proto, s_caffe);
                    _isWeChatEnabled = true;
                    Log("[初始化] 微信识别引擎加载成功。");
                }
                else
                {
                    Log("[初始化] 失败：模型文件路径不正确。");
                }
            }
            catch (Exception ex)
            {
                _isWeChatEnabled = false;
                Log($"[初始化] 异常：{ex.Message}");
            }
        }

        public (string[] texts, Point2f[][] points) DetectAndDecode(Mat image, bool skipPreprocess = false)
        {
            if (image == null || image.Empty() || !_isWeChatEnabled || _wechat == null)
                return (null, null);

            // 崩溃冷却保护
            if ((DateTime.Now - _lastCrashTime).TotalMilliseconds < 50) return (null, null);

            Stopwatch totalSw = Stopwatch.StartNew();
            Log($"--- 开始识别 [帧尺寸: {image.Width}x{image.Height}] ---");

            try
            {
                Mat[] rects;
                string[] texts;

                if (skipPreprocess)
                {
                    // 跳过内部预处理，直接交给 WeChatQR（调用方已自行处理图像）
                    Stopwatch aiSw = Stopwatch.StartNew();
                    _wechat.DetectAndDecode(image, out rects, out texts);
                    aiSw.Stop();
                    Log($"[AI推理-直通] 耗时: {aiSw.ElapsedMilliseconds}ms");
                }
                else
                {
                    // 1. 图像预处理：LAB 空间对比度增强（保留彩色信息）
                    using (Mat processed = PreprocessForColorQR(image))
                    {
                        // 调试：每隔一段时间保存一张处理后的图（可选）
                        if (DateTime.Now.Second % 5 == 0)
                            Cv2.ImWrite(Path.Combine(_debugFolder, "last_processed.jpg"), processed);

                        Stopwatch aiSw = Stopwatch.StartNew();
                        _wechat.DetectAndDecode(processed, out rects, out texts);
                        aiSw.Stop();

                    Log($"[AI推理] 耗时: {aiSw.ElapsedMilliseconds}ms");

                    if (texts != null && texts.Length > 0)
                    {
                        Log($"[成功] 检测到 {texts.Length} 个码：{string.Join(" | ", texts)}");
                        Point2f[][] allPoints = new Point2f[texts.Length][];
                        for (int i = 0; i < texts.Length; i++)
                        {
                            allPoints[i] = new Point2f[4];
                            for (int j = 0; j < 4; j++)
                                allPoints[i][j] = new Point2f(rects[i].At<float>(j, 0), rects[i].At<float>(j, 1));
                            rects[i].Dispose();
                        }
                        return (texts, allPoints);
                    }
                    else
                    {
                        Log("[失败] 未发现二维码。");
                    }
                }
            }   // close else
            }   // close try
            catch (OpenCVException cvEx)
            {
                _lastCrashTime = DateTime.Now;
                Log($"[底层异常] {cvEx.Message}");
            }
            catch (Exception ex)
            {
                Log($"[程序异常] {ex.Message}");
            }
            finally
            {
                totalSw.Stop();
                if (totalSw.ElapsedMilliseconds > 1) Log($"--- 识别流程结束，总耗时: {totalSw.ElapsedMilliseconds}ms ---");
            }

            return (null, null);
        }

        /// <summary>
        /// 针对彩色二维码设计的预处理：
        /// 保持颜色不丢失，但极大增强黑白/明暗对比度
        /// </summary>
        private Mat PreprocessForColorQR(Mat input)
        {
            Mat lab = new Mat();
            Mat enhanced = new Mat();

            // 1. 转到 LAB 空间
            Cv2.CvtColor(input, lab, ColorConversionCodes.BGR2Lab);

            // 2. 分离通道 (L: 亮度, a: 红绿, b: 蓝黄)
            Mat[] channels = Cv2.Split(lab);

            // 3. 对 L 通道进行自适应均衡化 (CLAHE) — 增强对比度帮助晃动时保持识别
            using (var clahe = Cv2.CreateCLAHE(6.0, new Size(5, 5)))
            {
                clahe.Apply(channels[0], channels[0]);
            }

            // 4. 合并通道并转回 BGR
            Cv2.Merge(channels, lab);
            Cv2.CvtColor(lab, enhanced, ColorConversionCodes.Lab2BGR);

            // 5. 强锐化，让二维码边缘在模糊时更清晰
            Mat sharpened = new Mat();
            Cv2.GaussianBlur(enhanced, sharpened, new Size(0, 0), 1.5);
            Cv2.AddWeighted(enhanced, 2.0, sharpened, -1.0, 0, sharpened);

            // 释放中间内存
            lab.Dispose();
            foreach (var c in channels) c.Dispose();
            enhanced.Dispose();

            return sharpened;
        }

        private void Log(string msg)
        {
            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.WriteLine($"[{time}][WeChatQR] {msg}");
        }

        public void Dispose()
        {
            if (_isWeChatEnabled && _wechat != null)
            {
                try { _wechat.Dispose(); } catch { }
            }
        }
    }
}