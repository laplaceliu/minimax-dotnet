using System;
using System.IO;

public static class EnvLoader
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    public static void LoadEnv()
    {
        var envFile = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env");
        if (File.Exists(envFile))
        {
            foreach (var line in File.ReadAllLines(envFile))
            {
                var idx = line.IndexOf('=');
                if (idx > 0)
                {
                    var key = line[..idx].Trim();
                    var value = line[(idx + 1)..].Trim();
                    if (!string.IsNullOrEmpty(key) && Environment.GetEnvironmentVariable(key) == null)
                    {
                        Environment.SetEnvironmentVariable(key, value);
                    }
                }
            }
        }
    }
}
