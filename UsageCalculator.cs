using System.Diagnostics;

GpuCalculator gpu = new GpuCalculator();
CpuCalculator cpu = new CpuCalculator();
RamCalculator ram = new RamCalculator();
NetworkCalculator net = new NetworkCalculator();

while (true)
{
    float cpuusage = cpu.GetCpuUsage();
    float gpuusage = gpu.GetGpuUsage();
    float ramusage = ram.GetRamUsage();
    var netusage = net.GetNetworkUsage();

    Console.Clear();
    Console.WriteLine("CPU:" + cpuusage + "%");
    Console.WriteLine("GPU:" + gpuusage + "%");
    Console.WriteLine("RAM:" + ramusage + "%");
    Console.WriteLine("Network In:" + (netusage.In/1000) + "KB/s | Network Out:" + (netusage.Out/1000) + "KB/s");

    Thread.Sleep(1000);
}

public class CpuCalculator
{
    private PerformanceCounter counter;
     
    public CpuCalculator()
    {
        counter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        counter.NextValue();

        Thread.Sleep(1000);
    }

    public float GetCpuUsage()
    {
        float usage;
        
        usage = counter.NextValue();

        return usage;
    }
}

public class GpuCalculator
{
    private PerformanceCounterCategory gpuCounterCat;
    private List<PerformanceCounter> gpuCountersList;

    public GpuCalculator()
    {
        gpuCounterCat = new PerformanceCounterCategory("GPU Engine");
        gpuCountersList = new List<PerformanceCounter>();

        string[] gpuInstanceNames = gpuCounterCat.GetInstanceNames();
    
        foreach (string name in gpuInstanceNames)
        {
            PerformanceCounter gpuCounter = new PerformanceCounter("GPU Engine", "Utilization Percentage", name);
            gpuCountersList.Add(gpuCounter);
        }

        foreach (PerformanceCounter counter in gpuCountersList)
        {
            counter.NextValue();
        }

        Thread.Sleep(1000);
    }

    public float GetGpuUsage()
    {
        float total = 0;

        foreach (PerformanceCounter counter in gpuCountersList)
        {
            total += counter.NextValue();
        }

        return total;
    }
}

public class RamCalculator
{
    private PerformanceCounter counter;

    public RamCalculator()
    {
        counter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
        counter.NextValue();

        Thread.Sleep(1000);
    }

    public float GetRamUsage()
    {
        float usage;

        usage = counter.NextValue();

        return usage;
    }
}

public class NetworkCalculator
{
    private PerformanceCounter incounter;
    private PerformanceCounter outcounter;

    public NetworkCalculator()
    {
        incounter = new PerformanceCounter("Network Adapter", "Bytes Received/sec", "realtek pcie 2.5gbe family controller");
        outcounter = new PerformanceCounter("Network Adapter", "Bytes Sent/sec", "realtek pcie 2.5gbe family controller");

        incounter.NextValue();
        outcounter.NextValue();

        Thread.Sleep(1000);
    }

    public (float In, float Out) GetNetworkUsage()
    {
        float usagein = incounter.NextValue();
        float usageout = outcounter.NextValue();

        return (usagein, usageout);
    }
}