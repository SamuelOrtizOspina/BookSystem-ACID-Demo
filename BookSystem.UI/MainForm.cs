#nullable disable
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace BookSystem.UI
{
    public partial class MainForm : Form
    {
        private DataGridView dgvBooks;
        private TextBox txtTitle, txtISBN, txtPrice, txtStock, txtAuthorId;
        private Label lblStatus;

        private string ConnectionString => "Server=localhost,1433;Database=BookStoreDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=False;";

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "BookSystem Professional - Gestion de Inventario";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 170, Padding = new Padding(20), BackColor = Color.White };
            
            int x = 20, y = 20;
            AddInput(pnlInput, "Titulo del Libro:", ref txtTitle, x, y, 250);
            AddInput(pnlInput, "ISBN:", ref txtISBN, x + 270, y, 150);
            AddInput(pnlInput, "Precio:", ref txtPrice, x + 440, y, 100);
            AddInput(pnlInput, "Stock:", ref txtStock, x + 560, y, 80);
            AddInput(pnlInput, "Autor (ID o Nombre):", ref txtAuthorId, x + 660, y, 200);

            Button btnAdd = new Button { Text = "Guardar", Location = new Point(20, 90), Size = new Size(140, 45), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += (s, e) => AddBook();
            pnlInput.Controls.Add(btnAdd);

            Button btnDelete = new Button { Text = "Eliminar Seleccionado", Location = new Point(170, 90), Size = new Size(200, 45), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnDelete.Click += (s, e) => DeleteSelected();
            pnlInput.Controls.Add(btnDelete);

            Button btnRefresh = new Button { Text = "Refrescar", Location = new Point(380, 90), Size = new Size(140, 45), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += (s, e) => LoadData();
            pnlInput.Controls.Add(btnRefresh);

            Button btnACID = new Button { Text = "Prueba ACID", Location = new Point(530, 90), Size = new Size(140, 45), BackColor = Color.FromArgb(108, 117, 125), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnACID.Click += (s, e) => RunACIDTest();
            pnlInput.Controls.Add(btnACID);

            dgvBooks = new DataGridView { 
                Dock = DockStyle.Fill, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            lblStatus = new Label { Dock = DockStyle.Bottom, Height = 35, Text = " Listo.", TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(220, 220, 220) };

            this.Controls.Add(dgvBooks);
            this.Controls.Add(pnlInput);
            this.Controls.Add(lblStatus);
        }

        private void AddInput(Panel p, string label, ref TextBox tb, int x, int y, int width)
        {
            Label lbl = new Label { Text = label, Location = new Point(x, y), AutoSize = true };
            tb = new TextBox { Location = new Point(x, y + 22), Width = width };
            p.Controls.Add(lbl);
            p.Controls.Add(tb);
        }

        private void LoadData()
        {
            try {
                using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT B.Id, B.Title, B.ISBN, B.Price, B.Stock, A.Name as Autor FROM Books B JOIN Authors A ON B.AuthorId = A.Id ORDER BY B.Id DESC", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvBooks.DataSource = dt;
                    lblStatus.Text = $" {dt.Rows.Count} libros cargados.";
                }
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void AddBook()
        {
            try {
                using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                    conn.Open();
                    int authorId;
                    if (int.TryParse(txtAuthorId.Text, out int id)) authorId = id;
                    else {
                        string sqlA = "INSERT INTO Authors (Name, Bio) OUTPUT INSERTED.Id VALUES (@n, 'Nuevo')";
                        using (SqlCommand cmdA = new SqlCommand(sqlA, conn)) {
                            cmdA.Parameters.AddWithValue("@n", txtAuthorId.Text);
                            authorId = (int)(cmdA.ExecuteScalar() ?? 0);
                        }
                    }

                    string sqlB = "INSERT INTO Books (Title, ISBN, Price, Stock, AuthorId) VALUES (@t, @i, @p, @s, @a)";
                    using (SqlCommand cmdB = new SqlCommand(sqlB, conn)) {
                        cmdB.Parameters.AddWithValue("@t", txtTitle.Text);
                        cmdB.Parameters.AddWithValue("@i", txtISBN.Text);
                        cmdB.Parameters.AddWithValue("@p", decimal.Parse(txtPrice.Text.Replace(",", ".")));
                        cmdB.Parameters.AddWithValue("@s", int.Parse(txtStock.Text));
                        cmdB.Parameters.AddWithValue("@a", authorId);
                        cmdB.ExecuteNonQuery();
                    }
                    LoadData();
                    ClearInputs();
                }
            } catch (Exception ex) {
                MessageBox.Show("Fallo al guardar: " + ex.Message);
            }
        }

        private void DeleteSelected()
        {
            if (dgvBooks.SelectedRows.Count == 0) {
                MessageBox.Show("Selecciona un libro en la tabla para eliminarlo.");
                return;
            }

            var selectedRow = dgvBooks.SelectedRows[0];
            if (selectedRow.Cells["Id"].Value == null) return;

            int id = Convert.ToInt32(selectedRow.Cells["Id"].Value);
            
            DialogResult res = MessageBox.Show($"Deseas eliminar el libro con ID {id}?", "Confirmar Borrado", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            
            if (res == DialogResult.Yes) {
                try {
                    using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                        conn.Open();
                        new SqlCommand($"DELETE FROM Books WHERE Id = {id}", conn).ExecuteNonQuery();
                        lblStatus.Text = $" Libro con ID {id} eliminado con exito.";
                        LoadData();
                    }
                } catch (Exception ex) {
                    MessageBox.Show("Fallo al borrar: " + ex.Message);
                }
            }
        }

        private void RunACIDTest()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction()) {
                    try {
                        new SqlCommand("UPDATE Books SET Price = 99999 WHERE Id = 1", conn, trans).ExecuteNonQuery();
                        DialogResult res = MessageBox.Show("Paso 1: Precio de ID 1 cambiado a 99999 (en memoria).\nDesea simular un error para probar el ROLLBACK?", "Atomicidad", MessageBoxButtons.YesNo);
                        if (res == DialogResult.Yes) throw new Exception("Error simulado.");
                        trans.Commit();
                        MessageBox.Show("COMMIT exitoso.");
                    } catch {
                        trans.Rollback();
                        MessageBox.Show("ROLLBACK ejecutado: Los datos no cambiaron.", "Prueba ACID OK");
                    }
                }
                LoadData();
            }
        }

        private void ClearInputs()
        {
            txtTitle.Clear(); txtISBN.Clear(); txtPrice.Clear(); txtStock.Clear(); txtAuthorId.Clear();
        }
    }
}
