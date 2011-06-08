using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Graph.Items
{
	public sealed class NodeColorItem : NodeItem
	{
		public event EventHandler<NodeItemEventArgs> Clicked;

		public NodeColorItem(string text, Color color, bool inputEnabled, bool outputEnabled) :
			base(inputEnabled, outputEnabled)
		{
			this.Text = text;
			this.Color = color;
		}

		public Color Color { get; set; }

		#region Text
		string internalText = string.Empty;
		public string Text
		{
			get { return internalText; }
			set
			{
				if (internalText == value)
					return;
				internalText = value;
				TextSize = Size.Empty;
			}
		}
		#endregion

		internal SizeF TextSize;

		public override bool OnClick()
		{
			base.OnClick();
			if (Clicked != null)
				Clicked(this, new NodeItemEventArgs(this));
			return true;
		}

		
		const int ColorBoxSize = 16;
		const int Spacing = 2;
		
		internal override SizeF Measure(IDeviceContext context)
		{
			if (!string.IsNullOrWhiteSpace(this.Text))
			{
				if (this.TextSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);

					if (this.Input.Enabled != this.Output.Enabled)
					{
						if (this.Input.Enabled)
							this.TextSize = TextRenderer.MeasureText(context, this.Text, SystemFonts.MenuFont, size, GraphConstants.LeftTextFlags);
						else
							this.TextSize = TextRenderer.MeasureText(context, this.Text, SystemFonts.MenuFont, size, GraphConstants.RightTextFlags);
					} else
						this.TextSize = TextRenderer.MeasureText(context, this.Text, SystemFonts.MenuFont, size, GraphConstants.CenterTextFlags);

					this.TextSize.Width  = Math.Max(size.Width, this.TextSize.Width + ColorBoxSize + Spacing);
					this.TextSize.Height = Math.Max(size.Height, this.TextSize.Height);
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

			var alignment	= HorizontalAlignment.Center;
			var format		= GraphConstants.CenterTextStringFormat;
			if (this.Input.Enabled != this.Output.Enabled)
			{
				if (this.Input.Enabled)
				{
					alignment	= HorizontalAlignment.Left;
					format		= GraphConstants.LeftTextStringFormat;
				} else
				{
					alignment	= HorizontalAlignment.Right;
					format		= GraphConstants.RightTextStringFormat;
				}
			}

			var rect		= new RectangleF(location, size);
			var colorBox	= new RectangleF(location, size);
			colorBox.Width	= ColorBoxSize;
			switch (alignment)
			{
				case HorizontalAlignment.Left:
					rect.Width	-= ColorBoxSize + Spacing;
					rect.X		+= ColorBoxSize + Spacing;
					break;
				case HorizontalAlignment.Right:
					colorBox.X	= rect.Right - colorBox.Width;
					rect.Width	-= ColorBoxSize + Spacing;
					break;
				case HorizontalAlignment.Center:
					rect.Width	-= ColorBoxSize + Spacing;
					rect.X		+= ColorBoxSize + Spacing;
					break;
			}

			graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, rect, format);

			using (var path = GraphRenderer.CreateRoundedRectangle(colorBox.Size, colorBox.Location))
			{
				using (var brush = new SolidBrush(this.Color))
				{
					graphics.FillPath(brush, path);
				}
				if ((state & RenderState.Hover) != 0)
					graphics.DrawPath(Pens.White, path);
				else
					graphics.DrawPath(Pens.Black, path);
			}
			//using (var brush = new SolidBrush(this.Color))
			//{
			//	graphics.FillRectangle(brush, colorBox.X, colorBox.Y, colorBox.Width, colorBox.Height);
			//}
			//graphics.DrawRectangle(Pens.Black, colorBox.X, colorBox.Y, colorBox.Width, colorBox.Height);
		}
	}
}
