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

	await CreateEntities(namespaceManager, "myqueue", "mytopic", "mysub").ConfigureAwait(false);
	
	#region Dispatching an event	
	
	var msg = new BrokeredMessage("A message");
	msg.Properties["region"] = "ab";
	
	var messagingFactory = MessagingFactory.CreateFromConnectionString(connectionString);
	var messageSender = await messagingFactory.CreateMessageSenderAsync("myqueue").ConfigureAwait(false);
	await messageSender.SendAsync(msg).ConfigureAwait(false);
	
	Debugger.Launch();
	
	var messageReceivier = await messagingFactory.CreateMessageReceiverAsync("myqueue", ReceiveMode.PeekLock).ConfigureAwait(false);
	
	var receivedMsg = await messageReceivier.ReceiveAsync();
	await receivedMsg.CompleteAsync();
	($"Received message with body `{receivedMsg.GetBody<string>()}` ({receivedMsg.Properties.Print()})").Dump();
	
	#endregion

	#region Dispatching an event
	
	var @event = new BrokeredMessage("An event");
	@event.Properties["country"] = "ca";

	messageSender = await messagingFactory.CreateMessageSenderAsync("mytopic").ConfigureAwait(false);
	await messageSender.SendAsync(@event).ConfigureAwait(false);

	Debugger.Launch();

	var subscriptionPath = SubscriptionClient.FormatSubscriptionPath("mytopic", "mysub");
	messageReceivier = await messagingFactory.CreateMessageReceiverAsync(subscriptionPath, ReceiveMode.PeekLock).ConfigureAwait(false);

	var receivedEvent = await messageReceivier.ReceiveAsync();
	await receivedEvent.CompleteAsync();
	($"Received event with body `{receivedEvent.GetBody<string>()}` ({receivedEvent.Properties.Print()})").Dump();
	
	#endregion
}

static async Task CreateEntities(NamespaceManager nsm, string queuePath, string topicPath, string subscriptionName)
{
	await EnsureQueueDoesNotExist(nsm, queuePath);
	await EnsureTopicDoesNotExist(nsm, topicPath);
	await nsm.CreateQueueAsync(new QueueDescription(queuePath)).ConfigureAwait(false);
	await nsm.CreateTopicAsync(new TopicDescription(topicPath)).ConfigureAwait(false);
	var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName);
	await nsm.CreateSubscriptionAsync(subscriptionDescription);
}

static async Task EnsureQueueDoesNotExist(NamespaceManager ns, string queuePath)
{
	if (await ns.QueueExistsAsync(queuePath).ConfigureAwait(false))
	{
		$"Queue `{queuePath}` was found".Dump();
		await ns.DeleteTopicAsync(queuePath);
		"and was deleted".Dump();
	}
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

public static class DictionaryExtensions
{
	public static string Print(this IDictionary<string, object> dic)
	{
		var sb = new StringBuilder();
		foreach (var element in dic)
		{
			sb.AppendFormat("{0}: {1} ", element.Key, element.Value);
		}
		return sb.ToString();
	}
}