using CertServer.Data;
using CertServer.Models;

namespace CertServer.DataModifiers
{
    public class PrivateKeysDBModifier
    {
        private readonly IMoviesPrivateKeysContext _dbContext;

		public PrivateKeysDBModifier(IMoviesPrivateKeysContext dbContext)
        {
            _dbContext = dbContext;
        }

		public void AddPrivateKey(PrivateKey privKey)
		{
			_dbContext.PrivateKeys.Add(privKey);
			_dbContext.SaveChanges();
		}

		public PrivateKey GetPrivateKey(User user)
		{
			return _dbContext.PrivateKeys.Find(user.Uid);
		}
    }
}
