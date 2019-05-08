namespace GoogleDriveVerifierConsole
{
	using System;


	public class Program
	{
		private static string GetHelpText()
		{
			// Generate the string containing all of the help information for the user
			return
				Environment.NewLine +
				"Google Drive Verifier v0.1" + Environment.NewLine +
				"Copyright 2015-2019 Blair Allen" + Environment.NewLine +
				Environment.NewLine +
				"Usage: " + AppDomain.CurrentDomain.FriendlyName + " [OPTION] Nickname FileName MD5Hash" + System.Environment.NewLine +
				System.Environment.NewLine +
				"OPTION switches:" + System.Environment.NewLine +
				"-h  Display this help information" + System.Environment.NewLine +
				System.Environment.NewLine +
				"ARGUMENTS:" + System.Environment.NewLine +
				"Nickname  A nickname for the Google Drive account (e.g. 'Work', 'Personal', 'Secret')" + System.Environment.NewLine +
				"FileName  The name of the file to verify the MD5 hash" + System.Environment.NewLine +
				"MD5Hash   The known MD5 hash to verify against the MD5 calculated by Google Drive" + System.Environment.NewLine;
		}

		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("No arguments specified. Please use the -h option for correct usage.");
			}
			if (args.Length == 1)
			{
				if (args[0].ToLower() == "-h")
				{
					// Print out help information
					Console.WriteLine(GetHelpText());
				}
			}

			Console.WriteLine("Hello World!");
		}
	}
}
