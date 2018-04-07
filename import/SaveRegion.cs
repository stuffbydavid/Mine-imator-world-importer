namespace import
{
	public class SaveRegion
	{
		public Point3D<int> start, end;

		public SaveRegion Copy()
		{
			SaveRegion c = new SaveRegion();
			c.start = start.Copy();
			c.end = end.Copy();
			return c;
		}
	}
}
