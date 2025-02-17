using System.Diagnostics;
using System.Net;
using Newtonsoft.Json.Linq;

namespace winform
{
    public partial class Form1 : Form
    {
        // �Ƽ��� HttpClient ��Ϊ��̬�����ã�����˿ںľ���
        private static readonly HttpClient _httpClient = new HttpClient();

        public Form1()
        {
            InitializeComponent();
        }

        // �첽 GET ����
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
                MessageBox.Show($"����ʧ��: {ex.Message}");
                return string.Empty;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("�������Ӱ��");
                return;
            }
            string json = await GetSubs(textBox1.Text);
            dataGridView1.Rows.Clear();  // ��ձ��
            // ���л� JSON �ַ���
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
                    dataGridView1.Rows.Add(sub["name"]?.ToString() ?? "δ֪", sub["languages"]?.ToString() ?? "δ֪", "����", sub["url"]?.ToString() ?? "δ֪");
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
            // ������ذ�ť
            if (e.ColumnIndex == 2)
            {
                if (dataGridView1.Rows[e.RowIndex].Cells[2].Value == "����")
                {
                    string url = dataGridView1.Rows[e.RowIndex].Cells[3].Value?.ToString();
                    if (string.IsNullOrWhiteSpace(url))
                    {
                        MessageBox.Show("��������Ϊ��");
                        return;
                    }
                    // ������Ļ
                    using (WebClient client = new WebClient())
                    {
                        string fileName = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                        client.DownloadFile(url, fileName);
                        // �Ѱ�ť���������
                        dataGridView1.Rows[e.RowIndex].Cells[2].Value = "��ʾ";
                    }
                }
                else if (dataGridView1.Rows[e.RowIndex].Cells[2].Value == "��ʾ")
                {
                    // �򿪵�ǰ�����ļ���
                    Process.Start("explorer.exe", Application.StartupPath);
                }
            }
        }
    }
}
