using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Graph.Items
{
	internal sealed class NodeTitleItem : NodeItem
	{
		#region Text
		string			internalTitle = string.Empty;
		public string	Title		
		{
			get { return internalTitle; }
			set
			{
				if (internalTitle == value)
					return;
				internalTitle = value;
				ForceResize();
			}
		}
		#endregion

		internal void ForceResize() { TextSize = Size.Empty; }
		internal SizeF				TextSize;
		
		internal override SizeF Measure(IDeviceContext context)
		{
			if (!string.IsNullOrWhiteSpace(this.Title))
			{
				if (this.TextSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.TitleHeight);
					this.TextSize			= TextRenderer.MeasureText(context, this.Title, SystemFonts.CaptionFont, size, GraphConstants.TitleTextFlags);

					this.TextSize.Width		= Math.Max(size.Width,  this.TextSize.Width + (GraphConstants.CornerSize * 2));
					this.TextSize.Height	= Math.Max(size.Height, this.TextSize.Height) + GraphConstants.TopHeight;
				}
				return this.TextSize;
			} else
			{
				return new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.TitleHeight + GraphConstants.TopHeight);
			}
		}

		internal override void Render(Graphics graphics, SizeF minimumSize, PointF location)
		{
			var size = Measure(graphics);
			size.Width  = Math.Max(minimumSize.Width, size.Width);
			size.Height = Math.Max(minimumSize.Height, size.Height);

			size.Height -= GraphConstants.TopHeight;
			location.Y += GraphConstants.TopHeight;

			if ((state & RenderState.Hover) == RenderState.Hover)
				graphics.DrawString(this.Title, SystemFonts.CaptionFont, Brushes.White, new RectangleF(location, size), GraphConstants.TitleStringFormat);
			else
				graphics.DrawString(this.Title, SystemFonts.CaptionFont, Brushes.Black, new RectangleF(location, size), GraphConstants.TitleStringFormat);
		}
	}
}
