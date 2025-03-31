module App.Components

open Feliz
open Feliz.DaisyUI
open Elmish

module FontAwesome =

    let boldIcon iconClass =
        Html.i [ prop.classes [ "fa"; iconClass ] ]

type TodoCategory =
    | Personal
    | Work

    member this.icon =
        FontAwesome.boldIcon (
            match this with
            | Work -> "fa-briefcase"
            | Personal -> "fa-home"
        )

    member this.description =
        match this with
        | Personal -> "Personal"
        | Work -> "Work"

    static member fromDescription =
        function
        | "Personal" -> Some Personal
        | "Work" -> Some Work
        | _ -> None

    static member all = [ Personal; Work ]

type View =
    | Create
    | List

    member this.Description =
        match this with
        | Create -> "Create"
        | List -> "Todo List"

    member this.icon =
        FontAwesome.boldIcon (
            match this with
            | List -> "fa-list"
            | Create -> "fa-pen-to-square"
        )



type Todo = {
    Category: TodoCategory
    Description: string
    Finished: bool
}


type model = {
    View: View
    Todos: Todo list
    SuccessAlert: bool
}

type msg =
    | SetView of View
    | SetFinished of int * bool
    | AddTodo of TodoCategory * string
    | DismissAlert
    | ClearFinished

let init () =
    {
        SuccessAlert = false
        View = List
        Todos = [
            {
                Category = Personal
                Description = "Buy groceries"
                Finished = false
            }
            {
                Category = Work
                Description = "Submit report"
                Finished = false
            }
            {
                Category = Personal
                Description = "Call dentist"
                Finished = true
            }
            {
                Category = Work
                Description = "Update documentation"
                Finished = false
            }
        ]

    },
    Cmd.none



let update msg model =
    match msg with
    | SetView view -> { model with View = view }, Cmd.none
    | DismissAlert -> { model with SuccessAlert = false }, Cmd.none
    | AddTodo(category, description) ->

        let todo = {
            Description = description
            Category = category
            Finished = false
        }

        {
            model with
                Todos = todo :: model.Todos
                SuccessAlert = true
        },
        Cmd.OfAsync.perform (fun (delay: int) -> Async.Sleep delay) 1000 (fun _ -> DismissAlert)
    | SetFinished(id, finished) ->

        let todo = {
            model.Todos[id] with
                Finished = finished
        }


        {
            model with
                Todos = model.Todos |> List.updateAt id todo
        },
        Cmd.none

    | ClearFinished ->
        {
            model with
                Todos = model.Todos |> List.filter (_.Finished >> not)
        },
        Cmd.none



let dock selectedView setView =
    Daisy.dock [
        dock.xl
        color.bgNeutral
        color.textNeutralContent

        prop.children [
            for view in [ List; Create ] do

                Html.button [
                    if selectedView = view then
                        dock.active
                    prop.children [ view.icon; Daisy.dockLabel view.Description ]
                    prop.onClick (fun _ -> setView view)
                ]


        ]
    ]


let categorySelect value setValue =
    Daisy.select [
        prop.onChange (fun v ->
            let selected =
                match v with
                | "Personal" -> Personal
                | "Work" -> Work
                | other -> failwith $"unknown selection {other}"

            setValue selected


        )
        prop.value (
            match value with
            | Work -> "Work"
            | Personal -> "Personal"
        )
        prop.children [ Html.option "Personal"; Html.option "Work" ]
    ]

[<ReactComponent>]
let CreateView saveTodo =
    let category, setCategory = React.useState Work
    let description, setDescription = React.useState ""

    React.fragment [
        Daisy.fieldset [
            Daisy.floatingLabel [
                Html.span "Description"
                Daisy.input [
                    prop.placeholder "Description"
                    prop.value description
                    prop.onChange setDescription
                ]
            ]
            Daisy.label.select [ Daisy.label "Category"; categorySelect category setCategory ]

        ]
        Daisy.cardActions [
            Daisy.button.button [ button.warning; prop.text "Clear"; prop.onClick (fun _ -> setDescription "") ]
            Daisy.button.button [
                button.success
                prop.text "Save"
                prop.onClick (fun _ ->
                    saveTodo category description
                    setDescription "")
            ]
        ]
    ]

[<ReactComponent>]
let listView (todos: Todo list) (setFinished: int -> bool -> unit) (clearFinished: unit -> unit) =
    let (viewedList, setViewedList) = React.useState (None)

    React.fragment [
        Daisy.filter.form [
            Daisy.filterReset [ prop.name "category"; prop.onClick (fun _ -> setViewedList None) ]
            for category in TodoCategory.all do
                Daisy.button.radio [
                    prop.name "category"
                    prop.ariaLabel category.description
                    prop.selected (Option.contains category viewedList)
                    prop.onChange (fun (v: string) -> setViewedList (Some category))
                ]
        ]

        let shown =
            match viewedList with
            | None -> todos
            | Some category -> todos |> List.filter (fun v -> v.Category = category)

        Daisy.list (
            shown
            |> List.mapi (fun index todo ->
                Daisy.listRow [
                    Daisy.checkbox [
                        prop.value todo.Finished
                        prop.onChange (fun state -> setFinished index state)
                    ]
                    Html.text todo.Description
                ]


            )
        )

        Daisy.cardActions [
            Daisy.button.button [
                prop.onClick (fun _ -> clearFinished ())
                button.sm
                button.warning
                prop.text "Clear finished tasks"
            ]
        ]
    ]

let view model dispatch =
    Html.div [
        Html.div [
            prop.className "flex flex-col justify-center items-center h-dvh gap-2"
            prop.children [
                if model.SuccessAlert then
                    Daisy.alert [
                        prop.onClick (fun _ -> DismissAlert |> dispatch)
                        alert.success
                        prop.text "Your Todo was created!"
                        prop.className "justify-self-start"
                    ]
                Daisy.card [
                    card.border
                    color.bgBase300
                    color.textBaseContent
                    prop.className "w-5/6 md:w-96"
                    prop.children [
                        Daisy.cardBody [
                            Daisy.cardTitle [ prop.text model.View.Description ]
                            match model.View with
                            | List ->
                                listView
                                    model.Todos
                                    (fun index status -> SetFinished(index, status) |> dispatch)
                                    (fun _ -> ClearFinished |> dispatch)
                            | Create ->
                                CreateView(fun category description -> AddTodo(category, description) |> dispatch)

                        ]
                    ]
                ]

            ]

        ]


        dock model.View (SetView >> dispatch)

    ]