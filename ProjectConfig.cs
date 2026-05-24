using System;
using System.Collections.Generic;

namespace HIK_DEMON
{
    /// <summary>
    /// 项目配置模型（config.json）
    /// </summary>
    public class ProjectConfig
    {
        // ========== 标定板 ==========
        public int BoardWidth { get; set; }
        public int BoardHeight { get; set; }
        public float SquareSize { get; set; }

        // ========== 相机内参 ==========
        public double Fx { get; set; }
        public double Fy { get; set; }
        public double Cx { get; set; }
        public double Cy { get; set; }
        public double K1 { get; set; }
        public double K2 { get; set; }
        public double P1 { get; set; }
        public double P2 { get; set; }
        public double K3 { get; set; }

        // ========== 标定质量 ==========
        public double LastReprojectionError { get; set; }
        public string LastCalibrated { get; set; }

        // ========== QR ==========
        public float QRSize { get; set; }
        public bool QREnabled { get; set; }

        // ========== 相机运行时参数 ==========
        public double ExposureTime { get; set; }
        public double Gain { get; set; }
        public string PixelFormat { get; set; }

        // ========== QR ID 分配持久化 ==========
        public List<QrIdAssignment> QrIdAssignments { get; set; }

        // ========== 颜色修正 ==========
        public bool SwapColorChannels { get; set; }

        /// <summary>
        /// 确保所有字段有合理的默认值（防止 JSON 缺失字段时空值）
        /// </summary>
        public void EnsureDefaults()
        {
            if (BoardWidth < 2) BoardWidth = 9;
            if (BoardHeight < 2) BoardHeight = 6;
            if (SquareSize <= 0) SquareSize = 23.7f;
            if (Fx <= 0) Fx = 1780.69;
            if (Fy <= 0) Fy = 1778.42;
            if (Cx <= 0) Cx = 744.87;
            if (Cy <= 0) Cy = 495.18;
            if (QRSize <= 0) QRSize = 20f;
            if (string.IsNullOrEmpty(LastCalibrated)) LastCalibrated = "从未";
            if (string.IsNullOrEmpty(PixelFormat)) PixelFormat = "";
            if (QrIdAssignments == null) QrIdAssignments = new List<QrIdAssignment>();
        }
    }

    /// <summary>
    /// QR 码内容 → 分配 ID 的持久化映射
    /// </summary>
    public class QrIdAssignment
    {
        public string Content { get; set; }
        public int AssignedId { get; set; }
    }
}
