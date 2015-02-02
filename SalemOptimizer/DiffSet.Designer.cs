namespace SalemOptimizer
{
    partial class DiffSet
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
            this.dGrid = new System.Windows.Forms.DataGridView();
            this.id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.inspName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Diff = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // dGrid
            // 
            this.dGrid.AllowUserToAddRows = false;
            this.dGrid.AllowUserToDeleteRows = false;
            this.dGrid.AllowUserToOrderColumns = true;
            this.dGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.id,
            this.inspName,
            this.Diff});
            this.dGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dGrid.Location = new System.Drawing.Point(0, 0);
            this.dGrid.Name = "dGrid";
            this.dGrid.Size = new System.Drawing.Size(284, 261);
            this.dGrid.TabIndex = 0;
            this.dGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dGrid_CellEndEdit);
            // 
            // id
            // 
            this.id.HeaderText = "ID";
            this.id.Name = "id";
            this.id.Visible = false;
            // 
            // inspName
            // 
            this.inspName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.inspName.HeaderText = "Name";
            this.inspName.Name = "inspName";
            this.inspName.ReadOnly = true;
            // 
            // Diff
            // 
            this.Diff.HeaderText = "Difficulty";
            this.Diff.Name = "Diff";
            // 
            // DiffSet
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.dGrid);
            this.Name = "DiffSet";
            this.Text = "DiffSet";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.DiffSet_FormClosed);
            this.Load += new System.EventHandler(this.DiffSet_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn id;
        private System.Windows.Forms.DataGridViewTextBoxColumn inspName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Diff;
    }
}