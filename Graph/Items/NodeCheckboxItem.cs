using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Graph.Items
{
	public sealed class NodeCheckboxItem : NodeItem
	{
		public NodeCheckboxItem(string text, bool inputEnabled, bool outputEnabled) :
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

		#region Checked
		bool internalChecked = false;
		public bool Checked
		{
			get { return internalChecked; }
			set
			{
				if (internalChecked == value)
					return;
				internalChecked = value;
				TextSize = Size.Empty;
			}
		}
		#endregion

		#region Hover
		internal bool Hover { get; set; }
		#endregion

		public override bool OnEnter()
		{
			base.OnEnter();
			Hover = true;
			return true;
		}

		public override bool OnLeave()
		{
			base.OnLeave();
			Hover = false;
			return true;
		}

		public override bool OnClick()
		{
			base.OnClick();
			Checked = !Checked;
			return true;
		}

		internal SizeF TextSize;


		internal override SizeF Measure(IDeviceContext context)
		{
			if (!string.IsNullOrWhiteSpace(this.Text))
			{
				if (this.TextSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);

					this.TextSize = TextRenderer.MeasureText(context, this.Text, SystemFonts.MenuFont, size, GraphConstants.CenterTextFlags);

					this.TextSize.Width	 = Math.Max(size.Width, this.TextSize.Width);
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
			
			using (var path = NodeUtility.CreateRoundedRectangle(size, location))
			{
				var rect = new RectangleF(location, size);
				if (this.Checked)
				{
					using (var brush = new SolidBrush(Color.FromArgb(128+32, Color.White)))
					{
						graphics.FillPath(brush, path);
					}
				} else
				{
					using (var brush = new SolidBrush(Color.FromArgb(64, Color.Black)))
					{
						graphics.FillPath(brush, path);
					}
				}
				graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, rect, GraphConstants.CenterTextStringFormat);

				if (this.Hover)
					graphics.DrawPath(Pens.White, path);
				else	
					graphics.DrawPath(Pens.Black, path);
			}
		}
	}
}
