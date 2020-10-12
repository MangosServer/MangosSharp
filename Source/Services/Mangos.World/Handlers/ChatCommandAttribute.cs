using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mangos.Common.Enums.Misc;

namespace Mangos.World.Handlers
{
	[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
	public class ChatCommandAttribute : Attribute
	{
		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string _cmdName;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private string _cmdHelp;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private AccessLevel _cmdAccess;

		public string cmdName
		{
			get;
			set;
		}

		public string cmdHelp
		{
			get;
			set;
		}

		public AccessLevel cmdAccess
		{
			get;
			set;
		}

		public ChatCommandAttribute(string cmdName, string cmdHelp = "No information available.", AccessLevel cmdAccess = AccessLevel.GameMaster)
		{
			this.cmdName = "";
			this.cmdHelp = "No information available.";
			this.cmdAccess = AccessLevel.GameMaster;
			this.cmdName = cmdName;
			this.cmdHelp = cmdHelp;
			this.cmdAccess = cmdAccess;
		}
	}
}
