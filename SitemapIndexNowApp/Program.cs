using MySql.Data.MySqlClient;
using RestSharp;
using System.Text.Json;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: Program <configFilePath>");
            return;
        }

        string configFilePath = args[0];
        var settings = ConfigLoader.LoadSettings(configFilePath);
        if (settings == null)
        {
            Console.WriteLine("Failed to load configuration.");
            return;
        }

        SitemapProcessor processor = new SitemapProcessor(settings);
        processor.ProcessSitemaps();
    }
}

class ConfigLoader
{
    public static Config LoadSettings(string filePath)
    {
        try
        {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Config>(json);
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error loading config file: " + ex.Message);
            return null;
        }
    }
}

class SitemapProcessor
{
    private readonly Config _settings;
    public SitemapProcessor(Config settings)
    {
        _settings = settings;
    }

    public void ProcessSitemaps()
    {
        string connectionString = _settings.ConnectionString;
        int rowCount;
        int sitemapId = DatabaseHelper.GetLastSitemapId(_settings.IndexNowId, connectionString);
        do
        {
            string sqlQuery = $"SELECT sitemapid, loc FROM tbsitemap WHERE sitemapid > {sitemapId} LIMIT {_settings.PageSize};";

            List<(int sitemapId, string loc)> urlList = DatabaseHelper.GetSitemapUrls(sqlQuery, connectionString);

            if (urlList.Count > 0)
            {
                rowCount = urlList.Count;

                ApiExecutor.ExecuteSitemapRequest(sitemapId, urlList.ConvertAll(u => u.loc), _settings);

                sitemapId = urlList.Last().sitemapId;
                DatabaseHelper.UpdateSitemapId(sitemapId, _settings.IndexNowId, connectionString);
                Thread.Sleep(_settings.SleepTime);
            }
            else
            {
                rowCount = 0;
            }
        } while (rowCount > 0);
    }
}

class DatabaseHelper
{
    public static int GetLastSitemapId(int indexNowId, string connectionString)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT sitemapid FROM tbindexnow WHERE indexnowid = @indexNowId";
                using (var command = new MySqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@indexNowId", indexNowId);
                    object result = command.ExecuteScalar();
                    return result != null && int.TryParse(result.ToString(), out int sitemapId) ? sitemapId : 0;
                }
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error fetching sitemap ID: " + ex.Message);
            return 0;
        }
    }

    public static List<(int sitemapId, string loc)> GetSitemapUrls(string sqlQuery, string connectionString)
    {
        List<(int, string)> urlList = new List<(int, string)>();
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand(sqlQuery, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        urlList.Add((reader.GetInt32("sitemapid"), reader.GetString("loc")));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error fetching URLs: " + ex.Message);
        }
        return urlList;
    }

    public static bool UpdateSitemapId(int sitemapId, int indexNowId, string connectionString)
    {
        try
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "UPDATE tbindexnow SET sitemapid = @sitemapId WHERE indexnowid = @indexNowId";
                using (var command = new MySqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@sitemapId", sitemapId);
                    command.Parameters.AddWithValue("@indexNowId", indexNowId);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
        catch (Exception ex)
        {
            LogHelper.LogError("Error updating sitemap ID: " + ex.Message);
            return false;
        }
    }
}

class LogHelper
{
    public static void LogError(string message)
    {
        string logFilePath = "error.log";
        File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}\n");
    }
}

class ApiExecutor
{
    public static void ExecuteSitemapRequest(int sitemapId, List<string> urlList, Config settings)
    {
        var request = new RestRequest();
        request.Method = Method.Post;
        request.AddHeader("Content-Type", "application/json");
        request.AddJsonBody(new { host = settings.Host, key = settings.ApiKey, keyLocation = settings.KeyLocation, urlList = urlList });

        ExecuteRequest(settings.IndexNowEndpoints.Yandex, request, "Yandex", sitemapId);
        ExecuteRequest(settings.IndexNowEndpoints.IndexNow, request, "IndexNow", sitemapId);
        ExecuteRequest(settings.IndexNowEndpoints.Bing, request, "Bing", sitemapId);
    }

    private static void ExecuteRequest(string endpoint, RestRequest request, string serviceName, int sitemapId)
    {
        try
        {
            var client = new RestClient(endpoint);
            var response = client.Execute(request);
            Console.WriteLine($" ExecuteRequest Service Name : {serviceName} , SitemapId {sitemapId} Response: {response.Content} {response.StatusCode}");
        }
        catch(Exception ex)
        {
            LogHelper.LogError($"Error ExecuteRequest Service Name:{serviceName}, SitemapId :{sitemapId} Exception : {ex.Message}");
        }
    }
}

class Config
{
    public string ConnectionString { get; set; }
    public int PageSize { get; set; }
    public int IndexNowId { get; set; }
    public string Host { get; set; }
    public string ApiKey { get; set; }
    public string KeyLocation { get; set; }
    public int SleepTime { get; set; }
    public IndexNowEndpoints IndexNowEndpoints { get; set; }
}

class IndexNowEndpoints
{
    public string Yandex { get; set; }
    public string IndexNow { get; set; }
    public string Bing { get; set; }
}