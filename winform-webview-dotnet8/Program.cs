namespace winform_webview_dotnet8
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            // 启用视觉样式，这是让控件看起来现代化的关键！
            Application.EnableVisualStyles();

            // 设置兼容的文本渲染，对于自定义绘制很重要。
            Application.SetCompatibleTextRenderingDefault(false);

            // 设置高 DPI 支持 (非常重要，防止在高分屏上界面模糊)
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.Run(new Form1());
        }
    }
}