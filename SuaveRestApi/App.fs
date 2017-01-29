open SuaveRestApi.Rest
open SuaveRestApi.Db
open Suave.Web

[<EntryPoint>]
let main argv = 
    let personWebPart = rest "people" {
        GetAll = Db.getPeople
        Create = Db.createPerson
        Update = Db.updatePerson
    }
    startWebServer defaultConfig personWebPart 
    0