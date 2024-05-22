using System.Data.OleDb;
using System.Data;

namespace OOP_PROJECT
{
    public partial class For_AdminRecords2 : Form
    {
        private string loggedInUsername;
        private OleDbConnection con = new OleDbConnection();
        private const string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";
        private DataTable dataTable; // Declare a DataTable to hold the data

        public For_AdminRecords2(string username)
        {
            InitializeComponent();
            loggedInUsername = username;
        }

        private void For_AdminRecords2_Load(object sender, EventArgs e)
        {
            LoadBookingRecords();
        }

        private void LoadBookingRecords()
        {
            try
            {
                con.ConnectionString = connectionString;
                con.Open();

                string query = "SELECT Booking_ID, Username, FirstName, Title, Tickets_Booked, Date_Booked, Date_of_Release, Total, Seat FROM Booking_Records";
                OleDbCommand cmd = new OleDbCommand(query, con);
                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                dataTable = new DataTable();
                adapter.Fill(dataTable);

                dgvBookingRecords.DataSource = dataTable; // Bind DataTable to DataGridView
                dgvBookingRecords.CellClick += new DataGridViewCellEventHandler(dgvBookingRecords_CellClick);
                dgvBookingRecords.SelectionChanged += new EventHandler(dgvBookingRecords_SelectionChanged);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                con.Close();
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

        private void btnProfile_Click(object sender, EventArgs e)
        {
            // Implement functionality for profile button if needed
        }

        private void btnBookTickets_Click(object sender, EventArgs e)
        {
            // Implement functionality for booking tickets button if needed
        }
    }
}
