import * as path from 'node:path';
import * as cdk from 'aws-cdk-lib';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as nodejsLambda from 'aws-cdk-lib/aws-lambda-nodejs';
import * as apigateway from 'aws-cdk-lib/aws-apigateway';
import * as logs from 'aws-cdk-lib/aws-logs';
import * as secretsmanager from 'aws-cdk-lib/aws-secretsmanager';
import { Construct } from 'constructs';

/**
 * API Gateway REST + Lambda para el CRUD de Clientes (HU1).
 *
 * Arquitectura hibrida: el computo corre en AWS (Lambda) pero la base de
 * datos es SQL Server on-premise. Las credenciales de conexion se guardan
 * en Secrets Manager (nunca hardcodeadas) y se inyectan en la Lambda.
 *
 * Nota de red: para que la Lambda alcance un SQL Server on-premise real,
 * la Lambda debe desplegarse en una VPC con conectividad hacia la red
 * corporativa (VPN Site-to-Site o Direct Connect). Aqui se deja el secreto
 * y las variables de entorno; el VPC/túnel se configura segun el entorno.
 */
export class ApiStack extends cdk.Stack {
  public readonly apiUrl: string;

  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // Secreto con las credenciales de SQL Server on-premise.
    // Rellenar el valor real tras el deploy (o via CLI) sin exponerlo en el codigo.
    const dbSecret = new secretsmanager.Secret(this, 'SqlServerSecret', {
      secretName: 'clientes/sqlserver',
      description: 'Credenciales de SQL Server on-premise para el modulo Clientes',
      secretObjectValue: {
        server: cdk.SecretValue.unsafePlainText('DESKTOP-0T7DSHL'),
        instance: cdk.SecretValue.unsafePlainText('SQLEXPRESS'),
        database: cdk.SecretValue.unsafePlainText('Kiro'),
        user: cdk.SecretValue.unsafePlainText('kiro'),
        password: cdk.SecretValue.unsafePlainText('CHANGE_ME'),
      },
    });

    const logGroup = new logs.LogGroup(this, 'CustomersFunctionLogs', {
      logGroupName: '/aws/lambda/ClientesHandler',
      retention: logs.RetentionDays.ONE_WEEK,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
    });

    const customersFn = new nodejsLambda.NodejsFunction(this, 'CustomersFunction', {
      functionName: 'ClientesHandler',
      entry: path.join(__dirname, '../../../backend/src/functions/customers/handler.ts'),
      handler: 'handler',
      runtime: lambda.Runtime.NODEJS_22_X,
      architecture: lambda.Architecture.ARM_64,
      timeout: cdk.Duration.seconds(30),
      memorySize: 512,
      tracing: lambda.Tracing.ACTIVE,
      logGroup,
      environment: {
        // Config de conexion a SQL Server on-premise.
        DB_SECRET_ARN: dbSecret.secretArn,
        DB_SERVER: 'DESKTOP-0T7DSHL',
        DB_INSTANCE: 'SQLEXPRESS',
        DB_NAME: 'Kiro',
        DB_USER: 'kiro',
        DB_ENCRYPT: 'false',
        DB_TRUST_CERT: 'true',
        NODE_OPTIONS: '--enable-source-maps',
      },
      bundling: {
        minify: true,
        sourceMap: true,
        target: 'node22',
        format: nodejsLambda.OutputFormat.ESM,
        // mssql usa modulos nativos/dinamicos; se externalizan para el bundle ESM.
        externalModules: [],
      },
    });

    // Least-privilege: la Lambda solo puede leer el secreto de la BD.
    dbSecret.grantRead(customersFn);

    const api = new apigateway.RestApi(this, 'ClientesApi', {
      restApiName: 'ClientesApi',
      description: 'API REST para el modulo de Clientes (HU1) - BD SQL Server on-premise',
      deployOptions: {
        stageName: 'prod',
        tracingEnabled: true,
        loggingLevel: apigateway.MethodLoggingLevel.INFO,
      },
      defaultCorsPreflightOptions: {
        allowOrigins: apigateway.Cors.ALL_ORIGINS,
        allowMethods: apigateway.Cors.ALL_METHODS,
        allowHeaders: ['Content-Type', 'Authorization'],
      },
    });

    const integration = new apigateway.LambdaIntegration(customersFn);

    // /customers  -> GET (list/search), POST (create)
    const customers = api.root.addResource('customers');
    customers.addMethod('GET', integration);
    customers.addMethod('POST', integration);

    // /customers/{id} -> GET, PUT, DELETE
    const customerById = customers.addResource('{id}');
    customerById.addMethod('GET', integration);
    customerById.addMethod('PUT', integration);
    customerById.addMethod('DELETE', integration);

    this.apiUrl = api.url;

    new cdk.CfnOutput(this, 'ApiUrl', { value: api.url });
    new cdk.CfnOutput(this, 'DbSecretName', { value: dbSecret.secretName });
  }
}
