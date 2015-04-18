using RDD.Infra.Models.Enums;
using RDD.Infra.Models.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Infra
{
	public interface IPrincipal
	{
		PrincipalType Type { get; }
		string Name { get; }
		ICollection<int> RolesID { get; }
		string Token { get; set; }
		Culture Culture { get; }
	}
}
