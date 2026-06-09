namespace Desuwa;

internal static class Program
{
    private const string SingleInstanceMutexName = @"Global\Desuwa.SingleInstance";

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using var singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out var createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "Desuwa 已经在运行。请先退出系统托盘中的现有实例。",
                "Desuwa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var context = new TrayApplicationContext();
        Application.Run(context);
    }
}