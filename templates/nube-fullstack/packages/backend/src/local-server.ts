import 'dotenv/config';
import express from 'express';
import cors from 'cors';
import { __Entity__Service } from './functions/__entity__/service.js';
import { NotFoundException, ValidationException } from './shared/errors/index.js';

// Runner local: expone el mismo service que usa la Lambda, vía Express.
// Permite probar el flujo end-to-end contra SQL Express sin API Gateway real.
const app = express();
app.use(cors());
app.use(express.json());

const service = new __Entity__Service();
const router = express.Router();

// Traduce las excepciones de dominio a códigos HTTP (igual que el handler Lambda).
const wrap =
  (fn: (req: express.Request, res: express.Response) => Promise<void>) =>
  async (req: express.Request, res: express.Response) => {
    try {
      await fn(req, res);
    } catch (error) {
      if (error instanceof NotFoundException) return void res.status(404).json({ error: error.message });
      if (error instanceof ValidationException) return void res.status(400).json({ error: error.message });
      console.error('Error inesperado:', error);
      res.status(500).json({ error: 'Error interno del servidor.' });
    }
  };

router.get('/', wrap(async (req, res) => {
  const pagina = Number(req.query.pagina ?? 1);
  const tamPagina = Number(req.query.tamPagina ?? 10);
  const busqueda = (req.query.busqueda as string | undefined) ?? undefined;
  res.json(await service.list(pagina, tamPagina, busqueda));
}));

router.get('/:id', wrap(async (req, res) => {
  res.json(await service.getById(Number(req.params.id)));
}));

router.post('/', wrap(async (req, res) => {
  res.status(201).json(await service.create(req.body));
}));

router.put('/:id', wrap(async (req, res) => {
  res.json(await service.update(Number(req.params.id), req.body));
}));

router.delete('/:id', wrap(async (req, res) => {
  await service.remove(Number(req.params.id));
  res.status(204).send();
}));

// Reemplazar "__entity__s" por la ruta en plural de la entidad (ej: /clientes).
app.use('/__entity__s', router);

const port = Number(process.env.PORT ?? 3000);
app.listen(port, () => {
  console.log(`API local escuchando en http://localhost:${port}`);
});
