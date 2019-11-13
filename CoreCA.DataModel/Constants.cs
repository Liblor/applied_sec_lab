namespace CoreCA.DataModel
{
    public static class Constants
    {
        public const int CRLNextUpdatedIntervalMinutes = 10;

        public const string IMoviesUserTableName = "users";
        public const string IMoviesCAPublicCertsTableName = "public_certificates";
        public const string IMoviesCAPrivateCertsTableName = "private_certificates";

        public const int MinPasswordLength = 8;
        public const int MinPassphraseLength = 12;
        public const string CrlMimeType = "application/pkix-crl";
    }
}
