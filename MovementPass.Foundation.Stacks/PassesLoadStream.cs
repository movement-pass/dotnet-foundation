namespace MovementPass.Foundation.Stacks;

using Amazon.CDK;
using Amazon.CDK.AWS.Kinesis;

public sealed class PassesLoadStream : BaseStack
{
    public PassesLoadStream (
        Construct scope,
        string id,
        IStackProps props = null) : base(scope, id, props) =>
        // ReSharper disable once ObjectCreationAsStatement
        new Stream(this, "Stream",
            new StreamProps {
                StreamName = $"{this.App}_passes-load_{this.Version}",
                RetentionPeriod = Duration.Hours(24)
            });
}