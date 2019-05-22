# Google Drive Verifier
Google Drive Verifier is a Windows command line utility for verifying files in any Google Drive. When uploading of a file to Google Drive is complete,
Google calculates the MD5 checksum of that file, but it is never made available to the account owner through the web interface. This utility pulls
that MD5 checksum from the Google Drive API and compares it to the locally calculated value. This gives the account owner added assurance that their
critical files uploaded to Google Drive are, in fact, bit for bit, identical copies.

#### Building
1. Ensure that you have the .NET Core SDK installed. This project currently targets v2.2, so ensure that you have <u>_at least_</u> that version of the SDK
installed. You can download the installer at [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download). You can check if, and
what version of, the .NET Core SDK is currently installed by running the following command from a terminal:<br>
`dotnet --version`
2. Clone or download the `master` branch to a directory of your choice and navigate there via a terminal.
3. Run `dotnet build PROJECT_FILE` where `PROJECT_FILE` is the .csproj file. If you are currently in the solution directory, the command will
look like `dotnet build GoogleDriveVerifierConsole\GoogleDriveVerifierConsole.csproj`. If you are currently in the
GoogleDriveVerifierConsole project directory, the command will be simply `dotnet build GoogleDriveVerifierConsole.csproj`. Run `dotnet build
-h` for more information about options for specifying how the build is configured.

#### Configuration
1. You will need to create an OAuth 2.0 client ID so that GoogleDriveVerifier can read metadata about files in your Google Drive. To do this,
go to the Google APIs dashboard at [https://console.developers.google.com/apis/dashboard](https://console.developers.google.com/apis/dashboard) and
log in with your Google account.
2. Select the **Credentials** items from the menu on the left, click the **Create credentials** button in the Credentials section, and select **OAuth
client ID** from the drop-down menu.
3. When prompted about the application type, select **Other** and then click **Create**.
4. Give a proper name to identify the new OAuth client (`Google Drive Verifier` is recommended) and click **Create** again.
5. A window will appear with the details about the client ID and secret. You can ignore these values and simply click **OK**.
6. You will see the new client listed in the Credentials section. Click the download icon (far right) to save the JSON file that contains the client
id and secret.
7. Rename this file to `.googledriveverifier.json` and save it in your user profile folder. This is the file name and path where the application
expects to find the JSON file. Because it will remain in your user profile folder, the client ID and secret should remain safe from other users. If,
for some reason, your user profile is open to other users, you are using an account that has shared credentials, or are using a public account, please
take EXTRA CAUTION to ensure that the `.googledriveverifier.json` file is protected from unauthorized access as it will, in certain circumstances,
allow access to your account via the Google APIs.
8. In the folder containing the build of the application there should be the `appsettings.json` file. This file currently only supports one
setting:
```
{
    // Indicates whether or not a sound should be played when processing is
    // finished (if not specified, defaults to 'true')
    "soundsOn": true
}
```

#### Usage
```
GoogleDriveVerifier [OPTION] NICKNAME FILE MD5CHECKSUM

OPTION Switches:
    -h  Display this help information

Arguments:
    NICKNAME      A nickname for the Google Drive account (e.g. 'Work',
                  'Personal', 'Secret'). If this is the first time the Google
                  account has been used, it will be given this nickname,
                  otherwise the cached account credentials will be used.
    FILE          The name of the file to verify the MD5 checksum
    MD5CHECKSUM   The known MD5 checksum to verify against that calculated by
                  Google Drive. If this is omitted, it will be calculated on the
                  fly.
```
##### Notes
- `NICKNAME` is an identifier that the Google Drive API .NET client uses to manage the authorization and refresh tokens used when making API calls.
Each time a new value is used, Google will force you to re-authenticate and authorize the OAuth client to access your account. As long as you use
a consistent identifier, you should be prompted to re-authenticate very infrequently, if ever.
- `FILE` can be an absolute or relative path. In either case, GoogleDriveVerifier will use the leaf, or file name, portion of the path for searching
Google Drive. As an example, `FILE` could be `C:\Users\example\Documents\myphoto.png`, in which case `myphoto.png` will be the filename
searched.

#### Release History
- _Currently unreleased_
