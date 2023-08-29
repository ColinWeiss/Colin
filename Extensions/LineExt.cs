namespace Colin.Core.Extensions
{
    public static class LineExt
    {
        /// <summary>
        /// 线线相交.
        /// </summary>
        public static bool LineIntersectLine( this Line line, Line targetLine )
        {
            return line.QuickReject( targetLine ) && line.Straddle( targetLine );
        }

        /// <summary>
        /// 跨立实验.
        /// </summary>
        static bool Straddle( this Line line, Line targetLine )
        {
            float l1x1 = line.Start.X;
            float l1x2 = line.End.X;
            float l1y1 = line.Start.Y;
            float l1y2 = line.End.Y;
            float l2x1 = targetLine.Start.X;
            float l2x2 = targetLine.End.X;
            float l2y1 = targetLine.Start.Y;
            float l2y2 = targetLine.End.Y;
            if( ((l1x1 - l2x1) * (l2y2 - l2y1) - (l1y1 - l2y1) * (l2x2 - l2x1)) *
                 ((l1x2 - l2x1) * (l2y2 - l2y1) - (l1y2 - l2y1) * (l2x2 - l2x1)) > 0 ||
                 ((l2x1 - l1x1) * (l1y2 - l1y1) - (l2y1 - l1y1) * (l1x2 - l1x1)) *
                 ((l2x2 - l1x1) * (l1y2 - l1y1) - (l2y2 - l1y1) * (l1x2 - l1x1)) > 0 )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 快速排序.
        /// </summary>
        static bool QuickReject( this Line line, Line targetLine )
        {
            float l1xMax = MathF.Max( line.Start.X, line.End.X );
            float l1yMax = MathF.Max( line.Start.Y, line.End.Y );
            float l1xMin = MathF.Min( line.Start.X, line.End.X );
            float l1yMin = MathF.Min( line.Start.Y, line.End.Y );
            float l2xMax = MathF.Max( targetLine.Start.X, targetLine.End.X );
            float l2yMax = MathF.Max( targetLine.Start.Y, targetLine.End.Y );
            float l2xMin = MathF.Min( targetLine.Start.X, targetLine.End.X );
            float l2yMin = MathF.Min( targetLine.Start.Y, targetLine.End.Y );
            if( l1xMax < l2xMin || l1yMax < l2yMin || l2xMax < l1xMin || l2yMax < l1yMin )
                return false;
            return true;
        }

    }
}