using TUnit.Aspire;
using TUnit.Sample.Common.Constants;

namespace TUnit.Sample.AppHost.IntegrationTests;

public class AppFixture : AspireFixture<Projects.TUnit_Sample_AppHost>
{
    protected override ResourceWaitBehavior WaitBehavior => ResourceWaitBehavior.Named;
    protected override IEnumerable<string> ResourcesToWaitFor() => [ResourceConstants.WebApi, ResourceConstants.WebFrontend];
    
    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        builder.Services.ConfigureHttpClientDefaults(clientBuilder => {
            clientBuilder.AddStandardResilienceHandler();
        });
    }
}