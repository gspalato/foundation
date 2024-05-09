namespace Foundation.Services.UPx.Types.Payloads;

public class RegisterDisposalInput
{
    public required Disposal[] Disposals { get; set; }
    
    public required string OperatorToken { get; set; }
}
