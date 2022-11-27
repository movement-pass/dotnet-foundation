namespace MovementPass.Foundation.Stacks;

using Constructs;
using Amazon.CDK;

public class Configuration : BaseStack
{
    public Configuration(
        Construct scope,
        string id,
        IStackProps props = null) : base(scope, id, props)
    {
        this.PutParameterStoreValue("app", this.App);
        this.PutParameterStoreValue("version", this.Version);
        this.PutParameterStoreValue("domain", this.Domain);
    }
}