<Query Kind="Program">
  <NuGetReference>WindowsAzure.ServiceBus</NuGetReference>
  <Namespace>Microsoft.ServiceBus</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Microsoft.ServiceBus.Messaging</Namespace>
</Query>

void Main()
{
	MainAsync().GetAwaiter().GetResult();
}

static async Task MainAsync()
{
	var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");
	var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

	var topicDescription = new TopicDescription("mytopic")
	{
		MaxSizeInMegabytes = 2048
	};
	var subscriptionDescription = new SubscriptionDescription(topicDescription.Path, "mysub")
	{
		MaxDeliveryCount = 5
	};
	
	await EnsureTopicDoesNotExist(namespaceManager, topicDescription).ConfigureAwait(false);
	
	var createdTopic = await namespaceManager.CreateTopicAsync(topicDescription).ConfigureAwait(false);
	var createdSubscription = await namespaceManager.CreateSubscriptionAsync(topicDescription.Path, subscriptionDescription.Name).ConfigureAwait(false);

	$"Path: `{createdSubscription.TopicPath}`".Dump("Topic");
	($"Name: `{createdSubscription.Name}`" + Environment.NewLine +
	$"MaxDeliveryCount: {createdSubscription.MaxDeliveryCount}").Dump("Subscription");
}

static async Task EnsureTopicDoesNotExist(NamespaceManager ns, TopicDescription td)
{
	if (await ns.TopicExistsAsync(td.Path).ConfigureAwait(false))
	{
		$"Topic `{td.Path}` was found".Dump();
		await ns.DeleteQueueAsync(td.Path);
		"and was deleted".Dump();
	}
}