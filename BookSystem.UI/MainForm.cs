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
        // Colores Modernos (Paleta Dashboard)
        private readonly Color PrimaryColor = Color.FromArgb(44, 62, 80);    // Midnight Blue
        private readonly Color SecondaryColor = Color.FromArgb(52, 152, 219); // Bright Blue
        private readonly Color SuccessColor = Color.FromArgb(39, 174, 96);   // Emerald Green
        private readonly Color DangerColor = Color.FromArgb(192, 57, 43);    // Alizarin Red
        private readonly Color BackgroundColor = Color.FromArgb(236, 240, 241); // Clouds Grey
        private readonly Color TextColor = Color.FromArgb(255, 255, 255);

        private DataGridView dgvBooks;
        private TextBox txtTitle, txtISBN, txtPrice, txtStock, txtAuthorId;
        private Label lblStatus;

        private string ConnectionString => "Server=127.0.0.1,1433;Database=BookStoreDB;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=False;";

        public MainForm()
        {
            InitializeComponent();
            SetupModernUI();
            LoadData();
        }

        private void SetupModernUI()
        {
            // Configuracion de Ventana
            this.Text = "BookSystem ERP - Modulo de Inventario";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = BackgroundColor;
            this.Font = new Font("Segoe UI", 10);

            // 1. PANEL LATERAL (Sidebar)
            Panel pnlSidebar = new Panel {
                Dock = DockStyle.Left,
                Width = 220,
                BackColor = PrimaryColor,
                Padding = new Padding(10)
            };

            Label lblLogo = new Label {
                Text = "BOOK SYSTEM\nADMIN",
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 14, FontStyle.Bold),
                Height = 80,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlSidebar.Controls.Add(lblLogo);

            // Botones del Sidebar
            AddSidebarButton(pnlSidebar, "Refrescar Lista", SecondaryColor, (s, e) => LoadData());
            AddSidebarButton(pnlSidebar, "Eliminar Registro", DangerColor, (s, e) => DeleteSelected());
            AddSidebarButton(pnlSidebar, "Prueba ACID", Color.FromArgb(142, 68, 173), (s, e) => RunACIDTest()); // Purple

            // 2. PANEL DE CABECERA
            Panel pnlHeader = new Panel {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                Padding = new Padding(20, 0, 0, 0)
            };
            Label lblTitle = new Label {
                Text = "Panel de Gestion de Libros e Inventario",
                ForeColor = PrimaryColor,
                Font = new Font("Segoe UI Semibold", 16),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblTitle);

            // 3. AREA CENTRAL (Inputs + Grid)
            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            // Tarjeta de Inputs (Card)
            Panel pnlCard = new Panel {
                Dock = DockStyle.Top,
                Height = 180,
                BackColor = Color.White,
                Padding = new Padding(20)
            };
            
            int x = 20, y = 20;
            AddModernInput(pnlCard, "Titulo del Libro", ref txtTitle, x, y, 300);
            AddModernInput(pnlCard, "ISBN", ref txtISBN, x + 330, y, 150);
            AddModernInput(pnlCard, "Precio ($)", ref txtPrice, x + 500, y, 100);
            AddModernInput(pnlCard, "Stock", ref txtStock, x + 620, y, 80);
            AddModernInput(pnlCard, "Autor (ID o Nombre)", ref txtAuthorId, x + 720, y, 200);

            Button btnSave = new Button {
                Text = "REGISTRAR LIBRO",
                Location = new Point(20, 100),
                Size = new Size(200, 45),
                BackColor = SuccessColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += (s, e) => AddBook();
            pnlCard.Controls.Add(btnSave);

            // Grid Styling
            dgvBooks = new DataGridView { 
                Dock = DockStyle.Fill, 
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                EnableHeadersVisualStyles = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                RowTemplate = { Height = 35 },
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvBooks.ColumnHeadersDefaultCellStyle.BackColor = PrimaryColor;
            dgvBooks.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI Semibold", 10);
            dgvBooks.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvBooks.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

            // Footer
            lblStatus = new Label {
                Dock = DockStyle.Bottom,
                Height = 35,
                Text = " Sistema Conectado a SQL Server Local",
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.White,
                ForeColor = Color.Gray
            };

            pnlMain.Controls.Add(dgvBooks);
            pnlMain.Controls.Add(new Label { Dock = DockStyle.Top, Height = 20 }); // Espaciador
            pnlMain.Controls.Add(pnlCard);

            this.Controls.Add(pnlMain);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(lblStatus);
        }

        private void AddSidebarButton(Panel parent, string text, Color color, EventHandler click)
        {
            Button btn = new Button {
                Text = text,
                Dock = DockStyle.Top,
                Height = 50,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = PrimaryColor,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = color;
            btn.Click += click;
            parent.Controls.Add(btn);
            
            // Espaciador
            Panel spacer = new Panel { Dock = DockStyle.Top, Height = 5 };
            parent.Controls.Add(spacer);
        }

        private void AddModernInput(Panel p, string label, ref TextBox tb, int x, int y, int width)
        {
            Label lbl = new Label { Text = label, Location = new Point(x, y), AutoSize = true, ForeColor = Color.Gray, Font = new Font("Segoe UI", 9) };
            tb = new TextBox { Location = new Point(x, y + 25), Width = width, BorderStyle = BorderStyle.FixedSingle };
            p.Controls.Add(lbl);
            p.Controls.Add(tb);
        }

        private void LoadData()
        {
            try {
                using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                    conn.Open();
                    string sql = "SELECT B.Id, B.Title as TITULO, B.ISBN, B.Price as PRECIO, B.Stock as STOCK, A.Name as AUTOR FROM Books B JOIN Authors A ON B.AuthorId = A.Id ORDER BY B.Id DESC";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvBooks.DataSource = dt;
                    lblStatus.Text = $" Auditoria: {dt.Rows.Count} registros sincronizados.";
                }
            } catch (Exception ex) {
                MessageBox.Show("Error de Sincronizacion: " + ex.Message, "System Error");
            }
        }

        private void AddBook()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text)) return;
            try {
                using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                    conn.Open();
                    int authorId;
                    if (int.TryParse(txtAuthorId.Text, out int id)) authorId = id;
                    else {
                        string sqlA = "INSERT INTO Authors (Name, Bio) OUTPUT INSERTED.Id VALUES (@n, 'Registro Automatico UI')";
                        using (SqlCommand cmdA = new SqlCommand(sqlA, conn)) {
                            cmdA.Parameters.AddWithValue("@n", txtAuthorId.Text.Trim());
                            authorId = (int)(cmdA.ExecuteScalar() ?? 0);
                        }
                    }

                    string sqlB = "INSERT INTO Books (Title, ISBN, Price, Stock, AuthorId) VALUES (@t, @i, @p, @s, @a)";
                    using (SqlCommand cmdB = new SqlCommand(sqlB, conn)) {
                        cmdB.Parameters.AddWithValue("@t", txtTitle.Text.Trim());
                        cmdB.Parameters.AddWithValue("@i", txtISBN.Text.Trim());
                        cmdB.Parameters.AddWithValue("@p", decimal.Parse(txtPrice.Text.Replace(",", ".")));
                        cmdB.Parameters.AddWithValue("@s", int.Parse(txtStock.Text));
                        cmdB.Parameters.AddWithValue("@a", authorId);
                        cmdB.ExecuteNonQuery();
                    }
                    LoadData();
                    ClearInputs();
                }
            } catch (Exception ex) {
                MessageBox.Show("Validacion fallida: " + ex.Message);
            }
        }

        private void DeleteSelected()
        {
            if (dgvBooks.SelectedRows.Count == 0) return;
            int id = Convert.ToInt32(dgvBooks.SelectedRows[0].Cells["Id"].Value);
            if (MessageBox.Show($"Seguro que desea eliminar el registro {id}?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                using (SqlConnection conn = new SqlConnection(ConnectionString)) {
                    conn.Open();
                    new SqlCommand($"DELETE FROM Books WHERE Id = {id}", conn).ExecuteNonQuery();
                    LoadData();
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
                        string msg = "PASO 1: Transaccion iniciada (Precio ID 1 -> 99999).\n¿Desea forzar un error para validar el ROLLBACK?";
                        if (MessageBox.Show(msg, "Prueba de Atomicidad", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                            throw new Exception("Error provocado por el usuario.");
                        }
                        trans.Commit();
                        MessageBox.Show("COMMIT: Datos guardados permanentemente.");
                    } catch (Exception ex) {
                        trans.Rollback();
                        MessageBox.Show("ROLLBACK: Los datos han vuelto a su estado original.\nMotivo: " + ex.Message, "Prueba ACID Exitosa");
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
