using TUnit.Aspire;
using TUnit.Sample.Common.Constants;

namespace TUnit.Sample.AppHost.IntegrationTests;

public class AppFixture : AspireFixture<Projects.TUnit_Sample_AppHost>
{
    protected override TimeSpan ResourceTimeout => TimeSpan.FromMinutes(4);
    protected override ResourceWaitBehavior WaitBehavior => ResourceWaitBehavior.Named;
    protected override IEnumerable<string> ResourcesToWaitFor() => [ResourceConstants.WebApi];
    
    protected override void ConfigureBuilder(IDistributedApplicationTestingBuilder builder)
    {
        builder.Services.ConfigureHttpClientDefaults(clientBuilder => {
            clientBuilder.AddStandardResilienceHandler();
        });
    }
}