<Query Kind="Program">
  <NuGetReference>WindowsAzure.ServiceBus</NuGetReference>
  <Namespace>Microsoft.ServiceBus</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

void Main()
{
	MainAsync().GetAwaiter().GetResult();
}

static async Task MainAsync()
{
	var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString");
	var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);
	var queues = await namespaceManager.GetQueuesAsync().ConfigureAwait(false);

	$"found {queues.Count()} queues:".Dump();
	
	foreach (var queue in queues)
	{
		$"{queue.MessageCount} messages".Dump($"`{queue.Path}`", 1, true);
	}
}