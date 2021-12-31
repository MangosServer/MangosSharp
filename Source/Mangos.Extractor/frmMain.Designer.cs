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
        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this._btnExtractUpdateFields = new System.Windows.Forms.Button();
            this._btnExtractOpcodes = new System.Windows.Forms.Button();
            this._btnExtractSpellFailedReasons = new System.Windows.Forms.Button();
            this._btnExtractChatTypes = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // _btnExtractUpdateFields
            // 
            this._btnExtractUpdateFields.Location = new System.Drawing.Point(14, 85);
            this._btnExtractUpdateFields.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnExtractUpdateFields.Name = "_btnExtractUpdateFields";
            this._btnExtractUpdateFields.Size = new System.Drawing.Size(188, 65);
            this._btnExtractUpdateFields.TabIndex = 0;
            this._btnExtractUpdateFields.Text = "Extract UPDATE FIELDs";
            this._btnExtractUpdateFields.UseVisualStyleBackColor = true;
            this._btnExtractUpdateFields.Click += new System.EventHandler(this.btnExtractUpdateFields_Click);
            // 
            // _btnExtractOpcodes
            // 
            this._btnExtractOpcodes.Location = new System.Drawing.Point(14, 14);
            this._btnExtractOpcodes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnExtractOpcodes.Name = "_btnExtractOpcodes";
            this._btnExtractOpcodes.Size = new System.Drawing.Size(188, 65);
            this._btnExtractOpcodes.TabIndex = 1;
            this._btnExtractOpcodes.Text = "Extract OPCODEs";
            this._btnExtractOpcodes.UseVisualStyleBackColor = true;
            this._btnExtractOpcodes.Click += new System.EventHandler(this.btnExtractOpcodes_Click);
            // 
            // _btnExtractSpellFailedReasons
            // 
            this._btnExtractSpellFailedReasons.Location = new System.Drawing.Point(14, 157);
            this._btnExtractSpellFailedReasons.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnExtractSpellFailedReasons.Name = "_btnExtractSpellFailedReasons";
            this._btnExtractSpellFailedReasons.Size = new System.Drawing.Size(188, 65);
            this._btnExtractSpellFailedReasons.TabIndex = 2;
            this._btnExtractSpellFailedReasons.Text = "Extract SpellFailedReasons";
            this._btnExtractSpellFailedReasons.UseVisualStyleBackColor = true;
            this._btnExtractSpellFailedReasons.Click += new System.EventHandler(this.btnExtractSpellFailedReasons_Click);
            // 
            // _btnExtractChatTypes
            // 
            this._btnExtractChatTypes.Location = new System.Drawing.Point(14, 228);
            this._btnExtractChatTypes.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this._btnExtractChatTypes.Name = "_btnExtractChatTypes";
            this._btnExtractChatTypes.Size = new System.Drawing.Size(188, 65);
            this._btnExtractChatTypes.TabIndex = 3;
            this._btnExtractChatTypes.Text = "Extract ChatTypes";
            this._btnExtractChatTypes.UseVisualStyleBackColor = true;
            this._btnExtractChatTypes.Click += new System.EventHandler(this.btnExtractChatTypes_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(219, 303);
            this.Controls.Add(this._btnExtractChatTypes);
            this.Controls.Add(this._btnExtractSpellFailedReasons);
            this.Controls.Add(this._btnExtractOpcodes);
            this.Controls.Add(this._btnExtractUpdateFields);
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "frmMain";
            this.Text = "WoW Extractor";
            this.ResumeLayout(false);

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
