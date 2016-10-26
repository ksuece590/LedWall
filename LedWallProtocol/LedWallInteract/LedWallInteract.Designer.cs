namespace LedWallInteract
{
    partial class LedWallInteract
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
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.btnChooseColor = new System.Windows.Forms.Button();
            this.txtColorDisplay = new System.Windows.Forms.TextBox();
            this.btnDoRainbow = new System.Windows.Forms.Button();
            this.btnLoadImage = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnLoadVideo = new System.Windows.Forms.Button();
            this.btnSort = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // colorDialog
            // 
            this.colorDialog.Color = System.Drawing.Color.Purple;
            // 
            // btnChooseColor
            // 
            this.btnChooseColor.Location = new System.Drawing.Point(153, 12);
            this.btnChooseColor.Name = "btnChooseColor";
            this.btnChooseColor.Size = new System.Drawing.Size(119, 23);
            this.btnChooseColor.TabIndex = 0;
            this.btnChooseColor.Text = "Choose Color";
            this.btnChooseColor.UseVisualStyleBackColor = true;
            this.btnChooseColor.Click += new System.EventHandler(this.btnChooseColor_Click);
            // 
            // txtColorDisplay
            // 
            this.txtColorDisplay.Location = new System.Drawing.Point(12, 12);
            this.txtColorDisplay.Name = "txtColorDisplay";
            this.txtColorDisplay.ReadOnly = true;
            this.txtColorDisplay.Size = new System.Drawing.Size(135, 20);
            this.txtColorDisplay.TabIndex = 1;
            // 
            // btnDoRainbow
            // 
            this.btnDoRainbow.Location = new System.Drawing.Point(12, 45);
            this.btnDoRainbow.Name = "btnDoRainbow";
            this.btnDoRainbow.Size = new System.Drawing.Size(135, 23);
            this.btnDoRainbow.TabIndex = 2;
            this.btnDoRainbow.Text = "Start Rainbow";
            this.btnDoRainbow.UseVisualStyleBackColor = true;
            this.btnDoRainbow.Click += new System.EventHandler(this.btnDoRainbow_Click);
            // 
            // btnLoadImage
            // 
            this.btnLoadImage.Location = new System.Drawing.Point(153, 45);
            this.btnLoadImage.Name = "btnLoadImage";
            this.btnLoadImage.Size = new System.Drawing.Size(119, 23);
            this.btnLoadImage.TabIndex = 3;
            this.btnLoadImage.Text = "Load Image";
            this.btnLoadImage.UseVisualStyleBackColor = true;
            this.btnLoadImage.Click += new System.EventHandler(this.btnDoSlideshow_Click);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog1";
            this.openFileDialog.Filter = "Bitmap | *.bmp";
            this.openFileDialog.Multiselect = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(114, 103);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 4;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnLoadVideo
            // 
            this.btnLoadVideo.Location = new System.Drawing.Point(153, 74);
            this.btnLoadVideo.Name = "btnLoadVideo";
            this.btnLoadVideo.Size = new System.Drawing.Size(119, 23);
            this.btnLoadVideo.TabIndex = 5;
            this.btnLoadVideo.Text = "Load Video";
            this.btnLoadVideo.UseVisualStyleBackColor = true;
            this.btnLoadVideo.Click += new System.EventHandler(this.btnLoadVideo_Click);
            // 
            // btnSort
            // 
            this.btnSort.Location = new System.Drawing.Point(12, 74);
            this.btnSort.Name = "btnSort";
            this.btnSort.Size = new System.Drawing.Size(135, 23);
            this.btnSort.TabIndex = 6;
            this.btnSort.Text = "RGBY";
            this.btnSort.UseVisualStyleBackColor = true;
            this.btnSort.Click += new System.EventHandler(this.btnSort_Click);
            // 
            // LedWallInteract
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 142);
            this.Controls.Add(this.btnSort);
            this.Controls.Add(this.btnLoadVideo);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnLoadImage);
            this.Controls.Add(this.btnDoRainbow);
            this.Controls.Add(this.txtColorDisplay);
            this.Controls.Add(this.btnChooseColor);
            this.Name = "LedWallInteract";
            this.Text = "LED Wall Interact";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.Button btnChooseColor;
        private System.Windows.Forms.TextBox txtColorDisplay;
        private System.Windows.Forms.Button btnDoRainbow;
        private System.Windows.Forms.Button btnLoadImage;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnLoadVideo;
        private System.Windows.Forms.Button btnSort;
    }
}

