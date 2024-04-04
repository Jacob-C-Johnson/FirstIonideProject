open Suave
open Suave.Operators
open Suave.Filters
open Suave.Successful
open System
open Newtonsoft.Json
open Suave.Writers

//Create custom type to hold timezone information
type TZInfo = {tzName: string; minDiff: float; localTime: string; utcOffset: float}

// Function for closest timezone
let getClosest () = 

    // This gets all the time zones into a List-like object

    let tzs = TimeZoneInfo.GetSystemTimeZones()

    // List comprehension + type inference allows us to easily perform conversions

    let tzList = [

        for tz in tzs do

        // convert the current time to the local time zone

        let localTz = TimeZoneInfo.ConvertTime(DateTime.Now, tz) 

        // Get the datetime object if it was 5:00pm 

        let fivePM = DateTime(localTz.Year, localTz.Month, localTz.Day, 17, 0, 0)

        // Get the difference between now local time and 5:00pm local time.

        let minDifference = (localTz - fivePM).TotalMinutes

        yield {

                tzName=tz.StandardName;

                minDiff=minDifference;

                localTime=localTz.ToString("hh:mm tt");

                utcOffset=tz.BaseUtcOffset.TotalHours;

             }

    ]

    // We use the pipe operator to chain function calls together

    tzList 

        // filter so that we only get tz after 5pm

        |> List.filter (fun (i:TZInfo) -> i.minDiff >= 0.0) 

        // sort by minDiff

        |> List.sortBy (fun (i:TZInfo) -> i.minDiff) 

        // Get the first item

        |> List.head

// We'll use argv later :)
let runWebServer argv = 

    // Define the port where you want to serve. We'll hardcode this for now.

    let port = 8080

    // create an app config with the port

    let cfg =

          { defaultConfig with

              bindings = [ HttpBinding.createSimple HTTP "0.0.0.0" port]}

    // We'll define a single GET route at the / endpoint that returns "Hello World"

    let app =

          choose

            [ GET >=> choose

                [ 

                    // We are getting the closest time zone, converting it to JSON, then setting the MimeType

                    path "/" >=> request (fun _ -> OK <| JsonConvert.SerializeObject(getClosest()))

                                >=> setMimeType "application/json; charset=utf-8"

                ]

            ]

    // Now we start the server

    startWebServer cfg app

[<EntryPoint>]

let main argv = 

    printfn "%s" <| JsonConvert.SerializeObject(getClosest())

    0