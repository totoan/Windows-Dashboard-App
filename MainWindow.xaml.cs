using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DashboardApp;
public partial class MainWindow : Window
{
    // Web Services
    private AuthService auth;

    // Calculators
    private CpuCalculator cpu;
    private GpuCalculator gpu;
    private RamCalculator ram;
    private NetworkCalculator net;
    private StorageCalculator stor;

    private DispatcherTimer timer;

    public MainWindow()
    {
        InitializeComponent();

        auth = new AuthService();

        cpu = new CpuCalculator();
        gpu = new GpuCalculator();
        ram = new RamCalculator();
        net = new NetworkCalculator();
        stor = new StorageCalculator();

        timer = new DispatcherTimer();
        
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        timer.Start();

        _ = YouTube_Service();
    }

    private async Task YouTube_Service()
    {
        await auth.InitializeAsync();

        if (auth?.AccessToken != null)
        {
            var api = new YouTubeService(auth.AccessToken);
            List<SubscriptionVideo> videos = await api.GetUploadsAsync();
            
            string text = "";

            foreach (var vid in videos)
            {
                text += $"{vid.Title}\n{vid.ChannelTitle}\n{vid.ThumbnailUrl}\n\n";
            }

            MessageBox.Show(text);
        }
        else
        {
            MessageBox.Show("[Exception] auth.AccessToken returned null.");
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        float cpuusage = cpu.GetCpuUsage();
        float gpuusage = gpu.GetGpuUsage();
        float ramusage = ram.GetRamUsage();
        var netusage = net.GetNetworkUsage();
        var storusage = stor.GetStorageUsage();

        string storText = "";
        
        foreach (var d in storusage)
        {
            string name = d.Name;
            float inuse = d.InUse;
            float size = d.Size;

            storText += $"{d.Name} {Math.Round(inuse/1024/1024/1024, 1)}GB / {Math.Round(size/1024/1024/1024, 1)}GB\n";
        }

        CpuUsage.Text = $"{Math.Round(cpuusage, 1)}%";
        GpuUsage.Text = $"{Math.Round(gpuusage, 1)}%";
        RamUsage.Text = $"{Math.Round(ramusage, 1)}%";
        NetUsage.Text = $"↑ {Math.Round(netusage.Out/1000)}KB/s\n↓ {Math.Round(netusage.In/1000)}KB/s";
        StorUsage.Text = storText;
    }
}