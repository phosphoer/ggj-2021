# AWS CloudFormationKit scripts for GGJ 2021

This folder contains CDK scripts used to create and update the backend systems running on AWS.

This includes the following:
* A SimpleQueueServer(SQS) used for handling messages between games
* A REST ApiGateway uses to expose a REST style web interface to the SQS

# Initial AWS and CDK setup
1. Install Node.JS https://nodejs.org/dist/v14.15.4/node-v14.15.4-x64.msi
2. Install AWS command line https://awscli.amazonaws.com/AWSCLIV2.msi
3. Get IAM access key from your amazon account on the AWS console
4. Store the access key credentials
  > aws configure
5. Install AWS Cloud Development Kit 
  > npm install -g aws-cdk
6. Perform one-time initial CDK setup
  > cdk --version
  > cdk bootstrap

## Deploying a new build
1. Make sure the build compiles
  > npm run build
2. See what will change on AWS if you deply the build
  > cdk diff
3. Deploy the build
  > cdk deploy
  
Make note of the output URL after the deployment runs. 
You'll need to refer to this URL in SQSManager.cs for the game client to use the backend:

Outputs:
Ggj2021BackendStack.ApiGatewayToSqsPatternRestApiEndpoint259C4A68 = https://xbiih0vg3c.execute-api.us-west-2.amazonaws.com/prod/
