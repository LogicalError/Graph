using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Graph.Items
{
	public sealed class NodeSliderItem : NodeItem
	{
		public event EventHandler<NodeItemEventArgs> Clicked;
		public event EventHandler<NodeItemEventArgs> ValueChanged;

		public NodeSliderItem(string text, float sliderSize, float textSize, float minValue, float maxValue, float defaultValue, bool inputEnabled, bool outputEnabled) :
			base(inputEnabled, outputEnabled)
		{
			this.Text = text;
			this.MinimumSliderSize = sliderSize;
			this.TextSize = textSize;
			this.MinValue = Math.Min(minValue, maxValue);
			this.MaxValue = Math.Max(minValue, maxValue);
			this.Value = defaultValue;
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
				itemSize = Size.Empty;
			}
		}
		#endregion

		#region Dragging
		internal bool Dragging { get; set; }
		#endregion

		public float MinimumSliderSize	{ get; set; }
		public float TextSize			{ get; set; }

		public float MinValue { get; set; }
		public float MaxValue { get; set; }

		float internalValue = 0.0f;
		public float Value				
		{
			get { return internalValue; }
			set
			{
				var newValue = value;
				if (newValue < MinValue) newValue = MinValue;
				if (newValue > MaxValue) newValue = MaxValue;
				if (internalValue == newValue)
					return;
				internalValue = newValue;
				if (ValueChanged != null)
					ValueChanged(this, new NodeItemEventArgs(this));
			}
		}


		public override bool OnClick()
		{
			base.OnClick();
			if (Clicked != null)
				Clicked(this, new NodeItemEventArgs(this));
			return true;
		}

		public override bool OnStartDrag(PointF location) 
		{
			base.OnStartDrag(location);
			var size = (MaxValue - MinValue);
			Value = ((location.X - sliderRect.Left) / sliderRect.Width) * size;
			Dragging = true; 
			return true; 
		}

		public override bool OnDrag(PointF location) 
		{
			base.OnDrag(location);
			var size = (MaxValue - MinValue);
			Value = ((location.X - sliderRect.Left) / sliderRect.Width) * size;
			return true; 
		}

		public override bool OnEndDrag() { base.OnEndDrag(); Dragging = false; return true; }


		internal SizeF itemSize;
		internal SizeF textSize;
		internal RectangleF sliderRect;

		
		const int SliderBoxSize = 4;
		const int SliderHeight	= 8;
		const int Spacing		= 2;

		internal override SizeF Measure(IDeviceContext context)
		{
			if (!string.IsNullOrWhiteSpace(this.Text))
			{
				if (this.itemSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);
					var sliderWidth = this.MinimumSliderSize + SliderBoxSize;

					this.textSize = (SizeF)TextRenderer.MeasureText(context, this.Text, SystemFonts.MenuFont, size, GraphConstants.LeftTextFlags);
					this.textSize.Width		= Math.Max(this.TextSize, this.textSize.Width + 4);
					this.itemSize.Width		= Math.Max(size.Width, this.textSize.Width + sliderWidth + Spacing);
					this.itemSize.Height	= Math.Max(size.Height, this.textSize.Height);
				}
				return this.itemSize;
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

			var sliderOffset	= Spacing + this.textSize.Width;
			var sliderWidth		= size.Width - (Spacing + this.textSize.Width);

			var textRect	= new RectangleF(location, size);
			var sliderBox	= new RectangleF(location, size);
			var sliderRect	= new RectangleF(location, size);
			sliderRect.X =		 sliderRect.Right - sliderWidth;
			sliderRect.Y		+= ((sliderRect.Bottom - sliderRect.Top) - SliderHeight) / 2.0f;
			sliderRect.Width	= sliderWidth;
			sliderRect.Height	= SliderHeight;
			textRect.Width -= sliderWidth + Spacing;

			var valueSize = (this.MaxValue - this.MinValue);
			this.sliderRect = sliderRect;
			this.sliderRect.Width -= SliderBoxSize;
			this.sliderRect.X += SliderBoxSize / 2.0f;

			sliderBox.Width = SliderBoxSize;
			sliderBox.X = sliderRect.X + (this.Value * this.sliderRect.Width) / valueSize;

			graphics.DrawString(this.Text, SystemFonts.MenuFont, Brushes.Black, textRect, GraphConstants.LeftTextStringFormat);

			using (var path = GraphRenderer.CreateRoundedRectangle(sliderRect.Size, sliderRect.Location))
			{
				if ((state & (RenderState.Hover | RenderState.Dragging)) != 0)
					graphics.DrawPath(Pens.White, path);
				else
					graphics.DrawPath(Pens.Black, path);
			}

			graphics.FillRectangle(Brushes.LightGray, sliderBox.X, sliderBox.Y, sliderBox.Width, sliderBox.Height);

			if ((state & (RenderState.Hover | RenderState.Dragging)) != 0)
				graphics.DrawRectangle(Pens.White, sliderBox.X, sliderBox.Y, sliderBox.Width, sliderBox.Height);
			else
				graphics.DrawRectangle(Pens.Black, sliderBox.X, sliderBox.Y, sliderBox.Width, sliderBox.Height);
		}
	}
}
