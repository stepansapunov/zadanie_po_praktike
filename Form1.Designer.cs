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
            SuspendLayout();
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(340, 79);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(259, 34);
            btnOpen.TabIndex = 0;
            btnOpen.Text = "Открыть файл COMTRADE";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click_1;
            // 
            // txtInfo
            // 
            txtInfo.Location = new Point(67, 207);
            txtInfo.Multiline = true;
            txtInfo.Name = "txtInfo";
            txtInfo.Size = new Size(803, 486);
            txtInfo.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1033, 792);
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
    }
}
