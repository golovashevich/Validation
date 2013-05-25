using System;
using System.Collections.Generic;

namespace Validation.Tests {
	public class Checks<T1, T2> : List<Tuple<T1, T2>> {
		public void Add(T1 t1, T2 t2) {
			Add(Tuple.Create(t1, t2));
		}
	}

	public class Checks<T1, T2, T3> : List<Tuple<T1, T2, T3>> {
		public void Add(T1 t1, T2 t2, T3 t3) {
			Add(Tuple.Create(t1, t2, t3));
		}
	}
}
