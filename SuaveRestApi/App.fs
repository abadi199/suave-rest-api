open SuaveRestApi.Rest
open SuaveRestApi.Db
open SuaveRestApi.MusicStoreDb
open Suave.Web 
open Suave

[<EntryPoint>]
let main argv = 
    let personWebPart = rest "people" {
        GetAll = Db.getPeople
        Create = Db.createPerson
        Update = Db.updatePerson
        Delete = Db.deletePerson
        GetById = Db.getPerson
        UpdateById = Db.updatePersonById
        IsExists = Db.isPersonExists
    }
    let albumWebPart = rest "album" {
        GetAll = MusicStoreDb.getAlbums
        Create = MusicStoreDb.createAlbum
        Update = MusicStoreDb.updateAlbum
        Delete = MusicStoreDb.deleteAlbum
        GetById = MusicStoreDb.getAlbumById
        UpdateById = MusicStoreDb.updateAlbumById
        IsExists = MusicStoreDb.isAlbumExists
    }
    startWebServer defaultConfig (choose [personWebPart;albumWebPart])
    0