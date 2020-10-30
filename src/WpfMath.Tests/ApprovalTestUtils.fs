module WpfMath.Tests.ApprovalTestUtils

open System
open System.Globalization
open System.IO
open System.Text
open System.Reflection
open System.Windows.Media

open ApprovalTests
open ApprovalTests.Namers
open ApprovalTests.Reporters
open ApprovalTests.Writers
open Newtonsoft.Json
open Newtonsoft.Json.Converters
open Newtonsoft.Json.Serialization

open WpfMath

type private BomlessFileWriter(data: string, ?extensionWithoutDot: string) =
    inherit ApprovalTextWriter(data, defaultArg extensionWithoutDot "txt")
    override this.WriteReceivedFile(received: string): string =
        Directory.CreateDirectory(Path.GetDirectoryName(received)) |> ignore
        File.WriteAllText(received, this.Data)
        received

[<assembly: UseReporter(typeof<DiffReporter>)>]
[<assembly: UseApprovalSubdirectory("TestResults")>]
do
    WriterFactory.TextWriterCreator <- Func<_, _>(fun data -> upcast BomlessFileWriter data)
    WriterFactory.TextWriterWithExtensionCreator <- Func<_, _, _>(fun data extensionWithoutDot ->
        upcast BomlessFileWriter(data, extensionWithoutDot)
    )

type private InnerPropertyContractResolver() =
    inherit DefaultContractResolver()
    member private _.DoCreateProperty(p, ms) =
        base.CreateProperty(p, ms, Readable = true)

    override this.CreateProperties(``type``, memberSerialization) =
        // override that to serialize internal properties, too
        let properties =
            ResizeArray(
                ``type``.GetProperties(BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance)
                |> Seq.filter (fun p -> Array.isEmpty <| p.GetIndexParameters()) // no indexers
                |> Seq.map (fun p -> this.DoCreateProperty(p, memberSerialization))
            )

        let baseTypes =
            ``type``
            |> Seq.unfold (fun t ->
                    if t.BaseType = null then
                        None
                    else
                        Some(t, t.BaseType)
                )

        if Seq.exists ((=) typeof<Atoms.Atom>) baseTypes then
            properties.Insert(0,
                new JsonProperty(
                    PropertyName = "[AtomType]",
                    PropertyType = typeof<string>,
                    Readable = true,
                    ValueProvider = {
                        new IValueProvider with
                            member this.GetValue(_) = upcast ``type``.Name
                            member this.SetValue(_, _) = failwith "Not supported"
                    }
                )
            )
        upcast properties

[<AbstractClass>]
type ReadOnlyJsonConverter<'a>() =
    inherit JsonConverter<'a>()
    override _.CanRead = false
    override _.ReadJson(_, _, _, _, _) = failwith "Not supported"

type private GlyphTypefaceConverter() =
    inherit ReadOnlyJsonConverter<GlyphTypeface>()
    override _.WriteJson(writer: JsonWriter, value: GlyphTypeface, _: JsonSerializer) =
        if isNull value then
            writer.WriteNull()
        else
            writer.WriteValue(value.FontUri)

/// This converter should provide the same results on both .NET 4.6.1 and .NET Core 3.0 which is important for approval
/// tests. The roundtrippable double formatting (used by default) differs between these frameworks.
type private UniversalDoubleConverter() =
    inherit ReadOnlyJsonConverter<float>()
    override _.WriteJson(writer: JsonWriter, value: float, _: JsonSerializer) =
        let stringified = value.ToString("0.0###############", CultureInfo.InvariantCulture)
        writer.WriteRawValue stringified

let private jsonSettings = JsonSerializerSettings(ContractResolver = InnerPropertyContractResolver(),
                                                  Formatting = Formatting.Indented,
                                                  Converters = [|
                                                      StringEnumConverter()
                                                      GlyphTypefaceConverter()
                                                      UniversalDoubleConverter()
                                                  |])

let private serialize o =
    JsonConvert.SerializeObject(o, jsonSettings)

let verifyObject: obj -> unit =
    serialize >> Approvals.Verify

let verifyParseResult (formulaText: string): unit =
    let parser = TexFormulaParser()
    let formula = parser.Parse formulaText
    verifyObject formula

let verifyParseResultScenario (scenario: string) (formulaText: string): unit =
    use block = NamerFactory.AsEnvironmentSpecificTest(fun () -> sprintf "(%s)" scenario)
    verifyParseResult formulaText

let processSpecialChars (text: string): string =
    (StringBuilder text)
        .Replace("\\", "")
        .Replace('{', '(')
        .Replace('}', ')')
        .Replace('^', '↑')
        .ToString()
