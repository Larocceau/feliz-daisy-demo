module App.Components

open Feliz
open Feliz.DaisyUI
open Elmish


type View =
    | Create
    | List

    member this.Description =
        match this with
        | Create -> "Create"
        | List -> "List"

    static member All = [ Create; List ]


type TodoCategory =
    | Personal
    | Work


type Todo = {
    Category: TodoCategory
    Description: string
}


type model = { View: View; Todos: Todo list }
type msg = SetView of View

let init () =
    {
        View = Create
        Todos = [
            {
                Category = Personal
                Description = "Buy groceries"
            }
            {
                Category = Work
                Description = "Submit report"
            }
            {
                Category = Personal
                Description = "Call dentist"
            }
            {
                Category = Work
                Description = "Update documentation"
            }
        ]

    },
    Cmd.none



let update msg model =
    match msg with
    | SetView view -> { model with View = view }, Cmd.none



let dock setView =
    Daisy.dock [
        dock.xl
        prop.children [
            for view in View.All do
                Html.button [ prop.text view.Description; prop.onClick (fun _ -> setView view) ]


        ]
    ]



let listView (todos: Todo list) =
    Daisy.list [
        for todo in todos do
            Daisy.listRow [ prop.text todo.Description ]
    ]

let view model dispatch =
    Html.div [
        prop.children [
            Daisy.card [
                prop.children [
                    Daisy.cardTitle [ prop.text model.View.Description ]
                    Daisy.cardBody [
                        match model.View with
                        | List -> listView model.Todos
                        | Create -> Html.p $"TODO: create view"
                    ]
                ]
            ]

            Daisy.button.button [ button.outline; button.primary; button.lg; prop.text "My button" ]

            dock (SetView >> dispatch)
        ]
    ]