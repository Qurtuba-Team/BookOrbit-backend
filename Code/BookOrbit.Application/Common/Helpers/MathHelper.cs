namespace BookOrbit.Application.Common.Helpers;

static public class MathHelper
{
    static public int CalculateTotalPages(int totalCount, int pageSize) => (int)Math.Ceiling(totalCount / (double)pageSize);
}

