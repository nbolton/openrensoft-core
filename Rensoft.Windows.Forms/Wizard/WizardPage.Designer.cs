namespace Rensoft.Windows.Forms.Wizard
{
    partial class WizardPage
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.backBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.nextBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.loadBackgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // backBackgroundWorker
            // 
            this.backBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backBackgroundWorker_DoWork);
            this.backBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backBackgroundWorker_RunWorkerCompleted);
            // 
            // nextBackgroundWorker
            // 
            this.nextBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.nextBackgroundWorker_DoWork);
            this.nextBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.nextBackgroundWorker_RunWorkerCompleted);
            // 
            // loadBackgroundWorker
            // 
            this.loadBackgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.loadBackgroundWorker_DoWork);
            this.loadBackgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.loadBackgroundWorker_RunWorkerCompleted);
            // 
            // WizardPage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "WizardPage";
            this.Size = new System.Drawing.Size(427, 247);
            this.Tag = "";
            this.VisibleChanged += new System.EventHandler(this.WizardPage_VisibleChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker backBackgroundWorker;
        private System.ComponentModel.BackgroundWorker nextBackgroundWorker;
        private System.ComponentModel.BackgroundWorker loadBackgroundWorker;
    }
}
