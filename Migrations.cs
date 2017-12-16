using System;
using System.Data;
using Orchard.Data.Migration;
using CodeSanook.AdministrativeDivision.Models;
using Orchard.Data;
using Flurl;
using System.IO;
using Orchard.Environment.Configuration;
using System.Linq;
using System.Web.Hosting;
using System.Reflection;
using System.Text;

namespace CodeSanook.AdministrativeDivision
{
    public class Migrations : DataMigrationImpl
    {
        private readonly ITransactionManager transactionManager;
        private readonly IShellSettingsManager shellSettingsManager;

        public Migrations(
            ITransactionManager transactionManager,
            IShellSettingsManager shellSettingsManager)
        {
            this.transactionManager = transactionManager;
            this.shellSettingsManager = shellSettingsManager;
        }

        public int Create()
        {
            SchemaBuilder.CreateTable(typeof(ProvinceRecord).Name, table =>
                table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Code")
                    .Column<string>("NameInThai")
                    .Column<string>("NameInEnglish")
            );

            SchemaBuilder.CreateTable(typeof(DistrictRecord).Name, table =>
                table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Code")
                    .Column<string>("NameInThai")
                    .Column<string>("NameInEnglish")
                    .Column<int>("ProvinceRecord_Id")
            );

            SchemaBuilder.CreateTable(typeof(SubdistrictRecord).Name, table =>
                table
                    .Column<int>("Id", column => column.PrimaryKey().Identity())
                    .Column<int>("Code")
                    .Column<string>("NameInThai")
                    .Column<string>("NameInEnglish")
                    .Column<decimal>("Latitude")
                    .Column<decimal>("Longitude")
                    .Column<int>("District_Id")
                    .Column<int>("ZipCode")
            );

            //TODO add indexes and foreign keys
            return 1;
        }

        public int UpdateFrom1()
        {
            //All script blocks must be end with GO statement.
            var scripts = GetAdminstrativeDivisionDataScript();

            var session = transactionManager.GetSession();
            var buffer = new StringBuilder();
            foreach (var script in scripts)
            {
                //If line starts with GO, execute immediately.
                if (script.StartsWith("GO"))
                {
                    var query = session.CreateSQLQuery(buffer.ToString());
                    query.ExecuteUpdate();
                    buffer.Clear();
                }
                else
                {
                    //If line does not start with GO, just append script. 
                    buffer.AppendLine(script);
                }
            }

            return 2;
        }

        private string GetConnectionString()
        {
            var settings = shellSettingsManager.LoadSettings();
            var defaultSetting = settings.Where(setting => setting.Name == "Default").Single();
            var connectionString = defaultSetting.DataConnectionString;
            return connectionString;
        }

        private string[] GetAdminstrativeDivisionDataScript()
        {
            var scriptUrl = Url.Combine(
                "~/Modules",
                this.GetType().Assembly.GetName().Name,
                "Contents",
                "AdministrativeDivisionData.sql");

            var scriptPath = HostingEnvironment.MapPath(scriptUrl);
            return File.ReadAllLines(scriptPath);
        }

        private string GetRootPath()
        {
            var codeBaseUrl = Assembly.GetExecutingAssembly().CodeBase;
            var filePathToCodeBase = new Uri(codeBaseUrl).LocalPath;
            var directoryPath = Path.GetDirectoryName(filePathToCodeBase);
            return directoryPath;
        }
    }
}