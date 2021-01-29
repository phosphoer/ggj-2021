import * as cdk from '@aws-cdk/core';
import * as lambda from '@aws-cdk/aws-lambda';
import * as api from '@aws-cdk/aws-apigateway';
import * as sqs from '@aws-cdk/aws-sqs';
import { ApiGatewayToSqs, ApiGatewayToSqsProps } from '@aws-solutions-constructs/aws-apigateway-sqs';

export class Ggj2021BackendStack extends cdk.Stack {
  constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
    super(scope, id, props);

    // Create a SimpleQueueingService wrapped in a Rest API gateway
    const api_sqs_props: ApiGatewayToSqsProps = {
      apiGatewayProps: {
        apiKeySourceType: api.ApiKeySourceType.HEADER,
        defaultMethodOptions: {
          apiKeyRequired: true,
          authorizationType: api.AuthorizationType.NONE,
        },
        restApiName: "biscuits-ggj-2021"
      },
      queueProps: {
        maxMessageSizeBytes: 262144,
        retentionPeriod: cdk.Duration.minutes(10),
        visibilityTimeout: cdk.Duration.seconds(0),
      },
      allowCreateOperation: true,
      allowReadOperation: true,
      allowDeleteOperation: true,
      deployDeadLetterQueue: false
    };

    // Create an API key so that only the biscuits-ggj-2021 can call this api
    const apiGatewayToSqs= new ApiGatewayToSqs(this, 'ApiGatewayToSqsPattern', api_sqs_props);
    const biscuitsRestAPI= apiGatewayToSqs.apiGateway;
    const apiKeyName = "biscuits-ggj-2021-key"
    const apiKey = new api.ApiKey(this, `BiscuitsGGJ2021Key`, {
        apiKeyName,
        description: `APIKey used by biscuits-ggj-2021 game`,
        enabled: true,
        value: '9bUJSVyao69ysDMW3Bqu16QliiHV7A9U9Iz9Kexz'
    });
    const usagePlanProps: api.UsagePlanProps = {
      name: "biscuits-ggl-2021-usage-plan",
      apiKey,
      apiStages: [{api: biscuitsRestAPI, stage: biscuitsRestAPI.deploymentStage}],
      throttle: {burstLimit: 500, rateLimit: 1000}, quota: {limit: 10000, period: api.Period.MONTH}
    }                
    biscuitsRestAPI.addUsagePlan("biscuits-ggl-2021-usage-plan", usagePlanProps);
  }
}
