namespace CountryService.Web.Services;

using CountryService.Web.Protos;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using System.Threading.Tasks;
using static CountryService.Web.Protos.CountryService;

public class CountryGrpcService : CountryServiceBase
{
    private readonly CountryManagementService countryManagementService;
    private readonly ILogger<CountryGrpcService> _logger;

    public CountryGrpcService(CountryManagementService countryManagementService, ILogger<CountryGrpcService> logger)
    {
        this.countryManagementService = countryManagementService;
        _logger = logger;
    }
    public override async Task GetAll(Empty request, IServerStreamWriter<CountryReply> responseStream,
        ServerCallContext context)
    {
        //////////// simulating an exception here ////////////
        throw new Exception("Something got really wrong here...");

        var countries = await countryManagementService.GetAllAsync();

        foreach (var country in countries)
        {
            await responseStream.WriteAsync(country);
        }
        await Task.CompletedTask;
    }

    public override async Task<CountryReply> Get(CountryIdRequest request, ServerCallContext context)
    {
        // Send a single country to the client in the gRPC response
        return await countryManagementService.GetAsync(request);
    }
    public override async Task<Empty> Delete(IAsyncStreamReader<CountryIdRequest> requestStream, 
        ServerCallContext context)
    {
        // Read and store all streamed input messages
        var countryIdRequestList = new List<CountryIdRequest>();
        await foreach (var countryIdRequest in requestStream.ReadAllAsync())
        {
            countryIdRequestList.Add(countryIdRequest);
        }
        // Delete in one shot all streamed countries
        await countryManagementService.DeleteAsync(countryIdRequestList);
        return new Empty();
    }
    public override async Task<Empty> Update(CountryUpdateRequest request, ServerCallContext context)
    {
        // read input message from the gRPC request
        await countryManagementService.UpdateAsync(request);
        return new Empty();
    }

    public override async Task Create(IAsyncStreamReader<CountryCreationRequest> requestStream, 
        IServerStreamWriter<CountryCreationReply> responseStream, ServerCallContext context)
    {
        // Read and store all streamed input messages before performing any action
        var countryCreationRequestList = new List<CountryCreationRequest>();
        await foreach (var countryCreationRequest in requestStream.ReadAllAsync())
        {
            countryCreationRequestList.Add(countryCreationRequest);
        }

        // Call in one shot the countryManagementService that will perform creation operations
        var createdCountries = await countryManagementService.CreateAsync(countryCreationRequestList);
        // Stream all created countries to the client
        foreach (var country in createdCountries)
        {
            await responseStream.WriteAsync(country);
        }
    }
}
