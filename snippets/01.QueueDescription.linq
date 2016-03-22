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

	var queueDescription = new QueueDescription("myqueue")
	{
		DefaultMessageTimeToLive = TimeSpan.FromSeconds(20)
	};
	
	await EnsureQueueDoesNotExist(namespaceManager, queueDescription).ConfigureAwait(false);
	
	var createdQueue = await namespaceManager.CreateQueueAsync(queueDescription).ConfigureAwait(false);

	($"Path: `{createdQueue.Path}`" + Environment.NewLine + 
	$"DefaultMessageTimeToLive: {createdQueue.DefaultMessageTimeToLive}").Dump("Queue");
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