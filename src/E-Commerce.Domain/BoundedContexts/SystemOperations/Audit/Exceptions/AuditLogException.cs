using E_Commerce.Domain.SharedKernel.Exceptions;

namespace E_Commerce.Domain.BoundedContexts.SystemOperations.Audit.Exceptions
{
    public class AuditLogException : Exception
    {
        string Action {get;set;}
        string Rule { get; set; }
        public AuditLogException(string action,string rule)
        { 
            Action = action;
            Rule = rule;
        }
    }
}