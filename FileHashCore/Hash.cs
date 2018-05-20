/**
 *
 * File Name: Hash.cs
 * Title    : Class Hash
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
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace FileHashCore
{
	public class HashException : Exception
	{
		public HashException() { }
		public HashException(string message)
			: base(message) { }
		public HashException(string message, Exception inner)
			: base(message, inner) { }
	}

	public class Hash
	{
		public static bool HashEquals(byte[] b1, byte[] b2)
		{
			if (b1.Length != b2.Length)
				return false;

			for (int i = 0; i < b1.Length; i++)
				if (b1 [i] != b2 [i])
					return false;

			return true;
		}

		public static bool HashEquals(HashRecord hash1, HashRecord hash2)
		{
			if (hash1.hash.Length != hash2.hash.Length)
				return false;

			for (int i = 0; i < hash1.hash.Length; i++)
				if (hash1.hash [i] != hash2.hash [i])
					return false;

			return true;
		}

		public static HashType ParseHashTypeFromString(string name)
		{
			HashType htype;

			switch (name.ToLower()) {
			case "md5":
				htype = HashType.MD5;
				break;
			case "sha1":
				htype = HashType.SHA1;
				break;
			case "sha256":
				htype = HashType.SHA256;
				break;
			case "sha384":
				htype = HashType.SHA384;
				break;
			case "sha512":
				htype = HashType.SHA512;
				break;
			default:
				htype = HashType.Invalid;
				break;
			}

			return htype;
		}

		public static string HashTypeToString(HashType htype)
		{
			string strType;

			switch (htype) {
			case HashType.MD5:
				strType = "MD5";
				break;
			case HashType.SHA1:
				strType = "SHA1";
				break;
			case HashType.SHA256:
				strType = "SHA256";
				break;
			case HashType.SHA384:
				strType = "SHA384";
				break;
			case HashType.SHA512:
				strType = "SHA512";
				break;
			default:
				strType = "Invalid";
				break;
			}

			return strType;
		}

		public static byte[] ComputeHash(string path, HashType htype)
		{
			FileStream fs = null;
			Byte[] hash = null;

			fs = new FileStream (path, FileMode.Open, FileAccess.Read);
			fs.Position = 0;

			switch (htype) {
			case HashType.MD5:
				MD5 md5 = MD5.Create ();
				hash = md5.ComputeHash (fs);
				md5.Dispose ();
				break;
			case HashType.SHA1:
				SHA1 sha1 = SHA1.Create ();
				hash = sha1.ComputeHash (fs);
				sha1.Dispose ();
				break;
			case HashType.SHA256:
				SHA256 sha256 = SHA256.Create ();
				hash = sha256.ComputeHash (fs);
				sha256.Dispose ();
				break;
			case HashType.SHA384:
				SHA384 sha384 = SHA384.Create ();
				hash = sha384.ComputeHash (fs);
				sha384.Dispose ();
				break;
			case HashType.SHA512:
				SHA512 sha512 = SHA512.Create ();
				hash = sha512.ComputeHash (fs);
				sha512.Dispose ();
				break;
			default:
				fs.Close ();
				throw new HashException("invalid hash algorithm");
			}

			fs.Close ();
			return hash;
		}

		public static byte[] ComputeHash(Stream s, HashType htype)
		{
			Byte[] hash = null;

			switch (htype) {
			case HashType.MD5:
				MD5 md5 = MD5.Create ();
				hash = md5.ComputeHash (s);
				md5.Dispose ();
				break;
			case HashType.SHA1:
				SHA1 sha1 = SHA1.Create ();
				hash = sha1.ComputeHash (s);
				sha1.Dispose ();
				break;
			case HashType.SHA256:
				SHA256 sha256 = SHA256.Create ();
				hash = sha256.ComputeHash (s);
				sha256.Dispose ();
				break;
			case HashType.SHA384:
				SHA384 sha384 = SHA384.Create ();
				hash = sha384.ComputeHash (s);
				sha384.Dispose ();
				break;
			case HashType.SHA512:
				SHA512 sha512 = SHA512.Create ();
				hash = sha512.ComputeHash (s);
				sha512.Dispose ();
				break;
			default:
				throw new HashException("invalid hash algorithm");
			}

			return hash;
		}

		public static int TransformBlock(byte[] buffer, int offset, int count,
			ref HashAlgorithm ha, HashType htype = HashType.MD5)
		{
			int ret;

			switch (htype) {
			case HashType.MD5:
				MD5 md5;

				if (ha == null) {
					md5 = MD5.Create ();
					ha = md5;
				} else
					md5 = (MD5) ha;

				ret = md5.TransformBlock(buffer, offset, count, buffer, 0);
				break;
			case HashType.SHA1:
				SHA1 sha1;

				if (ha == null) {
					sha1 = SHA1.Create ();
					ha = sha1;
				} else
					sha1 = (SHA1) ha;

				ret = sha1.TransformBlock(buffer, offset, count, buffer, 0);
				break;
			case HashType.SHA256:
				SHA256 sha256;

				if (ha == null) {
					sha256 = SHA256.Create ();
					ha = sha256;
				} else
					sha256 = (SHA256) ha;

				ret = sha256.TransformBlock(buffer, offset, count, buffer, 0);
				break;
			case HashType.SHA384:
				SHA384 sha384;

				if (ha == null) {
					sha384 = SHA384.Create ();
					ha = sha384;
				} else
					sha384 = (SHA384) ha;

				ret = sha384.TransformBlock(buffer, offset, count, buffer, 0);
				break;
			case HashType.SHA512:
				SHA512 sha512;

				if (ha == null) {
					sha512 = SHA512.Create ();
					ha = sha512;
				} else
					sha512 = (SHA512) ha;

				ret = sha512.TransformBlock(buffer, offset, count, buffer, 0);
				break;
			default:
				throw new HashException("invalid hash algorithm");
			}

			return ret;
		}

		public static byte[] TransformFinalBlock(byte[] buffer, int offset, int count,
			ref HashAlgorithm ha, HashType htype = HashType.MD5)
		{
			byte[] hash = null;

			switch (htype) {
			case HashType.MD5:
				MD5 md5;

				if (ha == null) {
					md5 = MD5.Create ();
					ha = md5;
				} else
					md5 = (MD5)ha;

				md5.TransformFinalBlock (buffer, offset, count);
				hash = md5.Hash;
				md5.Dispose ();
				break;
			case HashType.SHA1:
				SHA1 sha1;

				if (ha == null) {
					sha1 = SHA1.Create ();
					ha = sha1;
				} else
					sha1 = (SHA1)ha;

				sha1.TransformFinalBlock (buffer, offset, count);
				hash = sha1.Hash;
				sha1.Dispose ();
				break;
			case HashType.SHA256:
				SHA256 sha256;

				if (ha == null) {
					sha256 = SHA256.Create ();
					ha = sha256;
				} else
					sha256 = (SHA256)ha;

				sha256.TransformFinalBlock (buffer, offset, count);
				hash = sha256.Hash;
				sha256.Dispose ();
				break;
			case HashType.SHA384:
				SHA384 sha384;

				if (ha == null) {
					sha384 = SHA384.Create ();
					ha = sha384;
				} else
					sha384 = (SHA384)ha;

				sha384.TransformFinalBlock (buffer, offset, count);
				hash = sha384.Hash;
				sha384.Dispose ();
				break;
			case HashType.SHA512:
				SHA512 sha512;

				if (ha == null) {
					sha512 = SHA512.Create ();
					ha = sha512;
				} else
					sha512 = (SHA512)ha;

				sha512.TransformFinalBlock (buffer, offset, count);
				hash = sha512.Hash;
				sha512.Dispose ();
				break;
			default:
				throw new HashException("invalid hash algorithm");
			}

			return hash;
		}

		public static string ToHashString(byte[] hash, string path, HashType htype = HashType.MD5,
			HashFormat format = HashFormat.Default)
		{
			string strHash;
			string ret;

			strHash = HexConverter.ToHexString (hash, HexConverter.OPTION.Lower);

			switch (format) {
			case HashFormat.Default:
				ret = strHash + "  " + path;
				break;
			case HashFormat.BSD:
				switch (htype) {
				case HashType.MD5:
					ret = "MD5 (" + path + ") = " + strHash;
					break;
				case HashType.SHA1:
					ret = "SHA1 (" + path + ") = " + strHash;
					break;
				case HashType.SHA256:
					ret = "SHA256 (" + path + ") = " + strHash;
					break;
				case HashType.SHA384:
					ret = "SHA384 (" + path + ") = " + strHash;
					break;
				case HashType.SHA512:
					ret = "SHA512 (" + path + ") = " + strHash;
					break;
				default:
					throw new HashException ("invalid hash algorithm");
				}
				break;
			default:
				throw new HashException ("invalid hash format");
			}

			return ret;
		}

		public static string ToHashString(HashRecord hr)
		{
			string strHash;
			string ret;

			strHash = HexConverter.ToHexString (hr.hash, HexConverter.OPTION.Lower);

			switch (hr.hformat) {
			case HashFormat.Default:
				ret = strHash + "  " + hr.file;
				break;
			case HashFormat.BSD:
				switch (hr.htype) {
				case HashType.MD5:
					ret = "MD5 (" + hr.file + ") = " + strHash;
					break;
				case HashType.SHA1:
					ret = "SHA1 (" + hr.file + ") = " + strHash;
					break;
				case HashType.SHA256:
					ret = "SHA256 (" + hr.file + ") = " + strHash;
					break;
				case HashType.SHA384:
					ret = "SHA384 (" + hr.file + ") = " + strHash;
					break;
				case HashType.SHA512:
					ret = "SHA512 (" + hr.file + ") = " + strHash;
					break;
				default:
					throw new HashException ("invalid hash algorithm");
				}
				break;
			default:
				throw new HashException ("invalid hash format");
			}

			return ret;
		}

		private static HashFormat GetHashFormatFromString(string line)
		{
			string patDefault = @"^[0-9a-fA-F]+  |\*[\s\S]+$";
			string patBSD = @"^\S+ \([\s\S]+\) = [0-9a-fA-F]+$";

			if (Regex.IsMatch (line, patDefault))
				return HashFormat.Default;
			else if (Regex.IsMatch (line, patBSD))
				return HashFormat.BSD;
			else
				return HashFormat.Invalid;
		}

		private static HashType GetHashTypeFromString(string line, HashFormat format)
		{
			string patMD5 = @"^[0-9a-fA-F]{32}  |\*[\s\S]+$";
			string patSHA1 = @"^[0-9a-fA-F]{40}  |\*[\s\S]+$";
			string patSHA256 = @"^[0-9a-fA-F]{64}  |\*[\s\S]+$";
			string patSHA384 = @"^[0-9a-fA-F]{96}  |\*[\s\S]+$";
			string patSHA512 = @"^[0-9a-fA-F]{128}  |\*[\s\S]+$";
			string patMD5_BSD = @"^MD5 \([\s\S]+\) = [0-9a-fA-F]{32}$";
			string patSHA1_BSD = @"^SHA1 \([\s\S]+\) = [0-9a-fA-F]{40}$";
			string patSHA256_BSD = @"^SHA256 \([\s\S]+\) = [0-9a-fA-F]{64}$";
			string patSHA384_BSD = @"^SHA384 \([\s\S]+\) = [0-9a-fA-F]{96}$";
			string patSHA512_BSD = @"^SHA512 \([\s\S]+\) = [0-9a-fA-F]{128}$";
			HashType htype;

			switch (format) {
			case HashFormat.Default:
				if (Regex.IsMatch (line, patMD5))
					htype = HashType.MD5;
				else if (Regex.IsMatch (line, patSHA1))
					htype = HashType.SHA1;
				else if (Regex.IsMatch (line, patSHA256))
					htype = HashType.SHA256;
				else if (Regex.IsMatch (line, patSHA384))
					htype = HashType.SHA384;
				else if (Regex.IsMatch (line, patSHA512))
					htype = HashType.SHA512;
				else
					htype = HashType.Invalid;
				break;
			case HashFormat.BSD:
				if (Regex.IsMatch (line, patMD5_BSD))
					htype = HashType.MD5;
				else if (Regex.IsMatch (line, patSHA1_BSD))
					htype = HashType.SHA1;
				else if (Regex.IsMatch (line, patSHA256_BSD))
					htype = HashType.SHA256;
				else if (Regex.IsMatch (line, patSHA384_BSD))
					htype = HashType.SHA384;
				else if (Regex.IsMatch (line, patSHA512_BSD))
					htype = HashType.SHA512;
				else
					htype = HashType.Invalid;
				break;
			default:
				htype = HashType.Invalid;
				break;
			}

			return htype;
		}

		public static HashRecord GetHashRecordFromString(string line)
		{
			string file = null;
			byte[] hash = null;
			HashFormat format = HashFormat.Invalid;
			HashType htype = HashType.Invalid;
			HashRecord hr = null;
			string tmp = null;

			format = GetHashFormatFromString (line);

			if (format == HashFormat.Default) {
				htype = GetHashTypeFromString (line, format);

				if (htype == HashType.Invalid)
					return null;

				tmp = line.Substring (0, line.IndexOf (" "));
				file = line.Substring (line.IndexOf (" ") + 2);
			} else if (format == HashFormat.BSD) {
				htype = GetHashTypeFromString (line, format);

				if (htype == HashType.Invalid)
					return null;

				tmp = line.Substring (line.LastIndexOf (" = ") + 3);
				file = line.Substring (line.IndexOf ('(') + 1, line.LastIndexOf (')') - line.IndexOf ('(') - 1);
			} else
				return null;

			hash = HexConverter.ToByteArray (tmp);
			hr = new HashRecord (file, hash, htype, format);
			return hr;
		}
	}
}
