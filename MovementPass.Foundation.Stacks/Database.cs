namespace MovementPass.Foundation.Stacks;

using System;

using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;

public class Database : BaseStack
{
    public Database(Construct scope, string id, IStackProps props = null)
        : base(scope, id, props)
    {
        this.CreateTable(
            "applicants",
            CreateAttribute("id"),
            StreamViewType.NEW_IMAGE
        );

        this.CreateTable("passes",
            CreateAttribute("id"),
            StreamViewType.NEW_IMAGE,
            table =>
            {
                table.AddGlobalSecondaryIndex(
                    new GlobalSecondaryIndexProps
                    {
                        IndexName = "ix_applicantId-endAt",
                        PartitionKey = CreateAttribute("applicantId"),
                        SortKey = CreateAttribute("endAt")
                    });

                table.AddGlobalSecondaryIndex(
                    new GlobalSecondaryIndexProps
                    {
                        IndexName = "ix_applicantId-status",
                        PartitionKey = CreateAttribute("applicantId"),
                        SortKey = CreateAttribute("status")
                    });
            });
    }

    private static IAttribute CreateAttribute(string name) =>
        new Amazon.CDK.AWS.DynamoDB.Attribute
        {
            Name = name,
            Type = AttributeType.STRING
        };

    private void CreateTable(
        string name,
        IAttribute partitionKey,
        StreamViewType stream,
        Action<Table> configure = null)
    {
        var table = new Table(this, $"{name}Table", new TableProps
        {
            TableName = $"{this.App}_{name}_{this.Version}",
            PartitionKey = partitionKey,
            Stream = stream,
            BillingMode = BillingMode.PAY_PER_REQUEST,
            RemovalPolicy = RemovalPolicy.RETAIN,
            PointInTimeRecovery = true
        });

        configure?.Invoke(table);

        this.PutParameterStoreValue(
            $"dynamodbTables/{name}",
            table.TableName);

        this.PutParameterStoreValue(
            $"dynamodbTables/{name}StreamArn",
            table.TableStreamArn);
    }
}