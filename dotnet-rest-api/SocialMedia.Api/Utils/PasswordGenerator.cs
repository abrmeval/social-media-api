using System.Text;

namespace SocialMedia.Api.Utils
{
    /// <summary>
    /// Utility class for generating temporary passwords.
    /// </summary>
    public static class PasswordGenerator
    {
        public static string GetTempPassword()
        {
            var rndNumber = new Random();
            var encode = new UTF8Encoding();

            string specialChars = "!@#$%&*?_-";
            string upperCase = "ABCDEFGHJKLMNOPQRSTUVWXYZ";
            string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            string code = rndNumber.Next(0, 99999).ToString().PadLeft(5, '0');

            //Convert to BASE 64 (8 Characters)
            code = Convert.ToBase64String(encode.GetBytes(code));

            //One special character
            code = code + specialChars[rndNumber.Next(0, specialChars.Length)];

            //One digit letter
            code = rndNumber.Next(0, 10) + code;

            //One uppercase letter
            code = upperCase[rndNumber.Next(0, upperCase.Length)] + code;

            //One lowercase letter
            code = code + lowerCase[rndNumber.Next(0, lowerCase.Length)];

            return code;
        }
    }
}