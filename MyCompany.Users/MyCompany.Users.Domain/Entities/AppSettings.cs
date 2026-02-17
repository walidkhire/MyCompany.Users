namespace MyCompany.Users.Domain.Entities
{
    public class AppSettings
    {
        public string BaseUrl { get; set; }
        public string BaseUrlGaiaPut { get; set; }
        public string ApiKey { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string UsernameGaiaPut { get; set; }
        public string PasswordGaiaPut { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionStringProd { get; set; }
        public string ConnectionStringBasic { get; set; }
        public int? GaiaReleaseId { get; set; }
        public int? GaiaReferenceStatus { get; set; }
        public bool SafFeature { get; set; }
    }
}
