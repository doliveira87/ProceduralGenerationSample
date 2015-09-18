namespace Delaunay
{
	namespace LR
	{
		public enum Side
		{
			LEFT = 0,
			RIGHT
		}

		public class SideHelper
		{
			public static Side Other (LR.Side leftRight)
			{
				return leftRight == LR.Side.LEFT ? LR.Side.RIGHT : LR.Side.LEFT;
			}
		}

	}
}