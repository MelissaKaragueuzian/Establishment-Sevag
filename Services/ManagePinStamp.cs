using System.Security.Cryptography;
using System.Text;

namespace Website_Idea.Services;

public static class ManagePinStamp
{
    public const string ClaimType = "manage_pin_stamp";

    public static string For(string? pin) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pin ?? "")));
}
