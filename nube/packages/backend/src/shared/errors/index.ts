export class NotFoundException extends Error {
  constructor(resource: string, id: string | number) {
    super(`${resource} with id ${id} not found`);
    this.name = 'NotFoundException';
  }
}

export class ValidationException extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'ValidationException';
  }
}

export class DatabaseException extends Error {
  constructor(message: string, public readonly cause?: unknown) {
    super(message);
    this.name = 'DatabaseException';
  }
}
