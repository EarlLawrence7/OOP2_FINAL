using System;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace OOP_PROJECT
{
    public partial class For_AdminRecords : Form
    {
        private string loggedInUsername;
        private DataTable dataTable;

        public For_AdminRecords(string username)
        {
            InitializeComponent();
            loggedInUsername = username;
        }

        private const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";

        private void For_AdminRecords_Load_1(object sender, EventArgs e)
        {
            LoadBookingRecords();
        }

        private void btnProfile_Click(object sender, EventArgs e)
        {
            For_AdminProfile for_admin = new For_AdminProfile(loggedInUsername);
            for_admin.Show();
            this.Hide();
        }

        private void btnBookTickets_Click(object sender, EventArgs e)
        {
            For_AdminBooking for_booking = new For_AdminBooking(loggedInUsername);
            for_booking.Show();
            this.Hide();
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            MessageBox.Show("SUCCESSFULLY LOGGED OUT!");
            For_Login for_login = new For_Login();
            for_login.Show();
            this.Hide();
        }

        private void LoadData()
        {
            DataTable dt = GetDataFromAccess("SELECT * FROM Booking_Records");
            dgvBookingRecords.DataSource = dt;
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

            return dt;
        }

        private void LoadBookingRecords()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();

                    string query = "SELECT Booking_ID, Username, FirstName, Title, Tickets_Booked, Date_Booked, Date_of_Release, Total, Seat FROM Booking_Records";
                    using (OleDbCommand cmd = new OleDbCommand(query, conn))
                    {
                        using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                        {
                            dataTable = new DataTable();
                            adapter.Fill(dataTable);

                            dgvBookingRecords.DataSource = dataTable; // Bind DataTable to DataGridView
                            dgvBookingRecords.CellClick += new DataGridViewCellEventHandler(dgvBookingRecords_CellClick);
                            dgvBookingRecords.SelectionChanged += new EventHandler(dgvBookingRecords_SelectionChanged);

                            // Calculate total sales when data is loaded
                            CalculateTotalSales();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void dgvBookingRecords_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataTable.Rows.Count)
            {
                UpdateTextBoxes(e.RowIndex);
            }
        }

        private void dgvBookingRecords_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvBookingRecords.SelectedRows.Count > 0)
            {
                int selectedIndex = dgvBookingRecords.SelectedRows[0].Index;
                if (selectedIndex >= 0 && selectedIndex < dataTable.Rows.Count)
                {
                    UpdateTextBoxes(selectedIndex);
                }
            }
        }

        private void UpdateTextBoxes(int rowIndex)
        {
            DataRow row = dataTable.Rows[rowIndex];
            tbxBookingID.Text = row["Booking_ID"].ToString();
            tbxUsername.Text = row["Username"].ToString();
            tbxFirstName.Text = row["FirstName"].ToString();
            tbxMovieTitle.Text = row["Title"].ToString();
            tbxTicketsBooked.Text = row["Tickets_Booked"].ToString();
            tbxDateBooked.Text = row["Date_Booked"].ToString();
            tbxDateOfRelease.Text = row["Date_of_Release"].ToString();
            tbxTotal.Text = row["Total"].ToString();
            tbxSeat.Text = row["Seat"].ToString();
        }

        private void btnClear_Click_1(object sender, EventArgs e)
        {
            // Set DataSource to null to clear sorting
            dgvBookingRecords.DataSource = null;

            // Reassign the data source to reload the original data
            LoadData();

            // Clear any selected rows
            dgvBookingRecords.ClearSelection();

            // Calculate total sales after clearing
            CalculateTotalSales();

            cmbxSortSales.SelectedIndex = -1;
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dgvBookingRecords.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a row to delete.");
                return;
            }

            DataGridViewRow selectedRow = dgvBookingRecords.SelectedRows[0];
            int bookingID = Convert.ToInt32(selectedRow.Cells["Booking_ID"].Value);

            using (OleDbConnection conn = new OleDbConnection(connectionString))
            {
                string query = "DELETE FROM Booking_Records WHERE Booking_ID = ?";
                using (OleDbCommand cmd = new OleDbCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingID", bookingID);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Reload data after deletion
            LoadData();

            // Calculate total sales after deletion
            CalculateTotalSales();

            MessageBox.Show("Record deleted successfully!");
        }

        private void cmbxSortSales_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (cmbxSortSales.SelectedItem != null)
            {
                string selectedItem = cmbxSortSales.SelectedItem.ToString();
                string query = "";

                // Build the query based on the selected item
                switch (selectedItem)
                {
                    case "3 Days Ago":
                        query = "SELECT * FROM Booking_Records WHERE Date_Booked >= Date() - 3";
                        break;
                    case "A Week Ago":
                        query = "SELECT * FROM Booking_Records WHERE Date_Booked >= Date() - 7";
                        break;
                    case "A Month Ago":
                        query = "SELECT * FROM Booking_Records WHERE Date_Booked >= Date() - 30";
                        break;
                    case "More than 1 Month Ago":
                        query = "SELECT * FROM Booking_Records WHERE Date_Booked <= Date() - 30";
                        break;
                    case "A Year Ago":
                        query = "SELECT * FROM Booking_Records WHERE Date_Booked >= Date() - 365";
                        break;
                    case "More than 1 Year Ago":
                        query = "SELECT * FROM Booking_Records WHERE Date_Booked < Date() - 365";
                        break;
                    default:
                        query = "SELECT * FROM Booking_Records";
                        break;
                }

                // Get data and update DataGridView
                DataTable dt = GetDataFromAccess(query);
                dgvBookingRecords.DataSource = dt;

                // Calculate total sales after sorting/filtering
                CalculateTotalSales();
            }
        }

        private void CalculateTotalSales()
        {
            try
            {
                decimal totalSales = 0;

                foreach (DataGridViewRow row in dgvBookingRecords.Rows)
                {
                    if (row.Cells["Total"].Value != DBNull.Value)
                    {
                        totalSales += Convert.ToDecimal(row.Cells["Total"].Value);
                    }
                }

                tbxTotalSales.Text = $"{totalSales:C}";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error calculating total sales: " + ex.Message);
            }
        }
    }
}
