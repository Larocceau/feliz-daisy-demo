module App

open Feliz
open Browser.Dom
open Fable.Core.JsInterop



let root = ReactDOM.createRoot (document.getElementById "root")
root.render (App.Components.main)
