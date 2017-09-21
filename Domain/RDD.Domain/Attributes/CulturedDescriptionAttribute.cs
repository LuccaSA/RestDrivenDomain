﻿using System;
using System.Resources;
using System.Threading;

namespace RDD.Domain.Attributes
{
	public abstract class CulturedDescriptionAttribute : Attribute
	{
		string TermName { get; set; }
		ResourceManager ResxManager { get; set; }

		public string Description
		{
			get { return ResxManager.GetString(TermName, Thread.CurrentThread.CurrentCulture); }
		}

		public CulturedDescriptionAttribute(ResourceManager resxManager, string termName)
		{
			ResxManager = resxManager;
			TermName = termName;
		}

		/// <summary>
		/// Gets an attribute on an enum field value
		/// </summary>
		/// <param name="enumValue">The enum value</param>
		/// <returns>The enum Description if it exists, else an empty string</returns>
		public static string GetName(Enum enumValue)
		{
			var type = enumValue.GetType();
			var memInfo = type.GetMember(enumValue.ToString());
			var attributes = memInfo[0].GetCustomAttributes(typeof(CulturedDescriptionAttribute), false);
			return (attributes.Length > 0) ? ((CulturedDescriptionAttribute)attributes[0]).Description : String.Empty;
		}
	}
}