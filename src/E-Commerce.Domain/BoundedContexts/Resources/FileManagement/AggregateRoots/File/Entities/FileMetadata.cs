using E_Commerce.Domain.SharedKernel.Abstractions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Entities
{
    public class FileMetadata : BaseEntity
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public FileMetadata(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public void UpdateValue(string newValue)
        {
            Value = newValue;
        }
    }
}
