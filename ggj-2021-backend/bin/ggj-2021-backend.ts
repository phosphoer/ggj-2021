#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from '@aws-cdk/core';
import { Ggj2021BackendStack } from '../lib/ggj-2021-backend-stack';

const app = new cdk.App();
new Ggj2021BackendStack(app, 'Ggj2021BackendStack');
