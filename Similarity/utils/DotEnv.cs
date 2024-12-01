namespace Similarity.utils;

using System;
using System.IO;

public static class DotEnv
{
    public static Dictionary<string, string> GetEnv(string filePath)
    {
        var env = new Dictionary<string, string>();
        if (File.Exists(filePath))
        {
            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                    continue;
                
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                
                env[key] = value;

            }
        }        
        return env;
    }
}
