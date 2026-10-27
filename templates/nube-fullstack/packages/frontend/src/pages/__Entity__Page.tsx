import { useEffect, useState } from 'react';
import { use__Entity__Store } from '../store/__entity__Store.js';
import { PlusIcon, SearchIcon, EditIcon, DeleteIcon } from '../components/icons.js';
import type { __Entity__ } from '../types/__entity__.js';

// Página CRUD de __Entity__ con el estándar de diseño navy.
// Reemplazar __Entity__ / __entity__ y ajustar columnas según la entidad real.
export const __Entity__Page = () => {
  const {
    items, total, pagina, tamPagina, busqueda, isLoading, error,
    fetch, setBusqueda, setPagina, create, update, remove,
  } = use__Entity__Store();

  const [editing, setEditing] = useState<__Entity__ | null>(null);
  const [nombre, setNombre] = useState('');
  const [descripcion, setDescripcion] = useState('');

  useEffect(() => { void fetch(); }, [pagina]);

  const totalPaginas = Math.max(1, Math.ceil(total / tamPagina));

  const openCreate = () => { setEditing(null); setNombre(''); setDescripcion(''); };
  const openEdit = (item: __Entity__) => {
    setEditing(item);
    setNombre(item.nombre);
    setDescripcion(item.descripcion ?? '');
  };

  const submit = async () => {
    const data = { nombre, descripcion: descripcion || null };
    if (editing) await update(editing.id, data);
    else await create(data);
    openCreate();
  };

  return (
    <div className="container">
      <header className="page-header">
        <h1>Gestión de __Entity__s</h1>
      </header>

      <div className="toolbar">
        <input
          className="search"
          placeholder="Buscar..."
          aria-label="Buscar __entity__s"
          value={busqueda}
          onChange={(e) => setBusqueda(e.target.value)}
          onKeyDown={(e) => { if (e.key === 'Enter') void fetch(); }}
        />
        <button className="btn navy" onClick={() => void fetch()}>
          <SearchIcon /> Buscar
        </button>
        <button className="btn navy" onClick={openCreate}>
          <PlusIcon /> Nuevo
        </button>
      </div>

      <div className="card">
        {/* Formulario simple de alta/edición */}
        <div style={{ display: 'flex', gap: '0.75rem', padding: '1rem', flexWrap: 'wrap' }}>
          <input
            className="search"
            placeholder="Nombre"
            aria-label="Nombre"
            value={nombre}
            onChange={(e) => setNombre(e.target.value)}
          />
          <input
            className="search"
            placeholder="Descripción"
            aria-label="Descripción"
            value={descripcion}
            onChange={(e) => setDescripcion(e.target.value)}
          />
          <button className="btn navy" disabled={!nombre.trim()} onClick={() => void submit()}>
            {editing ? <><EditIcon /> Guardar</> : <><PlusIcon /> Agregar</>}
          </button>
        </div>

        {error && <div className="state" style={{ color: 'var(--danger)' }}>{error}</div>}
        {isLoading ? (
          <div className="state">Cargando...</div>
        ) : items.length === 0 ? (
          <div className="state">No hay registros.</div>
        ) : (
          <table className="table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Descripción</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {items.map((item) => (
                <tr key={item.id}>
                  <td>{item.nombre}</td>
                  <td>{item.descripcion ?? '—'}</td>
                  <td>
                    <div className="row-actions">
                      <button className="btn action" onClick={() => openEdit(item)}>
                        <EditIcon /> Editar
                      </button>
                      <button className="btn danger" onClick={() => void remove(item.id)}>
                        <DeleteIcon /> Eliminar
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}

        <div className="pagination">
          <button className="btn action" disabled={pagina <= 1} onClick={() => setPagina(pagina - 1)}>
            Anterior
          </button>
          <span className="info">Página {pagina} de {totalPaginas} ({total} registros)</span>
          <button
            className="btn action"
            disabled={pagina >= totalPaginas}
            onClick={() => setPagina(pagina + 1)}
          >
            Siguiente
          </button>
        </div>
      </div>
    </div>
  );
};
