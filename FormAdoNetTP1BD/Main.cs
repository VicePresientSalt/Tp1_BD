﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Oracle.DataAccess.Client;

namespace FormAdoNet
{
    public partial class GestionEmploye : Form
    {

        public GestionEmploye()
        {
            InitializeComponent();
        }

        private OracleConnection conn = new OracleConnection();
        private DataSet Data = new DataSet();
        private string Codedept = null;

        private void UpdateControls()
        {
            if (conn.State.ToString() != "Open")
            {
                BTN_First.Enabled = false;
                BTN_Suivant.Enabled = false;
                BTN_Prec.Enabled = false;
                BTN_Fin.Enabled = false;
                BTN_First.Enabled = false;
                BTN_Mod.Enabled = false;
                BTN_Supprimer.Enabled = false;
                TB_NoEMP.Enabled = false;
                TB_Prenom.Enabled = false;
                TB_Salaire.Enabled = false;
                TB_Nom.Enabled = false;
                LB_Dept.Items.Clear();
                LB_Dept.Enabled = false;
                BTN_Add.Enabled = false;
            }
            if (TB_NoEMP.Text == "")
            {
                BTN_First.Enabled = false;
                BTN_Prec.Enabled = false;
                BTN_Suivant.Enabled = false;
                BTN_Fin.Enabled = false;
                BTN_Mod.Enabled = false;
                BTN_Supprimer.Enabled = false;
            }
            else
            {
                BTN_First.Enabled = true;
                BTN_Prec.Enabled = true;
                BTN_Suivant.Enabled = true;
                BTN_Fin.Enabled = true;
                BTN_Mod.Enabled = true;
                BTN_Supprimer.Enabled = true;

            }
        }

        private void UpdateTextBox()
        {
            TB_NoEMP.DataBindings.Add("text", Data, "Resultats.Numemp");
            TB_Nom.DataBindings.Add("text", Data, "Resultats.Nomemp");
            TB_Prenom.DataBindings.Add("text", Data, "Resultats.Prenomemp");
            TB_Salaire.DataBindings.Add("text", Data, "Resultats.Salaireemp");
            DTP_Embauche.DataBindings.Add("text", Data, "Resultats.DateEmbauche");
            CB_EMPRESP.DataBindings.Add("text", Data, "Resultats.NumEmpresp");

            UpdateControls();

        }
        private void ClearBindings()
        {
            TB_NoEMP.DataBindings.Clear();
            TB_NoEMP.Clear();
            TB_Nom.DataBindings.Clear();
            TB_Nom.Clear();
            TB_Prenom.DataBindings.Clear();
            TB_Prenom.Clear();
            TB_Salaire.DataBindings.Clear();
            TB_Salaire.Clear();
            DTP_Embauche.DataBindings.Clear();
            DTP_Embauche.Value = DateTime.Now;
            CB_EMPRESP.Text = " ";
            CB_EMPRESP.DataBindings.Clear();

        }


        private void Lister()
        {
            try
            {
                LB_Dept.Items.Clear();
                string sql = "select nomdept from departements";
                OracleCommand oraCMD = new OracleCommand(sql, conn);
                oraCMD.CommandType = CommandType.Text;

                OracleDataReader oraRead = oraCMD.ExecuteReader();

                while (oraRead.Read())
                {
                    LB_Dept.Items.Add(oraRead.GetString(0));
                }
                LB_Dept.SelectedIndex = 0;
                oraRead.Close();
            }
            catch (OracleException ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }


        private void Connect()
        {
            try
            {
                string Dsource = "(DESCRIPTION="
               + "(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)"
               + "(HOST=205.237.244.251)(PORT=1521)))"
               + "(CONNECT_DATA=(SERVICE_NAME=ORCL)))";

                String ChaineConnexion = "Data Source=" + Dsource
                + ";User Id=CoteFran ; Password =oracle1";
                conn.ConnectionString = ChaineConnexion;

                conn.Open();

                UpdateControls();

                if (conn.State.ToString() == "Open")
                {
                    UpdateControls();
                }
            }
            catch (OracleException Ex)
            {
                switch (Ex.Number)
                {
                    case 12170:
                        MessageBox.Show("La base de données est indisponible,réessayer plus tard", "Erreur 12170", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case 12543:
                        MessageBox.Show("Connexion impossible. Vérifiez votre connexion internet", "Erreur 12543", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        MessageBox.Show(Ex.Message.ToString());
                        break;
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Connect();
            UpdateControls();
            Lister();
        }


        private void BTN_Quitter_Click(object sender, EventArgs e)
        {
            conn.Close();
            this.Close();
        }

        private void TB_Chiffre_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar)
        && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void ListeCompagnie_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                OracleCommand oraSelect = conn.CreateCommand();
                oraSelect.CommandText = "select codedept from departements where nomdept = '" + LB_Dept.Text + "'";
                using (OracleDataReader oraReader = oraSelect.ExecuteReader())
                {
                    if (oraReader.Read())
                    {
                        Codedept = oraReader.GetString(0);
                    }
                }

                OracleCommand oranbemploye = conn.CreateCommand();
                oranbemploye.CommandText = "select count(numemp) from employes where codedept = '" + Codedept + "'";
                using (OracleDataReader reader = oranbemploye.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        LB_NBEMP.Text = reader.GetInt32(0).ToString();
                    }

                }


                string sqlLISTES = "select numemp, nomemp, prenomemp, salaireemp, dateembauche, e.numempresp from employes e inner join departements d on d.codedept = e.codedept where nomdept = '" + LB_Dept.Text + "'";
                OracleDataAdapter oraliste = new OracleDataAdapter(sqlLISTES, conn);

                if (Data.Tables.Contains("Resultats"))
                {
                    Data.Tables["Resultats"].Clear();
                }
                oraliste.Fill(Data, "Resultats");

                ClearBindings();

                UpdateTextBox();
            }
            catch (OracleException ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void BTN_Add_Click(object sender, EventArgs e)
        {
            Ajouter_modifier Ajouter = new Ajouter_modifier();

            Ajouter.conn = this.conn;
            Ajouter.Text = "Ajout";
            Ajouter.CodeDept = Codedept;
            if (Ajouter.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string sqlIns = "insert into Employes(Nomemp, Prenomemp, salaireemp, dateembauche, codedept, numempresp) "
                    + "values (:NomEmp,:PrenomEmp,:Salaireemp,:DateEmbauche,:CodeDept,:NumEmpresp )";
                try
                {

                    OracleCommand oraAjout = new OracleCommand(sqlIns, conn);

                    OracleParameter OraParaNomEmp = new OracleParameter(":NomEmp", OracleDbType.Varchar2, 40);
                    OracleParameter OraParaPrenomEmp = new OracleParameter(":PrenomEmp", OracleDbType.Varchar2, 40);
                    OracleParameter OraParaSalaireemp = new OracleParameter(":Salaireemp", OracleDbType.Decimal, 8);  //Ajout
                    OracleParameter OraParaDateEmbauche = new OracleParameter(":DateEmbauche", OracleDbType.Date);
                    OracleParameter OraParaCodeDept = new OracleParameter(":CodeDept", OracleDbType.Char, 5);
                    OracleParameter OraParaNumEmpresp = new OracleParameter(":NumEmpresp", OracleDbType.Int32, 5);

                    OraParaNomEmp.Value = Ajouter.nomEmp;
                    OraParaPrenomEmp.Value = Ajouter.prenomEmp;
                    OraParaSalaireemp.Value = Ajouter.salaire;
                    OraParaDateEmbauche.Value = DateTime.Parse(Ajouter.Embauche);
                    OraParaCodeDept.Value = Ajouter.CodeDept;
                    OraParaNumEmpresp.Value = Ajouter.Empresp;

                    oraAjout.Parameters.Add(OraParaNomEmp);
                    oraAjout.Parameters.Add(OraParaPrenomEmp);
                    oraAjout.Parameters.Add(OraParaSalaireemp);
                    oraAjout.Parameters.Add(OraParaDateEmbauche);
                    oraAjout.Parameters.Add(OraParaCodeDept);
                    oraAjout.Parameters.Add(OraParaNumEmpresp);

                    oraAjout.ExecuteNonQuery();

                    Lister();
                }
                catch (OracleException ex)
                {
                    if (ex.Number == 2290)
                    {
                        MessageBox.Show("Le salaire doit être inférieur à 500 000.", "Erreur 2290", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }
        private void premier_Click(object sender, EventArgs e)
        {
            this.BindingContext[Data, "Resultats"].Position = 0;
        }

        private void dernier_Click(object sender, EventArgs e)
        {
            this.BindingContext[Data, "Resultats"].Position =
            this.BindingContext[Data, "Resultats"].Count - 1;

        }
        private void suivant_Click(object sender, EventArgs e)
        {
            this.BindingContext[Data, "Resultats"].Position++;

        }

        private void precedent_Click(object sender, EventArgs e)
        {
            this.BindingContext[Data, "Resultats"].Position--;
        }

        private void BTN_Supprimer_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûr de vouloir effacer cet employé ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                string sqldel = "delete from Employes where numemp = " + TB_NoEMP.Text;
                try
                {
                    OracleCommand orainsert = new OracleCommand(sqldel, conn);
                    orainsert.CommandType = CommandType.Text;
                    orainsert.ExecuteNonQuery();

                    Lister();
                }
                catch (OracleException ex)
                {
                    if (ex.Number == 2292)
                    {
                        MessageBox.Show("L'employé que vous tentez d'effacer a encore des employés à sa charge.", "Erreur 2292", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }

        private void BTN_Mod_Click(object sender, EventArgs e)
        {
            Ajouter_modifier Modifier = new Ajouter_modifier();

            Modifier.conn = this.conn;
            Modifier.Text = "Modification";
            Modifier.Numemp = TB_NoEMP.Text;
            Modifier.nomEmp = TB_Nom.Text;
            Modifier.prenomEmp = TB_Prenom.Text;
            Modifier.salaire = TB_Salaire.Text;
            Modifier.Embauche = DTP_Embauche.Value.ToString();
            Modifier.CodeDept = Codedept;
            Modifier.Empresp = CB_EMPRESP.Text;

            if (Modifier.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string sqlModifier = "Update employes set nomemp =:Nomemp, prenomemp =:Prenomemp, " +
                    "salaireemp =:Salaireemp, DateEmbauche = :Dateembauche, codedept = :CodeDept , NumempResp = :NumEmpResp Where Numemp =:NumEmp";

                    OracleCommand oraUpdate = new OracleCommand(sqlModifier, conn);
                    OracleParameter paramNom = new OracleParameter(":Nomemp", OracleDbType.Varchar2, 40);
                    OracleParameter paramPrenom = new OracleParameter(":Prenomemp", OracleDbType.Varchar2, 40);
                    OracleParameter paramSalaire = new OracleParameter(":Salaireemp", OracleDbType.Decimal, 8);
                    OracleParameter paramDate = new OracleParameter(":Dateembauche", OracleDbType.Date);
                    OracleParameter paramCodedept = new OracleParameter(":CodeDept", OracleDbType.Char, 5);
                    OracleParameter paramNumResp = new OracleParameter(":NumEmpResp", OracleDbType.Int32, 5);
                    OracleParameter paramNumemp = new OracleParameter(":NumEmp", OracleDbType.Int32, 5);

                    paramNom.Value = Modifier.nomEmp;
                    paramPrenom.Value = Modifier.prenomEmp;
                    paramSalaire.Value = Modifier.salaire;
                    paramDate.Value = DateTime.Parse(Modifier.Embauche);
                    paramCodedept.Value = Modifier.CodeDept;
                    if (Modifier.Empresp != "")
                        paramNumResp.Value = Modifier.Empresp;
                    else
                        paramNumResp.Value = null;
                    paramNumemp.Value = Modifier.Numemp;

                    oraUpdate.Parameters.Add(paramNom);
                    oraUpdate.Parameters.Add(paramPrenom);
                    oraUpdate.Parameters.Add(paramSalaire);
                    oraUpdate.Parameters.Add(paramDate);
                    oraUpdate.Parameters.Add(paramCodedept);
                    oraUpdate.Parameters.Add(paramNumResp);
                    oraUpdate.Parameters.Add(paramNumemp);

                    oraUpdate.ExecuteNonQuery();

                    Lister();
                }
                catch (OracleException ex)
                {
                    if (ex.Number == 2290)
                    {
                        MessageBox.Show("Le salaire doit être inférieur à 500 000.", "Erreur 2290", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }
    }
}

