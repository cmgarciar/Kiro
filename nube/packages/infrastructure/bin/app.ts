#!/usr/bin/env node
import * as cdk from 'aws-cdk-lib';
import { ApiStack } from '../lib/stacks/api-stack';
import { FrontendStack } from '../lib/stacks/frontend-stack';

const app = new cdk.App();

const env = {
  account: process.env.CDK_DEFAULT_ACCOUNT,
  region: process.env.CDK_DEFAULT_REGION ?? 'us-east-1',
};

const tags = { Project: 'Clientes', Module: 'HU1', Environment: 'dev' };

// Persistencia on-premise (SQL Server). No hay StorageStack: la Lambda
// se conecta a la base de datos Kiro en tierra via el secreto configurado.
new ApiStack(app, 'ClientesApiStack', { env, tags });

new FrontendStack(app, 'ClientesFrontendStack', { env, tags });
