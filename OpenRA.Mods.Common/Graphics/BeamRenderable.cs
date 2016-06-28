#region Copyright & License Information
/*
 * Copyright 2007-2016 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenRA.Graphics;

namespace OpenRA.Mods.Common.Graphics
{
	public enum BeamRenderableShape { Cylindrical, Flat, Sine }
	public struct BeamRenderable : IRenderable, IFinalizedRenderable
	{
		readonly WPos pos;
		readonly int zOffset;
		readonly WVec length;
		readonly BeamRenderableShape shape;
		readonly WDist width;
		readonly Color color;

		public BeamRenderable(WPos pos, int zOffset, WVec length, BeamRenderableShape shape, WDist width, Color color)
		{
			this.pos = pos;
			this.zOffset = zOffset;
			this.length = length;
			this.shape = shape;
			this.width = width;
			this.color = color;
		}

		public WPos Pos { get { return pos; } }
		public PaletteReference Palette { get { return null; } }
		public int ZOffset { get { return zOffset; } }
		public bool IsDecoration { get { return true; } }

		public IRenderable WithPalette(PaletteReference newPalette) { return new BeamRenderable(pos, zOffset, length, shape, width, color); }
		public IRenderable WithZOffset(int newOffset) { return new BeamRenderable(pos, zOffset, length, shape, width, color); }
		public IRenderable OffsetBy(WVec vec) { return new BeamRenderable(pos + vec, zOffset, length, shape, width, color); }
		public IRenderable AsDecoration() { return this; }

		public IFinalizedRenderable PrepareRender(WorldRenderer wr) { return this; }
		public void Render(WorldRenderer wr)
		{
			var vecLength = length.Length;
			if (vecLength == 0)
				return;

			if (shape == BeamRenderableShape.Flat)
			{
				var delta = length * width.Length / (2 * vecLength);
				var corner = new WVec(-delta.Y, delta.X, delta.Z);
				var a = wr.ScreenPosition(pos - corner);
				var b = wr.ScreenPosition(pos + corner);
				var c = wr.ScreenPosition(pos + corner + length);
				var d = wr.ScreenPosition(pos - corner + length);
				Game.Renderer.WorldRgbaColorRenderer.FillRect(a, b, c, d, color);
			}
			else if (shape == BeamRenderableShape.Cylindrical)
			{
				var start = wr.ScreenPosition(pos);
				var end = wr.ScreenPosition(pos + length);
				var screenWidth = wr.ScreenVector(new WVec(width, WDist.Zero, WDist.Zero))[0];
				Game.Renderer.WorldRgbaColorRenderer.DrawLine(start, end, screenWidth, color);
			}
			else if (shape == BeamRenderableShape.Sine)
			{
				var targetPos = pos + length;

				//var cyclesPerWRange = 1;
				var pointsPerWRange = 1;
				//var numOfWRangeSteps = 2048;

				var amplitudeScale = 1;//numOfWRangeSteps / 2048;
				var phaseScale = 1;//cyclesPerWRange / pointsPerWRange;
				var totalPoints = (targetPos - pos).Length * pointsPerWRange;

				var scaleDivisor = 4;

				var points = new List<WPos>();
				for (var i = 0; i < totalPoints; i++)
					points.Add(WPos.Lerp(pos, targetPos, i, totalPoints) + new WVec(0, 0, amplitudeScale * new WAngle(i * phaseScale).Sin() / scaleDivisor));

				var screenPoints = points.Select(p => wr.ScreenPosition(p));
				Game.Renderer.WorldRgbaColorRenderer.DrawLine(screenPoints, 2f, color, true);
			}
		}

		public void RenderDebugGeometry(WorldRenderer wr) { }
		public Rectangle ScreenBounds(WorldRenderer wr) { return Rectangle.Empty; }
	}
}
