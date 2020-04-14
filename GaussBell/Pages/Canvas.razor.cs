using System.Threading.Tasks;
using Blazor.Extensions;
using Blazor.Extensions.Canvas.Canvas2D;
using Microsoft.AspNetCore.Components;

using GaussBell.Services;
using System.Collections.Generic;
using GaussBell.Services.Domain;
using System;
using System.Linq;

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

        protected BECanvasComponent _canvasReference;
        
        private Canvas2DContext _context;

        protected IEnumerable<ChartPoint> Points;

        protected string PointsStr;
        protected string ConvertedPointsStr;

        protected override void OnInitialized()
        {
            Points = ChartPointService.BuildChartPoints(MinX, TestPoints);
            PointsStr = ChartPointService.PointsToString(Points);
            ConvertedPointsStr = ChartPointService.PointsToString(ConvertToCanvasCoordinate(Points));
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _context = await _canvasReference.CreateCanvas2DAsync();

            await AddCanvasBackGroundAsync(_context);
            await AddCanvasGridXAsync(_context);
            await AddCanvasGridYAsync(_context);
            await AddXValuesLabelsAsync(_context);

            await PlotGaussPointsAsync(_context);
            await PlotCriticPointAsync(_context, 1.65);
        }

        private async Task AddCanvasBackGroundAsync(Canvas2DContext context)
        {
            await context.RectAsync(0, 0, CanvasWidth, CanvasHeight);

            await context.SetFillStyleAsync("#d6dbd7");
            await context.FillRectAsync(UnitSizeX, UnitSizeY, CanvasWidth - UnitSizeX * 2, CanvasHeight - UnitSizeY * 2);
            await context.RectAsync(UnitSizeX, UnitSizeY, CanvasWidth - UnitSizeX * 2, CanvasHeight - UnitSizeY * 2);

            await context.StrokeAsync();
        }

        private async Task AddCanvasGridXAsync(Canvas2DContext context)
        {
            await context.SetLineWidthAsync(1);

            await context.BeginPathAsync();
            await context.SetStrokeStyleAsync("#fdfdfd");

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
            await context.SetStrokeStyleAsync("#fdfdfd");

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
            await context.SetFillStyleAsync("#000000");

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
            await context.SetStrokeStyleAsync("#121e5e");

            // move to the first point
            await context.MoveToAsync(UnitSizeX, CanvasHeight - UnitSizeY);
            var convertedPoints = ConvertToCanvasCoordinate(Points);
            var array = ConvertToCanvasCoordinate(Points).ToArray();

            var i = 1;
            for (i = 1; i < array.Length - 2; i ++)
            {
                var xc = (array[i].X + array[i + 1].X) / 2;
                var yc = (array[i].Y + array[i + 1].Y) / 2;
                
                await context.QuadraticCurveToAsync(array[i].X, array[i].Y, xc, yc);
            }

            // curve through the last two points
            await context.QuadraticCurveToAsync(array[i].X, array[i].Y, array[i+1].X, array[i+1].Y);            

            await context.StrokeAsync();
        }

        private async Task PlotCriticPointAsync(Canvas2DContext context, double value)
        {
            int x = ConvertValueToPixelX(value);
            int minusX = ConvertValueToPixelX(-1 * value);
            int y = ConvertValueToPixelY(0.4);

            await context.BeginPathAsync();
            await context.SetStrokeStyleAsync("#3a0647");

            await context.MoveToAsync(minusX, (CanvasHeight - UnitSizeY));
            await context.LineToAsync(minusX, y);

            await context.SetStrokeStyleAsync("#3a0647");
            await context.MoveToAsync(x, (CanvasHeight - UnitSizeY));
            await context.LineToAsync(x, y);

            await context.SetFontAsync("12px Arial");
            await context.SetFillStyleAsync("#000000");

            await context.FillTextAsync(value.ToString(), x, ConvertValueToPixelY(0.4) - 12);
            await context.FillTextAsync((-1 * value).ToString(), minusX, ConvertValueToPixelY(0.4) - 12);

            await context.ClosePathAsync();
            await context.StrokeAsync();
        }
    }
}