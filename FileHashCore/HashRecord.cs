/**
 *
 * File Name: HashRecord.cs
 * Title    : Class Hash Record
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

namespace FileHashCore
{
	public class HashRecord
	{
		private string m_file;
		private byte[] m_hash;
		private HashType m_htype;
		private HashFormat m_hformat;
		private bool m_valid;

		public string file { get { return m_file; } set { m_file = value; } }
		public byte[] hash { get { return m_hash; } set { m_hash = value; } }
		public HashType htype { get { return m_htype; } set { m_htype = value; } }
		public HashFormat hformat { get { return m_hformat; } set { m_hformat = value; } }
		public bool valid { get { return m_valid; } set { m_valid = value; } }

		public HashRecord()
		{
			m_file = null;
			m_hash = null;
			m_htype = HashType.Invalid;
			m_hformat = HashFormat.Invalid;
			m_valid = false;
		}

		public HashRecord(string file, byte[] hash, HashType htype = HashType.MD5, HashFormat hformat = HashFormat.Default)
		{
			m_file = file;
			m_hash = hash;
			m_htype = htype;
			m_hformat = hformat;
			m_valid = true;
		}
	}
}
