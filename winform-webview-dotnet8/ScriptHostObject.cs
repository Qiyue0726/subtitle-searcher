using System.Diagnostics;
using System.Runtime.InteropServices;

namespace winform_webview_dotnet8
{
    // 必须标记为 ComVisible(true)
    // ClassInterfaceType.AutoDual 会自动为类生成一个双接口，方便COM调用
    // 对于更严格的API设计，可以考虑 ClassInterfaceType.None 并显式定义接口
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class ScriptHostObject
    {
        // 推荐将 HttpClient 设为静态以重用（避免端口耗尽）
        private static readonly HttpClient _httpClient = new HttpClient();
        private Form _parentForm; // 用于持有 Form1 的引用，以便在需要时操作UI

        // 构造函数，传入 Form1 的引用
        public ScriptHostObject(Form parentForm)
        {
            _parentForm = parentForm;
        }

        [ComVisible(true)] // 即使类已ComVisible，方法最好也显式标记
        public async Task<string> MakeApiRequest(string apiUrl)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                // 在实际应用中，你可能需要更详细的日志记录
                Console.WriteLine($"Error in MakeApiRequest: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        [ComVisible(true)]
        public async Task<string> Download(string url, string name)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                // 使用 Application.StartupPath 确保路径在应用程序启动目录
                string filePath = Path.Combine(Application.StartupPath, name);
                File.WriteAllBytes(filePath, bytes);
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Download: {ex.Message}");
                return $"Error: {ex.Message}";
            }
        }

        [ComVisible(true)]
        public void OpenFolder()
        {
            try
            {
                // Process.Start 可以在任何线程调用，但如果涉及到UI交互，需要Invoke
                Process.Start("explorer.exe", Application.StartupPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OpenFolder: {ex.Message}");
                // 可以在这里通过 _parentForm.Invoke 显示错误信息给用户
            }
        }

        // 示例：如果你想从JavaScript调用方法来更新Form的UI
        [ComVisible(true)]
        public void UpdateFormTitle(string newTitle)
        {
            if (_parentForm != null && _parentForm.InvokeRequired)
            {
                _parentForm.Invoke(new Action(() => _parentForm.Text = newTitle));
            }
            else if (_parentForm != null)
            {
                _parentForm.Text = newTitle;
            }
        }
    }
}
