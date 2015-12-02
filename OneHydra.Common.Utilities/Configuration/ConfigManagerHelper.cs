using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using OneHydra.Common.Utilities.Extensions;

namespace OneHydra.Common.Utilities.Configuration
{
    public class ConfigManagerHelper : IConfigManagerHelper
    {
        #region Fields

        private static System.Configuration.Configuration _environmentSpecificConfig;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Loads the dictionary containing the default configurations based on the environment setting in App.config.
        /// </summary>
        static ConfigManagerHelper()
        {
            LoadEnvironmentSpecificConfig();
        }

        public ConfigManagerHelper()
        {
            CurrentEnvironment = GetEnvironment();
        }

        /// <summary>
        /// WARNING: OVERWRITES THE DEFAULT CONFIG.  
        /// You should only do this if you know what the effect will be and you intend to do so.
        /// In cases not involving debugging and most cases that do involve debugging, 
        /// the default constructor should be sufficient.
        /// </summary>
        /// <param name="configFileToLoad"></param>
        public ConfigManagerHelper(string configFileToLoad)
            : this()
        {
            if (File.Exists(configFileToLoad))
            {
                using (StreamReader configReader = File.OpenText(configFileToLoad))
                {
                    LoadEnvironmentSpecific(configReader);
                }
            }
            else
            {
                throw new Exception("The specified config file does not exist");
            }
        }

        #endregion Constructors

        #region Properties

        public ConfigEnvironment CurrentEnvironment { get; private set; }


        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets a setting from the environment scoped configuration first.
        /// If it does not exist there, get's the setting from the app or machine configs.
        /// </summary>
        /// <param name="settingName">The name of the setting.</param>
        /// <returns>The setting or an empty string if the setting doesn't exist.</returns>
        public string GetAppSetting(string settingName)
        {
            KeyValueConfigurationElement returnSettingObject =
                _environmentSpecificConfig.AppSettings.Settings[settingName];
            var returnSetting = (returnSettingObject == null || string.IsNullOrEmpty(returnSettingObject.Value)
                                        ? ConfigurationManager.AppSettings[settingName]
                                        : returnSettingObject.Value);

            return returnSetting;
        }

        /// <summary>
        /// Gets a connection string from the environment scoped configuration first.
        /// If it does not exist there, get's the connection string from the app or machine configs.
        /// </summary>
        /// <param name="connStringName">The name of the connection string.</param>
        /// <returns>The setting or an empty string if the setting doesn't exist.</returns>
        public string GetConnectionString(string connStringName)
        {
            string returnString =
                _environmentSpecificConfig.ConnectionStrings.ConnectionStrings[connStringName].ConnectionString;
            if (string.IsNullOrEmpty(returnString))
            {
                returnString = ConfigurationManager.ConnectionStrings[connStringName].ConnectionString;
            }
            return returnString;
        }

        /// <summary>
        /// Gets the current environment setting from the config file.  The environment should have a value
        /// like one of the ConfigEnvironent enums and be added to the appSettings element of the config file
        /// with a key of "Environment".  If this does not exist, the environment will default to Development.
        /// </summary>
        /// <returns></returns>
        private static ConfigEnvironment GetEnvironment()
        {
            ConfigEnvironment environment;
            string environmentSetting = ConfigurationManager.AppSettings["Environment"];
            if (!string.IsNullOrEmpty(environmentSetting))
            {
                try
                {
                    environment = environmentSetting.ToEnum<ConfigEnvironment>();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "The environment value in machine.config must be Development, Uat, or Release: "
                        + environmentSetting +
                        " is not a valid value.  The environment will now default to Development.", ex);
                }
            }
            else
            {
                throw new Exception(@"The appSetting with key 'Environment' was not found in the config.  This may be caused by permissions issues regarding the config file.");
            }
            return environment;
        }

        /// <summary>
        /// Loads the machine and callaway configs.
        /// </summary>
        private static void LoadEnvironmentSpecificConfig()
        {
            var environment = GetEnvironment();
            // Now get the configuration defined by the environment. 
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (var resourceStream = assembly.GetManifestResourceStream("OneHydra.Common.Utilities.Configuration." + environment + ".config"))
            {
                if (resourceStream != null)
                {
                    using (var configResourceReader = new StreamReader(resourceStream))
                    {
                        LoadEnvironmentSpecificConfig(configResourceReader, assembly);
                    }
                }
                else
                {
                    throw new Exception("Either the environment enum name changed or the embedded config files name changed so that the default configuration was not loaded.  Was this change intentional?");
                }
            }
        }

        private static void LoadEnvironmentSpecific(TextReader configReader)
        {
            LoadEnvironmentSpecificConfig(configReader, Assembly.GetExecutingAssembly());
        }

        private static void LoadEnvironmentSpecificConfig(TextReader configReader, Assembly assembly)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (assembly != null && assembly.Location != null)
            {
                var assemblyLocation = new FileInfo(assembly.Location);
                // Write the configuration out to the directory of the running code in case a someone needs to look at it.
                if (assemblyLocation.Directory != null)
                {
                    string configLocationToWriteTo = assemblyLocation.Directory.FullName + "\\Current.config";
                    try
                    {
                        File.WriteAllText(configLocationToWriteTo, configReader.ReadToEnd());
                        var fileMap = new ExeConfigurationFileMap { ExeConfigFilename = configLocationToWriteTo };
                        _environmentSpecificConfig = ConfigurationManager.OpenMappedExeConfiguration(fileMap,ConfigurationUserLevel.None);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("The environment specific configuration failed to load.", ex);
                    }
                }
            }
            else
            {
                throw new Exception("The assembly must be provided in LoadEnvironmentSpecificConfig method.");
            }
        }

        #endregion Methods
    }
}