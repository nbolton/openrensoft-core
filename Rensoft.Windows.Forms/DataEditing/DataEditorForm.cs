using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rensoft.Windows.Forms.DataAction;
using Rensoft.DataEditing;

namespace Rensoft.Windows.Forms.DataEditing
{
    public partial class DataEditorForm : Form
    {
        private IUniqueData editorData;
        private Button cancelButton;
        private DataEditorMode editorMode;
        private Dictionary<Guid, DataActionStatusState> statusTable;
        private List<DataEditorControl> editorControlList;
        private bool changesSaved = true;
        private bool suspendChangeMadeNotify;

        public event CancelEventHandler ValidateData;
        public event EventHandler EditorDataChanged;

        public event EventHandler BeforeLoad;
        public event EventHandler AfterLoad;

        public event DataActionBeforeEventHandler BeforeLoadAsync;
        public event DataActionBeforeEventHandler BeforeCreateAsync;
        public event DataActionBeforeEventHandler BeforeUpdateAsync;
        public event DataActionBeforeEventHandler BeforeSaveAsync;

        public event DataActionEventHandler LoadAsync;
        public event DataActionEventHandler CreateAsync;
        public event DataActionEventHandler UpdateAsync;
        public event DataActionEventHandler SaveAsync;

        public event DataActionAfterEventHandler AfterLoadAsync;
        public event DataActionAfterEventHandler AfterCreateAsync;
        public event DataActionAfterEventHandler AfterUpdateAsync;
        public event DataActionAfterEventHandler AfterSaveAsync;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string DefaultStatus { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisplayUserMessageAfter { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LoadAsyncOnFormLoad { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AutoChangeStatus { get; set; }

        delegate string GSDelegate();
        delegate void SSDelegate();

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Status
        {
            get
            {
                return (string)Invoke((GSDelegate)delegate() { return statusLabel.Text; });
            }
            set
            {
                Invoke((SSDelegate)delegate() { statusLabel.Text = value; });
            }
        }

        public DataEditorMode EditorMode
        {
            get { return editorMode; }
        }

        public DataEditorForm()
        {
            InitializeComponent();

            // Set as "Ready" which can be changed.
            this.DefaultStatus = "Ready";

            // Show error messages after async by default.
            this.DisplayUserMessageAfter = true;

            // Allow caller to run load before showing.
            this.LoadAsyncOnFormLoad = false;

            // By default, change async status automatically.
            this.AutoChangeStatus = true;

            this.editorControlList = new List<DataEditorControl>();
            this.statusTable = new Dictionary<Guid, DataActionStatusState>();

            this.cancelButton = new Button();
            this.cancelButton.Click += new EventHandler(cancelButton_Click);
            this.CancelButton = cancelButton;

            Load += new EventHandler(EditorForm_Load);
            EditorDataChanged += new EventHandler(DataEditorForm_EditorDataChanged);
            BeforeSaveAsync += new DataActionBeforeEventHandler(DataEditorForm_BeforeSaveAsync);
            BeforeLoad += new EventHandler(DataEditorForm_BeforeLoad);
            FormClosing += new FormClosingEventHandler(DataEditorForm_FormClosing);
        }

        void DataEditorForm_BeforeSaveAsync(object sender, DataActionBeforeEventArgs e)
        {
            editorControlList.ForEach(control => control.RunReflectFormToData(editorMode));
        }

        void DataEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!changesSaved)
            {
                DialogResult result = MessageBox.Show(
                    "Do you want to save changes?",
                    "Save changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    e.Cancel = true;
                    RunSaveCloseAsync();
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void EditorForm_Load(object sender, EventArgs e)
        {
            if (!DesignMode)
            {
                if (editorMode == DataEditorMode.None)
                {
                    throw new NotSupportedException(
                        "The editor mode must be set using " +
                        "either SetCreateMode or SetUpdateMode.");
                }

                if (LoadAsyncOnFormLoad)
                {
                    RunLoadAsync();
                }
            }
        }

        void DataEditorForm_EditorDataChanged(object sender, EventArgs e)
        {
            editorControlList.ForEach(control => control.SetEditorData(GetEditorData<object>()));
        }

        void DataEditorForm_BeforeLoad(object sender, EventArgs e)
        {
            editorControlList.ForEach(control => control.RunReflectDataToForm(editorMode));
        }

        public void AddEditorControl(DataEditorControl editorControl)
        {
            editorControl.DataChanged += new EventHandler(editorControl_DataChanged);

            editorControlList.Add(editorControl);
        }

        void editorControl_DataChanged(object sender, EventArgs e)
        {
            ChangeMade();
        }

        public void SetCreateMode(IUniqueData editorData)
        {
            this.editorMode = DataEditorMode.Create;
            SetEditorData(editorData);
        }

        public void SetUpdateMode(IUniqueData editorData)
        {
            this.editorMode = DataEditorMode.Update;
            SetEditorData(editorData);
        }

        public void SetEditorData(IUniqueData editorData)
        {
            this.editorData = editorData;
            OnEditorDataChanged(EventArgs.Empty);
        }

        public TValue GetEditorData<TValue>()
        {
            return (TValue)editorData;
        }

        public Guid AsyncStatusChange(string message)
        {
            return AsyncStatusChange(message, Cursors.WaitCursor);
        }

        public Guid AsyncStatusChange(string message, Cursor cursor)
        {
            if (!this.IsHandleCreated)
            {
                throw new InvalidOperationException(
                    "Cannot change status when window handle is not created.");
            }

            Guid guid = Guid.NewGuid();
            DataActionStatusState state = new DataActionStatusState(message, cursor);
            statusTable.Add(guid, state);
            Status = message;
            Cursor = cursor;
            return guid;
        }

        public void AsyncStatusRevert(Guid statusGuid)
        {
            AsyncStatusRevert(statusGuid, DefaultStatus);
        }

        public void AsyncStatusRevert(Guid statusGuid, string revertStatus)
        {
            if (!this.IsHandleCreated)
            {
                throw new InvalidOperationException(
                    "Cannot revert status when window handle is not created.");
            }

            statusTable.Remove(statusGuid);
            if (statusTable.Count != 0)
            {
                DataActionStatusState last = statusTable.Last().Value;
                Status = last.Message;
                Cursor = last.Cursor;
            }
            else
            {
                Status = string.IsNullOrEmpty(revertStatus) ? DefaultStatus : revertStatus;
                Cursor = Cursors.Default;
            }
        }

        void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        protected override void OnLoad(EventArgs e)
        {
            OnBeforeLoad(this, EventArgs.Empty);
            base.OnLoad(e);
            OnAfterLoad(this, EventArgs.Empty);
        }

        protected virtual void OnValidateData(CancelEventArgs e)
        {
            if (ValidateData != null) ValidateData(this, e);
        }

        protected virtual void OnEditorDataChanged(EventArgs e)
        {
            if (EditorDataChanged != null) EditorDataChanged(this, e);
        }

        protected virtual void OnBeforeLoad(object sender, EventArgs e)
        {
            if (BeforeLoad != null) BeforeLoad(this, e);
        }

        protected virtual void OnAfterLoad(object sender, EventArgs e)
        {
            if (AfterLoad != null) AfterLoad(this, e);
        }

        protected virtual void OnBeforeLoadAsync(DataActionBeforeEventArgs e)
        {
            if (BeforeLoadAsync != null) BeforeLoadAsync(this, e);
        }

        protected virtual void OnBeforeCreateAsync(DataActionBeforeEventArgs e)
        {
            if (BeforeCreateAsync != null) BeforeCreateAsync(this, e);
        }

        protected virtual void OnBeforeUpdateAsync(DataActionBeforeEventArgs e)
        {
            if (BeforeUpdateAsync != null) BeforeUpdateAsync(this, e);
        }

        protected virtual void OnBeforeSaveAsync(DataActionBeforeEventArgs e)
        {
            if (BeforeSaveAsync != null) BeforeSaveAsync(this, e);
        }

        protected virtual void OnLoadAsync(DataActionEventArgs e)
        {
            if (LoadAsync != null) LoadAsync(this, e);
        }

        protected virtual void OnCreateAsync(DataActionEventArgs e)
        {
            if (CreateAsync != null) CreateAsync(this, e);
        }

        protected virtual void OnUpdateAsync(DataActionEventArgs e)
        {
            if (UpdateAsync != null) UpdateAsync(this, e);
        }

        protected virtual void OnSaveAsync(DataActionEventArgs e)
        {
            if (SaveAsync != null) SaveAsync(this, e);
        }

        protected virtual void OnAfterLoadAsync(DataActionAfterEventArgs e)
        {
            if (AfterLoadAsync != null) AfterLoadAsync(this, e);
        }

        protected virtual void OnAfterCreateAsync(DataActionAfterEventArgs e)
        {
            if (AfterCreateAsync != null) AfterCreateAsync(this, e);
        }

        protected virtual void OnAfterUpdateAsync(DataActionAfterEventArgs e)
        {
            if (AfterUpdateAsync != null) AfterUpdateAsync(this, e);
        }

        protected virtual void OnAfterSaveAsync(DataActionAfterEventArgs e)
        {
            if (AfterSaveAsync != null) AfterSaveAsync(this, e);
        }

        public void RunLoadAsync()
        {
            RunLoadAsync(null);
        }

        public void RunSaveAsync()
        {
            RunSaveAsync(null);
        }

        public void RunSaveCloseAsync()
        {
            RunSaveCloseAsync(null);
        }

        public void RunLoadAsync(object inputData)
        {
            runActionAsync(DataEditorAction.Load, inputData);
        }

        public void RunSaveAsync(object inputData)
        {
            runActionAsync(DataEditorAction.Save, inputData);
        }

        public void RunSaveCloseAsync(object inputData)
        {
            runActionAsync(DataEditorAction.SaveClose, inputData);
        }

        public bool RunValidateData()
        {
            bool foundCancelled;

            CancelEventArgs validateArgs = new CancelEventArgs();
            OnValidateData(validateArgs);
            foundCancelled = validateArgs.Cancel;

            if (!foundCancelled)
            {
                foreach (DataEditorControl ec in editorControlList)
                {
                    if (!ec.RunValidateData())
                    {
                        foundCancelled = true;
                        break;
                    }
                }
            }

            // Return false if this or any view fails validation.
            return !foundCancelled;
        }

        private void runActionAsync(DataEditorAction action, object data)
        {
            if (!mainBackgroundWorker.IsBusy)
            {
                suspendChangeMadeNotify = true;

                bool cancelMainEvents = false;
                if ((action == DataEditorAction.Save) || (action == DataEditorAction.SaveClose))
                {
                    // Only validate on save or save and close.
                    cancelMainEvents = !RunValidateData();
                }

                if (!cancelMainEvents)
                {
                    runActionAsyncMainEvents(action, data);
                }
            }
        }

        private void runActionAsyncMainEvents(DataEditorAction action, object data)
        {
            DataActionBeforeEventArgs beforeArgs = new DataActionBeforeEventArgs(data);
            switch (action)
            {
                case DataEditorAction.Load:
                    {
                        if (AutoChangeStatus)
                        {
                            beforeArgs.StatusGuid = AsyncStatusChange("Loading...");
                        }
                        OnBeforeLoadAsync(beforeArgs);
                    }
                    break;

                case DataEditorAction.Save:
                case DataEditorAction.SaveClose:
                    {
                        if (AutoChangeStatus)
                        {
                            beforeArgs.StatusGuid = AsyncStatusChange("Saving...");
                        }

                        OnBeforeSaveAsync(beforeArgs);

                        switch (editorMode)
                        {
                            case DataEditorMode.Create:
                                OnBeforeCreateAsync(beforeArgs);
                                break;

                            case DataEditorMode.Update:
                                OnBeforeUpdateAsync(beforeArgs);
                                break;
                        }
                    }
                    break;
            }

            if (!beforeArgs.Cancel)
            {
                DataEditorActionArgs args = new DataEditorActionArgs(
                    beforeArgs.Data,
                    beforeArgs.StatusGuid,
                    action);

                mainBackgroundWorker.RunWorkerAsync(args);
            }
            else
            {
                DataEditorActionResult result = new DataEditorActionResult(
                    beforeArgs.Data,
                    beforeArgs.StatusGuid,
                    true,
                    string.Empty,
                    action);

                handleAfterDataAction(result);
            }
        }

        private void mainBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            DataEditorActionArgs actionArgs = (DataEditorActionArgs)e.Argument;
            DataActionEventArgs eventArgs = new DataActionEventArgs(actionArgs.Data);

            switch (actionArgs.Action)
            {
                case DataEditorAction.Load:
                    OnLoadAsync(eventArgs);
                    break;

                case DataEditorAction.Save:
                case DataEditorAction.SaveClose:
                    {
                        OnSaveAsync(eventArgs);

                        switch (editorMode)
                        {
                            case DataEditorMode.Create:
                                OnCreateAsync(eventArgs);
                                break;

                            case DataEditorMode.Update:
                                OnUpdateAsync(eventArgs);
                                break;
                        }
                    }
                    break;
            }

            DataEditorActionResult result = new DataEditorActionResult(
                eventArgs.Data, 
                actionArgs.StatusGuid,
                eventArgs.Cancelled, 
                eventArgs.UserMessage, 
                actionArgs.Action);

            e.Result = result;
        }

        private void mainBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DataEditorActionResult result = (DataEditorActionResult)e.Result;

            if (e.Error != null)
            {
                // Throw on windows forms thread.
                Invoke(
                    (MethodInvoker)delegate()
                    {
                        throw new Exception(
                        "An error occured while running the background worker.", e.Error);
                    }
                );
            }

            handleAfterDataAction(result);
        }

        private void handleAfterDataAction(DataEditorActionResult result)
        {
            DataActionAfterEventArgs eventArgs = new DataActionAfterEventArgs(
                result.StatusGuid, result.Cancelled, result.Data);

            switch (result.Action)
            {
                case DataEditorAction.Load:
                    OnAfterLoadAsync(eventArgs);
                    break;

                case DataEditorAction.Save:
                case DataEditorAction.SaveClose:
                    {
                        OnAfterSaveAsync(eventArgs);

                        switch (editorMode)
                        {
                            case DataEditorMode.Create:
                                OnAfterCreateAsync(eventArgs);
                                break;

                            case DataEditorMode.Update:
                                OnAfterUpdateAsync(eventArgs);
                                break;
                        }

                        // Ensure form can close.
                        changesSaved = true;

                        // When cancelled, do not change mode or close.
                        if (!result.Cancelled)
                        {
                            switch (result.Action)
                            {
                                case DataEditorAction.SaveClose:
                                    Close();
                                    break;

                                case DataEditorAction.Save:
                                    editorMode = DataEditorMode.Update;
                                    break;
                            }
                        }
                    }
                    break;
            }

            if (IsHandleCreated)
            {
                if (AutoChangeStatus)
                {
                    // Only revert when window is still alive.
                    AsyncStatusRevert(result.StatusGuid);
                }

                // Not sure if this is so elegant.
                if (DisplayUserMessageAfter
                    && result.Cancelled
                    && !string.IsNullOrEmpty(result.UserMessage))
                {
                    MessageBox.Show(
                        result.UserMessage,
                        "Cancelled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
            }

            suspendChangeMadeNotify = false;
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            RunSaveAsync();
        }

        private void saveCloseToolStripButton_Click(object sender, EventArgs e)
        {
            RunSaveCloseAsync();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunSaveAsync();
        }

        private void saveCloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunSaveCloseAsync();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cancelButton.PerformClick();
        }

        public void ChangeMade()
        {
            if (!suspendChangeMadeNotify)
            {
                // Indicates that user changed value.
                changesSaved = false;
            }
        }
    }
}
