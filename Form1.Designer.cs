namespace Osnovnoi_proekt
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            splitContainer1 = new SplitContainer();
            clbSignals = new CheckedListBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            выполнитьРасчетТКЗToolStripMenuItem = new ToolStripMenuItem();
            txtInfo = new TextBox();
            panel1 = new Panel();
            btnSave = new Button();
            btnOpen = new Button();
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            tabPage2 = new TabPage();
            splitContainer2 = new SplitContainer();
            formsPlotCurrents = new ScottPlot.WinForms.FormsPlot();
            formsPlotVoltages = new ScottPlot.WinForms.FormsPlot();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            panel1.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            SuspendLayout();
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 0);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(2358, 891);
            tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(splitContainer1);
            tabPage1.Location = new Point(4, 34);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(2350, 853);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Осциллограммы";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(3, 3);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(clbSignals);
            splitContainer1.Panel1.Controls.Add(txtInfo);
            splitContainer1.Panel1.Controls.Add(panel1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(formsPlot1);
            splitContainer1.Size = new Size(2344, 847);
            splitContainer1.SplitterDistance = 453;
            splitContainer1.TabIndex = 0;
            // 
            // clbSignals
            // 
            clbSignals.CheckOnClick = true;
            clbSignals.ContextMenuStrip = contextMenuStrip1;
            clbSignals.Dock = DockStyle.Fill;
            clbSignals.FormattingEnabled = true;
            clbSignals.Location = new Point(0, 179);
            clbSignals.Name = "clbSignals";
            clbSignals.Size = new Size(453, 0);
            clbSignals.TabIndex = 13;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(24, 24);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { выполнитьРасчетТКЗToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(267, 36);
            // 
            // выполнитьРасчетТКЗToolStripMenuItem
            // 
            выполнитьРасчетТКЗToolStripMenuItem.Name = "выполнитьРасчетТКЗToolStripMenuItem";
            выполнитьРасчетТКЗToolStripMenuItem.Size = new Size(266, 32);
            выполнитьРасчетТКЗToolStripMenuItem.Text = "Выполнить расчет ТКЗ";
            выполнитьРасчетТКЗToolStripMenuItem.Click += выполнитьРасчетТКЗToolStripMenuItem_Click;
            // 
            // txtInfo
            // 
            txtInfo.ContextMenuStrip = contextMenuStrip1;
            txtInfo.Dock = DockStyle.Bottom;
            txtInfo.Location = new Point(0, 135);
            txtInfo.Multiline = true;
            txtInfo.Name = "txtInfo";
            txtInfo.Size = new Size(453, 712);
            txtInfo.TabIndex = 12;
            // 
            // panel1
            // 
            panel1.Controls.Add(btnSave);
            panel1.Controls.Add(btnOpen);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(453, 179);
            panel1.TabIndex = 0;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(80, 71);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(276, 35);
            btnSave.TabIndex = 12;
            btnSave.Text = "Экспорт в COMTRADE";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(80, 19);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(276, 33);
            btnOpen.TabIndex = 11;
            btnOpen.Text = "Открыть файл COMTRADE";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click_1;
            // 
            // formsPlot1
            // 
            formsPlot1.ContextMenuStrip = contextMenuStrip1;
            formsPlot1.Dock = DockStyle.Fill;
            formsPlot1.Location = new Point(0, 0);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(1887, 847);
            formsPlot1.TabIndex = 10;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(splitContainer2);
            tabPage2.Location = new Point(4, 34);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(2350, 853);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Анализ ТКЗ";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(3, 3);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(formsPlotCurrents);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(formsPlotVoltages);
            splitContainer2.Size = new Size(2344, 847);
            splitContainer2.SplitterDistance = 430;
            splitContainer2.TabIndex = 0;
            // 
            // formsPlotCurrents
            // 
            formsPlotCurrents.ContextMenuStrip = contextMenuStrip1;
            formsPlotCurrents.Dock = DockStyle.Fill;
            formsPlotCurrents.Location = new Point(0, 0);
            formsPlotCurrents.Name = "formsPlotCurrents";
            formsPlotCurrents.Size = new Size(2344, 430);
            formsPlotCurrents.TabIndex = 0;
            // 
            // formsPlotVoltages
            // 
            formsPlotVoltages.ContextMenuStrip = contextMenuStrip1;
            formsPlotVoltages.Dock = DockStyle.Fill;
            formsPlotVoltages.Location = new Point(0, 0);
            formsPlotVoltages.Name = "formsPlotVoltages";
            formsPlotVoltages.Size = new Size(2344, 413);
            formsPlotVoltages.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2358, 891);
            Controls.Add(tabControl1);
            Name = "Form1";
            Text = "Form1";
            WindowState = FormWindowState.Maximized;
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            contextMenuStrip1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private SplitContainer splitContainer1;
        private SplitContainer splitContainer2;
        private ScottPlot.WinForms.FormsPlot formsPlotCurrents;
        private ScottPlot.WinForms.FormsPlot formsPlotVoltages;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem выполнитьРасчетТКЗToolStripMenuItem;
        private ScottPlot.WinForms.FormsPlot formsPlot1;
        private Panel panel1;
        private CheckedListBox clbSignals;
        private TextBox txtInfo;
        private Button btnOpen;
        private Button btnSave;
    }
}
