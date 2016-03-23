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

	await CreateEntities(namespaceManager, "mytopic", "mysub").ConfigureAwait(false);
	
	var msg = new BrokeredMessage("An event");
	msg.Properties["country"] = "ca";
	
	var topicClient = TopicClient.CreateFromConnectionString(connectionString, "mytopic");
	await topicClient.SendAsync(msg).ConfigureAwait(false);
	
	Debugger.Launch();
	
	var subscriptionClient = SubscriptionClient.CreateFromConnectionString(connectionString, "mytopic", "mysub", ReceiveMode.PeekLock);
	var receivedEvent = await subscriptionClient.ReceiveAsync();
	await receivedEvent.CompleteAsync();
	($"Received event with body `{receivedEvent.GetBody<string>()}`").Dump();
}

static async Task CreateEntities(NamespaceManager nsm, string topicPath, string subscriptionName)
{
	await EnsureTopicDoesNotExist(nsm, topicPath);
	await nsm.CreateTopicAsync(new TopicDescription(topicPath)).ConfigureAwait(false);
	var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName);
	await nsm.CreateSubscriptionAsync(subscriptionDescription);
}

static async Task EnsureTopicDoesNotExist(NamespaceManager ns, string topicPath)
{
	if (await ns.TopicExistsAsync(topicPath).ConfigureAwait(false))
	{
		$"Topic `{topicPath}` was found".Dump();
		await ns.DeleteTopicAsync(topicPath);
		"and was deleted".Dump();
	}
}