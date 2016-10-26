namespace LedWallInteract
{
    partial class WallSimulatorForm
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
            this.listViewPixels = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // listViewPixels
            // 
            this.listViewPixels.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewPixels.GridLines = true;
            this.listViewPixels.Location = new System.Drawing.Point(0, 0);
            this.listViewPixels.Name = "listViewPixels";
            this.listViewPixels.Size = new System.Drawing.Size(284, 261);
            this.listViewPixels.TabIndex = 0;
            this.listViewPixels.UseCompatibleStateImageBehavior = false;
            this.listViewPixels.View = System.Windows.Forms.View.Details;
            // 
            // WallSimulatorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.listViewPixels);
            this.Name = "WallSimulatorForm";
            this.Text = "WallSimulatorForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listViewPixels;
    }
}