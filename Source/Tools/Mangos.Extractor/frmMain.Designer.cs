using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Microsoft.VisualBasic.CompilerServices;

namespace Mangos.Extractor
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
            _btnExtractUpdateFields = new Button();
            _btnExtractUpdateFields.Click += new EventHandler(btnExtractUpdateFields_Click);
            _btnExtractOpcodes = new Button();
            _btnExtractOpcodes.Click += new EventHandler(btnExtractOpcodes_Click);
            _btnExtractSpellFailedReasons = new Button();
            _btnExtractSpellFailedReasons.Click += new EventHandler(btnExtractSpellFailedReasons_Click);
            _btnExtractChatTypes = new Button();
            _btnExtractChatTypes.Click += new EventHandler(btnExtractChatTypes_Click);
            SuspendLayout();
            // 
            // btnExtractUpdateFields
            // 
            _btnExtractUpdateFields.Location = new Point(12, 74);
            _btnExtractUpdateFields.Name = "_btnExtractUpdateFields";
            _btnExtractUpdateFields.Size = new Size(161, 56);
            _btnExtractUpdateFields.TabIndex = 0;
            _btnExtractUpdateFields.Text = "Extract UPDATE FIELDs";
            _btnExtractUpdateFields.UseVisualStyleBackColor = true;
            // 
            // btnExtractOpcodes
            // 
            _btnExtractOpcodes.Location = new Point(12, 12);
            _btnExtractOpcodes.Name = "_btnExtractOpcodes";
            _btnExtractOpcodes.Size = new Size(161, 56);
            _btnExtractOpcodes.TabIndex = 1;
            _btnExtractOpcodes.Text = "Extract OPCODEs";
            _btnExtractOpcodes.UseVisualStyleBackColor = true;
            // 
            // btnExtractSpellFailedReasons
            // 
            _btnExtractSpellFailedReasons.Location = new Point(12, 136);
            _btnExtractSpellFailedReasons.Name = "_btnExtractSpellFailedReasons";
            _btnExtractSpellFailedReasons.Size = new Size(161, 56);
            _btnExtractSpellFailedReasons.TabIndex = 2;
            _btnExtractSpellFailedReasons.Text = "Extract SpellFailedReasons";
            _btnExtractSpellFailedReasons.UseVisualStyleBackColor = true;
            // 
            // btnExtractChatTypes
            // 
            _btnExtractChatTypes.Location = new Point(12, 198);
            _btnExtractChatTypes.Name = "_btnExtractChatTypes";
            _btnExtractChatTypes.Size = new Size(161, 56);
            _btnExtractChatTypes.TabIndex = 3;
            _btnExtractChatTypes.Text = "Extract ChatTypes";
            _btnExtractChatTypes.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            AutoScaleDimensions = new SizeF(6.0f, 13.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(188, 263);
            Controls.Add(_btnExtractChatTypes);
            Controls.Add(_btnExtractSpellFailedReasons);
            Controls.Add(_btnExtractOpcodes);
            Controls.Add(_btnExtractUpdateFields);
            Name = "frmMain";
            Text = "WoW Extractor";
            ResumeLayout(false);
        }

        private Button _btnExtractUpdateFields;

        internal Button btnExtractUpdateFields
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _btnExtractUpdateFields;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_btnExtractUpdateFields != null)
                {
                    _btnExtractUpdateFields.Click -= btnExtractUpdateFields_Click;
                }

                _btnExtractUpdateFields = value;
                if (_btnExtractUpdateFields != null)
                {
                    _btnExtractUpdateFields.Click += btnExtractUpdateFields_Click;
                }
            }
        }

        private Button _btnExtractOpcodes;

        internal Button btnExtractOpcodes
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _btnExtractOpcodes;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_btnExtractOpcodes != null)
                {
                    _btnExtractOpcodes.Click -= btnExtractOpcodes_Click;
                }

                _btnExtractOpcodes = value;
                if (_btnExtractOpcodes != null)
                {
                    _btnExtractOpcodes.Click += btnExtractOpcodes_Click;
                }
            }
        }

        private Button _btnExtractSpellFailedReasons;

        internal Button btnExtractSpellFailedReasons
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _btnExtractSpellFailedReasons;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_btnExtractSpellFailedReasons != null)
                {
                    _btnExtractSpellFailedReasons.Click -= btnExtractSpellFailedReasons_Click;
                }

                _btnExtractSpellFailedReasons = value;
                if (_btnExtractSpellFailedReasons != null)
                {
                    _btnExtractSpellFailedReasons.Click += btnExtractSpellFailedReasons_Click;
                }
            }
        }

        private Button _btnExtractChatTypes;

        internal Button btnExtractChatTypes
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get
            {
                return _btnExtractChatTypes;
            }

            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                if (_btnExtractChatTypes != null)
                {
                    _btnExtractChatTypes.Click -= btnExtractChatTypes_Click;
                }

                _btnExtractChatTypes = value;
                if (_btnExtractChatTypes != null)
                {
                    _btnExtractChatTypes.Click += btnExtractChatTypes_Click;
                }
            }
        }
    }
}