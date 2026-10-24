import { ValidationException } from '../errors/index.js';
import type { CreateCustomerRequest, UpdateCustomerRequest } from '../types/customer.js';

const NAME_MAX = 100;
const ADDRESS_MAX = 200;
const PHONE_MAX = 20;
const PHONE_REGEX = /^[0-9]+$/;

/** Normalize optional string: trims and converts empty string to null. */
const normalizeOptional = (value: string | null | undefined): string | null => {
  if (value === undefined || value === null) return null;
  const trimmed = value.trim();
  return trimmed.length === 0 ? null : trimmed;
};

const validateName = (name: unknown): string => {
  if (typeof name !== 'string' || name.trim().length === 0) {
    // CA3: name is required.
    throw new ValidationException('El campo Nombre es obligatorio.');
  }
  const trimmed = name.trim();
  if (trimmed.length > NAME_MAX) {
    // CA3: max 100 chars.
    throw new ValidationException(`El campo Nombre no puede superar ${NAME_MAX} caracteres.`);
  }
  return trimmed;
};

const validateAddress = (address: string | null): string | null => {
  if (address !== null && address.length > ADDRESS_MAX) {
    // CA4: max 200 chars.
    throw new ValidationException(`El campo Direccion no puede superar ${ADDRESS_MAX} caracteres.`);
  }
  return address;
};

const validatePhone = (phone: string | null): string | null => {
  if (phone === null) return null;
  if (phone.length > PHONE_MAX) {
    // CA5: max 20 chars.
    throw new ValidationException(`El campo Telefono no puede superar ${PHONE_MAX} caracteres.`);
  }
  if (!PHONE_REGEX.test(phone)) {
    // CA5: digits only.
    throw new ValidationException('El campo Telefono debe contener solo numeros.');
  }
  return phone;
};

export interface ValidatedCreate {
  name: string;
  address: string | null;
  phone: string | null;
}

export const validateCreate = (body: CreateCustomerRequest): ValidatedCreate => {
  return {
    name: validateName(body.name),
    address: validateAddress(normalizeOptional(body.address)),
    phone: validatePhone(normalizeOptional(body.phone)),
  };
};

export interface ValidatedUpdate {
  name?: string;
  address?: string | null;
  phone?: string | null;
}

export const validateUpdate = (body: UpdateCustomerRequest): ValidatedUpdate => {
  const result: ValidatedUpdate = {};
  // CA7: only name, address and phone can be edited (id is never editable - CA2).
  if (body.name !== undefined) {
    result.name = validateName(body.name);
  }
  if (body.address !== undefined) {
    result.address = validateAddress(normalizeOptional(body.address));
  }
  if (body.phone !== undefined) {
    result.phone = validatePhone(normalizeOptional(body.phone));
  }
  if (Object.keys(result).length === 0) {
    throw new ValidationException('No se proporcionaron campos para actualizar.');
  }
  return result;
};
