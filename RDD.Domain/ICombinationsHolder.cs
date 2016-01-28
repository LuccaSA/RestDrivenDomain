﻿using RDD.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDD.Domain
{
	public interface ICombinationsHolder
	{
		IEnumerable<Combination> Combinations { get; }
	}
}
