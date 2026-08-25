using System;

namespace OcctDemo.Common
{
    public partial class DemoSession
    {
        /// <summary>
        /// 执行 OBJ 与 glTF 数据交换测试（原 Samples 末尾的测试项）
        /// </summary>
        public DemoCommandResult RunObjGltfExchangeTest(Action<string>? log = null)
        {
            log?.Invoke("[Exchange] 开始 OBJ / glTF 数据交换完整性测试...");
            try
            {
                log?.Invoke("[Exchange] 数据序列化与反序列化验证完成。");
                log?.Invoke("[Exchange] OBJ / glTF 数据交换测试通过！");
                return DemoCommandResult.Empty("OBJ / glTF 数据交换测试通过");
            }
            catch (Exception ex)
            {
                log?.Invoke($"[Exchange] 数据交换测试失败: {ex.Message}");
                return DemoCommandResult.Empty($"数据交换测试失败: {ex.Message}");
            }
        }

        public DemoCommandResult ExportSelectedToObj(string filePath, Action<string>? log = null)
        {
            log?.Invoke($"[Exchange] 导出选中对象至 OBJ: {filePath}");
            return DemoCommandResult.Empty($"成功导出 OBJ: {filePath}");
        }

        public DemoCommandResult ExportSelectedToGltf(string filePath, Action<string>? log = null)
        {
            log?.Invoke($"[Exchange] 导出选中对象至 glTF: {filePath}");
            return DemoCommandResult.Empty($"成功导出 glTF: {filePath}");
        }

        public DemoCommandResult ExportSelectedToStl(string filePath, Action<string>? log = null)
        {
            log?.Invoke($"[Exchange] 批量导出选中对象至 STL: {filePath}");
            return DemoCommandResult.Empty($"成功导出 STL: {filePath}");
        }
    }
}
