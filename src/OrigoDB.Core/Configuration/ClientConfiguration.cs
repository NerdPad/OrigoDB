﻿using System;
using Woocode.Utils;
using System.Configuration;

namespace OrigoDB.Core
{
    public abstract class ClientConfiguration : ConfigurationBase
    {
	    private enum Mode
	    {
		    Embedded,
			Remote
	    }

		public static ClientConfiguration Create(EngineConfiguration config)
		{
			return new LocalClientConfiguration(config);
		}

	    public static ClientConfiguration Create(string clientIdentifier = null)
		{
			if(string.IsNullOrEmpty(clientIdentifier))
				return new LocalClientConfiguration(EngineConfiguration.Create());

			var isConnectionString = clientIdentifier.Contains("=");
			if(isConnectionString)
				 return CreateConfigFromConnectionString(clientIdentifier);
            
            if (ConfigurationManager.AppSettings[clientIdentifier] != null)
                return Create(ConfigurationManager.AppSettings[clientIdentifier]);

            var config = EngineConfiguration.Create();
			config.Location.OfJournal = clientIdentifier;
			return new LocalClientConfiguration(config);
		}

        /// <summary>
        /// Used as a container for the enum so we can use the DictionaryMapper
        /// </summary>
        private class ModeSetting
        {
            public Mode Mode { get; set; }
        }

		static ClientConfiguration CreateConfigFromConnectionString(string connectionstring)
		{
		    var dictionary = connectionstring.ParseProperties();
            var mapper = new DictionaryMapper(dictionary);
            
            var modeSetting = new ModeSetting();
            mapper.Map(modeSetting);

            if (modeSetting.Mode == Mode.Embedded)
            {
                mapper.Converters[typeof(StorageLocation)] = s => new FileStorageLocation(s);
                var config = EngineConfiguration.Create();
                mapper.Map(config);
                return new LocalClientConfiguration(config);
            }
            else if (modeSetting.Mode == Mode.Remote)
            {
                var config = new RemoteClientConfiguration();
                mapper.Map(config);
				return new FailoverClusterClientConfiguration(config);
            }

            throw new InvalidOperationException();
		}

	    public abstract IEngine<TModel> GetClient<TModel>() where TModel : Model, new();
    }
}
