namespace GraphNodes
{
	partial class ExampleForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Graph.Compatibility.AlwaysCompatible alwaysCompatible1 = new Graph.Compatibility.AlwaysCompatible();
			this.label1 = new System.Windows.Forms.Label();
			this.showLabelsCheckBox = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.nodeMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.testMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.nodeToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.graphControl = new Graph.GraphControl();
			this.nodeMenu.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 35);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(59, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "some node";
			this.label1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SomeNode_MouseDown);
			// 
			// showLabelsCheckBox
			// 
			this.showLabelsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.showLabelsCheckBox.AutoSize = true;
			this.showLabelsCheckBox.Location = new System.Drawing.Point(12, 373);
			this.showLabelsCheckBox.Name = "showLabelsCheckBox";
			this.showLabelsCheckBox.Size = new System.Drawing.Size(87, 17);
			this.showLabelsCheckBox.TabIndex = 2;
			this.showLabelsCheckBox.Text = "Show Labels";
			this.showLabelsCheckBox.UseVisualStyleBackColor = true;
			this.showLabelsCheckBox.CheckedChanged += new System.EventHandler(this.OnShowLabelsChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(9, 12);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(84, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "drag&drop nodes:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 58);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(66, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "texture node";
			this.label3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TextureNode_MouseDown);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 81);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(57, 13);
			this.label4.TabIndex = 5;
			this.label4.Text = "color node";
			this.label4.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ColorNode_MouseDown);
			// 
			// nodeMenu
			// 
			this.nodeMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testMenuItem});
			this.nodeMenu.Name = "NodeMenu";
			this.nodeMenu.Size = new System.Drawing.Size(97, 26);
			// 
			// testMenuItem
			// 
			this.testMenuItem.Name = "testMenuItem";
			this.testMenuItem.Size = new System.Drawing.Size(96, 22);
			this.testMenuItem.Text = "Test";
			// 
			// nodeToolTip
			// 
			this.nodeToolTip.ToolTipTitle = "Test";
			// 
			// graphControl
			// 
			this.graphControl.AllowDrop = true;
			this.graphControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.graphControl.BackColor = System.Drawing.SystemColors.Window;
			this.graphControl.CompatibilityStrategy = alwaysCompatible1;
			this.graphControl.FocusElement = null;
			this.graphControl.GridColor = System.Drawing.Color.LightGray;
			this.graphControl.HighlightCompatible = true;
			this.graphControl.Location = new System.Drawing.Point(108, 12);
			this.graphControl.Name = "graphControl";
			this.graphControl.ShowLabels = false;
			this.graphControl.Size = new System.Drawing.Size(678, 378);
			this.graphControl.TabIndex = 0;
			this.graphControl.Text = "graphControl";
			// 
			// ExampleForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(798, 402);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.showLabelsCheckBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.graphControl);
			this.DoubleBuffered = true;
			this.Name = "ExampleForm";
			this.Text = "Form1";
			this.nodeMenu.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Graph.GraphControl graphControl;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox showLabelsCheckBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ContextMenuStrip nodeMenu;
		private System.Windows.Forms.ToolStripMenuItem testMenuItem;
		private System.Windows.Forms.ToolTip nodeToolTip;
	}
}

