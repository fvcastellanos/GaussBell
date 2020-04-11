namespace GaussBell.Services.Domain
{
    public class ChartPoint
    {
        public ChartPoint(double x, double y)
        {
            X = x;
            Y = y;
        }        
        
        public double X { get; }
        public double Y { get; }
    }
}
