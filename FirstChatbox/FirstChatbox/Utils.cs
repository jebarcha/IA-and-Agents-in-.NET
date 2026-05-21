namespace FirstChatbox;

internal class Utils
{
    internal static void LoadEnvironmentVars()
    {
        foreach (var line in File.ReadAllLines(".env"))
        {
            // KEY - VALUE
            var parts = line.Split('=');
            if (parts.Length == 2)
            {
                Environment.SetEnvironmentVariable(parts[0], parts[1]);
            }
        }
    }
}
