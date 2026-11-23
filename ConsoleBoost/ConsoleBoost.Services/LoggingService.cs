namespace ConsoleBoost.Services;

public class LoggingService : ILoggingService
{
    private readonly string _logDirectory;
    private readonly string _logFilePath;
    private readonly object _lockObject = new();

    public LoggingService()
    {
        string docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        _logDirectory = Path.Combine(docsPath, "ConsoleBoost", "Logs");

        Directory.CreateDirectory(_logDirectory);

        string fileName = $"consoleboost_{DateTime.Now:yyyyMMdd}.log";
        _logFilePath = Path.Combine(_logDirectory, fileName);
    }

    public void LogInfo(string message) => WriteLog("INFO", message);
    public void LogWarning(string message) => WriteLog("WARN", message);
    public void LogError(string message, Exception? exception = null)
    {
        string fullMsg = exception != null ? $"{message} | Exception: {exception.Message}\n{exception.StackTrace}" : message;
        WriteLog("ERROR", fullMsg);
    }

    public void LogOptimization(string details) => WriteLog("OPTIMIZE", details);
    public void LogGameLaunch(string gameName, string path) => WriteLog("LAUNCH", $"Game Launched: {gameName} ({path})");

    private void WriteLog(string level, string message)
    {
        lock (_lockObject)
        {
            try
            {
                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(_logFilePath, entry);
            }
            catch
            {
                // Silently swallow file lock errors
            }
        }
    }
}
