namespace MovementPass.Foundation.Stacks
{
    using System;
    using System.Globalization;

    using Amazon.CDK;
    using Amazon.CDK.AWS.CertificateManager;
    using Amazon.CDK.AWS.CloudFront;
    using Amazon.CDK.AWS.Route53;
    using Amazon.CDK.AWS.Route53.Targets;
    using Amazon.CDK.AWS.S3;

    public class Photos : BaseStack
    {
        public Photos(
            Construct scope,
            string id,
            IStackProps props = null) : base(scope, id, props)
        {
            var bucketNamePrefix =
                this.GetContextValue<string>("photoBucketNamePrefix");

            var subDomain = $"{bucketNamePrefix}.{this.Domain}";
            var expiration = TimeSpan.Parse(
                this.GetContextValue<string>(
                    "photoUploadExpiration"),
                CultureInfo.InvariantCulture);

            var bucket = new Bucket(
                this,
                "Bucket",
                new BucketProps
                {
                    // ReSharper disable once VirtualMemberCallInConstructor
                    BucketName = $"{bucketNamePrefix}.{this.Region}.{this.Domain}",
                    BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                    RemovalPolicy = RemovalPolicy.RETAIN,
                    Cors = new ICorsRule[]
                    {
                        new CorsRule
                        {
                            AllowedHeaders = new[] { "*" },
                            AllowedOrigins = new[] { "*" },
                            AllowedMethods = new[] { HttpMethods.PUT },
                            MaxAge = expiration.TotalSeconds
                        }
                    }
                });

            this.PutParameterStoreValue(
                "photoBucket/name",
                bucket.BucketName);

            this.PutParameterStoreValue(
                "photoBucket/uploadExpiration",
                expiration.ToString());

            var certificateArn =
                this.GetParameterStoreValue("clientCertificateArn");

            var certificate =
                Certificate.FromCertificateArn(
                    this,
                    "CertificateArn",
                    certificateArn);

            var accessIdentity = new OriginAccessIdentity(
                this,
                "AccessIdentity",
                new OriginAccessIdentityProps
                {
                    Comment = $"{this.App}-{subDomain}-identity"
                });

            var distribution = new CloudFrontWebDistribution(
                this,
                "Distribution",
                new CloudFrontWebDistributionProps
                {
                    PriceClass = PriceClass.PRICE_CLASS_ALL,
                    ViewerProtocolPolicy =
                        ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                    ViewerCertificate = ViewerCertificate.FromAcmCertificate(
                        certificate,
                        new ViewerCertificateOptions
                        {
                            Aliases = new[] { subDomain },
                            SslMethod = SSLMethod.SNI,
                            SecurityPolicy =
                                SecurityPolicyProtocol.TLS_V1_2_2021
                        }),
                    OriginConfigs =
                        new ISourceConfiguration[]
                        {
                            new SourceConfiguration
                            {
                                S3OriginSource =
                                    new S3OriginConfig
                                    {
                                        S3BucketSource = bucket,
                                        OriginAccessIdentity = accessIdentity
                                    },
                                Behaviors = new IBehavior[]
                                {
                                    new Behavior
                                    {
                                        IsDefaultBehavior = true,
                                        ForwardedValues =
                                            new CfnDistribution.
                                                ForwardedValuesProperty
                                                {
                                                    QueryString = true
                                                }
                                    }
                                }
                            }
                        },
                    ErrorConfigurations =
                        new CfnDistribution.ICustomErrorResponseProperty[]
                        {
                            new CfnDistribution.CustomErrorResponseProperty
                            {
                                ErrorCode = 404,
                                ResponseCode = 200,
                                ResponsePagePath = "/index.html",
                            }
                        }
                });

            // ReSharper disable once ObjectCreationAsStatement
            new ARecord(this, "Mount", new ARecordProps
            {
                RecordName = subDomain,
                Target =
                    RecordTarget.FromAlias(new CloudFrontTarget(distribution)),
                Zone = HostedZone.FromLookup(
                    this,
                    "Zone",
                    new HostedZoneProviderProps
                    {
                        DomainName = this.Domain
                    })
            });
        }
    }
}