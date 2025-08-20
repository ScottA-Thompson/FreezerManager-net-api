using System.Security.Cryptography;
using System.Text;
namespace FreezerManager.Services;

public interface IAuthenticationService
{
    bool ValidatePin(string pin);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IConfiguration _configuration;

    public AuthenticationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool ValidatePin(string pin)
    {
        var configuredPin = _configuration["Auth:PIN"];
        var configuredPinHash = _configuration["Auth:PINHash"];

        if (string.IsNullOrEmpty(configuredPin) && string.IsNullOrEmpty(configuredPinHash))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(configuredPin))
        {
            return pin == configuredPin;
        }

        if (!string.IsNullOrEmpty(configuredPinHash))
        {
            return VerifyPin(pin, configuredPinHash);
        }

        return false;
    }

    private string HashPin(string pin)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin + "FreezerSalt"));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPin(string pin, string hash)
    {
        var pinHash = HashPin(pin);
        return pinHash == hash;
    }
}  