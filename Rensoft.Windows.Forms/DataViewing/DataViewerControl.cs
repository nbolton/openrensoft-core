using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Rensoft.Windows.Forms.DataAction;
using Rensoft.Windows.Forms.DataEditing;
using System.Reflection;
using Rensoft.DataEditing;

namespace Rensoft.Windows.Forms.DataViewing
{
    public partial class DataViewerControl : UserControl
    {
        private List<DataEditorForm> editorList;
        private List<ListView> listViewList;

        public event EventHandler BeforeLoad;
        public event EventHandler AfterLoad;
        public event EventHandler Shown;

        public event DataActionBeforeEventHandler BeforeLoadAsync;
        public event DataActionBeforeEventHandler BeforeOpenAsync;
        public event DataActionBeforeEventHandler BeforeNewAsync;
        public event DataActionBeforeEventHandler BeforeDeleteAsync;

        public event DataActionEventHandler LoadAsync;
        public event DataActionEventHandler OpenAsync;
        public event DataActionEventHandler NewAsync;
        public event DataActionEventHandler DeleteAsync;

        public event DataActionAfterEventHandler AfterLoadAsync;
        public event DataActionAfterEventHandler AfterOpenAsync;
        public event DataActionAfterEventHandler AfterNewAsync;
        public event DataActionAfterEventHandler AfterDeleteAsync;

        public event StatusChangeEventHandler StatusChange;
        public event StatusRevertEventHandler StatusRevert;

        public event EventHandler New;
        public event EventHandler Open;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisplayUserMessageAfter { get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LoadAsyncOnControlLoad { get; set; } 

        public DataViewerControl()
        {
            InitializeComponent();

            this.editorList = new List<DataEditorForm>();
            this.listViewList = new List<ListView>();

            this.LoadAsyncOnControlLoad = true;
            this.DisplayUserMessageAfter = true;

            Load += new EventHandler(DataViewerControl_Load);
            AfterDeleteAsync += new DataActionAfterEventHandler(DataViewerControl_AfterDeleteAsync);
        }

        void DataViewerControl_AfterDeleteAsync(object sender, DataActionAfterEventArgs e)
        {
            RunLoadAsync();
        }

        void DataViewerControl_Load(object sender, EventArgs e)
        {
            if (!DesignMode && LoadAsyncOnControlLoad)
            {
                RunLoadAsync();
            }
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);

            if (Visible)
            {
                OnShown(EventArgs.Empty);
            }
        }

        protected virtual void OnShown(EventArgs e)
        {
            if (Shown != null) Shown(this, e);
        }

        public TEditor GetEditor<TEditor>(IUniqueData data)
            where TEditor : DataEditorForm
        {
            DataEditorForm foundForm = editorList.Find(
                f => f.GetEditorData<IUniqueData>().Equals(data));

            if (foundForm != null)
            {
                foundForm.Focus();
                return (TEditor)foundForm;
            }
            else
            {
                //ConstructorInfo ctor = typeof(TEditor).GetConstructor(new Type[0]);
                //if (ctor == null)
                //{
                //    throw new Exception(
                //        "Could not create new data editor of type '"
                //        + typeof(TEditor).Name +
                //        "' because no empty constructor exists.");
                //}

                //TEditor editor = (TEditor)ctor.Invoke(new object[0]);
                TEditor editor = Activator.CreateInstance<TEditor>();
                editorList.Add(editor);
                return editor;
            }
        }

        protected class LoadEditorArgs
        {
            public Guid StatusGuid { get; private set; }
            public bool ShowAfterLoad { get; private set; }

            public LoadEditorArgs(Guid statusGuid, bool showAfterLoad)
            {
                this.StatusGuid = statusGuid;
                this.ShowAfterLoad = showAfterLoad;
            }
        }

        public void ShowEditorAsync(DataEditorForm dataEditor)
        {
            dataEditor.FormClosed += new FormClosedEventHandler(dataEditor_FormClosed);
            dataEditor.AfterLoadAsync += new DataActionAfterEventHandler(dataEditor_AfterLoadAsync);
            dataEditor.AfterCreateAsync += new DataActionAfterEventHandler(dataEditor_AfterCreateAsync);
            dataEditor.AfterUpdateAsync += new DataActionAfterEventHandler(dataEditor_AfterUpdateAsync);

            // Ensure that status is not changed when form not created.
            dataEditor.AutoChangeStatus = false;

            Guid statusGuid = AsyncStatusChange("Loading editor...");
            dataEditor.RunLoadAsync(new LoadEditorArgs(statusGuid, true));
        }

        void dataEditor_AfterUpdateAsync(object sender, DataActionAfterEventArgs e)
        {
            RunLoadAsync();
        }

        void dataEditor_AfterCreateAsync(object sender, DataActionAfterEventArgs e)
        {
            RunLoadAsync();
        }

        void dataEditor_AfterLoadAsync(object sender, DataActionAfterEventArgs e)
        {
            if (e.CheckType<LoadEditorArgs>())
            {
                LoadEditorArgs args = e.GetData<LoadEditorArgs>();
                DataEditorForm dataEditor = (DataEditorForm)sender;

                if (args.ShowAfterLoad)
                {
                    dataEditor.Show();
                }

                AsyncStatusRevert(args.StatusGuid);

                // Ensure that status auto changes again (assumes it was enabled before).
                dataEditor.AutoChangeStatus = true;
            }
        }

        void dataEditor_FormClosed(object sender, FormClosedEventArgs e)
        {
            editorList.Remove((DataEditorForm)sender);
        }

        public void AddListView(ListView listView)
        {
            listViewList.Add(listView);

            listView.KeyDown += new KeyEventHandler(listView_KeyDown);
            listView.MouseDoubleClick += new MouseEventHandler(listView_MouseDoubleClick);
        }

        void listView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && (((ListView)sender).SelectedIndices.Count != 0))
            {
                OpenSelectedAsync((ListView)sender);
            }
        }

        public void OpenSelectedAsync(ListView listView)
        {
            DataViewerOpenArgs args = new DataViewerOpenArgs
            {
                SelectedIndices = listView.GetSelectedIndexArray()
            };
            RunOpenAsync(args);
        }

        void listView_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5: RunLoadAsync(); break;
                case Keys.Enter: OpenSelectedAsync((ListView)sender); break;
                case Keys.Delete: DeleteSelectedAsync((ListView)sender); break;
            }
        }

        public void DeleteSelectedAsync(ListView listView)
        {
            DataViewerOpenArgs args = new DataViewerOpenArgs
            {
                SelectedIndices = listView.GetSelectedIndexArray()
            };
            RunDeleteAsync(args);
        }

        protected virtual void OnNew(EventArgs e)
        {
            if (New != null) New(this, e);
        }

        protected virtual void OnOpen(EventArgs e)
        {
            if (Open != null) Open(this, e);
        }

        protected virtual void OnStatusChange(StatusChangeEventArgs e)
        {
            if (StatusChange != null) StatusChange(this, e);
        }

        protected virtual void OnStatusRevert(StatusRevertEventArgs e)
        {
            if (StatusRevert != null) StatusRevert(this, e);
        }

        protected override void OnLoad(EventArgs e)
        {
            OnBeforeLoad(this, EventArgs.Empty);
            base.OnLoad(e);
            OnAfterLoad(this, EventArgs.Empty);
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

        protected virtual void OnBeforeOpenAsync(DataActionBeforeEventArgs e)
        {
            if (BeforeOpenAsync != null) BeforeOpenAsync(this, e);
        }

        protected virtual void OnBeforeDeleteAsync(DataActionBeforeEventArgs e)
        {
            if (BeforeDeleteAsync != null) BeforeDeleteAsync(this, e);
        }

        protected virtual void OnBeforeNewAsync(DataActionBeforeEventArgs e)
        {
            if (BeforeNewAsync != null) BeforeNewAsync(this, e);
        }

        protected virtual void OnLoadAsync(DataActionEventArgs e)
        {
            if (LoadAsync != null) LoadAsync(this, e);
        }

        protected virtual void OnOpenAsync(DataActionEventArgs e)
        {
            if (OpenAsync != null) OpenAsync(this, e);
        }

        protected virtual void OnNewAsync(DataActionEventArgs e)
        {
            if (NewAsync != null) NewAsync(this, e);
        }

        protected virtual void OnDeleteAsync(DataActionEventArgs e)
        {
            if (DeleteAsync != null) DeleteAsync(this, e);
        }

        protected virtual void OnAfterLoadAsync(DataActionAfterEventArgs e)
        {
            if (AfterLoadAsync != null) AfterLoadAsync(this, e);
        }

        protected virtual void OnAfterOpenAsync(DataActionAfterEventArgs e)
        {
            if (AfterOpenAsync != null) AfterOpenAsync(this, e);
        }

        protected virtual void OnAfterNewAsync(DataActionAfterEventArgs e)
        {
            if (AfterNewAsync != null) AfterNewAsync(this, e);
        }

        protected virtual void OnAfterDeleteAsync(DataActionAfterEventArgs e)
        {
            if (AfterDeleteAsync != null) AfterDeleteAsync(this, e);
        }

        public Guid AsyncStatusChange(string statusText)
        {
            StatusChangeEventArgs e = new StatusChangeEventArgs(statusText);
            OnStatusChange(e);
            return e.StatusGuid;
        }

        public Guid AsyncStatusChange(string statusText, Cursor cursor)
        {
            StatusChangeEventArgs e = new StatusChangeEventArgs(statusText, cursor);
            OnStatusChange(e);
            return e.StatusGuid;
        }

        public void AsyncStatusRevert(Guid statusGuid)
        {
            StatusRevertEventArgs e = new StatusRevertEventArgs(statusGuid);
            OnStatusRevert(e);
        }

        public void AsyncStatusRevert(Guid statusGuid, string revertStatus)
        {
            StatusRevertEventArgs e = new StatusRevertEventArgs(statusGuid, revertStatus);
            OnStatusRevert(e);
        }

        public void RunLoadAsync()
        {
            RunLoadAsync(null);
        }

        public void RunOpenAsync()
        {
            RunOpenAsync(null);
        }

        public void RunNewAsync()
        {
            RunNewAsync(null);
        }

        public void RunDeleteAsync()
        {
            RunDeleteAsync(null);
        }

        public void RunLoadAsync(object inputData)
        {
            runActionAsync(DataViewerAction.Load, inputData);
        }

        public void RunOpenAsync(object inputData)
        {
            OnOpen(EventArgs.Empty);
            runActionAsync(DataViewerAction.Open, inputData);
        }

        public void RunNewAsync(object inputData)
        {
            OnNew(EventArgs.Empty);
            runActionAsync(DataViewerAction.New, inputData);
        }

        public void RunDeleteAsync(object inputData)
        {
            runActionAsync(DataViewerAction.Delete, inputData);
        }

        private void runActionAsync(DataViewerAction action, object inputData)
        {
            if (!mainBackgroundWorker.IsBusy)
            {
                DataActionBeforeEventArgs beforeArgs = new DataActionBeforeEventArgs(inputData);
                switch (action)
                {
                    case DataViewerAction.Load:
                        OnBeforeLoadAsync(beforeArgs);
                        break;

                    case DataViewerAction.Open:
                        OnBeforeOpenAsync(beforeArgs);
                        break;

                    case DataViewerAction.New:
                        OnBeforeNewAsync(beforeArgs);
                        break;

                    case DataViewerAction.Delete:
                        OnBeforeDeleteAsync(beforeArgs);
                        break;
                }

                if (!beforeArgs.Cancel)
                {
                    DataViewerActionArgs args = new DataViewerActionArgs(
                        beforeArgs.Data,
                        beforeArgs.StatusGuid,
                        action);

                    mainBackgroundWorker.RunWorkerAsync(args);
                }
                else
                {
                    DataViewerActionResult result = new DataViewerActionResult(
                        beforeArgs.Data,
                        beforeArgs.StatusGuid,
                        true,
                        string.Empty,
                        action);

                    handleAfterDataAction(result);
                }
            }
        }

        private void mainBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            DataViewerActionArgs actionArgs = (DataViewerActionArgs)e.Argument;
            DataActionEventArgs eventArgs = new DataActionEventArgs(actionArgs.Data);

            switch (actionArgs.Action)
            {
                case DataViewerAction.Load:
                    OnLoadAsync(eventArgs);
                    break;

                case DataViewerAction.Open:
                    OnOpenAsync(eventArgs);
                    break;

                case DataViewerAction.New:
                    OnNewAsync(eventArgs);
                    break;

                case DataViewerAction.Delete:
                    OnDeleteAsync(eventArgs);
                    break;
            }

            DataViewerActionResult result = new DataViewerActionResult(
                eventArgs.Data,
                actionArgs.StatusGuid,
                eventArgs.Cancelled,
                eventArgs.UserMessage,
                actionArgs.Action);

            e.Result = result;
        }

        private void mainBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            DataViewerActionResult result = (DataViewerActionResult)e.Result;

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

        private void handleAfterDataAction(DataViewerActionResult result)
        {
            DataActionAfterEventArgs eventArgs = new DataActionAfterEventArgs(
                result.StatusGuid, result.Cancelled, result.Data);

            switch (result.Action)
            {
                case DataViewerAction.Load:
                    OnAfterLoadAsync(eventArgs);
                    break;

                case DataViewerAction.Open:
                    OnAfterOpenAsync(eventArgs);
                    break;

                case DataViewerAction.New:
                    OnAfterNewAsync(eventArgs);
                    break;

                case DataViewerAction.Delete:
                    OnAfterDeleteAsync(eventArgs);
                    break;
            }
        }
    }
}
