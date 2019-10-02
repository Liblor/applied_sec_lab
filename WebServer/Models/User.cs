namespace WebServer.Models
{
	// TODO: Configure model validation
	// TODO: Configure serialization
	public class User
	{
		public string Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }

		public string Name => $"{FirstName} {LastName}";
	}
}
