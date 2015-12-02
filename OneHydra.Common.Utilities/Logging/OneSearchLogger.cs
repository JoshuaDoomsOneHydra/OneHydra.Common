using System;
using System.Data;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using OneHydra.Common.Utilities.Configuration;
using OneHydra.Common.Utilities.Extensions;

namespace OneHydra.Common.Utilities.Logging
{
    public class OneHydraLogger : IOneHydraLogger
    {
        #region Fields

        private static readonly object ConfigureLock = new object();
        private static IAppender _adoAppender;
        private readonly ILog _logger;

        #endregion Fields

        #region Constructors

        public OneHydraLogger(Type type)
            : this(type.FullName)
        {
        }

        public OneHydraLogger(string loggerName)
            : this(loggerName, new ConfigManagerHelper())
        {
        }

        public OneHydraLogger(string loggerName, IConfigManagerHelper config)
        {
            var hier = LogManager.GetRepository() as Hierarchy;
            // If we have the setting in the configuration set the log level.  Otherwise, default to only errors and fatal
            // debug messages.
            if (hier != null)
            {
                var logLevel = hier.LevelMap["ERROR"];
                var configLogLevel = config.GetAppSetting("OneHydraLogLevel");
                if (configLogLevel != null)
                {
                    logLevel = hier.LevelMap[configLogLevel.ToUpper()];
                }

                // Only do this once when the first instance is created.  After that, the adoAppender should be available
                // to all subsequent instantiations.  We're also only setting the log level for the entire configuration once.
                // If subsequently, someone uses log4net and usurps the hierarchy in the architecture, it could change the log level.     
                lock (ConfigureLock)
                {
                    if (_adoAppender == null)
                    {
                        _adoAppender = GetAdoAppender(config);
                        log4net.Config.BasicConfigurator.Configure(_adoAppender);
                        ILogger[] loggers = hier.GetCurrentLoggers();
                        foreach (var logger in loggers)
                        {
                            ((Logger)logger).Level = logLevel;
                        }
                        //Configure the root logger.
                        Logger rootLogger = hier.Root;
                        rootLogger.Level = logLevel;
                    }

                }

                // Set the appender and log level for this instance.  We know they should already be configured.
                var loggerToConfigure = hier.GetLogger(loggerName, hier.LoggerFactory);
                if (loggerToConfigure != null)
                {
                    loggerToConfigure.Level = logLevel;
                    _logger = LogManager.GetLogger(loggerName);

                }
            }
        }

        #endregion Constructors

        #region Methods

        private static IAppender GetAdoAppender(IConfigManagerHelper config)
        {
            var connectionString = config.GetConnectionString("OneHydraLog");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new Exception("A connection string to the log database is required.  Please add the connection string by the name OneHydraLog to the config and make sure the connection string points to a database with the expected OneSearchLog table.");
            }
            var adoAppender = new AdoNetAppender
            {
                Name = "ADONetAppender",
                BufferSize = 1,
                ConnectionType =
                    "System.Data.SqlClient.SqlConnection, System.Data, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b77a5c561934e089",
                ConnectionString = connectionString,
                CommandText =
                    "INSERT INTO [OneSearchLog] ([Date],[Thread],[Level],[Logger],[Message],[Exception], [MachineName]) VALUES (@log_date, @thread, @log_level, @logger, @message, @exception, @machineName)",
                CommandType = CommandType.Text
            };

            //<conversionPattern value="%property{log4net:HostName}" />

            var parameter = new AdoNetAppenderParameter { ParameterName = "@log_date", DbType = DbType.DateTime };
            var patternLayout = new PatternLayout { ConversionPattern = "%date{MM/dd/yyyy HH:mm:ss}" };
            patternLayout.ActivateOptions();
            parameter.Layout = new Layout2RawLayoutAdapter(patternLayout);
            adoAppender.AddParameter(parameter);

            parameter = new AdoNetAppenderParameter { ParameterName = "@thread", DbType = DbType.String, Size = 255 };
            patternLayout = new PatternLayout { ConversionPattern = "%thread" };
            patternLayout.ActivateOptions();
            parameter.Layout = new Layout2RawLayoutAdapter(patternLayout);
            adoAppender.AddParameter(parameter);

            parameter = new AdoNetAppenderParameter { ParameterName = "@log_level", DbType = DbType.String, Size = 50 };
            patternLayout = new PatternLayout { ConversionPattern = "%level" };
            patternLayout.ActivateOptions();
            parameter.Layout = new Layout2RawLayoutAdapter(patternLayout);
            adoAppender.AddParameter(parameter);

            parameter = new AdoNetAppenderParameter { ParameterName = "@logger", DbType = DbType.String, Size = 255 };
            patternLayout = new PatternLayout { ConversionPattern = "%logger" };
            patternLayout.ActivateOptions();
            parameter.Layout = new Layout2RawLayoutAdapter(patternLayout);
            adoAppender.AddParameter(parameter);

            parameter = new AdoNetAppenderParameter { ParameterName = "@message", DbType = DbType.String, Size = 200000000 };
            patternLayout = new PatternLayout { ConversionPattern = "%message" };
            patternLayout.ActivateOptions();
            parameter.Layout = new Layout2RawLayoutAdapter(patternLayout);
            adoAppender.AddParameter(parameter);

            parameter = new AdoNetAppenderParameter { ParameterName = "@machineName", DbType = DbType.String, Size = 255 };
            patternLayout = new PatternLayout { ConversionPattern = "%property{machineName}" };
            patternLayout.ActivateOptions();
            parameter.Layout = new Layout2RawLayoutAdapter(patternLayout);
            adoAppender.AddParameter(parameter);

            parameter = new AdoNetAppenderParameter { ParameterName = "@exception", DbType = DbType.String, Size = 200000000 };
            patternLayout = new PatternLayout { ConversionPattern = "%property{fullException}" };
            patternLayout.ActivateOptions();
            parameter.Layout = new Layout2RawLayoutAdapter(patternLayout);
            //var exceptionLayout = new ExceptionLayout(){IgnoresException = true};
            //exceptionLayout.ActivateOptions();
            //parameter.Layout = new Layout2RawLayoutAdapter(exceptionLayout);
            adoAppender.AddParameter(parameter);

            adoAppender.ActivateOptions();
            return adoAppender;
        }

        public void Debug(object message)
        {
            _logger.Debug(message);
        }

        public void Debug(object message, Exception exception)
        {
            LogFullExceptionString(() => _logger.Debug(message, exception), exception);
        }

        public void Info(object message)
        {
            _logger.Info(message);
        }

        public void Info(object message, Exception exception)
        {
            LogFullExceptionString(() => _logger.Info(message, exception), exception);
        }

        public void Warn(object message)
        {
            _logger.Warn(message);
        }

        public void Warn(object message, Exception exception)
        {
            LogFullExceptionString(() => _logger.Warn(message, exception), exception);
        }

        public void Error(object message)
        {
            _logger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            LogFullExceptionString(() => _logger.Error(message, exception), exception);
        }

        public void Fatal(object message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            LogFullExceptionString(() => _logger.Fatal(message, exception), exception);
        }

        private static void LogFullExceptionString(Action loggerFunction, Exception exception)
        {
            ThreadContext.Properties["fullException"] = exception.FullExceptionString();
            ThreadContext.Properties["machineName"] = Environment.MachineName;
            loggerFunction();
            // clear it, so it's not used in future calls 
            ThreadContext.Properties.Remove("fullException");
            ThreadContext.Properties.Remove("machineName");
        }

        #endregion Methods
    }
}