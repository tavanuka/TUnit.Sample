using Bunit;
using TUnit.Sample.Web.Components;

namespace TUnit.Sample.Web.Tests;

public class TitleBoxTests : BunitContext
{
    [Test]
    public async Task TitleBox_ShouldRender_Title()
    {
        // language=HTML
        const string expectedMarkup = """
                                      <div>
                                         <h1 diff:ignoreChildren></h1>
                                      </div>
                                      """;

        var cut = Render<TitleBox>(p => p
            .Add(c => c.Title, "Test Title")
        );

        await Assert.That(cut.Find("h1").TextContent).IsEqualTo("Test Title");
        cut.MarkupMatches(expectedMarkup);
    }
}