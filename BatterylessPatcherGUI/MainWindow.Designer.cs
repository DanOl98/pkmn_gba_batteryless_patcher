namespace BatterylessPatcher
{
    partial class MainWindow
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
            btnBrowse = new Button();
            repackCheck = new CheckBox();
            btnPatchGood2 = new Button();
            btnHardRepackOnly = new Button();
            ofd = new OpenFileDialog();
            txtRomPath = new TextBox();
            outputText = new RichTextBox();
            btnSoftRepackOnly = new Button();
            txtSavePath = new TextBox();
            btnSaveBrowse = new Button();
            label1 = new Label();
            label2 = new Label();
            ofd2 = new OpenFileDialog();
            btnRemoveSave = new Button();
            SuspendLayout();
            // 
            // btnBrowse
            // 
            btnBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBrowse.Location = new Point(796, 41);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(115, 37);
            btnBrowse.TabIndex = 0;
            btnBrowse.Text = "Choose ROM";
            btnBrowse.Click += BtnBrowse_Click;
            // 
            // repackCheck
            // 
            repackCheck.Location = new Point(12, 212);
            repackCheck.Name = "repackCheck";
            repackCheck.Size = new Size(160, 50);
            repackCheck.TabIndex = 2;
            repackCheck.Text = "Force repack while applying patch";
            // 
            // btnPatchGood2
            // 
            btnPatchGood2.Location = new Point(12, 156);
            btnPatchGood2.Name = "btnPatchGood2";
            btnPatchGood2.Size = new Size(160, 50);
            btnPatchGood2.TabIndex = 5;
            btnPatchGood2.Text = "Apply Patch";
            btnPatchGood2.Click += BtnPatchGood2_Click;
            // 
            // btnHardRepackOnly
            // 
            btnHardRepackOnly.Location = new Point(12, 540);
            btnHardRepackOnly.Name = "btnHardRepackOnly";
            btnHardRepackOnly.Size = new Size(160, 50);
            btnHardRepackOnly.TabIndex = 6;
            btnHardRepackOnly.Text = "Hard Repack only";
            btnHardRepackOnly.Click += BtnRepackOnly_Click;
            // 
            // ofd
            // 
            ofd.Filter = "GBA ROM|*.gba";
            // 
            // txtRomPath
            // 
            txtRomPath.Location = new Point(12, 49);
            txtRomPath.Name = "txtRomPath";
            txtRomPath.ReadOnly = true;
            txtRomPath.Size = new Size(778, 23);
            txtRomPath.TabIndex = 1;
            txtRomPath.Text = "No ROM selected";
            // 
            // outputText
            // 
            outputText.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            outputText.Location = new Point(178, 156);
            outputText.Name = "outputText";
            outputText.ReadOnly = true;
            outputText.Size = new Size(733, 434);
            outputText.TabIndex = 7;
            outputText.Text = "";
            outputText.TextChanged += outputText_TextChanged;
            // 
            // btnSoftRepackOnly
            // 
            btnSoftRepackOnly.Location = new Point(12, 484);
            btnSoftRepackOnly.Name = "btnSoftRepackOnly";
            btnSoftRepackOnly.Size = new Size(160, 50);
            btnSoftRepackOnly.TabIndex = 6;
            btnSoftRepackOnly.Text = "Soft Repack only";
            btnSoftRepackOnly.Click += btnSoftRepackOnly_Click;
            // 
            // txtSavePath
            // 
            txtSavePath.Location = new Point(12, 121);
            txtSavePath.Name = "txtSavePath";
            txtSavePath.ReadOnly = true;
            txtSavePath.Size = new Size(656, 23);
            txtSavePath.TabIndex = 9;
            txtSavePath.TextChanged += txtSavePath_TextChanged;
            // 
            // btnSaveBrowse
            // 
            btnSaveBrowse.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveBrowse.Location = new Point(796, 113);
            btnSaveBrowse.Name = "btnSaveBrowse";
            btnSaveBrowse.Size = new Size(115, 37);
            btnSaveBrowse.TabIndex = 8;
            btnSaveBrowse.Text = "Choose save";
            btnSaveBrowse.Click += btnSaveBrowse_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label1.Location = new Point(12, 13);
            label1.Name = "label1";
            label1.Size = new Size(47, 21);
            label1.TabIndex = 10;
            label1.Text = "ROM";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            label2.Location = new Point(12, 86);
            label2.Name = "label2";
            label2.Size = new Size(152, 21);
            label2.TabIndex = 11;
            label2.Text = "Savedata injection";
            // 
            // ofd2
            // 
            ofd2.Filter = "Savedata|*.sav";
            // 
            // btnRemoveSave
            // 
            btnRemoveSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRemoveSave.Location = new Point(674, 113);
            btnRemoveSave.Name = "btnRemoveSave";
            btnRemoveSave.Size = new Size(115, 37);
            btnRemoveSave.TabIndex = 12;
            btnRemoveSave.Text = "Remove selection";
            btnRemoveSave.Click += btnRemoveSave_Click;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(923, 602);
            Controls.Add(btnRemoveSave);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(txtSavePath);
            Controls.Add(btnSaveBrowse);
            Controls.Add(outputText);
            Controls.Add(txtRomPath);
            Controls.Add(btnBrowse);
            Controls.Add(repackCheck);
            Controls.Add(btnPatchGood2);
            Controls.Add(btnSoftRepackOnly);
            Controls.Add(btnHardRepackOnly);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainWindow";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "GBA Pokemon Batteryless Patcher";
            Load += MainWindow_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnBrowse;
        private Button btnPatchGood2;
        private Button btnHardRepackOnly;
        private CheckBox repackCheck;
        private OpenFileDialog ofd;
        private TextBox txtRomPath;
        private RichTextBox outputText;
        private Button btnSoftRepackOnly;
        private TextBox txtSavePath;
        private Button btnSaveBrowse;
        private Label label1;
        private Label label2;
        private OpenFileDialog ofd2;
        private Button btnRemoveSave;
    }
}