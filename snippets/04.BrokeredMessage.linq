<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <NuGetReference>WindowsAzure.ServiceBus</NuGetReference>
  <Namespace>Microsoft.ServiceBus</Namespace>
  <Namespace>Microsoft.ServiceBus.Messaging</Namespace>
  <Namespace>System.Runtime.Serialization</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

void Main()
{
	MainAsync().GetAwaiter().GetResult();
}

static async Task MainAsync()
{
	// message as a string
	var msg = new BrokeredMessage("First message");
	msg.GetBody<string>().Dump("Message body", 1, true);

	// message as a class
	msg = new BrokeredMessage(new CustomMessage { Id = Guid.NewGuid(), Name = "Test" });
	msg.GetBody<CustomMessage>().Dump("DataContruct message body", 1, true);

	// message as a poco
	var poco = new PocoMessage { Id = Guid.NewGuid(), Name = "Test" };
	msg = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(poco))));
	var stream = msg.GetBody<Stream>();
	var str = await new StreamReader(stream, true).ReadToEndAsync().ConfigureAwait(false);
	JsonConvert.DeserializeObject<PocoMessage>(str).Dump();
}

[DataContract]
class CustomMessage
{
	[DataMember]
	public Guid Id { get; set; }
	[DataMember]
	public string Name { get; set; }
}

class PocoMessage
{
	public Guid Id { get; set; }
	public string Name { get; set; }
}