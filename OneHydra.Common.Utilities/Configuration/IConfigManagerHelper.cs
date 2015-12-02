namespace OneHydra.Common.Utilities.Configuration
{
    public enum ConfigEnvironment
    {
        Development,
        Uat,
        Release
    }

    public interface IConfigManagerHelper
    {
        string GetAppSetting(string settingName);
        string GetConnectionString(string connStringName);
        ConfigEnvironment CurrentEnvironment { get; }
    }
}
