// See https://aka.ms/new-console-template for more information

using Grpc.Net.Client;
using GrpcDemo.Protos;

var clientOptions = new GrpcChannelOptions();

using var channel = GrpcChannel.ForAddress("https://localhost:7222", clientOptions);

var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);