using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace winform_webview2
{

    [System.Runtime.InteropServices.ComVisible(true)]
    public partial class Form1: Form
    {
        // 推荐将 HttpClient 设为静态以重用（避免端口耗尽）
        private static readonly HttpClient _httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
            CheckWebView2Installation();
            InitializeAsync();
        }

        async void CheckWebView2Installation()
        {
            try
            {
                await EnsureWebView2Async();
                //MessageBox.Show("WebView2 已成功安装。");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 未安装或初始化失败：{ex.Message}");
            }
        }

        async Task EnsureWebView2Async()
        {
            var webView2 = new WebView2();
            await webView2.EnsureCoreWebView2Async(null);
        }

        async void InitializeAsync()
        {
            // 初始化WebView2
            await webView21.EnsureCoreWebView2Async(null);
            webView21.CoreWebView2.AddHostObjectToScript("winFormProxy", this);

            // 加载网页
            //webView21.CoreWebView2.Navigate("https://www.baidu.com");

            // 获取项目输出目录
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // 构建网页文件的完整路径
            string htmlFilePath = Path.Combine(baseDirectory, "assets/index.html");

            // 加载本地网页
            webView21.CoreWebView2.Navigate(htmlFilePath);
        }

        [System.Runtime.InteropServices.ComVisible(true)]
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
                return $"Error: {ex.Message}";
            }
        }

        [System.Runtime.InteropServices.ComVisible(true)]
        public async Task<string> Download(string url, string name)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, name);
                File.WriteAllBytes(filePath, bytes);
                return filePath;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        [System.Runtime.InteropServices.ComVisible(true)]
        public void OpenFolder()
        {
            // 打开当前程序文件夹
            Process.Start("explorer.exe", Application.StartupPath);
        }
    }
}
