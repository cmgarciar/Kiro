import { useEffect, useState } from 'react';
import type { CustomerResponse, CreateCustomerRequest } from '../types/customer';

interface CustomerFormProps {
  initial?: CustomerResponse | null;
  isLoading?: boolean;
  onSubmit: (data: CreateCustomerRequest) => Promise<void>;
  onCancel: () => void;
}

interface FormErrors {
  name?: string;
  address?: string;
  phone?: string;
}

const NAME_MAX = 100;
const ADDRESS_MAX = 200;
const PHONE_MAX = 20;

const validate = (name: string, address: string, phone: string): FormErrors => {
  const errors: FormErrors = {};
  if (name.trim().length === 0) errors.name = 'El nombre es obligatorio.';
  else if (name.trim().length > NAME_MAX) errors.name = `Maximo ${NAME_MAX} caracteres.`;
  if (address.length > ADDRESS_MAX) errors.address = `Maximo ${ADDRESS_MAX} caracteres.`;
  if (phone.length > 0) {
    if (phone.length > PHONE_MAX) errors.phone = `Maximo ${PHONE_MAX} caracteres.`;
    else if (!/^[0-9]+$/.test(phone)) errors.phone = 'Solo se permiten numeros.';
  }
  return errors;
};

export function CustomerForm({ initial, isLoading = false, onSubmit, onCancel }: CustomerFormProps) {
  const [name, setName] = useState('');
  const [address, setAddress] = useState('');
  const [phone, setPhone] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});

  useEffect(() => {
    setName(initial?.name ?? '');
    setAddress(initial?.address ?? '');
    setPhone(initial?.phone ?? '');
    setErrors({});
  }, [initial]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    const found = validate(name, address, phone);
    setErrors(found);
    if (Object.keys(found).length > 0) return;
    await onSubmit({
      name: name.trim(),
      address: address.trim() || null,
      phone: phone.trim() || null,
    });
  };

  return (
    <form className="card form" onSubmit={handleSubmit} noValidate>
      <h2>{initial ? 'Editar cliente' : 'Nuevo cliente'}</h2>

      <div className="field">
        <label htmlFor="name">Nombre *</label>
        <input
          id="name"
          type="text"
          value={name}
          maxLength={NAME_MAX}
          onChange={(e) => setName(e.target.value)}
          aria-invalid={!!errors.name}
          aria-describedby={errors.name ? 'name-error' : undefined}
        />
        {errors.name && (
          <span id="name-error" className="error">
            {errors.name}
          </span>
        )}
      </div>

      <div className="field">
        <label htmlFor="address">Direccion</label>
        <input
          id="address"
          type="text"
          value={address}
          maxLength={ADDRESS_MAX}
          onChange={(e) => setAddress(e.target.value)}
          aria-invalid={!!errors.address}
          aria-describedby={errors.address ? 'address-error' : undefined}
        />
        {errors.address && (
          <span id="address-error" className="error">
            {errors.address}
          </span>
        )}
      </div>

      <div className="field">
        <label htmlFor="phone">Telefono</label>
        <input
          id="phone"
          type="text"
          inputMode="numeric"
          value={phone}
          maxLength={PHONE_MAX}
          onChange={(e) => setPhone(e.target.value.replace(/[^0-9]/g, ''))}
          aria-invalid={!!errors.phone}
          aria-describedby={errors.phone ? 'phone-error' : undefined}
        />
        {errors.phone && (
          <span id="phone-error" className="error">
            {errors.phone}
          </span>
        )}
      </div>

      <div className="actions">
        <button type="submit" className="btn primary" disabled={isLoading}>
          {initial ? 'Guardar cambios' : 'Crear cliente'}
        </button>
        <button type="button" className="btn" onClick={onCancel} disabled={isLoading}>
          Cancelar
        </button>
      </div>
    </form>
  );
}
