/**
 *
 * File Name: GetOptions.cs
 * Title    : CLI argument parser; similar to the GNU C Library (glibc) functions
 *            getopt() and getopt_long()
 * Project  : CLIUtils in FileHash solution
 * Author   : Copyright (C) 2017, 2018 Johannes Krottmayer <krjdev@gmail.com>
 * Created  : 2017-01-28
 * Modified : 2018-09-03
 * Revised  : 
 * Version  : 0.1.1.1
 * License  : ISC (see file LICENSE_isc.txt)
 *
 * NOTE: This code is currently below version 1.0, and therefore is considered
 * to be lacking in some functionality or documentation, or may not be fully
 * tested. Nonetheless, you can expect most functions to work.
 *
 */

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CLIUtils
{
	public class GetOptionsException : Exception
	{
		public GetOptionsException () : base () { }
		public GetOptionsException (string message) : base (message) { }
		public GetOptionsException (string message, Exception inner) : base (message, inner) { }
	}

	public class GetOptions
	{
		public enum ARGUMENT {
			NoArgument,
			RequiredArgument,
			OptionalArgument }

		public struct Option {
			private readonly int o_optID;
			private readonly Nullable<char> o_optShort;
			private readonly string o_optLong;
			private readonly ARGUMENT o_hasArg;
			private readonly bool o_isRepeated;

			public int optID { get { return o_optID; } }
			public Nullable<char> optShort { get { return o_optShort; } }
			public string optLong { get { return o_optLong; } }
			public ARGUMENT hasArg { get { return o_hasArg; } }
			public bool isRepeated { get { return o_isRepeated; } }

			public Option (int optID, Nullable<char> optShort, string optLong, 
				ARGUMENT hasArg, bool isRepeated)
			{
				o_optID = optID;
				o_optShort = optShort;
				o_optLong = optLong;
				o_hasArg = hasArg;
				o_isRepeated = isRepeated;
			}
		}

		private class OptionPrivate {
			private int m_optID;
			private Nullable<char> m_optShort;
			private string m_optLong;
			private ARGUMENT m_hasArg;
			private List<string> m_args;
			private bool m_isRepeated;
			private bool m_valid;

			private OptionPrivate()
			{
				m_args = new List<string> ();
				m_valid = false;
			}

			public int optID { get { return m_optID; } set { m_optID = value; } }
			public Nullable<char> optShort { get { return m_optShort; } set { m_optShort = value; } }
			public string optLong { get { return m_optLong; } set { m_optLong = value; } }
			public ARGUMENT hasArg { get { return m_hasArg; } set { m_hasArg = value; } }
			public bool isRebeated { get { return m_isRepeated; } set { m_isRepeated = value; } }
			public bool valid { get { return m_valid; } set { m_valid = value; } }

			public OptionPrivate (int optID, Nullable<char> optShort, string optLong, 
				ARGUMENT hasArg, bool isRepeated)
			{
				m_args = new List<string> ();
				m_optID = optID;
				m_optShort = optShort;
				m_optLong = optLong;
				m_hasArg = hasArg;
				m_isRepeated = isRepeated;
				m_valid = true;
			}

			public void AddArgument (string arg)
			{
				if (m_valid && (hasArg == ARGUMENT.RequiredArgument || 
					hasArg == ARGUMENT.OptionalArgument))
					m_args.Add (arg);
			}

			public string[] ArgumentsToArray ()
			{
				if (m_valid && (hasArg == ARGUMENT.RequiredArgument || 
					hasArg == ARGUMENT.OptionalArgument))
					return m_args.ToArray ();
				else
					return null;
			}
		}

		private string[] m_args;
		private Option[] m_optsInput;
		private List<OptionPrivate> m_optsInternal;
		private List<string> m_noneOpts;
		private int m_optsInternal_id;
		private int m_opt_id;
		private int m_noneOpts_id;
		private bool m_initDone;

		private void ParseShortOption(int i)
		{
			string exmsg;
			int optID;
			string optLong;
			Nullable<char> optShort;
			ARGUMENT hasArg;
			bool isRepeated;
			Nullable<Option> optInput;
			OptionPrivate optInternal;
			string tmp;
			List<string> sopts;

			tmp = ExtractOption (m_args [i]);
			sopts = CombinedShortOptionsToStringList (tmp);

			foreach (string s in sopts) {
				string arg = null;
				int id;

				if ((optInput = FindInOptionInput (s.ToCharArray()[0])) == null) {
					exmsg = "invalid option '-" + s.ToCharArray()[0] + "'";
					throw new GetOptionsException (exmsg);
				}

				optID = optInput.Value.optID;
				optShort = optInput.Value.optShort;
				optLong = optInput.Value.optLong;
				hasArg = optInput.Value.hasArg;
				isRepeated = optInput.Value.isRepeated;

				if (hasArg == ARGUMENT.RequiredArgument || hasArg == ARGUMENT.OptionalArgument) {
					arg = ExtractOptionArgument (s, true);

					if ((arg == null) && (m_args.Length > (i + 1))) {
						i++;
						arg = m_args [i];
					} else {
						if ((hasArg == ARGUMENT.RequiredArgument) && (arg == null)) {
							exmsg = "option \"-" + s.ToCharArray()[0] + "\" requires an argument";
							throw new GetOptionsException (exmsg);
						}
					}
				}

				optInternal = FindInOptionInternal (optShort);

				if (optInternal == null) {
					optInternal = new OptionPrivate (optID, optShort, optLong, hasArg, isRepeated);
					optInternal.valid = true;

					if (hasArg == ARGUMENT.RequiredArgument || hasArg == ARGUMENT.OptionalArgument)
					if (arg != null)
						optInternal.AddArgument (arg);

					m_optsInternal.Add (optInternal);
				} else {
					if ((id = FindOptionInternalIndex (optShort)) != -1) {
						if (id >= m_optsInternal.Count)
							throw new GetOptionsException ("Oops, internal error (index out of range)");

						if (optInternal.isRebeated)
						if (arg != null)
							m_optsInternal [id].AddArgument (arg);
						else {
							exmsg = "option '-" + optShort + "' cannot occur several times";
							throw new GetOptionsException (exmsg);
						}
					}
				}
			}
		}

		private void ParseLongOption(int i)
		{
			string exmsg;
			int optID;
			string optLong;
			Nullable<char> optShort;
			ARGUMENT hasArg;
			bool isRepeated;
			Nullable<Option> optInput;
			OptionPrivate optInternal;
			string tmp;
			string arg = null;
			int id;

			tmp = ExtractOption (m_args [i]);

			if ((optInput = FindInOptionInput (tmp)) == null) {
				exmsg = "invalid option '--" + tmp + "'";
				throw new GetOptionsException (exmsg);
			}

			optID = optInput.Value.optID;
			optShort = optInput.Value.optShort;
			optLong = optInput.Value.optLong;
			hasArg = optInput.Value.hasArg;
			isRepeated = optInput.Value.isRepeated;

			if (hasArg == ARGUMENT.RequiredArgument || hasArg == ARGUMENT.OptionalArgument) {
				arg = ExtractOptionArgument (m_args[i], false);

				if ((arg == null) && (m_args.Length > (i + 1))) {
					i++;
					arg = m_args [i];
				} else {
					if ((hasArg == ARGUMENT.RequiredArgument) && (arg == null)) {
						exmsg = "option '" + m_args[i].Remove (m_args[i].IndexOf ('='),
							(m_args[i].Length - m_args[i].IndexOf ('='))) + "' requires an argument";
						throw new GetOptionsException (exmsg);
					}
				}
			}

			optInternal = FindInOptionInternal (optLong);

			if (optInternal == null) {
				optInternal = new OptionPrivate (optID, optShort, optLong, hasArg, isRepeated);
				optInternal.valid = true;

				if (hasArg == ARGUMENT.RequiredArgument || hasArg == ARGUMENT.OptionalArgument)
				if (arg != null)
					optInternal.AddArgument (arg);

				m_optsInternal.Add (optInternal);
			} else {
				if ((id = FindOptionInternalIndex (optLong)) != -1) {
					if (id >= m_optsInternal.Count)
						throw new GetOptionsException ("Oops, internal error (index out of range)");

					if (optInternal.isRebeated)
					if (arg != null)
						m_optsInternal [id].AddArgument (arg);
					else {
						exmsg = "option '--" + optLong + "' cannot occur several times";
						throw new GetOptionsException (exmsg);
					}
				}
			}
		}

		private void Initialize()
		{			
			for (int i = 0; i < m_args.Length; i++)
				if (IsShortOption (m_args [i])) {
					ParseShortOption (i++);
				} else if (IsLongOption (m_args [i])) {
					ParseLongOption (i);
				} else
					m_noneOpts.Add (m_args [i]);

			m_optsInternal.Sort (delegate (OptionPrivate opt1, OptionPrivate opt2) {
				return opt1.optID.CompareTo (opt2.optID); } );
			m_initDone = true;
		}

		private bool IsShortOption (string opt)
		{
			if (Regex.IsMatch (opt, @"^-[^-]$") || Regex.IsMatch (opt, @"^-[^-]\S+$"))
				return true;
			else
				return false;
		}

		private bool IsLongOption (string opt)
		{
			return Regex.IsMatch (opt, @"^--\S+$");;
		}

		private Nullable<Option> FindInOptionInput (Nullable<char> opt)
		{
			foreach (Option o in m_optsInput)
				if (o.optShort == opt)
					return o;

			return null;
		}

		private Nullable<Option> FindInOptionInput (string opt)
		{
			foreach (Option o in m_optsInput)
				if (o.optLong.Equals (opt))
					return o;

			return null;
		}

		private OptionPrivate FindInOptionInternal (Nullable<char> opt)
		{
			if (m_optsInternal.Count > 0)
				foreach (OptionPrivate o in m_optsInternal)
					if (o.optShort == opt)
						return o;

			return null;
		}

		private OptionPrivate FindInOptionInternal (string opt)
		{
			if (m_optsInternal.Count > 0)
				foreach (OptionPrivate o in m_optsInternal)
					if (o.optLong.Equals(opt))
						return o;

			return null;
		}


		private int FindOptionInternalIndex (Nullable<char> opt)
		{
			foreach (OptionPrivate o in m_optsInternal)
				if (o.optShort == opt)
					return m_optsInternal.IndexOf(o);

			return -1;
		}

		private int FindOptionInternalIndex (string opt)
		{
			foreach (OptionPrivate o in m_optsInternal)
				if (o.optLong.Equals (opt))
					return m_optsInternal.IndexOf(o);

			return -1;
		}

		private string ExtractOption (string opt)
		{
			string tmp;

			if (IsShortOption (opt))
				tmp = opt.Remove (0, 1);
			else
				tmp = opt.Remove (0, 2);

			if (tmp.Contains ("=") && IsLongOption(opt))
				tmp = tmp.Remove (tmp.IndexOf ('='), (tmp.Length - tmp.IndexOf('=')));

			return tmp;
		}

		private string ExtractOptionArgument (string opt, bool isShortOpt)
		{
			string tmp = null;
			string exmsg = null;

			if (!opt.Contains ("="))
				return null;
			else
				if (isShortOpt)
					exmsg = "argument for option \"-" + opt.Remove (opt.IndexOf ('='),
						(opt.Length - opt.IndexOf ('='))) + "\" is missing";
				else
					exmsg = "argument for option '" + opt.Remove (opt.IndexOf ('='),
						(opt.Length - opt.IndexOf ('='))) + "' is missing";

			if (opt.Length > (opt.IndexOf ('=') + 1))
				tmp = opt.Remove (0, opt.IndexOf ('=') + 1);
			else
				throw new GetOptionsException (exmsg);

			return tmp;
		}

		private List<string> CombinedShortOptionsToStringList (string sopts)
		{
			List<string> ret = new List<string> ();
			string tmp;

			for (int i = 0; i < sopts.Length; i++) {
				tmp = sopts [i].ToString();

				if (sopts.Contains("="))
					if (sopts [i + 1] == '=') {
						tmp = tmp + sopts.Substring (sopts.IndexOf ('='), sopts.Length - sopts.IndexOf ('='));
						ret.Add (tmp);
						break;
					}

				ret.Add (tmp);
			}

			return ret;
		}

		private GetOptions ()
		{
			m_optsInternal = new List<OptionPrivate> ();
			m_noneOpts = new List<string> ();
			m_optsInternal_id = 0;
			m_noneOpts_id = 0;
			m_initDone = false;
		}

		public GetOptions (string[] args, Option[] opts)
		{
			m_args = args;
			m_optsInput = opts;
			m_optsInternal = new List<OptionPrivate> ();
			m_noneOpts = new List<string> ();
			m_optsInternal_id = 0;
			m_noneOpts_id = 0;
			m_initDone = false;
		}

		public Nullable<int> NextOption ()
		{
			Nullable<int> tmp;

			if (!m_initDone)
				Initialize ();

			if (m_optsInternal.Count > m_optsInternal_id) {
				m_opt_id = m_optsInternal_id;
				tmp = m_optsInternal [m_optsInternal_id++].optID;
			}
			else
				tmp = null;

			return tmp;
		}

		public string GetLastLongOption ()
		{
			if (!m_initDone)
				Initialize ();
			
			return m_optsInternal [m_opt_id].optLong;
		}

		public Nullable<char> GetLastShortOption ()
		{
			if (!m_initDone)
				Initialize ();
			
			return m_optsInternal [m_opt_id].optShort;
		}

		public string GetLongOption (int optID)
		{
			if (!m_initDone)
				Initialize ();
			
			foreach (OptionPrivate o in m_optsInternal)
				if (o.optID == optID)
					return o.optLong;

			return null;
		}

		public Nullable<char> GetShortOption(int optID)
		{
			if (!m_initDone)
				Initialize ();
			
			foreach (OptionPrivate o in m_optsInternal)
				if (o.optID == optID)
					return o.optShort;

			return null;
		}

		public string[] GetLastOptionArguments ()
		{
			if (!m_initDone)
				Initialize ();
			
			if (m_optsInternal.Count < m_optsInternal_id)
				throw new GetOptionsException ("Oops, internal error (index out of range)");

			return m_optsInternal [m_opt_id].ArgumentsToArray ();
		}

		public string[] GetOptionArguments (int optID)
		{
			if (!m_initDone)
				Initialize ();
			
			foreach (OptionPrivate o in m_optsInternal)
				if (o.optID == optID)
					return o.ArgumentsToArray ();

			return null;
		}

		public string NextNoneOption ()
		{
			string tmp;

			if (!m_initDone)
				Initialize ();

			if (m_noneOpts.Count > m_noneOpts_id)
				tmp = m_noneOpts [m_noneOpts_id++];
			else
				tmp = null;

			return tmp;
		}
	}
}
