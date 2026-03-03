using E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Brand.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.Catalog.AggregateRoots.Brand.Behaviors
{
    // This partial file handles all document-related operations for the Brand aggregate.
    public partial class Brand
    {
        /// <summary>
        /// Attaches an existing document (from the Administration context) to this brand.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document.</param>
        /// <exception cref="BrandException">Thrown if the document ID is invalid or already attached.</exception>
        public void AttachDocument(Guid documentId)
        {
            if (documentId == Guid.Empty)
                throw new InvalidOperationException("Document ID cannot be empty.");

            if (_documentsIds.Contains(documentId))
                throw new InvalidOperationException($"Document {documentId} is already attached to this brand.");

            _documentsIds.Add(documentId);
        }

        /// <summary>
        /// Detaches a document from this brand.
        /// </summary>
        /// <param name="documentId">The unique identifier of the document to detach.</param>
        /// <exception cref="BrandException">Thrown if the document is not attached.</exception>
        public void DetachDocument(Guid documentId)
        {
            if (!_documentsIds.Remove(documentId))
                throw new InvalidOperationException($"Document {documentId} is not attached to this brand.");
        }

        /// <summary>
        /// Checks whether a specific document is attached to this brand.
        /// </summary>
        public bool HasDocument(Guid documentId)
        {
            return _documentsIds.Contains(documentId);
        }

        /// <summary>
        /// Returns the number of documents currently attached.
        /// </summary>
        public int DocumentCount => _documentsIds.Count;
    }
}
