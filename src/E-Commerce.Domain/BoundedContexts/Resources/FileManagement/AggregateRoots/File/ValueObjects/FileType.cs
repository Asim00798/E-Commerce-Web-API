using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.ValueObjects
{
    public sealed record FileType
    {
        public FileTypeEnum Value { get; init; }

        public FileType(FileTypeEnum value)
        {
            Value = value;
        }

        public static FileType Generic => new(FileTypeEnum.Generic);
        public static FileType Image => new(FileTypeEnum.Image);
        public static FileType Video => new(FileTypeEnum.Video);
    }
}
