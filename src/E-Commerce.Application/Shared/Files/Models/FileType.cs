namespace E_Commerce.Application.Shared.Files.Models;

/// <summary>
/// Generic application-level classification of a file's content.
/// Infrastructure determines this from actual bytes, not from extension or declared type.
/// </summary>
public enum FileType
{
    Unknown = 0,
    Image = 1,
    Document = 2,
    Video = 3,
    Audio = 4,
    Archive = 5
}