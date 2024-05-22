using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace OOP_PROJECT
{
    public partial class For_Booking : Form
    {
        private int bookingID;
        private string movieTitle;
        private DateTime dateOfRelease;
        private decimal ticketPrice;
        private string loggedInUsername;
        private List<string> selectedSeats = new List<string>();
        private string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\Users\bagui\OneDrive\Documents\MOVIES.accdb";

        public For_Booking(int bookingID, string movieTitle, DateTime dateOfRelease, decimal ticketPrice, string loggedInUsername)
        {
            InitializeComponent();
            this.bookingID = bookingID;
            this.movieTitle = movieTitle;
            this.dateOfRelease = dateOfRelease;
            this.ticketPrice = ticketPrice;
            this.loggedInUsername = loggedInUsername;


            
            tbxMovieTitle.Text = movieTitle;
            tbxDateofRelease.Text = dateOfRelease.ToShortDateString();
            tbxPriceEachTicket.Text = ticketPrice.ToString("C2");
            FetchLastBookingIDFromDatabase();
            WireUpPictureBoxes();
        }

        private void bttnBookTicket_Click(object sender, EventArgs e)
        {
            // Check if at least one seat has been selected
            if (selectedSeats.Count == 0)
            {
                MessageBox.Show("Please select at least one seat before booking a ticket.");
                return;
            }

            // Validate that the number of tickets entered matches the number of selected seats
            if (!int.TryParse(tbxNumberOfTicketsBooked.Text, out int numberOfTickets) || numberOfTickets != selectedSeats.Count)
            {
                MessageBox.Show("The number of tickets entered does not match the number of seats selected. Please adjust your selection.");
                return;
            }

            // Check if any of the selected seats are already booked for this movie
            foreach (string seat in selectedSeats)
            {
                if (IsSeatBooked(seat))
                {
                    MessageBox.Show($"Seat {seat} is already booked for {movieTitle}.");
                    return;
                }
            }

            // Calculate total payment
            decimal totalPayment = selectedSeats.Count * ticketPrice;

            // Update available tickets in the database
            UpdateAvailableTickets(selectedSeats.Count);

            string firstName = GetCustomerFirstName(loggedInUsername);

            // Concatenate selected seats into one string
            string seatsConcatenated = string.Join(",", selectedSeats);

            // Save booking record to the database
            SaveBookingRecord(firstName, seatsConcatenated, totalPayment);

            // Save booked seats to the database
            foreach (string seat in selectedSeats)
            {
                // Save the booked seat in the Seats table
                SaveBookedSeat(seat, movieTitle);
            }

            // Save receipt to PDF
            SaveReceiptToPDF(firstName, movieTitle, selectedSeats, totalPayment);

            // Display booking details
            tbxDateBooked.Text = DateTime.Now.ToString("MM/dd/yyyy");
            tbxSeat.Text = string.Join(", ", selectedSeats); // Display selected seats
            tbxTotalPayment.Text = totalPayment.ToString("C");

            MessageBox.Show("Thank you for booking. Enjoy the movie!!");
            this.Close();
        }


        private int FetchLastBookingIDFromDatabase()
        {
            int actualBookingID = 0;

            string query = "SELECT MAX(Booking_ID) FROM Booking_Records";

            
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            actualBookingID = Convert.ToInt32(result); 
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error fetching last Booking ID: " + ex.Message);
                    }
                }
            }

            // If no booking exists, start from 1
            if (actualBookingID == 0)
            {
                actualBookingID = 1;
            }

            return actualBookingID;
        }


        private void SaveBookedSeat(string seat, string movieTitle)
        {
            // Save booked seat to the Seats table in your database
            string query = "INSERT INTO Seats (Seat, MovieTitle) VALUES (@Seat, @MovieTitle)";

            // Execute the insert query using your database connection
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    try
                    {
                        command.Parameters.AddWithValue("@Seat", seat);
                        command.Parameters.AddWithValue("@MovieTitle", movieTitle);
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving booked seat: " + ex.Message);
                    }
                }
            }
        }
        private string GetCustomerFirstName(string username)
        {
            string firstName = "";

            string query = "SELECT FirstName FROM Accounts WHERE Username = @Username";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);

                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            firstName = result.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error fetching customer's first name: " + ex.Message);
                    }
                }
            }

            return firstName;
        }

        private void UpdateAvailableTickets(int ticketsBooked)
        {
            // Update available tickets in the database using SQL UPDATE statement
            string query = "UPDATE Movies SET Available_Tickets = Available_Tickets - @TicketsBooked WHERE Title = @MovieTitle";

            // Execute the update query using your database connection
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    try
                    {
                        command.Parameters.AddWithValue("@TicketsBooked", ticketsBooked);
                        command.Parameters.AddWithValue("@MovieTitle", movieTitle); // Specify the movie title for which tickets are being booked
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating available tickets: " + ex.Message);
                    }
                }
            }
        }

        private void SaveReceiptToPDF(string firstName, string movieTitle, List<string> selectedSeats, decimal totalPayment)
        {
            // Fetch the actual Booking_ID from the database
            int actualBookingID = FetchLastBookingIDFromDatabase();

            string fileName = $"Receipt_{actualBookingID}.pdf";

            // Create a new PDF document
            iTextSharp.text.Document document = new iTextSharp.text.Document();

            try
            {
                // Set up the PDF writer
                PdfWriter.GetInstance(document, new FileStream(fileName, FileMode.Create));

                // Open the document
                document.Open();

                // Add content to the PDF for each selected seat
                foreach (var seat in selectedSeats)
                {
                    // Add a new paragraph for each seat
                    Paragraph paragraph = new Paragraph();
                    paragraph.Add($"Booking ID: {actualBookingID}\n"); 
                    paragraph.Add($"Customer Name: {firstName}\n");
                    paragraph.Add($"Movie Title: {movieTitle}\n");
                    paragraph.Add($"Date of Release: {dateOfRelease.ToShortDateString()}\n"); 
                    paragraph.Add($"Date Booked: {DateTime.Now.ToString("MM/dd/yyyy")}\n");
                    paragraph.Add($"Seat: {seat}\n");
                    paragraph.Add($"Number of Tickets Booked: {selectedSeats.Count}\n"); 
                    paragraph.Add($"Total Payment: {totalPayment.ToString("C")}\n\n");

                    // Add the paragraph to the document
                    document.Add(paragraph);
                }
            }
            catch (DocumentException de)
            {
                MessageBox.Show("Error creating PDF: " + de.Message);
            }
            catch (IOException ioe)
            {
                MessageBox.Show("Error saving PDF: " + ioe.Message);
            }
            finally
            {
                // Close the document
                document.Close();
            }
        }

        private void bttnCancel_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to Go back?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void SaveBookingRecord(string firstName, string seatsConcatenated, decimal totalPayment)
        {
            // Save booking record to the Booking_Records table in your database
            string query = "INSERT INTO Booking_Records (Username, FirstName, Title, Tickets_Booked, Date_Booked, Date_of_Release, Total, Seat) VALUES (@Username, @FirstName, @Title, @TicketsBooked, @DateBooked, @DateOfRelease, @Total, @Seats)";

            // Execute the insert query using your database connection
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    try
                    {
                        command.Parameters.AddWithValue("@Username", loggedInUsername);
                        command.Parameters.AddWithValue("@FirstName", firstName);
                        command.Parameters.AddWithValue("@Title", movieTitle);
                        command.Parameters.AddWithValue("@TicketsBooked", selectedSeats.Count); // Use the actual number of tickets booked
                        command.Parameters.AddWithValue("@DateBooked", DateTime.Now.ToString("MM/dd/yyyy"));
                        command.Parameters.AddWithValue("@DateOfRelease", dateOfRelease);
                        command.Parameters.AddWithValue("@Total", totalPayment);
                        command.Parameters.AddWithValue("@Seats", seatsConcatenated); // Add concatenated seats parameter
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error saving booking record: " + ex.Message);
                    }
                }
            }
        }
        private void WireUpPictureBoxes()
        {
            // Add PictureBox_Click event handler to each picture box
            foreach (Control control in Controls)
            {
                if (control is PictureBox pictureBox)
                {
                    pictureBox.Click += PictureBox_Click;
                    // Check if the seat is already booked and set the color accordingly
                    string seatName = pictureBox.Name;
                    if (IsSeatBooked(seatName))
                    {
                        pictureBox.BackColor = Color.Red;
                        pictureBox.Enabled = false; // Disable booking for already booked seats
                    }
                }
            }
        }

        // Event handler for picture box click events
        private void PictureBox_Click(object sender, EventArgs e)
        {
            // Check if the number of tickets has been entered
            if (string.IsNullOrWhiteSpace(tbxNumberOfTicketsBooked.Text))
            {
                MessageBox.Show("Please enter the number of tickets before selecting seats.");
                return;
            }

            PictureBox pictureBox = sender as PictureBox;
            string seatName = pictureBox.Name;

            // Check if the seat is already booked
            if (IsSeatBooked(seatName))
            {
                MessageBox.Show("This seat is already booked.");
                pictureBox.BackColor = Color.Red; 
                return;
            }

            // Check if the maximum number of tickets is reached
            int maxTickets = Convert.ToInt32(tbxNumberOfTicketsBooked.Text);
            if (selectedSeats.Count >= maxTickets && !selectedSeats.Contains(seatName))
            {
                MessageBox.Show($"You have already selected the maximum number of tickets ({maxTickets}).");
                return;
            }

            // Toggle seat selection
            if (selectedSeats.Contains(seatName))
            {
                selectedSeats.Remove(seatName);
                pictureBox.BackColor = Color.Black;
            }
            else
            {
                selectedSeats.Add(seatName);
                pictureBox.BackColor = Color.Green; 
            }
        }

        // Method to check if a seat is already booked
        private bool IsSeatBooked(string seat)
        {
            // Query the database to check if the seat is booked for this movie
            string query = "SELECT COUNT(*) FROM Seats WHERE Seat = @Seat AND MovieTitle = @MovieTitle";
            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Seat", seat);
                    command.Parameters.AddWithValue("@MovieTitle", movieTitle);
                    connection.Open();
                    int count = (int)command.ExecuteScalar();
                    return count > 0;
                }
            }
        }
    }
}
