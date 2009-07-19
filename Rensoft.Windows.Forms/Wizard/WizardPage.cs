using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Rensoft.Windows.Forms.Wizard
{
    public partial class WizardPage : UserControl
    {
        private bool isBusy;
        private WizardForm parentWizard;
        private bool enableNext = true;
        private bool enableBack = true;
        private string titleText;
        private string infoText;
        private bool enableNextAfterLoad = true;
        private bool isLastPage;

        public bool IsLastPage
        {
            get { return isLastPage; }
            set
            {
                isLastPage = value;
                OnIsLastPageChanged(EventArgs.Empty);
            }
        }

        public event DoWorkEventHandler BeforeLoadAsync;
        public event DoWorkEventHandler BeforeBackAsync;
        public event DoWorkEventHandler BeforeNextAsync;

        public event DoWorkEventHandler LoadAsync;
        public event DoWorkEventHandler BackAsync;
        public event DoWorkEventHandler NextAsync;

        public event RunWorkerCompletedEventHandler AfterLoadAsync;
        public event RunWorkerCompletedEventHandler AfterBackAsync;
        public event RunWorkerCompletedEventHandler AfterNextAsync;

        public event EventHandler IsLastPageChanged;
        public event EventHandler EnableNextChanged;
        public event EventHandler EnableBackChanged;
        public event EventHandler IsBusyChanged;
        public event EventHandler Shown;

        public string TitleText
        {
            get { return titleText; }
            set { titleText = value; }
        }

        public string InfoText
        {
            get { return infoText; }
            set { infoText = value; }
        }

        public WizardForm ParentWizard
        {
            get { return parentWizard; }
            set { parentWizard = value; }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                OnIsBusyChanged(EventArgs.Empty);
            }
        }

        public bool EnableNext
        {
            get { return enableNext; }
            protected set
            {
                enableNext = value;
                OnEnableNextChanged(EventArgs.Empty);
            }
        }

        public bool EnableBack
        {
            get { return enableBack; }
            protected set
            {
                enableBack = value;
                OnEnableBackChanged(EventArgs.Empty);
            }
        }

        public bool EnableNextAfterLoad
        {
            get { return enableNextAfterLoad; }
            set { enableNextAfterLoad = value; }
        }

        public WizardPage()
        {
            InitializeComponent();
            Shown += new EventHandler(WizardPage_Shown);
        }

        void WizardPage_Shown(object sender, EventArgs e)
        {
            RunLoadAsync();
        }

        void WizardPage_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                OnShown(EventArgs.Empty);
            }
        }

        protected void OnShown(EventArgs e)
        {
            if (Shown != null) Shown(this, e);
        }

        protected void OnIsBusyChanged(EventArgs e)
        {
            if (IsBusyChanged != null) IsBusyChanged(this, e);
        }

        protected void OnEnableNextChanged(EventArgs e)
        {
            if (EnableNextChanged != null) EnableNextChanged(this, e);
        }

        protected void OnEnableBackChanged(EventArgs e)
        {
            if (EnableBackChanged != null) EnableBackChanged(this, e);
        }

        protected virtual void OnBeforeLoadAsync(DoWorkEventArgs e)
        {
            if (BeforeLoadAsync != null) BeforeLoadAsync(this, e);
        }

        protected virtual void OnBeforeBackAsync(DoWorkEventArgs e)
        {
            if (BeforeBackAsync != null) BeforeBackAsync(this, e);
        }

        protected virtual void OnBeforeNextAsync(DoWorkEventArgs e)
        {
            if (BeforeNextAsync != null) BeforeNextAsync(this, e);
        }

        protected virtual void OnLoadAsync(DoWorkEventArgs e)
        {
            if (LoadAsync != null) LoadAsync(null, e);
        }

        protected virtual void OnBackAsync(DoWorkEventArgs e)
        {
            if (BackAsync != null) BackAsync(null, e);
        }

        protected virtual void OnNextAsync(DoWorkEventArgs e)
        {
            if (NextAsync != null) NextAsync(null, e);
        }

        protected virtual void OnAfterLoadAsync(RunWorkerCompletedEventArgs e)
        {
            if (AfterLoadAsync != null) AfterLoadAsync(this, e);
        }

        protected virtual void OnAfterBackAsync(RunWorkerCompletedEventArgs e)
        {
            if (AfterBackAsync != null) AfterBackAsync(this, e);
        }

        protected virtual void OnAfterNextAsync(RunWorkerCompletedEventArgs e)
        {
            if (AfterNextAsync != null) AfterNextAsync(this, e);
        }

        protected virtual void OnIsLastPageChanged(EventArgs e)
        {
            if (IsLastPageChanged != null) { IsLastPageChanged(this, e); }
        }

        public bool RunLoadAsync()
        {
            return RunLoadAsync(null);
        }

        public bool RunBackAsync()
        {
            return RunBackAsync(null);
        }

        public bool RunNextAsync()
        {
            return RunNextAsync(null);
        }

        public bool RunLoadAsync(object argument)
        {
            DoWorkEventArgs e = new DoWorkEventArgs(argument);
            OnBeforeLoadAsync(e);

            if (!e.Cancel && !loadBackgroundWorker.IsBusy)
            {
                this.EnableNext = false;
                this.Enabled = false;
                this.IsBusy = true;

                loadBackgroundWorker.RunWorkerAsync(argument);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RunBackAsync(object argument)
        {
            DoWorkEventArgs e = new DoWorkEventArgs(argument);
            OnBeforeBackAsync(e);

            if (!e.Cancel && !backBackgroundWorker.IsBusy)
            {
                this.Enabled = false;
                this.IsBusy = true;

                backBackgroundWorker.RunWorkerAsync(argument);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RunNextAsync(object argument)
        {
            DoWorkEventArgs e = new DoWorkEventArgs(argument);
            OnBeforeNextAsync(e);

            if (!e.Cancel && !nextBackgroundWorker.IsBusy)
            {
                this.Enabled = false;
                this.IsBusy = true;

                nextBackgroundWorker.RunWorkerAsync(argument);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void loadBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnLoadAsync(e);
        }

        private void backBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnBackAsync(e);
        }

        private void nextBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnNextAsync(e);
        }

        private void loadBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                throw new Exception("Could not perform 'load' operation.", e.Error);
            }

            if (EnableNextAfterLoad)
            {
                this.EnableNext = true;
            }

            this.Enabled = true;
            this.IsBusy = false;

            OnAfterLoadAsync(e);
        }

        private void backBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                throw new Exception("Could not perform 'back' operation.", e.Error);
            }

            this.Enabled = true;
            this.IsBusy = false;

            OnAfterBackAsync(e);
        }

        private void nextBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                throw new Exception("Could not perform 'next' operation.", e.Error);
            }

            this.Enabled = true;
            this.IsBusy = false;

            OnAfterNextAsync(e);
        }

        public DialogResult ShowMessageBox(
            string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return InvokeShowMessageBox(text, caption, buttons, icon);
        }

        protected DialogResult InvokeShowMessageBox(
            string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            showMessageBoxDelegate method = new showMessageBoxDelegate(showMessageBox);
            return (DialogResult)this.Invoke(method, text, caption, buttons, icon);
        }

        private delegate DialogResult showMessageBoxDelegate(
            string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon);

        private DialogResult showMessageBox(
            string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            return MessageBox.Show(this, text, caption, buttons, icon);
        }
    }
}
