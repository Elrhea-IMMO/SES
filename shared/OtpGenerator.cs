using System;
using System.Security.Cryptography;

namespace SharedLib
{
	public static class OtpGenerator
	{
		private static readonly int OtpLength = 6;

		public static string GenerateOtp()
		{
			// Using a cryptographically secure RNG for better randomness
			using var rng = RandomNumberGenerator.Create();
			var bytes = new byte[OtpLength];
			rng.GetBytes(bytes);

			// Convert bytes to digits 0-9
			char[] digits = new char[OtpLength];
			for (int i = 0; i < OtpLength; i++)
			{
				digits[i] = (char)('0' + (bytes[i] % 10));
			}

			return new string(digits);
		}
	}
}
