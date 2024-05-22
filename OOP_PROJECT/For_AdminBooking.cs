using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace OOP_PROJECT
{
    public partial class For_AdminBooking : Form
    {
        private string loggedInUsername;
        private OleDbConnection con = new OleDbConnection();

        public For_AdminBooking(string username)
        {
            InitializeComponent();
            loggedInUsername = username;
        }

        private const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";

        private void For_AdminBooking_Load_1(object sender, EventArgs e)
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

                    // Update the DateTimePicker with the selected release date
                    if (DateTime.TryParse(selectedRow.Cells["Date_of_Release"].Value?.ToString(), out DateTime releaseDate))
                    {
                        dateTimePicker1.Value = releaseDate;
                    }

                    // Update the image in pictureBox2
                    UpdatePictureBox(selectedRow);
                }
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

        private void btnLogOut_Click_1(object sender, EventArgs e)
        {
            MessageBox.Show("SUCCESSFULLY LOGGED OUT!");
            For_Login for_login = new For_Login();
            for_login.Show();
            this.Hide();
        }

        private void bttnInsertPicture_Click(object sender, EventArgs e)
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

        private void bttnSaveBookings_Click(object sender, EventArgs e)
        {
            // Create a new OleDbConnection
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    conn.Open();

                    // Get values from textboxes
                    int movieID = Convert.ToInt32(dgvBookTickets.SelectedRows[0].Cells["Movie_ID"].Value);
                    string title = tbxMovieTitle.Text;
                    string genre = tbxGenre.Text;
                    DateTime dateOfRelease = Convert.ToDateTime(tbxDateofRelease.Text);
                    int runtimeHours = Convert.ToInt32(tbxRuntimeHours.Text);
                    int runtimeMinutes = Convert.ToInt32(tbxRunTimeMinutes.Text);
                    decimal ticketPrice = Convert.ToDecimal(tbxTicketPrice.Text);
                    int availableTickets = Convert.ToInt32(tbxTickets.Text);

                    // Construct the SQL query to update the movie
                    string query = "UPDATE Movies SET Title = ?, Genre = ?, Date_of_Release = ?, Runtime_Hours = ?, RunTime_Minutes = ?, Ticket_Price = ?, Available_Tickets = ? WHERE Movie_ID = ?";

                    // Create a new OleDbCommand
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        // Add parameters to the command
                        cmd.Parameters.AddWithValue("@Title", title);
                        cmd.Parameters.AddWithValue("@Genre", genre);
                        cmd.Parameters.AddWithValue("@Date_of_Release", dateOfRelease);
                        cmd.Parameters.AddWithValue("@Runtime_Hours", runtimeHours);
                        cmd.Parameters.AddWithValue("@RunTime_Minutes", runtimeMinutes);
                        cmd.Parameters.AddWithValue("@Ticket_Price", ticketPrice);
                        cmd.Parameters.AddWithValue("@Available_Tickets", availableTickets);
                        cmd.Parameters.AddWithValue("@Movie_ID", movieID);

                        // Execute the command
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Movie data saved successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    // Close the connection
                    conn.Close();
                }
            }
        }


        private void bttnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBookTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a movie to delete.");
                return;
            }

            DataGridViewRow selectedRow = dgvBookTickets.SelectedRows[0];
            int movieID = Convert.ToInt32(selectedRow.Cells["Movie_ID"].Value);

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                string query = "DELETE FROM Movies WHERE Movie_ID = ?";

                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Movie_ID", movieID);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Refresh the data grid view
            DataTable dt = GetDataFromAccess("SELECT * FROM Movies");
            dgvBookTickets.DataSource = dt;

            MessageBox.Show("Movie deleted successfully!");
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            For_AdminProfile for_admin = new For_AdminProfile(loggedInUsername);
            for_admin.Show();
            this.Hide();
        }


        private void btnAddNewMovie_Click(object sender, EventArgs e)
        {
            // Check if the DataGridView is bound to a data source
            if (dgvBookTickets.DataSource != null && dgvBookTickets.DataSource is DataTable dt)
            {
                // Create a new DataRow
                DataRow newRow = dt.NewRow();

                // Fill the new DataRow with data from textboxes and dateTimePicker
                newRow["Title"] = tbxMovieTitle.Text;
                newRow["Genre"] = tbxGenre.Text;
                newRow["Date_of_Release"] = dateTimePicker1.Value.ToShortDateString();
                newRow["Runtime_Hours"] = tbxRuntimeHours.Text;
                newRow["RunTime_Minutes"] = tbxRunTimeMinutes.Text;
                newRow["Ticket_Price"] = tbxTicketPrice.Text;
                newRow["Available_Tickets"] = tbxTickets.Text;

                // Add the new DataRow to the DataTable
                dt.Rows.Add(newRow);

                // Update the database with the new row
                UpdateDatabase(dt);

                // Refresh the DataGridView
                dgvBookTickets.DataSource = dt;
            }
        }

        private void UpdateDatabase(DataTable dt)
        {
            // Create a new OleDbConnection
            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                try
                {
                    // Open the connection
                    conn.Open();

                    // Create a new OleDbDataAdapter
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter())
                    {
                        // Create the insert command
                        adapter.InsertCommand = new OleDbCommand("INSERT INTO Movies (Title, Genre, Date_of_Release, Runtime_Hours, RunTime_Minutes, Ticket_Price, Available_Tickets) VALUES (?, ?, ?, ?, ?, ?, ?)", conn);
                        adapter.InsertCommand.Parameters.Add("@Title", OleDbType.VarChar, 255, "Title");
                        adapter.InsertCommand.Parameters.Add("@Genre", OleDbType.VarChar, 255, "Genre");
                        adapter.InsertCommand.Parameters.Add("@Date_of_Release", OleDbType.Date, 255, "Date_of_Release");
                        adapter.InsertCommand.Parameters.Add("@Runtime_Hours", OleDbType.Integer, 255, "Runtime_Hours");
                        adapter.InsertCommand.Parameters.Add("@RunTime_Minutes", OleDbType.Integer, 255, "RunTime_Minutes");
                        adapter.InsertCommand.Parameters.Add("@Ticket_Price", OleDbType.Decimal, 255, "Ticket_Price");
                        adapter.InsertCommand.Parameters.Add("@Available_Tickets", OleDbType.Integer, 255, "Available_Tickets");

                        // Update the database with the new row
                        adapter.Update(dt);
                    }

                    MessageBox.Show("New movie added successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    // Close the connection
                    conn.Close();
                }
            }
        }

        private void tbxSearchBox_TextChanged_1(object sender, EventArgs e)
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

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            For_AdminRecords for_bookingRecords = new For_AdminRecords(loggedInUsername);
            for_bookingRecords.Show();
            this.Hide();
        }
    }
}
