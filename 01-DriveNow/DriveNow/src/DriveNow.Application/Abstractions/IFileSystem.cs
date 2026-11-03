namespace DriveNow.Application.Abstractions;

public interface IFileSystem
{
    string BaseDirectory { get; }

    string Combine(params string[] paths);

    void CreateDirectory(string path);

    void WriteAllText(string path, string content);
    
    void AppendAllText(string path, string content);
}