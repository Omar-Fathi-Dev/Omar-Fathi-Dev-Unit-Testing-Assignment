using DriveNow.Application.Abstractions;

namespace DriveNow.Infrastructure.External;

public class PhysicalFileSystem : IFileSystem
{
    public string BaseDirectory => AppContext.BaseDirectory;

    public string Combine(params string[] paths)
    {
        return Path.Combine(paths);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public void WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content);
    }
    
    public void AppendAllText(string path, string content)
    {
        File.AppendAllText(path, content);
    }
}