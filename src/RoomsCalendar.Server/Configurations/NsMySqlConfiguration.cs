using MySql.Data.MySqlClient;
using RoomsCalendar.Share.Configuration;

namespace RoomsCalendar.Server.Configurations
{
    sealed class NsMySqlConfiguration : IMySqlConfiguration
    {
        [ConfigurationKeyName("NS_MARIADB_USER")]
        public string? Username { get; set; }

        [ConfigurationKeyName("NS_MARIADB_PASSWORD")]
        public string? Password { get; set; }

        [ConfigurationKeyName("NS_MARIADB_DATABASE")]
        public string? DatabaseName { get; set; }

        [ConfigurationKeyName("NS_MARIADB_HOSTNAME")]
        public string? Host { get; set; }

        [ConfigurationKeyName("NS_MARIADB_PORT")]
        public uint Port { get; set; }

        public string GetConnectionString() => new MySqlConnectionStringBuilder
        {
            UserID = Username,
            Password = Password,
            Database = DatabaseName,
            Server = Host,
            Port = Port
        }.ConnectionString;
    }
}
