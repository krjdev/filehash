/**
 *
 * File Name: HexConverter.cs
 * Title    : Class Hex Converter
 * Project  : FileHashCore in FileHash solution
 * Author   : Copyright (C) 2017 krjdev@gmail.com
 * Created  : 2017-01-28
 * Modified : 
 * Revised  : 
 * Version  : 0.1.0.0
 * License  : BSD-3-Clause (see file LICENSE_bsd3.txt)
 *
 * NOTE: This code is currently below version 1.0, and therefore is considered
 * to be lacking in some functionality or documentation, or may not be fully
 * tested. Nonetheless, you can expect most functions to work.
 *
 */

using System;
using System.Text;

namespace FileHashCore
{
	public class HexConverterException : Exception
	{
		public HexConverterException() { }
		public HexConverterException(string message)
			: base(message) { }
		public HexConverterException(string message, Exception inner)
			: base(message, inner) { }
	}

	public class HexConverter
	{
		public enum OPTION
		{
			Upper,
			Lower
		};

		public static bool CheckHexString(string str)
		{
			foreach (Char c in str)
				if (!((c >= '0' && c <= '9') ||
					(c >= 'a' && c <= 'f') ||
					(c >= 'A' && c <= 'F')))
					return false;

			return true;
		}

		public static string ToHexString(byte[] hash, OPTION opt = OPTION.Lower)
		{
			StringBuilder sb = new StringBuilder (hash.Length * 2);

			foreach (Byte b in hash)
				if (opt == OPTION.Lower)
					sb.AppendFormat ("{0:x2}", b);
				else
					sb.AppendFormat ("{0:X2}", b);

			return sb.ToString ();
		}

		public static byte[] ToByteArray(string str)
		{
			byte[] tmp = new Byte[str.Length / 2];

			if (!CheckHexString (str))
				throw new HexConverterException ("invalid hex string");

			for (int i = 0; i < str.Length; i += 2)
				tmp [i / 2] = Convert.ToByte (str.Substring (i, 2), 16);

			return tmp;
		}
	}
}
