import * as path from 'node:path';
import * as fs from 'node:fs';
import * as cdk from 'aws-cdk-lib';
import * as s3 from 'aws-cdk-lib/aws-s3';
import * as s3deploy from 'aws-cdk-lib/aws-s3-deployment';
import * as cloudfront from 'aws-cdk-lib/aws-cloudfront';
import * as origins from 'aws-cdk-lib/aws-cloudfront-origins';
import { Construct } from 'constructs';

/**
 * S3 + CloudFront to host the React SPA (HU1).
 * Only deploys the built frontend if packages/frontend/dist exists.
 */
export class FrontendStack extends cdk.Stack {
  constructor(scope: Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    const hostingBucket = new s3.Bucket(this, 'HostingBucket', {
      blockPublicAccess: s3.BlockPublicAccess.BLOCK_ALL,
      encryption: s3.BucketEncryption.S3_MANAGED,
      removalPolicy: cdk.RemovalPolicy.DESTROY,
      autoDeleteObjects: true,
    });

    const distribution = new cloudfront.Distribution(this, 'Distribution', {
      defaultBehavior: {
        origin: origins.S3BucketOrigin.withOriginAccessControl(hostingBucket),
        viewerProtocolPolicy: cloudfront.ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
        cachePolicy: cloudfront.CachePolicy.CACHING_OPTIMIZED,
      },
      defaultRootObject: 'index.html',
      // SPA routing: serve index.html on 403/404.
      errorResponses: [
        { httpStatus: 403, responseHttpStatus: 200, responsePagePath: '/index.html' },
        { httpStatus: 404, responseHttpStatus: 200, responsePagePath: '/index.html' },
      ],
    });

    const distPath = path.join(__dirname, '../../../frontend/dist');
    if (fs.existsSync(distPath)) {
      new s3deploy.BucketDeployment(this, 'DeployFrontend', {
        sources: [s3deploy.Source.asset(distPath)],
        destinationBucket: hostingBucket,
        distribution,
        distributionPaths: ['/*'],
      });
    }

    new cdk.CfnOutput(this, 'DistributionDomain', {
      value: `https://${distribution.distributionDomainName}`,
    });
    new cdk.CfnOutput(this, 'HostingBucketName', { value: hostingBucket.bucketName });
  }
}
