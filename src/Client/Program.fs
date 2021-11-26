module App

open Browser.Dom

Fable.Core.JsInterop.importAll "./Program.scss"

let helloH2 =
    document.querySelector ("#hello") :?> Browser.Types.HTMLDivElement

helloH2.innerText <- "Welcome to Fable!!"
