/**
 *
 * File Name: HashType.cs
 * Title    : Enumeration Hash Type
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
	public enum HashType
	{
		Invalid,
		MD5, 
		SHA1,
		SHA256,
		SHA384,
		SHA512
	}
}
