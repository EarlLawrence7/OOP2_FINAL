using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OOP_PROJECT
{
    public partial class For_BrowseMovies : Form
    {
        private string loggedInUsername;

        public For_BrowseMovies(string username)
        {
            InitializeComponent();
            loggedInUsername = username;


        }

        private const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";

        public For_BrowseMovies()
        {
            InitializeComponent();
        }
        private void btnHome_Click(object sender, EventArgs e)
        {
            For_Home for_home = new For_Home(loggedInUsername);
            for_home.Show();
            this.Hide();
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            For_Profile for_Profile = new For_Profile(loggedInUsername);
            for_Profile.Show();
            this.Hide();
        }
        private void btnAbout_Click(object sender, EventArgs e)
        {

        }
        private void btnBookTickets_Click(object sender, EventArgs e)
        {
            For_BookTickets for_bookTickets = new For_BookTickets(loggedInUsername);
            for_bookTickets.Show();
            this.Hide();
        }

        private void btnLogOut_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("SUCCESSFULLY LOGGED OUT!");
            For_Login for_login = new For_Login();
            for_login.Show();
            this.Hide();
        }
        private void For_BrowseMovies_Load_1(object sender, EventArgs e)
        {
            DataTable dt = GetDataFromAccess("SELECT * FROM Movies");

            // Bind the DataTable to the DataGridView
            dgvBookTickets.DataSource = dt;

            // Attach the SelectionChanged event handler
            dgvBookTickets.SelectionChanged += dgvBookTickets_SelectionChanged;

            cmbxSortGenre.SelectedIndexChanged += cmbxSortGenre_SelectedIndexChanged;
        }


        private DataTable GetDataFromAccess(string query)
        {
            DataTable dt = new DataTable();

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    conn.Open();
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            if (dt.Columns.Contains("Picture"))
            {
                dt.Columns.Remove("Picture");
            }
            return dt;
        }


        private void dgvBookTickets_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBookTickets.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvBookTickets.SelectedRows[0];

                // Populate text boxes with data from selected row
                tbxMovieTitle.Text = selectedRow.Cells["Title"].Value.ToString();
                tbxGenre.Text = selectedRow.Cells["Genre"].Value.ToString();
                tbxDateofRelease.Text = selectedRow.Cells["Date_of_Release"].Value.ToString();
                tbxRuntimeHours.Text = selectedRow.Cells["Runtime_Hours"].Value.ToString();
                tbxRunTimeMinutes.Text = selectedRow.Cells["RunTime_Minutes"].Value.ToString();

                UpdatePictureBox(selectedRow);
            }
        }
        private void UpdatePictureBox(DataGridViewRow selectedRow)
        {
            object movieIDValue = selectedRow.Cells["Movie_ID"].Value;

            if (movieIDValue != DBNull.Value)
            {
                int movieID = Convert.ToInt32(movieIDValue);

                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    string query = "SELECT Picture FROM Movies WHERE Movie_ID = ?";
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Movie_ID", movieID);
                        conn.Open();
                        using (OleDbDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                object pictureValue = reader["Picture"];
                                if (pictureValue != DBNull.Value)
                                {
                                    // Read the image from database and display it in pictureBox2
                                    byte[] imageBytes = (byte[])pictureValue;
                                    using (MemoryStream ms = new MemoryStream(imageBytes))
                                    {
                                        pictureBox4.Image = Image.FromStream(ms);
                                    }
                                }
                                else
                                {
                                    // Clear pictureBox2 if no image is available
                                    pictureBox4.Image = null;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                pictureBox4.Image = null;
            }
        }
        private void cmbxSortGenre_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbxSortGenre.SelectedItem != null)
            {
                string selectedGenre = cmbxSortGenre.SelectedItem.ToString();
                DataTable dt = GetDataFromAccess($"SELECT * FROM Movies WHERE Genre = '{selectedGenre}'");
                dgvBookTickets.DataSource = dt;
            }
        }
        private void tbxSearchBox_TextChanged(object sender, EventArgs e)
        {
            string searchText = tbxSearchBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                DataTable dt = GetDataFromAccess($"SELECT * FROM Movies WHERE Title LIKE '%{searchText}%' OR Genre LIKE '%{searchText}%'");
                dgvBookTickets.DataSource = dt;
            }
            else
            {
                // If the search box is empty, show all movies
                DataTable dt = GetDataFromAccess("SELECT * FROM Movies");
                dgvBookTickets.DataSource = dt;
            }
        }

        private void cmbxSortGenre_SelectedIndexChanged_1(object sender, EventArgs e)
        {

        }

    }
}
