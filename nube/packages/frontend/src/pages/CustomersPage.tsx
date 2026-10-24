import { useEffect, useState } from 'react';
import { useCustomerStore } from '../store/customerStore';
import { CustomerForm } from '../components/CustomerForm';
import type { CustomerResponse, CreateCustomerRequest } from '../types/customer';

const formatDate = (iso: string | null): string => {
  if (!iso) return '-';
  const d = new Date(iso);
  return Number.isNaN(d.getTime()) ? '-' : d.toLocaleString('es');
};

export function CustomersPage() {
  const {
    customers,
    search,
    isLoading,
    error,
    page,
    totalPages,
    total,
    setSearch,
    fetchFirstPage,
    nextPage,
    prevPage,
    createCustomer,
    updateCustomer,
    deleteCustomer,
  } = useCustomerStore();

  const [showForm, setShowForm] = useState(false);
  const [editing, setEditing] = useState<CustomerResponse | null>(null);
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    void fetchFirstPage();
  }, [fetchFirstPage]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    void fetchFirstPage();
  };

  const openCreate = () => {
    setEditing(null);
    setFormError(null);
    setShowForm(true);
  };

  const openEdit = (customer: CustomerResponse) => {
    setEditing(customer);
    setFormError(null);
    setShowForm(true);
  };

  const handleSubmit = async (data: CreateCustomerRequest) => {
    setFormError(null);
    try {
      if (editing) {
        await updateCustomer(editing.id, data);
      } else {
        await createCustomer(data);
      }
      setShowForm(false);
      setEditing(null);
    } catch (err) {
      setFormError(err instanceof Error ? err.message : 'Error al guardar.');
    }
  };

  const handleDelete = async (customer: CustomerResponse) => {
    if (!window.confirm(`Eliminar al cliente "${customer.name}"?`)) return;
    try {
      await deleteCustomer(customer.id);
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Error al eliminar.');
    }
  };

  return (
    <div className="page">
      <header className="page-header">
        <h1>Gestion de Clientes</h1>
        <button className="btn primary" onClick={openCreate}>
          + Nuevo cliente
        </button>
      </header>

      <form className="toolbar" onSubmit={handleSearch} role="search">
        <input
          type="search"
          placeholder="Buscar por nombre o telefono"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          aria-label="Buscar clientes"
        />
        <button type="submit" className="btn">
          Buscar
        </button>
      </form>

      {error && <div className="alert error-alert">{error}</div>}

      <div className="card">
        <table className="table">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Direccion</th>
              <th>Telefono</th>
              <th>Creado</th>
              <th>Modificado</th>
              <th aria-label="Acciones"></th>
            </tr>
          </thead>
          <tbody>
            {isLoading && (
              <tr>
                <td colSpan={6} className="muted">
                  Cargando...
                </td>
              </tr>
            )}
            {!isLoading && customers.length === 0 && (
              <tr>
                <td colSpan={6} className="muted">
                  No hay clientes para mostrar.
                </td>
              </tr>
            )}
            {!isLoading &&
              customers.map((c) => (
                <tr key={c.id}>
                  <td>{c.name}</td>
                  <td>{c.address ?? '-'}</td>
                  <td>{c.phone ?? '-'}</td>
                  <td>{formatDate(c.createdAt)}</td>
                  <td>{formatDate(c.updatedAt)}</td>
                  <td className="row-actions">
                    <button className="btn small" onClick={() => openEdit(c)}>
                      Editar
                    </button>
                    <button className="btn small danger" onClick={() => handleDelete(c)}>
                      Eliminar
                    </button>
                  </td>
                </tr>
              ))}
          </tbody>
        </table>

        <div className="pagination">
          <button className="btn" onClick={() => void prevPage()} disabled={page <= 1 || isLoading}>
            Anterior
          </button>
          <span className="muted">
            Pagina {page} de {totalPages} ({total} clientes)
          </span>
          <button
            className="btn"
            onClick={() => void nextPage()}
            disabled={page >= totalPages || isLoading}
          >
            Siguiente
          </button>
        </div>
      </div>

      {showForm && (
        <div className="modal-backdrop" onClick={() => setShowForm(false)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            {formError && <div className="alert error-alert">{formError}</div>}
            <CustomerForm
              initial={editing}
              isLoading={isLoading}
              onSubmit={handleSubmit}
              onCancel={() => setShowForm(false)}
            />
          </div>
        </div>
      )}
    </div>
  );
}
