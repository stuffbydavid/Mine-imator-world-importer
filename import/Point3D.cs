using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace import
{
	public class Point3D<T>
	{
		/// <summary>A point with a x,y and z value.</summary>
		public T X, Y, Z;

		public Point3D(T x, T y, T z)
		{
			X = x; Y = y; Z = z;
		}

		public override string ToString()
		{
			return X + "," + Y + "," + Z;
		}
	}
}