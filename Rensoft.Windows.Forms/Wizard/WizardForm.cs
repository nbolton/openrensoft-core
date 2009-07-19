using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Rensoft.Windows.Forms.Wizard
{
    public partial class WizardForm : Form
    {
        private int pageIndex = 0;
        private bool enableNext = true;
        private bool enableBack = true;
        private bool closeAfterSave = true;

        public event DoWorkEventHandler BeforeSaveAsync;
        public event DoWorkEventHandler SaveAsync;
        public event RunWorkerCompletedEventHandler AfterSaveAsync;

        protected WizardPage CurrentPage
        {
            get { return pagePanel.Controls[pageIndex] as WizardPage; }
        }

        protected bool EnableNext
        {
            get { return enableNext; }
            set
            {
                enableNext = value;
                refreshNextButton();
            }
        }

        protected bool EnableBack
        {
            get { return enableBack; }
            set
            {
                enableBack = value;
                refreshBackButton();
            }
        }

        public bool HasPages
        {
            get { return pagePanel.Controls.Count != 0; }
        }

        public bool LastPageActive
        {
            get
            {
                return ((pageIndex == (pagePanel.Controls.Count - 1))
                    || CurrentPage.IsLastPage);
            }
        }

        public bool CloseAfterSave
        {
            get { return closeAfterSave; }
            set { closeAfterSave = value; }
        }

        public WizardForm()
        {
            InitializeComponent();
        }

        void WizardForm_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                if (HasPages)
                {
                    CurrentPage.Visible = true;
                    refreshBannerLabels();
                }
            }
        }

        protected virtual void OnBeforeSaveAsync(DoWorkEventArgs e)
        {
            if (BeforeSaveAsync != null) BeforeSaveAsync(this, e);
        }

        protected virtual void OnSaveAsync(DoWorkEventArgs e)
        {
            // Purposely pass null sender to avoid illegal cross-thread reference.
            if (SaveAsync != null) SaveAsync(null, e);
        }

        protected virtual void OnAfterSaveAsync(RunWorkerCompletedEventArgs e)
        {
            if (AfterSaveAsync != null) AfterSaveAsync(this, e);
        }

        public bool RunSaveAsync()
        {
            return RunSaveAsync(null);
        }

        public bool RunSaveAsync(object argument)
        {
            DoWorkEventArgs beforeArgs = new DoWorkEventArgs(argument);
            OnBeforeSaveAsync(beforeArgs);

            if (!beforeArgs.Cancel && !saveBackgroundWorker.IsBusy)
            {
                Cursor = Cursors.WaitCursor;
                CurrentPage.Enabled = false;

                saveBackgroundWorker.RunWorkerAsync();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void saveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnSaveAsync(e);
        }

        private void saveBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Default;
            CurrentPage.Enabled = true;

            if (e.Error != null)
            {
                throw new Exception("Could not perform 'save' operation.", e.Error);
            }

            OnAfterSaveAsync(e);

            if (!e.Cancelled && CloseAfterSave)
            {
                Close();
            }
        }

        protected void AddPage(WizardPage wizardPage)
        {
            pagePanel.Controls.Add(wizardPage);
            wizardPage.ParentWizard = this;
            wizardPage.AfterBackAsync += new RunWorkerCompletedEventHandler(wizardPage_AfterBack);
            wizardPage.AfterNextAsync += new RunWorkerCompletedEventHandler(wizardPage_AfterNext);
            wizardPage.EnableNextChanged += new EventHandler(wizardPage_EnableNextChanged);
            wizardPage.EnableBackChanged += new EventHandler(wizardPage_EnableBackChanged);
            wizardPage.IsBusyChanged += new EventHandler(wizardPage_IsBusyChanged);
            wizardPage.AfterLoadAsync += new RunWorkerCompletedEventHandler(wizardPage_AfterLoadAsync);
            wizardPage.IsLastPageChanged += new EventHandler(wizardPage_IsLastPageChanged);
            wizardPage.Dock = DockStyle.Fill;
            wizardPage.Visible = false;
        }

        void wizardPage_IsLastPageChanged(object sender, EventArgs e)
        {
            refreshNextButton();
        }

        void wizardPage_AfterLoadAsync(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshButtons();
        }

        void wizardPage_IsBusyChanged(object sender, EventArgs e)
        {
            if (((WizardPage)sender).IsBusy)
            {
                Cursor = Cursors.WaitCursor;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        void wizardPage_EnableBackChanged(object sender, EventArgs e)
        {
            this.EnableBack = ((WizardPage)sender).EnableBack;
        }

        void wizardPage_EnableNextChanged(object sender, EventArgs e)
        {
            this.EnableNext = ((WizardPage)sender).EnableNext;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            if (CurrentPage.RunBackAsync())
            {
                Cursor = Cursors.WaitCursor;
                disableButtons();
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if (CurrentPage.RunNextAsync())
            {
                Cursor = Cursors.WaitCursor;
                disableButtons();
            }
        }

        void wizardPage_AfterBack(object sender, AsyncCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                CurrentPage.Visible = false;
                pageIndex--;
                CurrentPage.Visible = true;
                refreshBannerLabels();
            }

            RefreshButtons();

            if (!CurrentPage.IsBusy)
            {
                Cursor = Cursors.Default;
            }
        }

        void wizardPage_AfterNext(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                // Do nothing and make sure finish pending is off.
                DialogResult = DialogResult.None;
            }
            else if (LastPageActive)
            {
                // Next on finish completed ok, so close.
                DialogResult = DialogResult.OK;
                RunSaveAsync();
            }
            else
            {
                // Just move to next page as normal.
                CurrentPage.Visible = false;
                pageIndex++;
                CurrentPage.Visible = true;
                refreshBannerLabels();
            }

            RefreshButtons();

            if (!CurrentPage.IsBusy)
            {
                Cursor = Cursors.Default;
            }
        }

        private void refreshBannerLabels()
        {
            titleLabel.Text = CurrentPage.TitleText;
            infoLabel.Text = CurrentPage.InfoText;
        }

        private void disableButtons()
        {
            nextButton.Enabled = false;
            backButton.Enabled = false;
        }

        private void refreshNextButton()
        {
            nextButton.Enabled = enableNext && (pageIndex <= pagePanel.Controls.Count - 1);

            if (LastPageActive)
            {
                nextButton.Text = "&Finish";
            }
            else
            {
                nextButton.Text = "&Next >";
            }
        }

        private void refreshBackButton()
        {
            backButton.Enabled = enableBack && (pageIndex != 0);
        }

        protected void RefreshButtons()
        {
            RefreshButtons(true);
        }

        public void RefreshButtons(bool focusNext)
        {
            refreshBackButton();
            refreshNextButton();

            if (focusNext) nextButton.Focus();
        }

        protected void PerformNextClick()
        {
            nextButton.PerformClick();
        }

        protected void PerformBackClick()
        {
            backButton.PerformClick();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}