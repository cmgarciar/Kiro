using abcclientes.Application.DTOs;
using abcclientes.WinForms.Services;
using System.Text.RegularExpressions;

namespace abcclientes.WinForms.Forms;

/// <summary>
/// Main customer management form with DataGridView listing, search, pagination, and CRUD operations.
/// </summary>
public partial class CustomerForm : Form
{
    private readonly CustomerApiService _apiService;

    // Pagination state
    private int _currentPage = 1;
    private int _pageSize = 10;
    private int _totalPages = 1;

    // Controls
    private DataGridView _dgvCustomers = null!;
    private TextBox _txtSearch = null!;
    private TextBox _txtNombre = null!;
    private TextBox _txtDireccion = null!;
    private TextBox _txtTelefono = null!;
    private Label _lblPageInfo = null!;
    private Button _btnFirst = null!;
    private Button _btnPrev = null!;
    private Button _btnNext = null!;
    private Button _btnLast = null!;

    public CustomerForm(CustomerApiService apiService)
    {
        _apiService = apiService;
        InitializeComponent();
    }

    protected override async void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        await LoadDataAsync();
    }

    // ─── Data Loading ───────────────────────────────────────────────

    private async Task LoadDataAsync()
    {
        try
        {
            var search = string.IsNullOrWhiteSpace(_txtSearch.Text) ? null : _txtSearch.Text;
            var result = await _apiService.GetAllAsync(search, _currentPage, _pageSize);

            _dgvCustomers.DataSource = result.Items;
            _totalPages = result.TotalPages > 0 ? result.TotalPages : 1;

            UpdatePaginationControls();
            UpdatePageInfoLabel();
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Error al conectar con la API. Asegurate de que el servidor este en ejecucion.\n\nDetalle: {ex.Message}",
                "Error de conexion", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar los datos: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ─── CRUD Operations ────────────────────────────────────────────

    private async void BtnCreate_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs())
            return;

        try
        {
            var request = new CreateCustomerRequest
            {
                Nombre = _txtNombre.Text.Trim(),
                Direccion = string.IsNullOrWhiteSpace(_txtDireccion.Text) ? null : _txtDireccion.Text.Trim(),
                Telefono = string.IsNullOrWhiteSpace(_txtTelefono.Text) ? null : _txtTelefono.Text.Trim()
            };

            await _apiService.CreateAsync(request);
            ClearInputs();
            _currentPage = 1;
            await LoadDataAsync();
            MessageBox.Show("Cliente creado correctamente.", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Error al crear el cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DgvCustomers_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0)
            return;

        var row = _dgvCustomers.Rows[e.RowIndex];
        _txtNombre.Text = row.Cells["Nombre"].Value?.ToString() ?? string.Empty;
        _txtDireccion.Text = row.Cells["Direccion"].Value?.ToString() ?? string.Empty;
        _txtTelefono.Text = row.Cells["Telefono"].Value?.ToString() ?? string.Empty;
    }

    private async void BtnUpdate_Click(object? sender, EventArgs e)
    {
        if (!ValidateInputs())
            return;

        if (_dgvCustomers.CurrentRow is null)
        {
            MessageBox.Show("Selecciona un cliente de la lista para actualizar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var id = Convert.ToInt32(_dgvCustomers.CurrentRow.Cells["Id"].Value);

        try
        {
            var request = new UpdateCustomerRequest
            {
                Nombre = _txtNombre.Text.Trim(),
                Direccion = string.IsNullOrWhiteSpace(_txtDireccion.Text) ? null : _txtDireccion.Text.Trim(),
                Telefono = string.IsNullOrWhiteSpace(_txtTelefono.Text) ? null : _txtTelefono.Text.Trim()
            };

            await _apiService.UpdateAsync(id, request);
            ClearInputs();
            await LoadDataAsync();
            MessageBox.Show("Cliente actualizado correctamente.", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Error al actualizar el cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnDelete_Click(object? sender, EventArgs e)
    {
        if (_dgvCustomers.CurrentRow is null)
        {
            MessageBox.Show("Selecciona un cliente de la lista para eliminar.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var id = Convert.ToInt32(_dgvCustomers.CurrentRow.Cells["Id"].Value);
        var nombre = _dgvCustomers.CurrentRow.Cells["Nombre"].Value?.ToString();

        var confirm = MessageBox.Show($"¿Seguro que quieres eliminar el cliente \"{nombre}\"?",
            "Confirmar eliminacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
            return;

        try
        {
            await _apiService.DeleteAsync(id);
            ClearInputs();
            await LoadDataAsync();
            MessageBox.Show("Cliente eliminado correctamente.", "Exito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (HttpRequestException ex)
        {
            MessageBox.Show($"Error al eliminar el cliente: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnSearch_Click(object? sender, EventArgs e)
    {
        _currentPage = 1;
        await LoadDataAsync();
    }

    private async void BtnRefresh_Click(object? sender, EventArgs e)
    {
        _txtSearch.Clear();
        _currentPage = 1;
        await LoadDataAsync();
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        ClearInputs();
        _dgvCustomers.ClearSelection();
    }

    // ─── Pagination ────────────────────────────────────────────────

    private async void BtnFirst_Click(object? sender, EventArgs e)
    {
        _currentPage = 1;
        await LoadDataAsync();
    }

    private async void BtnPrev_Click(object? sender, EventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            await LoadDataAsync();
        }
    }

    private async void BtnNext_Click(object? sender, EventArgs e)
    {
        if (_currentPage < _totalPages)
        {
            _currentPage++;
            await LoadDataAsync();
        }
    }

    private async void BtnLast_Click(object? sender, EventArgs e)
    {
        _currentPage = _totalPages;
        await LoadDataAsync();
    }

    // ─── Helpers ───────────────────────────────────────────────────

    private bool ValidateInputs()
    {
        if (string.IsNullOrWhiteSpace(_txtNombre.Text))
        {
            MessageBox.Show("El campo Nombre es obligatorio.", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _txtNombre.Focus();
            return false;
        }

        if (_txtNombre.Text.Length > 100)
        {
            MessageBox.Show("El campo Nombre no puede superar 100 caracteres.", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (_txtDireccion.Text.Length > 200)
        {
            MessageBox.Show("El campo Direccion no puede superar 200 caracteres.", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_txtTelefono.Text))
        {
            if (_txtTelefono.Text.Length > 20)
            {
                MessageBox.Show("El campo Telefono no puede superar 20 caracteres.", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!Regex.IsMatch(_txtTelefono.Text, @"^\d+$"))
            {
                MessageBox.Show("El campo Telefono solo debe contener digitos.", "Validacion", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        return true;
    }

    private void ClearInputs()
    {
        _txtNombre.Clear();
        _txtDireccion.Clear();
        _txtTelefono.Clear();
    }

    private void UpdatePaginationControls()
    {
        _btnFirst.Enabled = _currentPage > 1;
        _btnPrev.Enabled = _currentPage > 1;
        _btnNext.Enabled = _currentPage < _totalPages;
        _btnLast.Enabled = _currentPage < _totalPages;
    }

    private void UpdatePageInfoLabel()
    {
        _lblPageInfo.Text = $"Pagina {_currentPage} de {_totalPages}";
    }
}
