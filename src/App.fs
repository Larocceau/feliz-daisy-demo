module App

open Feliz
open Browser.Dom
open Fable.Core.JsInterop
open Elmish
open Elmish.React
open App.Components

importSideEffects "./index.css"

Program.mkProgram init update view
|> Program.withReactSynchronous "root"
|> Program.run
