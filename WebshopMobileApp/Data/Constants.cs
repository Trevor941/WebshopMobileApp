namespace WebshopMobileApp.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "AppSQLite.db3";
        public const string API_URL = "https://orders.lumarfoods.co.za:20603";
        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";
    }
}