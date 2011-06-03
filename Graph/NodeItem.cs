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
using System.ComponentModel.Composition;

namespace Graph
{
	public class NodeItemEventArgs : EventArgs
	{
		public NodeItemEventArgs(NodeItem item) { Item = item; }
		public NodeItem Item { get; private set; }
	}

	public abstract class NodeItem
	{
		public NodeItem()
		{
			this.Input		= new NodeConnector(this, false);
			this.Output		= new NodeConnector(this, false);
		}

		public NodeItem(bool enableInput, bool enableOutput)
		{
			this.Input		= new NodeConnector(this, enableInput);
			this.Output		= new NodeConnector(this, enableOutput);
		}

		public Node					Node			{ get; internal set; }
		public object				Tag				{ get; set; }

		public NodeConnector		Input			{ get; private set; }
		public NodeConnector		Output			{ get; private set; }

		internal RectangleF			bounds;

		public virtual bool			OnClick()		{ return false; }
		public virtual bool			OnEnter()		{ return false; }
		public virtual bool			OnLeave()		{ return false; }
		public virtual bool			OnStartDrag(PointF location) { return false; }
		public virtual bool			OnDrag(PointF location)		 { return false; }		
		public virtual bool			OnEndDrag() 				 { return false; }
	}
}
