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

    member this.icon =
        Html.i [
            prop.classes [
                "fa"
                match this with
                | List -> "fa-list"
                | Create -> "fa-pen-to-square"
            ]
        ]

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
        color.bgNeutral
        color.textNeutralContent
        prop.children [
            for view in View.All do
                Html.button [ prop.children [ view.icon ]; prop.onClick (fun _ -> setView view) ]


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
            Html.div [
                prop.className "flex justify-center items-center h-dvh"
                prop.children [
                    Daisy.card [
                        card.border
                        color.bgBase300
                        color.textBaseContent
                        prop.children [
                            Daisy.cardBody [
                                Daisy.cardTitle [ prop.text model.View.Description ]
                                match model.View with
                                | List -> listView model.Todos
                                | Create -> Html.p $"TODO: create view"
                            ]
                        ]
                    ]

                ]

            ]


            dock (SetView >> dispatch)
        ]
    ]