using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SecretSanta
{

	public interface ILocalResource
	{
		string WebAPIUrl { get; }

	}


	/// <summary>
	/// LocalResource is a singleton, representing _one_ of the 
	/// Resources/configXXXX.resx files.
	/// 
	/// </summary>
	public class LocalResource : ILocalResource
	{
		private static LocalResource _instance = null;
		private static object _syncLock = new Object();

		private ResourceManager _rm = null;

#region Singleton stuff
		// Default constructor is private - so class can't be instanciated with "new"
		private LocalResource()
		{
			bool isDebug = false;
			string resourceFileName;

#if DEBUG
			isDebug = true;
#endif

			if (isDebug)
			{
				resourceFileName = "SecretSanta.Resources.configDebug";
			}
			else
			{
				resourceFileName = "SecretSanta.Resources.configRelease";
			}

			try
			{
				_rm = new ResourceManager(resourceFileName, Assembly.GetExecutingAssembly());
			}
			catch (Exception ex)
			{
				// can't get Resources file... and can't really do much in a constructor either...
				Debug.WriteLine(String.Format("Can't get Resource file: {0} {1}", resourceFileName, ex.Message));
			}
		}


		/// <summary>
		/// Thread-safe method to create the singleton
		/// </summary>
		public static LocalResource GetInstance
		{
			get
			{
				if (_instance == null)
				{
					// should be thread-safe
					lock (_syncLock)
					{
						if (_instance == null)
						{
							_instance = new LocalResource();
						}
					}
				}
				return (_instance);
			}
		}

#endregion

		public string WebAPIUrl
		{
			get
			{
				if (_rm == null)
					return ("");
				else
				{
					return(_rm.GetString("WebAPIUrl"));
				}
			}
		}
	}
}
