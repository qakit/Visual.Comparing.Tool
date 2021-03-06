using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VCT.Server.Helpers
{
	public class SimpleLoggerOptions
	{
		public IList<string> RequestKeys { get; set; }
		public IList<string> ResponseKeys { get; set; }
		public Action<string, object> Log { get; set; }
	}

	public class SimpleLogger
	{
		private readonly Func<IDictionary<string, object>, Task> _appFunc;
		private readonly SimpleLoggerOptions _options;

		public SimpleLogger(Func<IDictionary<string, object>, Task> func, SimpleLoggerOptions options)
		{
			_appFunc = func;
			_options = options;
		}

		public async Task Invoke(IDictionary<string, object> env)
		{
			foreach (var key in _options.RequestKeys)
			{
				_options.Log(key, env[key]);
			}

			await _appFunc(env);

			foreach (var key in _options.ResponseKeys)
			{
				_options.Log(key, env[key]);
			}
		}

	}
}