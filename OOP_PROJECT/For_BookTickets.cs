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
    public partial class For_BookTickets : Form
    {
        private string loggedInUsername;

        public For_BookTickets(string username)
        {
            InitializeComponent();
            loggedInUsername = username;


        }

        private const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";

        public For_BookTickets()
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

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            For_BrowseMovies for_browse = new For_BrowseMovies(loggedInUsername);
            for_browse.Show();
            this.Hide();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {

        }

        private void For_BookTickets_Load(object sender, EventArgs e)
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
            if (dt.Columns.Contains("TrailerFilePath"))
            {
                dt.Columns.Remove("TrailerFilePath");
            }

            // Filter out movies that have passed their date of release
            dt = FilterMoviesByReleaseDate(dt);

            return dt;
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            MessageBox.Show("SUCCESSFULLY LOGGED OUT!");
            For_Login for_login = new For_Login();
            for_login.Show();
            this.Hide();
        }

        private void dgvBookTickets_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBookTickets.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dgvBookTickets.SelectedRows[0];

                // Check if the selected row and its cells are not null
                if (selectedRow != null)
                {
                    // Populate text boxes with data from selected row
                    tbxMovieTitle.Text = selectedRow.Cells["Title"].Value?.ToString();
                    tbxGenre.Text = selectedRow.Cells["Genre"].Value?.ToString();
                    tbxDateofRelease.Text = selectedRow.Cells["Date_of_Release"].Value?.ToString();
                    tbxRuntimeHours.Text = selectedRow.Cells["Runtime_Hours"].Value?.ToString();
                    tbxRunTimeMinutes.Text = selectedRow.Cells["RunTime_Minutes"].Value?.ToString();
                    tbxTicketPrice.Text = selectedRow.Cells["Ticket_Price"].Value?.ToString();
                    tbxTickets.Text = selectedRow.Cells["Available_Tickets"].Value?.ToString();

                    // Update the image in pictureBox2
                    UpdatePictureBox(selectedRow);
                }
            }
        }


        private void bttnBookTicket_Click(object sender, EventArgs e)
        {

        }

        private void bttnInsertPicture_Click(object sender, EventArgs e)
        {

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
                                        pictureBox2.Image = Image.FromStream(ms);
                                    }
                                }
                                else
                                {
                                    // Clear pictureBox2 if no image is available
                                    pictureBox2.Image = null;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Clear pictureBox2 if no movie ID is available
                pictureBox2.Image = null;
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

        private void UpdatePictureInDatabase(int movieID, byte[] imageBytes)
        {
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                string query = "UPDATE Movies SET Picture = ? WHERE Movie_ID = ?";

                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.Add("@Picture", OleDbType.VarBinary).Value = imageBytes;
                    cmd.Parameters.Add("@Movie_ID", OleDbType.Integer).Value = movieID;

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
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

        private void bttnBookTicket_Click_1(object sender, EventArgs e)
        {
            if (dgvBookTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a movie to book tickets.");
                return;
            }

            // Get the selected movie details
            DataGridViewRow selectedRow = dgvBookTickets.SelectedRows[0];
            int availableTickets = Convert.ToInt32(selectedRow.Cells["Available_Tickets"].Value);

            // Check if there are available tickets
            if (availableTickets == 0)
            {
                MessageBox.Show("Sorry, there are no available tickets for this movie.");
                return;
            }

            // Get the movie details
            int movieID = Convert.ToInt32(selectedRow.Cells["Movie_ID"].Value);
            string movieTitle = selectedRow.Cells["Title"].Value.ToString();
            DateTime dateOfRelease = Convert.ToDateTime(selectedRow.Cells["Date_of_Release"].Value);
            decimal ticketPrice = Convert.ToDecimal(selectedRow.Cells["Ticket_Price"].Value);

            // Open the booking form and pass the movie details
            For_Booking bookingForm = new For_Booking(movieID, movieTitle, dateOfRelease, ticketPrice, loggedInUsername);
            bookingForm.ShowDialog(); // Show as a dialog
        }
        private DataTable FilterMoviesByReleaseDate(DataTable dt)
        {
            // Create a new DataTable to hold the filtered data
            DataTable filteredDt = dt.Clone();

            // Get the current date
            DateTime currentDate = DateTime.Today;

            // Iterate through each row in the original DataTable
            foreach (DataRow row in dt.Rows)
            {
                // Get the date of release from the row
                DateTime dateOfRelease;
                if (DateTime.TryParse(row["Date_of_Release"].ToString(), out dateOfRelease))
                {
                    // Check if the date of release is in the future (not passed yet)
                    if (dateOfRelease >= currentDate)
                    {
                        // Add the row to the filtered DataTable
                        filteredDt.ImportRow(row);
                    }
                }
            }

            return filteredDt;
        }

        private void bttnInsertPicture_Click_1(object sender, EventArgs e)
        {
            if (dgvBookTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to insert the picture.");
                return;
            }

            DataGridViewRow selectedRow = dgvBookTickets.SelectedRows[0];
            int movieID = Convert.ToInt32(selectedRow.Cells["Movie_ID"].Value);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string imagePath = openFileDialog.FileName;

                    // Read the selected image file into a byte array
                    byte[] imageBytes = File.ReadAllBytes(imagePath);

                    // Update the database with the picture for the selected row
                    UpdatePictureInDatabase(movieID, imageBytes);

                    // Display the inserted picture on pictureBox2
                    pictureBox2.Image = Image.FromFile(imagePath);

                    MessageBox.Show("Picture inserted successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void tbxTicketPrice_TextChanged(object sender, EventArgs e)
        {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
