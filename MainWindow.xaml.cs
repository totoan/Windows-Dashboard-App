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

    private DispatcherTimer usageTimer;
    private DispatcherTimer youtubeTimer;

    public MainWindow()
    {
        InitializeComponent();

        auth = new AuthService();

        cpu = new CpuCalculator();
        gpu = new GpuCalculator();
        ram = new RamCalculator();
        net = new NetworkCalculator();
        stor = new StorageCalculator();

        usageTimer = new DispatcherTimer();
        youtubeTimer = new DispatcherTimer();
        
        usageTimer.Interval = TimeSpan.FromSeconds(1);
        usageTimer.Tick += UsageTimer_Tick;
        usageTimer.Start();

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await auth.InitializeAsync();

        youtubeTimer.Interval = TimeSpan.FromMinutes(15);
        youtubeTimer.Tick += async (s, e) => await YouTube_Service();
        youtubeTimer.Start();

        await YouTube_Service();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void RefreshYTButton_Click(object sender, RoutedEventArgs e)
    {
        await YouTube_Service();
    }

    private async Task YouTube_Service()
    {
        youtubeTimer.Stop();

        try
        {
            if (auth?.AccessToken != null)
            {
                var api = new YouTubeService(auth.AccessToken);
                List<SubscriptionVideo> videos = await api.GetUploadsAsync();

                if (videos == null)
                {
                    MessageBox.Show("Videos was null");  
                    return;      
                }

                if (videos.Count == 0)
                {
                    MessageBox.Show("Videos list was empty");
                    return;
                }

                if (videos.Count > 0)
                {
                    YouTubeUploadsList.Children.Clear();
                }

                foreach (var vid in videos)
                {
                    StackPanel itemPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                    };

                    Image thumbnail = new Image
                    {
                        Width = 120,
                        Height = 68,
                    };

                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.UriSource = new Uri(vid.ThumbnailUrl);
                    bi.EndInit();

                    thumbnail.Source = bi;

                    StackPanel textPanel = new StackPanel
                    {
                        Orientation = Orientation.Vertical
                    };

                    TextBlock titleBlock = new TextBlock
                    {
                        Text = vid.Title,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = Brushes.White
                    };

                    TextBlock channelBlock = new TextBlock
                    {
                        Text = vid.ChannelTitle,
                        Foreground = Brushes.White
                    };

                    textPanel.Children.Add(titleBlock);
                    textPanel.Children.Add(channelBlock);

                    itemPanel.Children.Add(thumbnail);
                    itemPanel.Children.Add(textPanel);

                    YouTubeUploadsList.Children.Add(itemPanel);
                }
            }
            
            else
            {
                MessageBox.Show("[Exception] auth.AccessToken returned null.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
        youtubeTimer.Start();
    }

    private void UsageTimer_Tick(object? sender, EventArgs e)
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