namespace CompiladorModula2
{
    partial class Form1
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
            label7 = new Label();
            label6 = new Label();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            dgvTablaSimbolos = new DataGridView();
            btnAnalisisLexico = new Button();
            txtBloqueCodigo = new TextBox();
            label2 = new Label();
            label1 = new Label();
            tvArbol = new TreeView();
            lbControl = new ListBox();
            lbTokens = new ListBox();
            ((System.ComponentModel.ISupportInitialize)dgvTablaSimbolos).BeginInit();
            SuspendLayout();
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Microsoft JhengHei", 12F, FontStyle.Bold);
            label7.Location = new Point(1602, 143);
            label7.Margin = new Padding(5, 0, 5, 0);
            label7.Name = "label7";
            label7.Size = new Size(78, 25);
            label7.TabIndex = 25;
            label7.Text = "ÁRBOL";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Microsoft JhengHei", 12F, FontStyle.Bold);
            label6.Location = new Point(827, 487);
            label6.Margin = new Padding(5, 0, 5, 0);
            label6.Name = "label6";
            label6.Size = new Size(83, 25);
            label6.TabIndex = 23;
            label6.Text = "SALIDA";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Microsoft JhengHei", 12F, FontStyle.Bold);
            label5.Location = new Point(1324, 143);
            label5.Margin = new Padding(5, 0, 5, 0);
            label5.Name = "label5";
            label5.Size = new Size(92, 25);
            label5.TabIndex = 22;
            label5.Text = "TOKENS";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Microsoft JhengHei", 12F, FontStyle.Bold);
            label4.Location = new Point(763, 143);
            label4.Margin = new Padding(5, 0, 5, 0);
            label4.Name = "label4";
            label4.Size = new Size(216, 25);
            label4.TabIndex = 19;
            label4.Text = "TABLA DE SIMBOLOS";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Microsoft JhengHei", 12F, FontStyle.Bold);
            label3.Location = new Point(169, 143);
            label3.Margin = new Padding(5, 0, 5, 0);
            label3.Name = "label3";
            label3.Size = new Size(93, 25);
            label3.TabIndex = 18;
            label3.Text = "CÓDIGO";
            // 
            // dgvTablaSimbolos
            // 
            dgvTablaSimbolos.ColumnHeadersHeight = 29;
            dgvTablaSimbolos.Location = new Point(458, 174);
            dgvTablaSimbolos.Margin = new Padding(5, 4, 5, 4);
            dgvTablaSimbolos.Name = "dgvTablaSimbolos";
            dgvTablaSimbolos.RowHeadersWidth = 51;
            dgvTablaSimbolos.Size = new Size(807, 304);
            dgvTablaSimbolos.TabIndex = 17;
            dgvTablaSimbolos.CellContentClick += dgvTablaSimbolos_CellContentClick;
            // 
            // btnAnalisisLexico
            // 
            btnAnalisisLexico.Cursor = Cursors.Hand;
            btnAnalisisLexico.Font = new Font("Microsoft JhengHei", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnAnalisisLexico.Location = new Point(123, 649);
            btnAnalisisLexico.Margin = new Padding(5, 4, 5, 4);
            btnAnalisisLexico.Name = "btnAnalisisLexico";
            btnAnalisisLexico.Size = new Size(184, 58);
            btnAnalisisLexico.TabIndex = 16;
            btnAnalisisLexico.Text = "ANÁLISIS LÉXICO";
            btnAnalisisLexico.UseVisualStyleBackColor = true;
            btnAnalisisLexico.Click += btnAnalisisLexico_Click;
            // 
            // txtBloqueCodigo
            // 
            txtBloqueCodigo.BorderStyle = BorderStyle.FixedSingle;
            txtBloqueCodigo.Font = new Font("Consolas", 10.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtBloqueCodigo.Location = new Point(25, 174);
            txtBloqueCodigo.Margin = new Padding(5, 4, 5, 4);
            txtBloqueCodigo.Multiline = true;
            txtBloqueCodigo.Name = "txtBloqueCodigo";
            txtBloqueCodigo.Size = new Size(415, 467);
            txtBloqueCodigo.TabIndex = 15;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold);
            label2.Location = new Point(859, 79);
            label2.Margin = new Padding(5, 0, 5, 0);
            label2.Name = "label2";
            label2.Size = new Size(177, 41);
            label2.TabIndex = 14;
            label2.Text = "MODULA-2";
            label2.Click += label2_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Franklin Gothic Demi", 25.8000011F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(758, 28);
            label1.Margin = new Padding(5, 0, 5, 0);
            label1.Name = "label1";
            label1.Size = new Size(390, 51);
            label1.TabIndex = 13;
            label1.Text = "C O M P I L A D O R";
            // 
            // tvArbol
            // 
            tvArbol.Font = new Font("Consolas", 10.8F);
            tvArbol.Location = new Point(1486, 174);
            tvArbol.Margin = new Padding(5, 4, 5, 4);
            tvArbol.Name = "tvArbol";
            tvArbol.Size = new Size(311, 529);
            tvArbol.TabIndex = 27;
            // 
            // lbControl
            // 
            lbControl.Font = new Font("Consolas", 10.8F);
            lbControl.FormattingEnabled = true;
            lbControl.ItemHeight = 22;
            lbControl.Location = new Point(458, 521);
            lbControl.Margin = new Padding(5, 4, 5, 4);
            lbControl.Name = "lbControl";
            lbControl.Size = new Size(807, 180);
            lbControl.TabIndex = 28;
            // 
            // lbTokens
            // 
            lbTokens.Font = new Font("Century Gothic", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lbTokens.FormattingEnabled = true;
            lbTokens.ItemHeight = 21;
            lbTokens.Location = new Point(1285, 174);
            lbTokens.Margin = new Padding(5, 4, 5, 4);
            lbTokens.Name = "lbTokens";
            lbTokens.Size = new Size(184, 508);
            lbTokens.TabIndex = 29;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(14F, 27F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1815, 732);
            Controls.Add(lbTokens);
            Controls.Add(lbControl);
            Controls.Add(tvArbol);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(dgvTablaSimbolos);
            Controls.Add(btnAnalisisLexico);
            Controls.Add(txtBloqueCodigo);
            Controls.Add(label2);
            Controls.Add(label1);
            Font = new Font("Century Gothic", 13.8F, FontStyle.Bold);
            Margin = new Padding(5, 4, 5, 4);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvTablaSimbolos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label7;
        private Label label6;
        private Label label5;
        private Label label4;
        private Label label3;
        private DataGridView dgvTablaSimbolos;
        private Button btnAnalisisLexico;
        private TextBox txtBloqueCodigo;
        private Label label2;
        private Label label1;
        private TreeView tvArbol;
        private ListBox lbControl;
        private ListBox lbTokens;
    }
}
