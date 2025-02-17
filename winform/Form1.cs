using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;

namespace winform
{
    public partial class Form1 : Form
    {
        // 推荐将 HttpClient 设为静态以重用（避免端口耗尽）
        private static readonly HttpClient _httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        // 异步 GET 请求
        private async Task<string> GetSubs(string name)
        {
            try
            {
                string url = "https://api-shoulei-ssl.xunlei.com/oracle/subtitle?name=" + name;
                string response = await _httpClient.GetStringAsync(url);
                return response;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"请求失败: {ex.Message}");
                return string.Empty;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("请输入电影名");
                return;
            }
            string json = await GetSubs(textBox1.Text);
            dataGridView1.Rows.Clear();  // 清空表格
            // 序列化 JSON 字符串
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            JObject subs = JObject.Parse(json);
            if (subs?["data"] is JArray dataArray)
            {
                List<string> subsList = new List<string>();
                foreach (var sub in dataArray)
                {
                    dataGridView1.Rows.Add(sub["name"]?.ToString() ?? "未知", sub["languages"]?.ToString() ?? "未知", "下载", sub["url"]?.ToString() ?? "未知");
                }

            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 点击下载按钮
            if (e.ColumnIndex == 2)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[2].Value == "下载")
                {
                    string url = dataGridView1.Rows[e.RowIndex].Cells[3].Value?.ToString();
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        MessageBox.Show("下载链接为空");
                        return;
                    }
                    // 下载字幕
                    using (WebClient client = new WebClient())
                    {
                        string fileName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                        client.DownloadFile(url, fileName);
                        // 把按钮变成已下载
                        dataGridView1.Rows[e.RowIndex].Cells[2].Value = "显示";
                    }
                }
                else if (dataGridView1.Rows[e.RowIndex].Cells[2].Value == "显示")
                {
                    // 打开当前程序文件夹
                    Process.Start("explorer.exe", Application.StartupPath);
                }
            }
        }
    }
}
