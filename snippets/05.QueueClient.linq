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

	await CreateEntities(namespaceManager, "myqueue").ConfigureAwait(false);
	
	var msg = new BrokeredMessage("First message");
}

static async Task CreateEntities(NamespaceManager nsm, string queuePath)
{
	await EnsureQueueDoesNotExist(nsm, new QueueDescription(queuePath));
	await nsm.CreateQueueAsync(queuePath).ConfigureAwait(false);
}

static async Task EnsureQueueDoesNotExist(NamespaceManager ns, QueueDescription qd)
{
	if (await ns.QueueExistsAsync(qd.Path).ConfigureAwait(false))
	{
		$"Queue `{qd.Path}` was found".Dump();
		await ns.DeleteQueueAsync(qd.Path);
		"and was deleted".Dump();
	}
}