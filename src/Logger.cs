namespace I3Helper;

public static class Logger
{
    const string _logFilePath = "log.txt";

    public static async Task Log(string message)
    {
        File.AppendAllLines(_logFilePath, new string[1] { message });
    }
}
