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

		#region Hover
		internal bool Hover { get; set; }
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
	}

	[NodeItemDescription(typeof(NodeSliderItem))]
	public class NodeSliderRenderer : NodeItemRenderer<NodeSliderItem>
	{
		const int SliderBoxSize = 4;
		const int SliderHeight	= 8;
		const int Spacing		= 2;
		public override SizeF Measure(IDeviceContext context, SizeF minimumSize, NodeSliderItem item)
		{
			if (!string.IsNullOrWhiteSpace(item.Text))
			{
				if (item.itemSize.IsEmpty)
				{
					var size = new Size(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);
					var sliderWidth = item.MinimumSliderSize + SliderBoxSize;

					item.textSize = (SizeF)TextRenderer.MeasureText(context, item.Text, SystemFonts.MenuFont, size, GraphConstants.LeftTextFlags);
					item.textSize.Width		= Math.Max(item.TextSize, item.textSize.Width + 4);
					item.itemSize.Width	= Math.Max(size.Width, item.textSize.Width + sliderWidth + Spacing);
					item.itemSize.Height	= Math.Max(size.Height, item.textSize.Height);
				}
				var measuredSize = item.itemSize;
				measuredSize.Width	= Math.Max(minimumSize.Width, measuredSize.Width);
				measuredSize.Height = Math.Max(minimumSize.Height, measuredSize.Height);
				return measuredSize;
			} else
			{
				var measuredSize = new SizeF(GraphConstants.MinimumItemWidth, GraphConstants.MinimumItemHeight);
				measuredSize.Width  = Math.Max(minimumSize.Width, measuredSize.Width);
				measuredSize.Height = Math.Max(minimumSize.Height, measuredSize.Height);
				return measuredSize;
			}
		}

		public override void Render(Graphics graphics, SizeF minimumSize, NodeSliderItem item, PointF location)
		{
			var size			= Measure(graphics, minimumSize, item);
			var sliderOffset	= Spacing + item.textSize.Width;
			var sliderWidth		= size.Width - (Spacing + item.textSize.Width);

			var textRect	= new RectangleF(location, size);
			var sliderBox	= new RectangleF(location, size);
			var sliderRect	= new RectangleF(location, size);
			sliderRect.X =		 sliderRect.Right - sliderWidth;
			sliderRect.Y		+= ((sliderRect.Bottom - sliderRect.Top) - SliderHeight) / 2.0f;
			sliderRect.Width	= sliderWidth;
			sliderRect.Height	= SliderHeight;
			textRect.Width -= sliderWidth + Spacing;

			var valueSize = (item.MaxValue - item.MinValue);
			item.sliderRect = sliderRect;
			item.sliderRect.Width -= SliderBoxSize;
			item.sliderRect.X += SliderBoxSize / 2.0f;

			sliderBox.Width = SliderBoxSize;
			sliderBox.X = sliderRect.X + (item.Value * item.sliderRect.Width) / valueSize;

			graphics.DrawString(item.Text, SystemFonts.MenuFont, Brushes.Black, textRect, GraphConstants.LeftTextStringFormat);

			using (var path = NodeUtility.CreateRoundedRectangle(sliderRect.Size, sliderRect.Location))
			{
				if (item.Hover || item.Dragging)
					graphics.DrawPath(Pens.White, path);
				else
					graphics.DrawPath(Pens.Black, path);
			}

			graphics.FillRectangle(Brushes.LightGray, sliderBox.X, sliderBox.Y, sliderBox.Width, sliderBox.Height);

			if (item.Hover || item.Dragging)
				graphics.DrawRectangle(Pens.White, sliderBox.X, sliderBox.Y, sliderBox.Width, sliderBox.Height);
			else
				graphics.DrawRectangle(Pens.Black, sliderBox.X, sliderBox.Y, sliderBox.Width, sliderBox.Height);
		}
	}
}
