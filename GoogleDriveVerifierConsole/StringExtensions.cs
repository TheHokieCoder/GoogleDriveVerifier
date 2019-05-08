namespace GoogleDriveVerifier.Console
{
	using System;


	internal static class StringExtensions
	{
		private static readonly string[] BINARY_UNITS =
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
		};
		private static readonly string[] METRIC_UNITS =
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
		};

		public static string ToFileSizeString(this long? fileSize, bool metricPrefix = false, bool addSpace = false)
		{
			if (!fileSize.HasValue)
			{
				// Short-circuit because the nullable long is actually null, so the file size is unknown/undefined
				return "Unknown";
			}

			long divisor = metricPrefix ? 1000 : 1024;
			long numerator = fileSize.Value;
			long resultingDivisor = 1;
			int unitIndex = 0;

			while (numerator >= divisor)
			{
				numerator /= divisor;
				resultingDivisor *= divisor;
				unitIndex++;
			}

			return Math.Round(fileSize.Value / (double)resultingDivisor, 2) + (addSpace ? " " : "") + (metricPrefix ? METRIC_UNITS[unitIndex] :
				BINARY_UNITS[unitIndex]);
		}
	}
}
