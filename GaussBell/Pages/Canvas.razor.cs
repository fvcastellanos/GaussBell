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

        protected const int PixelSizeX = (CanvasWidth - UnitSizeX * 2) / TestPoints;

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

            await PlotGaussPointsAsync(_context);
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

        private int ConvertValueToPixelX(double value)
        {
            var basePoint = CanvasWidth / 2;

            // return (int) (value * PixelSizeX + basePoint);
            return (int) (value * UnitSizeX + basePoint);
        }

        private int ConvertValueToPixelY(double value)
        {
            return (int) ((CanvasHeight - UnitSizeY) - (value * UnitSizeY * 5));
        }

        private IEnumerable<ChartPoint> ConvertToCanvasCoordinate(IEnumerable<ChartPoint> points)
        {
            return points.Select(point => new ChartPoint(ConvertValueToPixelX(point.X), ConvertValueToPixelY(point.Y)));
        }

        private async Task PlotGaussPointsAsync(Canvas2DContext context)
        {
            await context.BeginPathAsync();
            await context.SetStrokeStyleAsync("#121e5e");

            var stack = new Stack<ChartPoint>();
            await context.MoveToAsync(UnitSizeX, CanvasHeight - UnitSizeY);
            var convertedPoints = ConvertToCanvasCoordinate(Points);

            foreach(var point in convertedPoints)   
            {
                await context.LineToAsync(point.X, point.Y);
            }

            await context.ClosePathAsync();
            await context.StrokeAsync();

            await context.MoveToAsync(UnitSizeX, CanvasHeight - UnitSizeY);
            await context.QuadraticCurveToAsync(CanvasWidth / 2, 100, CanvasWidth - UnitSizeX, CanvasHeight - UnitSizeY);
            await context.StrokeAsync();
        }
    }
}