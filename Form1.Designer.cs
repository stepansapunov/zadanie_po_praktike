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
            btnOpen = new Button();
            txtInfo = new TextBox();
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            SuspendLayout();
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(129, 154);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(276, 33);
            btnOpen.TabIndex = 0;
            btnOpen.Text = "Открыть файл COMTRADE";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click_1;
            // 
            // txtInfo
            // 
            txtInfo.Location = new Point(670, 109);
            txtInfo.Multiline = true;
            txtInfo.Name = "txtInfo";
            txtInfo.Size = new Size(559, 585);
            txtInfo.TabIndex = 1;
            // 
            // formsPlot1
            // 
            formsPlot1.Location = new Point(1342, 170);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(1007, 490);
            formsPlot1.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2396, 822);
            Controls.Add(formsPlot1);
            Controls.Add(txtInfo);
            Controls.Add(btnOpen);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnOpen;
        private TextBox txtInfo;
        private ScottPlot.WinForms.FormsPlot formsPlot1;
    }
}
