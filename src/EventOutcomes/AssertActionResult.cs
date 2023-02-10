namespace EventOutcomes;

public sealed class AssertActionResult
{
    private AssertActionResult(bool success, string? failMessage)
    {
        Success = success;
        FailMessage = failMessage;
    }

    public bool Success { get; set; }
    public string? FailMessage { get; set; }

    public static AssertActionResult Successful() => new(true, null);
    public static AssertActionResult Failed(string? failMessage = null) => new(false, failMessage);

    public static implicit operator AssertActionResult(bool success) => new(success, null);
    public static implicit operator bool(AssertActionResult result) => result.Success;
}
