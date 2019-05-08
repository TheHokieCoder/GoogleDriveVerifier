﻿namespace GoogleDriveVerifier.Console
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
		///		An unknown error occurred.
		/// </summary>
		Unknown = int.MaxValue
	}

	public class Program
	{
		private static ConsoleColor _defaultConsoleBackgroundColor;
		private static ConsoleColor _defaultConsoleForegroundColor;

		private static string GetHelpText()
		{
			// Generate the string containing all of the help information for the user
			return
				ApplicationInfo.Title + " v" + ApplicationInfo.Version + Environment.NewLine +
				ApplicationInfo.Copyright + Environment.NewLine +
				Environment.NewLine +
				"Usage: " + AppDomain.CurrentDomain.FriendlyName + " [OPTION] NICKNAME FILE MD5HASH" + Environment.NewLine +
				Environment.NewLine +
				"OPTION switches:" + Environment.NewLine +
				"    -h  Display this help information" + Environment.NewLine +
				Environment.NewLine +
				"Arguments:" + Environment.NewLine +
				"    NICKNAME  A nickname for the Google Drive account (e.g. 'Work', 'Personal'," + Environment.NewLine +
				"              'Secret'). If this is the first time the Google account has been" + Environment.NewLine +
				"              used, it will be given this nickname, otherwise the cached account" + Environment.NewLine +
				"              credentials will be used." + Environment.NewLine +
				"    FILE      The name of the file to verify the MD5 hash" + Environment.NewLine +
				"    MD5HASH   The known MD5 hash to verify against the MD5 calculated by Google" + Environment.NewLine +
				"              Drive. If this is omitted, it will be calculated on the fly." + Environment.NewLine;
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

			string inputFileHash = null;
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
					// Since the MD5HASH argument was not specified, calculate the MD5 hash 
					Console.Write("Computing MD5 hash for '" + inputFileName + "'...");
					using (Stream inputFileStream = File.OpenRead(args[1]))
					{
						inputFileHash = inputFileStream.ComputeHash<MD5CryptoServiceProvider>();
					}
					Console.WriteLine("complete!");
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
				// If only 2 arguments were specified, the MD5 hash was calculated above, already. If 3 arguments were specified, grab the specified
				// MD5 hash from the arguments
				if (args.Length == 3)
				{
					inputFileHash = args[2];
					inputFileName = Path.GetFileName(args[1]);
				}

				// We now have all parts needed to connect to the Google Drive, check for the file's existence, and compare the MD5 hashes for
				// verification

				// Configure the ClientSecrets object for Google Drive
				// TODO: Make this an option the app configures as a different option argument, and then the secrets data is loaded from a secret
				//       key cache
				Google.Apis.Auth.OAuth2.ClientSecrets clientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets()
				{
					ClientId = "42693917941-9mnp6ehi45mk1tv48alvbiu6jj51lhum.apps.googleusercontent.com",
					ClientSecret = "DQAJBMpvELcPaH88AcAYYyLi"
				};

				// Configure the UserCredential object using the ClientSecrets object
				Google.Apis.Auth.OAuth2.UserCredential userCredential = await Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
					clientSecrets, new[] { Google.Apis.Drive.v3.DriveService.Scope.DriveMetadataReadonly }, args[0], CancellationToken.None, null);

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
							md5ChecksumVerified = (String.Compare(file.Md5Checksum, inputFileHash, true) == 0);
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
