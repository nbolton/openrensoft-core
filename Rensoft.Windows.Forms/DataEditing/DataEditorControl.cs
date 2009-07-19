using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rensoft.Windows.Forms.DataEditing
{
    public partial class DataEditorControl : UserControl
    {
        private object editorData;

        public event DataEditorReflectEventHandler ReflectDataToForm;
        public event DataEditorReflectEventHandler ReflectFormToData;
        public event CancelEventHandler ValidateData;
        public event EventHandler DataChanged;

        new public DataEditorForm ParentForm
        {
            get { return (DataEditorForm)base.ParentForm; }
        }

        public DataEditorControl()
        {
            InitializeComponent();
        }

        protected virtual void OnDataChanged(EventArgs e)
        {
            if (DataChanged != null) DataChanged(this, e);
        }

        protected virtual void OnReflectDataToForm(DataEditorReflectEventArgs e)
        {
            if (ReflectDataToForm != null) ReflectDataToForm(this, e);
        }

        protected virtual void OnReflectFormToData(DataEditorReflectEventArgs e)
        {
            if (ReflectFormToData != null) ReflectFormToData(this, e);
        }

        protected virtual void OnValidateData(CancelEventArgs e)
        {
            if (ValidateData != null) ValidateData(this, e);
        }

        public void SetEditorData(object editorData)
        {
            this.editorData = editorData;
        }

        public bool RunValidateData()
        {
            CancelEventArgs e = new CancelEventArgs();
            OnValidateData(e);
            return !e.Cancel;
        }

        public void RunReflectDataToForm(DataEditorMode mode)
        {
            OnReflectDataToForm(new DataEditorReflectEventArgs(editorData, mode));
        }

        public void RunReflectFormToData(DataEditorMode mode)
        {
            OnReflectFormToData(new DataEditorReflectEventArgs(editorData, mode));
        }

        protected void ChangeMade()
        {
            OnDataChanged(EventArgs.Empty);
        }
    }
}
