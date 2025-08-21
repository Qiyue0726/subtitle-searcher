using System;
using System.Diagnostics;
using System.IO;
using System.Linq; // 确保添加此引用，用于 Take 方法
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Reflection;

namespace winform_webview_dotnet8
{
    // ... ScriptHostObject 类定义（保持不变）

    public partial class Form1 : Form
    {
        private ScriptHostObject _scriptHostObject;
        private string _tempAssetsPath;

        public Form1()
        {
            InitializeComponent();
            InitializeAsync();
            this.FormClosed += Form1_FormClosed;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_tempAssetsPath) && Directory.Exists(_tempAssetsPath))
            {
                try
                {
                    // 确保 WebView2 已经关闭，否则文件可能被锁定
                    // 这通常在 FormClosing 事件中处理更好，但在 FormClosed 中也可以尝试清理
                    webView21.Dispose(); // 强制释放 WebView2 资源
                    Directory.Delete(_tempAssetsPath, true);
                    Debug.WriteLine($"Temporary assets directory deleted: {_tempAssetsPath}");
                }
                catch (IOException ioEx)
                {
                    // 目录可能被其他进程锁定，或文件仍在被使用
                    Debug.WriteLine($"Error deleting temporary assets directory (IO): {ioEx.Message}");
                    // 可以在这里提示用户手动删除，或者在下次启动时尝试清理旧的临时目录
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error deleting temporary assets directory: {ex.Message}");
                }
            }
        }

        async void InitializeAsync()
        {
            await webView21.EnsureCoreWebView2Async(null);

            _scriptHostObject = new ScriptHostObject(this);
            webView21.CoreWebView2.AddHostObjectToScript("winFormProxy", _scriptHostObject);

            try
            {
                // 创建一个唯一的临时目录
                _tempAssetsPath = Path.Combine(Path.GetTempPath(), "WebView2Assets_" + Path.GetRandomFileName());
                Directory.CreateDirectory(_tempAssetsPath);
                Debug.WriteLine($"Created temporary assets directory: {_tempAssetsPath}");

                // 解压嵌入资源
                ExtractEmbeddedResourcesToDirectory(Assembly.GetExecutingAssembly(), "winform_webview_dotnet8.assets", _tempAssetsPath);

                // 设置虚拟主机映射
                webView21.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "app.local",
                    _tempAssetsPath,
                    CoreWebView2HostResourceAccessKind.Allow);

                // 导航到虚拟域名下的 index.html
                webView21.CoreWebView2.Navigate("https://app.local/index.html");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Web 资源加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"Web resource loading failed: {ex}");
                webView21.CoreWebView2.Navigate("about:blank");
            }
        }

        // 辅助方法：解压嵌入资源到指定目录
        private void ExtractEmbeddedResourcesToDirectory(Assembly assembly, string resourcePrefix, string targetDirectory)
        {
            // 完整的资源前缀，包括命名空间和文件夹名，后面跟一个点
            // 例如："winform_webview_dotnet8.assets."
            string fullResourcePrefix = resourcePrefix + ".";

            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                // 检查资源是否属于我们想解压的 assets 组
                if (!resourceName.StartsWith(fullResourcePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // 获取资源名称中表示文件路径的部分
                // 例如：从 "winform_webview_dotnet8.assets.layui.css.layui.css" 得到 "layui.css.layui.css"
                string relativeResourcePath = resourceName.Substring(fullResourcePrefix.Length);

                string targetFilePath;
                try
                {
                    // 将资源路径分解成各个部分
                    // 例如："layui.css.layui.css" -> ["layui", "css", "layui", "css"]
                    string[] parts = relativeResourcePath.Split('.');

                    string fileName;
                    string[] directoryParts;

                    // 判断是文件名还是目录结构
                    // 通常，嵌入资源的命名约定是：
                    // RootNamespace.Folder1.Folder2.FileName.Extension
                    // FileName 和 Extension 是最后两个点分隔的部分
                    if (parts.Length >= 2)
                    {
                        // 实际的文件名 (例如 "layui.css" from "layui.css.layui.css")
                        fileName = parts[parts.Length - 2] + "." + parts[parts.Length - 1];
                        // 目录部分 (例如 "layui", "css" from "layui.css.layui.css")
                        directoryParts = parts.Take(parts.Length - 2).ToArray();
                    }
                    else
                    {
                        // 处理没有扩展名或只有单个部分的资源名 (例如 "README" 或 "myimage")
                        fileName = parts[0];
                        directoryParts = Array.Empty<string>(); // 没有目录
                    }

                    // 组合目录部分形成相对路径 (例如 "layui\css")
                    string relativeDirectoryPath = Path.Combine(directoryParts);

                    // 组合目标目录、相对目录路径和文件名，形成最终文件路径
                    targetFilePath = Path.Combine(targetDirectory, relativeDirectoryPath, fileName);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error parsing resource path '{resourceName}': {ex.Message}");
                    continue; // 跳过此资源，避免崩溃
                }

                // 确保目标文件所在的目录存在
                string targetFileDirectory = Path.GetDirectoryName(targetFilePath);
                if (!string.IsNullOrEmpty(targetFileDirectory) && !Directory.Exists(targetFileDirectory))
                {
                    Directory.CreateDirectory(targetFileDirectory);
                    Debug.WriteLine($"Created directory: {targetFileDirectory}");
                }

                // 将资源流写入文件
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debug.WriteLine($"Failed to get stream for resource: {resourceName}");
                        continue;
                    }

                    try
                    {
                        using (FileStream fileStream = File.Create(targetFilePath))
                        {
                            stream.CopyTo(fileStream);
                        }
                        Debug.WriteLine($"Extracted: {resourceName} to {targetFilePath}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error writing file '{targetFilePath}': {ex.Message}");
                        // 可以在这里添加更详细的错误处理，例如删除部分解压的文件，或者通知用户
                    }
                }
            }
        }
    }
}
