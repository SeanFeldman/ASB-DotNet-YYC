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

	var messagingFactory = MessagingFactory.CreateFromConnectionString(connectionString);

	var messageReceivier = await messagingFactory.CreateMessageReceiverAsync("myqueue", ReceiveMode.PeekLock).ConfigureAwait(false);

	// message pump options
	var onMessageOptions = new OnMessageOptions
	{
		AutoComplete = false,
		AutoRenewTimeout = TimeSpan.FromMinutes(2),
		MaxConcurrentCalls = 5
	};

	// exception handler
	onMessageOptions.ExceptionReceived += (sender, exceptionReceivedEventArgs) => { exceptionReceivedEventArgs.Exception.Dump("Exception in pump.", 1, true); };

	// message pump
	messageReceivier.OnMessageAsync(async message =>
	{
		($"Received: {message.GetBody<string>()}").Dump();

		await message.CompleteAsync().ConfigureAwait(false);
	}, onMessageOptions);


	// send messages
	var messageSender = await messagingFactory.CreateMessageSenderAsync("myqueue").ConfigureAwait(false);
	var batch = new List<BrokeredMessage>();
	var runningCount = 1;
	while (runningCount < 50)
	{
		for (int i = 0; i < 10; i++)
		{
			batch.Add(new BrokeredMessage("Message #" + runningCount++));
		}
		await messageSender.SendBatchAsync(batch).ConfigureAwait(false);
		$"Batch #{runningCount / 10} sent".Dump();
		batch.Clear();
	}

	Util.ReadLine();
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