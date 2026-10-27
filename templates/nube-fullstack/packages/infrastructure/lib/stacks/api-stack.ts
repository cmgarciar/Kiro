import * as cdk from 'aws-cdk-lib';
import type { Construct } from 'constructs';
import * as lambda from 'aws-cdk-lib/aws-lambda';
import * as nodejsLambda from 'aws-cdk-lib/aws-lambda-nodejs';
import * as apigateway from 'aws-cdk-lib/aws-apigateway';

// Stack de REFERENCIA. En el entorno local de Carlitos NO se sintetiza ni despliega
// (no hay cuenta ni credenciales AWS). Queda escrito para el día que se migre a la nube.
// Reemplazar __Entity__ / __entity__ al reutilizar.
export class __Entity__ApiStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const fn = new nodejsLambda.NodejsFunction(this, '__Entity__Function', {
      entry: '../backend/src/functions/__entity__/handler.ts',
      handler: 'handler',
      runtime: lambda.Runtime.NODEJS_20_X,
      timeout: cdk.Duration.seconds(30),
      memorySize: 512,
      environment: {
        DB_DRIVER: 'tedious',
        DB_SECRET_ARN: process.env.DB_SECRET_ARN ?? '',
      },
      bundling: { minify: true, sourceMap: true },
    });

    const api = new apigateway.RestApi(this, '__Entity__Api', {
      restApiName: '__Entity__Api',
      defaultCorsPreflightOptions: {
        allowOrigins: apigateway.Cors.ALL_ORIGINS,
        allowMethods: apigateway.Cors.ALL_METHODS,
      },
    });

    const resource = api.root.addResource('__entity__s');
    const integration = new apigateway.LambdaIntegration(fn);
    resource.addMethod('GET', integration);
    resource.addMethod('POST', integration);

    const byId = resource.addResource('{id}');
    byId.addMethod('GET', integration);
    byId.addMethod('PUT', integration);
    byId.addMethod('DELETE', integration);
  }
}
