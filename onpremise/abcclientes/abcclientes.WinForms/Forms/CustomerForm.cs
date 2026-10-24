using abcclientes.Application.DTOs;
using abcclientes.WinForms.Services;
using System.Text.RegularExpressions;

namespace abcclientes.WinForms.Forms;

public partial class CustomerForm : Form
{
    // ── Colors ────────────────────────────────────────────────────────
    private static readonly Color CHeaderBar  = Color.FromArgb(31,  31,  47);
    private static readonly Color CGold        = Color.FromArgb(212, 175, 55);
    private static readonly Color CFormBg      = Color.FromArgb(245, 245, 248);
    private static readonly Color CBtnDark     = Color.FromArgb(60,  60,  80);
    private static readonly Color CWhite       = Color.White;
    private static readonly Color CLabelColor  = Color.FromArgb(80,  80, 100);
    private static readonly Color CGridHeader  = Color.FromArgb(31,  31,  47);
    private static readonly Color CGridAlt     = Color.FromArgb(248, 248, 252);

    // ── State ─────────────────────────────────────────────────────────
    private enum FormMode { Read, New, Edit }
    private FormMode _mode = FormMode.Read;
    private int _selectedId = 0;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private const int PageSize = 10;

    // ── Services ──────────────────────────────────────────────────────
    private readonly CustomerApiService _api;

    // ── Controls ──────────────────────────────────────────────────────
    private Panel _pnlHeader = null!;
    private Panel _pnlToolbar = null!;
    private Panel _pnlMaster = null!;

    private Button _btnNuevo = null!, _btnBuscar = null!, _btnEditar = null!, _btnGrabar = null!, _btnCancelar = null!;
    private Label _lblModoLectura = null!, _lblEstado = null!, _lblRecordId = null!, _lblUser = null!;

    private TextBox _txtSearch = null!, _txtNombre = null!, _txtDireccion = null!, _txtTelefono = null!;

    private DataGridView _dgv = null!;
    private Label _lblPageInfo = null!;
    private Button _btnFirst = null!, _btnPrev = null!, _btnNext = null!, _btnLast = null!;

    public CustomerForm(CustomerApiService api)
    {
        _api = api;
        BuildLayout();
    }

    // ═══════════════════════════════════════════════════════════════════
    //  LAYOUT CONSTRUCTION
    // ═══════════════════════════════════════════════════════════════════
    private void BuildLayout()
    {
        SuspendLayout();
        BackColor     = CFormBg;
        Font          = new Font("Segoe UI", 9.5f);
        Text          = "Gestión de Clientes";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize   = new Size(1000, 680);
        WindowState   = FormWindowState.Maximized;

        BuildHeaderBar();
        BuildToolbar();
        BuildMasterPanel();

        ResumeLayout(false);
        PerformLayout();
    }

    // ── Zone 1 — Top header bar ───────────────────────────────────────
    private void BuildHeaderBar()
    {
        _pnlHeader = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 68,
            BackColor = CHeaderBar,
            Padding   = new Padding(16, 0, 16, 0)
        };

        var lblApp = new Label
        {
            Text      = "ABC Clientes",
            ForeColor = CGold,
            Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
            AutoSize  = true,
            Location  = new Point(56, 12)
        };

        var lblSub = new Label
        {
            Text      = "Sistema de Gestión de Clientes",
            ForeColor = Color.FromArgb(160, 160, 180),
            Font      = new Font("Segoe UI", 8.5f),
            AutoSize  = true,
            Location  = new Point(58, 42)
        };

        _lblUser = new Label
        {
            Text      = "Administrador",
            ForeColor = Color.FromArgb(200, 200, 210),
            Font      = new Font("Segoe UI", 9f),
            AutoSize  = true,
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
        };

        // Icon circle
        var pnlIcon = new Panel
        {
            Size      = new Size(40, 40),
            BackColor = CGold,
            Location  = new Point(10, 14)
        };
        pnlIcon.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(new SolidBrush(CGold), 0, 0, 39, 39);
            var sf = new StringFormat
            {
                Alignment  = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            e.Graphics.DrawString("AC", new Font("Segoe UI", 11f, FontStyle.Bold),
                new SolidBrush(CHeaderBar), new RectangleF(0, 0, 40, 40), sf);
        };

        _pnlHeader.SizeChanged += (s, e) =>
        {
            _lblUser.Location = new Point(_pnlHeader.Width - 160, 24);
        };

        _pnlHeader.Controls.AddRange(new Control[] { pnlIcon, lblApp, lblSub, _lblUser });
        Controls.Add(_pnlHeader);
    }

    // ── Zone 2 — Toolbar ─────────────────────────────────────────────
    private void BuildToolbar()
    {
        _pnlToolbar = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 54,
            BackColor = CWhite,
            Padding   = new Padding(16, 0, 16, 0)
        };
        _pnlToolbar.Paint += (s, e) =>
            e.Graphics.DrawLine(new Pen(Color.FromArgb(210, 210, 220)),
                0, _pnlToolbar.Height - 1, _pnlToolbar.Width, _pnlToolbar.Height - 1);

        _btnNuevo    = MakeToolBtn("＋  Nuevo",   CBtnDark,                   10);
        _btnBuscar   = MakeToolBtn("🔍  Buscar",  CBtnDark,                  120);
        _btnEditar   = MakeToolBtn("✏  Editar",   CBtnDark,                  230);
        _btnGrabar   = MakeToolBtn("💾  Grabar",   CGold,                     340);
        _btnCancelar = MakeToolBtn("✕  Cancelar", Color.FromArgb(160, 60, 60), 460);

        _lblModoLectura = new Label
        {
            Text      = "MODO LECTURA",
            ForeColor = Color.FromArgb(120, 120, 140),
            Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
            AutoSize  = true,
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
        };

        _lblEstado = new Label
        {
            Text      = "● ACTIVO",
            ForeColor = Color.FromArgb(34, 160, 90),
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            AutoSize  = true,
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
        };

        _pnlToolbar.SizeChanged += (s, e) =>
        {
            _lblEstado.Location      = new Point(_pnlToolbar.Width - 90, 18);
            _lblModoLectura.Location = new Point(_pnlToolbar.Width - 220, 18);
        };

        _btnNuevo.Click    += BtnNuevo_Click;
        _btnBuscar.Click   += BtnBuscar_Click;
        _btnEditar.Click   += BtnEditar_Click;
        _btnGrabar.Click   += BtnGrabar_Click;
        _btnCancelar.Click += BtnCancelar_Click;

        _pnlToolbar.Controls.AddRange(new Control[]
        {
            _btnNuevo, _btnBuscar, _btnEditar, _btnGrabar, _btnCancelar,
            _lblModoLectura, _lblEstado
        });
        Controls.Add(_pnlToolbar);
    }

    private Button MakeToolBtn(string text, Color back, int x)
    {
        var btn = new Button
        {
            Text      = text,
            BackColor = back,
            ForeColor = CWhite,
            FlatStyle = FlatStyle.Flat,
            Size      = new Size(100, 34),
            Location  = new Point(x, 10),
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor    = Cursors.Hand
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    // ── Zone 3 — Master panel (fields + search + grid) ───────────────
    private void BuildMasterPanel()
    {
        _pnlMaster = new Panel
        {
            Dock    = DockStyle.Fill,
            Padding = new Padding(16)
        };

        // Initialize _lblRecordId before paint handler
        _lblRecordId = new Label
        {
            Text      = "",
            ForeColor = Color.FromArgb(180, 180, 200),
            Font      = new Font("Segoe UI", 8.5f),
            AutoSize  = true,
            Anchor    = AnchorStyles.Top | AnchorStyles.Right,
            Location  = new Point(700, 10)
        };

        // Section title bar
        var pnlSectionTitle = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 38,
            BackColor = CHeaderBar
        };
        pnlSectionTitle.Paint += (s, e) =>
        {
            var sf = new StringFormat { LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString("  ☰  DATOS GENERALES DEL CLIENTE",
                new Font("Segoe UI", 10f, FontStyle.Bold),
                new SolidBrush(CGold), new RectangleF(8, 0, 400, 38), sf);
        };
        pnlSectionTitle.Controls.Add(_lblRecordId);
        pnlSectionTitle.SizeChanged += (s, e) =>
        {
            _lblRecordId.Location = new Point(pnlSectionTitle.Width - 200, 10);
        };

        // Fields panel
        var pnlFields = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 180,
            BackColor = CWhite,
            Padding   = new Padding(16, 12, 16, 8)
        };

        var tbl = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = 3,
            BackColor   = CWhite
        };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));

        _txtNombre    = MakeField(tbl, "NOMBRE COMPLETO *", 0, 0);
        _txtDireccion = MakeField(tbl, "DIRECCIÓN / DOMICILIO", 0, 1);
        _txtTelefono  = MakeField(tbl, "TELÉFONO PRINCIPAL", 1, 0);

        // Search box on row 1 col 1 — fixed layout with button
        var pnlSearch = MakeSearchPanel();
        tbl.Controls.Add(pnlSearch, 1, 1);

        pnlFields.Controls.Add(tbl);

        // Separator line
        var sep = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 2,
            BackColor = Color.FromArgb(230, 230, 238)
        };

        // Grid section title
        var pnlGridTitle = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 38,
            BackColor = CHeaderBar
        };
        pnlGridTitle.Paint += (s, e) =>
        {
            var sf = new StringFormat { LineAlignment = StringAlignment.Center };
            e.Graphics.DrawString("  ☰  LISTADO DE CLIENTES",
                new Font("Segoe UI", 10f, FontStyle.Bold),
                new SolidBrush(CGold), new RectangleF(8, 0, 400, 38), sf);
        };

        // DataGridView
        _dgv = new DataGridView
        {
            Dock                          = DockStyle.Fill,
            BorderStyle                   = BorderStyle.None,
            BackgroundColor               = CWhite,
            GridColor                     = Color.FromArgb(220, 220, 230),
            ColumnHeadersHeight           = 36,
            RowTemplate                   = { Height = 30 },
            SelectionMode                 = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect                   = false,
            AllowUserToAddRows            = false,
            AllowUserToDeleteRows         = false,
            ReadOnly                      = true,
            AutoSizeColumnsMode           = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles     = false,
            ColumnHeadersDefaultCellStyle = { BackColor = CGridHeader, ForeColor = CWhite, Font = new Font("Segoe UI", 9f, FontStyle.Bold) },
            AlternatingRowsDefaultCellStyle = { BackColor = CGridAlt },
            DefaultCellStyle              = { Font = new Font("Segoe UI", 9f), SelectionBackColor = Color.FromArgb(212, 175, 55, 80), SelectionForeColor = Color.Black }
        };
        _dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 0, 0, 0);
        _dgv.CellClick += DgvCustomers_CellClick;
        _dgv.DataBindingComplete += DgvCustomers_DataBindingComplete;
        _dgv.CellFormatting += DgvCustomers_CellFormatting;

        // Pagination panel
        var pnlPagination = BuildPaginationPanel();

        // Assemble master panel (order matters for Dock layout)
        _pnlMaster.Controls.Add(_dgv);            // Fill
        _pnlMaster.Controls.Add(pnlPagination);   // Bottom
        _pnlMaster.Controls.Add(pnlGridTitle);     // Top
        _pnlMaster.Controls.Add(sep);             // Top
        _pnlMaster.Controls.Add(pnlFields);       // Top
        _pnlMaster.Controls.Add(pnlSectionTitle); // Top (added last = appears at very top)

        Controls.Add(_pnlMaster);

        SetMode(FormMode.Read);
    }

    private Panel MakeSearchPanel()
    {
        var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CWhite };

        var lbl = new Label
        {
            Text      = "BUSCAR (Nombre / Teléfono)",
            ForeColor = CLabelColor,
            Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            Dock      = DockStyle.Top,
            Height    = 18,
            Padding   = new Padding(0, 2, 0, 0)
        };

        var pnlInput = new Panel { Dock = DockStyle.Fill, BackColor = CWhite };

        _txtSearch = new TextBox
        {
            Dock       = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Font       = new Font("Segoe UI", 10f),
            BackColor  = Color.FromArgb(250, 250, 252)
        };

        var btnSearch = new Button
        {
            Text      = "🔍",
            Size      = new Size(32, 28),
            BackColor = CHeaderBar,
            ForeColor = CGold,
            FlatStyle = FlatStyle.Flat,
            Dock      = DockStyle.Right,
            Cursor    = Cursors.Hand,
            Font      = new Font("Segoe UI", 10f)
        };
        btnSearch.FlatAppearance.BorderSize = 0;
        btnSearch.Click += async (s, e) =>
        {
            _currentPage = 1;
            await LoadGridAsync();
        };

        // Accept Enter key in search box
        _txtSearch.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                _currentPage = 1;
                await LoadGridAsync();
            }
        };

        pnlInput.Controls.Add(_txtSearch);
        pnlInput.Controls.Add(btnSearch);
        pnl.Controls.Add(pnlInput);
        pnl.Controls.Add(lbl);
        return pnl;
    }

    private Panel MakeFieldPanel(string label)
    {
        var pnl = new Panel { Dock = DockStyle.Fill, BackColor = CWhite };
        var lbl = new Label
        {
            Text      = label,
            ForeColor = CLabelColor,
            Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
            Dock      = DockStyle.Top,
            Height    = 18,
            Padding   = new Padding(0, 2, 0, 0)
        };
        var txt = new TextBox
        {
            Dock       = DockStyle.Top,
            BorderStyle = BorderStyle.FixedSingle,
            Font       = new Font("Segoe UI", 10f),
            BackColor  = Color.FromArgb(250, 250, 252)
        };
        pnl.Controls.Add(txt);
        pnl.Controls.Add(lbl);
        return pnl;
    }

    private TextBox MakeField(TableLayoutPanel tbl, string label, int col, int row)
    {
        var pnl = MakeFieldPanel(label);
        tbl.Controls.Add(pnl, col, row);
        return (TextBox)pnl.Controls[0];
    }

    private Panel BuildPaginationPanel()
    {
        var pnl = new Panel { Dock = DockStyle.Bottom, Height = 36, BackColor = CWhite };
        _btnFirst = MakePagBtn("◀◀", 8);
        _btnPrev  = MakePagBtn("◀",  50);
        _btnNext  = MakePagBtn("▶",  130);
        _btnLast  = MakePagBtn("▶▶", 172);
        _lblPageInfo = new Label
        {
            Text      = "Página 1 de 1",
            AutoSize  = true,
            Location  = new Point(92, 9),
            ForeColor = CLabelColor,
            Font      = new Font("Segoe UI", 9f)
        };
        _btnFirst.Click += async (s, e) => { _currentPage = 1; await LoadGridAsync(); };
        _btnPrev.Click  += async (s, e) => { if (_currentPage > 1) { _currentPage--; await LoadGridAsync(); } };
        _btnNext.Click  += async (s, e) => { if (_currentPage < _totalPages) { _currentPage++; await LoadGridAsync(); } };
        _btnLast.Click  += async (s, e) => { _currentPage = _totalPages; await LoadGridAsync(); };
        pnl.Controls.AddRange(new Control[] { _btnFirst, _btnPrev, _lblPageInfo, _btnNext, _btnLast });
        return pnl;
    }

    private Button MakePagBtn(string text, int x)
    {
        var b = new Button
        {
            Text      = text,
            Size      = new Size(36, 26),
            Location  = new Point(x, 5),
            FlatStyle = FlatStyle.Flat,
            BackColor = CHeaderBar,
            ForeColor = CWhite,
            Cursor    = Cursors.Hand,
            Font      = new Font("Segoe UI", 8f)
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  MODE MANAGEMENT
    // ═══════════════════════════════════════════════════════════════════
    private void SetMode(FormMode mode)
    {
        _mode = mode;
        bool isReadOnly = mode == FormMode.Read;

        _txtNombre.ReadOnly    = isReadOnly;
        _txtDireccion.ReadOnly = isReadOnly;
        _txtTelefono.ReadOnly  = isReadOnly;
        _txtNombre.BackColor    = isReadOnly ? Color.FromArgb(240, 240, 245) : Color.White;
        _txtDireccion.BackColor = isReadOnly ? Color.FromArgb(240, 240, 245) : Color.White;
        _txtTelefono.BackColor  = isReadOnly ? Color.FromArgb(240, 240, 245) : Color.White;

        _btnGrabar.Enabled   = !isReadOnly;
        _btnGrabar.BackColor  = !isReadOnly ? CGold : Color.FromArgb(180, 160, 100);
        _btnCancelar.Visible  = !isReadOnly;
        _btnEditar.Enabled    = isReadOnly && _selectedId > 0;

        _lblModoLectura.Text        = mode == FormMode.Read ? "MODO LECTURA" : (mode == FormMode.New ? "MODO NUEVO" : "MODO EDICIÓN");
        _lblModoLectura.ForeColor   = mode == FormMode.Read ? Color.FromArgb(120, 120, 140) : Color.FromArgb(34, 160, 90);
    }

    private void ClearFields()
    {
        _txtNombre.Clear();
        _txtDireccion.Clear();
        _txtTelefono.Clear();
        _selectedId = 0;
        _lblRecordId.Text = "";
        _lblEstado.Text      = "";
        _lblEstado.ForeColor = Color.Gray;
        _btnEditar.Enabled   = false;
    }

    private void PopulateFields(CustomerResponse c)
    {
        _txtNombre.Text    = c.Nombre;
        _txtDireccion.Text = c.Direccion ?? string.Empty;
        _txtTelefono.Text  = c.Telefono  ?? string.Empty;
        _selectedId        = c.Id;
        _lblRecordId.Text  = $"ID: {c.Id}  |  Creado: {c.FechaCreacion:dd/MM/yyyy}";
        _lblEstado.Text      = c.Activo ? "● ACTIVO" : "● INACTIVO";
        _lblEstado.ForeColor = c.Activo ? Color.FromArgb(34, 160, 90) : Color.FromArgb(200, 60, 60);
        _pnlMaster.Invalidate(true);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  DATA
    // ═══════════════════════════════════════════════════════════════════
    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadGridAsync();
    }

    private async Task LoadGridAsync()
    {
        try
        {
            var search = _txtSearch.Text.Trim().Length > 0 ? _txtSearch.Text.Trim() : null;
            var result = await _api.GetAllAsync(search, _currentPage, PageSize);

            _dgv.DataSource = result.Items;
            _totalPages = result.TotalPages > 0 ? result.TotalPages : 1;
            _lblPageInfo.Text = $"Página {_currentPage} de {_totalPages}";

            _btnFirst.Enabled = _btnPrev.Enabled = _currentPage > 1;
            _btnNext.Enabled  = _btnLast.Enabled  = _currentPage < _totalPages;
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Error al conectar con la API.\n{ex.Message}", "Error de conexión",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvCustomers_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || _dgv.Rows[e.RowIndex].DataBoundItem is not CustomerResponse c) return;
        PopulateFields(c);
        SetMode(FormMode.Read);
    }

    private void DgvCustomers_DataBindingComplete(object? sender, DataGridViewBindingCompleteEventArgs e)
    {
        // Hide columns we don't want to show in the grid
        if (_dgv.Columns["FechaModificacion"] != null)
            _dgv.Columns["FechaModificacion"].Visible = false;
        if (_dgv.Columns["Activo"] != null)
            _dgv.Columns["Activo"].Visible = false;
        if (_dgv.Columns["FechaCreacion"] != null)
            _dgv.Columns["FechaCreacion"].HeaderText = "Fecha Creación";
    }

    private void DgvCustomers_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0) return;
        if (_dgv.Columns[e.ColumnIndex].Name == "FechaCreacion" && e.Value is DateTime dt)
        {
            e.Value = dt.ToString("dd/MM/yyyy");
            e.FormattingApplied = true;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  TOOLBAR EVENTS
    // ═══════════════════════════════════════════════════════════════════
    private void BtnNuevo_Click(object? sender, EventArgs e)
    {
        ClearFields();
        SetMode(FormMode.New);
        _txtNombre.Focus();
    }

    private async void BtnBuscar_Click(object? sender, EventArgs e)
    {
        _currentPage = 1;
        await LoadGridAsync();
    }

    private void BtnEditar_Click(object? sender, EventArgs e)
    {
        if (_selectedId == 0)
        {
            MessageBox.Show("Selecciona un cliente de la lista primero.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        SetMode(FormMode.Edit);
        _txtNombre.Focus();
    }

    private async void BtnGrabar_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs()) return;

        try
        {
            if (_mode == FormMode.New)
            {
                var req = new CreateCustomerRequest
                {
                    Nombre    = _txtNombre.Text.Trim(),
                    Direccion = NullIfEmpty(_txtDireccion.Text),
                    Telefono  = NullIfEmpty(_txtTelefono.Text)
                };
                var created = await _api.CreateAsync(req);
                PopulateFields(created);
                MessageBox.Show("Cliente creado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var req = new UpdateCustomerRequest
                {
                    Nombre    = _txtNombre.Text.Trim(),
                    Direccion = NullIfEmpty(_txtDireccion.Text),
                    Telefono  = NullIfEmpty(_txtTelefono.Text)
                };
                var updated = await _api.UpdateAsync(_selectedId, req);
                PopulateFields(updated);
                MessageBox.Show("Cliente actualizado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            SetMode(FormMode.Read);
            _currentPage = 1;
            await LoadGridAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al guardar: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnCancelar_Click(object? sender, EventArgs e)
    {
        ClearFields();
        SetMode(FormMode.Read);
        await LoadGridAsync();
    }

    // ═══════════════════════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════════════════════
    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(_txtNombre.Text))
        {
            MessageBox.Show("El campo Nombre es obligatorio.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNombre.Focus();
            return false;
        }
        if (_txtNombre.Text.Length > 100)
        {
            MessageBox.Show("Nombre no puede superar 100 caracteres.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (_txtDireccion.Text.Length > 200)
        {
            MessageBox.Show("Dirección no puede superar 200 caracteres.", "Validación",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        if (!string.IsNullOrWhiteSpace(_txtTelefono.Text))
        {
            if (_txtTelefono.Text.Length > 20)
            {
                MessageBox.Show("Teléfono no puede superar 20 caracteres.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!Regex.IsMatch(_txtTelefono.Text, @"^\d+$"))
            {
                MessageBox.Show("Teléfono solo debe contener dígitos.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }
        return true;
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
