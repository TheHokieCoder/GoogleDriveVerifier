namespace GoogleDriveVerifier.Console
{
	using GoogleDriveVerifier.Console.Configuration;
	using GoogleDriveVerifier.Console;
	using Microsoft.Extensions.Configuration;
	using System;
	using System.IO;
	using System.Security.Cryptography;
	using System.Threading;
	using System.Threading.Tasks;


	/// <summary>
	///		An enumeration of possible codes the application can exit with.
	/// </summary>
	internal enum ExitCode : int
	{
		/// <summary>
		///		No errors occurred, execution was successful.
		/// </summary>
		Success = 0,
		/// <summary>
		///		The number of arguments specified at the console is not valid.
		/// </summary>
		InvalidArgumentCount = 1,
		/// <summary>
		///		An option argument specified is not valid.
		/// </summary>
		InvalidOption = 2,
		/// <summary>
		///		The specified file was not found at the specified local path.
		/// </summary>
		FileNotFoundLocally = 3,
		/// <summary>
		///		The specified file was not found in the Google Drive account.
		/// </summary>
		FileNotFoundInDrive = 4,
		/// <summary>
		///		The specified file path for the client secrets configuration JSON file is not valid.
		/// </summary>
		ConfigFileNotFound = 5,
		/// <summary>
		///		The client secrets configuration JSON file is of invalid format and could not be read.
		/// </summary>
		ConfigFileInvalid = 6,
		/// <summary>
		///		An unknown error occurred.
		/// </summary>
		Unknown = int.MaxValue
	}

	public class Program
	{
		private static string _clientIDFilePath = "client_id.json";
		private static ConsoleColor _defaultConsoleBackgroundColor;
		private static ConsoleColor _defaultConsoleForegroundColor;

		private static string GetHelpText()
		{
			// Generate the string containing all of the help information for the user
			return
				ApplicationInfo.Title + " v" + ApplicationInfo.Version + Environment.NewLine +
				ApplicationInfo.Copyright + Environment.NewLine +
				Environment.NewLine +
				"Usage: " + AppDomain.CurrentDomain.FriendlyName + " [OPTION] NICKNAME FILE MD5CHECKSUM" + Environment.NewLine +
				Environment.NewLine +
				"OPTION switches:" + Environment.NewLine +
				"    -h  Display this help information" + Environment.NewLine +
				Environment.NewLine +
				"Arguments:" + Environment.NewLine +
				"    NICKNAME      A nickname for the Google Drive account (e.g. 'Work'," + Environment.NewLine +
				"                  'Personal', 'Secret'). If this is the first time the Google" + Environment.NewLine +
				"                  account has been used, it will be given this nickname," + Environment.NewLine +
				"                  otherwise the cached account credentials will be used." + Environment.NewLine +
				"    FILE          The name of the file to verify the MD5 checksum" + Environment.NewLine +
				"    MD5CHECKSUM   The known MD5 checksum to verify against that calculated by" + Environment.NewLine +
				"                  Google Drive. If this is omitted, it will be calculated on the" + Environment.NewLine +
				"                  fly.";
		}

		public static async Task<int> Main(string[] args)
		{
			const char BELL = (char)7;
			_defaultConsoleBackgroundColor = Console.BackgroundColor;
			_defaultConsoleForegroundColor = Console.ForegroundColor;

			// Set the default exit code in the case that an unknown/unhandled error occurs
			Environment.ExitCode = (int)ExitCode.Unknown;

			if (args.Length == 0)
			{
				// Print out error message and exit app
				WriteError("No arguments specified. Please use the -h option for correct usage.");
				return (int)ExitCode.InvalidArgumentCount;
			}

			if (args.Length == 1)
			{
				if (args[0].ToLower() == "-h")
				{
					// Print out help information and exit app
					Console.WriteLine(GetHelpText());
					return (int)ExitCode.Success;
				}
				else
				{
					WriteError("Invalid option '" + args[0] + "'. Please use the '-h' option for correct usage.");
					return (int)ExitCode.InvalidOption;
				}
			}

			// Determine if the client secrets JSON file exists
			if (!File.Exists(_clientIDFilePath))
			{
				WriteError("The client secrets configuration JSON file (" + _clientIDFilePath + ") could not be found in the current working " +
					"directory. Unable to authenticate with Google Drive.");
				return (int)ExitCode.ConfigFileNotFound;
			}

			string inputFileMD5Checksum = null;
			string inputFileName = null;
			bool isFileFound = false;

			if (args.Length == 2)
			{
				// First check if any of the arguments are the "-h" options
				foreach (string argument in args)
				{
					if (argument.ToLower() == "-h")
					{
						// Print out help information and exit app
						Console.WriteLine(GetHelpText());
						return (int)ExitCode.Success;
					}
				}

				// Next, assume that the NICKNAME argument is valid, so check that the FILE argument is an existing file
				inputFileName = Path.GetFileName(args[1]);
				if (isFileFound = File.Exists(args[1]))
				{
					// Since the MD5CHECKSUM argument was not specified, calculate the MD5 checksum
					Console.Write("Computing MD5 checksum for '" + inputFileName + "'...");
					using (Stream inputFileStream = File.OpenRead(args[1]))
					{
						inputFileMD5Checksum = inputFileStream.ComputeHash<MD5CryptoServiceProvider>();
					}
					Console.WriteLine("complete!");
					Console.WriteLine("Local MD5 checksum: " + inputFileMD5Checksum);
				}
				else
				{
					// The specified file cannot be found, so write out an error message and exit app
					WriteError("The file '" + inputFileName + "' could not be found at the specified local path.");
					return (int)ExitCode.FileNotFoundLocally;
				}
			}

			if (args.Length < 4)
			{
				// If only 2 arguments were specified, the MD5 checksum was calculated above, already. If 3 arguments were specified, grab the
				// specified MD5 checksum from the arguments
				if (args.Length == 3)
				{
					inputFileMD5Checksum = args[2];
					inputFileName = Path.GetFileName(args[1]);
				}

				// We now have all parts needed to connect to the Google Drive API, check for the file's existence, and compare the MD5 checksums for
				// verification

				// Configure the client secrets object for the Google Drive API
				Google.Apis.Auth.OAuth2.GoogleClientSecrets googleClientSecrets;
				using (FileStream clientIDFileStream = new FileStream(_clientIDFilePath, FileMode.Open, FileAccess.Read))
				{
					try
					{
						// Load the secrets data from the JSON file
						googleClientSecrets = Google.Apis.Auth.OAuth2.GoogleClientSecrets.Load(clientIDFileStream);
					}
					catch
					{
						googleClientSecrets = null;
					}

					if (googleClientSecrets == null)
					{
						WriteError("Unable to read client secrets from the configuration JSON file.");
						return (int)ExitCode.ConfigFileInvalid;
					}
				}

				// Configure the UserCredential object using the ClientSecrets object
				Google.Apis.Auth.OAuth2.UserCredential userCredential = await Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
					googleClientSecrets.Secrets, new[] { Google.Apis.Drive.v3.DriveService.Scope.DriveMetadataReadonly }, args[0],
					CancellationToken.None, null);

				// Create a DriveService object to handle the connection to the Google Drive account
				using (Google.Apis.Drive.v3.DriveService driveService = new Google.Apis.Drive.v3.DriveService(
					new Google.Apis.Services.BaseClientService.Initializer()
					{
						ApplicationName = "Google Drive Verifier",
						HttpClientInitializer = userCredential
					}))
				{
					// Create a ListRequest object to query the Drive service and receive a list of matching items
					Google.Apis.Drive.v3.FilesResource.ListRequest listRequest = new Google.Apis.Drive.v3.FilesResource.ListRequest(driveService)
					{
						Fields = "files(createdTime,size,id,md5Checksum,originalFilename)",
						Q = "name='" + inputFileName + "'"
					};

					Console.Write("Searching Google Drive for the specified file...");

					// Execute the list request and save the list of matching files
					Google.Apis.Drive.v3.Data.FileList fileList = await listRequest.ExecuteAsync();

					if (fileList.Files.Count == 0)
					{
						Console.WriteLine();
						WriteError("The file was not found in Google Drive.");
						return (int)ExitCode.FileNotFoundInDrive;
					}
					else
					{
						Console.WriteLine("found!");
						bool md5ChecksumVerified;
						foreach (Google.Apis.Drive.v3.Data.File file in fileList.Files)
						{
							Console.WriteLine("File");
							Console.WriteLine("{");
							Console.WriteLine("    id: " + file.Id);
							Console.WriteLine("    createdTime: " + file.CreatedTime);
							Console.WriteLine("    originalFilename: " + file.OriginalFilename);
							Console.WriteLine("    size: " + file.Size.ToFileSizeString(false, true) + " (" + file.Size + " bytes)");
							Console.WriteLine("    md5Checksum: " + file.Md5Checksum);
							Console.Write("    MD5 Checksum Verified: ");
							md5ChecksumVerified = (String.Compare(file.Md5Checksum, inputFileMD5Checksum, true) == 0);
							Console.BackgroundColor = md5ChecksumVerified ? ConsoleColor.DarkGreen : ConsoleColor.DarkRed;
							Console.ForegroundColor = ConsoleColor.Gray;
							Console.WriteLine(md5ChecksumVerified.ToString());
							Console.BackgroundColor = _defaultConsoleBackgroundColor;
							Console.ForegroundColor = _defaultConsoleForegroundColor;
							Console.WriteLine("}");
						}

						Console.WriteLine();
						Console.Write(BELL);
						return (int)ExitCode.Success;
					}
				}
			}
			else
			{
				WriteError("Invalid number of arguments specified. Please use the '-h' option for correct usage.");
				return (int)ExitCode.InvalidArgumentCount;
			}
		}

		private static void WriteError(string errorMessage)
		{
			Console.WriteLine();
			Console.BackgroundColor = ConsoleColor.DarkRed;
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.Write("ERROR");
			Console.BackgroundColor = _defaultConsoleBackgroundColor;
			Console.ForegroundColor = _defaultConsoleForegroundColor;
			Console.WriteLine(": " + errorMessage);
		}

		private static void WriteWarning(string warningMessage)
		{
			Console.WriteLine();
			Console.BackgroundColor = ConsoleColor.Yellow;
			Console.ForegroundColor = ConsoleColor.Black;
			Console.Write("WARNING");
			Console.BackgroundColor = _defaultConsoleBackgroundColor;
			Console.ForegroundColor = _defaultConsoleForegroundColor;
			Console.WriteLine(": " + warningMessage);
		}
	}
}
