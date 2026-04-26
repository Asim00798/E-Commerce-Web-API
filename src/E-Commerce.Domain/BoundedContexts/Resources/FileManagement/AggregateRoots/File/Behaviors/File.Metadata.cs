using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Entities;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Enums;
using E_Commerce.Domain.BoundedContexts.Resources.FileManagement.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Resources.FileManagement.AggregateRoots.File.Behaviors
{
    public partial class File
    {
        public void AddTag(string tagValue)
        {
            if (string.IsNullOrWhiteSpace(tagValue)) throw new ArgumentException("Tag value cannot be empty.", nameof(tagValue));
            if (Status == FileStatusEnum.Deleted)
                throw new FileManagementDomainException("Cannot add tags to a deleted file.");

            if (!_tags.Any(t => t.TagValue.Equals(tagValue, StringComparison.OrdinalIgnoreCase)))
            {
                _tags.Add(new FileTag(tagValue));
            }
        }

        public void RemoveTag(string tagValue)
        {
            var tag = _tags.FirstOrDefault(t => t.TagValue.Equals(tagValue, StringComparison.OrdinalIgnoreCase));
            if (tag != null)
            {
                _tags.Remove(tag);
            }
        }

        public void UpdateMetadata(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Metadata key cannot be empty.", nameof(key));
            if (Status == FileStatusEnum.Deleted)
                throw new FileManagementDomainException("Cannot update metadata of a deleted file.");

            var existing = _metadata.FirstOrDefault(m => m.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.UpdateValue(value);
            }
            else
            {
                _metadata.Add(new FileMetadata(key, value));
            }
        }

        public void RemoveMetadata(string key)
        {
            var existing = _metadata.FirstOrDefault(m => m.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                _metadata.Remove(existing);
            }
        }

    }
}
