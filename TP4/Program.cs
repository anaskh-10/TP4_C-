using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace TP4
{
       static class DB
    {
        public const string Serveur = @"(localdb)\MSSQLLocalDB";
        public static string Cnx = "Data Source=" + Serveur + ";Initial Catalog=BDStock;Integrated Security=True";
        static string Master = "Data Source=" + Serveur + ";Initial Catalog=master;Integrated Security=True";

        const string Schema = @"
IF OBJECT_ID('dbo.Client','U') IS NULL
CREATE TABLE dbo.Client (
    CodeClient int NOT NULL PRIMARY KEY,
    Nom nvarchar(50) NULL, Prenom nvarchar(50) NULL,
    Adresse nvarchar(100) NULL, Telephone nvarchar(20) NULL);

IF OBJECT_ID('dbo.Fournisseur','U') IS NULL
CREATE TABLE dbo.Fournisseur (
    CodeFourn int NOT NULL PRIMARY KEY,
    NomPrenom nvarchar(80) NULL, Adresse nvarchar(100) NULL,
    Telephone nvarchar(20) NULL, Fax nvarchar(20) NULL);

IF OBJECT_ID('dbo.Produit','U') IS NULL
CREATE TABLE dbo.Produit (
    CodeProduit int NOT NULL PRIMARY KEY,
    CodeFourn int NOT NULL,
    NomProduit nvarchar(80) NULL, PrixUnitaire float NULL, QtEnStock int NULL,
    CONSTRAINT FK_Produit_Fournisseur FOREIGN KEY (CodeFourn) REFERENCES dbo.Fournisseur(CodeFourn));

IF OBJECT_ID('dbo.Facture','U') IS NULL
CREATE TABLE dbo.Facture (
    CodeFacture bigint NOT NULL PRIMARY KEY,
    CodeClient int NOT NULL,
    DateFacture datetime NULL, Retenue float NULL,
    CONSTRAINT FK_Facture_Client FOREIGN KEY (CodeClient) REFERENCES dbo.Client(CodeClient));

IF OBJECT_ID('dbo.LigneFacture','U') IS NULL
CREATE TABLE dbo.LigneFacture (
    CodeFacture bigint NOT NULL,
    CodeProduit int NOT NULL,
    Quantite int NULL,
    CONSTRAINT PK_LigneFacture PRIMARY KEY (CodeFacture, CodeProduit),
    CONSTRAINT FK_LigneFacture_Facture1 FOREIGN KEY (CodeFacture) REFERENCES dbo.Facture(CodeFacture),
    CONSTRAINT FK_LigneFacture_Produit FOREIGN KEY (CodeProduit) REFERENCES dbo.Produit(CodeProduit));

IF NOT EXISTS (SELECT 1 FROM dbo.Client)
INSERT INTO dbo.Client VALUES
 (1,N'Ben Salah',N'Ahmed',N'Sousse',N'73100100'),
 (2,N'Trabelsi',N'Sonia',N'Tunis',N'71200200'),
 (3,N'Gharbi',N'Karim',N'Sfax',N'74300300');

IF NOT EXISTS (SELECT 1 FROM dbo.Fournisseur)
INSERT INTO dbo.Fournisseur VALUES
 (1,N'Societe Delice',N'Sousse',N'73111111',N'73111112'),
 (2,N'Tunisie Lait',N'Tunis',N'71222222',N'71222223'),
 (3,N'Mega Distribution',N'Sfax',N'74333333',N'74333334');

IF NOT EXISTS (SELECT 1 FROM dbo.Produit)
INSERT INTO dbo.Produit VALUES
 (1,1,N'Jus d''orange 1L',2.5,100),
 (2,1,N'Biscuits',1.2,250),
 (3,2,N'Lait 1L',1.4,300),
 (4,3,N'Eau minerale 1.5L',0.8,500);

IF NOT EXISTS (SELECT 1 FROM dbo.Facture)
INSERT INTO dbo.Facture VALUES
 (1001,1,'2026-01-10',0),
 (1002,2,'2026-01-12',0.05),
 (1003,3,'2026-01-15',0);

IF NOT EXISTS (SELECT 1 FROM dbo.LigneFacture)
INSERT INTO dbo.LigneFacture VALUES
 (1001,1,10),(1001,3,20),(1002,2,15),(1002,4,30),(1003,3,12);";

        public static void Initialiser()
        {
            using (var c = new SqlConnection(Master))
            {
                c.Open();
                new SqlCommand("IF DB_ID('BDStock') IS NULL CREATE DATABASE BDStock", c).ExecuteNonQuery();
            }
            using (var c = new SqlConnection(Cnx))
            {
                c.Open();
                new SqlCommand(Schema, c).ExecuteNonQuery();
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            try { DB.Initialiser(); }
            catch (Exception ex)
            {
                MessageBox.Show("Impossible de créer/ouvrir la base BDStock :\n" + ex.Message +
                                "\n\nVérifiez que SQL Server LocalDB est installé (voir DB.Serveur).",
                                "Erreur base de données", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Application.Run(new frmAccueil());
        }
    }

       class frmAccueil : Form
    {
        public frmAccueil()
        {
            Text = "Gestion du Stock"; ClientSize = new Size(760, 420);
            StartPosition = FormStartPosition.CenterScreen;
            var b = new Bitmap(400, 300);
            using (var g = Graphics.FromImage(b))
            using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, 400, 300), Color.White, Color.FromArgb(100, 180, 250), 90f))
                g.FillRectangle(br, 0, 0, 400, 300);
            BackgroundImage = b; BackgroundImageLayout = ImageLayout.Stretch;

            var menu = new MenuStrip();
            var mCli = new ToolStripMenuItem("Clients");
            mCli.DropDownItems.Add("Gestion des clients", null, (s, e) => new frmGrille("Client", "Gestion des clients").ShowDialog());

            var mFou = new ToolStripMenuItem("Fournisseurs");
            mFou.DropDownItems.Add("Fiches (navigation)", null, (s, e) => new frmFournisseur().ShowDialog());
            mFou.DropDownItems.Add("Liste (grille)", null, (s, e) => new frmGrille("Fournisseur", "Liste des fournisseurs").ShowDialog());

            var mPro = new ToolStripMenuItem("Produits");
            mPro.DropDownItems.Add("Gestion des produits", null, (s, e) => new frmGrille("Produit", "Gestion des produits").ShowDialog());

            var mFac = new ToolStripMenuItem("Factures");
            mFac.DropDownItems.Add("Gestion des factures", null, (s, e) => new frmFactures().ShowDialog());

            var mAide = new ToolStripMenuItem("?");
            mAide.DropDownItems.Add("A propos", null, (s, e) => MessageBox.Show("TP 4 : Accès aux données\nBase : BDStock (SQL Server LocalDB)", "A propos"));
            mAide.DropDownItems.Add("Quitter", null, (s, e) => Application.Exit());

            menu.Items.AddRange(new ToolStripItem[] { mCli, mFou, mPro, mFac, mAide });
            Controls.Add(menu); MainMenuStrip = menu;
        }
    }

      class frmFournisseur : Form
    {
        // declaration globale
        SqlConnection cnx; SqlCommand cmd; SqlDataAdapter adap;
        SqlCommandBuilder cb; DataSet dset; DataTable tab; DataRow dtr;
        int indice;

        TextBox txtCode, txtNom, txtAdr, txtTel, txtFax;
        Button BTnouveau, BTajout, BTmodif, BTsupp, BTpremier, BTprec, BTsuiv, BTdernier, BTenreg, BTquit;

        public frmFournisseur()
        {
            Text = "Gestion des fournisseurs"; ClientSize = new Size(560, 330);
            StartPosition = FormStartPosition.CenterParent;
            string[] noms = { "Code :", "Nom et prénom :", "Adresse :", "Téléphone :", "Fax :" };
            TextBox[] t = new TextBox[5];
            for (int k = 0; k < 5; k++)
            {
                Controls.Add(new Label { Text = noms[k], Left = 25, Top = 25 + k * 34, Width = 120 });
                t[k] = new TextBox { Left = 150, Top = 22 + k * 34, Width = 250 }; Controls.Add(t[k]);
            }
            txtCode = t[0]; txtNom = t[1]; txtAdr = t[2]; txtTel = t[3]; txtFax = t[4];

            BTpremier = Bt("|<", 25, 205, 60); BTprec = Bt("<", 90, 205, 60);
            BTsuiv = Bt(">", 155, 205, 60); BTdernier = Bt(">|", 220, 205, 60);
            BTnouveau = Bt("Nouveau", 25, 250, 80); BTajout = Bt("Ajouter", 110, 250, 80);
            BTmodif = Bt("Modifier", 195, 250, 80); BTsupp = Bt("Supprimer", 280, 250, 85);
            BTenreg = Bt("Enregistrer", 370, 250, 90); BTquit = Bt("Quitter", 465, 250, 80);

            Load += frmFournisseur_Load;
            BTnouveau.Click += BTnouveau_Click; BTajout.Click += BTajout_Click;
            BTmodif.Click += BTmodif_Click; BTsupp.Click += BTsupp_Click; BTenreg.Click += BTenreg_Click;
            BTpremier.Click += (s, e) => { indice = 0; Afficher(); };
            BTprec.Click += (s, e) => { if (indice > 0) { indice--; Afficher(); } };
            BTsuiv.Click += (s, e) => { if (indice < tab.Rows.Count - 1) { indice++; Afficher(); } };
            BTdernier.Click += (s, e) => { indice = tab.Rows.Count - 1; Afficher(); };
            BTquit.Click += (s, e) => Close();
            FormClosed += (s, e) => { if (cnx != null) cnx.Close(); };
        }

        Button Bt(string t, int x, int y, int w) { var b = new Button { Text = t, Left = x, Top = y, Width = w, Height = 32 }; Controls.Add(b); return b; }

        void Afficher()
        {
            if (tab.Rows.Count > 0 && indice >= 0 && indice < tab.Rows.Count)
            {
                txtCode.Text = tab.Rows[indice][0].ToString(); txtNom.Text = tab.Rows[indice][1].ToString();
                txtAdr.Text = tab.Rows[indice][2].ToString(); txtTel.Text = tab.Rows[indice][3].ToString();
                txtFax.Text = tab.Rows[indice][4].ToString();
            }
            else Vider();
        }
        void Vider() { txtCode.Text = ""; txtNom.Text = ""; txtAdr.Text = ""; txtTel.Text = ""; txtFax.Text = ""; }

        void frmFournisseur_Load(object sender, EventArgs e)
        {
            try
            {
                cnx = new SqlConnection();
                cnx.ConnectionString = DB.Cnx;
                cnx.Open();
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "select * from Fournisseur";
                cmd.Connection = cnx;
                adap = new SqlDataAdapter(cmd);
                cb = new SqlCommandBuilder(adap);       // genere INSERT / UPDATE / DELETE
                dset = new DataSet();
                adap.Fill(dset, "Fournisseur");
                tab = dset.Tables["Fournisseur"];
                indice = 0;
                Afficher();
                BTajout.Enabled = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        void BTnouveau_Click(object sender, EventArgs e)
        {
            Vider(); BTajout.Enabled = true; txtCode.Focus();
        }

        void BTajout_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(txtCode.Text, out int code)) { MessageBox.Show("Le code doit être un entier."); return; }
                if (tab.Rows.Find(code) != null) { MessageBox.Show("Ce code existe déjà."); return; }
                dtr = tab.NewRow();
                dtr[0] = code; dtr[1] = txtNom.Text; dtr[2] = txtAdr.Text; dtr[3] = txtTel.Text; dtr[4] = txtFax.Text;
                tab.Rows.Add(dtr);
                indice = tab.Rows.Count - 1;
                MessageBox.Show("Fournisseur ajouté (cliquez sur Enregistrer pour valider dans la base)");
                BTajout.Enabled = false;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        void BTsupp_Click(object sender, EventArgs e)
        {
            try
            {
                if (tab.Rows.Count == 0) return;
                if (MessageBox.Show("Voulez vous vraiment supprimer ce fournisseur?", "Confirmer suppression", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    tab.Rows[indice].Delete();
                    adap.Update(dset, "Fournisseur");           // applique la suppression dans la base
                    if (indice >= tab.Rows.Count) indice = tab.Rows.Count - 1;
                    if (indice < 0) indice = 0;
                    Afficher();
                    MessageBox.Show("Fournisseur Supprimé");
                }
            }
            catch (Exception ex)
            {
                dset.RejectChanges();   // ex. fournisseur lie a des produits : la base refuse la suppression
                Afficher();
                MessageBox.Show(ex.Message);
            }
        }

        void BTmodif_Click(object sender, EventArgs e)
        {
            try
            {
                if (tab.Rows.Count == 0) return;
                if (!int.TryParse(txtCode.Text, out int code)) { MessageBox.Show("Le code doit être un entier."); return; }
                tab.Rows[indice][0] = code;
                tab.Rows[indice][1] = txtNom.Text; tab.Rows[indice][2] = txtAdr.Text;
                tab.Rows[indice][3] = txtTel.Text; tab.Rows[indice][4] = txtFax.Text;
                MessageBox.Show("Modification effectuée (cliquez sur Enregistrer pour valider dans la base)");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        void BTenreg_Click(object sender, EventArgs e)
        {
            try
            {
                cb = new SqlCommandBuilder(adap);
                adap.Update(dset, "Fournisseur");
                dset.Clear();
                adap.Fill(dset, "Fournisseur");
                tab = dset.Tables["Fournisseur"];
                if (indice >= tab.Rows.Count) indice = tab.Rows.Count - 1;
                if (indice < 0) indice = 0;
                Afficher();
                MessageBox.Show("Données enregistrées dans la base");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }

       class frmGrille : Form
    {
        SqlDataAdapter adap; SqlCommandBuilder cb; DataSet dset;
        DataGridView dgv; string table;

        public frmGrille(string nomTable, string titre)
        {
            table = nomTable;
            Text = titre; ClientSize = new Size(760, 440);
            StartPosition = FormStartPosition.CenterParent;
            dgv = new DataGridView
            {
                Left = 10,
                Top = 10,
                Width = 740,
                Height = 360,
                AllowUserToAddRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };
            Controls.Add(dgv);
            var bSupp = Bt("Supprimer", 10); var bModif = Bt("Enregistrer", 135);
            var bAct = Bt("Actualiser", 260); var bQuit = Bt("Quitter", 640);
            bSupp.Click += Supprimer_Click; bModif.Click += Enregistrer_Click;
            bAct.Click += (s, e) => Charger(); bQuit.Click += (s, e) => Close();
            dgv.DataError += (s, e) => { MessageBox.Show("Valeur invalide : " + e.Exception.Message); e.ThrowException = false; };
            Charger();
        }

        Button Bt(string t, int x) { var b = new Button { Text = t, Left = x, Top = 385, Width = 110, Height = 34 }; Controls.Add(b); return b; }

        void Charger()
        {
            try
            {
                adap = new SqlDataAdapter("select * from " + table, DB.Cnx);
                cb = new SqlCommandBuilder(adap);
                adap.MissingSchemaAction = MissingSchemaAction.AddWithKey;   // recupere la cle primaire
                dset = new DataSet();
                adap.Fill(dset, table);
                dgv.DataSource = dset.Tables[table];
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        void Supprimer_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgv.CurrentRow == null || dgv.CurrentRow.IsNewRow) return;
                if (MessageBox.Show("Voulez vous vraiment supprimer cet enregistrement?", "Confirmer suppression", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    int i = dgv.CurrentRow.Index;
                    dset.Tables[table].Rows[i].Delete();
                    adap.Update(dset, table);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); Charger(); }
        }

        void Enregistrer_Click(object sender, EventArgs e)
        {
            try
            {
                dgv.EndEdit();
                if (MessageBox.Show("Voulez vous enregistrer les modifications?", "Confirmer modification", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    adap.Update(dset, table);
                    MessageBox.Show("Données enregistrées dans la base");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }

       class frmFactures : Form
    {
        SqlDataAdapter adapF, adapL; SqlCommandBuilder cbF, cbL; DataSet dset;
        DataGridView dgvF, dgvL;

        public frmFactures()
        {
            Text = "Gestion des factures"; ClientSize = new Size(760, 560);
            StartPosition = FormStartPosition.CenterParent;
            Controls.Add(new Label { Text = "Factures", Left = 10, Top = 8, Width = 200, Font = new Font("Segoe UI", 10f, FontStyle.Bold) });
            dgvF = new DataGridView { Left = 10, Top = 32, Width = 740, Height = 210, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            Controls.Add(dgvF);
            Controls.Add(new Label { Text = "Lignes de la facture sélectionnée", Left = 10, Top = 252, Width = 300, Font = new Font("Segoe UI", 10f, FontStyle.Bold) });
            dgvL = new DataGridView { Left = 10, Top = 276, Width = 740, Height = 210, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            Controls.Add(dgvL);

            var bEnr = Bt("Enregistrer", 10); var bSupp = Bt("Supprimer facture", 135, 150);
            var bAct = Bt("Actualiser", 295); var bQ = Bt("Quitter", 640);
            bEnr.Click += Enregistrer_Click; bSupp.Click += SupprimerFacture_Click;
            bAct.Click += (s, e) => Charger(); bQ.Click += (s, e) => Close();
            dgvF.DataError += (s, e) => { MessageBox.Show("Valeur invalide : " + e.Exception.Message); e.ThrowException = false; };
            dgvL.DataError += (s, e) => { MessageBox.Show("Valeur invalide : " + e.Exception.Message); e.ThrowException = false; };
            Charger();
        }

        Button Bt(string t, int x, int w = 110) { var b = new Button { Text = t, Left = x, Top = 505, Width = w, Height = 34 }; Controls.Add(b); return b; }

        void Charger()
        {
            try
            {
                adapF = new SqlDataAdapter("select * from Facture", DB.Cnx); cbF = new SqlCommandBuilder(adapF);
                adapL = new SqlDataAdapter("select * from LigneFacture", DB.Cnx); cbL = new SqlCommandBuilder(adapL);
                adapF.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                adapL.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                dset = new DataSet();
                adapF.Fill(dset, "Facture"); adapL.Fill(dset, "LigneFacture");
                dset.Relations.Add(new DataRelation("FactureLignes",
                    dset.Tables["Facture"].Columns["CodeFacture"], dset.Tables["LigneFacture"].Columns["CodeFacture"], false));
                dgvF.DataSource = dset; dgvF.DataMember = "Facture";
                dgvL.DataSource = dgvF.DataSource; dgvL.DataMember = "Facture.FactureLignes";
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        void SupprimerFacture_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvF.CurrentRow == null || dgvF.CurrentRow.IsNewRow) return;
                if (MessageBox.Show("Supprimer cette facture et ses lignes ?", "Confirmer suppression", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                var rowF = ((DataRowView)dgvF.CurrentRow.DataBoundItem).Row;
                foreach (DataRow l in rowF.GetChildRows("FactureLignes")) l.Delete();   // lignes d'abord
                rowF.Delete();
                Enregistrer();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); Charger(); }
        }

        void Enregistrer_Click(object sender, EventArgs e)
        {
            try { dgvF.EndEdit(); dgvL.EndEdit(); Enregistrer(); MessageBox.Show("Données enregistrées dans la base"); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        void Enregistrer()
        {
            DataTable dtL = dset.Tables["LigneFacture"], dtF = dset.Tables["Facture"];
            adapL.Update(dtL.Select(null, null, DataViewRowState.Deleted));
            adapF.Update(dtF);
            adapL.Update(dtL);
        }
    }
}