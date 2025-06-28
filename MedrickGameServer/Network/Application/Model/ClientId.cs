namespace MedrickGameServer.Network.Application;

public class ClientId
{
    public string Value { get; }
    
    public ClientId(string value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
    
    public static implicit operator string(ClientId clientId) => clientId.Value;
    public static implicit operator ClientId(string value) => new(value);
    
    public override string ToString() => Value;
}