namespace CertServer.Models
{
    public class PasswordPolicyValidator
    {
        private string passwordList;

        public PasswordPolicyValidator()
        {
            passwordList = System.IO.File.ReadAllText(CAConfig.PasswordListPath);
        }

        public bool IsValidPassword(string password) => !passwordList.Contains(password);
    }
}