namespace SuaveRestApi.Rest


[<AutoOpen>]
module RestFul = 
    open Newtonsoft.Json
    open Newtonsoft.Json.Serialization
    open Suave.Successful
    open Suave
    open Suave.Operators
    open Suave.Filters
    open Suave.RequestErrors

    type RestResource<'a> = {
        GetAll : unit -> 'a seq
        Create : 'a -> 'a
        Update : 'a -> 'a option
    }

    let JSON v =
        let settings = new JsonSerializerSettings()
        settings.ContractResolver <- 
            new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, settings)
        |> OK
        >=> Writers.setMimeType "application/json; charset=utf-8"

    let fromJson<'a> json =
        JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

    let getResourceFromReq<'a> (req : HttpRequest) =
        let getString rawForm =
            System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm |> getString |> fromJson<'a>

    let rest resourceName resource  = 
        let resourcePath = "/" + resourceName
        let getAll = warbler (fun _ -> resource.GetAll () |> JSON)
        let badRequest = BAD_REQUEST "Resource not found"
        let handleResource requestError = function
            | Some r -> r |> JSON
            | None -> requestError

        path resourcePath >=> choose [ 
            GET >=> getAll
            POST >=> request (getResourceFromReq >> resource.Create >> JSON)
            PUT >=> request (getResourceFromReq >> resource.Update >> handleResource badRequest)
        ]