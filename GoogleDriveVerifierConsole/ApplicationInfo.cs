namespace GoogleDriveVerifier.Console
{
	using System.IO;
	using System.Reflection;


	/// <summary>
	///		Provides easy access to the various properties of the application.
	///	</summary>
	/// <remarks>
	///		Taken from the solutions posted to a question on StackOverflow (http://stackoverflow.com/questions/909555/).
	///	</remarks>
	internal static class ApplicationInfo
	{
		/// <summary>
		///		The copright set for the application.
		/// </summary>
		public static string Copyright
		{
			get
			{
				object[] attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				return attributes.Length == 0 ? null : ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		/// <summary>
		///		The title of the application.
		/// </summary>
		public static string Title
		{
			get
			{
				object[] attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = attributes[0] as AssemblyTitleAttribute;
					if (titleAttribute != null && titleAttribute.Title.Length > 0)
					{
						return titleAttribute.Title;
					}
				}

				return Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		/// <summary>
		///		The version number of the application.
		/// </summary>
		public static string Version
		{
			get
			{
				return Assembly.GetCallingAssembly().GetName().Version.ToString();
			}
		}
	}
}
