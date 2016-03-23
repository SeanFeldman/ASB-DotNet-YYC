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

static bool enforceSubscriptions = false;

static async Task MainAsync()
{
	var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");
	var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

//	 enforceSubscriptions = true;

	await CreateEntities(namespaceManager, "mytopic").ConfigureAwait(false);
	
	var msg = new BrokeredMessage("An event");
	msg.Properties["country"] = "ca";
	
	var topicClient = TopicClient.CreateFromConnectionString(connectionString, "mytopic");
	await topicClient.SendAsync(msg).ConfigureAwait(false);
}

static async Task CreateEntities(NamespaceManager nsm, string topicPath)
{
	await EnsureTopicDoesNotExist(nsm, topicPath);
	await nsm.CreateTopicAsync(new TopicDescription(topicPath) { EnableFilteringMessagesBeforePublishing = enforceSubscriptions }).ConfigureAwait(false);
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