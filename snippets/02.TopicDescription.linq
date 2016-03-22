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
	
	await EnsureTopicDoesNotExist(namespaceManager, topicDescription).ConfigureAwait(false);
	
	var createdTopic = await namespaceManager.CreateTopicAsync(topicDescription).ConfigureAwait(false);

	($"Path: `{createdTopic.Path}`" + Environment.NewLine +
	$"MaxSizeInMegabytes: {createdTopic.MaxSizeInMegabytes}").Dump("Topic");
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