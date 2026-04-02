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
    private CpuCalculator cpu;
    private GpuCalculator gpu;
    private RamCalculator ram;
    private NetworkCalculator net;
    private DispatcherTimer timer;

    public MainWindow()
    {
        InitializeComponent();

        cpu = new CpuCalculator();
        gpu = new GpuCalculator();
        ram = new RamCalculator();
        net = new NetworkCalculator();

        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        float cpuusage = cpu.GetCpuUsage();
        float gpuusage = gpu.GetGpuUsage();
        float ramusage = ram.GetRamUsage();
        var netusage = net.GetNetworkUsage();

        CpuText.Text = $"CPU: {cpuusage}%";
        GpuText.Text = $"GPU: {gpuusage}%";
        RamText.Text = $"RAM: {ramusage}%";
        NetText.Text = $"Network In: {Math.Round(netusage.In/1000)}KB/s | Network Out: {Math.Round(netusage.Out/1000)}KB/s";
    }
}