namespace LRDetect
{
    partial class MainForm
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
      this.btn_CollectInfo = new System.Windows.Forms.Button();
      this.btn_SendInfo = new System.Windows.Forms.Button();
      this.progressBar1 = new System.Windows.Forms.ProgressBar();
      this.menuStrip1 = new System.Windows.Forms.MenuStrip();
      this.includeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.systemProcessesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.installedProgramsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.networkToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.dllsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.relatedLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.bgWorker = new System.ComponentModel.BackgroundWorker();
      this.progressStatus = new System.Windows.Forms.Label();
      this.windowsUpdatesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
      this.menuStrip1.SuspendLayout();
      this.SuspendLayout();
      // 
      // btn_CollectInfo
      // 
      this.btn_CollectInfo.Location = new System.Drawing.Point(11, 35);
      this.btn_CollectInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.btn_CollectInfo.Name = "btn_CollectInfo";
      this.btn_CollectInfo.Size = new System.Drawing.Size(80, 41);
      this.btn_CollectInfo.TabIndex = 0;
      this.btn_CollectInfo.Text = "Collect Info";
      this.btn_CollectInfo.UseVisualStyleBackColor = true;
      this.btn_CollectInfo.Click += new System.EventHandler(this.btn_CollectInfo_Click);
      // 
      // btn_SendInfo
      // 
      this.btn_SendInfo.Enabled = false;
      this.btn_SendInfo.Location = new System.Drawing.Point(110, 35);
      this.btn_SendInfo.Name = "btn_SendInfo";
      this.btn_SendInfo.Size = new System.Drawing.Size(80, 41);
      this.btn_SendInfo.TabIndex = 2;
      this.btn_SendInfo.Text = "Send Info";
      this.btn_SendInfo.UseVisualStyleBackColor = true;
      this.btn_SendInfo.Click += new System.EventHandler(this.btn_SendInfo_Click);
      // 
      // progressBar1
      // 
      this.progressBar1.Location = new System.Drawing.Point(11, 110);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new System.Drawing.Size(178, 23);
      this.progressBar1.TabIndex = 4;
      // 
      // menuStrip1
      // 
      this.menuStrip1.AutoSize = false;
      this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
      this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(0);
      this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.includeToolStripMenuItem});
      this.menuStrip1.Location = new System.Drawing.Point(5, 0);
      this.menuStrip1.Name = "menuStrip1";
      this.menuStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
      this.menuStrip1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.menuStrip1.Size = new System.Drawing.Size(194, 24);
      this.menuStrip1.TabIndex = 5;
      this.menuStrip1.Text = "menuStrip1";
      // 
      // includeToolStripMenuItem
      // 
      this.includeToolStripMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
      this.includeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.systemProcessesToolStripMenuItem,
            this.installedProgramsToolStripMenuItem,
            this.windowsUpdatesToolStripMenuItem,
            this.networkToolStripMenuItem,
            this.dllsToolStripMenuItem,
            this.relatedLogsToolStripMenuItem});
      this.includeToolStripMenuItem.Name = "includeToolStripMenuItem";
      this.includeToolStripMenuItem.Size = new System.Drawing.Size(58, 20);
      this.includeToolStripMenuItem.Text = "Include";
      // 
      // systemProcessesToolStripMenuItem
      // 
      this.systemProcessesToolStripMenuItem.Checked = true;
      this.systemProcessesToolStripMenuItem.CheckOnClick = true;
      this.systemProcessesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.systemProcessesToolStripMenuItem.Name = "systemProcessesToolStripMenuItem";
      this.systemProcessesToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.systemProcessesToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
      this.systemProcessesToolStripMenuItem.Text = "System processes";
      this.systemProcessesToolStripMenuItem.ToolTipText = "Adds information about the running processes";
      this.systemProcessesToolStripMenuItem.Click += new System.EventHandler(this.DetailsToolStripMenuItem_Click);
      // 
      // installedProgramsToolStripMenuItem
      // 
      this.installedProgramsToolStripMenuItem.Checked = true;
      this.installedProgramsToolStripMenuItem.CheckOnClick = true;
      this.installedProgramsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
      this.installedProgramsToolStripMenuItem.Name = "installedProgramsToolStripMenuItem";
      this.installedProgramsToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.installedProgramsToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
      this.installedProgramsToolStripMenuItem.Text = "Installed Programs";
      this.installedProgramsToolStripMenuItem.ToolTipText = "Adds information about all insatlled programs";
      this.installedProgramsToolStripMenuItem.Click += new System.EventHandler(this.DetailsToolStripMenuItem_Click);
      // 
      // networkToolStripMenuItem
      // 
      this.networkToolStripMenuItem.CheckOnClick = true;
      this.networkToolStripMenuItem.Name = "networkToolStripMenuItem";
      this.networkToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.networkToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
      this.networkToolStripMenuItem.Text = "Network information";
      this.networkToolStripMenuItem.ToolTipText = "Adds the output of \'ipconfig /all\' command";
      this.networkToolStripMenuItem.Click += new System.EventHandler(this.DetailsToolStripMenuItem_Click);
      // 
      // dllsToolStripMenuItem
      // 
      this.dllsToolStripMenuItem.CheckOnClick = true;
      this.dllsToolStripMenuItem.Name = "dllsToolStripMenuItem";
      this.dllsToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.dllsToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
      this.dllsToolStripMenuItem.Text = "DLLs versions";
      this.dllsToolStripMenuItem.ToolTipText = "Adds a version check for all DLLs in the \'bin\' folder. NOTE: It is time consuming" +
    "";
      this.dllsToolStripMenuItem.Click += new System.EventHandler(this.DetailsToolStripMenuItem_Click);
      // 
      // relatedLogsToolStripMenuItem
      // 
      this.relatedLogsToolStripMenuItem.CheckOnClick = true;
      this.relatedLogsToolStripMenuItem.Name = "relatedLogsToolStripMenuItem";
      this.relatedLogsToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.relatedLogsToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
      this.relatedLogsToolStripMenuItem.Text = "Related logs";
      this.relatedLogsToolStripMenuItem.Click += new System.EventHandler(this.DetailsToolStripMenuItem_Click);
      // 
      // bgWorker
      // 
      this.bgWorker.WorkerReportsProgress = true;
      this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
      this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
      this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
      // 
      // progressStatus
      // 
      this.progressStatus.Location = new System.Drawing.Point(12, 87);
      this.progressStatus.Name = "progressStatus";
      this.progressStatus.Size = new System.Drawing.Size(170, 16);
      this.progressStatus.TabIndex = 6;
      this.progressStatus.Text = "Click \'Collect Info\' to start                  ";
      // 
      // windowsUpdatesToolStripMenuItem
      // 
      this.windowsUpdatesToolStripMenuItem.CheckOnClick = true;
      this.windowsUpdatesToolStripMenuItem.Name = "windowsUpdatesToolStripMenuItem";
      this.windowsUpdatesToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
      this.windowsUpdatesToolStripMenuItem.Size = new System.Drawing.Size(185, 22);
      this.windowsUpdatesToolStripMenuItem.Text = "Windows updates";
      this.windowsUpdatesToolStripMenuItem.Click += new System.EventHandler(this.DetailsToolStripMenuItem_Click);
      // 
      // MainForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(203, 149);
      this.Controls.Add(this.progressStatus);
      this.Controls.Add(this.progressBar1);
      this.Controls.Add(this.btn_SendInfo);
      this.Controls.Add(this.btn_CollectInfo);
      this.Controls.Add(this.menuStrip1);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MainMenuStrip = this.menuStrip1;
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "MainForm";
      this.Text = "LR Detect Tool";
      this.menuStrip1.ResumeLayout(false);
      this.menuStrip1.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_CollectInfo;
        private System.Windows.Forms.Button btn_SendInfo;
        public System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem includeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem networkToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dllsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem systemProcessesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem relatedLogsToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker bgWorker;
        public System.Windows.Forms.Label progressStatus;
        private System.Windows.Forms.ToolStripMenuItem installedProgramsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowsUpdatesToolStripMenuItem;
    }
}

