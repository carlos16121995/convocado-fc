namespace ConvocadoFc.Domain.Shared;

public class ValidationFailure
{
    public string PropertyName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
