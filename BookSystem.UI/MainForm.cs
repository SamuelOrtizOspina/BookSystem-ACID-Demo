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

        private string ConnectionString => "Server=localhost,1433;Database=BookStoreDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            LoadData();
        }

        private void SetupUI()
        {
            this.Text = "BookSystem Professional - Gestión de Inventario";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Panel Superior (Formulario)
            Panel pnlInput = new Panel { Dock = DockStyle.Top, Height = 150, Padding = new Padding(20), BackColor = Color.White };
            
            int x = 20, y = 20;
            AddInput(pnlInput, "Título:", ref txtTitle, x, y);
            AddInput(pnlInput, "ISBN:", ref txtISBN, x + 200, y);
            AddInput(pnlInput, "Precio:", ref txtPrice, x + 400, y);
            AddInput(pnlInput, "Stock:", ref txtStock, x + 550, y);
            AddInput(pnlInput, "ID Autor:", ref txtAuthorId, x + 700, y);

            Button btnAdd = new Button { Text = "Guardar Libro", Location = new Point(20, 80), Size = new Size(150, 40), BackColor = Color.FromArgb(0, 122, 204), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnAdd.Click += (s, e) => AddBook();
            pnlInput.Controls.Add(btnAdd);

            Button btnRefresh = new Button { Text = "Refrescar Tabla", Location = new Point(180, 80), Size = new Size(150, 40), BackColor = Color.FromArgb(40, 167, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnRefresh.Click += (s, e) => LoadData();
            pnlInput.Controls.Add(btnRefresh);

            Button btnACID = new Button { Text = "Prueba ACID", Location = new Point(340, 80), Size = new Size(150, 40), BackColor = Color.FromArgb(220, 53, 69), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnACID.Click += (s, e) => RunACIDTest();
            pnlInput.Controls.Add(btnACID);

            // DataGridView
            dgvBooks = new DataGridView { 
                Dock = DockStyle.Fill, 
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true
            };

            // Status Label
            lblStatus = new Label { Dock = DockStyle.Bottom, Height = 30, Text = " Listo", TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(220, 220, 220) };

            this.Controls.Add(dgvBooks);
            this.Controls.Add(pnlInput);
            this.Controls.Add(lblStatus);
        }

        private void AddInput(Panel p, string label, ref TextBox tb, int x, int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(x, y), AutoSize = true };
            tb = new TextBox { Location = new Point(x, y + 20), Width = 150 };
            p.Controls.Add(lbl);
            p.Controls.Add(tb);
        }

        private void LoadData()
        {
            try {
                using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                    conn.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter("SELECT B.Id, B.Title, B.ISBN, B.Price, B.Stock, A.Name as Autor FROM Books B JOIN Authors A ON B.AuthorId = A.Id", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvBooks.DataSource = dt;
                    lblStatus.Text = $" {dt.Rows.Count} libros cargados.";
                }
            } catch (Exception ex) {
                MessageBox.Show("Error al conectar: " + ex.Message);
            }
        }

        private void AddBook()
        {
            try {
                using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                    conn.Open();
                    string query = "INSERT INTO Books (Title, ISBN, Price, Stock, AuthorId) VALUES (@t, @i, @p, @s, @a)";
                    using (SqlCommand cmd = new SqlCommand(query, conn)) {
                        cmd.Parameters.AddWithValue("@t", txtTitle.Text);
                        cmd.Parameters.AddWithValue("@i", txtISBN.Text);
                        cmd.Parameters.AddWithValue("@p", decimal.Parse(txtPrice.Text));
                        cmd.Parameters.AddWithValue("@s", int.Parse(txtStock.Text));
                        cmd.Parameters.AddWithValue("@a", int.Parse(txtAuthorId.Text));
                        cmd.ExecuteNonQuery();
                    }
                    lblStatus.Text = " ✅ Libro guardado correctamente.";
                    LoadData();
                    ClearInputs();
                }
            } catch (Exception ex) {
                MessageBox.Show("Error al guardar: " + ex.Message);
            }
        }

        private void RunACIDTest()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction()) {
                    try {
                        new SqlCommand("UPDATE Books SET Price = 99999 WHERE Id = 1", conn, trans).ExecuteNonQuery();
                        DialogResult result = MessageBox.Show("Paso 1: Precio de ID 1 cambiado a 99999 (en memoria).
¿Desea simular un error para probar el ROLLBACK?", "Prueba de Atomicidad", MessageBoxButtons.YesNo);
                        
                        if (result == DialogResult.Yes) {
                            throw new Exception("Fallo simulado del sistema.");
                        }
                        trans.Commit();
                        MessageBox.Show("Transacción completada (COMMIT).");
                    } catch (Exception ex) {
                        trans.Rollback();
                        MessageBox.Show("Rollback ejecutado: Los datos no cambiaron.
Motivo: " + ex.Message, "Atomicidad OK");
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
