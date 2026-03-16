using System.Security.Cryptography;
using System.Text;

namespace MilliyMock.Shared.Helpers;

public static class OtpGenerator
{
    // Defined charset: Removed 0, O, 1, I to prevent user error
    private const string Charset = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public static string GenerateOtp(int length = 6)
    {
        var otp = new StringBuilder(length);
        
        for (var i = 0; i < length; i++)
        {
            // Use GetInt32 for a cryptographically secure random index
            var randomIndex = RandomNumberGenerator.GetInt32(Charset.Length);
            otp.Append(Charset[randomIndex]);
        }

        return otp.ToString();
    }
}