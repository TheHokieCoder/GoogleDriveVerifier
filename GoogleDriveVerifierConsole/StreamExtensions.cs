namespace GoogleDriveVerifier.Console
{
	using System.IO;
	using System.Security.Cryptography;
	using System.Text;


	/// <summary>
	///		Extension methods for <see cref="Stream"/> objects.
	/// </summary>
	internal static class StreamExtensions
	{
		/// <summary>
		///		Calculates a cryptographic hash of a <see cref="Stream"/> using the specified <see cref="HashAlgorithm"/>.
		/// </summary>
		/// <typeparam name="T">
		///		The <see cref="HashAlgorithm"/> to use to compute the cryptographic hash of the stream
		/// </typeparam>
		/// <param name="objectStream">
		///		The <see cref="Stream"/> to compute the cryptographic hash of
		/// </param>
		/// <returns>
		///		A string containing the cryptographic hash
		/// </returns>
		/// <remarks>
		///		This method was implemented using answers and recommendations from the StackOverflow question found at:
		///		https://stackoverflow.com/questions/10520048/calculate-md5-checksum-for-a-file
		/// </remarks>
		public static string ComputeHash<T>(this Stream objectStream) where T : HashAlgorithm, new()
		{
			StringBuilder hashString = new StringBuilder();
			// Record the current stream position so that it can be reset after calculating the cryptographic hash
			long currentPosition = objectStream.Position;

			using (T hasher = new T())
			{
				// Compute the bytes that make up the cryptographic hash
				byte[] hashBytes = hasher.ComputeHash(objectStream);
				// Convert each byte into a two-character hexademical string and appead it to the complete hash string
				foreach (byte bite in hashBytes)
				{
					hashString.Append(bite.ToString("x2"));
				}
			}

			// Reset the stream position back to where it was when the method was called
			objectStream.Seek(currentPosition, SeekOrigin.Begin);

			return hashString.ToString();
		}
	}
}
