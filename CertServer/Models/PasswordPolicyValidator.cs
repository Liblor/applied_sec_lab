namespace CertServer.Models
{
    public class PasswordPolicyValidator
    {
        private static string passwordList;
        private static readonly PasswordPolicyValidator _passwordValidatorInstance = new PasswordPolicyValidator();

        private PasswordPolicyValidator()
        {
            passwordList = System.IO.File.ReadAllText(CAConfig.PasswordListPath);
        }

        public static PasswordPolicyValidator GetInstance() => _passwordValidatorInstance;
        public bool IsValidPassword(string password) => passwordList.Contains(password);
    }
}