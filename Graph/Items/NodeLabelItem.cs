using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Graph.Items
{
	public sealed class NodeLabelItem : NodeItem
	{
		public NodeLabelItem(string text, bool inputEnabled, bool outputEnabled) :
			base(inputEnabled, outputEnabled)
		{
			this.Text = text;
		}

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

					this.TextSize.Width  = Math.Max(size.Width, this.TextSize.Width);
					this.TextSize.Height = Math.Max(size.Height, this.TextSize.Height);
				}
				return this.TextSize;
			} else
			{
				return new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);
			}
		}

		internal override void Render(Graphics graphics, SizeF minimumSize, PointF location)
		{
			var size = Measure(graphics);
			size.Width  = Math.Max(minimumSize.Width, size.Width);
			size.Height = Math.Max(minimumSize.Height, size.Height);

			if (this.Input.Enabled != this.Output.Enabled)
			{
				if (this.Input.Enabled)
					graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, new RectangleF(location, size), GraphConstants.LeftTextStringFormat);
				else
					graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, new RectangleF(location, size), GraphConstants.RightTextStringFormat);
			} else
				graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, new RectangleF(location, size), GraphConstants.CenterTextStringFormat);
		}
	}
}
