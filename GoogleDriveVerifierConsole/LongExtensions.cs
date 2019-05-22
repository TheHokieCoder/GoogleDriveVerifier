namespace GoogleDriveVerifier.Console
{
	using System;


	/// <summary>
	///		Extension methods for <see cref="long"/> values.
	/// </summary>
	internal static class LongExtensions
	{
		/// <summary>
		///		Converts the size of a file, in number of bytes represented as a <see cref="long"/> value, to a human-readable string with units.
		/// </summary>
		/// <param name="fileSize">
		///		The number of bytes the file consists of
		/// </param>
		/// <param name="metricPrefix">
		///		A Boolean indicating whether to use metric units (true) or binary units (false). Defaults to metric (true).
		/// </param>
		/// <param name="addSpace">
		///		A Boolean indicating whether to include a space between the value and unit (true) or not (false). Defaults to not include the space
		///		(false).
		/// </param>
		/// <returns>
		///		A string containing the human-readable representation of the file size.
		/// </returns>
		/// <remarks>
		///		This method will always round the fractional portion of the resultant file size to two digits.
		/// </remarks>
		public static string ToFileSizeString(this long? fileSize, bool metricPrefix = false, bool addSpace = false)
		{
			// Define constants for accessing the units array, which will be used to adding a human-readable unit to the file size string
			const int BINARY = 0;
			const int METRIC = 1;
			string[,] UNITS =
			{
				{
					"B",
					"KiB",
					"MiB",
					"GiB",
					"TiB",
					"PiB",
					"EiB",
					"Zib",
					"YiB"
				},
				{
					"B",
					"KB",
					"MB",
					"GB",
					"TB",
					"PB",
					"EB",
					"ZB",
					"YB"
				}
			};

			if (!fileSize.HasValue)
			{
				// Short-circuit because the nullable long is actually null, so the file size is unknown/undefined
				return "Unknown";
			}

			long divisor = metricPrefix ? 1000 : 1024;
			long numerator = fileSize.Value;
			long resultingDivisor = 1;
			int unitIndex = 0;

			// Iteratively reduce the file size by order of magnitude until the smallest integer representation is found
			while (numerator >= divisor)
			{
				numerator /= divisor;
				resultingDivisor *= divisor;
				unitIndex++;
			}

			return Math.Round(fileSize.Value / (double)resultingDivisor, 2) + (addSpace ? " " : "") + (metricPrefix ? UNITS[METRIC, unitIndex] :
				UNITS[BINARY, unitIndex]);
		}
	}
}
