# NuGet Showcase: Testen mit TUnit – die „New Generation" der Test-Framework-Alternativen

*Untertitel: Source-Generierung, AOT und Parallelität für schnellere, robustere .NET-Tests*

> *Alternativtitel: „TUnit – Neuling am Markt oder ernstzunehmende Alternative zu xUnit & Co.?"*

> **Hinweis zur Version:** TUnit befindet sich in aktiver Entwicklung. Der begleitende Proof of
> Concept ist auf **TUnit 1.30.8** festgesetzt (Stand zum Zeitpunkt dieses Artikels); ergänzend
> kommen `TUnit.AspNetCore` und `TUnit.Aspire` (jeweils 1.30.8) sowie `TUnit.Mocks` 1.34.0-beta
> auf **.NET 10** zum Einsatz. Spätere Versionen können in Details abweichen.

---

## Einleitung

Software entwickelt sich ständig weiter – genau wie jeder Organismus, sei es auf natürliche
Weise oder planmäßig. So wie eine Sprache wächst und reift, entwickeln sich auch die Frameworks
und Werkzeuge weiter, mit denen wir diese Sprache implementieren und testen. In solchen Phasen
tauchen immer wieder neue Werkzeuge auf, die das Potenzial haben, eine Ära einzuläuten, in der
sie sich als „unverzichtbar" etablieren.

Als .NET-Entwickler kennt man das Gefühl: Die Test-Suite wächst, und mit ihr die Wartezeit auf
das grüne Häkchen. Im .NET-Umfeld sind **xUnit**, **NUnit** und **MSTest** seit Jahren gesetzt.
Sie alle haben historisch auf der **VSTest**-Plattform und auf **Reflection** zur Laufzeit
aufgesetzt: Tests werden zur Ausführungszeit per Reflexion entdeckt, parallele Ausführung ist –
wenn überhaupt – ein nachträgliches Opt-in, und Szenarien wie **Native AOT** oder **Trimming**
sind schwierig.

In diesem Artikel stelle ich **[TUnit](https://tunit.dev/)** vor – ein modernes Test-Framework
für .NET, das genau hier ansetzt. Die zentrale Fragestellung: Ist TUnit nur ein „Neuling am
Markt" oder eine vielversprechende, ernstzunehmende Alternative? Der Nutzen für Sie als Leser:
ein fundierter Überblick über die Architektur sowie **reale Code- und Pipeline-Beispiele** aus
einem begleitenden Proof of Concept (PoC), den Sie selbst nachvollziehen können:
**[github.com/tavanuka/TUnit.Sample](https://github.com/tavanuka/TUnit.Sample)**.

---

### Grenzen des klassischen Ansatzes

Die etablierten Frameworks leisten gute Arbeit, tragen aber das Erbe ihrer Entstehungszeit mit
sich. Reflection-basierte Test-Discovery kostet Startzeit und verlagert Fehler in die Laufzeit
statt in den Compiler. Parallelität ist oft nicht der Standard. Und der Trend zu
**Ahead-of-Time-Kompilierung (AOT)** sowie Trimming verträgt sich schlecht mit Reflexion, die
dem Compiler verbirgt, welcher Code tatsächlich benötigt wird. Kurz: Die Werkzeuge sind solide,
aber nicht für die Architektur-Trends von heute entworfen.

### Was TUnit anders macht

An mehreren Stellen bricht TUnit bewusst mit den gewohnten Konventionen – und jede dieser
Entscheidungen lässt sich in der offiziellen Dokumentation und im Projekt-Repository nachlesen.
Am grundlegendsten ist das Fundament: TUnit baut vollständig auf der
[Microsoft.Testing.Platform](https://tunit.dev/docs/intro) auf – „built on Microsoft.Testing.Platform
for simpler, more extensible .NET testing" –, also auf genau jener Plattform, in die Microsoft
derzeit stark investiert, statt auf dem in die Jahre gekommenen VSTest.

Darauf aufbauend verzichtet TUnit auf Reflexion zur Laufzeit. Tests werden laut
[Projekt-Repository](https://github.com/thomhurst/TUnit) „source-generated at compile time", also
bereits beim Build erzeugt, statt zur Laufzeit per Reflexion entdeckt zu werden. Das beschleunigt
nicht nur die Discovery, sondern verschiebt viele Fehler vom Laufzeit- in den Kompilierzeitpunkt –
und ebnet zugleich den Weg für „Native AOT & trimming support", der ohne Reflexion überhaupt erst
praktikabel wird.

Auch beim Ausführungsmodell denkt das Framework anders herum: Tests laufen „in parallel by default".
Damit das gefahrlos gelingt, bekommt jeder Test eine eigene Instanz, und geteilter Zustand muss
explizit als `static` deklariert werden (siehe
[Framework-Vergleich](https://tunit.dev/docs/comparison/framework-differences)). Abgerundet wird das
Bild durch eine konsequent **async-first** gehaltene API: Tests geben `Task` zurück, die
Lifecycle-Hooks `[Before]` und `[After]` sind asynchron, und selbst Assertions werden erwartet –
`await Assert.That(x).IsEqualTo(...)`. Diese Einheitlichkeit ist mehr als Kosmetik. Weil überall
dasselbe Muster gilt, entfällt das Raten, *wann* etwas synchron oder asynchron ist; I/O-lastige
Szenarien wie HTTP-Aufrufe oder Datenbankzugriffe fügen sich nahtlos ein, ganz ohne `.Result` oder
`.Wait()` und die damit verbundenen Deadlock-Fallen.

In Bezug auf **Performance** wirbt TUnit damit, dass es durch Source-Generierung und
Compile-Zeit-Optimierungen traditionelle Frameworks zur Laufzeit übertrifft. Der ehrliche
Trade-off: Die Source-Generierung kann die **Build-Zeit** leicht erhöhen. Aktuelle, reproduzierbare
Benchmark-Zahlen veröffentlicht das Projekt fortlaufend – wir verweisen bewusst auf die
**[offiziellen Benchmarks im Projekt-Repository](https://github.com/thomhurst/TUnit)**, statt
hier Momentaufnahmen zu zitieren, die sich von Release zu Release ändern.

### TUnit in der Praxis

Damit das nicht abstrakt bleibt, begleitet den Artikel ein konkreter Proof of Concept: eine
**ASP.NET-Core-Minimal-API** zur Verwaltung von Büchern und Autoren samt ISBN-Validierung,
orchestriert mit **.NET Aspire**, persistiert über **EF Core** und **PostgreSQL**. Getestet wird auf
**.NET 10** mit **TUnit 1.30.8** – Mocking via `TUnit.Mocks` 1.34.0-beta, ASP.NET-Integration via
`TUnit.AspNetCore` und Aspire via `TUnit.Aspire`.

Der Einstieg ist bewusst unspektakulär: Ein `[Test]` genügt, ein Klassen-Attribut ist nicht nötig,
und die Assertion liest sich asynchron und beinahe wie ein Satz
(`test/TUnit.Sample.ApiService.Tests/Services/IsbnFormatterTests.cs`):

```csharp
[Test]
public async Task ValidateIsbn13_ValidIsbn_ReturnsTrue()
{
    // 978-0-306-40615-7 is a well-known valid ISBN-13
    var result = _sut.ValidateIsbn13("9780306406157");

    await Assert.That(result).IsTrue();
}
```

Sobald Setup und Teardown ins Spiel kommen, übernehmen `[Before(Test)]` und `[After(Test)]`.
Mehrere solcher Hooks lassen sich stapeln, und dank Source-Generierung ist ihre
Ausführungsreihenfolge deterministisch – ohne dass man sie zur Laufzeit erraten müsste
(`test/.../IntegrationTests/Utility/CoreIntegrationTestBase.cs`):

```csharp
// Due to nature of source generation, base class hooks will be executed first, and onwards (bottoms-up).
// Additionally, the Order property can also be assigned to dictate which hook gets triggered
[Before(Test)]
public Task SetupDatabaseContextBeforeTest()
{
    _scope = Factory.Services.CreateScope();
    DbContext = _scope.ServiceProvider.GetRequiredService<CoreDbContext>();
    return Task.CompletedTask;
}

[After(Test)]
public async Task CleanupSchema() { /* DROP SCHEMA ... CASCADE */ }
```

Richtig interessant wird es bei den Integrationstests – hier zeigt sich, warum Parallelität und
deklaratives Fixture-Scoping bei TUnit so gut zusammenspielen. Die Basisklasse begrenzt die
Parallelität über ein Attribut und bindet einen PostgreSQL-Testcontainer ein, dessen Lebensdauer
sich **deklarativ** über `Shared = SharedType` steuern lässt (`CoreIntegrationTestBase.cs`):

```csharp
[ParallelLimiter<ProcessorCountParallelLimit>]
public abstract class CoreIntegrationTestBase : WebApplicationTest<WebApplicationFactory, Program>
{
    [ClassDataSource<PostgreSqlTestContainer>(Shared = SharedType.PerTestSession)]
    public PostgreSqlTestContainer PostgreSqlTestContainer { get; init; } = null!;
}
```

Das Fixture selbst implementiert die TUnit-Schnittstelle `IAsyncInitializer` und startet einen
**echten** Datenbank-Container via Testcontainers (`Utility/PostgreSqlTestContainer.cs`):

```csharp
public sealed class PostgreSqlTestContainer : IAsyncInitializer, IAsyncDisposable
{
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder("postgres:17-alpine").Build();

    public async Task InitializeAsync() => await Container.StartAsync();
    public async ValueTask DisposeAsync() => await Container.DisposeAsync();
}
```

Der Clou liegt im **Zusammenspiel zweier Scopes**: Der Docker-Container wird über
`SharedType.PerTestSession` **genau einmal** für den gesamten Testlauf hochgefahren – das spart die
teure Container-Startzeit. Trotzdem laufen die Tests **parallel und vollständig isoliert**, weil
jeder einzelne Test sein **eigenes Datenbank-Schema** erhält. In `SetupAsync` erzeugt jeder Test
über `GetIsolatedName(...)` einen eindeutigen Schema-Namen, legt das Schema per SQL an und baut
darin via EF Core seine Tabellen auf (`CoreIntegrationTestBase.cs`):

```csharp
protected override async Task SetupAsync()
{
    SchemaName = GetIsolatedName("schema");
    var connectionString = PostgreSqlTestContainer.Container.GetConnectionString();

    // Create the schema via raw SQL
    await using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    await using var cmd = connection.CreateCommand();
    cmd.CommandText = $"""CREATE SCHEMA IF NOT EXISTS "{SchemaName}" """;
    await cmd.ExecuteNonQueryAsync();

    // Anschließend legt EF Core die Tabellen im neuen Schema an ...
}
```

Nach dem Test räumt der `[After(Test)]`-Hook das Schema wieder restlos ab
(`DROP SCHEMA ... CASCADE`). Das Ergebnis: ein gemeinsamer, günstiger Container, aber **pro Test
ein frisches, privates Schema** – kein geteilter Zustand, keine Reihenfolge-Abhängigkeiten, keine
„flaky" Tests durch Daten, die ein anderer Test hinterlassen hat. Damit lässt sich die von TUnit
standardmäßig genutzte Parallelität bei Integrationstests **ohne Kompromisse bei der Isolation**
ausspielen – etwas, das bei datenbankgestützten Tests klassischerweise der heikelste Punkt ist.

Auf dieser Grundlage geraten die eigentlichen Web-API-Tests fast beiläufig: Die von
`TUnit.AspNetCore` bereitgestellte Basisklasse `WebApplicationTest<TFactory, TEntryPoint>` reicht
jedem Test einen `HttpClient` gegen die real hochgefahrene API – jeweils gebunden an das eben
beschriebene, test-eigene Schema. Ein vollständiger POST/GET-Roundtrip liest sich dann so
(`test/.../IntegrationTests/People/PersonEndpointsTests.cs`):

```csharp
[Test]
public async Task PostAndGetPerson_Roundtrip()
{
    var client = Factory.CreateClient();
    var createRequest = new CreatePersonRequest("Test", "Person",
        new DateTime(1990, 1, 15, 0, 0, 0, DateTimeKind.Utc));

    var postResponse = await client.PostAsJsonAsync("/persons", createRequest);
    await Assert.That(postResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

    var created = await postResponse.Content.ReadFromJsonAsync<Guid>();
    await Assert.That(created).IsNotDefault();

    var getResponse = await client.GetAsync($"/persons/{created}");
    await Assert.That(getResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);

    var person = await getResponse.Content.ReadFromJsonAsync<PersonDetailResponse>();
    await Assert.That(person).IsNotNull();
    await Assert.That(person.FirstName).IsEqualTo(createRequest.FirstName);
}
```

Über diese Kerndisziplinen hinaus zeigt der PoC, wie nahtlos sich TUnit in das moderne
.NET-Ökosystem einfügt. Mocks etwa entstehen mit `TUnit.Mocks` direkt per Source-Generierung – ganz
ohne separate Mocking-Bibliothek (`Books/BookServiceTests.cs`):

```csharp
private readonly IIsbnFormatterMock _formatter = IIsbnFormatter.Mock();

_formatter.ValidateIsbn13(Any<string>()).Returns(true);
// ...
_publisher.BookCreatedAsync(Is(result!.Value), Any<string>(), Any<CancellationToken>())
    .WasCalled(Times.Once);
```

Verteilte Anwendungen lassen sich mit `TUnit.Aspire` testen, das über ein geteiltes `AppFixture` die
gesamte Aspire-Topologie hochfährt und einen vorkonfigurierten `HttpClient` je Ressource bereitstellt
(`AppHost.IntegrationTests/WebApiTests.cs`):

```csharp
[ClassDataSource<AppFixture>(Shared = SharedType.PerTestSession)]
public class WebApiTests(AppFixture fixture)
{
    [Test]
    public async Task GetWeatherForecast_Returns_StatusCode_OK()
    {
        var client = fixture.CreateHttpClient(ResourceConstants.WebApi);
        var response = await client.GetAsync("/weatherforecast");
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}
```

Und selbst Blazor-Komponenten bleiben nicht außen vor: In Kombination mit *bunit* lassen sie sich
ebenso prüfen wie der Rest der Anwendung (`Web.Tests/TitleBoxTests.cs`):

```csharp
public class TitleBoxTests : BunitContext
{
    [Test]
    public async Task TitleBox_ShouldRender_Title()
    {
        var cut = Render<TitleBox>(p => p.Add(c => c.Title, "Test Title"));
        await Assert.That(cut.Find("h1").TextContent).IsEqualTo("Test Title");
    }
}
```

> *Hinweis zur Vollständigkeit:* Weitere TUnit-Features wie datengetriebene Tests (`[Arguments]`,
> `[Matrix]`, `[MethodDataSource]`), `[Retry]`, `[Timeout]` oder `[DependsOn]` werden im PoC
> bewusst nicht eingesetzt; sie sind aber Teil des Funktionsumfangs und in der
> [Dokumentation](https://tunit.dev/docs/comparison/framework-differences) beschrieben.

### TUnit auf dem CI-Runner

Seine wahre Stärke spielt ein Test-Framework erst im automatisierten Lauf aus – und der PoC liefert
dafür gleich zwei vollständige Pipelines, die dieselbe Strategie auf unterschiedlichen Plattformen
umsetzen. Auf **GitHub Actions** (`.github/workflows/build.yml`) folgt einem gemeinsamen `build`-Job
ein Fächer aus vier kategoriegefilterten Test-Jobs (`test`, `component-test`, `integration-test`,
`aspire-test`), ehe ein abschließender `report`-Job die Ergebnisse zu **Codecov** hochlädt und eine
**SonarQube**-Analyse anstößt; eine separate Pipeline (`code-inspection.yml`) ergänzt das Bild um
eine **Qodana**-Codeanalyse. Die **Azure-Pipeline** (`azure-pipelines.yml`) spiegelt dieselbe
Struktur und führt die Coverage über *ReportGenerator* zu einem Gesamtbericht zusammen.

Entscheidend ist, dass **beide** Pipelines TUnit über genau denselben, nativen
Microsoft.Testing.Platform-Aufruf ansteuern – ohne zusätzlichen Test-Adapter:

```bash
dotnet test --configuration Release --no-build --results-directory ./TestResults \
  -- --coverage --report-trx \
     --treenode-filter "/*/*/*/*[Category=UnitTest]" \
     --coverage-output-format cobertura --ignore-exit-code 8
```

Die Kategorien stammen aus Assembly-Attributen wie `[assembly: Category("Integration")]` (in den
jeweiligen `GlobalSetup.cs`). Über `--treenode-filter` lässt sich so jeder Test-Typ – Unit,
Komponenten, Integration, Aspire – als eigener Pipeline-Job ausführen. Reporting (`--report-trx`)
und Coverage (`--coverage`, Cobertura) sind direkt in die Plattform integriert.

Genau diese Kategorisierung ist auf der CI-Ebene mehr als nur Ordnung – sie ist ein
**Performance-Hebel**. Statt alle Tests in einem einzigen, langen Lauf nacheinander abzuarbeiten,
schneidet `--treenode-filter` die Suite in klar abgegrenzte Jobs, die nach dem gemeinsamen
`build` **parallel auf eigenen Runnern** laufen. Gerade die ressourcenhungrigen Suiten profitieren
davon am stärksten: Die **Aspire**-Tests, die eine ganze verteilte Anwendung hochfahren, und die
**Integrationstests**, die Docker-Container samt Datenbank starten, blockieren so nicht mehr die
leichtgewichtigen Unit- und Komponententests. Jede schwere Last bekommt ihren eigenen Runner und
läuft nebenläufig zu den übrigen – die Gesamtlaufzeit der Pipeline richtet sich damit nach dem
**langsamsten Job statt nach der Summe aller Tests**. Kombiniert mit der test-internen
Parallelität von TUnit ergibt das einen spürbaren Zeit- und Performance-Gewinn auf den CI-Runnern.
Genau das ist der springende Punkt: TUnit ist nicht nur eine Test-Bibliothek, sondern ein
Werkzeug, das von Grund auf für CI/CD-Workflows konzipiert wurde.

---

## Fazit

TUnit ist mehr als ein weiteres Test-Framework. Die Kombination aus **Source-Generierung**,
**Native-AOT-Tauglichkeit**, **standardmäßiger Parallelität**, einer durchgängigen
Async-Konvention und der Basis auf **Microsoft.Testing.Platform** adressiert genau die
Schwachstellen, die der reflexions- und VSTest-basierte Ansatz historisch mit sich bringt – und
der Proof of Concept zeigt das nicht in der Theorie, sondern an realistischen Unit-,
Integrations-, Komponenten- und Aspire-Tests samt zweier produktionsnaher CI/CD-Pipelines.
Besonders bei den Integrationstests wird greifbar, wie elegant gescopte Fixtures und echte
Parallelität zusammenwirken, wenn jeder Test in seinem eigenen, isolierten Schema läuft.

Für Neuprojekte auf aktuellen .NET-Versionen ist TUnit aus meiner Sicht damit bereits heute eine
ernstzunehmende Wahl – vor allem dort, wo Performance, Parallelität und AOT-Tauglichkeit zählen.
Ehrlich bleiben sollte man dabei trotzdem: Das Ökosystem ist jünger als das von xUnit oder NUnit,
und einzelne Bausteine wie `TUnit.Mocks` befinden sich noch im Beta-Stadium, was man bei einer
Entscheidung für oder gegen das Framework abwägen muss. Ist TUnit also nur ein „Neuling am Markt"?
Die Architektur und die enge Verzahnung mit der Microsoft.Testing.Platform sprechen eher dafür,
dass hier ein Werkzeug heranwächst, das sich mittelfristig als unverzichtbar etablieren könnte –
denn schnellere, verlässlichere Tests bedeuten am Ende genau das, was im Entwickleralltag zählt:
kürzere Feedback-Schleifen und mehr Vertrauen in jeden grünen Build. Die beste Art, sich selbst
ein Urteil zu bilden, bleibt das Ausprobieren: Klonen Sie den
**[Proof of Concept](https://github.com/tavanuka/TUnit.Sample)**, lassen Sie die Tests lokal und
in der Pipeline laufen und werfen Sie einen Blick in die offizielle Dokumentation unter
**[tunit.dev](https://tunit.dev/)**.

---

### Quellen & weiterführende Links

- TUnit – Einführung & Architektur: <https://tunit.dev/docs/intro>
- TUnit – Vergleich mit xUnit/NUnit/MSTest: <https://tunit.dev/docs/comparison/framework-differences>
- TUnit – Projekt-Repository & Benchmarks: <https://github.com/thomhurst/TUnit>
- TUnit – NuGet-Paket: <https://www.nuget.org/packages/TUnit>
- Begleitender Proof of Concept (dieses Projekt): <https://github.com/tavanuka/TUnit.Sample>

---

### Danksagung

Ein besonderer Dank gilt **[Thom Longhurst](https://github.com/thomhurst)** für die Konzeption und
Entwicklung von TUnit sowie für sein fortlaufendes Engagement rund um das Framework und die
Microsoft.Testing.Platform. Sein Beitrag zur .NET-Community macht Projekte wie diesen Proof of
Concept überhaupt erst möglich.
