using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.DBCReader
{
    [DesignerGenerated()]
    public partial class frmMain : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is object)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            components = new Container();
            txtFile = new TextBox();
            _cmdBrowse = new Button();
            _cmdBrowse.Click += new EventHandler(cmdBrowse_Click);
            _DBCData = new ListView();
            _DBCData.ColumnClick += new ColumnClickEventHandler(DBCData_ColumnClick);
            ProgressBar = new ProgressBar();
            BindingSource1 = new BindingSource(components);
            cmbColumn = new ComboBox();
            _txtQuery = new TextBox();
            _txtQuery.KeyDown += new KeyEventHandler(txtQuery_KeyDown);
            _cmdSearch = new Button();
            _cmdSearch.Click += new EventHandler(cmdSearch_Click);
            ((ISupportInitialize)BindingSource1).BeginInit();
            SuspendLayout();
            // 
            // txtFile
            // 
            txtFile.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFile.Enabled = false;
            txtFile.Location = new Point(2, 8);
            txtFile.Margin = new Padding(4, 3, 4, 3);
            txtFile.Name = "txtFile";
            txtFile.Size = new Size(781, 23);
            txtFile.TabIndex = 1;
            // 
            // cmdBrowse
            // 
            _cmdBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _cmdBrowse.Location = new Point(791, 7);
            _cmdBrowse.Margin = new Padding(4, 3, 4, 3);
            _cmdBrowse.Name = "_cmdBrowse";
            _cmdBrowse.Size = new Size(104, 23);
            _cmdBrowse.TabIndex = 2;
            _cmdBrowse.Text = "Browse";
            _cmdBrowse.UseVisualStyleBackColor = true;
            // 
            // DBCData
            // 
            _DBCData.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            _DBCData.BorderStyle = BorderStyle.FixedSingle;
            _DBCData.Font = new Font("Tahoma", 8.25f, FontStyle.Regular, GraphicsUnit.Point);
            _DBCData.FullRowSelect = true;
            _DBCData.GridLines = true;
            _DBCData.HideSelection = false;
            _DBCData.Location = new Point(2, 68);
            _DBCData.Margin = new Padding(4, 3, 4, 3);
            _DBCData.Name = "_DBCData";
            _DBCData.Size = new Size(892, 437);
            _DBCData.TabIndex = 3;
            _DBCData.UseCompatibleStateImageBehavior = false;
            _DBCData.View = View.Details;
            // 
            // ProgressBar
            // 
            ProgressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ProgressBar.Location = new Point(2, 509);
            ProgressBar.Margin = new Padding(4, 3, 4, 3);
            ProgressBar.Name = "ProgressBar";
            ProgressBar.Size = new Size(891, 23);
            ProgressBar.Step = 2;
            ProgressBar.TabIndex = 4;
            // 
            // cmbColumn
            // 
            cmbColumn.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbColumn.FormattingEnabled = true;
            cmbColumn.Location = new Point(2, 37);
            cmbColumn.Margin = new Padding(4, 3, 4, 3);
            cmbColumn.Name = "cmbColumn";
            cmbColumn.Size = new Size(177, 23);
            cmbColumn.TabIndex = 5;
            // 
            // txtQuery
            // 
            _txtQuery.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            _txtQuery.Location = new Point(187, 38);
            _txtQuery.Margin = new Padding(4, 3, 4, 3);
            _txtQuery.Name = "_txtQuery";
            _txtQuery.Size = new Size(597, 23);
            _txtQuery.TabIndex = 6;
            // 
            // cmdSearch
            // 
            _cmdSearch.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _cmdSearch.Location = new Point(791, 38);
            _cmdSearch.Margin = new Padding(4, 3, 4, 3);
            _cmdSearch.Name = "_cmdSearch";
            _cmdSearch.Size = new Size(104, 23);
            _cmdSearch.TabIndex = 7;
            _cmdSearch.Text = "Search";
            _cmdSearch.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(7.0f, 15.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(898, 533);
            Controls.Add(_cmdSearch);
            Controls.Add(_txtQuery);
            Controls.Add(cmbColumn);
            Controls.Add(ProgressBar);
            Controls.Add(_DBCData);
            Controls.Add(_cmdBrowse);
            Controls.Add(txtFile);
            Margin = new Padding(4, 3, 4, 3);
            Name = "frmMain";
            Text = "DBC (Database Code) Reader";
            ((ISupportInitialize)BindingSource1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        internal TextBox txtFile;
        private Button _cmdBrowse;

        internal Button cmdBrowse
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdBrowse;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdBrowse != null)
                {
                    _cmdBrowse.Click -= cmdBrowse_Click;
                }

                _cmdBrowse = value;
                if (_cmdBrowse != null)
                {
                    _cmdBrowse.Click += cmdBrowse_Click;
                }
            }
        }

        private ListView _DBCData;

        internal ListView DBCData
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _DBCData;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_DBCData != null)
                {
                    _DBCData.ColumnClick -= DBCData_ColumnClick;
                }

                _DBCData = value;
                if (_DBCData != null)
                {
                    _DBCData.ColumnClick += DBCData_ColumnClick;
                }
            }
        }

        internal ProgressBar ProgressBar;
        internal BindingSource BindingSource1;
        internal ComboBox cmbColumn;
        private TextBox _txtQuery;

        internal TextBox txtQuery
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _txtQuery;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_txtQuery != null)
                {
                    _txtQuery.KeyDown -= txtQuery_KeyDown;
                }

                _txtQuery = value;
                if (_txtQuery != null)
                {
                    _txtQuery.KeyDown += txtQuery_KeyDown;
                }
            }
        }

        private Button _cmdSearch;

        internal Button cmdSearch
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _cmdSearch;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_cmdSearch != null)
                {
                    _cmdSearch.Click -= cmdSearch_Click;
                }

                _cmdSearch = value;
                if (_cmdSearch != null)
                {
                    _cmdSearch.Click += cmdSearch_Click;
                }
            }
        }
    }
}