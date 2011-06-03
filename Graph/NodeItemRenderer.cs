﻿#region License
// Copyright (c) 2009 Sander van Rossen
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Composition;

namespace Graph
{
	[InheritedExport]
	public abstract class NodeItemRenderer<T> : INodeItemRenderer
		where T : NodeItem
	{
		public abstract SizeF Measure(IDeviceContext context, SizeF minimumSize, T item);
		public abstract void Render(Graphics graphics, SizeF minimumSize, T item, PointF location);

		SizeF INodeItemRenderer.Measure(IDeviceContext context, SizeF minimumSize, NodeItem item) { return Measure(context, minimumSize, item as T); }
		void INodeItemRenderer.Render(Graphics graphics, SizeF minimumSize, NodeItem item, PointF location) { Render(graphics, minimumSize, item as T, location); }
	}
}
