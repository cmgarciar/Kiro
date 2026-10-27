import * as cdk from 'aws-cdk-lib';
import { __Entity__ApiStack } from '../lib/stacks/api-stack.js';

// Entry point CDK de referencia. OJO .gitignore: la regla amplia "bin/" puede
// excluir este archivo; reincluirlo con una negación si se versiona.
const app = new cdk.App();
new __Entity__ApiStack(app, '__Entity__ApiStack');
app.synth();
