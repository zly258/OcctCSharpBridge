using System;

namespace OcctDemo.Common
{
    public enum ViewCubeCornerPosition
    {
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }

    /// <summary>
    /// ViewCube 导航立方体综合配置参数
    /// </summary>
    public class ViewCubeOptions
    {
        public bool Visible { get; set; } = true;
        public ViewCubeCornerPosition Position { get; set; } = ViewCubeCornerPosition.TopRight;
        public double Size { get; set; } = 80.0;
        public int OffsetX { get; set; } = 15;
        public int OffsetY { get; set; } = 15;

        // 颜色配置 (ARGB/Hex)
        public int BoxColorArgb { get; set; } = unchecked((int)0xFFDCDCDC);    // 主体面颜色
        public int FacetColorArgb { get; set; } = unchecked((int)0xFF007ACC);  // 高亮选中面颜色
        public int TextColorArgb { get; set; } = unchecked((int)0xFF282828);   // 文字颜色

        // 样式与字体
        public string FontName { get; set; } = "Segoe UI";
        public double FontHeight { get; set; } = 12.0;
        public double CornerRadius { get; set; } = 4.0;
        public double EdgeWidth { get; set; } = 1.0;

        public ViewCubeOptions Clone()
        {
            return (ViewCubeOptions)this.MemberwiseClone();
        }
    }
}
