using System.Threading.Tasks;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;

using GaussBell.Services;
using System.Collections.Generic;
using GaussBell.Services.Domain;
using System.Linq;
using GaussBell.Pages.Model;

namespace GaussBell.Pages
{
    public class CanvasBase : ComponentBase
    {
        protected const int CanvasWidth = 600;
        protected const int CanvasHeight = 400;
        protected const int UnitNumbersX = 10;
        protected const int UnitNumbersY = 6;
        protected const int UnitSizeX = CanvasWidth / UnitNumbersX;
        protected const int UnitSizeY = CanvasHeight / UnitNumbersY;
        protected const int MinX = -4;
        protected const int TestPoints = 9;

        private const string CanvasBackgroundColor = "#d6dbd7";
        private const string CanvasGridLinesColor = "#fdfdfd";
        private const string ChartXValuesColor = "#000000";
        private const string GaussCurveColor = "#121e5e";
        private const string CriticalValueLinesColor = "#3a0647";

        protected BECanvasComponent _canvasReference;
        
        private Canvas2DContext _context;

        protected IEnumerable<ChartPoint> Points;

        protected CanvasModel canvasModel = new CanvasModel();

        protected override void OnInitialized()
        {
            Points = ChartPointService.BuildChartPoints(MinX, TestPoints);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _context = await _canvasReference.CreateCanvas2DAsync();
        }

        protected async Task PerfomTestAsync()
        {
            await DrawChartAsync(_context);
            await PlotCriticalValuesAsync(_context, canvasModel.CriticalValue);
            await PlotZValueAsync(_context, canvasModel.CriticalValue, canvasModel.ZValue);
        }

        private async Task DrawChartAsync(Canvas2DContext context)
        {
            await AddCanvasBackGroundAsync(context);
            await AddCanvasGridXAsync(context);
            await AddCanvasGridYAsync(context);
            await AddXValuesLabelsAsync(context);

            await PlotGaussPointsAsync(context);
        }

        private async Task AddCanvasBackGroundAsync(Canvas2DContext context)
        {
            await context.SetStrokeStyleAsync("#000000");
            await context.SetLineWidthAsync(1);
            await context.RectAsync(0, 0, CanvasWidth, CanvasHeight);

            await context.SetFillStyleAsync(CanvasBackgroundColor);
            await context.FillRectAsync(UnitSizeX, UnitSizeY, CanvasWidth - UnitSizeX * 2, CanvasHeight - UnitSizeY * 2);
            await context.RectAsync(UnitSizeX, UnitSizeY, CanvasWidth - UnitSizeX * 2, CanvasHeight - UnitSizeY * 2);

            await context.StrokeAsync();
        }

        private async Task AddCanvasGridXAsync(Canvas2DContext context)
        {
            await context.SetLineWidthAsync(1);

            await context.BeginPathAsync();
            await context.SetStrokeStyleAsync(CanvasGridLinesColor);

            for (int i=1;i<UnitNumbersX-2;i++)
            {
                await context.MoveToAsync(UnitSizeX + i * UnitSizeX, UnitSizeY);
                await context.LineToAsync(UnitSizeX + i * UnitSizeX, CanvasHeight - UnitSizeY - 1);
            }

            await context.ClosePathAsync();
            await context.StrokeAsync();
        }

        private async Task AddCanvasGridYAsync(Canvas2DContext context)
        {
            await context.SetLineWidthAsync(1);

            await context.BeginPathAsync();
            await context.SetStrokeStyleAsync(CanvasGridLinesColor);

            for (var i=1;i<UnitNumbersY-2;i++)
            {
                await context.MoveToAsync(UnitSizeX + 1, UnitSizeY + i * UnitSizeY);
                await context.LineToAsync(CanvasWidth - UnitSizeX - 1, UnitSizeY + i * UnitSizeY);
            }

            await context.ClosePathAsync();
            await context.StrokeAsync();
        }

        private async Task AddXValuesLabelsAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();
            await context.SetFontAsync("12px Arial");
            await context.SetFillStyleAsync(ChartXValuesColor);

            foreach(var point in Points)
            {
                await context.FillTextAsync(point.X.ToString(), ConvertValueToPixelX(point.X) - 4, (CanvasHeight - UnitSizeY + 13));
            }

            await context.ClosePathAsync();
            await context.StrokeAsync();
        }

        private int ConvertValueToPixelX(double value)
        {
            var basePoint = CanvasWidth / 2;
            return (int) (value * UnitSizeX + basePoint);
        }

        private int ConvertValueToPixelY(double value)
        {
            return (int) ((CanvasHeight - UnitSizeY) - (value * UnitSizeY * 7));
        }

        private IEnumerable<ChartPoint> ConvertToCanvasCoordinate(IEnumerable<ChartPoint> points)
        {
            return points.Select(point => new ChartPoint(ConvertValueToPixelX(point.X), ConvertValueToPixelY(point.Y)));
        }

        private async Task PlotGaussPointsAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();
            await context.SetStrokeStyleAsync(GaussCurveColor);

            // move to the first point
            await context.MoveToAsync(UnitSizeX, CanvasHeight - UnitSizeY);
            var pointArray = ConvertToCanvasCoordinate(Points).ToArray();

            var i = 1;
            for (i = 1; i < pointArray.Length - 2; i ++)
            {
                var xc = (pointArray[i].X + pointArray[i + 1].X) / 2;
                var yc = (pointArray[i].Y + pointArray[i + 1].Y) / 2;
                
                await context.QuadraticCurveToAsync(pointArray[i].X, pointArray[i].Y, xc, yc);
            }

            // curve through the last two points
            await context.QuadraticCurveToAsync(pointArray[i].X, pointArray[i].Y, pointArray[i+1].X, pointArray[i+1].Y);            

            await context.ClosePathAsync();
            await context.StrokeAsync();
        }

        private async Task PlotCriticalValuesAsync(Canvas2DContext context, double value)
        {
            int x = ConvertValueToPixelX(value);
            int minusX = ConvertValueToPixelX(-1 * value);
            int y = ConvertValueToPixelY(0.4);

            await context.BeginPathAsync();
            await context.SetStrokeStyleAsync(CriticalValueLinesColor);

            await context.MoveToAsync(minusX, (CanvasHeight - UnitSizeY));
            await context.LineToAsync(minusX, y);

            await context.MoveToAsync(x, (CanvasHeight - UnitSizeY));
            await context.LineToAsync(x, y);

            await context.ClosePathAsync();
            await context.StrokeAsync();

            await WriteCritialValueLabels(context, value);
        }

        private async Task WriteCritialValueLabels(Canvas2DContext context, double value)
        {            
            double minusValue = value * -1;
            int x = ConvertValueToPixelX(value);
            int minusX = ConvertValueToPixelX(-1 * value);

            await context.BeginPathAsync();
            await context.SetFontAsync("12px Arial");
            await context.SetFillStyleAsync("#000000");

            await context.FillTextAsync(value.ToString(), x, ConvertValueToPixelY(0.4) - 12);
            await context.FillTextAsync(minusValue.ToString(), minusX, ConvertValueToPixelY(0.4) - 12);
            await context.ClosePathAsync();
            await context.StrokeAsync();
        }

        private async Task PlotZValueAsync(Canvas2DContext context, double criticalValue, double zValue)
        {            
            int x = ConvertValueToPixelX(zValue);
            int y = ConvertValueToPixelY(0.4);

            var lineColor = ((zValue > criticalValue * -1) && (zValue < criticalValue))?"#009688":"#d13438";

            var valuePrefix = "";
            if (zValue > 3.6 || zValue < -3.6)
            {
                valuePrefix = "...";
                var newXValue = zValue >= 0?3.6:-3.6;
                x = ConvertValueToPixelX(newXValue);

            }
 
            await context.BeginPathAsync();
            await context.MoveToAsync(x, (CanvasHeight - UnitSizeY));
            await context.LineToAsync(x, y);
            await context.ClosePathAsync();
            await context.SetStrokeStyleAsync(lineColor);
            await context.StrokeAsync();

            // Write z value
            await context.BeginPathAsync();
            await context.SetFontAsync("12px Arial");
            await context.SetFillStyleAsync("#000000");
            await context.FillTextAsync(valuePrefix + zValue.ToString(), x, ConvertValueToPixelY(0.4) - 12);
            await context.ClosePathAsync();
            await context.StrokeAsync();           
        }
    }
}