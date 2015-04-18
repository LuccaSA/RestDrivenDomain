﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra.Models.Querying
{
	/// <summary>
	/// Opérateurs de comparaison d'expressions
	/// </summary>
	/// <see cref="https://msdn.microsoft.com/en-us/library/bb361179%28v=vs.110%29.aspx"/>
	public enum FilterOperand
	{
		[Description("Filter on Equality. usage : users?name=equals,bob  or users?name=bob or users?name=bob,arnold")]
		Equals,
		[Description("Filter on Unequality. usage : users?name=notequal,bob")]
		NotEqual,
		[Description("Filter on data starting with the following parameter. usage : users?name=starts,a")]
		Starts,
		[Description("Filter on data containing the following text. usage : users?name=like,a")]
		Like,
		[Description("Filter on date for which value is between following parameters. usage : users?dtContractEnd=between,2013-01-01,2014-01-01")]
		Between,
		/// <summary>
		/// Equivalent à l'opérateur 'LessThanOrEqual', mais exclusif aux dates
		/// </summary>
		[Description("Filter on date for which value is inferior to the following parameter. usage : users?dtContractEnd=until,today")]
		Until,
		/// <summary>
		/// Equivalent à l'opérateur 'GreaterThanOrEqual', mais exclusif aux dates
		/// </summary>
		[Description("Filter on date for which value is superior to the following parameter. usage : users?dtContractEnd=since,today")]
		Since,
		/// <summary>
		/// Filter on expression strictly superior to the second operand expression
		/// </summary>
		/// <example>files?size=greaterthan,50</example>
		[Description("Filter on expression strictly superior to the second operand expression. usage : files?size=greaterthan,50")]
		GreaterThan,
		/// <summary>
		/// Filter on expression strictly superior or equal to the second operand expression
		/// </summary>
		/// <example>files?size=greaterthanorequal,50</example>
		[Description("Filter on expression superior or equal to the second operand expression. usage : files?size=greaterthanorequal,50")]
		GreaterThanOrEqual,
		/// <summary>
		/// Filter on expression strictly inferior to the second operand expression
		/// </summary>
		/// <example>files?size=lessthan,50</example>
		[Description("Filter on expression strictly inferior to the second operand expression. usage : files?size=lessthan,50")]
		LessThan,
		/// <summary>
		/// Filter on expression inferior or equal to the second operand expression
		/// </summary>
		/// <example>files?size=lessthanorequal,50</example>
		[Description("Filter on expression inferior or equal to the second operand expression. usage : files?size=lessthanorequal,50")]
		LessThanOrEqual
	}
}
