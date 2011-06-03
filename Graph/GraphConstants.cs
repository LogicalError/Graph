using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Graph
{
	public static class GraphConstants
	{
		public const int MinimumItemWidth		= 64+8;
		public const int MinimumItemHeight		= 16;
		public const int TitleHeight			= 12;
		public const int ItemSpacing			= 3;
		public const int TopHeight				= 4;
		public const int BottomHeight			= 4;
		public const int CornerSize				= 4;
		public const int ConnectorSize			= 8;
		public const int HorizontalSpacing		= 2;
		public const int NodeExtraWidth			= ((int)GraphConstants.ConnectorSize + (int)GraphConstants.HorizontalSpacing) * 2;
		
		internal const TextFormatFlags TitleTextFlags	=	TextFormatFlags.ExternalLeading |
															TextFormatFlags.GlyphOverhangPadding |
															TextFormatFlags.HorizontalCenter |
															TextFormatFlags.NoClipping |
															TextFormatFlags.NoPadding |
															TextFormatFlags.NoPrefix |
															TextFormatFlags.VerticalCenter;

		internal const TextFormatFlags CenterTextFlags	=	TextFormatFlags.ExternalLeading |
															TextFormatFlags.GlyphOverhangPadding |
															TextFormatFlags.HorizontalCenter |
															TextFormatFlags.NoClipping |
															TextFormatFlags.NoPadding |
															TextFormatFlags.NoPrefix |
															TextFormatFlags.VerticalCenter;

		internal const TextFormatFlags LeftTextFlags	=	TextFormatFlags.ExternalLeading |
															TextFormatFlags.GlyphOverhangPadding |
															TextFormatFlags.Left |
															TextFormatFlags.NoClipping |
															TextFormatFlags.NoPadding |
															TextFormatFlags.NoPrefix |
															TextFormatFlags.VerticalCenter;

		internal const TextFormatFlags RightTextFlags	=	TextFormatFlags.ExternalLeading |
															TextFormatFlags.GlyphOverhangPadding |
															TextFormatFlags.Right |
															TextFormatFlags.NoClipping |
															TextFormatFlags.NoPadding |
															TextFormatFlags.NoPrefix |
															TextFormatFlags.VerticalCenter;

		internal static readonly StringFormat TitleStringFormat;
		internal static readonly StringFormat CenterTextStringFormat;
		internal static readonly StringFormat LeftTextStringFormat;
		internal static readonly StringFormat RightTextStringFormat;

		static GraphConstants()
		{
			TitleStringFormat					 = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
			TitleStringFormat.Alignment			 = StringAlignment.Center;
			TitleStringFormat.LineAlignment		 = StringAlignment.Center;
			TitleStringFormat.Trimming			 = StringTrimming.EllipsisCharacter;

			CenterTextStringFormat				 = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
			CenterTextStringFormat.Alignment	 = StringAlignment.Center;
			CenterTextStringFormat.LineAlignment = StringAlignment.Center;
			CenterTextStringFormat.Trimming		 = StringTrimming.EllipsisCharacter;

			LeftTextStringFormat				 = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
			LeftTextStringFormat.Alignment		 = StringAlignment.Near;
			LeftTextStringFormat.LineAlignment	 = StringAlignment.Center;
			LeftTextStringFormat.Trimming		 = StringTrimming.EllipsisCharacter;

			RightTextStringFormat				 = new StringFormat(StringFormatFlags.NoClip | StringFormatFlags.NoWrap);
			RightTextStringFormat.Alignment		 = StringAlignment.Far;
			RightTextStringFormat.LineAlignment	 = StringAlignment.Center;
			RightTextStringFormat.Trimming		 = StringTrimming.EllipsisCharacter;
		}
	}
}
