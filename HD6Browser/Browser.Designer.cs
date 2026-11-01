namespace HD6Browser
{
    partial class Browser
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            treeViewFiles = new TreeView();
            listViewFiles = new ListView();
            buttonOpen = new Button();
            buttonExtract = new Button();
            buttonExtractCurrent = new Button();
            extractionPanel = new TableLayoutPanel();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(treeViewFiles);
            splitContainer1.Panel1.Controls.Add(buttonOpen);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(listViewFiles);
            splitContainer1.Panel2.Controls.Add(extractionPanel);
            splitContainer1.Size = new Size(800, 450);
            splitContainer1.SplitterDistance = 266;
            splitContainer1.TabIndex = 0;
            // 
            // treeViewFiles
            // 
            treeViewFiles.Dock = DockStyle.Fill;
            treeViewFiles.Location = new Point(0, 0);
            treeViewFiles.Name = "treeViewFiles";
            treeViewFiles.Size = new Size(266, 410);
            treeViewFiles.TabIndex = 0;
            // 
            // listViewFiles
            // 
            listViewFiles.Dock = DockStyle.Fill;
            listViewFiles.Location = new Point(0, 0);
            listViewFiles.Name = "listViewFiles";
            listViewFiles.Size = new Size(530, 410);
            listViewFiles.TabIndex = 0;
            listViewFiles.UseCompatibleStateImageBehavior = false;
            listViewFiles.MultiSelect = true;
            listViewFiles.SelectedIndexChanged += listViewFiles_SelectedIndexChanged;
            // 
            // buttonOpen
            // 
            buttonOpen.Dock = DockStyle.Top;
            buttonOpen.Location = new Point(0, 0);
            buttonOpen.Name = "buttonOpen";
            buttonOpen.Size = new Size(266, 40);
            buttonOpen.TabIndex = 1;
            buttonOpen.Text = "Open HD6...";
            buttonOpen.UseVisualStyleBackColor = true;
            buttonOpen.Click += ButtonOpen_Click;
            // 
            // buttonExtract
            // 
            buttonExtract.Dock = DockStyle.Fill;
            buttonExtract.Location = new Point(265, 0);
            buttonExtract.Name = "buttonExtract";
            buttonExtract.Size = new Size(265, 40);
            buttonExtract.TabIndex = 1;
            buttonExtract.Text = "Extract Selected (Archive Root)...";
            buttonExtract.Enabled = false;
            buttonExtract.UseVisualStyleBackColor = true;
            buttonExtract.Click += ButtonExtract_Click;
            // 
            // buttonExtractCurrent
            // 
            buttonExtractCurrent.Dock = DockStyle.Fill;
            buttonExtractCurrent.Location = new Point(0, 0);
            buttonExtractCurrent.Name = "buttonExtractCurrent";
            buttonExtractCurrent.Size = new Size(265, 40);
            buttonExtractCurrent.TabIndex = 2;
            buttonExtractCurrent.Text = "Extract Selected (Current Folder)...";
            buttonExtractCurrent.Enabled = false;
            buttonExtractCurrent.UseVisualStyleBackColor = true;
            buttonExtractCurrent.Click += ButtonExtractCurrent_Click;

            // 
            // extractionPanel
            // 
            extractionPanel.ColumnCount = 2;
            extractionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            extractionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            extractionPanel.Controls.Add(buttonExtractCurrent, 0, 0);
            extractionPanel.Controls.Add(buttonExtract, 1, 0);
            extractionPanel.Dock = DockStyle.Top;
            extractionPanel.Location = new Point(0, 0);
            extractionPanel.Name = "extractionPanel";
            extractionPanel.RowCount = 1;
            extractionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            extractionPanel.Size = new Size(530, 40);
            // 
            // Browser
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 600);
            Controls.Add(splitContainer1);
            Name = "Browser";
            Text = "HD6 Browser";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private TreeView treeViewFiles;
        private ListView listViewFiles;
        private Button buttonOpen;
        private Button buttonExtract;
        private Button buttonExtractCurrent;
        private TableLayoutPanel extractionPanel;
    }
}
