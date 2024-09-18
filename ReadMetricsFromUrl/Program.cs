// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;

var metricsUrl = "http://localhost:2020/base/metrics"; // Replace with the actual URL
try
{
    // Fetch metrics from the server
    var metricsData = await FetchMetricsDataAsync(metricsUrl);

    // Parse and output metrics data
    ParseMetricsData(metricsData);
}
catch (Exception ex)
{
    Console.WriteLine($"Error fetching or parsing metrics: {ex.Message}");
}

return;

static async Task<string> FetchMetricsDataAsync(string url)
{
    using var client = new HttpClient();
    var response = await client.GetAsync(url);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}

static void ParseMetricsData(string metricsData)
{
    // Regular expressions to capture different metrics
    var memoryUsageRegex = new Regex(@"process_memory_usage_bytes.*? (\d+)");
    var virtualMemoryRegex = new Regex(@"process_memory_virtual_bytes.*? (\d+)");
    var cpuTimeUserRegex = new Regex(@"process_cpu_time_seconds.*?state=""user"".*? (\d+\.\d+)");
    var cpuTimeSystemRegex = new Regex(@"process_cpu_time_seconds.*?state=""system"".*? (\d+\.\d+)");
    var cpuCountRegex = new Regex(@"process_cpu_count.*? (\d+)");
    var threadCountRegex = new Regex(@"process_threads.*? (\d+)");
    var gcCollectionsRegex = new Regex(@"process_runtime_dotnet_gc_collections_count.*?generation=""gen(\d+)""} (\d+)");
    var gcHeapSizeRegex = new Regex(@"process_runtime_dotnet_gc_heap_size_bytes.*?generation=""gen(\d+)""} (\d+)");
    var jitMethodsCompiledRegex = new Regex(@"process_runtime_dotnet_jit_methods_compiled_count.*? (\d+)");
    var jitCompilationTimeRegex = new Regex(@"process_runtime_dotnet_jit_compilation_time_nanoseconds.*? (\d+)");
    var threadPoolThreadsCountRegex = new Regex(@"process_runtime_dotnet_thread_pool_threads_count.*? (\d+)");
    var httpRequestDurationSumRegex = new Regex(@"http_server_request_duration_seconds_sum.*? (\d+\.\d+)");
    var httpRequestDurationCountRegex = new Regex(@"http_server_request_duration_seconds_count.*? (\d+)");

    // Match memory usage
    var memoryMatch = memoryUsageRegex.Match(metricsData);
    if (memoryMatch.Success)
    {
        var memoryUsageBytes = long.Parse(memoryMatch.Groups[1].Value);
        Console.WriteLine($"Memory Usage (Bytes): {memoryUsageBytes}");
    }

    // Match virtual memory usage
    var virtualMemoryMatch = virtualMemoryRegex.Match(metricsData);
    if (virtualMemoryMatch.Success)
    {
        var virtualMemoryBytes = long.Parse(virtualMemoryMatch.Groups[1].Value);
        Console.WriteLine($"Virtual Memory Usage (Bytes): {virtualMemoryBytes}");
    }

    // Match CPU time (user)
    var cpuTimeUserMatch = cpuTimeUserRegex.Match(metricsData);
    if (cpuTimeUserMatch.Success)
    {
        var cpuTimeUser = double.Parse(cpuTimeUserMatch.Groups[1].Value);
        Console.WriteLine($"CPU Time (User): {cpuTimeUser} seconds");
    }

    // Match CPU time (system)
    var cpuTimeSystemMatch = cpuTimeSystemRegex.Match(metricsData);
    if (cpuTimeSystemMatch.Success)
    {
        var cpuTimeSystem = double.Parse(cpuTimeSystemMatch.Groups[1].Value);
        Console.WriteLine($"CPU Time (System): {cpuTimeSystem} seconds");
    }

    // Match CPU count
    var cpuCountMatch = cpuCountRegex.Match(metricsData);
    if (cpuCountMatch.Success)
    {
        var cpuCount = int.Parse(cpuCountMatch.Groups[1].Value);
        Console.WriteLine($"CPU Count: {cpuCount}");
    }

    // Match thread count
    var threadCountMatch = threadCountRegex.Match(metricsData);
    if (threadCountMatch.Success)
    {
        var threadCount = int.Parse(threadCountMatch.Groups[1].Value);
        Console.WriteLine($"Thread Count: {threadCount}");
    }

    // Match GC collections
    foreach (Match gcMatch in gcCollectionsRegex.Matches(metricsData))
    {
        var generation = gcMatch.Groups[1].Value;
        var count = int.Parse(gcMatch.Groups[2].Value);
        Console.WriteLine($"GC Collections (Gen {generation}): {count}");
    }

    // Match GC heap sizes
    foreach (Match heapSizeMatch in gcHeapSizeRegex.Matches(metricsData))
    {
        var generation = heapSizeMatch.Groups[1].Value;
        var size = long.Parse(heapSizeMatch.Groups[2].Value);
        Console.WriteLine($"GC Heap Size (Gen {generation}): {size} bytes");
    }

    // Match JIT compiled methods
    var jitMethodsCompiledMatch = jitMethodsCompiledRegex.Match(metricsData);
    if (jitMethodsCompiledMatch.Success)
    {
        var methodsCompiled = int.Parse(jitMethodsCompiledMatch.Groups[1].Value);
        Console.WriteLine($"JIT Methods Compiled: {methodsCompiled}");
    }

    // Match JIT compilation time
    var jitCompilationTimeMatch = jitCompilationTimeRegex.Match(metricsData);
    if (jitCompilationTimeMatch.Success)
    {
        var jitTimeNanoseconds = long.Parse(jitCompilationTimeMatch.Groups[1].Value);
        Console.WriteLine($"JIT Compilation Time: {jitTimeNanoseconds} nanoseconds");
    }

    // Match thread pool threads count
    var threadPoolThreadsCountMatch = threadPoolThreadsCountRegex.Match(metricsData);
    if (threadPoolThreadsCountMatch.Success)
    {
        var threadPoolThreads = int.Parse(threadPoolThreadsCountMatch.Groups[1].Value);
        Console.WriteLine($"Thread Pool Threads Count: {threadPoolThreads}");
    }

    // Match HTTP request duration (sum)
    var httpRequestDurationSumMatch = httpRequestDurationSumRegex.Match(metricsData);
    if (httpRequestDurationSumMatch.Success)
    {
        var durationSum = double.Parse(httpRequestDurationSumMatch.Groups[1].Value);
        Console.WriteLine($"HTTP Request Duration Sum: {durationSum} seconds");
    }

    // Match HTTP request count
    var httpRequestDurationCountMatch = httpRequestDurationCountRegex.Match(metricsData);
    if (httpRequestDurationCountMatch.Success)
    {
        var requestCount = int.Parse(httpRequestDurationCountMatch.Groups[1].Value);
        Console.WriteLine($"HTTP Request Count: {requestCount}");
    }
}