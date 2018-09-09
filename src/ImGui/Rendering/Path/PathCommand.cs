﻿using System.Diagnostics;
using ImGui.Common.Primitive;
using ImGui.Development.DebuggerViews;

namespace ImGui.Rendering
{
    internal class MoveToCommand : PathCommand
    {
        public Point Point { get; set;}

        public MoveToCommand(Point point) : base(PathCommandType.PathMoveTo)
        {
            this.Point = point;
        }
    }
    
    internal class LineToCommand : PathCommand
    {
        public Point Point { get; set;}

        public LineToCommand(Point point) : base(PathCommandType.PathLineTo)
        {
            this.Point = point;
        }
    }

    internal class CurveToCommand : PathCommand
    {
        public Point ControlPoint0 { get; set;}
        public Point ControlPoint1 { get; set;}
        public Point EndPoint { get; set;}

        public CurveToCommand(Point control0, Point control1, Point end) : base(PathCommandType.PathCurveTo)
        {
            this.ControlPoint0 = control0;
            this.ControlPoint1 = control1;
            this.EndPoint = end;
        }
    }

    internal class ClosePathCommand : PathCommand
    {
        public ClosePathCommand() : base(PathCommandType.PathClosePath)
        {
        }
    }

    internal class StrokeCommand : PathCommand
    {
        public double LineWidth { get; set;}
        public Color Color { get; set;}
        public StrokeCommand(double lineWidh, Color lineColor) : base(PathCommandType.Stroke)
        {
            this.LineWidth = lineWidh;
            this.Color = lineColor;
        }
    }

    internal class FillCommand : PathCommand
    {
        public Color Color { get; set; }
        public FillCommand(Color fillColor) : base(PathCommandType.Fill)
        {
            this.Color = fillColor;
        }
    }

    [DebuggerTypeProxy(typeof(PathCommandDebuggerView))]
    internal class PathCommand
    {
        public PathCommandType Type { get; set; }

        protected PathCommand(PathCommandType type)
        {
            this.Type = type;
        }
    }
}
