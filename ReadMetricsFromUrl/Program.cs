// See https://aka.ms/new-console-template for more information

using System.Text.RegularExpressions;
using StackExchange.Redis;

var coreMetricsUrl = "http://192.168.108.150:2020/base/metrics";
var rabbitMqMetricsUrl = "http://localhost:15692/metrics";
var cadvisorMetricsUrl = "http://localhost:8080/metrics";

try
{
    var aspMetrics = new Dictionary<string, string>
    {
        { "MemoryUsage", @"process_memory_usage_bytes.*? (\d+)" },
        { "VirtualMemoryUsage", @"process_memory_virtual_bytes.*? (\d+)" },
        { "CpuTimeUser", @"process_cpu_time_seconds.*?state=""user"".*? (\d+\.\d+)" },
        { "CpuTimeSystem", @"process_cpu_time_seconds.*?state=""system"".*? (\d+\.\d+)" },
        { "CpuCount", @"process_cpu_count.*? (\d+)" },
        { "ThreadCount", @"process_threads.*? (\d+)" },
        { "GCCollections", @"process_runtime_dotnet_gc_collections_count.*?generation=""gen(\d+)""} (\d+)" },
        { "GCHeapSize", @"process_runtime_dotnet_gc_heap_size_bytes.*?generation=""gen(\d+)""} (\d+)" },
        { "JITMethodsCompiled", @"process_runtime_dotnet_jit_methods_compiled_count.*? (\d+)" },
        { "JITCompilationTime", @"process_runtime_dotnet_jit_compilation_time_nanoseconds.*? (\d+)" },
        { "ThreadPoolThreadsCount", @"process_runtime_dotnet_thread_pool_threads_count.*? (\d+)" },
        { "HttpRequestDurationSum", @"http_server_request_duration_seconds_sum.*? (\d+\.\d+)" },
        { "HttpRequestDurationCount", @"http_server_request_duration_seconds_count.*? (\d+)" },
    };
    var rabbitMqMetrics = new Dictionary<string, string>
    {
        { "RabbitMQ Ready Message", @"rabbitmq_queue_messages_ready (\d+)" },
        { "Queues Count", @"rabbitmq_queues (\d+)" },
        { "Consumers Count", @"rabbitmq_consumers (\d+)" },
        { "Channels Count", @"rabbitmq_channels (\d+)" },
    };

    var cadvisorMetrics = new Dictionary<string, string>
    {
        { "jobs-service", @"container_cpu_system_seconds_total{container_label_Description="""",container_label_Vendor="""",container_label_com_docker_compose_config_hash=""095b1cde8aa3140d672040abc873f2dcb64db380c2909d8bac43dd605e46759b"",container_label_com_docker_compose_container_number=""1"",container_label_com_docker_compose_depends_on="""",container_label_com_docker_compose_image=""sha256:db415033f0da973bf1e893b6e60ebd884efc25d1eb7c64515eae23d125ff4a94"",container_label_com_docker_compose_oneoff=""False"",container_label_com_docker_compose_project=""fusion-server"",container_label_com_docker_compose_project_config_files=""D:\\Programming\\Docker Compose Volumes\\Fusion\\docker-compose.yml"",container_label_com_docker_compose_project_working_dir=""D:\\Programming\\Docker Compose Volumes\\Fusion"",container_label_com_docker_compose_replace="""",container_label_com_docker_compose_service=""jobs-service"",container_label_com_docker_compose_version=""2.26.1"",container_label_com_microsoft_created_by="""",container_label_com_microsoft_product="""",container_label_com_microsoft_version="""",container_label_com_microsoft_visual_studio_project_name="""",container_label_desktop_docker_io_binds_0_Source="""",container_label_desktop_docker_io_binds_0_SourceKind="""",container_label_desktop_docker_io_binds_0_Target="""",container_label_desktop_docker_io_binds_1_Source="""",container_label_desktop_docker_io_binds_1_SourceKind="""",container_label_desktop_docker_io_binds_1_Target="""",container_label_desktop_docker_io_binds_2_Source="""",container_label_desktop_docker_io_binds_2_SourceKind="""",container_label_desktop_docker_io_binds_2_Target="""",container_label_desktop_docker_io_binds_3_Source="""",container_label_desktop_docker_io_binds_3_SourceKind="""",container_label_desktop_docker_io_binds_3_Target="""",container_label_desktop_docker_io_binds_4_Source="""",container_label_desktop_docker_io_binds_4_SourceKind="""",container_label_desktop_docker_io_binds_4_Target="""",container_label_maintainer="""",container_label_net_dot_runtime_majorminor="""",container_label_net_dot_sdk_version="""",container_label_org_opencontainers_artifact_created="""",container_label_org_opencontainers_image_authors="""",container_label_org_opencontainers_image_base_digest="""",container_label_org_opencontainers_image_base_name="""",container_label_org_opencontainers_image_created="""",container_label_org_opencontainers_image_ref_name="""",container_label_org_opencontainers_image_version="""",container_label_vendor="""",id=""/docker/68785301a7a82c35cde27be94669e0dbfd4355dcf30a5a7a88fceaeee163be74"",image=""fusion/jobs:5.5.1.0"",name=""jobs-service""} (\d+)" },
    };

    //ParseMetricsData(await FetchMetricsDataAsync(coreMetricsUrl), aspMetrics);
    //ParseMetricsData(await FetchMetricsDataAsync(rabbitMqMetricsUrl), rabbitMqMetrics);
    //ParseMetricsData(await FetchMetricsDataAsync(cadvisorMetricsUrl), cadvisorMetrics);

    //GetCPUCadvisor(await FetchMetricsDataAsync(cadvisorMetricsUrl));
    //GetStartTimeCadvisor(await FetchMetricsDataAsync(cadvisorMetricsUrl));
    GetLastSeenCadvisor(await FetchMetricsDataAsync(cadvisorMetricsUrl));

    //await GetRedisData();
}
catch (Exception ex)
{
    Console.WriteLine($"Error fetching or parsing metrics: {ex.Message}");
}

return;

static void GetCPUCadvisor(string metricsData)
{
    // Updated regex to capture either the ID or name, CPU time, and timestamp
    var cpuSystemTimeRegex = new Regex(@"container_cpu_system_seconds_total.*?id=""([^""]*)""|name=""([^""]*)""} (\d+\.\d+) (\d+)");
    // Match the data
    var matches = cpuSystemTimeRegex.Matches(metricsData);

    foreach (Match match in matches)
    {
        if (!match.Success) continue;
        try
        {
            var idOrName = !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value;
            if (idOrName == "" || idOrName == "/")
                continue;
            var cpuTime = double.Parse(match.Groups[3].Value);
            var timestamp = long.Parse(match.Groups[4].Value);

            Console.WriteLine($"ID/Name: {idOrName}");
            Console.WriteLine($"CPU System Time (seconds): {cpuTime}");
            Console.WriteLine($"Timestamp: {timestamp}");
            Console.WriteLine($"=========");
        }
        catch (Exception)
        {
            // ignored
        }
    }
}

static void GetLastSeenCadvisor(string metricsData)
{
    // Updated regex to capture either the ID or name, CPU time, and timestamp
    var cpuSystemTimeRegex = new Regex(@"container_last_seen{.*?id=""([^""]*)""|name=""([^""]*)""} (\d+\.\d+e[+-]?\d+) (\d+)");
    // Match the data
    var matches = cpuSystemTimeRegex.Matches(metricsData);

    int i = 1;
    foreach (Match match in matches)
    {
        if (!match.Success) continue;
        try
        {
            var idOrName = !string.IsNullOrEmpty(match.Groups[1].Value) ? match.Groups[1].Value : match.Groups[2].Value;
            if (idOrName == "" || idOrName == "/")
                continue;
            var lastSeenValueD = double.Parse(match.Groups[3].Value);
            var lastSeenValue = match.Groups[3].Value;
            var timestamp = long.Parse(match.Groups[4].Value);

            var startTime = DateTimeOffset.FromUnixTimeSeconds((long)lastSeenValueD).DateTime;
            var offset = DateTime.Now - startTime;

            Console.WriteLine($"{i}. {idOrName}: {Humanize(offset)}");
            //Console.WriteLine($"ID/Name: {idOrName}");
            //Console.WriteLine($"Last Seen Value: {lastSeenValue}");
            //Console.WriteLine($"Last Seen Value H: {Humanize(offset)}");
            //Console.WriteLine($"Timestamp: {timestamp}");
            //Console.WriteLine($"=========");
            i++;
        }
        catch (Exception)
        {
            // ignored
        }
    }
}

static void GetStartTimeCadvisor(string metricsData)
{
    var serviceMetrics = new Dictionary<string, (double, DateTime)>();

    // Regex to match the service name and metric value
    string pattern = @"container_start_time_seconds{.*?container_label_com_docker_compose_service=""(.*?)"".*?} (\d+\.\d+e[+-]?\d+)";
    MatchCollection matches = Regex.Matches(metricsData, pattern);

    foreach (Match match in matches)
    {
        if (match.Groups.Count == 3)
        {
            string serviceName = match.Groups[1].Value;
            if (double.TryParse(match.Groups[2].Value, out double metricValue))
            {
                DateTime startTime = DateTimeOffset.FromUnixTimeSeconds((long)metricValue).DateTime;

                // Store both the metric value and the start time
                serviceMetrics[serviceName] = (metricValue, startTime);
            }
        }
    }

    Console.WriteLine($"{"Service Name",-30} {"Item2 Value",-30} {"Created At",-30}");
    Console.WriteLine(new string('-', 90));
    foreach (var service in serviceMetrics)
    {
        var offset = DateTime.Now - service.Value.Item2;

        Console.WriteLine($"{service.Key,-30} {service.Value.Item2,-30} {Humanize(offset),-30}");
    }
}

//-----------------------------

static async Task<string> FetchMetricsDataAsync(string url)
{
    using var client = new HttpClient();
    var response = await client.GetAsync(url);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}

static void ParseMetricsData(string metricsData, Dictionary<string, string> metricsDictionary)
{
    foreach (var metric in metricsDictionary)
    {
        var metricName = metric.Key;
        var pattern = metric.Value;
        var value = GetValue(metricsData, pattern);
        if (value.HasValue)
        {
            Console.WriteLine($"{metricName}: {value}");
        }
    }
}

static decimal? GetValue(string metricsData, string pattern)
{
    var regex = new Regex(pattern);
    var match = regex.Match(metricsData);
    if (!match.Success) return null;
    var value = Convert.ToDecimal(match.Groups[1].Value);
    return value;
}

static async Task GetRedisData()
{
    var metricsInfo = new Dictionary<string, string>()
    {
        { "connected_clients","Connected Clients" },
        { "maxclients","Max Client Count" },
        { "used_memory","Used Memory" },
        { "used_cpu_sys","Used System CPU" },
        { "used_cpu_user","Used User CPU" },
    };

    Console.WriteLine("---------REDIS-----------");
    var redis = ConnectionMultiplexer.Connect("localhost", x => x.AllowAdmin = true);
    var server = redis.GetServer("localhost", 6379);
    var db = redis.GetDatabase();

    var keysCount = await server.DatabaseSizeAsync();
    Console.WriteLine($"Keys count: {keysCount}");

    var memoryInfoAll = server.Info();

    // Find and output the used memory
    foreach (var memoryInfo in memoryInfoAll)
    {
        foreach (var info in memoryInfo)
        {
            var exist = metricsInfo.TryGetValue(info.Key, out var name);
            if (exist)
                Console.WriteLine($"{name}: {info.Value}");
        }
    }
}

//----------------------------------

static string Humanize(TimeSpan timeSpan)
{
    if (timeSpan.TotalSeconds < 0)
    {
        return "Invalid TimeSpan";
    }

    string humanizedTime = "";

    if (timeSpan.Days > 0)
    {
        humanizedTime += $"{timeSpan.Days} day{(timeSpan.Days > 1 ? "s" : "")}, ";
    }

    if (timeSpan.Hours > 0)
    {
        humanizedTime += $"{timeSpan.Hours} hour{(timeSpan.Hours > 1 ? "s" : "")}, ";
    }

    if (timeSpan.Minutes > 0)
    {
        humanizedTime += $"{timeSpan.Minutes} minute{(timeSpan.Minutes > 1 ? "s" : "")}, ";
    }

    if (timeSpan.Seconds > 0 || string.IsNullOrEmpty(humanizedTime))
    {
        humanizedTime += $"{timeSpan.Seconds} second{(timeSpan.Seconds > 1 ? "s" : "")}";
    }

    // Remove the trailing comma and space if necessary
    return humanizedTime.TrimEnd(',', ' ');
}