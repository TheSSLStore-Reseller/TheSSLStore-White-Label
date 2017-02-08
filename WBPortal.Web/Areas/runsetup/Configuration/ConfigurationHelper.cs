using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using WBSSLStore.Data;
using System.Linq;
using WBSSLStore.Domain;

namespace WhiteBrandShrink.Migrations
{

    public class ConfigurationHelper : IDisposable
    {

        private static readonly string Configurationpath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString() + "\\Configuration\\siteconfiguration.xml";
        public SetupConfig AllSettings
        {
            get;
            set;
        }

        public static bool IsConfigurationFileExist()
        {
            return File.Exists(Configurationpath);
        }

        public ConfigurationHelper()
        {
            if (!System.IO.File.Exists(Configurationpath))
            {
                SetupConfig sl = new SetupConfig();
                //sl.Configurations = new Configuration();
                sl.DataBaseSetting = new DataBaseSettings();
                XmlSerializer serializer = new XmlSerializer(typeof(SetupConfig));
                using (FileStream stream = File.Create(Configurationpath))
                {
                    serializer.Serialize(stream, sl);
                }
            }
        }

        public SetupConfig GetAllSettings()
        {
            if (!System.IO.File.Exists(Configurationpath))
                return AllSettings;

            XmlSerializer serializer = new XmlSerializer(typeof(SetupConfig));
            using (FileStream stream = File.OpenRead(Configurationpath))
            {
                var result = (SetupConfig)serializer.Deserialize(stream);
                if (result != null)
                    AllSettings = result;
            }
            return AllSettings;
        }

        public void SaveDBSettingsFile(SetupConfig dbsettings)
        {

            XmlSerializer serializer = new XmlSerializer(typeof(SetupConfig));
            using (FileStream stream = File.Create(Configurationpath))
            {
                serializer.Serialize(stream, dbsettings);
            }
        }

        public static bool migration(string connectionstringname, string connectionstring)
        {

            using (WBSSLStoreDb context = new WBSSLStoreDb(connectionstring))
            {

                try
                {
                    if (context.Database.Exists() && context.Database.CompatibleWithModel(true))
                    {
                        Site obj = context.Sites.Where(x => x.isActive).ToList().FirstOrDefault();
                        if(obj == null)
                        {
                            //context.Database.Delete();
                            //context.Database.Create();
                            DefaultDataSeed seed=  new DefaultDataSeed();
                            seed.MySeed(context);
                            seed = null;
                        }
                        return true;
                    }
                    else
                    {
                        context.Database.Delete();
                        context.Database.Create();
                        DefaultDataSeed seed = new DefaultDataSeed();
                        seed.MySeed(context);
                        seed = null;
                        return true;

                    }
                }
                catch
                {
                    context.Database.Delete();
                    context.Database.Create();
                    DefaultDataSeed seed = new DefaultDataSeed();
                    seed.MySeed(context);
                    seed = null;
                    return true;
                }
            }

        }
        void IDisposable.Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }


    [Serializable()]
    public class DataBaseSettings
    {
        private string _connectionstring = "";
        private string _servername = "";
        private string _databaseName = "";
        private string _username = "";
        private string _password = "";
        private string _tcpport = "";
        private int _connectiontimeout = 0;

        [XmlElement(ElementName = "IsExists")]
        public bool IsExists { get; set; }

        [XmlElement(ElementName = "IsWindowsAuthentication")]
        public bool IsWindowsAuthentication { get; set; }

        [XmlElement(ElementName = "IsCustomRawConnection")]
        public bool IsCustomRawConnection { get; set; }



        [XmlIgnore]
        public string ConnectionString { get { return !string.IsNullOrEmpty(_connectionstring) ? _connectionstring : ""; } set { _connectionstring = value; } }

        [XmlElement(ElementName = "ConnectionString")]
        public System.Xml.XmlCDataSection ConnectionString_Cdt
        {
            get
            {
                return new System.Xml.XmlDocument().CreateCDataSection(ConnectionString);
            }
            set
            {
                ConnectionString = value.Value;
            }
        }


        [XmlElement(ElementName = "ServerName")]
        public string ServerName { get { return !string.IsNullOrEmpty(_servername) ? _servername : ""; } set { _servername = value; } }

        [XmlElement(ElementName = "DatabaseName")]
        public string DatabaseName { get { return !string.IsNullOrEmpty(_databaseName) ? _databaseName : ""; } set { _databaseName = value; } }

        [XmlElement(ElementName = "UserName")]
        public string UserName { get { return !string.IsNullOrEmpty(_username) ? _username : ""; } set { _username = value; } }

        [XmlElement(ElementName = "Password")]
        public string Password { get { return !string.IsNullOrEmpty(_password) ? _password : ""; } set { _password = value; } }

        [XmlElement(ElementName = "TCPPort")]
        public string TCPPort { get { return !string.IsNullOrEmpty(_tcpport) ? _tcpport : ""; } set { _tcpport = value; } }

        [XmlElement(ElementName = "ConnectionTimeout")]
        public int ConnectionTimeout { get { return _connectiontimeout > 0 ? _connectiontimeout : 300; } set { _connectiontimeout = value; } }

        [XmlElement(ElementName = "UseSSL")]
        public bool UseSSL { get; set; }

        [XmlElement(ElementName = "ID")]
        public int ID { get; set; }


        [XmlElement(ElementName = "IsFinishDBSetup")]
        public bool IsFinishDBSetup { get; set; }

        [XmlElement(ElementName = "IsFinishPaymentSetup")]
        public bool IsFinishPaymentSetup { get; set; }

        [XmlElement(ElementName = "IsFinishSMTPSetup")]
        public bool IsFinishSMTPSetup { get; set; }

        [XmlElement(ElementName = "IsFinishAdminSetup")]
        public bool IsFinishAdminSetup { get; set; }

        [XmlElement(ElementName = "IsFinishGeneralSetup")]
        public bool IsFinishGeneralSetup { get; set; }


    }

    [Serializable()]
    [System.Xml.Serialization.XmlRoot("SetupConfig")]
    public class SetupConfig
    {

        [XmlElement(ElementName = "DataBaseSettings")]
        public DataBaseSettings DataBaseSetting { get; set; }
    }

}