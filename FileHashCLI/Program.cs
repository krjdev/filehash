/**
 *
 * File Name: Program.cs
 * Title    : FileHash Command Line Interface ("Rewrite from scratch" of
 *            the GNU-Coreutils md5sum, sha1sum, sha256sum, sha384sum
 *            and sha512sum with some additional functions.)
 * Project  : FileHashCLI in FileHash solution
 * Author   : Copyright (C) 2017 krjdev@gmail.com
 * Created  : 2017-01-28
 * Modified : 2018-07-13
 * Revised  : 
 * Version  : 0.1.1.0
 * License  : GPLv3+ (see file LICENSE_gplv3.txt)
 *
 * NOTE: This code is currently below version 1.0, and therefore is considered
 * to be lacking in some functionality or documentation, or may not be fully
 * tested. Nonetheless, you can expect most functions to work.
 *
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CLIUtils;
using FileHashCore;

namespace FileHashCLI {
	
	class MainClass
	{
		private const string msgWarnFormat = "{0}: WARNING: {1} line{2} is improperly formatted";
		private const string msgWarnOpenRead = "{0}: WARNING: {1} listed file{2} could not be read";
		private const string msgWarnFailed = "{0}: WARNING: {1} computed checksum{2} did NOT match";
		private const string msgNoFormattedLines = "{0}: {1}: no properly formatted {2} checksum lines found";
		private const string msgNoFormattedLine = "{0}: {1}: {2}: improperly formatted {3} checksum line";
		private const string msgOnlyVerify = "{0}: the --{1} option is meaningful only when verifying checksums";
		private const string msgMeaninglessBinaryText = "{0}: the --binary and --text options are meaningless " + 
			"when verifying checksums";
		private const string msgMeaninglessTag = "{0}: the --tag option is meaningless when verifying checksums";

		private static string assName;
		private static bool optWarn;
		private static bool optStrict;
		private static bool optIgnore;
		private static bool optStatus;
		private static bool optQuiet;

		private static FileVersionInfo GetAssemblyFileVersion()
		{
			FileVersionInfo fvi;

			Assembly a = Assembly.GetEntryAssembly ();
			fvi = FileVersionInfo.GetVersionInfo (a.Location);
			return fvi;
		}

		private static void ShowUsage()
		{			
			if (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				Console.Write ("Usage: mono {0} ", GetAssemblyFileVersion().OriginalFilename);
			else
				Console.Write ("Usage: {0} ", GetAssemblyFileVersion().OriginalFilename);
			
			Console.WriteLine ("[OPTION]... [FILE]...");
		}

		private static void ShowVersion()
		{
			Console.WriteLine ("{0} {1}", GetAssemblyFileVersion().OriginalFilename,
				GetAssemblyFileVersion().ProductVersion);
			Console.WriteLine (GetAssemblyFileVersion ().LegalCopyright);
			Console.WriteLine ("License GPLv3+: GNU GPL version 3 or later <http://gnu.org/licenses/gpl.html>.");
			Console.WriteLine ("This is free software: you are free to change and redistribute it.");
			Console.WriteLine ("There is NO WARRANTY, to the extent permitted by law.");
		}

		private static void ShowMessageTryHelp()
		{
			string assName = GetAssemblyFileVersion().OriginalFilename;

			if (Environment.OSVersion.Platform == PlatformID.Unix ||
				Environment.OSVersion.Platform == PlatformID.MacOSX)
				Console.Error.WriteLine ("Try 'mono {0} --help' for more information.", assName);
			else
				Console.Error.WriteLine ("Try '{0} --help' for more information.", assName);
		}

		private static void ShowHelp()
		{
			ShowUsage ();
			Console.WriteLine ("Print or check file checksums.");
			Console.WriteLine ();
			Console.WriteLine ("With no FILE, or when FILE is -, read standard input.");
			Console.WriteLine ();
			Console.WriteLine ("\t-b, --binary");
			Console.WriteLine ("\t\tread in binary mode (not supported, for compatibility with the GNU tools)");
			Console.WriteLine ("\t-c, --check");
			Console.WriteLine ("\t\tread hash sums from the FILEs and check them");
			Console.WriteLine ("\t-h <HASH>, --hash-type=<HASH>");
			Console.WriteLine ("\t\thash algorithm; if this option is not given,");
			Console.WriteLine ("\t\tthe default algorithm MD5 is used");
			Console.WriteLine ();
			Console.WriteLine ("\t\tavailable <hash> algorithms:");
			Console.WriteLine ("\t\tmd5");
			Console.WriteLine ("\t\tsha1");
			Console.WriteLine ("\t\tsha256");
			Console.WriteLine ("\t\tsha384");
			Console.WriteLine ("\t\tsha512");
			Console.WriteLine ();
			Console.WriteLine ("\t    --tag");
			Console.WriteLine ("\t\tcreate a BSD-style checksum");
			Console.WriteLine ("\t-t, --text");
			Console.WriteLine ("\t\tread in text mode (not supported, for compatibility with the GNU tools)");
			Console.WriteLine ();
			Console.WriteLine ("The following five options are useful only when verifying checksums:");
			Console.WriteLine ("\t    --ignore-missing");
			Console.WriteLine ("\t\tdon't fail or report status for missing files");
			Console.WriteLine ("\t    --quiet");
			Console.WriteLine ("\t\tdon't print OK for each successfully verified file");
			Console.WriteLine ("\t    --status");
			Console.WriteLine ("\t\tdon't output anything, status code shows success");
			Console.WriteLine ("\t    --strict");
			Console.WriteLine ("\t\texit non-zero for improperly formatted checksum lines");
			Console.WriteLine ("\t-w, --warn");
			Console.WriteLine ("\t\twarn about improperly formatted checksum lines");
			Console.WriteLine ();
			Console.WriteLine ("\t    --help");
			Console.WriteLine ("\t\tdisplay this help and exit");
			Console.WriteLine ("\t    --version");
			Console.WriteLine ("\t\toutput version information and exit");
		}

		private static void ShowException(Exception ex, string assName, string file)
		{
			if (ex is FileNotFoundException || ex is DirectoryNotFoundException)
				Console.Error.WriteLine ("{0}: {1}: No such file or directory", assName, file);
			else if (ex is UnauthorizedAccessException)
				Console.Error.WriteLine ("{0}: {1}: Permission denied", assName, file);
			else
				Console.Error.WriteLine ("{0}: {1}: {2}", assName, file, ex.Message);
		}

		private static int CheckFile(string file, HashType htype)
		{
			StreamReader sr = null;
			HashRecord hr = null;
			string line;
			int lineCnt = 0;
			int errFormat = 0;
			int errOpenRead = 0;
			int errFailed = 0;
			bool bErrOpenRead;
			int ret = 0;
			byte[] hash;

			try {
				if (!file.Equals("-"))
					sr = new StreamReader (file);
				else
					sr = new StreamReader (Console.OpenStandardInput ());
			} catch (Exception ex) {
				ShowException (ex, assName, file);
			}

			while (!sr.EndOfStream) {
				bErrOpenRead = false;
				hash = null;

				try {
					line = sr.ReadLine ();
					hr = Hash.GetHashRecordFromString (line);
					lineCnt++;

					if (hr != null)
						hash = Hash.ComputeHash (hr.file, hr.htype);
					else {
						if (optWarn)
							Console.Error.WriteLine(msgNoFormattedLine, assName, file,
								lineCnt, Hash.HashTypeToString(htype));

						errFormat++;

						if (optStrict)
							ret = 1;

						continue;
					}
				} catch (Exception ex) {
					if (!optIgnore && !optStatus)
						ShowException (ex, assName, hr.file);

					errOpenRead++;
					bErrOpenRead = true;
					ret = 1;
				}

				if (!bErrOpenRead) {
					if (Hash.HashEquals(hash, hr.hash)) {
						if (!optQuiet && !optStatus)
							Console.WriteLine(hr.file + ": OK");
					} else {
						if (!optStatus)
							Console.WriteLine(hr.file + ": FAILED");

						errFailed++;
						ret = 1;
					}
				} else
					if (!optIgnore && !optStatus)
						Console.WriteLine (hr.file + ": FAILED open or read");
			}

			if ((errFormat >= 1) && (errFormat != lineCnt) && !optStatus)
				Console.Error.WriteLine (msgWarnFormat, assName, errFormat,
					(errFormat > 1) ? "s" : "");
			else if (errFormat == lineCnt)
				Console.Error.WriteLine(msgNoFormattedLines, assName, file, htype);

			if ((errOpenRead >= 1) && !optIgnore && !optStatus)
				Console.Error.WriteLine (msgWarnOpenRead, assName, errOpenRead,
					(errOpenRead > 1) ? "s" : "");

			if (errFailed >= 1 && !optStatus)
				Console.Error.WriteLine(msgWarnFailed, assName, errFailed,
					(errFailed > 1) ? "s" : "");

			return ret;
		}

		private static int ComputeHash(string file, HashType htype, HashFormat hformat)
		{
			byte[] hash;

			try {
				if (!file.Equals("-"))
					hash = Hash.ComputeHash (file, htype);
				else
					hash = Hash.ComputeHash(Console.OpenStandardInput (), htype);

				Console.WriteLine (Hash.ToHashString (hash, file, htype, hformat));
			} catch (Exception ex) {
				ShowException (ex, assName, file);
				return 1;
			}

			return 0;
		}

		public static int Main (string[] args)
		{
			assName = GetAssemblyFileVersion ().OriginalFilename;
			GetOptions argParser;
			GetOptions.Option[] opts = {
				new GetOptions.Option(0, 'b', "binary", GetOptions.ARGUMENT.NoArgument, false), // Not supported (dummy)
				new GetOptions.Option(1, 'c', "check", GetOptions.ARGUMENT.NoArgument, false),
				new GetOptions.Option(2, 'h', "hash-type", GetOptions.ARGUMENT.RequiredArgument, false),
				new GetOptions.Option(3, null, "tag", GetOptions.ARGUMENT.NoArgument, false),
				new GetOptions.Option(4, 't', "text", GetOptions.ARGUMENT.NoArgument, false), // Not supported (dummy)
				new GetOptions.Option(5, null, "quiet", GetOptions.ARGUMENT.NoArgument, false),
				new GetOptions.Option(6, null, "ignore-missing", GetOptions.ARGUMENT.NoArgument, false),
				new GetOptions.Option(7, null, "status", GetOptions.ARGUMENT.NoArgument, false),
				new GetOptions.Option(8, 'w', "warn", GetOptions.ARGUMENT.NoArgument, false),
				new GetOptions.Option(9, null, "strict", GetOptions.ARGUMENT.NoArgument, false),
				new GetOptions.Option(10, null, "help", GetOptions.ARGUMENT.NoArgument, false),
				new GetOptions.Option(11, null, "version", GetOptions.ARGUMENT.NoArgument, false) };
			Nullable<int> opt;
			string noptString;
			int noptCount = 0;
			List<string> files = new List<string> ();
			HashType htype = HashType.MD5;
			HashFormat hformat = HashFormat.Default;
			bool optBinary = false;
			bool optCheck = false;
			bool optTag = false;
			bool optText = false;
			optQuiet = false;
			optIgnore = false;
			optStatus = false;
			optWarn = false;
			optStrict = false;
			int ret = 0;

			try {
				argParser = new GetOptions(args, opts);

				while ((opt = argParser.NextOption()) != null)
					switch (opt) {
					case 0:
						optBinary = true;
						break;
					case 1:
						optCheck = true;
						break;
					case 2:
						string[] a = argParser.GetLastOptionArguments ();
						htype = Hash.ParseHashTypeFromString (a [0]);
						
						if (htype == HashType.Invalid) {
							Console.Error.WriteLine("{0}: {1}: invalid hash algorithm",
								assName, a [0]);
							ShowMessageTryHelp();
							return 1;
						}

						break;
					case 3:
						hformat = HashFormat.BSD;
						optTag = true;
						break;
					case 4:
						optText = true;
						break;
					case 5:
						optQuiet = true;
						break;
					case 6:
						optIgnore = true;
						break;
					case 7:
						optStatus = true;
						break;
					case 8:
						optWarn = true;
						break;
					case 9:
						optStrict = true;
						break;
					case 10:
						ShowHelp ();
						return 0;
					case 11:
						ShowVersion ();
						return 0;
					default: // Should never be reached
						Console.Error.WriteLine("{0}: invalid option", assName);
						ShowMessageTryHelp();
						return 1;
					}

				if (optQuiet && !optCheck) {
					Console.Error.WriteLine(msgOnlyVerify, assName, "quiet");
					ShowMessageTryHelp ();
					return 1;
				} else if (optStatus && !optCheck) {
					Console.Error.WriteLine(msgOnlyVerify, assName, "status");
					ShowMessageTryHelp ();
					return 1;
				} else if (optStrict && !optCheck) {
					Console.Error.WriteLine(msgOnlyVerify, assName, "strict");
					ShowMessageTryHelp ();
					return 1;
				} else if (optWarn && !optCheck) {
					Console.Error.WriteLine(msgOnlyVerify, assName, "warn");
					ShowMessageTryHelp ();
					return 1;
				} else if (optIgnore && !optCheck) {
					Console.Error.WriteLine(msgOnlyVerify, assName, "ignore-missing");
					ShowMessageTryHelp();
					return 1;
				} else if ((optBinary || optText) && optCheck) {
					Console.Error.WriteLine(msgMeaninglessBinaryText, assName);
					ShowMessageTryHelp();
					return 1;
				} else if (optTag && optCheck) {
					Console.Error.WriteLine(msgMeaninglessTag, assName);
					ShowMessageTryHelp();
					return 1;
				}

				while ((noptString = argParser.NextNoneOption()) != null) {
					noptCount++;
					files.Add(noptString);
				}

				if (noptCount == 0)
					files.Add("-");
				
			} catch (GetOptionsException ex) {
				Console.Error.WriteLine ("{0}: {1}", assName, ex.Message);
				ShowMessageTryHelp ();
				return 1;
			}

			foreach (string file in files)
				if (optCheck) {
					ret = CheckFile (file, htype);
				} else {
					ret = ComputeHash (file, htype, hformat);
				}

			return ret;
		}
	}
}
