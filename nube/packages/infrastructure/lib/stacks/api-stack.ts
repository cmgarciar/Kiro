import * as path from 'node:path';
import * as cdk from 'aws-cdk-lib';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as nodejsLambda from 'aws-cdk-lib/aws-lambda-nodejs';
import * as apigateway from 'aws-cdk-lib/aws-apigateway';
import * as dynamodb from 'aws-cdk-lib/aws-dynamodb';
import * as logs from 'aws-cdk-lib/aws-logs';
import { Construct } from 'constructs';

export interface ApiStackProps extends cdk.StackProps {
  table: dynamodb.Table;
}

/**
 * API Gateway REST + single Lambda handling the Customers CRUD (HU1).
 */
export class ApiStack extends cdk.Stack {
  public readonly apiUrl: string;

  constructor(scope: Construct, id: string, props: ApiStackProps) {
    super(scope, id, props);

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
        TABLE_NAME: props.table.tableName,
        NODE_OPTIONS: '--enable-source-maps',
      },
      bundling: {
        minify: true,
        sourceMap: true,
        target: 'node22',
        format: nodejsLambda.OutputFormat.ESM,
      },
    });

    // Least-privilege: grant only read/write on the table (and its indexes).
    props.table.grantReadWriteData(customersFn);

    const api = new apigateway.RestApi(this, 'ClientesApi', {
      restApiName: 'ClientesApi',
      description: 'API REST para el modulo de Clientes (HU1)',
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
  }
}
