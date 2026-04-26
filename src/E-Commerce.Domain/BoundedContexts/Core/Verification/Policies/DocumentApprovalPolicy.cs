using E_Commerce.Domain.BoundedContexts.Core.Verification.AggregateRoots.Document.Behaviors;

namespace E_Commerce.Domain.BoundedContexts.Core.Verification.Policies
{
    /// <summary>
    /// Business rule dictating who can approve a document. 
    /// For instance, ensuring the person approving the document is not the person who owns it.
    /// </summary>
    public class DocumentApprovalPolicy
    {
        public virtual bool IsAllowedToApprove(Document document, Guid verifierId)
        {
            // Basic domain rule: You cannot verify your own document.
            if (document.Owner.OwnerId == verifierId)
            {
                return false;
            }

            return true;
        }
    }
}
