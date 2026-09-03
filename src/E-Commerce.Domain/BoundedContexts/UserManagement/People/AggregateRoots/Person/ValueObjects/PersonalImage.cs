namespace E_Commerce.Domain.BoundedContexts.UserManagement.People.AggregateRoots.Person.ValueObjects
{
    public sealed record PersonalImage
    {
        public Guid FileId { get; }
        public PersonalImage(Guid fileId)
        {
            if(fileId == Guid.Empty)
                throw new ArgumentNullException(nameof(fileId));  
            
            FileId = fileId;
        }

        public PersonalImage ChangeFileId(Guid fileId) => new PersonalImage(fileId);
    }
}
