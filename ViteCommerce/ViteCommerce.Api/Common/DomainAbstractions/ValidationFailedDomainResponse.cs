namespace ViteCommerce.Api.Common.DomainAbstractions;


[Serializable]
public class ValidationFailedException : Exception
{
    public ValidationFailedException(IReadOnlyList<ValidationError> errors)
    {
        Errors = errors;
    }

    protected ValidationFailedException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    public IReadOnlyList<ValidationError> Errors { get; }
}
