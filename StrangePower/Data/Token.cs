namespace StrangePower.Data;

public class Token
{
    public int Id { get; set; }
    public required string AccessToken { get; set; }
    public DateTime Expiry { get; set; }
}