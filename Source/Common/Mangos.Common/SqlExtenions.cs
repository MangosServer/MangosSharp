using System;
using System.Data;

namespace Mangos.Common
{
	public static class SqlExtenions
	{
		public static T As<T>(this DataRow row, string field)
		{
			if (row == null || row[field] == null)
				throw new Exception("Null data row.");

			return (T)Convert.ChangeType(row[field], typeof(T));
		}

		/// <typeparam name="T1">Cast1</typeparam>
		/// <typeparam name="T2">Cast2</typeparam>
		public static T2 As<T1, T2>(this DataRow row, string field)
		{
			if (row == null || row[field] == null)
				throw new Exception("Null data row.");

			var t1 = (T1)Convert.ChangeType(row[field], typeof(T1));
			return (T2)Convert.ChangeType(t1, typeof(T2));
		}
	}
}
