using E_Commerce.Domain.BoundedContexts.Core.Verification.Enums;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Behaviors
{
    public partial class Document
    {
        public void AttachFile(Guid fileId)
        {
            if (Status != VerificationStatus.Draft && Status != VerificationStatus.PendingReview)
                throw new Exceptions.DocumentException($"Cannot attach file when status is {Status}.");
            if (!_fileIds.Contains(fileId))
                _fileIds.Add(fileId);
        }

        public void RemoveFile(Guid fileId)
        {
            if (Status != VerificationStatus.Draft && Status != VerificationStatus.PendingReview)
                throw new Exceptions.DocumentException($"Cannot remove file when status is {Status}.");
            _fileIds.Remove(fileId);
        }

        public bool HasFile(Guid fileId) => _fileIds.Contains(fileId);
        public int FileCount => _fileIds.Count;
    }
}

