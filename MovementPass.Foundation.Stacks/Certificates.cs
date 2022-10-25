namespace MovementPass.Foundation.Stacks;

using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Route53;

public class Certificates : BaseStack
{
    public Certificates(
        Construct scope,
        string id,
        IStackProps props = null) : base(scope, id, props)
    {
        var zone = HostedZone.FromLookup(
            this,
            "Zone",
            new HostedZoneProviderProps { DomainName = this.Domain });

        var clientCertificate = new DnsValidatedCertificate(
            this,
            "ClientCertificate",
            new DnsValidatedCertificateProps
            {
                DomainName = this.Domain,
                SubjectAlternativeNames = new[] { $"*.{this.Domain}" },
                Region = "us-east-1",
                HostedZone = zone
            });

        this.PutParameterStoreValue(
            "clientCertificateArn",
            clientCertificate.CertificateArn);

        // ReSharper disable once VirtualMemberCallInConstructor
        if (this.Region == "us-east-1")
        {
            this.PutParameterStoreValue(
                "serverCertificateArn",
                clientCertificate.CertificateArn);
        }
        else
        {
            var serverCertificate = new DnsValidatedCertificate(
                this,
                "ServerCertificate",
                new DnsValidatedCertificateProps
                {
                    DomainName = this.Domain,
                    SubjectAlternativeNames = new[] { $"*.{this.Domain}" },
                    HostedZone = zone
                });

            this.PutParameterStoreValue("serverCertificateArn",
                serverCertificate.CertificateArn);
        }
    }
}