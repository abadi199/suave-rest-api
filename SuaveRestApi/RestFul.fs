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
        Delete : int -> unit
        GetById : int -> 'a option
        UpdateById : int -> 'a -> 'a option
        IsExists : int -> bool
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
        let resourcePathId =
            let path = resourcePath + "/%d"
            new PrintfFormat<(int -> string),unit,string,string,int>(path) 
        let deleteResourceById id =
            resource.Delete id
            NO_CONTENT
        let getResourceById =
            resource.GetById >> handleResource (NOT_FOUND "Resource not found")
        let updateResourceById id =
            request (getResourceFromReq >> (resource.UpdateById id) >> handleResource badRequest)
        let isResourceExists id =
            if resource.IsExists id then 
                OK ""
            else
                NOT_FOUND ""
        
        choose [
            path resourcePath >=> choose [ 
                GET >=> getAll
                POST >=> request (getResourceFromReq >> resource.Create >> JSON)
                PUT >=> request (getResourceFromReq >> resource.Update >> handleResource badRequest)
            ]
            DELETE >=> pathScan resourcePathId deleteResourceById
            GET >=> pathScan resourcePathId getResourceById
            PUT >=> pathScan resourcePathId updateResourceById
            HEAD >=> pathScan resourcePathId isResourceExists
        ]